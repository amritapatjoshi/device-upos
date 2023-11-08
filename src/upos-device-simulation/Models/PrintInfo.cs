namespace upos_device_simulation.Models
{
   public class PrintInfo
    {
        public string OrderId { get; set; }
        public string LaneId { get; set; }
        public string TransactionId { get; set; }
        public PosReceipt POS { get; set; }
       
    }
    public class PosReceipt
    {
        public string ReceiptMetadata { get; set; }

        public bool IsPrintingCompleted { get; set; }
    }
}
