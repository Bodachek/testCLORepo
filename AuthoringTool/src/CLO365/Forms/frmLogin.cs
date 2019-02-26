using CLO365.Forms;
using System;
using System.Windows.Forms;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace CLO365
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            toolTip_Login.SetToolTip(this.loginBtn, "Log in");
            this.Height = 300;
            lblSeparator.Top = 205;
            loginBtn.Top = 220;
            lblOTP.Visible = false;
            lblOTPInfo.Visible = false;
            lnkOPTInfo.Visible = false;
            txtOTP.Visible = false;
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            fnLogin();
        }

        private void fnLogin()
        {
            Authentication objAuthentication = null;
            string strUserName = string.Empty;
            string strPassWord = string.Empty;
            string strOtp = string.Empty;
            LoginStatus enumLoginStatus = LoginStatus.InvalidCredentials;

            try
            {
                strUserName = txtUserName.Text.Trim();
                strPassWord = txtPassword.Text.Trim();
                strOtp = txtOTP.Text.Trim();

                if (string.IsNullOrEmpty(strUserName) || strUserName == "Provide GitHub username") { MessageBox.Show(Properties.Resources.MsgUsernameCannotbeEmpty, Properties.Resources.TitleError); return; }
                bool isEmail = Regex.IsMatch(strUserName, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                if (isEmail) { MessageBox.Show(Properties.Resources.MsgProvideUsernameNotEmail, Properties.Resources.TitleError); return; }
                if (string.IsNullOrEmpty(strPassWord) || strPassWord == "Provide GitHub password") { MessageBox.Show(Properties.Resources.MsgPasswordCannotbeEmpty, Properties.Resources.TitleError); return; }

                objAuthentication = new Authentication();
                fnManageAppDataFolderStructure();
                enumLoginStatus = objAuthentication.fnAuthenticateUser(strUserName, strPassWord, strOtp);

                if (enumLoginStatus == LoginStatus.InvalidOTP)
                {
                    if (txtOTP.Visible == false)
                    {
                        this.Height = 390;
                        lblSeparator.Top = 295;
                        loginBtn.Top = 310;
                        txtUserName.Enabled = false;
                        txtPassword.Enabled = false;
                        lblOTP.Visible = true;
                        lblOTPInfo.Visible = true;
                        lnkOPTInfo.Visible = true;
                        txtOTP.Visible = true;
                        txtOTP.Focus();
                        return;
                    }
                    else if (string.IsNullOrEmpty(strOtp)) { MessageBox.Show(Properties.Resources.MsgOTPCannotbeEmpty, Properties.Resources.TitleError); return; }
                    else { MessageBox.Show(Properties.Resources.MsgInvalidOTP); return; }
                }
                
                fnPostLogin(enumLoginStatus);

            }
            catch (Exception ex)
            {
                //MessageBox.Show("Login: " + ex.Message);
            }
            finally
            {
                objAuthentication = null;
                strUserName = null;
                strPassWord = null;
                strOtp = null;
            }
        }

        private void fnPostLogin(LoginStatus _LoginStatus)
        {
            try
            {
                switch (_LoginStatus)
                {
                    case LoginStatus.Success:                        
                        XDocument xdoc = XDocument.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CLO365", "config.xml"));
                        string repoPath = xdoc.Descendants("GitLocalRepo").First().Value;
                        if (repoPath.Trim() == "" || !Directory.Exists(Path.Combine(repoPath, "Staging")))
                        {
                            frmClone cloneForm = new frmClone();
                            cloneForm.Show();
                            this.Hide();
                        }
                        else
                        {
                            frmMain _mainWindow = new frmMain();
                            _mainWindow.Show();
                            this.Hide();
                        }
                        break;
                    case LoginStatus.InvalidCredentials:
                        MessageBox.Show(Properties.Resources.MsgInvalidCredentials, Properties.Resources.TitleError);
                        break;
                    case LoginStatus.NetworkError:
                        MessageBox.Show(Properties.Resources.MsgNetworkError, Properties.Resources.TitleError);
                        break;
                    case LoginStatus.AccessDenied:
                        MessageBox.Show(Properties.Resources.MsgAccessDenied, Properties.Resources.TitleError);
                        break;
                    default:
                        MessageBox.Show(Properties.Resources.MsgGenericError, Properties.Resources.TitleError);
                        break;
                }
            }
            catch(Exception ex) { //MessageBox.Show("Post Login: " + ex.Message); 
            }
        }

        private void fnManageAppDataFolderStructure()
        {
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(roaming + "/CLO365")) Directory.CreateDirectory(roaming + "/CLO365");
            if (!File.Exists(Path.Combine(roaming, "CLO365", "config.xml")))
                fnCreateConfigXml(roaming);
        }

        private void fnCreateConfigXml(string roaming)
        {
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement element1 = doc.CreateElement(string.Empty, "root", string.Empty);
            doc.AppendChild(element1);

            XmlElement element2 = doc.CreateElement(string.Empty, "GitLocalRepo", string.Empty);
            element1.AppendChild(element2);

            XmlElement element3 = doc.CreateElement(string.Empty, "UserRole", string.Empty);
            element1.AppendChild(element3);

            XmlElement element4 = doc.CreateElement(string.Empty, "UserName", string.Empty);
            element1.AppendChild(element4);

            doc.Save(Path.Combine(roaming, "CLO365", "config.xml"));
        }

        private void lnkOPTInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkOPTInfo.LinkVisited = true;
            System.Diagnostics.Process.Start("https://help.github.com/articles/accessing-github-using-two-factor-authentication/");
        }

        private void txtUserName_Enter(object sender, EventArgs e)
        {
            if (txtUserName.Text == "Provide GitHub username")
            {
                txtUserName.ForeColor = System.Drawing.Color.Black;
                txtUserName.Text = "";
            }
        }

        private void txtUserName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                txtUserName.ForeColor = System.Drawing.Color.DimGray;
                txtUserName.Text = "Provide GitHub username";
            }
        }

        private void txtPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                txtPassword.ForeColor = System.Drawing.Color.DimGray;
                txtPassword.PasswordChar = '\0';
                txtPassword.Text = "Provide GitHub password";
            }
        }

        private void txtPassword_Enter(object sender, EventArgs e)
        {
            if (txtPassword.Text == "Provide GitHub password")
            {
                txtPassword.ForeColor = System.Drawing.Color.Black;
                txtPassword.Text = "";
                txtPassword.PasswordChar = '*';
            }
        }

        private void txtOTP_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOTP.Text))
            {
                txtOTP.ForeColor = System.Drawing.Color.DimGray;
                txtOTP.Text = "Provide the 2FA code received";
            }
        }

        private void txtOTP_Enter(object sender, EventArgs e)
        {
            if (txtOTP.Text == "Provide the 2FA code received")
            {
                txtOTP.ForeColor = System.Drawing.Color.Black;
                txtOTP.Text = "";
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            this.ActiveControl = lblUserName;
        }

        private void txtUserName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                fnLogin();
            }
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                fnLogin();
            }
        }

        private void txtOTP_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                fnLogin();
            }
        }
    }
}
