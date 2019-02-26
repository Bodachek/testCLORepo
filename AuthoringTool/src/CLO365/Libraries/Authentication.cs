using System;
using System.Linq;
using System.Net;

namespace CLO365
{
    class Authentication
    {
        public LoginStatus fnAuthenticateUser(string UserName, string Password, string OTP)
        {
            CredentialCache objCredentialCache = null;
            HttpWebRequest objHttpWebRequest = null;
            WebResponse objWebResponse = null;
            LoginStatus enumLoginStatus = LoginStatus.InvalidCredentials;
            string strRequestURIString = string.Empty;
            string strHttpRequestHeaderValue = string.Empty;

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                objCredentialCache = new CredentialCache();
                strRequestURIString = GetSetConfig.gitRepoPermissionUrl + UserName + "/permission";
                objHttpWebRequest = (HttpWebRequest)WebRequest.Create(strRequestURIString);
                objHttpWebRequest.UserAgent = GetSetConfig.gitRepoName;
                strHttpRequestHeaderValue = "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(UserName + ":" + Password));
                objHttpWebRequest.Headers.Add(HttpRequestHeader.Authorization, strHttpRequestHeaderValue);
                if (OTP != null && OTP.Trim() != string.Empty) { objHttpWebRequest.Headers.Add("X-GitHub-OTP", OTP); }
                objWebResponse = objHttpWebRequest.GetResponse();
                GetSetConfig.fnSaveUserInfo(objWebResponse, UserName);
                enumLoginStatus = LoginStatus.Success;
            }
            catch (WebException ex)
            {
                if (ex.Response.Headers.AllKeys.Contains("X-GitHub-OTP"))
                {
                    enumLoginStatus = LoginStatus.InvalidOTP;
                }
                else if (ex.Message.Contains("401"))
                {
                    enumLoginStatus = LoginStatus.InvalidCredentials;
                }
                else if (ex.Message.Contains("403"))
                {
                    enumLoginStatus = LoginStatus.AccessDenied;
                }
                else
                {
                    enumLoginStatus = LoginStatus.NetworkError;
                }
            }
            finally
            {
                objCredentialCache = null;
                objHttpWebRequest = null;
                objWebResponse = null;
                strRequestURIString = null;
                strHttpRequestHeaderValue = null;
            }

            return enumLoginStatus;
        }

    }
}
