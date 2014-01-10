namespace VSSonarExtension.VSControls.DialogOptions
{
    partial class SonarGeneralOptionsControlForm {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.groupBoxCPP = new System.Windows.Forms.GroupBox();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.buttonTestConnection = new System.Windows.Forms.Button();
            this.labelUserPassWord = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelSonarUserName = new System.Windows.Forms.Label();
            this.labelSonarHost = new System.Windows.Forms.Label();
            this.textBoxSonarHost = new System.Windows.Forms.TextBox();
            this.toolTipSonarServer = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.PluginsUserControl = new System.Windows.Forms.Button();
            this.groupBoxCPP.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCPP
            // 
            this.groupBoxCPP.Controls.Add(this.textBoxUserName);
            this.groupBoxCPP.Controls.Add(this.buttonTestConnection);
            this.groupBoxCPP.Controls.Add(this.labelUserPassWord);
            this.groupBoxCPP.Controls.Add(this.textBoxPassword);
            this.groupBoxCPP.Controls.Add(this.labelSonarUserName);
            this.groupBoxCPP.Controls.Add(this.labelSonarHost);
            this.groupBoxCPP.Controls.Add(this.textBoxSonarHost);
            this.groupBoxCPP.Location = new System.Drawing.Point(0, -2);
            this.groupBoxCPP.Name = "groupBoxCPP";
            this.groupBoxCPP.Size = new System.Drawing.Size(380, 140);
            this.groupBoxCPP.TabIndex = 1;
            this.groupBoxCPP.TabStop = false;
            this.groupBoxCPP.Text = "Server Configuration";
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(241, 49);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(129, 20);
            this.textBoxUserName.TabIndex = 7;
            // 
            // buttonTestConnection
            // 
            this.buttonTestConnection.Location = new System.Drawing.Point(241, 101);
            this.buttonTestConnection.Name = "buttonTestConnection";
            this.buttonTestConnection.Size = new System.Drawing.Size(129, 23);
            this.buttonTestConnection.TabIndex = 6;
            this.buttonTestConnection.Text = "Test Connection";
            this.buttonTestConnection.UseVisualStyleBackColor = true;
            this.buttonTestConnection.Click += new System.EventHandler(this.ButtonTestConnectionClick);
            // 
            // labelUserPassWord
            // 
            this.labelUserPassWord.AutoSize = true;
            this.labelUserPassWord.Location = new System.Drawing.Point(6, 78);
            this.labelUserPassWord.Name = "labelUserPassWord";
            this.labelUserPassWord.Size = new System.Drawing.Size(216, 13);
            this.labelUserPassWord.TabIndex = 5;
            this.labelUserPassWord.Text = "User Password [Empty for no authentication]";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(241, 75);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(129, 20);
            this.textBoxPassword.TabIndex = 4;
            // 
            // labelSonarUserName
            // 
            this.labelSonarUserName.AutoSize = true;
            this.labelSonarUserName.Location = new System.Drawing.Point(6, 52);
            this.labelSonarUserName.Name = "labelSonarUserName";
            this.labelSonarUserName.Size = new System.Drawing.Size(198, 13);
            this.labelSonarUserName.TabIndex = 3;
            this.labelSonarUserName.Text = "User Name [Empty for no authentication]";
            // 
            // labelSonarHost
            // 
            this.labelSonarHost.AutoSize = true;
            this.labelSonarHost.Location = new System.Drawing.Point(6, 26);
            this.labelSonarHost.Name = "labelSonarHost";
            this.labelSonarHost.Size = new System.Drawing.Size(60, 13);
            this.labelSonarHost.TabIndex = 1;
            this.labelSonarHost.Text = "Sonar Host";
            // 
            // textBoxSonarHost
            // 
            this.textBoxSonarHost.Location = new System.Drawing.Point(144, 23);
            this.textBoxSonarHost.Name = "textBoxSonarHost";
            this.textBoxSonarHost.Size = new System.Drawing.Size(226, 20);
            this.textBoxSonarHost.TabIndex = 0;
            // 
            // toolTipSonarServer
            // 
            this.toolTipSonarServer.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.PluginsUserControl);
            this.groupBox2.Location = new System.Drawing.Point(0, 144);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(380, 52);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "PluginController";
            // 
            // PluginsUserControl
            // 
            this.PluginsUserControl.Location = new System.Drawing.Point(9, 19);
            this.PluginsUserControl.Name = "PluginsUserControl";
            this.PluginsUserControl.Size = new System.Drawing.Size(129, 23);
            this.PluginsUserControl.TabIndex = 0;
            this.PluginsUserControl.Text = "PluginController Configuration";
            this.PluginsUserControl.UseVisualStyleBackColor = true;
            this.PluginsUserControl.Click += new System.EventHandler(this.PluginsUserControlClick);
            // 
            // SonarGeneralOptionsControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBoxCPP);
            this.Name = "SonarGeneralOptionsControlForm";
            this.Size = new System.Drawing.Size(383, 281);
            this.groupBoxCPP.ResumeLayout(false);
            this.groupBoxCPP.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxCPP;
        private System.Windows.Forms.Label labelSonarHost;
        private System.Windows.Forms.TextBox textBoxSonarHost;
        private System.Windows.Forms.ToolTip toolTipSonarServer;
        private System.Windows.Forms.Label labelUserPassWord;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelSonarUserName;
        private System.Windows.Forms.Button buttonTestConnection;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button PluginsUserControl;

    }
}
