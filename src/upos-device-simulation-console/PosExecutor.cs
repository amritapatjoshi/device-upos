using System;
using System.Configuration;
using upos_device_simulation.Models;
using upos_device_simulation.Interfaces;
using System.Threading.Tasks;
using SocketIOClient;
using Newtonsoft.Json;
using upos_device_simulation.Helpers;
using upos_device_simulation.Services;

namespace UposDeviceSimulationConsole
{
    public class PosExecutor
    {
        readonly ILogger logger;
        readonly IBarcodeScanner barcodeScanner;
        readonly IReceiptPrinter receiptPrinter;
        readonly IPayMsr payMSR;
        readonly IPaypinpad paypinpad;
        readonly SocketClient _socketClient;
        readonly Utils utils;
        static DeviceConnectionInfo deviceConnectionInfo = new DeviceConnectionInfo();
        static OrderInfo _orderInfo =new OrderInfo();

        public PosExecutor(ILogger logger, IBarcodeScanner barcodeScanner, IPaypinpad paypinpad, IPayMsr payMSR, IReceiptPrinter receiptPrinter,SocketClient socketClient, Utils utils)
        {
            this.logger = logger;   
            this.barcodeScanner=barcodeScanner;
            this.paypinpad=paypinpad;
            this.payMSR = payMSR;
            this.receiptPrinter=receiptPrinter;
            _socketClient = socketClient;
            this.utils = utils;
        }
        public  void Execute()
        {
            try
            {
                logger.Info("upos Service started.");
                Task.Run(()=>ConnectSocket());
                logger.Info("Starting scanner simmulator");
                barcodeScanner.Scanned += BarcodeScanner_Scanned;
                barcodeScanner.ScannedError += Device_ErrorHandler;
                barcodeScanner.Start();
                Console.ReadLine();
                
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured in PosExecutor" + ex.Message );
            }
           
        }
        async Task ConnectSocket() {
            await _socketClient.Connect();
            _socketClient.client.On(ConfigurationManager.AppSettings["SCOConnectEvent"], OnSCOConnect);
            _socketClient.client.On(ConfigurationManager.AppSettings["SCOConnectCompleteEvent"], OnSCOConnectComplete);
            _socketClient.client.On(ConfigurationManager.AppSettings["APIOrderEvent"], OnAPIOrder);
            _socketClient.client.On(ConfigurationManager.AppSettings["APIPaymentEvent"], OnAPIPayment);
            _socketClient.client.On(ConfigurationManager.AppSettings["APICardVerifyCompleteEvent"], OnAPICardVerifyComplete);
            _socketClient.client.On(ConfigurationManager.AppSettings["APIPinPadCompleteEvent"], OnAPIPinPadCompleteEvent);
            _socketClient.client.On(ConfigurationManager.AppSettings["APIReceiptEvent"], OnAPIReceiptEvent); 
        }

        private void PayMsr_CardSwiped(object sender, UposPaymnetInfo paymentInfo)
        {
            try
            {
                logger.Info("Card Swiped and read user card Details ");
                _socketClient.EmitEvent(ConfigurationManager.AppSettings["UPOSMSREvent"], paymentInfo);
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured in PayMsr_CardSwiped." + ex.Message + ex.StackTrace);
            }

        }
        private void ReceiptPrinter_PrintComplete(object sender, PrintInfo printInfo)
        {
            try
            {
                logger.Info("Print receipt completed");
                _socketClient.EmitEvent(ConfigurationManager.AppSettings["UPOSReceiptCompleteEvent"], printInfo);
                barcodeScanner.Start();
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured in ReceiptPrinter_PrintComplete." + ex.Message + ex.StackTrace);
            }

        }

