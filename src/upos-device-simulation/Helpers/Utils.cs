using Microsoft.PointOfService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using upos_device_simulation.Interfaces;


namespace upos_device_simulation.Helpers
{
    public class Utils
    {
        private readonly ILogger logger;

        public Utils(ILogger logger)
        {
            this.logger = logger;
        }
        public string GetDeviceId()
        {

            try
            {
                string cpuInfo = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    if (cpuInfo == String.Empty)
                    {
                        cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                    }
                }
                return cpuInfo;
            }
            catch (Exception ex)
            {
                logger.Error("Error occured while getting Device ID." + ex.Message + ex.StackTrace);
                return "1";
            }
        }
    }
}
