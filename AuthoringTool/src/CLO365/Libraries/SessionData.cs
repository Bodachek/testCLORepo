using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CLO365
{
    public class SessionData
    {
        public SessionData()
        {
            fnLoadSessionInfo();
        }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string StagingLocalRepositoryPath { get; set; }
        public string ProductionLocalRepositoryPath { get; set; }
        private void fnLoadSessionInfo()
        {
            XDocument _XDocument = null;
            string strUserName = string.Empty;
            string strEncodedUserName = string.Empty;
            string strUserRole = string.Empty;
            string strStagingLocalRepositoryPath = string.Empty;
            string strProductionLocalRepositoryPath = string.Empty;
            byte[] _bytes = null;
            try
            {
                _XDocument = XDocument.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CLO365", "config.xml"));
                strEncodedUserName = _XDocument.Descendants("UserName").First().Value;
                _bytes = Convert.FromBase64String(strEncodedUserName);
                strUserName = System.Text.Encoding.UTF8.GetString(_bytes);
                this.UserName = strUserName;
                strStagingLocalRepositoryPath = Path.Combine(_XDocument.Descendants("GitLocalRepo").First().Value, Properties.Resources.StrStagingFolderName);
                strProductionLocalRepositoryPath = Path.Combine(_XDocument.Descendants("GitLocalRepo").First().Value, Properties.Resources.StrProductionFolderName);
                this.StagingLocalRepositoryPath = strStagingLocalRepositoryPath;
                this.ProductionLocalRepositoryPath = strProductionLocalRepositoryPath;
                strUserRole = _XDocument.Descendants("UserRole").First().Value;
                strUserRole = Utility.fnConvertFromBase64String(strUserRole);
                this.UserRole = strUserRole;
            }
            catch (Exception ex) { }
            finally
            {
                _XDocument = null;
                strUserName = null;
                strEncodedUserName = null;
                strStagingLocalRepositoryPath = null;
                strProductionLocalRepositoryPath = null;
                _bytes = null;
            }
        }
    }
}
