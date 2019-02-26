using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CLO365
{
    public static class GetSetConfig
    {
        private static string fnGetAppValue(string Key)
        {
            try
            {
                return ConfigurationManager.AppSettings[Key];
            }
            catch(Exception ex)
            {
                return "";
            }
        }

        public static string gitRepoName
        {
            get
            {
                return fnGetAppValue("gitreponame");
            }
        }

        public static string gitRepoUrl
        {
            get
            {
                return fnGetAppValue("gitrepourl");
            }
        }

        public static string gitProdRepoName
        {
            get
            {
                return fnGetAppValue("gitprodreponame");
            }
        }

        public static string gitProdRepoUrl
        {
            get
            {
                return fnGetAppValue("gitprodrepourl");
            }
        }

        public static string gitProdRemoteName
        {
            get
            {
                return fnGetAppValue("gitprodremotename");
            }
        }

        public static string gitRepoPermissionUrl
        {
            get
            {
                return fnGetAppValue("gitrepopermissionurl");
            }
        }

        public static string gitPat
        {
            get
            {
                return fnGetAppValue("gitpat");
            }
        }

        public static string imageSizeLimit
        {
            get
            {
                return fnGetAppValue("imageSizeLimit");
            }
        }

        public static string gitApiUrl
        {
            get
            {
                return fnGetAppValue("gitapiurl");
            }
        }

        public static string gitFolderName
        {
            get
            {
                return fnGetAppValue("gitfoldername");
            }
        }

        //public static string configXmlRelativePath
        //{
        //    get
        //    {
        //        return fnGetAppValue("configXmlRelativePath");
        //    }
        //}

        //public static string filesRelativePath
        //{
        //    get
        //    {
        //        return fnGetAppValue("filesRelativePath");
        //    }
        //}

        public static string assetjsonFilename
        {
            get
            {
                return fnGetAppValue("assetjsonFilename");
            }
        }

        public static string playlistjsonFilename
        {
            get
            {
                return fnGetAppValue("playlistjsonFilename");
            }
        }

        public static string metadatajsonFilename
        {
            get
            {
                return fnGetAppValue("metadatajsonFilename");
            }
        }

        public static string repoImagePathTechnology
        {
            get
            {
                return fnGetAppValue("repoImagePathTechnology");
            }
        }

        public static string repoImagePathCategory
        {
            get
            {
                return fnGetAppValue("repoImagePathCategory");
            }
        }

        public static string repoImagePathAudience
        {
            get
            {
                return fnGetAppValue("repoImagePathAudience");
            }
        }

        public static string repoImagePathPlaylist
        {
            get
            {
                return fnGetAppValue("repoImagePathPlaylist");
            }
        }

        public static void fnSaveUserInfo(WebResponse objWebResponse, string UserName)
        {
            Encoding objASCIIEncoding = ASCIIEncoding.ASCII;

            try
            {
                using (var objStreamReader = new System.IO.StreamReader(objWebResponse.GetResponseStream(), objASCIIEncoding))
                {
                    string strResponseText = objStreamReader.ReadToEnd();
                    JObject _JObject = JObject.Parse(strResponseText);

                    var objXDocument = XElement.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CLO365", "config.xml"));

                    XElement UserRoleElement = objXDocument.Element("UserRole");
                    string strUserPermissions = Utility.fnConvertToBase64String(_JObject["permission"].ToString());
                    UserRoleElement.Value = strUserPermissions.Trim();

                    XElement UserNameElement = objXDocument.Element("UserName");
                    UserNameElement.Value = Utility.fnConvertToBase64String(UserName);

                    objXDocument.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CLO365", "config.xml"));
                }
            }
            catch (Exception Ex)
            { }
        }

        public static void fnSaveLocalRepoPath(string LocalRepoPath)
        {
            XElement _ConfigXML = null;
            XElement _LocalRepoPathNode = null;

            try
            {
                _ConfigXML = XElement.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CLO365", "config.xml"));
                _LocalRepoPathNode = _ConfigXML.Element("GitLocalRepo");
                _LocalRepoPathNode.Value = LocalRepoPath;
                _ConfigXML.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CLO365", "config.xml"));
            }
            catch (Exception ex) { }
            finally
            {
                _ConfigXML = null;
                _LocalRepoPathNode = null;
            }
        }
    }
}
