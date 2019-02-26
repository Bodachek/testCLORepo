using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CLO365.Forms
{    
    public partial class frmCommit : Form
    {
        public string CommitMsg { get; private set; }
        public frmCommit()
        {
            InitializeComponent();
            toolTip_Commit.SetToolTip(this.btnSubmitCommit, "Commit");
        }

        private void btnSubmitCommit_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCommitMsg.Text))
            {
                this.CommitMsg = txtCommitMsg.Text;
                this.Close();
            }
            else
                MessageBox.Show(Properties.Resources.MsgCommitMsgCannotbeEmpty, Properties.Resources.TitleError);
        }
    }
}
