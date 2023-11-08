using Microsoft.PointOfService;
using Serilog;
using Serilog.Sinks.Graylog;
using System;
using System.Configuration;
using upos_device_simulation.Interfaces;
using ILogger = Serilog.ILogger;

namespace upos_device_simulation.Helpers
{
    public class Logger : Interfaces.ILogger
    {
        private static ILogger log;
        private static string logFileName="log.txt";
        readonly LoggerConfiguration _loggerConfiguration;
        readonly GraylogSinkOptions _graylogSinkOptions;


        public Logger(LoggerConfiguration loggerConfiguration,IFileHelper fileHelper, GraylogSinkOptions graylogSinkOptions)
        {
            _loggerConfiguration = loggerConfiguration;
            _graylogSinkOptions = graylogSinkOptions;
            fileHelper.CreateLogFile(logFileName);
            CreateLogger();
        }
       
        private  void CreateLogger()
        {
            _graylogSinkOptions.HostnameOrAddress = ConfigurationManager.AppSettings["loggerHostanme"];
            _graylogSinkOptions.Port = Convert.ToInt32(ConfigurationManager.AppSettings["loggerPort"]);
            log = _loggerConfiguration
                    .WriteTo.Console()
                    .WriteTo.File(logFileName)
                    .WriteTo.Graylog(_graylogSinkOptions)
                    .CreateLogger();
        }
        public  void Info(string message)
        {
            log.Information(message);
        }
        public  void Error(string message,ErrorCode errorCode)
        {
            log.Error(message + " " + GetErrorDescription(errorCode));
        }
        public  void Error(string message)
        {
            log.Error(message);
        }
        public  void Debug(string message)
        {
            log.Debug(message);
        }
        public  void Verbose(string message)
        {
            log.Verbose(message);
        }

        private   string GetErrorDescription (ErrorCode errorCode)
        {
            switch(errorCode)
            {
                case ErrorCode.Closed: return "The device must be opened.";
                case ErrorCode.NotClaimed: return " The device is opened but not claimed. No other application has the device claimed,so it can and must be claimed.";
                case ErrorCode.NoService: return " The control cannot communicate with the service object, normally because of a setup or configuration error.";
                case ErrorCode.Disabled : return " The device is opened and claimed (if this is an exclusive use device), but not enabled.";
                case ErrorCode.Illegal: return " An attempt was made to perform an illegal or unsupported operation with the device,or an invalid parameter value was used.";
                case ErrorCode.NoHardware: return "The physical device is not connected to the system or is not powered on.";
                case ErrorCode.Offline: return "The physical device is off-line.";
                case ErrorCode.NoExist: return "The file name (or other specified value) does not exist.";
                case ErrorCode.Exists: return "The file name (or other specified value) already exists.";
                case ErrorCode.Failure: return " The device cannot perform the requested procedure, even though the physical device is connected to the system, powered on, and on-line.";
                case ErrorCode.Timeout: return " The service object timed out waiting for a response from the physical device or the control timed out waiting for a response from the service object.";
                case ErrorCode.Busy: return "The current state does not allow this request. For example: if asynchronous output is in progress, certain methods may not be allowed.";
                case ErrorCode.Deprecated : return " The method has been deprecated and is no longer available.";
                case ErrorCode.Extended:return "A device category-specific error condition occurred. The error condition code is held in the exception's Microsoft.PointOfService.PosControlException.ErrorCodeExtende property.";
                default: return "POS Error Occured";
            }

        }

        public string GetPosException(Exception e)
        {
            string error;
            Exception inner = e.InnerException;
            if (inner != null)
            {
                GetPosException(inner);
            }

            if (e is PosControlException)
            {
                PosControlException pe = (PosControlException)e;

                error =
                    "POSControlException ErrorCode(" +
                    pe.ErrorCode.ToString() +
                    ") ExtendedErrorCode(" +
                    pe.ErrorCodeExtended.ToString(System.Globalization.CultureInfo.CurrentCulture) +
                    ") occurred: " +
                    pe.Message;
            }
            else
            {
                error = e.ToString();
            }
            return error;
        }

    }
}
