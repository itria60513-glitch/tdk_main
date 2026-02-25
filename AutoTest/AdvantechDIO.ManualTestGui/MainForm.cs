using AdvantechDIO.Config;
using AdvantechDIO.Module;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using TDKLogUtility.Module;

namespace AdvantechDIO.ManualTestGui
{
    public partial class MainForm : Form
    {
        private const string UiLogKey = "AdvantechDIO.ManualTestGui";

        private readonly AdvantechDIOConfig _config;
        private readonly ILogUtility _logger;

        private AdvantechDIO.Module.AdvantechDIO _dio;
        private bool _isMonitoring;
        private bool _firstDiPopupShown;

        public MainForm()
        {
            InitializeComponent();

            _config = new AdvantechDIOConfig
            {
                Index = 0,
                DIPortCount = 2,
                DIPinCountPerPort = 8,
                DOPortCount = 1,
                DOPinCountPerPort = 8
            };

            _logger = new UiLogUtility();
            ApplyDefaultUiValues();
            UpdateUiState(false);
            WriteStatus("Application started with default values.");
        }

        private void ApplyDefaultUiValues()
        {
            txtDeviceId.Text = _config.Index.ToString(CultureInfo.InvariantCulture);
            txtDiPort.Text = "0";
            txtDiBit.Text = "0";
            txtDoPort.Text = "0";
            txtDoBit.Text = "0";
            txtDoValue.Text = "0";
            lblDiPortValue.Text = "--";
            lblDiBitValue.Text = "--";
            lblDoPortValue.Text = "--";
            lblDoBitValue.Text = "--";
            lblDeviceName.Text = "--";
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!TryParseNonNegative(txtDeviceId.Text, "DeviceID", out int deviceId))
                {
                    return;
                }

                EnsureDioInstance(deviceId);
                int result = _dio.Connect();
                WriteResult(nameof(_dio.Connect), result);

