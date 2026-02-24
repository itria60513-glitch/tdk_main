using Communication.Interface;
using EFEM.DataCenter;
using EFEM.FileUtilities;
using LogUtility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Connector.Enum;
using Communication.Protocol;
using TDKLogUtility.Module;
using SerialPort = System.IO.Ports.SerialPort;
namespace Communication.Connector
{
    internal class Rs232b: IConnector
    {
        #region Private Data
        private Mutex mut = new Mutex();
        private IProtocol _protocol;
        private SerialPort handle = null;
        private IConnectorConfig _config = null;
        private string _objName = "Carrier1";
        private ExceptionDictionary _ExDictionary = null;
        private ILogUtility _log = null;
        #endregion

        #region Property
        public IProtocol Protocol
        {
            set { this._protocol = value; }
            get { return this._protocol; }
        }
        public string Name
        {
            set { _objName = value; }
            get { return _objName; }
        }
        #endregion Property

        #region Constructors
        public Rs232b(ILogUtility log, IConnectorConfig config)
        {
            this._protocol = new DefaultProtocol();
            _log = log;
            _config = config;
        }
        public Rs232b(IProtocol protocol, ILogUtility log, IConnectorConfig config)
        {
            this._protocol = protocol;
            _log = log;
            _config = config;
        }
        #endregion

        #region Event Declarations
        public event ReceivedDataEventHandler DataReceived;
        private void Fire_DataReceived(byte[] byData, int Length)
        {
            if (DataReceived != null)
            {
                DataReceived(byData, Length);
                WriteLog("Event forwarded==> ", byData, Length);
            }
            else
            {
                WriteLog("Cannot forward an event.", byData, Length);
            }
        }
        #endregion

        #region Event Sink
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int readBytes;
                byte[] byInBuffer = new byte[_protocol.BufferSize];
                if (handle.BytesToRead > 0)
                {
                    readBytes = handle.Read(byInBuffer, 0, _protocol.BufferSize);
                    this._protocol.Push(byInBuffer, readBytes);
                }

