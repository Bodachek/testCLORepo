using CLO365.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace CLO365
{
    public partial class frmClone : Form
    {

        public frmClone()
        {
            InitializeComponent();
            label_Downloading.Visible = false;
            toolTip_Clone.SetToolTip(this.browseBtn, "Browse folder");
            toolTip_Clone.SetToolTip(this.cloneBtn, "Download files");
        }

        private void browseBtn_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtPath.Text = dialog.FileName;
            }
        }

        private void cloneBtn_Click(object sender, EventArgs e)
        {
            string strRepositoryPath = string.Empty;
            strRepositoryPath = txtPath.Text.Trim();
            GITOperations objGITOperations = null;
            GenericStatus _GenericStatus = null;

            try
            {
                if (strRepositoryPath != "")
                {
                    Cursor.Current = Cursors.WaitCursor;
                    label_Downloading.Visible = true;
                    this.Update();
                    objGITOperations = new GITOperations();
                    strRepositoryPath = Path.Combine(txtPath.Text.Trim(),Properties.Resources.StrStagingFolderName);
                    _GenericStatus = objGITOperations.fnCloneGITRepository(strRepositoryPath);
                    if (_GenericStatus.IsSuccess == true)
                    {
                        GetSetConfig.fnSaveLocalRepoPath(txtPath.Text.Trim());
                        Cursor.Current = Cursors.Default;
                        frmMain _MainWindow = new frmMain();
                        _MainWindow.Show();
                        this.Hide();
                    }
                    else
                    {
                        label_Downloading.Visible = false;
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(Properties.Resources.MsgGenericError, Properties.Resources.TitleError);
                    }
                }
                else
                {
                    MessageBox.Show(Properties.Resources.MsgSelectFoldertoClone, Properties.Resources.TitleError);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Exception:RepoActions:Clone Click " + ex.Message);
                //Console.WriteLine("Exception:RepoActions:Clone Click " + ex.Message);
            }
            finally
            {
                strRepositoryPath = null;
                objGITOperations = null;
                _GenericStatus = null;
            }
        }

        private void frmClone_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.CanFocus == true)
            {
                if (MessageBox.Show(Properties.Resources.MsgConfirmCloseCloneForm,
                           Properties.Resources.TitleConfirmCloseCloneForm,
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }          
        }

        private void frmClone_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }       

    }
}
