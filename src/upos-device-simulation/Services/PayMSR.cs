using Microsoft.PointOfService;
using System;
using upos_device_simulation.Interfaces;
using upos_device_simulation.Models;

namespace upos_device_simulation.Services
{
    public class PayMsr : IPayMsr
    {

        private readonly PosExplorer posExplorer;
        private Msr msr;
        public event EventHandler<UposPaymnetInfo> OnCardSwiped;
        public event EventHandler<ErrorInfo> MsrError;
        readonly ILogger logger;
        UposPaymnetInfo paymentInfo;
        public PayMsr(ILogger logger, PosExplorer explorer)
        {
            this.logger = logger;
            posExplorer = explorer;
            posExplorer.DeviceAddedEvent += new DeviceChangedEventHandler(posExplorer_DeviceAddedEvent);
            posExplorer.DeviceRemovedEvent += new DeviceChangedEventHandler(posExplorer_DeviceRemovedEvent);
        }

        public void Start(ApiPaymentInfo payInfo = null)
        {
            logger.Info("Getting MSR Device from posExplorer.");
            DeviceInfo device = posExplorer.GetDevices(DeviceType.Msr)[0];
            logger.Info("Got MSR Device.");
            msr = (Msr)posExplorer.CreateInstance(device);
            msr.DataEvent += new DataEventHandler(msr_DataEvent);
            msr.ErrorEvent += new DeviceErrorEventHandler(msr_ErrorEvent);
            msr.Open();
            msr.Claim(1000);
            msr.DeviceEnabled = true;
            msr.DataEventEnabled = true;
            msr.DecodeData = true;
            msr.ParseDecodeData = true;
            paymentInfo = new UposPaymnetInfo
            {
                LaneId = payInfo.LaneId,
                OrderId = payInfo.OrderId,
                TransactionId = payInfo.TransactionId,
                UPOS = new UposPayment() { Amount = payInfo.MPOS.Amount }
            };

            logger.Info("MSR Started.");

        }

        public string CheckDeviceHealth()
        {
            try
            {
                string res = msr.CheckHealth(HealthCheckLevel.Internal);
                return "CheckHealth(Internal) returned: " + res;

            }
            catch (Exception ex)
            {
                return logger.GetPosException(ex);
            }
        }
        private void msr_DataEvent(object sender, DataEventArgs e)
        {
            string name = msr.FirstName + " " + msr.MiddleInitial + " " + msr.Surname;
            paymentInfo.UPOS.AccountNumber = msr.AccountNumber;
            paymentInfo.UPOS.Name = name;
            paymentInfo.UPOS.ExpirationDate = msr.ExpirationDate;
            paymentInfo.UPOS.Suffix = msr.Suffix;
            paymentInfo.UPOS.ServiceCode = msr.ServiceCode;
            paymentInfo.UPOS.Title = msr.Title;
            logger.Info("Card Swiped. Card Details is as below  AccountNumber: " + paymentInfo.UPOS.AccountNumber + " Name : " + paymentInfo.UPOS.Name + " ExpirationDate: " + paymentInfo.UPOS.ExpirationDate + " Service Code" + paymentInfo.UPOS.ServiceCode);
            OnCardSwiped?.Invoke(this, paymentInfo);
            Close();
        }
        private void Close()
        {
            msr.DeviceEnabled = false;
            msr.DataEventEnabled = false;
            msr.Release();
            msr.Close();
        }

        private void msr_ErrorEvent(object sender, DeviceErrorEventArgs e)
        {
            string errorMessage = null;

            if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == Msr.ExtendedErrorStart)
                errorMessage = "Indicates a start sentinel error.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == Msr.ExtendedErrorEnd)
                errorMessage = "Indicates an end sentinel error.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == Msr.ExtendedErrorParity)
                errorMessage = "Indicates a parity error";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == Msr.ExtendedErrorLrc)
                errorMessage = "Indicates an LRC error.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == Msr.ExtendedErrorDeviceAuthenticationFailed)
                errorMessage = "ndicates an extended error where the device authentication process failed.";
            else if (e.ErrorCode == ErrorCode.Extended || e.ErrorCodeExtended == Msr.ExtendedErrorDeviceDeauthenticationFailed)
                errorMessage = "Indicates an extended error where the device deauthentication failed.";
            else
                errorMessage = "Error while reading swiped card details.";
            logger.Error(errorMessage,e.ErrorCode);
            MsrError?.Invoke(this, new ErrorInfo { UposError = new UPOSErrorInfo { Message = errorMessage, ErrorCode = e.ErrorCode.ToString(), ErrorCodeExtended = e.ErrorCodeExtended.ToString()} });

        }

        private void posExplorer_DeviceRemovedEvent(object sender, DeviceChangedEventArgs e)
        {
            if (e.Device.Type == "msr")
            {
                msr.DeviceEnabled = false;
                msr.Release();
                msr.Close();
                logger.Info("MSR removed");
            }
        }

        private void posExplorer_DeviceAddedEvent(object sender, DeviceChangedEventArgs e)
        {
            if (e.Device.Type == "msr")
            {
                msr = (Msr)posExplorer.CreateInstance(e.Device);
                msr.Open();
                msr.Claim(1000);
                msr.DeviceEnabled = true;
                logger.Info("MSR Attached");
            }
        }
    }
}
