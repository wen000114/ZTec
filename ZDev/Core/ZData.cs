namespace ZDev.Core
{
    #region

    using Aimtec;

    #endregion

    internal class ZData
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
}