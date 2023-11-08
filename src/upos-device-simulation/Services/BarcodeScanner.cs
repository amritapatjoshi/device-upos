using System;
using System.Text;
using Microsoft.PointOfService;
using upos_device_simulation.Interfaces;
using upos_device_simulation.Models;

namespace upos_device_simulation.Services
{

    public sealed class BarcodeScanner : IBarcodeScanner
    {
        private readonly ILogger logger;
        private readonly PosExplorer posExplorer;
        private Scanner scanner;
        public event EventHandler<ScannedEventArgs> Scanned;
        public event EventHandler<ErrorInfo> ScannedError;
        public BarcodeScanner(ILogger logger, PosExplorer explorer)
        {
            this.logger = logger;
            posExplorer = explorer;
            posExplorer.DeviceAddedEvent += new DeviceChangedEventHandler(posExplorer_DeviceAddedEvent);
            posExplorer.DeviceRemovedEvent += new DeviceChangedEventHandler(posExplorer_DeviceRemovedEvent);
        }

        public void Start()
        {
            logger.Info("Getting Scanner form posExplorer.");
            DeviceInfo device = posExplorer.GetDevices(DeviceType.Scanner)[0];
            logger.Info("Got Scanner");
            scanner = (Scanner)posExplorer.CreateInstance(device);
            scanner.Open();
            scanner.Claim(1000);
            scanner.DataEvent += new DataEventHandler(scanner_DataEvent);
            scanner.ErrorEvent += new DeviceErrorEventHandler(scanner_ErrorEvent);
            scanner.DeviceEnabled = true;
            scanner.DataEventEnabled = true;
            scanner.DecodeData = true;
            logger.Info("Scanner Started");
        }
        private void Close()
        {
            scanner.DataEventEnabled = false;
            scanner.DeviceEnabled = false;
            scanner.Release();
            scanner.Close();
            logger.Info("Scanner Closed");
        }

        private void scanner_ErrorEvent(object sender, DeviceErrorEventArgs e)
        {
            ScannedError?.Invoke(this, new ErrorInfo { UposError = new UPOSErrorInfo { Message = "Error occured while scanning product.", ErrorCode = e.ErrorCode.ToString() } });
            logger.Error("Error occured while scanning product.", e.ErrorCode);
        }
        public string CheckDeviceHealth()
        {
            try
            {
                string res = scanner.CheckHealth(HealthCheckLevel.Interactive);
                return "CheckHealth(Internal) returned: " + res;

            }
            catch (Exception ex)
            {
                return logger.GetPosException(ex);
            }
        }
      
        private void scanner_DataEvent(object sender, DataEventArgs e)
        {
            logger.Info(CheckDeviceHealth());
            ASCIIEncoding asciiEncoding = new ASCIIEncoding();
            var scandata = asciiEncoding.GetString(scanner.ScanDataLabel);
            logger.Info("Barcode Scanned " + scandata);
            Scanned?.Invoke(this, new ScannedEventArgs { UPOS=new ScanUpos { UPC = scandata } });
            Close();
        }
        private void posExplorer_DeviceRemovedEvent(object sender, DeviceChangedEventArgs e)
        {
            if (e.Device.Type == "Scanner")
            {
                scanner.DataEventEnabled = false;
                scanner.DeviceEnabled = false;
                scanner.Release();
                scanner.Close();
                logger.Info("Scanner removed");
            }
        }
        
        private void posExplorer_DeviceAddedEvent(object sender, DeviceChangedEventArgs e)
        {
            if (e.Device.Type == "Scanner")
            {
                scanner = (Scanner)posExplorer.CreateInstance(e.Device);
                scanner.Open();
                scanner.Claim(1000);
                scanner.DataEvent += new DataEventHandler(scanner_DataEvent);
                scanner.DeviceEnabled = true;
                scanner.DataEventEnabled = true;
                scanner.DecodeData = true;
                logger.Info("Scanner Attached");
            }
        }
    }
}
