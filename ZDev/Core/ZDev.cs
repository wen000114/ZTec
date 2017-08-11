namespace ZDev.Core
{
    #region

    using System;
    using System.IO;
    using System.Linq;
    using Aimtec;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Util;

    #endregion

    internal class ZDev
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public Menu Menu { get; set; }

        /// <summary>
        ///     Gets or sets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        public Obj_AI_Hero Player { get; set; }

        /// <summary>
        ///     Gets or sets the test missile.
        /// </summary>
        /// <value>
        ///     The test missile.
        /// </value>
        public MissileClient TestMissile { get; set; }

        /// <summary>
        ///     Gets or sets the test missile start time.
        /// </summary>
        /// <value>
        ///     The test missile start time.
        /// </value>
        public int TestMissileStartTime { get; set; }

        /// <summary>
        ///     Gets or sets the test missile start position.
        /// </summary>
        /// <value>
        ///     The test missile start position.
        /// </value>
        public Vector2 TestMissileStartPos { get; set; }

        /// <summary>
        ///     Gets or sets the last hero cast position.
        /// </summary>
        /// <value>
        ///     The last hero cast position.
        /// </value>
        public Vector2 LastHeroCastPosition { get; set; }

        /// <summary>
        ///     Gets or sets the last hero cast time.
        /// </summary>
        /// <value>
        ///     The last hero cast time.
        /// </value>
        public int LastHeroCastTime { get; set; }

        /// <summary>
        ///     Gets or sets the test data.
        /// </summary>
        /// <value>
        ///     The test data.
        /// </value>
        public ZData TestData { get; set; }

        #endregion

        #region Constructors and Destructors

        public ZDev()
        {
            TestData = new ZData();
            TestMissile = null;
            TestMissileStartPos = new Vector2();

            Player = ObjectManager.GetLocalPlayer();
            Menu = new Menu("ZDev", "ZDev", true);
            Menu.Add(new MenuSeperator("sep", "\"AimtecLoader\\Data\\System\\Logs\\ZZephyr\""));
            Menu.Add(new MenuBool("DumpData", "Debug & Dump Spell Data", false));
            Menu.Add(new MenuBool("DumpToLower", "Dump String Values ToLower"));
            Menu.Attach();

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDestroy += GameObject_OnDestroy;
        }

        #endregion

        #region Private Methods and Operators

        private void GameObject_OnDestroy(GameObject sender)
        {
            var missile = sender as MissileClient;
            if (missile != null && missile.SpellCaster is Obj_AI_Hero)
            {
                if (TestMissile != null && TestMissile.NetworkId == sender.NetworkId)
                {
                    var range = sender.Position.To2D().Distance(TestMissile.StartPosition.To2D());
                    var delay = 1000 * TestMissile.SpellData.CastFrame / 30;

                    TestData.CastRange = range;
                    TestData.ChampionName = ((Obj_AI_Hero) missile.SpellCaster).ChampionName;
                    TestData.SpellName = TestMissile.SpellData.Name;
                    TestData.MissileName = TestMissile.SpellData.Name;
                    TestData.Delay = delay > 0 ? delay : 250;
                    TestData.SpellTargetingType = TestMissile.SpellData.TargettingType;
                    TestData.MissileSpeed = 1000 * (range / (Game.TickCount - TestMissileStartTime));

                    Console.WriteLine("== " + missile.SpellData.Name);
                    Console.WriteLine("Core Cast Range: " + missile.SpellData.CastRange);
                    Console.WriteLine("Core Missile Speed: " + missile.SpellData.MissileSpeed);
                    Console.WriteLine("Core TargetingType: " + missile.SpellData.TargettingType);
                    Console.WriteLine("Est. Missile Speed: " + TestData.MissileSpeed);
                    Console.WriteLine("Est. Missile Delay: " + TestData.Delay + " (" + missile.SpellData.CastFrame / 30 + ")");
                    Console.WriteLine("Est. Missile Range: " + TestData.CastRange);
                    Console.WriteLine("Exported (OnDestory)");
                    Console.WriteLine("");
                    ExportSpellData(TestData);
                }
            }
        }

        private void GameObject_OnCreate(GameObject sender)
        {
            var missile = sender as MissileClient;
            if (missile != null && Menu["DumpData"].As<MenuBool>().Enabled)
            {
                var aiHero = missile.SpellCaster as Obj_AI_Hero;
                if (aiHero == null)
                {
                    return;
                }

                if (missile.SpellData.Name.ToLower().Contains("attack"))
                {
                    return;
                }

                DelayAction.Queue(250 + Game.Ping, () =>
                {
                    if (missile != null && missile.IsValid && !missile.IsDead)
                    {
                        //TestMissileStartTime = Game.TickCount;
                        TestMissileStartPos = missile.Position.To2D();
                    }
                });

                TestMissile = missile;
                TestMissileStartPos = missile.Position.To2D();
                TestMissileStartTime = Game.TickCount;

                TestData.SpellName = missile.SpellData.Name;
                TestData.ChampionName = ((Obj_AI_Hero) missile.SpellCaster).ChampionName;
                TestData.CastTime = (Game.TickCount - LastHeroCastTime);
                TestData.MissileName = missile.SpellData.Name;
                TestData.Radius = missile.SpellData.LineWidth;
                TestData.MissileAccel = missile.SpellData.MissileAccel;
                TestData.MissileMaxSpeed = missile.SpellData.MissileMaxSpeed;
                TestData.Radius = missile.SpellData.LineWidth;
                TestData.CastRange = missile.SpellData.CastRange;
                TestData.SpellTargetingType = missile.SpellData.TargettingType;

                DelayAction.Queue(750, () =>
                {
                    if (missile != null && missile.IsValid && !missile.IsDead)
                    {
                        var dist = missile.Position.To2D().Distance(TestMissileStartPos);
                        TestData.MissileSpeed = 1000 * (dist / (Game.TickCount - TestMissileStartTime));
                        ExportSpellData(TestData);
                    }
                });
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            var aiHero = sender as Obj_AI_Hero;
            if (aiHero != null && Menu["DumpData"].As<MenuBool>().Enabled)
            {
                if (args.SpellData.Name.ToLower().Contains("attack"))
                {
                    return;
                }

                TestData.SpellName = args.SpellData.Name;
                LastHeroCastTime = Game.TickCount;
                LastHeroCastPosition = sender.ServerPosition.To2D();

                var delay = 1000 * args.SpellData.CastFrame / 30;
                var processData = new ZData
                {
                    SpellName = args.SpellData.Name.ToLower(),
                    ChampionName = aiHero.UnitSkinName.ToLower(),
                    Slot = args.SpellSlot,
                    Radius = args.SpellData.LineWidth > 0
                        ? args.SpellData.LineWidth
                        : (args.SpellData.CastRadiusSecondary > 0
                            ? args.SpellData.CastRadiusSecondary
                            : args.SpellData.CastRadius),
                    CastRange = args.SpellData.CastRange,
                    Delay = delay > 0 ? delay : 250f,
                    MissileSpeed = args.SpellData.MissileSpeed,
                    MissileAccel = args.SpellData.MissileAccel,
                    MissileMaxSpeed = args.SpellData.MissileMaxSpeed,
                    SpellTargetingType = args.SpellData.TargettingType,
                    CastTime = Game.TickCount - LastHeroCastTime
                };

                var time = (int) (delay > 0 ? delay : 250);

                DelayAction.Queue(time, () =>
                {
                    if (TestMissile == null)
                    {
                        // 0 - self cast (e.g orianna w)
                        // 1 - targeted/unit cast (e.g orianna e)
                        // 4 - self cast with a target (e.g nami w, amumu r)
                        // 5 - sefl cast with aoe effect?? (e.g locket, heal, idfk)
                        // this may not get you the right results you want. 
                        // i did this to try and estimated cast speeds based on how 
                        // fast your hero has traveled within the time, good for instant cast spells
                        // or spells with no missile but are kind of like a dash. not sure how accurate it is :^)

                        if (new[] { 0, 1, 4, 5 }.Any(x => args.SpellData.TargettingType == x))
                        {
                            var dist = sender.ServerPosition.To2D().Distance(LastHeroCastPosition);
                            var castTime = Math.Min(4800, time / dist * 1000);
                            var castSpeed = time + 1000 * (dist / castTime);

                            if (dist < 100)
                            {
                                processData.CastTime = processData.Delay / castTime;
                                processData.MissileSpeed = castTime;
                            }
                            else
                            {
                                processData.CastTime = castTime;
                                processData.MissileSpeed = castSpeed;
                            }

                            Console.WriteLine("== " + args.SpellData.Name);
                            Console.WriteLine("Core Cast Range: " + processData.CastRange);
                            Console.WriteLine("Core Missile Speed: " + args.SpellData.MissileSpeed);
                            Console.WriteLine("Core TargetingType: " + args.SpellData.TargettingType);
                            Console.WriteLine("Est. CastTime: " + processData.CastTime);
                            Console.WriteLine("Est. Cast Delay: " + processData.Delay);
                            Console.WriteLine("Est. Travel Speed: " + processData.MissileSpeed);
                            Console.WriteLine("Exported (OnProcess)");
                            Console.WriteLine("");
                            ExportSpellData(processData);
                        }
                        else
                        {
                            Console.WriteLine("== " + args.SpellData.Name);
                            Console.WriteLine("Core Cast Range: " + processData.CastRange);
                            Console.WriteLine("Core Missile Speed: " + args.SpellData.MissileSpeed);
                            Console.WriteLine("Core TargetingType: " + args.SpellData.TargettingType);
                            Console.WriteLine("Est. Cast Delay: " + processData.Delay);
                            Console.WriteLine("Exported (OnProcess)");
                            Console.WriteLine("");
                            ExportSpellData(processData);
                        }
                    }

                });
            }
        }

        private void ExportSpellData(ZData data)
        {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Logs", "ZZephyr",
                $"zzephyr_{data.ChampionName.ToLower()}.txt");

            var file = new StreamWriter(path, true);
            if (data.SpellName.Contains("attack"))
            {
                return;
            }

            var toLower = Menu["DumpToLower"].As<MenuBool>().Enabled;

            file.WriteLine(@"#region Spell data dumper © 2015 Kurisu Solutions");
            file.WriteLine(@"// Dumps spell data from the client into a text file.");
            file.WriteLine(@"// {0}", DateTime.Now.ToString("F"));
            file.WriteLine(@"#endregion");
            file.WriteLine(@"");
            file.WriteLine(@"Spells.Add(new Gamedata");
            file.WriteLine(@"{");
            file.WriteLine(@"    // TargetingType = ""{0}"",", data.SpellTargetingType);
            file.WriteLine(@"    SpellName = ""{0}"",", toLower ? data.SpellName.ToLower() : data.SpellName);
            file.WriteLine(@"    ChampionName = ""{0}"",", toLower ? data.ChampionName.ToLower() : data.ChampionName);
            file.WriteLine(@"    Slot = SpellSlot.{0},", data.Slot);
            file.WriteLine(@"    CastRange = ""{0}"",", data.CastRange);
            file.WriteLine(@"    Radius = ""{0}"",", data.Radius);
            file.WriteLine(@"    Delay = {0}f,", data.Delay);
            file.WriteLine(@"    EventTypes = new[] {{ }},");
            file.WriteLine(@"    FixedRange = true,");
            file.WriteLine(@"    MissileName = ""{0}"",", data.MissileName);
            file.WriteLine(@"    EventTypes = new EventType[] {{ }},");
            file.WriteLine(@"    MissileSpeed = {0},", data.MissileSpeed);
            file.WriteLine(@"});");
            file.WriteLine(@"");
            file.Close();

            TestMissile = null;
        }

        #endregion
    }
}