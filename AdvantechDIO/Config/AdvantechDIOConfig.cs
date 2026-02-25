namespace AdvantechDIO.Config
{
    /// <summary>
    /// Configuration model for AdvantechDIO, mapped from XML settings.
    /// Drives device ID and DI/DO port/pin topology.
    /// </summary>
    public class AdvantechDIOConfig
    {
        /// <summary>
        /// Advantech device ID, mapped to DeviceID.
        /// </summary>
        public int DeviceID { get; set; }

        /// <summary>
        /// Number of digital input ports. 0 means DI is not configured.
        /// </summary>
        public int DIPortCount { get; set; }

        /// <summary>
        /// Number of pins (bits) per DI port.
        /// </summary>
        public int DIPinCountPerPort { get; set; }

        /// <summary>
        /// Number of digital output ports. 0 means DO is not configured.
        /// </summary>
        public int DOPortCount { get; set; }

        /// <summary>
        /// Number of pins (bits) per DO port.
        /// </summary>
        public int DOPinCountPerPort { get; set; }
    }
}
