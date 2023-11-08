using Microsoft.PointOfService;
using System;
using upos_device_simulation.Interfaces;
using upos_device_simulation.Models;

namespace upos_device_simulation.Services
{

    public class Paypinpad : IPaypinpad
    {
        private readonly PosExplorer posExplorer;
        private PinPad pinpad;
        public event EventHandler<UposPaymnetInfo> OnPinEntered;
        public event EventHandler<ErrorInfo> PayPinpadError;
        readonly ILogger logger;
        public UposPaymnetInfo PaymentInfo { get; set; }
        public Paypinpad(ILogger logger, PosExplorer explorer)
        {
            this.logger = logger;
            posExplorer = explorer;
            posExplorer.DeviceAddedEvent += new DeviceChangedEventHandler(posExplorer_DeviceAddedEvent);
            posExplorer.DeviceRemovedEvent += new DeviceChangedEventHandler(posExplorer_DeviceRemovedEvent);
        }

        public void Start(UposPaymnetInfo payInfo = null)
        {
            logger.Info("Getting PinPad Device from posExplorer.");
            DeviceInfo device = posExplorer.GetDevices(DeviceType.PinPad)[0];
            logger.Info("Got Pinpad Device");
            pinpad = (PinPad)posExplorer.CreateInstance(device);
            pinpad.DataEvent += new DataEventHandler(pinpad_DataEvent);
            pinpad.ErrorEvent += new DeviceErrorEventHandler(pinpad_ErrorEvent);
            pinpad.Open();
            pinpad.Claim(1000);
            pinpad.DeviceEnabled = true;
            pinpad.DataEventEnabled = true;
            if (payInfo != null)
            {
                pinpad.AccountNumber = payInfo.UPOS.AccountNumber;
                pinpad.Amount = payInfo.UPOS.Amount;
                PaymentInfo = payInfo;
            }

            PinPadSystem pps = (PinPadSystem)Enum.Parse(typeof(PinPadSystem), PinPadSystem.Dukpt.ToString());
            int transactionHost = int.Parse("1", System.Globalization.CultureInfo.CurrentCulture);
            pinpad.BeginEftTransaction(pps, transactionHost);
            enterPin();
            logger.Info("Pinpad Started");


        }
        public string CheckDeviceHealth()
        {
            try
            {
                string res = pinpad.CheckHealth(HealthCheckLevel.Internal);
                return "CheckHealth(Internal) returned: " + res;

            }
            catch (Exception ex)
            {
                return logger.GetPosException(ex);
            }
        }
        private void enterPin()
        {
            pinpad.EnablePinEntry();
        }

        private void EndEftTransaction()
        {
            pinpad.EndEftTransaction(EftTransactionCompletion.Normal);
        }

        private void pinpad_DataEvent(object sender, DataEventArgs e)
        {
            if ((PinEntryStatus)e.Status == PinEntryStatus.Success)
                logger.Info("EncryptedPIN = " + pinpad.EncryptedPin + "\r\nAdditionalSecurityInformation = " + pinpad.AdditionalSecurityInformation);
            else if ((PinEntryStatus)e.Status == PinEntryStatus.Cancel)
                logger.Info("Pin entry was cancelled.");
            else if ((PinEntryStatus)e.Status == PinEntryStatus.Timeout)
                logger.Info("A timeout condition occured in the pinpad.");
            else
                logger.Info("pinpad returned status code: " + e.Status.ToString(System.Globalization.CultureInfo.CurrentCulture));
            logger.Info("User enter Pin: " + PaymentInfo.UPOS.Pin);
            PaymentInfo.UPOS.Pin = pinpad.EncryptedPin;
            PaymentInfo.UPOS.AccountNumber = pinpad.AccountNumber;
            PaymentInfo.UPOS.Amount = pinpad.Amount;
            PaymentInfo.UPOS.PaymentStatus = e.Status == 1;
            OnPinEntered?.Invoke(this, PaymentInfo);
            Close();
            logger.Info("Pinpad Transaction Ended.");
          
        }
        private void Close()
        {
            EndEftTransaction();
            pinpad.DeviceEnabled = false;
            pinpad.DataEventEnabled = false;
            pinpad.Release();
            pinpad.Close();
        }
        private void pinpad_ErrorEvent(object sender, DeviceErrorEventArgs e)
        {
            string errorMessage = null;

            if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == PinPad.ExtendedErrorBadKey)

                errorMessage = "An encryption key is corrupted or missing.";

            else
                errorMessage = "Error while reading user entered pin.";

            logger.Error(errorMessage, e.ErrorCode);
            PayPinpadError?.Invoke(this, new ErrorInfo { UposError = new UPOSErrorInfo { Message = errorMessage, ErrorCode = e.ErrorCode.ToString(), ErrorCodeExtended = e.ErrorCodeExtended.ToString() } });

            Close();

        }

        private void posExplorer_DeviceRemovedEvent(object sender, DeviceChangedEventArgs e)
        {
            if (e.Device.Type == "pinpad")
            {
                pinpad.DeviceEnabled = false;
                pinpad.Release();
                pinpad.Close();
                logger.Info("Pinpad removed.");
            }
        }

        private void posExplorer_DeviceAddedEvent(object sender, DeviceChangedEventArgs e)
        {
            if (e.Device.Type == "pinpad")
            {
                pinpad = (PinPad)posExplorer.CreateInstance(e.Device);
                pinpad.Open();
                pinpad.Claim(1000);
                pinpad.DeviceEnabled = true;
                logger.Info("Pinpad Attached.");
            }
        }

    }
}
