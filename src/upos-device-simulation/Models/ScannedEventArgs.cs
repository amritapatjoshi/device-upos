using System;

namespace upos_device_simulation.Models
{
    public class ScannedEventArgs : EventArgs
    {
        public ScanUpos UPOS { get; set; }
        public string LaneId { get; set; }
    }
    public class ScanUpos
    {
        public string UPC { get; set; }
    }
    public class DeviceConnectionInfo
    {
        public string UniqueId { get; set; }
        public string MacId { get; set; }
        public string LaneId { get; set; }

    }
}


