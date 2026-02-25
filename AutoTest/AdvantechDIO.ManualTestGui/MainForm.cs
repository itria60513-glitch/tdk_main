using AdvantechDIO.Config;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using TDKLogUtility.Module;

namespace AdvantechDIO.ManualTestGui
{
    public partial class MainForm : Form
    {
        private readonly ILogUtility _logger;

        private AdvantechDIO.Module.AdvantechDIO _dioDev0;
        private AdvantechDIO.Module.AdvantechDIO _dioDev1;
        private readonly AdvantechDIOConfig _configDev0;
        private readonly AdvantechDIOConfig _configDev1;
        private Label[] _diLedsDev0;
        private Label[] _diLedsDev1;
        private Label[] _doLedsDev0;
        private Label[] _doLedsDev1;
        private DeviceTestControls _testControlsDev0;
        private DeviceTestControls _testControlsDev1;
        private Timer _statusRefreshTimer;
        private bool _isRefreshingStatusPanel;

        public MainForm()
        {
            InitializeComponent();

            _logger = new UiLogUtility();

            _configDev0 = new AdvantechDIOConfig
            {
                DeviceID = 0,
                DIPortCount = 1,
                DIPinCountPerPort = 8,
                DOPortCount = 1,
                DOPinCountPerPort = 8
            };
            _configDev1 = new AdvantechDIOConfig
            {
                DeviceID = 1,
                DIPortCount = 1,
                DIPinCountPerPort = 8,
                DOPortCount = 1,
                DOPinCountPerPort = 8
            };

            InitializeDevicePanel(grpDevice0, 0, out _diLedsDev0, out _doLedsDev0);
            InitializeDevicePanel(grpDevice1, 1, out _diLedsDev1, out _doLedsDev1);

            SetDeviceOfflineStyle(_diLedsDev0, _doLedsDev0, lblDev0State);
            SetDeviceOfflineStyle(_diLedsDev1, _doLedsDev1, lblDev1State);

            _statusRefreshTimer = new Timer { Interval = 500 };
            _statusRefreshTimer.Tick += StatusRefreshTimer_Tick;
            _statusRefreshTimer.Start();

            WriteStatus("Application started.");
        }

