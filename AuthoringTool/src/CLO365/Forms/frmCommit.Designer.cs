namespace CLO365.Forms
{
    partial class frmCommit
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSubmitCommit = new System.Windows.Forms.Button();
            this.txtCommitMsg = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip_Commit = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Message";
            // 
            // btnSubmitCommit
            // 
            this.btnSubmitCommit.Location = new System.Drawing.Point(277, 145);
            this.btnSubmitCommit.Name = "btnSubmitCommit";
            this.btnSubmitCommit.Size = new System.Drawing.Size(86, 26);
            this.btnSubmitCommit.TabIndex = 7;
            this.btnSubmitCommit.Text = "Submit";
            this.btnSubmitCommit.UseVisualStyleBackColor = true;
            this.btnSubmitCommit.Click += new System.EventHandler(this.btnSubmitCommit_Click);
            // 
            // txtCommitMsg
            // 
            this.txtCommitMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommitMsg.Location = new System.Drawing.Point(72, 52);
            this.txtCommitMsg.Multiline = true;
            this.txtCommitMsg.Name = "txtCommitMsg";
            this.txtCommitMsg.Size = new System.Drawing.Size(291, 87);
            this.txtCommitMsg.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(385, 60);
            this.label2.TabIndex = 5;
            this.label2.Text = "Commit Files";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmCommit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 183);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSubmitCommit);
            this.Controls.Add(this.txtCommitMsg);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCommit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CLO365 Content authoring – Commit";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSubmitCommit;
        private System.Windows.Forms.TextBox txtCommitMsg;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolTip toolTip_Commit;
    }
}