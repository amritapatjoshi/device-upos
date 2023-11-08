using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace upos_device_simulation.Models
{
    public class ErrorInfo : EventArgs
    {
        public UPOSErrorInfo UposError { get; set; }
        public string OrderId { get; set; }
        public string LaneId { get; set; }
        public string TransactionId { get; set; }
    }
    public class UPOSErrorInfo
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorCodeExtended { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
    }
}


