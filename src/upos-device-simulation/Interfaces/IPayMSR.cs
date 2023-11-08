using System;
using upos_device_simulation.Models;

namespace upos_device_simulation.Interfaces
{
    public interface IPayMsr
    {
        event EventHandler<UposPaymnetInfo> OnCardSwiped;
        event EventHandler<ErrorInfo> MsrError;
        void Start(ApiPaymentInfo payInfo = null);
        string CheckDeviceHealth();

    }
}
