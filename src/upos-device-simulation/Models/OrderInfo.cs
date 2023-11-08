namespace upos_device_simulation.Models
{
    public class OrderInfo
    {
        public string OrderId { get; set; }
        public string LaneId { get; set; }
        public string TransactionId { get; set; }
    }
    public class MposPayment 
    {
        public decimal Amount { get; set; }
    }
    public class ApiPaymentInfo : OrderInfo
    {
        public MposPayment MPOS { get; set; }
    }
   

    public class UposPaymnetInfo :OrderInfo 
    {
        public UposPayment UPOS { get; set; }
    }

  
    public class UposPayment
    {
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public string ExpirationDate { get; set; }
        public string ServiceCode { get; set; }
        public string Pin { get; set; }
        public decimal Amount { get; set; }
        public bool PaymentStatus { get; set; }
        public string Title { get; set; }
        public string Suffix { get; set; }

    }
}