                do
                {
                    readBytes = this._protocol.Pop(ref byInBuffer);
                    if (readBytes > 0)
                    {
                        if (this._protocol.VerifyInFrameStructure(byInBuffer, readBytes))
                        {
                            Fire_DataReceived(byInBuffer, readBytes);
                        }
                    }
                }
                while (readBytes > 0);
            }
            catch (Exception ex)
            {
                WriteLog(4, "Received [Exception]==> " + ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region Log Services
        private void WriteLog(int category, string msg)
        {
            if (_log == null)
                return;
            string head;
            switch (category)
            {
                case 4:
                    head = String.Format("[SOFT_EXCEPTION] {0}", msg);
                    break;
                case 10:
                    head = String.Format("[INFO] {0}", msg);
                    break;
                case 11:
                    head = String.Format("[KEEP EYES ON] {0}", msg);
                    break;
                case 12:
                    head = String.Format("[CALL] {0}", msg);
                    break;
                case 100:
                    head = msg;
                    break;
                default:
                    return;
            }
            _log.WriteLog("Rs232-" + _objName, head);
        }

        private void WriteLog(int category, string msg, uint alid)
        {
            if (_log == null)
                return;
            string head;
            switch (category)
            {
                case 0:
                    head = string.Format("[EX_NOTIFY][{0}] {1}", alid, msg);
                    break;
                case 1:
                    head = string.Format("[EX_WARNING][{0}] {1}", alid, msg);
                    break;
                case 2:
                    head = string.Format("[EX_ERROR][{0}] {1}", alid, msg);
                    break;
                case 3:
                    head = string.Format("[EX_ALARM][{0}] {1}", alid, msg);
                    break;
                default:
                    return;
            }
            _log.WriteLog("Rs232-" + _objName, head);
        }

        private void WriteLog(string msg, byte[] byPtBuf, int length)
        {
            if (_log == null)
                return;
            string head = "[DATA] " + msg + " ";
            int len = length;
            if (len > 1024)
                len = 1024;
            for (int i = 0; i < len; i++)
            {
                string cc;
                if ((byPtBuf[i] >= (byte)33 && byPtBuf[i] <= (byte)122) || byPtBuf[i] == (byte)0x20)
                    cc = string.Format("{0}", (char)byPtBuf[i]);
                else
                    cc = string.Format("({0})", byPtBuf[i].ToString("X"));
                head = string.Format("{0}{1}", head, cc);
            }
            string aa = string.Format("  Length:{0}", length);
            head = string.Format("{0}{1}", head, aa);
            _log.WriteLog("Rs232-" + _objName, head);
        }
        #endregion

        #region Public Methods

        private bool TryParseParity(int value, out Parity result)
        {
            if (System.Enum.IsDefined(typeof(Parity), value))
            {
                result = (Parity)value;
                return true;
            }

            result = Parity.None;
            return false;
        }

        public bool TryParseStopBits(int value, out StopBits result)
        {
            if (System.Enum.IsDefined(typeof(StopBits), value))
            {
                result = (StopBits)value;
                return true;
            }

            result = StopBits.None;
            return false;
        }

        private Parity ParseParity(int val) => TryParseParity(val, out var p)
            ? p
            : Parity.None;
        private StopBits ParseStopBits(int val) => TryParseStopBits(val, out var s)
            ? s
            : StopBits.None;
        public HRESULT Connect()
        {
            mut.WaitOne();
            try
            {
                if (handle != null)
                {
                    WriteLog(12, "Connect()");

                    if (handle != null)
                    {
                        WriteLog(10, "Port is in use. Call Disconnect() first.");

                        HRESULT hr = _ExDictionary["FAIL_TO_OPEN_PORT"].hRESULT;
                        WriteLog((int)hr._category, hr._message, hr.ALID);
                        return MAKEHR(hr);
                    }
                }

                WriteLog(12, string.Format("Connect(port:{0}, baud rate:{1}, parity:{2}, data bits:{3}, stop bitd:{4})",
                                            _config.Port.ToString(), (int)_config.Baud, _config.Parity.ToString(),
                                            (int)_config.DataBits, _config.StopBits.ToString()));
                handle = new System.IO.Ports.SerialPort(_config.Port.ToString(), (int)_config.Baud, ParseParity(_config.Parity),
                    (int)_config.DataBits, ParseStopBits(_config.StopBits));
                handle.DtrEnable = true;
                handle.RtsEnable = false;

                try
                {
                    handle.ReadBufferSize = _protocol.BufferSize;
                    handle.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//This DataReceived is .NET Serial Event
                    handle.Open();
                }
                catch (Exception ex)
                {
                    WriteLog(4, ex.Message + ex.StackTrace);
                    WriteLog(4, "[Recovery] Release SerialPort associated resources...");
                    handle.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
                    handle.Close();
                    handle.Dispose();
                    handle = null;
                    HRESULT hr = _ExDictionary["FAIL_TO_OPEN_PORT"].hRESULT;
                    WriteLog((int)hr._category, hr._message, hr.ALID);
                    return MAKEHR(hr);
                }
                return null;
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        public void Disconnect()
        {
            mut.WaitOne();
            try
            {
                WriteLog(12, "Disconnect()");
                if (handle != null)
                {
                    handle.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
                    if (handle.IsOpen)
                        handle.Close();
                    handle.Dispose();
                    this._protocol.Purge();
                    handle = null;
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        public HRESULT Send(byte[] byPtBuf, int Length)
        {
            if (Length == 0)
                return null;

            mut.WaitOne();
            try
            {
                if (handle == null || !handle.IsOpen)
                {
                    HRESULT hr = _ExDictionary["PORT_NOT_OPEN"].hRESULT;
                    WriteLog((int)hr._category, "Send() " + hr._message, hr.ALID);
                    return MAKEHR(hr);
                }

                Length = this._protocol.AddOutFrameInfo(ref byPtBuf, Length);

                try
                {
                    WriteLog("Sending==> ", byPtBuf, Length);
                    handle.Write(byPtBuf, 0, Length);
                }
                catch (Exception ex)
                {
                    WriteLog(4, ex.Message + ex.StackTrace);

                    HRESULT hr = _ExDictionary["FAIL_TO_SEND_DATA"].hRESULT;
                    WriteLog((int)hr._category, hr._message, hr.ALID);
                    return MAKEHR(hr);
                }
                return null;
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        private HRESULT MAKEHR(HRESULT hr)
        {
            hr._extramessage = string.Format("Target device: {0}", _objName);
            return hr;
        }
        #endregion
    }
}