        private  void BarcodeScanner_Scanned(object sender, ScannedEventArgs e)
        {
            try 
            { 
                logger.Info("UPC: " + e.UPOS.UPC + " scanned and sending to UI");
               _socketClient.EmitEvent(ConfigurationManager.AppSettings["UPOSScanEvent"], e);
                logger.Info("Barcode send to UI");
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured in BarcodeScanner_Scanned." + ex.Message + ex.StackTrace);
            }
        }
        private void Device_ErrorHandler(object sender, ErrorInfo errorInfo)
        {
            try
            {
                errorInfo.LaneId = deviceConnectionInfo.LaneId;
                errorInfo.OrderId = _orderInfo.OrderId;
                errorInfo.TransactionId = _orderInfo.TransactionId;
                _socketClient.EmitEvent(ConfigurationManager.AppSettings["UPOSErrorEvent"], errorInfo);
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured in Device_ErrorHandler." + ex.Message + ex.StackTrace);
                _socketClient.EmitEvent(ConfigurationManager.AppSettings["UPOSErrorEvent"], ("Exception occured in Device_ErrorHandler." + ex.Message + ex.StackTrace));
            }
        }
        private void PayPinpad_PinEntered(object sender, UposPaymnetInfo paymentInfo)
        {
            try
            {
                logger.Info("Pin : " + paymentInfo.UPOS.Pin + " Entered and sending Payment details to UI");
                _socketClient.EmitEvent(ConfigurationManager.AppSettings["UPOSPinPadEvent"], paymentInfo);
                logger.Info("Payment details send to UI");
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured in PayPinpad_PinEntered." + ex.Message + ex.StackTrace);
            }

        }
        public void OnSCOConnect(SocketIOResponse socketIOResponse)
        {
            var deviceInfo = JsonConvert.DeserializeObject<DeviceConnectionInfo>(socketIOResponse.GetValue<object>().ToString());
            deviceInfo.MacId = utils.GetDeviceId();
            deviceConnectionInfo.MacId = deviceInfo.MacId;
            deviceConnectionInfo.UniqueId = deviceInfo.UniqueId;
            logger.Info("Macid : " + deviceConnectionInfo.MacId + " , Unique Id : " + deviceConnectionInfo.UniqueId);
            _socketClient.EmitEvent(ConfigurationManager.AppSettings["UPOSConnectEvent"], deviceInfo);

        }
        public void OnSCOConnectComplete(SocketIOResponse socketIOResponse)
        {
            var deviceInfo = JsonConvert.DeserializeObject<DeviceConnectionInfo>(socketIOResponse.GetValue<object>().ToString());
            if (deviceInfo.MacId == deviceConnectionInfo.MacId)
            {
                deviceConnectionInfo.LaneId = deviceInfo.LaneId;
                logger.Info("SCO and UPOS connection successful with MacId : " + deviceConnectionInfo.MacId + " , Lane Id : " + deviceConnectionInfo.LaneId);
            }
            else
            {
                logger.Error("SCO connection MacId : " + deviceInfo.MacId + " , LaneId: "+deviceInfo.LaneId+" is not matching with UPOS MacId : " + deviceConnectionInfo.MacId + " , LaneId :"+deviceConnectionInfo.LaneId+ "in SCOConnectComplete event.");
            }
        }
        public void OnAPIOrder(SocketIOResponse socketIOResponse)
        {
            var orderInfo = JsonConvert.DeserializeObject<OrderInfo>(socketIOResponse.GetValue<object>().ToString());
            if (deviceConnectionInfo.LaneId == orderInfo.LaneId)
            {
                _orderInfo.LaneId = orderInfo.LaneId;
                _orderInfo.OrderId = orderInfo.OrderId;
                _orderInfo.TransactionId = orderInfo.TransactionId;
                logger.Info("Starting Scanner");
                barcodeScanner.Scanned -= BarcodeScanner_Scanned;
                barcodeScanner.Scanned += BarcodeScanner_Scanned;
                barcodeScanner.ScannedError -= Device_ErrorHandler;
                barcodeScanner.ScannedError += Device_ErrorHandler;
                barcodeScanner.Start();
            }
            else
            {
                logger.Error("Device Connection LaneId: " + deviceConnectionInfo.LaneId + " is not matching with order LaneId: " + orderInfo.LaneId + "in APIOrder event.");
            }
           
        }
        public void OnAPIPinPadCompleteEvent(SocketIOResponse socketIOResponse)
        {
            logger.Info("Payment Process completed.");
        }

        public void OnAPICardVerifyComplete(SocketIOResponse socketIOResponse)
        {
            logger.Info("Starting Pinpad");
            var paymentIfo  = JsonConvert.DeserializeObject<UposPaymnetInfo>(socketIOResponse.GetValue<object>().ToString());
            if (paymentIfo.OrderId== _orderInfo.OrderId && paymentIfo.TransactionId==_orderInfo.TransactionId)
            {
                paypinpad.OnPinEntered -= PayPinpad_PinEntered;
                paypinpad.OnPinEntered += PayPinpad_PinEntered;
                paypinpad.PayPinpadError -= Device_ErrorHandler;
                paypinpad.PayPinpadError += Device_ErrorHandler;

                paypinpad.Start(paymentIfo);
            }
            else
            {
                logger.Error("OrderInfo orderId: " + _orderInfo.OrderId + " is not matching with payment OrderId: " + paymentIfo.OrderId + " in APICardVerifyComplete event.");
            }
        }
        public void OnAPIPayment(SocketIOResponse socketIOResponse)
        {
            var apiPaymentIfo = JsonConvert.DeserializeObject<ApiPaymentInfo>(socketIOResponse.GetValue<object>().ToString());
            logger.Info("Starting MSR");
            if (apiPaymentIfo.OrderId == _orderInfo.OrderId && apiPaymentIfo.TransactionId == _orderInfo.TransactionId)
            {
                payMSR.OnCardSwiped -= PayMsr_CardSwiped;
                payMSR.OnCardSwiped += PayMsr_CardSwiped;
                payMSR.MsrError -= Device_ErrorHandler;
                payMSR.MsrError += Device_ErrorHandler;
                payMSR.Start(apiPaymentIfo);
            }
            else
            {
                logger.Error("OrderInfo orderId: " + _orderInfo.OrderId + " is not matching with payment OrderId: " + apiPaymentIfo.OrderId + " in APIPayment event.");
            }
        }
        public void OnAPIReceiptEvent(SocketIOResponse socketIOResponse)
        {
            logger.Info("printing started");
            var printInfo= JsonConvert.DeserializeObject<PrintInfo>(socketIOResponse.GetValue<object>().ToString());
            receiptPrinter.OnPrintComplete -= ReceiptPrinter_PrintComplete;
            receiptPrinter.OnPrintComplete += ReceiptPrinter_PrintComplete;
            receiptPrinter.ReceiptPrinterError -= Device_ErrorHandler;
            receiptPrinter.ReceiptPrinterError += Device_ErrorHandler;

            receiptPrinter.Start(printInfo);
           
        }
    }
}
