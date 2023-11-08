using System;
using upos_device_simulation.Models;

namespace upos_device_simulation.Interfaces
{
    public interface IReceiptPrinter
    {
        event EventHandler<PrintInfo> OnPrintComplete;

        event EventHandler<ErrorInfo> ReceiptPrinterError;
        void Start(PrintInfo printInfo);
        string CheckDeviceHealth();
    }
}
