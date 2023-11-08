using System;
using System.Threading;
using Microsoft.PointOfService;
using upos_device_simulation.Interfaces;
using upos_device_simulation.Models;

namespace upos_device_simulation.Services
{

    public class ReceiptPrinter: IReceiptPrinter
    {
        private readonly ILogger logger;
        private readonly PosExplorer posExplorer;
        private PosPrinter printer;
        public event EventHandler<PrintInfo> OnPrintComplete;
        private PrintInfo _printInfo;
        public event EventHandler<ErrorInfo> ReceiptPrinterError;
        public ReceiptPrinter(ILogger logger, PosExplorer explorer)
        {
            this.logger = logger;
            posExplorer = explorer;
            posExplorer.DeviceAddedEvent += new DeviceChangedEventHandler(posExplorer_DeviceAddedEvent);
            posExplorer.DeviceRemovedEvent += new DeviceChangedEventHandler(posExplorer_DeviceRemovedEvent);
        }

        public void Start(PrintInfo printInfo)
        {
            logger.Info("Getting Printer");
            DeviceInfo device = posExplorer.GetDevices(DeviceType.PosPrinter)[0];
            logger.Info("Got Printer");
            printer = (PosPrinter)posExplorer.CreateInstance(device);
            printer.Open();
            printer.AsyncMode = true;
            printer.ErrorEvent += new DeviceErrorEventHandler(printer_ErrorEvent);
            printer.OutputCompleteEvent += new OutputCompleteEventHandler(printer_OutputCompleteEvent);
           
            _printInfo = printInfo;
            Print(printInfo.POS.ReceiptMetadata);
            logger.Info("Printer started");
        }
        public string CheckDeviceHealth()
        {
            try
            {
                string res = printer.CheckHealth(HealthCheckLevel.Internal);
                return "CheckHealth(Internal) returned: " + res;

            }
            catch (Exception ex)
            {
                return logger.GetPosException(ex);
            }
        }
        private void printer_ErrorEvent(object sender, DeviceErrorEventArgs e)
        {
            string errorMessage = null;

            if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorCoverOpen)
                errorMessage = "Indicates that the printer cover is open.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorJournalEmpty)
                errorMessage = "Indicates the journal station is out of paper.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorReceiptEmpty)
                errorMessage = "Indicates the receipt station is out of paper.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorSlipEmpty)
                errorMessage = "Indicates a form has not been inserted into the slip station.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorSlipForm)
                errorMessage = "Indicates a form is present while the printer is being taken out of from removal";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorTooBig)
                errorMessage = "Indicates the bitmap is either too wide to print without transformation, or too big .to tranform.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorBadFormat)
                errorMessage = "Indicates an unsupported format.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorJournalCartridgeRemoved)
                errorMessage = " Indicates the journal cartridge has been removed..";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorJournalCartridgeEmpty)
                errorMessage = "Indicates the journal cartridge is empty.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorJournalHeadCleaning)
                errorMessage = "Indicates the journal cartridge head is being cleaned.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorReceiptCartridgeRemoved)
                errorMessage = "Indicates the receipt cartridge has been removed.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorReceiptCartridgeEmpty)
                errorMessage = "Indicates the receipt cartridge is empty.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorReceiptHeadCleaning)
                errorMessage = "Indicates the receipt cartridge head is being cleaned.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorSlipCartridgeRemoved)
                errorMessage = " Indicates the slip cartridge has been removed.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorSlipCartridgeEmpty)
                errorMessage = "Indicates the slip cartridge is empty.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PosPrinter.ExtendedErrorSlipHeadCleaning)
                errorMessage = "Indicates the slip cartridge head is being cleaned.";
            else
                errorMessage = "Error occured while Printing receipt.";
            logger.Error(errorMessage, e.ErrorCode);
            ReceiptPrinterError?.Invoke(this, new ErrorInfo { UposError = new UPOSErrorInfo { Message = errorMessage, ErrorCode = e.ErrorCode.ToString(), ErrorCodeExtended = e.ErrorCodeExtended.ToString() } });

        }

        void posExplorer_DeviceRemovedEvent(object sender, DeviceChangedEventArgs e)
        {
            if (e.Device.Type == "printer")
            {
                printer.DeviceEnabled = false;
                printer.Release();
                printer.Close();
                logger.Info("Printer removed");
            }
        }
        void printer_OutputCompleteEvent(object sender, OutputCompleteEventArgs e)
        {
           _printInfo.POS.IsPrintingCompleted = true;
            Thread.Sleep(10000);
            Close();
            OnPrintComplete?.Invoke(this, _printInfo );
        }
        void Close()
        {
            printer.DeviceEnabled = false;
            printer.ClearOutput();
            printer.Release();
            printer.Close();
        }

        void Print(string data)
        {
            if (!printer.Claimed)
            {
                printer.Claim(1000);
                printer.DeviceEnabled = true;
            }
            if (!printer.CoverOpen)
            {
                printer.PrintNormal(PrinterStation.Receipt, data);
            }
            else
            {
                logger.Info("Close Printer Cover");
            }
        }
        void posExplorer_DeviceAddedEvent(object sender, DeviceChangedEventArgs e)
        {
            if (e.Device.Type == "printer")
            {
                printer = (PosPrinter)posExplorer.CreateInstance(e.Device);
                printer.Open();
                printer.Claim(1000);
                printer.DeviceEnabled = true;
                logger.Info("printer Attached");
            }
        }
    }
}
