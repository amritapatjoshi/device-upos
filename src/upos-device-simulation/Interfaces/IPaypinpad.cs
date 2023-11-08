using System;
using upos_device_simulation.Models;

namespace upos_device_simulation.Interfaces
{
    public interface IPaypinpad
    {
        event EventHandler<UposPaymnetInfo> OnPinEntered;
        
        event EventHandler<ErrorInfo> PayPinpadError;
        void Start(UposPaymnetInfo payInfo = null);
        string CheckDeviceHealth();
    }
}
