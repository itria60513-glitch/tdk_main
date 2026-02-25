namespace AdvantechDIO.ManualTestGui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblDeviceId;
        private System.Windows.Forms.TextBox txtDeviceId;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Label lblConnectionState;
        private System.Windows.Forms.Label lblDeviceNameCaption;
        private System.Windows.Forms.Label lblDeviceName;
        private System.Windows.Forms.GroupBox grpDi;
        private System.Windows.Forms.Label lblDiPort;
        private System.Windows.Forms.TextBox txtDiPort;
        private System.Windows.Forms.Button btnGetInput;
        private System.Windows.Forms.Label lblDiPortValueCaption;
        private System.Windows.Forms.Label lblDiPortValue;
        private System.Windows.Forms.Label lblDiBit;
        private System.Windows.Forms.TextBox txtDiBit;
        private System.Windows.Forms.Button btnGetInputBit;
        private System.Windows.Forms.Label lblDiBitValueCaption;
        private System.Windows.Forms.Label lblDiBitValue;
        private System.Windows.Forms.Button btnSnapStart;
        private System.Windows.Forms.Button btnSnapStop;
        private System.Windows.Forms.GroupBox grpDo;
        private System.Windows.Forms.Label lblDoPort;
        private System.Windows.Forms.TextBox txtDoPort;
        private System.Windows.Forms.Label lblDoBit;
        private System.Windows.Forms.TextBox txtDoBit;
        private System.Windows.Forms.Label lblDoValue;
        private System.Windows.Forms.TextBox txtDoValue;
        private System.Windows.Forms.Button btnSetOutput;
        private System.Windows.Forms.Button btnSetOutputBit;
        private System.Windows.Forms.Button btnGetOutput;
        private System.Windows.Forms.Button btnGetOutputBit;
        private System.Windows.Forms.Label lblDoPortValueCaption;
        private System.Windows.Forms.Label lblDoPortValue;
        private System.Windows.Forms.Label lblDoBitValueCaption;
        private System.Windows.Forms.Label lblDoBitValue;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListBox lstStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblDeviceId = new System.Windows.Forms.Label();
            this.txtDeviceId = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.lblConnectionState = new System.Windows.Forms.Label();
            this.lblDeviceNameCaption = new System.Windows.Forms.Label();
            this.lblDeviceName = new System.Windows.Forms.Label();
            this.grpDi = new System.Windows.Forms.GroupBox();
            this.btnSnapStop = new System.Windows.Forms.Button();
            this.btnSnapStart = new System.Windows.Forms.Button();
            this.lblDiBitValue = new System.Windows.Forms.Label();
            this.lblDiBitValueCaption = new System.Windows.Forms.Label();
            this.btnGetInputBit = new System.Windows.Forms.Button();
            this.txtDiBit = new System.Windows.Forms.TextBox();
            this.lblDiBit = new System.Windows.Forms.Label();
            this.lblDiPortValue = new System.Windows.Forms.Label();
            this.lblDiPortValueCaption = new System.Windows.Forms.Label();
            this.btnGetInput = new System.Windows.Forms.Button();
            this.txtDiPort = new System.Windows.Forms.TextBox();
            this.lblDiPort = new System.Windows.Forms.Label();
            this.grpDo = new System.Windows.Forms.GroupBox();
            this.lblDoBitValue = new System.Windows.Forms.Label();
            this.lblDoBitValueCaption = new System.Windows.Forms.Label();
            this.lblDoPortValue = new System.Windows.Forms.Label();
            this.lblDoPortValueCaption = new System.Windows.Forms.Label();
            this.btnGetOutputBit = new System.Windows.Forms.Button();
            this.btnGetOutput = new System.Windows.Forms.Button();
            this.btnSetOutputBit = new System.Windows.Forms.Button();
            this.btnSetOutput = new System.Windows.Forms.Button();
            this.txtDoValue = new System.Windows.Forms.TextBox();
            this.lblDoValue = new System.Windows.Forms.Label();
            this.txtDoBit = new System.Windows.Forms.TextBox();
            this.lblDoBit = new System.Windows.Forms.Label();
            this.txtDoPort = new System.Windows.Forms.TextBox();
            this.lblDoPort = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lstStatus = new System.Windows.Forms.ListBox();
            this.grpDi.SuspendLayout();
            this.grpDo.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDeviceId
            // 
            this.lblDeviceId.AutoSize = true;
            this.lblDeviceId.Location = new System.Drawing.Point(12, 15);
            this.lblDeviceId.Name = "lblDeviceId";
            this.lblDeviceId.Size = new System.Drawing.Size(57, 13);
            this.lblDeviceId.TabIndex = 0;
            this.lblDeviceId.Text = "Device ID";
            // 
            // txtDeviceId
            // 
            this.txtDeviceId.Location = new System.Drawing.Point(75, 12);
            this.txtDeviceId.Name = "txtDeviceId";
            this.txtDeviceId.Size = new System.Drawing.Size(80, 20);
            this.txtDeviceId.TabIndex = 1;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(170, 10);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(90, 23);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Location = new System.Drawing.Point(266, 10);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(90, 23);
            this.btnDisconnect.TabIndex = 3;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.BtnDisconnect_Click);
            // 
            // lblConnectionState
            // 
            this.lblConnectionState.AutoSize = true;
            this.lblConnectionState.Location = new System.Drawing.Point(372, 15);
            this.lblConnectionState.Name = "lblConnectionState";
            this.lblConnectionState.Size = new System.Drawing.Size(116, 13);
            this.lblConnectionState.TabIndex = 4;
            this.lblConnectionState.Text = "State: Disconnected";
            // 
            // lblDeviceNameCaption
            // 
            this.lblDeviceNameCaption.AutoSize = true;
            this.lblDeviceNameCaption.Location = new System.Drawing.Point(12, 43);
            this.lblDeviceNameCaption.Name = "lblDeviceNameCaption";
            this.lblDeviceNameCaption.Size = new System.Drawing.Size(75, 13);
            this.lblDeviceNameCaption.TabIndex = 5;
            this.lblDeviceNameCaption.Text = "Device Name";
            // 
            // lblDeviceName
            // 
            this.lblDeviceName.AutoSize = true;
            this.lblDeviceName.Location = new System.Drawing.Point(93, 43);
            this.lblDeviceName.Name = "lblDeviceName";
            this.lblDeviceName.Size = new System.Drawing.Size(16, 13);
            this.lblDeviceName.TabIndex = 6;
            this.lblDeviceName.Text = "--";
            // 
            // grpDi
            // 
            this.grpDi.Controls.Add(this.btnSnapStop);
            this.grpDi.Controls.Add(this.btnSnapStart);
            this.grpDi.Controls.Add(this.lblDiBitValue);
            this.grpDi.Controls.Add(this.lblDiBitValueCaption);
            this.grpDi.Controls.Add(this.btnGetInputBit);
            this.grpDi.Controls.Add(this.txtDiBit);
            this.grpDi.Controls.Add(this.lblDiBit);
            this.grpDi.Controls.Add(this.lblDiPortValue);
            this.grpDi.Controls.Add(this.lblDiPortValueCaption);
            this.grpDi.Controls.Add(this.btnGetInput);
            this.grpDi.Controls.Add(this.txtDiPort);
            this.grpDi.Controls.Add(this.lblDiPort);
            this.grpDi.Location = new System.Drawing.Point(15, 68);
            this.grpDi.Name = "grpDi";
            this.grpDi.Size = new System.Drawing.Size(370, 160);
            this.grpDi.TabIndex = 7;
            this.grpDi.TabStop = false;
            this.grpDi.Text = "Digital Input";
            // 
            // btnSnapStop
            // 
            this.btnSnapStop.Location = new System.Drawing.Point(270, 122);
            this.btnSnapStop.Name = "btnSnapStop";
            this.btnSnapStop.Size = new System.Drawing.Size(90, 23);
            this.btnSnapStop.TabIndex = 11;
            this.btnSnapStop.Text = "Snap Stop";
            this.btnSnapStop.UseVisualStyleBackColor = true;
            this.btnSnapStop.Click += new System.EventHandler(this.BtnSnapStop_Click);
            // 
            // btnSnapStart
            // 
            this.btnSnapStart.Location = new System.Drawing.Point(174, 122);
            this.btnSnapStart.Name = "btnSnapStart";
            this.btnSnapStart.Size = new System.Drawing.Size(90, 23);
            this.btnSnapStart.TabIndex = 10;
            this.btnSnapStart.Text = "Snap Start";
            this.btnSnapStart.UseVisualStyleBackColor = true;
            this.btnSnapStart.Click += new System.EventHandler(this.BtnSnapStart_Click);
            // 
            // lblDiBitValue
            // 
            this.lblDiBitValue.AutoSize = true;
            this.lblDiBitValue.Location = new System.Drawing.Point(108, 91);
            this.lblDiBitValue.Name = "lblDiBitValue";
            this.lblDiBitValue.Size = new System.Drawing.Size(16, 13);
            this.lblDiBitValue.TabIndex = 9;
            this.lblDiBitValue.Text = "--";
            // 
            // lblDiBitValueCaption
            // 
            this.lblDiBitValueCaption.AutoSize = true;
            this.lblDiBitValueCaption.Location = new System.Drawing.Point(20, 91);
            this.lblDiBitValueCaption.Name = "lblDiBitValueCaption";
            this.lblDiBitValueCaption.Size = new System.Drawing.Size(66, 13);
            this.lblDiBitValueCaption.TabIndex = 8;
            this.lblDiBitValueCaption.Text = "DI Bit Value";
            // 
            // btnGetInputBit
            // 
            this.btnGetInputBit.Location = new System.Drawing.Point(270, 53);
            this.btnGetInputBit.Name = "btnGetInputBit";
            this.btnGetInputBit.Size = new System.Drawing.Size(90, 23);
            this.btnGetInputBit.TabIndex = 7;
            this.btnGetInputBit.Text = "Get Input Bit";
            this.btnGetInputBit.UseVisualStyleBackColor = true;
            this.btnGetInputBit.Click += new System.EventHandler(this.BtnGetInputBit_Click);
            // 
            // txtDiBit
            // 
            this.txtDiBit.Location = new System.Drawing.Point(197, 55);
            this.txtDiBit.Name = "txtDiBit";
            this.txtDiBit.Size = new System.Drawing.Size(50, 20);
            this.txtDiBit.TabIndex = 6;
            // 
            // lblDiBit
            // 
            this.lblDiBit.AutoSize = true;
            this.lblDiBit.Location = new System.Drawing.Point(171, 58);
            this.lblDiBit.Name = "lblDiBit";
            this.lblDiBit.Size = new System.Drawing.Size(20, 13);
            this.lblDiBit.TabIndex = 5;
            this.lblDiBit.Text = "Bit";
            // 
            // lblDiPortValue
            // 
            this.lblDiPortValue.AutoSize = true;
            this.lblDiPortValue.Location = new System.Drawing.Point(108, 32);
            this.lblDiPortValue.Name = "lblDiPortValue";
            this.lblDiPortValue.Size = new System.Drawing.Size(16, 13);
            this.lblDiPortValue.TabIndex = 4;
            this.lblDiPortValue.Text = "--";
            // 
            // lblDiPortValueCaption
            // 
            this.lblDiPortValueCaption.AutoSize = true;
            this.lblDiPortValueCaption.Location = new System.Drawing.Point(20, 32);
            this.lblDiPortValueCaption.Name = "lblDiPortValueCaption";
            this.lblDiPortValueCaption.Size = new System.Drawing.Size(74, 13);
            this.lblDiPortValueCaption.TabIndex = 3;
            this.lblDiPortValueCaption.Text = "DI Port Value";
            // 
            // btnGetInput
            // 
            this.btnGetInput.Location = new System.Drawing.Point(270, 23);
            this.btnGetInput.Name = "btnGetInput";
            this.btnGetInput.Size = new System.Drawing.Size(90, 23);
            this.btnGetInput.TabIndex = 2;
            this.btnGetInput.Text = "Get Input";
            this.btnGetInput.UseVisualStyleBackColor = true;
            this.btnGetInput.Click += new System.EventHandler(this.BtnGetInput_Click);
            // 
            // txtDiPort
            // 
            this.txtDiPort.Location = new System.Drawing.Point(111, 55);
            this.txtDiPort.Name = "txtDiPort";
            this.txtDiPort.Size = new System.Drawing.Size(50, 20);
            this.txtDiPort.TabIndex = 1;
            // 
            // lblDiPort
            // 
            this.lblDiPort.AutoSize = true;
            this.lblDiPort.Location = new System.Drawing.Point(20, 58);
            this.lblDiPort.Name = "lblDiPort";
            this.lblDiPort.Size = new System.Drawing.Size(74, 13);
            this.lblDiPort.TabIndex = 0;
            this.lblDiPort.Text = "DI Port Index";
            // 
            // grpDo
            // 
            this.grpDo.Controls.Add(this.lblDoBitValue);
            this.grpDo.Controls.Add(this.lblDoBitValueCaption);
            this.grpDo.Controls.Add(this.lblDoPortValue);
            this.grpDo.Controls.Add(this.lblDoPortValueCaption);
            this.grpDo.Controls.Add(this.btnGetOutputBit);
            this.grpDo.Controls.Add(this.btnGetOutput);
            this.grpDo.Controls.Add(this.btnSetOutputBit);
            this.grpDo.Controls.Add(this.btnSetOutput);
            this.grpDo.Controls.Add(this.txtDoValue);
            this.grpDo.Controls.Add(this.lblDoValue);
            this.grpDo.Controls.Add(this.txtDoBit);
            this.grpDo.Controls.Add(this.lblDoBit);
            this.grpDo.Controls.Add(this.txtDoPort);
            this.grpDo.Controls.Add(this.lblDoPort);
            this.grpDo.Location = new System.Drawing.Point(401, 68);
            this.grpDo.Name = "grpDo";
            this.grpDo.Size = new System.Drawing.Size(390, 160);
            this.grpDo.TabIndex = 8;
            this.grpDo.TabStop = false;
            this.grpDo.Text = "Digital Output";
            // 
            // lblDoBitValue
            // 
            this.lblDoBitValue.AutoSize = true;
            this.lblDoBitValue.Location = new System.Drawing.Point(114, 131);
            this.lblDoBitValue.Name = "lblDoBitValue";
            this.lblDoBitValue.Size = new System.Drawing.Size(16, 13);
            this.lblDoBitValue.TabIndex = 13;
            this.lblDoBitValue.Text = "--";
            // 
            // lblDoBitValueCaption
            // 
            this.lblDoBitValueCaption.AutoSize = true;
            this.lblDoBitValueCaption.Location = new System.Drawing.Point(20, 131);
            this.lblDoBitValueCaption.Name = "lblDoBitValueCaption";
            this.lblDoBitValueCaption.Size = new System.Drawing.Size(74, 13);
            this.lblDoBitValueCaption.TabIndex = 12;
            this.lblDoBitValueCaption.Text = "DO Bit Value";
            // 
            // lblDoPortValue
            // 
            this.lblDoPortValue.AutoSize = true;
            this.lblDoPortValue.Location = new System.Drawing.Point(114, 104);
            this.lblDoPortValue.Name = "lblDoPortValue";
            this.lblDoPortValue.Size = new System.Drawing.Size(16, 13);
            this.lblDoPortValue.TabIndex = 11;
            this.lblDoPortValue.Text = "--";
            // 
            // lblDoPortValueCaption
            // 
            this.lblDoPortValueCaption.AutoSize = true;
            this.lblDoPortValueCaption.Location = new System.Drawing.Point(20, 104);
            this.lblDoPortValueCaption.Name = "lblDoPortValueCaption";
            this.lblDoPortValueCaption.Size = new System.Drawing.Size(82, 13);
            this.lblDoPortValueCaption.TabIndex = 10;
            this.lblDoPortValueCaption.Text = "DO Port Value";
            // 
            // btnGetOutputBit
            // 
            this.btnGetOutputBit.Location = new System.Drawing.Point(276, 123);
            this.btnGetOutputBit.Name = "btnGetOutputBit";
            this.btnGetOutputBit.Size = new System.Drawing.Size(100, 23);
            this.btnGetOutputBit.TabIndex = 9;
            this.btnGetOutputBit.Text = "Get Output Bit";
            this.btnGetOutputBit.UseVisualStyleBackColor = true;
            this.btnGetOutputBit.Click += new System.EventHandler(this.BtnGetOutputBit_Click);
            // 
            // btnGetOutput
            // 
            this.btnGetOutput.Location = new System.Drawing.Point(170, 123);
            this.btnGetOutput.Name = "btnGetOutput";
            this.btnGetOutput.Size = new System.Drawing.Size(100, 23);
            this.btnGetOutput.TabIndex = 8;
            this.btnGetOutput.Text = "Get Output";
            this.btnGetOutput.UseVisualStyleBackColor = true;
            this.btnGetOutput.Click += new System.EventHandler(this.BtnGetOutput_Click);
            // 
            // btnSetOutputBit
            // 
            this.btnSetOutputBit.Location = new System.Drawing.Point(276, 78);
            this.btnSetOutputBit.Name = "btnSetOutputBit";
            this.btnSetOutputBit.Size = new System.Drawing.Size(100, 23);
            this.btnSetOutputBit.TabIndex = 7;
            this.btnSetOutputBit.Text = "Set Output Bit";
            this.btnSetOutputBit.UseVisualStyleBackColor = true;
            this.btnSetOutputBit.Click += new System.EventHandler(this.BtnSetOutputBit_Click);
            // 
            // btnSetOutput
            // 
            this.btnSetOutput.Location = new System.Drawing.Point(170, 78);
            this.btnSetOutput.Name = "btnSetOutput";
            this.btnSetOutput.Size = new System.Drawing.Size(100, 23);
            this.btnSetOutput.TabIndex = 6;
            this.btnSetOutput.Text = "Set Output";
            this.btnSetOutput.UseVisualStyleBackColor = true;
            this.btnSetOutput.Click += new System.EventHandler(this.BtnSetOutput_Click);
            // 
            // txtDoValue
            // 
            this.txtDoValue.Location = new System.Drawing.Point(74, 76);
            this.txtDoValue.Name = "txtDoValue";
            this.txtDoValue.Size = new System.Drawing.Size(70, 20);
            this.txtDoValue.TabIndex = 5;
            // 
            // lblDoValue
            // 
            this.lblDoValue.AutoSize = true;
            this.lblDoValue.Location = new System.Drawing.Point(20, 79);
            this.lblDoValue.Name = "lblDoValue";
            this.lblDoValue.Size = new System.Drawing.Size(34, 13);
            this.lblDoValue.TabIndex = 4;
            this.lblDoValue.Text = "Value";
            // 
            // txtDoBit
            // 
            this.txtDoBit.Location = new System.Drawing.Point(286, 32);
            this.txtDoBit.Name = "txtDoBit";
            this.txtDoBit.Size = new System.Drawing.Size(70, 20);
            this.txtDoBit.TabIndex = 3;
            // 
            // lblDoBit
            // 
            this.lblDoBit.AutoSize = true;
            this.lblDoBit.Location = new System.Drawing.Point(247, 35);
            this.lblDoBit.Name = "lblDoBit";
            this.lblDoBit.Size = new System.Drawing.Size(20, 13);
            this.lblDoBit.TabIndex = 2;
            this.lblDoBit.Text = "Bit";
            // 
            // txtDoPort
            // 
            this.txtDoPort.Location = new System.Drawing.Point(74, 32);
            this.txtDoPort.Name = "txtDoPort";
            this.txtDoPort.Size = new System.Drawing.Size(70, 20);
            this.txtDoPort.TabIndex = 1;
            // 
            // lblDoPort
            // 
            this.lblDoPort.AutoSize = true;
            this.lblDoPort.Location = new System.Drawing.Point(20, 35);
            this.lblDoPort.Name = "lblDoPort";
            this.lblDoPort.Size = new System.Drawing.Size(82, 13);
            this.lblDoPort.TabIndex = 0;
            this.lblDoPort.Text = "DO Port Index";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 244);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(63, 13);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "Status / Log";
            // 
            // lstStatus
            // 
            this.lstStatus.FormattingEnabled = true;
            this.lstStatus.HorizontalScrollbar = true;
            this.lstStatus.Location = new System.Drawing.Point(15, 260);
            this.lstStatus.Name = "lstStatus";
            this.lstStatus.Size = new System.Drawing.Size(776, 173);
            this.lstStatus.TabIndex = 10;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 451);
            this.Controls.Add(this.lstStatus);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grpDo);
            this.Controls.Add(this.grpDi);
            this.Controls.Add(this.lblDeviceName);
            this.Controls.Add(this.lblDeviceNameCaption);
            this.Controls.Add(this.lblConnectionState);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.txtDeviceId);
            this.Controls.Add(this.lblDeviceId);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AdvantechDIO Simple Test GUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.grpDi.ResumeLayout(false);
            this.grpDi.PerformLayout();
            this.grpDo.ResumeLayout(false);
            this.grpDo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
