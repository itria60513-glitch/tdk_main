using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EFEM.DataCenter;
using EFEM.FileUtilities;
using Communication.GUI.ViewModels;
using System.Threading;

namespace Communication.GUI
{
    public partial class RS232SettingGUI : UserControl
    {
        private readonly RS232ConfigGuiViewModel _viewModel;
        private readonly SynchronizationContext _ctx;
        AbstractFileUtilities _fu = FileUtility.GetUniqueInstance();
        private string _select = string.Empty;
        public RS232SettingGUI(string select)
        {
            InitializeComponent();
            _viewModel = new RS232ConfigGuiViewModel(_ctx);
            _select = select;
            DataBinding();

            SerialParamSetting(_select);


        }

        private void SerialParamSetting(string select)
        {
            RS232Config serialSetting = _fu.GetSerialSetting(select);
            _viewModel.BaudRate = serialSetting.Baud;
            _viewModel.DataBits = serialSetting.DataBits;
            _viewModel.Parity = serialSetting.Parity;
            _viewModel.PortNumber = serialSetting.Port;
            _viewModel.StopBits = serialSetting.StopBits;
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to save current settings?", "TDK_Controller", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                _fu.SerialPortConfigSave(_select, _viewModel.PortNumber, _viewModel.BaudRate, _viewModel.Parity, _viewModel.DataBits, _viewModel.StopBits);
                MessageBox.Show("Save Success.", "TDK_Controller", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void button_default_setting_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to set to default value?", "TDK_Controller", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                _fu.ResetToDefaultValue("RS232", _select);
                SerialParamSetting(_select);
                ComPortCombo.Refresh();
                BaudRateCombo.Refresh();
                ParityCombo.Refresh();
                DataBitCombo.Refresh();
                StopBitCombo.Refresh();
                MessageBox.Show("Reset to Default Value Success.", "TDK_Controller", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void DataBinding()
        {
            _viewModel.PortNumber = 0;
            ComPortCombo.DataBindings.Add(nameof(ComboBox.Text), _viewModel, nameof(_viewModel.PortNumber));

            _viewModel.BaudRate = 0;
            BaudRateCombo.DataBindings.Add(nameof(ComboBox.Text), _viewModel, nameof(_viewModel.BaudRate));

            _viewModel.StopBits = 0;
            StopBitCombo.DataBindings.Add(nameof(ComboBox.Text), _viewModel, nameof(_viewModel.StopBits));

            _viewModel.DataBits = 0;
            DataBitCombo.DataBindings.Add(nameof(ComboBox.Text), _viewModel, nameof(_viewModel.DataBits));

            _viewModel.Parity = ParityCombo.SelectedIndex;
            ParityCombo.DataBindings.Add(nameof(ComboBox.SelectedIndex), _viewModel, nameof(_viewModel.Parity));


        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            _fu.SerialConfigApply(_select, _viewModel.PortNumber, _viewModel.BaudRate, _viewModel.Parity, _viewModel.DataBits, _viewModel.StopBits);
            MessageBox.Show("Apply Success.", "TDK_Controller", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #region ConnectionCheck
        private void SerialButtonValidationCheck()
        {

        }

        private void ConnectionCheck(Label lbl, ComboBox cb)
        {
            if (cb.SelectedIndex >= 0)
            {
                lbl.ForeColor = Color.Black;
                lbl.Text = lbl.Name;
                btn_Apply.Enabled = true;
                button_save.Enabled = true;
            }
            else
            {
                lbl.ForeColor = Color.Red;
                lbl.Text = lbl.Name + " (Invalid)";
                btn_Apply.Enabled = false;
                button_save.Enabled = false;
            }

        }


        #endregion

    }
}
