using Communication.Connector.Enum;

namespace Communication.Interface
{
    public delegate void ReceivedDataEventHandler(byte[] byData, int length);
    public interface IConnector
    {
        IProtocol Protocol { set; get; }
        HRESULT Send(byte[] byPtBuf, int length);
        HRESULT Connect();
        void Disconnect();
        event ReceivedDataEventHandler DataReceived;
    }
}