        private void InitializeDevicePanel(GroupBox grp, int deviceId, out Label[] diLeds, out Label[] doLeds)
        {
            const int bitCount = 8;
            const int ledSize = 28;
            const int cellWidth = 42;
            const int startX = 48;
            const int testBaseY = 62;
            const int diBitLabelY = 190;
            const int diLedY = 206;
            const int doBitLabelY = 240;
            const int doLedY = 256;

            DeviceTestControls testControls = CreateDeviceTestControls(grp, deviceId, testBaseY);
            if (deviceId == 0)
            {
                _testControlsDev0 = testControls;
            }
            else
            {
                _testControlsDev1 = testControls;
            }

            grp.Controls.Add(new Label
            {
                Text = "DI",
                AutoSize = true,
                Location = new Point(10, diLedY + 8),
                Font = new Font(Font, FontStyle.Bold)
            });

            grp.Controls.Add(new Label
            {
                Text = "DO",
                AutoSize = true,
                Location = new Point(10, doLedY + 8),
                Font = new Font(Font, FontStyle.Bold)
            });

            diLeds = new Label[bitCount];
            doLeds = new Label[bitCount];

            for (int bit = 0; bit < bitCount; bit++)
            {
                int x = startX + bit * cellWidth;

                // DI bit number label
                grp.Controls.Add(new Label
                {
                    Text = bit.ToString(CultureInfo.InvariantCulture),
                    AutoSize = true,
                    Location = new Point(x + 9, diBitLabelY)
                });

                // DI LED (read-only indicator)
                Label diLed = new Label
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(x, diLedY),
                    Size = new Size(ledSize, ledSize),
                    BackColor = Color.DarkGray,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                grp.Controls.Add(diLed);
                diLeds[bit] = diLed;

                // DO bit number label
                grp.Controls.Add(new Label
                {
                    Text = bit.ToString(CultureInfo.InvariantCulture),
                    AutoSize = true,
                    Location = new Point(x + 9, doBitLabelY)
                });

                // DO LED (clickable to toggle)
                Label doLed = new Label
                {
                    BorderStyle = BorderStyle.Fixed3D,
                    Location = new Point(x, doLedY),
                    Size = new Size(ledSize, ledSize),
                    BackColor = Color.DarkGray,
                    Cursor = Cursors.Hand,
                    Tag = (deviceId << 8) | bit,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                doLed.Click += DoLed_Click;
                grp.Controls.Add(doLed);
                doLeds[bit] = doLed;
            }
        }

        private DeviceTestControls CreateDeviceTestControls(GroupBox grp, int deviceId, int baseY)
        {
            DeviceTestControls controls = new DeviceTestControls();

            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(10, baseY + 2), Text = "DI Port" });
            controls.DiPortTextBox = new TextBox { Location = new Point(58, baseY), Size = new Size(36, 20), Text = "0" };
            grp.Controls.Add(controls.DiPortTextBox);

            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(103, baseY + 2), Text = "Bit" });
            controls.DiBitTextBox = new TextBox { Location = new Point(126, baseY), Size = new Size(36, 20), Text = "0" };
            grp.Controls.Add(controls.DiBitTextBox);

            controls.GetDiPortButton = new Button { Location = new Point(172, baseY - 1), Size = new Size(88, 23), Text = "Get DI Port" };
            controls.GetDiPortButton.Click += (s, e) => ExecuteGetDiPort(deviceId);
            grp.Controls.Add(controls.GetDiPortButton);

            controls.GetDiBitButton = new Button { Location = new Point(266, baseY - 1), Size = new Size(88, 23), Text = "Get DI Bit" };
            controls.GetDiBitButton.Click += (s, e) => ExecuteGetDiBit(deviceId);
            grp.Controls.Add(controls.GetDiBitButton);

            controls.DiResultLabel = new Label { AutoSize = true, Location = new Point(10, baseY + 27), Text = "DI Value: --" };
            grp.Controls.Add(controls.DiResultLabel);

            int doBaseY = baseY + 52;
            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(10, doBaseY + 2), Text = "DO Port" });
            controls.DoPortTextBox = new TextBox { Location = new Point(58, doBaseY), Size = new Size(36, 20), Text = "0" };
            grp.Controls.Add(controls.DoPortTextBox);

            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(103, doBaseY + 2), Text = "Bit" });
            controls.DoBitTextBox = new TextBox { Location = new Point(126, doBaseY), Size = new Size(36, 20), Text = "0" };
            grp.Controls.Add(controls.DoBitTextBox);

            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(171, doBaseY + 2), Text = "Value" });
            controls.DoValueTextBox = new TextBox { Location = new Point(209, doBaseY), Size = new Size(36, 20), Text = "0" };
            grp.Controls.Add(controls.DoValueTextBox);

            controls.GetDoPortButton = new Button { Location = new Point(251, doBaseY - 1), Size = new Size(103, 23), Text = "Get DO Port" };
            controls.GetDoPortButton.Click += (s, e) => ExecuteGetDoPort(deviceId);
            grp.Controls.Add(controls.GetDoPortButton);

            controls.GetDoBitButton = new Button { Location = new Point(10, doBaseY + 24), Size = new Size(88, 23), Text = "Get DO Bit" };
            controls.GetDoBitButton.Click += (s, e) => ExecuteGetDoBit(deviceId);
            grp.Controls.Add(controls.GetDoBitButton);

            controls.SetDoPortButton = new Button { Location = new Point(104, doBaseY + 24), Size = new Size(88, 23), Text = "Set DO Port" };
            controls.SetDoPortButton.Click += (s, e) => ExecuteSetDoPort(deviceId);
            grp.Controls.Add(controls.SetDoPortButton);

            controls.SetDoBitButton = new Button { Location = new Point(198, doBaseY + 24), Size = new Size(88, 23), Text = "Set DO Bit" };
            controls.SetDoBitButton.Click += (s, e) => ExecuteSetDoBit(deviceId);
            grp.Controls.Add(controls.SetDoBitButton);

            controls.DoResultLabel = new Label { AutoSize = true, Location = new Point(10, doBaseY + 52), Text = "DO Value: --" };
            grp.Controls.Add(controls.DoResultLabel);

            return controls;
        }

        private void ExecuteGetDiPort(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DiPortTextBox.Text, $"Dev{deviceId} DI Port", out int portIndex))
            {
                return;
            }

            byte value;
            int result = device.GetInput(portIndex, out value);
            controls.DiResultLabel.Text = result == 0 ? $"DI Value: {value}" : "DI Value: --";
            WriteStatus($"Dev{deviceId} GetInput(port={portIndex}): Code={result}" + (result == 0 ? $", Value={value}" : string.Empty));
        }

        private void ExecuteGetDiBit(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DiPortTextBox.Text, $"Dev{deviceId} DI Port", out int portIndex) ||
                !TryParseNonNegative(controls.DiBitTextBox.Text, $"Dev{deviceId} DI Bit", out int bitIndex))
            {
                return;
            }

            byte value;
            int result = device.GetInputBit(portIndex, bitIndex, out value);
            controls.DiResultLabel.Text = result == 0 ? $"DI Value: {value}" : "DI Value: --";
            WriteStatus($"Dev{deviceId} GetInputBit(port={portIndex}, bit={bitIndex}): Code={result}" + (result == 0 ? $", Value={value}" : string.Empty));
        }

        private void ExecuteGetDoPort(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DoPortTextBox.Text, $"Dev{deviceId} DO Port", out int portIndex))
            {
                return;
            }

            byte value;
            int result = device.GetOutput(portIndex, out value);
            controls.DoResultLabel.Text = result == 0 ? $"DO Value: {value}" : "DO Value: --";
            WriteStatus($"Dev{deviceId} GetOutput(port={portIndex}): Code={result}" + (result == 0 ? $", Value={value}" : string.Empty));
        }

        private void ExecuteGetDoBit(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DoPortTextBox.Text, $"Dev{deviceId} DO Port", out int portIndex) ||
                !TryParseNonNegative(controls.DoBitTextBox.Text, $"Dev{deviceId} DO Bit", out int bitIndex))
            {
                return;
            }

            byte value;
            int result = device.GetOutputBit(portIndex, bitIndex, out value);
            controls.DoResultLabel.Text = result == 0 ? $"DO Value: {value}" : "DO Value: --";
            WriteStatus($"Dev{deviceId} GetOutputBit(port={portIndex}, bit={bitIndex}): Code={result}" + (result == 0 ? $", Value={value}" : string.Empty));
        }

        private void ExecuteSetDoPort(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DoPortTextBox.Text, $"Dev{deviceId} DO Port", out int portIndex))
            {
                return;
            }

            if (!byte.TryParse(controls.DoValueTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte value))
            {
                WriteStatus($"Dev{deviceId} validation failed: DO Value must be 0..255.");
                return;
            }

            int result = device.SetOutput(portIndex, value);
            WriteStatus($"Dev{deviceId} SetOutput(port={portIndex}, value={value}): Code={result}");
            if (result == 0)
            {
                controls.DoResultLabel.Text = $"DO Value: {value}";
                RefreshDioPanel();
            }
        }

        private void ExecuteSetDoBit(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DoPortTextBox.Text, $"Dev{deviceId} DO Port", out int portIndex) ||
                !TryParseNonNegative(controls.DoBitTextBox.Text, $"Dev{deviceId} DO Bit", out int bitIndex))
            {
                return;
            }

            if (!byte.TryParse(controls.DoValueTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte value) || (value != 0 && value != 1))
            {
                WriteStatus($"Dev{deviceId} validation failed: DO Bit Value must be 0 or 1.");
                return;
            }

            int result = device.SetOutputBit(portIndex, bitIndex, value);
            WriteStatus($"Dev{deviceId} SetOutputBit(port={portIndex}, bit={bitIndex}, value={value}): Code={result}");
            if (result == 0)
            {
                controls.DoResultLabel.Text = $"DO Value: {value}";
                RefreshDioPanel();
            }
        }

        private DeviceTestControls GetDeviceControls(int deviceId)
        {
            return deviceId == 0 ? _testControlsDev0 : _testControlsDev1;
        }

        private AdvantechDIO.Module.AdvantechDIO GetConnectedDevice(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = deviceId == 0 ? _dioDev0 : _dioDev1;
            if (device == null || !device.IsConnected)
            {
                WriteStatus($"Dev{deviceId}: Not connected.");
                return null;
            }

            return device;
        }

        private bool TryParseNonNegative(string raw, string fieldName, out int value)
        {
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) || value < 0)
            {
                WriteStatus($"Validation failed: {fieldName} must be non-negative integer.");
                return false;
            }

            return true;
        }

        private void DoLed_Click(object sender, EventArgs e)
        {
            Label led = (Label)sender;
            int tag = (int)led.Tag;
            int deviceId = tag >> 8;
            int bit = tag & 0xFF;

            AdvantechDIO.Module.AdvantechDIO device = deviceId == 0 ? _dioDev0 : _dioDev1;
            if (device == null || !device.IsConnected)
            {
                WriteStatus($"Dev{deviceId}: Not connected.");
                return;
            }

            byte currentBitValue;
            int result = device.GetOutputBit(0, bit, out currentBitValue);
            if (result != 0)
            {
                WriteStatus($"Dev{deviceId} DO[{bit}] read failed: Code={result}");
                return;
            }

            byte newValue = currentBitValue == 0 ? (byte)1 : (byte)0;
            result = device.SetOutputBit(0, bit, newValue);
            if (result == 0)
            {
                WriteStatus($"Dev{deviceId} DO[{bit}] -> {newValue}");
                RefreshDioPanel();
            }
            else
            {
                WriteStatus($"Dev{deviceId} DO[{bit}] set failed: Code={result}");
            }
        }

        private void BtnConnectDev0_Click(object sender, EventArgs e)
        {
            EnsureDeviceConnection(0);
            RefreshDioPanel();
        }

        private void BtnDisconnectDev0_Click(object sender, EventArgs e)
        {
            DisconnectDevice(0);
            RefreshDioPanel();
        }

        private void BtnConnectDev1_Click(object sender, EventArgs e)
        {
            EnsureDeviceConnection(1);
            RefreshDioPanel();
        }

        private void BtnDisconnectDev1_Click(object sender, EventArgs e)
        {
            DisconnectDevice(1);
            RefreshDioPanel();
        }

        private void StatusRefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshDioPanel();
        }

        private void RefreshDioPanel()
        {
            if (_isRefreshingStatusPanel)
            {
                return;
            }

            _isRefreshingStatusPanel = true;
            try
            {
                UpdateDeviceLeds(_dioDev0, _diLedsDev0, _doLedsDev0, lblDev0State);
                UpdateDeviceLeds(_dioDev1, _diLedsDev1, _doLedsDev1, lblDev1State);
            }
            finally
            {
                _isRefreshingStatusPanel = false;
            }
        }

        private void UpdateDeviceLeds(AdvantechDIO.Module.AdvantechDIO device, Label[] diLeds, Label[] doLeds, Label stateLabel)
        {
            if (device == null || !device.IsConnected)
            {
                SetDeviceOfflineStyle(diLeds, doLeds, stateLabel);
                return;
            }

            byte diValue;
            int diResult = device.GetInput(0, out diValue);
            byte doValue;
            int doResult = device.GetOutput(0, out doValue);

            if (diResult != 0 || doResult != 0)
            {
                stateLabel.Text = $"Error ({diResult}/{doResult})";
                SetDeviceOfflineStyle(diLeds, doLeds, null);
                return;
            }

            stateLabel.Text = "Connected";
            stateLabel.ForeColor = Color.Green;
            for (int bit = 0; bit < 8; bit++)
            {
                bool diOn = (diValue & (1 << bit)) != 0;
                bool doOn = (doValue & (1 << bit)) != 0;
                diLeds[bit].BackColor = diOn ? Color.Lime : Color.Silver;
                doLeds[bit].BackColor = doOn ? Color.Tomato : Color.Silver;
            }
        }

        private void SetDeviceOfflineStyle(Label[] diLeds, Label[] doLeds, Label stateLabel)
        {
            if (stateLabel != null)
            {
                stateLabel.Text = "Disconnected";
                stateLabel.ForeColor = Color.Gray;
            }

            for (int bit = 0; bit < 8; bit++)
            {
                diLeds[bit].BackColor = Color.DarkGray;
                doLeds[bit].BackColor = Color.DarkGray;
            }
        }

        private void EnsureDeviceConnection(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = deviceId == 0 ? _dioDev0 : _dioDev1;
            AdvantechDIOConfig config = deviceId == 0 ? _configDev0 : _configDev1;

            if (device != null && device.IsConnected)
            {
                return;
            }

            if (device == null)
            {
                device = new AdvantechDIO.Module.AdvantechDIO(_logger, config);
                if (deviceId == 0)
                {
                    _dioDev0 = device;
                }
                else
                {
                    _dioDev1 = device;
                }
            }

            int result = device.Connect();
            WriteStatus($"Connect Dev{deviceId}: Code={result}");
        }

        private void DisconnectDevice(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = deviceId == 0 ? _dioDev0 : _dioDev1;
            if (device == null)
            {
                return;
            }

            int result = device.Disconnect();
            WriteStatus($"Disconnect Dev{deviceId}: Code={result}");
        }

        private void DisposeDioInstance(ref AdvantechDIO.Module.AdvantechDIO device)
        {
            if (device == null)
            {
                return;
            }

            try
            {
                device.Disconnect();
            }
            catch
            {
                // Swallow cleanup exceptions to keep form shutdown safe.
            }

            device.Dispose();
            device = null;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_statusRefreshTimer != null)
            {
                _statusRefreshTimer.Stop();
                _statusRefreshTimer.Tick -= StatusRefreshTimer_Tick;
                _statusRefreshTimer.Dispose();
                _statusRefreshTimer = null;
            }

            DisposeDioInstance(ref _dioDev0);
            DisposeDioInstance(ref _dioDev1);
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

        private sealed class UiLogUtility : ILogUtility
        {
#pragma warning disable CS0067
            public event ForceWritesEventHandler ForceWritesEvent;
            public event BufferSizeChangedEventHandler BufferSizeChangedEvent;
            public event MainDirectoryChangedEventHandler MainDirectoryChangedEvent;
            public event LogListChangedEventHandler LogListChangedEvent;
#pragma warning restore CS0067

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

        private sealed class DeviceTestControls
        {
            public TextBox DiPortTextBox { get; set; }
            public TextBox DiBitTextBox { get; set; }
            public Button GetDiPortButton { get; set; }
            public Button GetDiBitButton { get; set; }
            public Label DiResultLabel { get; set; }
            public TextBox DoPortTextBox { get; set; }
            public TextBox DoBitTextBox { get; set; }
            public TextBox DoValueTextBox { get; set; }
            public Button GetDoPortButton { get; set; }
            public Button GetDoBitButton { get; set; }
            public Button SetDoPortButton { get; set; }
            public Button SetDoBitButton { get; set; }
            public Label DoResultLabel { get; set; }
        }
    }
}
