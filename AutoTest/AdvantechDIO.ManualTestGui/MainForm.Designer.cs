namespace AdvantechDIO.ManualTestGui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.GroupBox grpDevice1;
        private System.Windows.Forms.Button btnConnectDev1;
        private System.Windows.Forms.Button btnDisconnectDev1;
        private System.Windows.Forms.Label lblDev1State;
        private System.Windows.Forms.GroupBox grpDevice0;
        private System.Windows.Forms.Button btnConnectDev0;
        private System.Windows.Forms.Button btnDisconnectDev0;
        private System.Windows.Forms.Label lblDev0State;
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
            this.components = new System.ComponentModel.Container();
            this.grpDevice1 = new System.Windows.Forms.GroupBox();
            this.btnConnectDev1 = new System.Windows.Forms.Button();
            this.btnDisconnectDev1 = new System.Windows.Forms.Button();
            this.lblDev1State = new System.Windows.Forms.Label();
            this.grpDevice0 = new System.Windows.Forms.GroupBox();
            this.btnConnectDev0 = new System.Windows.Forms.Button();
            this.btnDisconnectDev0 = new System.Windows.Forms.Button();
            this.lblDev0State = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lstStatus = new System.Windows.Forms.ListBox();
            this.grpDevice1.SuspendLayout();
            this.grpDevice0.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpDevice1  (Right Panel)
            // 
            this.grpDevice1.Controls.Add(this.lblDev1State);
            this.grpDevice1.Controls.Add(this.btnDisconnectDev1);
            this.grpDevice1.Controls.Add(this.btnConnectDev1);
            this.grpDevice1.Location = new System.Drawing.Point(434, 12);
            this.grpDevice1.Name = "grpDevice1";
            this.grpDevice1.Size = new System.Drawing.Size(390, 330);
            this.grpDevice1.TabIndex = 0;
            this.grpDevice1.TabStop = false;
            this.grpDevice1.Text = "Device 1";
            // 
            // btnConnectDev1
            // 
            this.btnConnectDev1.Location = new System.Drawing.Point(10, 22);
            this.btnConnectDev1.Name = "btnConnectDev1";
            this.btnConnectDev1.Size = new System.Drawing.Size(88, 28);
            this.btnConnectDev1.TabIndex = 0;
            this.btnConnectDev1.Text = "Connect";
            this.btnConnectDev1.UseVisualStyleBackColor = true;
            this.btnConnectDev1.Click += new System.EventHandler(this.BtnConnectDev1_Click);
            // 
            // btnDisconnectDev1
            // 
            this.btnDisconnectDev1.Location = new System.Drawing.Point(106, 22);
            this.btnDisconnectDev1.Name = "btnDisconnectDev1";
            this.btnDisconnectDev1.Size = new System.Drawing.Size(88, 28);
            this.btnDisconnectDev1.TabIndex = 1;
            this.btnDisconnectDev1.Text = "Disconnect";
            this.btnDisconnectDev1.UseVisualStyleBackColor = true;
            this.btnDisconnectDev1.Click += new System.EventHandler(this.BtnDisconnectDev1_Click);
            // 
            // lblDev1State
            // 
            this.lblDev1State.AutoSize = true;
            this.lblDev1State.Location = new System.Drawing.Point(204, 28);
            this.lblDev1State.Name = "lblDev1State";
            this.lblDev1State.Size = new System.Drawing.Size(72, 13);
            this.lblDev1State.TabIndex = 2;
            this.lblDev1State.Text = "Disconnected";
            // 
            // grpDevice0  (Left Panel)
            // 
            this.grpDevice0.Controls.Add(this.lblDev0State);
            this.grpDevice0.Controls.Add(this.btnDisconnectDev0);
            this.grpDevice0.Controls.Add(this.btnConnectDev0);
            this.grpDevice0.Location = new System.Drawing.Point(12, 12);
            this.grpDevice0.Name = "grpDevice0";
            this.grpDevice0.Size = new System.Drawing.Size(410, 330);
            this.grpDevice0.TabIndex = 1;
            this.grpDevice0.TabStop = false;
            this.grpDevice0.Text = "Device 0";
            // 
            // btnConnectDev0
            // 
            this.btnConnectDev0.Location = new System.Drawing.Point(10, 22);
            this.btnConnectDev0.Name = "btnConnectDev0";
            this.btnConnectDev0.Size = new System.Drawing.Size(88, 28);
            this.btnConnectDev0.TabIndex = 0;
            this.btnConnectDev0.Text = "Connect";
            this.btnConnectDev0.UseVisualStyleBackColor = true;
            this.btnConnectDev0.Click += new System.EventHandler(this.BtnConnectDev0_Click);
            // 
            // btnDisconnectDev0
            // 
            this.btnDisconnectDev0.Location = new System.Drawing.Point(106, 22);
            this.btnDisconnectDev0.Name = "btnDisconnectDev0";
            this.btnDisconnectDev0.Size = new System.Drawing.Size(88, 28);
            this.btnDisconnectDev0.TabIndex = 1;
            this.btnDisconnectDev0.Text = "Disconnect";
            this.btnDisconnectDev0.UseVisualStyleBackColor = true;
            this.btnDisconnectDev0.Click += new System.EventHandler(this.BtnDisconnectDev0_Click);
            // 
            // lblDev0State
            // 
            this.lblDev0State.AutoSize = true;
            this.lblDev0State.Location = new System.Drawing.Point(204, 28);
            this.lblDev0State.Name = "lblDev0State";
            this.lblDev0State.Size = new System.Drawing.Size(72, 13);
            this.lblDev0State.TabIndex = 2;
            this.lblDev0State.Text = "Disconnected";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 351);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(63, 13);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Status / Log";
            // 
            // lstStatus
            // 
            this.lstStatus.FormattingEnabled = true;
            this.lstStatus.HorizontalScrollbar = true;
            this.lstStatus.Location = new System.Drawing.Point(12, 369);
            this.lstStatus.Name = "lstStatus";
            this.lstStatus.Size = new System.Drawing.Size(812, 130);
            this.lstStatus.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(836, 512);
            this.Controls.Add(this.lstStatus);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grpDevice0);
            this.Controls.Add(this.grpDevice1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AdvantechDIO Manual Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.grpDevice1.ResumeLayout(false);
            this.grpDevice1.PerformLayout();
            this.grpDevice0.ResumeLayout(false);
            this.grpDevice0.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
