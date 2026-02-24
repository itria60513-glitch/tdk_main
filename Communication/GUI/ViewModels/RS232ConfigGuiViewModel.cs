using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.GUI.ViewModels
{
    class RS232ConfigGuiViewModel : ViewModelBase
    {
        public RS232ConfigGuiViewModel(SynchronizationContext ctx = null) : base(ctx)
        {

        }

        private int _portNumber;
        public int PortNumber
        {
            get => _portNumber;
            set => SetProperty(ref _portNumber, value);
        }

        private int _baudRate;
        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        private int _stopBits;
        public int StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        private int _dataBits;
        public int DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        private int _parity;
        public int Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }
    }
}