                if (result == 0)
                {
                    UpdateUiState(true);
                    lblDeviceName.Text = string.IsNullOrWhiteSpace(_dio.DeviceName) ? "--" : _dio.DeviceName;
                }
                else
                {
                    // Connect-time initialization failure is considered critical per FR-015A.
                    ShowCriticalPopup($"Connect failed. Code={result}.");
                    UpdateUiState(false);
                }
            }
            catch (Exception ex)
            {
                WriteStatus($"Connect exception: {ex.Message}");
                ShowCriticalPopup("Unexpected error during Connect.");
                UpdateUiState(false);
            }
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectCurrentDio();
        }

        private void BtnGetInput_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected())
            {
                return;
            }

            if (!TryValidateDiPort(out int portIndex))
            {
                return;
            }

            byte value;
            int result = _dio.GetInput(portIndex, out value);
            lblDiPortValue.Text = value.ToString(CultureInfo.InvariantCulture);
            WriteResult("GetInput", result, value.ToString(CultureInfo.InvariantCulture));
        }

        private void BtnGetInputBit_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected())
            {
                return;
            }

            if (!TryValidateDiPort(out int portIndex) || !TryValidateDiBit(out int bitIndex))
            {
                return;
            }

            byte value;
            int result = _dio.GetInputBit(portIndex, bitIndex, out value);
            lblDiBitValue.Text = value.ToString(CultureInfo.InvariantCulture);
            WriteResult("GetInputBit", result, value.ToString(CultureInfo.InvariantCulture));
        }

        private void BtnSetOutput_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected())
            {
                return;
            }

            if (!TryValidateDoPort(out int portIndex) || !TryValidateDoValue(out byte value))
            {
                return;
            }

            int result = _dio.SetOutput(portIndex, value);
            WriteResult("SetOutput", result);
        }

        private void BtnSetOutputBit_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected())
            {
                return;
            }

            if (!TryValidateDoPort(out int portIndex) || !TryValidateDoBit(out int bitIndex) || !TryValidateDoBitValue(out byte value))
            {
                return;
            }

            int result = _dio.SetOutputBit(portIndex, bitIndex, value);
            WriteResult("SetOutputBit", result);
        }

        private void BtnGetOutput_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected())
            {
                return;
            }

            if (!TryValidateDoPort(out int portIndex))
            {
                return;
            }

            byte value;
            int result = _dio.GetOutput(portIndex, out value);
            lblDoPortValue.Text = value.ToString(CultureInfo.InvariantCulture);
            WriteResult("GetOutput", result, value.ToString(CultureInfo.InvariantCulture));
        }

        private void BtnGetOutputBit_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected())
            {
                return;
            }

            if (!TryValidateDoPort(out int portIndex) || !TryValidateDoBit(out int bitIndex))
            {
                return;
            }

            byte value;
            int result = _dio.GetOutputBit(portIndex, bitIndex, out value);
            lblDoBitValue.Text = value.ToString(CultureInfo.InvariantCulture);
            WriteResult("GetOutputBit", result, value.ToString(CultureInfo.InvariantCulture));
        }

        private void BtnSnapStart_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected())
            {
                return;
            }

            int result = _dio.SnapStart();
            WriteResult("SnapStart", result);
            if (result == 0)
            {
                _isMonitoring = true;
                _firstDiPopupShown = false;
            }
        }

        private void BtnSnapStop_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected())
            {
                return;
            }

            int result = _dio.SnapStop();
            WriteResult("SnapStop", result);
            if (result == 0)
            {
                _isMonitoring = false;
            }
        }

        private void OnDiValueChanged(object sender, EventArgs e)
        {
            UiSafe(() =>
            {
                if (!_isMonitoring)
                {
                    return;
                }

                WriteStatus("DI value changed event received.");
                if (!_firstDiPopupShown)
                {
                    _firstDiPopupShown = true;
                    MessageBox.Show(this, "DI changed (first event after SnapStart).", "DI Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
        }

        private void OnExceptionOccurred(object sender, EventArgs e)
        {
            UiSafe(() =>
            {
                // FR-015A: ExceptionOccurred is always treated as critical.
                WriteStatus("Critical backend exception event received.");
                ShowCriticalPopup("Critical backend exception occurred. UI will reset to disconnected state.");
                ResetToDisconnectedState();
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectCurrentDio();
            if (_dio != null)
            {
                _dio.DI_ValueChanged -= OnDiValueChanged;
                _dio.ExceptionOccurred -= OnExceptionOccurred;
                _dio.Dispose();
                _dio = null;
            }
        }

        private void EnsureDioInstance(int deviceId)
        {
            if (_dio != null && _dio.IsConnected)
            {
                return;
            }

            if (_dio != null)
            {
                _dio.DI_ValueChanged -= OnDiValueChanged;
                _dio.ExceptionOccurred -= OnExceptionOccurred;
                _dio.Dispose();
                _dio = null;
            }

            _config.Index = deviceId;
            _dio = new AdvantechDIO.Module.AdvantechDIO(_logger, _config);
            _dio.DI_ValueChanged += OnDiValueChanged;
            _dio.ExceptionOccurred += OnExceptionOccurred;
        }

        private void DisconnectCurrentDio()
        {
            if (_dio == null)
            {
                UpdateUiState(false);
                return;
            }

            int result = _dio.Disconnect();
            WriteResult("Disconnect", result);
            _isMonitoring = false;
            _firstDiPopupShown = false;
            UpdateUiState(false);
            lblDeviceName.Text = "--";
        }

        private void ResetToDisconnectedState()
        {
            _isMonitoring = false;
            _firstDiPopupShown = false;

            if (_dio != null)
            {
                try
                {
                    _dio.Disconnect();
                }
                catch
                {
                    // Ignore secondary errors while forcing UI-safe state.
                }
            }

            UpdateUiState(false);
            lblDeviceName.Text = "--";
        }

        private bool EnsureConnected()
        {
            if (_dio == null || !_dio.IsConnected)
            {
                WriteStatus("Action rejected: device is not connected.");
                return false;
            }

            return true;
        }

        private void UpdateUiState(bool connected)
        {
            lblConnectionState.Text = connected ? "State: Connected" : "State: Disconnected";
            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
            grpDi.Enabled = connected;
            grpDo.Enabled = connected;
        }

        private bool TryParseNonNegative(string raw, string fieldName, out int value)
        {
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) || value < 0)
            {
                WriteStatus($"Validation failed: {fieldName} must be a non-negative integer.");
                return false;
            }

            return true;
        }

        private bool TryValidateDiPort(out int portIndex)
        {
            if (!TryParseNonNegative(txtDiPort.Text, "DI Port", out portIndex))
            {
                return false;
            }

            if (portIndex >= _config.DIPortCount)
            {
                WriteStatus($"Validation failed: DI port out of range [0..{_config.DIPortCount - 1}].");
                return false;
            }

            return true;
        }

        private bool TryValidateDiBit(out int bitIndex)
        {
            if (!TryParseNonNegative(txtDiBit.Text, "DI Bit", out bitIndex))
            {
                return false;
            }

            if (bitIndex >= _config.DIPinCountPerPort)
            {
                WriteStatus($"Validation failed: DI bit out of range [0..{_config.DIPinCountPerPort - 1}].");
                return false;
            }

            return true;
        }

        private bool TryValidateDoPort(out int portIndex)
        {
            if (!TryParseNonNegative(txtDoPort.Text, "DO Port", out portIndex))
            {
                return false;
            }

            if (portIndex >= _config.DOPortCount)
            {
                WriteStatus($"Validation failed: DO port out of range [0..{_config.DOPortCount - 1}].");
                return false;
            }

            return true;
        }

        private bool TryValidateDoBit(out int bitIndex)
        {
            if (!TryParseNonNegative(txtDoBit.Text, "DO Bit", out bitIndex))
            {
                return false;
            }

            if (bitIndex >= _config.DOPinCountPerPort)
            {
                WriteStatus($"Validation failed: DO bit out of range [0..{_config.DOPinCountPerPort - 1}].");
                return false;
            }

            return true;
        }

        private bool TryValidateDoValue(out byte value)
        {
            if (!byte.TryParse(txtDoValue.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                WriteStatus("Validation failed: DO value must be byte [0..255].");
                return false;
            }

            return true;
        }

        private bool TryValidateDoBitValue(out byte value)
        {
            if (!byte.TryParse(txtDoValue.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                WriteStatus("Validation failed: DO bit value must be 0 or 1.");
                return false;
            }

            if (value != 0 && value != 1)
            {
                WriteStatus("Validation failed: DO bit value must be 0 or 1.");
                return false;
            }

            return true;
        }

        private void WriteResult(string action, int resultCode, string valueText = null)
        {
            string suffix = valueText == null ? string.Empty : $", Value={valueText}";
            WriteStatus($"{action}: Code={resultCode}{suffix}");
        }

        private void WriteStatus(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            lstStatus.Items.Insert(0, line);
            if (lstStatus.Items.Count > 500)
            {
                lstStatus.Items.RemoveAt(lstStatus.Items.Count - 1);
            }
        }

        private void UiSafe(Action action)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private void ShowCriticalPopup(string message)
        {
            MessageBox.Show(this, message, "Critical Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private sealed class UiLogUtility : ILogUtility
        {
            public event ForceWritesEventHandler ForceWritesEvent;
            public event BufferSizeChangedEventHandler BufferSizeChangedEvent;
            public event MainDirectoryChangedEventHandler MainDirectoryChangedEvent;
            public event LogListChangedEventHandler LogListChangedEvent;

            private readonly Hashtable _activeLogList = new Hashtable();

            public bool IsEnableDebugLog => true;
            public string MainLogDirectory { get; set; } = string.Empty;
            public int BufferSizeInKB { get; set; }
            public int AutoFlushTimerMinutes { get; set; }
            public int DaysForPerservingLog => 0;
            public Hashtable ActiveLogList => _activeLogList;

            public bool WriteLog(string szKey, string szLogMessage)
            {
                Debug.WriteLine($"[{szKey}] {szLogMessage}");
                return true;
            }

            public bool WriteLog(string szKey, LogHeadType enLogType, string szLogMessage)
            {
                Debug.WriteLine($"[{szKey}] [{enLogType}] {szLogMessage}");
                return true;
            }

            public bool WriteLog(string szKey, LogHeadType enLogType, string szLogMessage, string szRemark)
            {
                Debug.WriteLine($"[{szKey}] [{enLogType}] {szLogMessage} | {szRemark}");
                return true;
            }

            public bool WriteLog(string szKey, LogHeadType enLogType, LogCateType enCateType, string szLogMessage, string szRemark = null)
            {
                Debug.WriteLine($"[{szKey}] [{enLogType}] [{enCateType}] {szLogMessage} | {szRemark}");
                return true;
            }

            public bool WriteLogWithSecured(string szLogKey, LogHeadType enLogType, string szLogMessage, string[] SecuredSections, string szRemark = null)
            {
                Debug.WriteLine($"[{szLogKey}] [{enLogType}] {szLogMessage} | {szRemark}");
                return true;
            }

            public bool WriteLogWithSecured(string szLogKey, LogHeadType enLogType, LogCateType enCateType, string szLogMessage, string[] SecuredSections, string szRemark = null)
            {
                Debug.WriteLine($"[{szLogKey}] [{enLogType}] [{enCateType}] {szLogMessage} | {szRemark}");
                return true;
            }
        }
    }
}
