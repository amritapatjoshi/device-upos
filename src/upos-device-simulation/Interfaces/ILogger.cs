using Microsoft.PointOfService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace upos_device_simulation.Interfaces
{
    public interface ILogger
    {
       // void CreateLogger();
        void Info(string message);
        void Error(string message, ErrorCode errorCode);
        void Error(string message);
        void Debug(string message);
        void Verbose(string message);
        string GetPosException(Exception ex);
    }
}
