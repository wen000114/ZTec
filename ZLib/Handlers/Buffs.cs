#region Copyright © 2015 Kurisu Solutions

// All rights are reserved. Transmission or reproduction in part or whole,
// any form or by any means, mechanical, electronical or otherwise, is prohibited
// without the prior written consent of the copyright owner.
//
// Document:    Handlers/Buffs.cs
// Date:        22/09/2015
// Author:      Robin Kurisu

#endregion

namespace ZLib.Handlers
{
    #region

    using System;
    using System.Linq;
    using Aimtec;
    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Util;
    using Base;
    using Data;

    #endregion

    internal static class Buffs
    {
        #region Private Methods and Operators

        private static void OnBuffSurrounding()
        {
            try
            {
                foreach (var unit in ZLib.GetUnits())
                {
                    if (unit.Instance == null || !unit.Instance.IsValidTarget(float.MaxValue, true))
                    {
                        continue;
                    }

                    var aura = ZLib.CachedAuras.Find(au => unit.Instance.HasBuff(au.Name));
                    if (aura == null)
                    {
                        continue;
                    }

                    var owner = GetBuffCaster(unit, aura.Name);
                    if (owner == null)
                    {
                        continue;
                    }

                    Gamedata data = null;

                    if (aura.Champion == null && aura.Slot == SpellSlot.Unknown)
                        data = new Gamedata { SpellName = aura.Name };

                    if (aura.Champion != null && aura.Slot != SpellSlot.Unknown)
                    {
                        data = ZLib.CachedSpells
                            .Where(x => x.Slot == aura.Slot).FirstOrDefault(x => x.HeroNameMatch(aura.Champion));
                    }

                    if (aura.Reverse && aura.DoT)
                    {
                        if ((int) (Game.ClockTime * 1000) - aura.TickLimiter >= aura.Interval * 1000)
                        {
                            foreach (var ally in ZLib.GetUnits())
                            {
                                if (ally.Instance.Distance(unit.Instance) <= aura.Radius + 35)
                                {
                                    Projections.EmulateDamage(owner, ally, data, EventType.Buff, "aura.dot");
                                }
                            }

                            aura.TickLimiter = (int) (Game.ClockTime * 1000);
                        }
                    }

                    if (aura.Reverse && aura.Evade)
                    {
                        if ((int) (Game.ClockTime * 1000) - aura.TickLimiter >= 100)
                        {
                            foreach (var ally in ZLib.GetUnits())
                            {
                                if (ally.Instance.Distance(unit.Instance) <= aura.Radius + 35)
                                {
                                    Projections.EmulateDamage(owner, ally, data, EventType.Buff, "aura.evade");
                                }
                            }

                            aura.TickLimiter = (int) (Game.ClockTime * 1000);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (ZLib.Menu["logerror"].As<MenuBool>().Enabled)
                {
                    Console.WriteLine("== Error at: ZLib.OnBuffSurrounding");
                    Console.WriteLine(e);
                }
            }
        }

        private static void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Buff buff)
        {
            #region Buffs

            foreach (var ally in ZLib.GetUnits())
            {
                if (sender.NetworkId == ally.Instance.NetworkId)
                {
                    if (buff.Name == "rengarralertsound")
                    {
                        Projections.EmulateDamage(sender, ally, new Gamedata { SpellName = "Stealth" }, EventType.Stealth,
                            "handlers.onbuffadd");
                    }
                }
            }

            #endregion
        }

        private static void OnBuffUpdate()
        {
            try
            {
                foreach (var hero in ZLib.GetUnits())
                {
                    if (hero.Instance == null || !hero.Instance.IsValidTarget(float.MaxValue, true))
                    {
                        continue;
                    }

                    var aura = ZLib.CachedAuras.Find(au => hero.Instance.HasBuff(au.Name));
                    if (aura == null)
                    {
                        continue;
                    }

                    var owner = GetBuffCaster(hero, aura.Name);
                    if (owner == null)
                    {
                        continue;
                    }

                    if (aura.Reverse)
                    {
                        continue;
                    }

                    var data = new Gamedata();

                    if (aura.Champion == null && aura.Slot == SpellSlot.Unknown)
                        data = new Gamedata { SpellName = aura.Name };

                    if (aura.Champion != null && aura.Slot != SpellSlot.Unknown)
                    {
                        data = ZLib.CachedSpells.Where(x => x.Slot == aura.Slot)
                            .FirstOrDefault(x => x.HeroNameMatch(aura.Champion));
                    }

                    if (aura.Evade)
                    {
                        DelayAction.Queue(aura.EvadeTimer,
                            () =>
                            {
                                // double check after delay incase we no longer have the buff
                                if (hero.Instance.HasBuff(aura.Name))
                                {
                                    if ((int) (Game.ClockTime * 1000) - aura.TickLimiter >= 250)
                                    {
                                        Projections.EmulateDamage(owner, hero, data, EventType.Buff, "aura.evade");
                                        aura.TickLimiter = (int) (Game.ClockTime * 1000);
                                    }
                                }
                            });
                    }

                    if (aura.DoT)
                    {
                        if ((int) (Game.ClockTime * 1000) - aura.TickLimiter >= aura.Interval * 1000)
                        {
                            if (aura.Name == "velkozresearchstack"
                                && !hero.Instance.HasBuffOfType(BuffType.Slow))
                            {
                                continue;
                            }

                            // ReSharper disable once PossibleNullReferenceException
                            Projections.EmulateDamage(owner, hero, data, EventType.Buff, "aura.dot");
                            aura.TickLimiter = (int) (Game.ClockTime * 1000);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (ZLib.Menu["logerror"].As<MenuBool>().Enabled)
                {
                    Console.WriteLine("== Error at: ZLib.OnBuffUpdate");
                    Console.WriteLine(e);
                }
            }
        }

        private static Obj_AI_Base GetBuffCaster(Unit unit, string auraName)
        {
            var buffInst = unit.Instance.BuffManager.GetBuff(auraName, true);
            if (buffInst != null)
            {
                if (buffInst.IsActive && buffInst.IsValid)
                {
                    return buffInst.Caster as Obj_AI_Base;
                }
            }

            return null;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the custom damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="auraname">The auraname.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static int GetCustomDamage(this Obj_AI_Base source, string auraname, Obj_AI_Base target)
        {
            // todo: needs updating badly! ;_;

            if (auraname == "sheen")
            {
                return
                    (int)
                    source.CalculateDamage(target, DamageType.Physical,
                        1.0 * source.FlatPhysicalDamageMod + source.BaseAttackDamage);
            }

            if (auraname == "lichbane")
            {
                return
                    (int)
                    source.CalculateDamage(target, DamageType.Magical,
                        (0.75 * source.FlatPhysicalDamageMod + source.BaseAttackDamage) +
                        (0.50 * source.FlatMagicDamageMod));
            }

            return 0;
        }

        #endregion

        #region Internal Methods and Operators

        internal static void StartOnUpdate()
        {
            Game.OnUpdate += OnBuffUpdate;
            Game.OnUpdate += OnBuffSurrounding;
            BuffManager.OnAddBuff += Obj_AI_Base_OnBuffAdd;
        }

        #endregion
    }
}