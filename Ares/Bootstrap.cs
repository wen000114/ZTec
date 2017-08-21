namespace Ares
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

    /// <summary>
    ///     Enum ProcessSpellType
    /// </summary>
    public enum ProcessSpellType
    {
        /// <summary>
        ///     The self process type
        /// </summary>
        Self = 0,

        /// <summary>
        ///     The targeted process type
        /// </summary>
        Targeted = 1,

        /// <summary>
        ///     The location circle process type
        /// </summary>
        LocationCircle = 2,

        /// <summary>
        ///     The location cone process type
        /// </summary>
        LocationCone = 3,

        /// <summary>
        ///     The self and unit process type
        /// </summary>
        SelfAndUnit = 4,

        /// <summary>
        ///     The self aoe/location something idk process type ?
        /// </summary>
        UnitAndAoE = 5,

        /// <summary>
        ///     The location line process type
        /// </summary>
        LocationLineMissile = 6,

        /// <summary>
        ///     The location line2 process type
        /// </summary>
        LocationLine = 7,

        /// <summary>
        ///     The location line3 process type
        /// </summary>
        LocationUnknown = 11
    }

    internal class Data
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the name of the spell.
        /// </summary>
        /// <value>
        ///     The name of the spell.
        /// </value>
        public string SpellName { get; set; }

        /// <summary>
        ///     Gets or sets the cast time.
        /// </summary>
        /// <value>
        ///     The cast time.
        /// </value>
        public float CastTime { get; set; }

        /// <summary>
        ///     Gets or sets the name of the champion.
        /// </summary>
        /// <value>
        ///     The name of the champion.
        /// </value>
        public string ChampionName { get; set; }

        /// <summary>
        ///     Gets or sets the slot.
        /// </summary>
        /// <value>
        ///     The slot.
        /// </value>
        public SpellSlot Slot { get; set; }

        /// <summary>
        ///     Gets or sets the radius.
        /// </summary>
        /// <value>
        ///     The radius.
        /// </value>
        public float Radius { get; set; }

        /// <summary>
        ///     Gets or sets the cast range.
        /// </summary>
        /// <value>
        ///     The cast range.
        /// </value>
        public float CastRange { get; set; }

        /// <summary>
        ///     Gets or sets the delay.
        /// </summary>
        /// <value>
        ///     The delay.
        /// </value>
        public float Delay { get; set; }

        /// <summary>
        ///     Gets or sets the name of the missile.
        /// </summary>
        /// <value>
        ///     The name of the missile.
        /// </value>
        public string MissileName { get; set; }

        /// <summary>
        ///     Gets or sets the missile speed.
        /// </summary>
        /// <value>
        ///     The missile speed.
        /// </value>
        public float MissileSpeed { get; set; }

        /// <summary>
        ///     Gets or sets the missile accel.
        /// </summary>
        /// <value>
        ///     The missile accel.
        /// </value>
        public float MissileAccel { get; set; }

        /// <summary>
        ///     Gets or sets the missile maximum speed.
        /// </summary>
        /// <value>
        ///     The missile maximum speed.
        /// </value>
        public float MissileMaxSpeed { get; set; }

        /// <summary>
        ///     Gets or sets the type of the spell targeting.
        /// </summary>
        /// <value>
        ///     The type of the spell targeting.
        /// </value>
        public uint SpellTargetingType { get; set; }

        #endregion
    }

    internal class Bootstrap
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
        public Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

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

        #endregion

        #region Constructors and Destructors

        public Bootstrap()
        {
            TestMissile = null;
            TestMissileStartPos = new Vector2();
            LastHeroCastPosition = new Vector2();
            LastHeroCastTime = Game.TickCount;
            TestMissileStartTime = Game.TickCount;

            Menu = new Menu("Ares", "Ares", true);
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

                    var missileData = new Data
                    {
                        CastRange = range,
                        ChampionName = ((Obj_AI_Hero) missile.SpellCaster).ChampionName,
                        SpellName = TestMissile.SpellData.Name,
                        MissileName = TestMissile.SpellData.Name,
                        Delay = delay > 0 ? delay : 250,
                        SpellTargetingType = TestMissile.SpellData.TargettingType,
                        MissileSpeed = 1000 * (range / (Game.TickCount - TestMissileStartTime))
                    };

                    Console.WriteLine("== " + missile.SpellData.Name);
                    Console.WriteLine("Core Cast Range: " + missile.SpellData.CastRange);
                    Console.WriteLine("Core Missile Speed: " + missile.SpellData.MissileSpeed);
                    Console.WriteLine("Core TargetingType: " + missile.SpellData.TargettingType);
                    Console.WriteLine("Core Missile Width: " + missile.SpellData.LineWidth);
                    Console.WriteLine("Est. Missile Speed: " + missileData.MissileSpeed);
                    Console.WriteLine("Est. Missile Delay: " + missileData.Delay + " (" + missile.SpellData.CastFrame / 30 + ")");
                    Console.WriteLine("Est. Missile Range: " + missileData.CastRange);
                    Console.WriteLine("Exported (OnDestory)");
                    Console.WriteLine("");

                    ExportSpellData(missileData);
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
                    if (missile.IsValid && !missile.IsDead)
                    {
                        //TestMissileStartTime = Game.TickCount;
                        TestMissileStartPos = missile.Position.To2D();
                    }
                });

                TestMissile = missile;
                TestMissileStartPos = missile.Position.To2D();
                TestMissileStartTime = Game.TickCount;

                var missileData = new Data();
                missileData.SpellName = missile.SpellData.Name;
                missileData.ChampionName = ((Obj_AI_Hero) missile.SpellCaster).ChampionName;
                missileData.CastTime = (Game.TickCount - LastHeroCastTime);
                missileData.MissileName = missile.SpellData.Name;
                missileData.Radius = missile.SpellData.LineWidth;
                missileData.MissileAccel = missile.SpellData.MissileAccel;
                missileData.MissileMaxSpeed = missile.SpellData.MissileMaxSpeed;
                missileData.Radius = missile.SpellData.LineWidth;
                missileData.CastRange = missile.SpellData.CastRange;
                missileData.SpellTargetingType = missile.SpellData.TargettingType;

                DelayAction.Queue(1200, () =>
                {
                    if (missile.IsValid && !missile.IsDead)
                    {
                        var dist = missile.Position.To2D().Distance(TestMissileStartPos);
                        missileData.MissileSpeed = 1000 * (dist / (Game.TickCount - TestMissileStartTime));
                        ExportSpellData(missileData);
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

                LastHeroCastTime = Game.TickCount;
                LastHeroCastPosition = sender.ServerPosition.To2D();

                var delay = 1000 * args.SpellData.CastFrame / 30;
                var processData = new Data
                {
                    SpellName = args.SpellData.Name,
                    ChampionName = aiHero.UnitSkinName,
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
                });
            }
        }

        private void ExportSpellData(Data data)
        {
            TestMissile = null;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "ZZephyr",
                $"zzephyr_{data.ChampionName.ToLower()}.txt");

            var file = new StreamWriter(path, true);
            if (data.SpellName.Contains("attack"))
            {
                return;
            }

            var toLower = Menu["DumpToLower"].As<MenuBool>().Enabled;

            file.WriteLine(@"#region ZFlux © 2017 Steiner Solutions");
            file.WriteLine(@"// Dumps spell data from the client into a text file.");
            file.WriteLine(@"// {0}", DateTime.Now.ToString("F"));
            file.WriteLine(@"#endregion");
            file.WriteLine(@"");
            file.WriteLine(@"Spells.Add(new Gamedata");
            file.WriteLine(@"{");
            file.WriteLine(@"    SpellName = ""{0}"",", toLower ? data.SpellName.ToLower() : data.SpellName);
            file.WriteLine(@"    ChampionName = ""{0}"",", toLower ? data.ChampionName.ToLower() : data.ChampionName);
            file.WriteLine(@"    Slot = SpellSlot.{0},", data.Slot);
            file.WriteLine(@"    CastRange = {0}f,", data.CastRange);
            file.WriteLine(@"    Radius = {0}f,", data.Radius);
            file.WriteLine(@"    Delay = {0}f,", data.Delay);
            file.WriteLine(@"    TargetingType = {0},", (ProcessSpellType) data.SpellTargetingType);
            file.WriteLine(@"    EventTypes = new[] {{ }},");
            file.WriteLine(@"    FixedRange = true,");
            file.WriteLine(@"    MissileName = ""{0}"",", toLower ? data.MissileName.ToLower() : data.MissileName);
            file.WriteLine(@"    MissileSpeed = {0},", data.MissileSpeed);
            file.WriteLine(@"});");
            file.WriteLine(@"");
            file.Close();
        }

        #endregion
    }
}