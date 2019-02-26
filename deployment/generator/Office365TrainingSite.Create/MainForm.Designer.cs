namespace Office365TrainingSite.Create
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAdalLogin = new System.Windows.Forms.Button();
            this.lblAuthenticationStatus = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtSite = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSppkgFolder = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tgExtractAsPnP = new System.Windows.Forms.CheckBox();
            this.lblMessages = new System.Windows.Forms.Label();
            this.lblOverview = new System.Windows.Forms.Label();
            this.prgMessages = new System.Windows.Forms.ProgressBar();
            this.prgMain = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.btnExtract = new System.Windows.Forms.Button();
            this.txtTemplateName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnAdalLogin);
            this.groupBox1.Controls.Add(this.lblAuthenticationStatus);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtSite);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(521, 108);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Authenticate";
            // 
            // btnAdalLogin
            // 
            this.btnAdalLogin.Location = new System.Drawing.Point(419, 24);
            this.btnAdalLogin.Name = "btnAdalLogin";
            this.btnAdalLogin.Size = new System.Drawing.Size(87, 68);
            this.btnAdalLogin.TabIndex = 12;
            this.btnAdalLogin.Text = "Login";
            this.btnAdalLogin.UseVisualStyleBackColor = true;
            this.btnAdalLogin.Click += new System.EventHandler(this.btnAdalLogin_Click);
            // 
            // lblAuthenticationStatus
            // 
            this.lblAuthenticationStatus.AutoSize = true;
            this.lblAuthenticationStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthenticationStatus.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblAuthenticationStatus.Location = new System.Drawing.Point(99, 79);
            this.lblAuthenticationStatus.Name = "lblAuthenticationStatus";
            this.lblAuthenticationStatus.Size = new System.Drawing.Size(108, 13);
            this.lblAuthenticationStatus.TabIndex = 11;
            this.lblAuthenticationStatus.Text = "Login succeeded!";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(96, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "https://";
            // 
            // txtSite
            // 
            this.txtSite.Location = new System.Drawing.Point(145, 24);
            this.txtSite.Name = "txtSite";
            this.txtSite.Size = new System.Drawing.Size(247, 20);
            this.txtSite.TabIndex = 8;
            this.txtSite.Text = "vnextday.sharepoint.com/sites/CLO365_WinterUpdate";
            this.txtSite.TextChanged += new System.EventHandler(this.txt_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(99, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(258, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "e.g. https://microsoft.sharepoint.com/sites/usecase1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Site name:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.txtSppkgFolder);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.tgExtractAsPnP);
            this.groupBox2.Controls.Add(this.lblMessages);
            this.groupBox2.Controls.Add(this.lblOverview);
            this.groupBox2.Controls.Add(this.prgMessages);
            this.groupBox2.Controls.Add(this.prgMain);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btnExtract);
            this.groupBox2.Controls.Add(this.txtTemplateName);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 137);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(521, 267);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Extract site as PnP tenant template";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(99, 127);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(305, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "Path to assets (e.g. sppkgfile), empty is same folder as extractor";
            // 
            // txtSppkgFolder
            // 
            this.txtSppkgFolder.Location = new System.Drawing.Point(99, 100);
            this.txtSppkgFolder.Name = "txtSppkgFolder";
            this.txtSppkgFolder.Size = new System.Drawing.Size(293, 20);
            this.txtSppkgFolder.TabIndex = 23;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 103);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "Assets folder:";
            // 
            // tgExtractAsPnP
            // 
            this.tgExtractAsPnP.AutoSize = true;
            this.tgExtractAsPnP.Checked = true;
            this.tgExtractAsPnP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tgExtractAsPnP.Location = new System.Drawing.Point(99, 72);
            this.tgExtractAsPnP.Name = "tgExtractAsPnP";
            this.tgExtractAsPnP.Size = new System.Drawing.Size(115, 17);
            this.tgExtractAsPnP.TabIndex = 21;
            this.tgExtractAsPnP.Text = "Extract as .PnP file";
            this.tgExtractAsPnP.UseVisualStyleBackColor = true;
            // 
            // lblMessages
            // 
            this.lblMessages.AutoSize = true;
            this.lblMessages.Location = new System.Drawing.Point(9, 238);
            this.lblMessages.Name = "lblMessages";
            this.lblMessages.Size = new System.Drawing.Size(16, 13);
            this.lblMessages.TabIndex = 20;
            this.lblMessages.Text = "...";
            // 
            // lblOverview
            // 
            this.lblOverview.AutoSize = true;
            this.lblOverview.Location = new System.Drawing.Point(10, 185);
            this.lblOverview.Name = "lblOverview";
            this.lblOverview.Size = new System.Drawing.Size(16, 13);
            this.lblOverview.TabIndex = 19;
            this.lblOverview.Text = "...";
            // 
            // prgMessages
            // 
            this.prgMessages.Location = new System.Drawing.Point(9, 208);
            this.prgMessages.Name = "prgMessages";
            this.prgMessages.Size = new System.Drawing.Size(487, 23);
            this.prgMessages.TabIndex = 18;
            // 
            // prgMain
            // 
            this.prgMain.Location = new System.Drawing.Point(9, 156);
            this.prgMain.Name = "prgMain";
            this.prgMain.Size = new System.Drawing.Size(487, 23);
            this.prgMain.TabIndex = 17;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(99, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "e.g. Office365TrainingSite.pnp";
            // 
            // btnExtract
            // 
            this.btnExtract.Enabled = false;
            this.btnExtract.Location = new System.Drawing.Point(419, 22);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(87, 76);
            this.btnExtract.TabIndex = 13;
            this.btnExtract.Text = "Extract";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.Location = new System.Drawing.Point(99, 22);
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(293, 20);
            this.txtTemplateName.TabIndex = 9;
            this.txtTemplateName.Text = "O365CL.pnp";
            this.txtTemplateName.TextChanged += new System.EventHandler(this.txt_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Template name:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 416);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "MainForm";
            this.Text = "SharePoint Online Use case extractor";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnAdalLogin;
        private System.Windows.Forms.Label lblAuthenticationStatus;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtSite;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.TextBox txtTemplateName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar prgMessages;
        private System.Windows.Forms.ProgressBar prgMain;
        private System.Windows.Forms.Label lblMessages;
        private System.Windows.Forms.Label lblOverview;
        private System.Windows.Forms.CheckBox tgExtractAsPnP;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtSppkgFolder;
        private System.Windows.Forms.Label label5;
    }
}

