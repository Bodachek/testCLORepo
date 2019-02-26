using OfficeDevPnP.Core.Framework.Provisioning.Extensibility;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core.Diagnostics;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers.TokenDefinitions;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers.Utilities;
using System.Text.RegularExpressions;
using OfficeDevPnP.Core.Utilities;
using System.IO;
using System.Web;

namespace Office365TrainingSite.Create
{
    public class ClientSidePageProvider : IProvisioningExtensibilityHandler
    {
        private const string CAMLQueryByExtension = @"
                <View Scope='Recursive'>
                  <Query>
                    <Where>
                      <Contains>
                        <FieldRef Name='File_x0020_Type'/>
                        <Value Type='text'>aspx</Value>
                      </Contains>
                    </Where>
                  </Query>
                </View>";
        private const string FileRefField = "FileRef";
        private const string FileLeafRefField = "FileLeafRef";
        private const string ClientSideApplicationId = "ClientSideApplicationId";
        private static readonly Guid FeatureId_Web_ModernPage = new Guid("B6917CB1-93A0-4B97-A84D-7CF49975D4EC");


        #region Extensions for template creation
        public ProvisioningTemplate Extract(ClientContext ctx, ProvisioningTemplate template, ProvisioningTemplateCreationInformation creationInformation, PnPMonitoredScope scope, string configurationData)
        {
            var web = ctx.Web;

            #region Extract all client side pages
            var clientSidePageContentsHelper = new ClientSidePageContentsHelper();

            // Extract the Home Page
            web.EnsureProperties(w => w.RootFolder.WelcomePage, w => w.ServerRelativeUrl, w => w.Url);
            var homePageUrl = web.RootFolder.WelcomePage;

            // Get pages library
            ListCollection listCollection = web.Lists;
            listCollection.EnsureProperties(coll => coll.Include(li => li.BaseTemplate, li => li.RootFolder));
            var sitePagesLibrary = listCollection.Where(p => p.BaseTemplate == (int)ListTemplateType.WebPageLibrary).FirstOrDefault();
            if (sitePagesLibrary != null)
            {
                CamlQuery query = new CamlQuery
                {
                    ViewXml = CAMLQueryByExtension
                };
                var pages = sitePagesLibrary.GetItems(query);
                web.Context.Load(pages);
                web.Context.ExecuteQueryRetry();
                if (pages.FirstOrDefault() != null)
                {
                    foreach (var page in pages)
                    {
                        string pageUrl = null;
                        string pageName = "";
                        if (page.FieldValues.ContainsKey(FileRefField) && !String.IsNullOrEmpty(page[FileRefField].ToString()))
                        {
                            pageUrl = page[FileRefField].ToString();
                            pageName = page[FileLeafRefField].ToString();
                        }
                        else
                        {
                            //skip page
                            continue;
                        }

                        // Is this page the web's home page?
                        bool isHomePage = false;
                        if (pageUrl.EndsWith(homePageUrl, StringComparison.InvariantCultureIgnoreCase))
                        {
                            isHomePage = true;
                        }

                        // Is this a client side page?
                        if (FieldExistsAndUsed(page, ClientSideApplicationId) && page[ClientSideApplicationId].ToString().Equals(FeatureId_Web_ModernPage.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //if (pageName.Equals("For-administrators.aspx", StringComparison.InvariantCultureIgnoreCase)) //|| 
                            //    pageName.Equals("Getting Started in Microsoft Teams.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                            //    pageName.Equals("Quick Start Guide for Admins.aspx", StringComparison.InvariantCultureIgnoreCase))
                            //{
                            // extract the page using the OOB logic
                            clientSidePageContentsHelper.ExtractClientSidePage(web, template, creationInformation, scope, pageUrl, pageName, isHomePage);
                            //}

                        }
                    }
                }
            }
            #endregion

            #region Cleanup template
            // Mark all pages as overwrite
            foreach (var page in template.ClientSidePages)
            {
                page.Overwrite = true;
            }

            // Drop all lists except FaqList and Site Assets
            foreach (var list in template.Lists.ToList())
            {
                if (!(list.Url.Equals("SiteAssets", StringComparison.CurrentCultureIgnoreCase) ||
                      list.Url.Equals("Lists/CustomAssets", StringComparison.CurrentCultureIgnoreCase) ||
                      list.Url.Equals("Lists/CustomConfig", StringComparison.CurrentCultureIgnoreCase) ||
                      list.Url.Equals("Lists/CustomPlaylists", StringComparison.CurrentCultureIgnoreCase)
                     ))
                {
                    template.Lists.Remove(list);
                }
                else
                {
                    if (!list.Url.Equals("Lists/CustomConfig", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Drop custom security on these lists, if any.
                        list.Security = null;
                    }
                    else
                    {
                        // replace group tokens with groupid tokens
                        foreach (var sec in list.Security.RoleAssignments)
                        {
                            if (sec.Principal.Equals("{associatedownergroup}", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sec.Principal = "{associatedownergroupid}";
                            }
                            if (sec.Principal.Equals("{associatedmembergroup}", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sec.Principal = "{associatedmembergroupid}";
                            }
                            if (sec.Principal.Equals("{associatedvisitorgroup}", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sec.Principal = "{associatedvisitorgroupid}";
                            }
                        }
                    }
                }
            }

            // Cleanup navigation
            foreach(var node in template.Navigation.CurrentNavigation.StructuralNavigation.NavigationNodes.ToList())
            {
                if (node.Title.Equals("Recent"))
                {
                    template.Navigation.CurrentNavigation.StructuralNavigation.NavigationNodes.Remove(node);
                }
            }

            // Mark all files to be published in target
            foreach (var file in template.Files)
            {
                file.Level = OfficeDevPnP.Core.Framework.Provisioning.Model.FileLevel.Published;
            }
            #endregion

            #region Extract all Site Assets            
            try
            {
                var assetsTemplateList = template.Lists.Where(p => p.Url.Equals("SiteAssets", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (assetsTemplateList != null)
                {
                    var assetsList = web.GetListByUrl("SiteAssets");
                    ListItemCollection assetsListItems = assetsList.GetItems(CamlQuery.CreateAllItemsQuery());
                    web.Context.Load(assetsListItems, f => f.Include(item => item.File,
                                                                   item => item.FileSystemObjectType,
                                                                   item => item.Id,
                                                                   item => item.File.ServerRelativeUrl));
                    web.Context.Load(web, w => w.Url);
                    web.Context.ExecuteQueryRetry();
                    foreach (ListItem item in assetsListItems)
                    {
                        if (item.FileSystemObjectType == FileSystemObjectType.File)
                        {
                            if (item.File != null && !String.IsNullOrWhiteSpace(item.File.ServerRelativeUrl))
                            {
                                try
                                {
                                    var fileToCheck = GetTemplateFile(web, item.File.ServerRelativeUrl);

                                    if (!template.Files.Contains(fileToCheck))
                                    {
                                        if (PersistFile(web, creationInformation, scope, item.File.ServerRelativeUrl))
                                        {
                                            template.Files.Add(GetTemplateFile(web, item.File.ServerRelativeUrl));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToDetailedString());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                scope.LogError("Something went wrong with the extraction of the Site Assets list items. Error {0}", exc.Message);
                throw exc;
            }
            #endregion

            #region Extract all Documents
            try
            {
                var documentsList = web.GetListByUrl("Shared%20Documents");
                ListItemCollection documentsListItems = documentsList.GetItems(CamlQuery.CreateAllItemsQuery());
                web.Context.Load(documentsListItems, f => f.Include(item => item.File,
                                                               item => item.FileSystemObjectType,
                                                               item => item.Id,
                                                               item => item.File.ServerRelativeUrl));
                web.Context.Load(web, w => w.Url);
                web.Context.ExecuteQueryRetry();
                foreach (ListItem item in documentsListItems)
                {
                    if (item.FileSystemObjectType == FileSystemObjectType.File)
                    {
                        if (item.File != null && !String.IsNullOrWhiteSpace(item.File.ServerRelativeUrl))
                        {
                            try
                            {
                                var fileToCheck = GetTemplateFile(web, item.File.ServerRelativeUrl);

                                // Ensure the already added files are "seen" to avoid duplicate entries in the template
                                if (fileToCheck.Src.Contains("%20"))
                                {
                                    fileToCheck.Src = fileToCheck.Src.Replace("%20", " ");
                                }

                                if (!template.Files.Contains(fileToCheck))
                                {
                                    if (PersistFile(web, creationInformation, scope, item.File.ServerRelativeUrl))
                                    {
                                        template.Files.Add(GetTemplateFile(web, item.File.ServerRelativeUrl));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToDetailedString());
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                scope.LogError("Something went wrong with the extraction of the shared document list items. Error {0}", exc.Message);
                throw exc;
            }
            #endregion
            return template;
        }

        private string TokenizeField(Web web, string json)
        {
            web.EnsureProperties(w => w.ServerRelativeUrl, w => w.Url);

            // HostUrl token replacement
            var uri = new Uri(web.Url);
            json = Regex.Replace(json, $"{uri.Scheme}://{uri.DnsSafeHost}:{uri.Port}", "{hosturl}", RegexOptions.IgnoreCase);
            json = Regex.Replace(json, $"{uri.Scheme}://{uri.DnsSafeHost}", "{hosturl}", RegexOptions.IgnoreCase);

            // Site token replacement
            json = Regex.Replace(json, "(\"" + web.ServerRelativeUrl + ")(?!&)", "\"{site}", RegexOptions.IgnoreCase);
            json = Regex.Replace(json, "'" + web.ServerRelativeUrl, "'{site}", RegexOptions.IgnoreCase);
            json = Regex.Replace(json, ">" + web.ServerRelativeUrl, ">{site}", RegexOptions.IgnoreCase);
            json = Regex.Replace(json, web.ServerRelativeUrl, "{site}", RegexOptions.IgnoreCase);

            return json;
        }

        private static bool FieldExistsAndUsed(ListItem item, string fieldName)
        {
            return (item.FieldValues.ContainsKey(fieldName) && item[fieldName] != null);
        }

        private static bool PersistFile(Web web, ProvisioningTemplateCreationInformation creationInfo, PnPMonitoredScope scope, string serverRelativeUrl)
        {
            var success = false;
            if (creationInfo.PersistBrandingFiles)
            {
                if (creationInfo.FileConnector != null)
                {
                    if (UrlUtility.IsIisVirtualDirectory(serverRelativeUrl))
                    {
                        scope.LogWarning("File is not located in the content database. Not retrieving {0}", serverRelativeUrl);
                        return success;
                    }

                    try
                    {
                        var file = web.GetFileByServerRelativeUrl(serverRelativeUrl);
                        string fileName = string.Empty;
                        if (serverRelativeUrl.IndexOf("/") > -1)
                        {
                            fileName = serverRelativeUrl.Substring(serverRelativeUrl.LastIndexOf("/") + 1);
                        }
                        else
                        {
                            fileName = serverRelativeUrl;
                        }
                        web.Context.Load(file);
                        web.Context.ExecuteQueryRetry();
                        ClientResult<Stream> stream = file.OpenBinaryStream();
                        web.Context.ExecuteQueryRetry();

                        var baseUri = new Uri(web.Url);
                        var fullUri = new Uri(baseUri, file.ServerRelativeUrl);
                        var folderPath = HttpUtility.UrlDecode(fullUri.Segments.Take(fullUri.Segments.Count() - 1).ToArray().Aggregate((i, x) => i + x).TrimEnd('/'));

                        // Configure the filename to use 
                        fileName = HttpUtility.UrlDecode(fullUri.Segments[fullUri.Segments.Count() - 1]);

                        // Build up a site relative container url...might end up empty as well
                        String container = HttpUtility.UrlDecode(folderPath.Replace(web.ServerRelativeUrl, "")).Trim('/').Replace("/", "\\");

                        using (Stream memStream = new MemoryStream())
                        {
                            CopyStream(stream.Value, memStream);
                            memStream.Position = 0;
                            if (!string.IsNullOrEmpty(container))
                            {
                                creationInfo.FileConnector.SaveFileStream(fileName, container, memStream);
                            }
                            else
                            {
                                creationInfo.FileConnector.SaveFileStream(fileName, memStream);
                            }
                        }
                        success = true;
                    }
                    catch (ServerException ex1)
                    {
                        // If we are referring a file from a location outside of the current web or at a location where we cannot retrieve the file an exception is thrown. We swallow this exception.
                        if (ex1.ServerErrorCode != -2147024809)
                        {
                            throw;
                        }
                        else
                        {
                            scope.LogWarning("File is not necessarily located in the current web. Not retrieving {0}", serverRelativeUrl);
                        }
                    }
                }
                else
                {
                    scope.LogError("No connector present to persist homepage");
                }
            }
            else
            {
                success = true;
            }
            return success;
        }

        private static void CopyStream(Stream source, Stream destination)
        {
            byte[] buffer = new byte[32768];
            int bytesRead;

            do
            {
                bytesRead = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);
        }

        private static OfficeDevPnP.Core.Framework.Provisioning.Model.File GetTemplateFile(Web web, string serverRelativeUrl)
        {

            var webServerUrl = web.EnsureProperty(w => w.Url);
            var serverUri = new Uri(webServerUrl);
            var serverUrl = $"{serverUri.Scheme}://{serverUri.Authority}";
            var fullUri = new Uri(UrlUtility.Combine(serverUrl, serverRelativeUrl));

            var folderPath = fullUri.Segments.Take(fullUri.Segments.Count() - 1).ToArray().Aggregate((i, x) => i + x).TrimEnd('/');
            var fileName = fullUri.Segments[fullUri.Segments.Count() - 1].Replace("%20", " ");

            // store as site relative path
            folderPath = folderPath.Replace(web.ServerRelativeUrl, "").Trim('/');
            var templateFile = new OfficeDevPnP.Core.Framework.Provisioning.Model.File()
            {
                Folder = Tokenize(folderPath, web.Url),
                Src = !string.IsNullOrEmpty(folderPath) ? $"{folderPath}/{fileName}" : fileName,
                Overwrite = true,
                Level = OfficeDevPnP.Core.Framework.Provisioning.Model.FileLevel.Published,
            };

            return templateFile;
        }

        private static string Tokenize(string url, string webUrl, Web web = null)
        {
            String result = null;

            if (string.IsNullOrEmpty(url))
            {
                // nothing to tokenize...
                result = String.Empty;
            }
            else
            {
                // Decode URL
                url = Uri.UnescapeDataString(url);
                // Try with theme catalog
                if (url.IndexOf("/_catalogs/theme", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    var subsite = false;
                    if (web != null)
                    {
                        subsite = web.IsSubSite();
                    }
                    if (subsite)
                    {
                        result = url.Substring(url.IndexOf("/_catalogs/theme", StringComparison.InvariantCultureIgnoreCase)).Replace("/_catalogs/theme", "{sitecollection}/_catalogs/theme");
                    }
                    else
                    {
                        result = url.Substring(url.IndexOf("/_catalogs/theme", StringComparison.InvariantCultureIgnoreCase)).Replace("/_catalogs/theme", "{themecatalog}");
                    }
                }

                // Try with master page catalog
                if (url.IndexOf("/_catalogs/masterpage", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    var subsite = false;
                    if (web != null)
                    {
                        subsite = web.IsSubSite();
                    }
                    if (subsite)
                    {
                        result = url.Substring(url.IndexOf("/_catalogs/masterpage", StringComparison.InvariantCultureIgnoreCase)).Replace("/_catalogs/masterpage", "{sitecollection}/_catalogs/masterpage");
                    }
                    else
                    {
                        result = url.Substring(url.IndexOf("/_catalogs/masterpage", StringComparison.InvariantCultureIgnoreCase)).Replace("/_catalogs/masterpage", "{masterpagecatalog}");
                    }
                }

                // Try with site URL
                if (result != null)
                {
                    url = result;
                }
                Uri uri;
                if (Uri.TryCreate(webUrl, UriKind.Absolute, out uri))
                {
                    string webUrlPathAndQuery = System.Web.HttpUtility.UrlDecode(uri.PathAndQuery);
                    // Don't do additional replacement when masterpagecatalog and themecatalog (see #675)
                    if (url.IndexOf(webUrlPathAndQuery, StringComparison.InvariantCultureIgnoreCase) > -1 && (url.IndexOf("{masterpagecatalog}") == -1) && (url.IndexOf("{themecatalog}") == -1))
                    {
                        result = (uri.PathAndQuery.Equals("/") && url.StartsWith(uri.PathAndQuery))
                            ? "{site}" + url // we need this for DocumentTemplate attribute of pnp:ListInstance also on a root site ("/") without managed path
                            : url.Replace(webUrlPathAndQuery, "{site}");
                    }
                }

                // Default action
                if (String.IsNullOrEmpty(result))
                {
                    result = url;
                }
            }

            return (result);
        }

        #endregion

        #region Extensions for template "applying"
        public IEnumerable<TokenDefinition> GetTokens(ClientContext ctx, ProvisioningTemplate template, string configurationData)
        {
            throw new NotImplementedException();
        }

        public void Provision(ClientContext ctx, ProvisioningTemplate template, ProvisioningTemplateApplyingInformation applyingInformation, TokenParser tokenParser, PnPMonitoredScope scope, string configurationData)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}