using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core.Framework.Provisioning.Connectors;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers;
using OfficeDevPnP.Core.Framework.Provisioning.Providers.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADAL = Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Office365TrainingSite.Create
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private bool loginDone = false;
        private ClientContext cc = null;
        private string userName = null;
        private ADAL.AuthenticationContext authContext = null;
        private const string AuthorityUri = "https://login.windows.net/common/oauth2/authorize";
        private string authToken = null;       

        public MainForm()
        {
            InitializeComponent();
        }

        private async void btnAdalLogin_Click(object sender, EventArgs e)
        {
            try
            {
                await AdalLogin(true);

                this.cc = GetAzureADAccessTokenAuthenticatedContext($"https://{txtSite.Text}", this.authToken);
                cc.Load(cc.Web, w => w.Title, w => w.Url);
                cc.ExecuteQueryRetry();

                this.loginDone = true;
                lblAuthenticationStatus.Text = "Login succeeded";
                lblAuthenticationStatus.ForeColor = Color.DarkGreen;
                EnableDisableUI();
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(ex.ToDetailedString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

        }

        private bool EnableCreate()
        {
            return this.loginDone && !string.IsNullOrEmpty(txtTemplateName.Text);
        }

        private void EnableDisableUI()
        {
            btnExtract.Enabled = EnableCreate();
            btnAdalLogin.Enabled = !btnExtract.Enabled;
            txtSite.Enabled = !btnExtract.Enabled;
        }

        private void txt_TextChanged(object sender, EventArgs e)
        {
            EnableDisableUI();
        }
        private void Log(string message)
        {
            lblOverview.Text = message;
        }

        private ClientContext GetAzureADAccessTokenAuthenticatedContext(String siteUrl, String accessToken)
        {
            var clientContext = new ClientContext(siteUrl);

            clientContext.ExecutingWebRequest += (sender, args) =>
            {
                args.WebRequestExecutor.RequestHeaders["Authorization"] = accessToken;
            };

            return clientContext;
        }

        private async Task AdalLogin(bool forcePrompt)
        {
            // Credits go to Mikael on this one (https://www.techmikael.com/2017/08/a-workaround-to-support-switching.html)
            var spUri = new Uri($"https://{txtSite.Text}");

            string resourceUri = spUri.Scheme + "://" + spUri.Authority;
            const string clientId = "9bc3ab49-b65d-410a-85ad-de819febfddc";
            const string redirectUri = "https://oauth.spops.microsoft.com/";

            ADAL.AuthenticationResult authenticationResult;

            if (authContext == null || forcePrompt)
            {
                ADAL.TokenCache cache = new ADAL.TokenCache();
                authContext = new ADAL.AuthenticationContext(AuthorityUri, cache);
            }
            try
            {
                if (forcePrompt) throw new ADAL.AdalSilentTokenAcquisitionException();
                authenticationResult = await authContext.AcquireTokenSilentAsync(resourceUri, clientId);
            }
            catch (ADAL.AdalSilentTokenAcquisitionException)
            {
                authenticationResult = await authContext.AcquireTokenAsync(resourceUri, clientId, new Uri(redirectUri), ADAL.PromptBehavior.Always, ADAL.UserIdentifier.AnyUser, null, null);
            }

            authToken = authenticationResult.CreateAuthorizationHeader();
            userName = authenticationResult.UserInfo.DisplayableId;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.lblAuthenticationStatus.Text = "";
            this.lblMessages.Text = "";
            this.lblOverview.Text = "";
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            try
            {

                Cursor.Current = Cursors.WaitCursor;
                string templateName = txtTemplateName.Text.Replace(" ", "");

                if (tgExtractAsPnP.Checked)
                {
                    templateName = $"{System.IO.Path.GetFileNameWithoutExtension(templateName)}.pnp";
                }
                else
                {
                    templateName = $"{System.IO.Path.GetFileNameWithoutExtension(templateName)}.xml";
                }

                string templateNameXml = $"{System.IO.Path.GetFileNameWithoutExtension(templateName)}.xml";

                txtTemplateName.Text = templateName;
                txtTemplateName.Enabled = false;
                btnExtract.Enabled = false;

                // extraction process
                Log("Extracting the template...");
                var connector = new FileSystemConnector(@".", "");

                var extensibilityHandlers = new List<ExtensibilityHandler>();
                extensibilityHandlers.Add(new ExtensibilityHandler()
                {
                    Assembly = $"Office365TrainingSite.Create",
                    Type = "Office365TrainingSite.Create.ClientSidePageProvider",
                    Enabled = true,
                    Configuration = ""
                });

                // Setup provisioning template creation object
                ProvisioningTemplateCreationInformation ptci = new ProvisioningTemplateCreationInformation(cc.Web)
                {
                    // Limit the amount of handlers for this package
                    // Final one
                    HandlersToProcess = Handlers.Lists | Handlers.Navigation | Handlers.WebSettings | Handlers.ExtensibilityProviders,
                    //HandlersToProcess = Handlers.Lists | Handlers.ExtensibilityProviders | Handlers.Navigation,
                    //HandlersToProcess = Handlers.ExtensibilityProviders,

                    // Persist the files
                    PersistBrandingFiles = true,
                    // Plug in custom code for page extraction
                    ExtensibilityHandlers = extensibilityHandlers,
                    // show progress
                    ProgressDelegate = delegate (String message, Int32 progress, Int32 total)
                    {
                        prgMain.Maximum = total;
                        prgMain.Value = progress;
                        Log(message);
                    },
                    // show messages
                    MessagesDelegate = delegate (string message, ProvisioningMessageType messageType)
                    {
                        if (messageType == ProvisioningMessageType.Progress)
                        {
                            string[] splitMessage = message.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                            prgMessages.Maximum = splitMessage[3].ToInt32();
                            prgMessages.Value = splitMessage[2].ToInt32();
                            lblMessages.Text = splitMessage[1];
                        }
                    }
                };

                // Create FileSystemConnector, so that we can store composed files temporarely somewhere 
                if (tgExtractAsPnP.Checked)
                {
                    ptci.FileConnector = new OpenXMLConnector(templateName, new FileSystemConnector(@".", ""), templateFileName:templateNameXml);
                }
                else
                {
                    ptci.FileConnector = new FileSystemConnector(".", "");
                }

                // Execute actual extraction of the tepmplate
                ProvisioningTemplate template = this.cc.Web.GetProvisioningTemplate(ptci);

                // Convert to tenant template
                ProvisioningHierarchy ph = new ProvisioningHierarchy
                {
                    Tenant = new ProvisioningTenant(new OfficeDevPnP.Core.Framework.Provisioning.Model.AppCatalog(), new ContentDeliveryNetwork()),
                    Author = "Microsoft",
                    Generator = "SharePoint PnP",
                    Description = "Custom learning for Office 365",
                    DisplayName = "Custom learning for Office 365",
                    ImagePreviewUrl = "https://raw.githubusercontent.com/SharePoint/sp-dev-provisioning-templates/master/tenant/O365Learning/o365cl-frontpage.png?token=AHGyU3GQfO1AHGdeRz0BsC8dtf3jCDNrks5cJhkTwA%3D%3D"
                };

                // Give template an ID
                template.Id = "CUSTOMLEARNING";

                // Configure navigation to remote existing nodes
                template.Navigation.GlobalNavigation.StructuralNavigation.RemoveExistingNodes = true;
                template.Navigation.CurrentNavigation.StructuralNavigation.RemoveExistingNodes = true;
                

                // Add parameters                
                ph.Parameters.Add("SiteUrl", "/sites/O365CL");

                // Add the site to create
                CommunicationSiteCollection site = new CommunicationSiteCollection
                {
                    ProvisioningId = "O365CLSite",
                    Language = 1033,
                    SiteDesign = "Topic",
                    Title = "Custom learning for Office 365",
                    Url = "{parameter:SiteUrl}",
                    IsHubSite = false,
                    Owner = "{CurrentUserLoginName}",
                    Description = ""
                };
                site.Templates.Add("CUSTOMLEARNING");

                ProvisioningSequence sequence = new ProvisioningSequence
                {
                    // ID is a mandatory attribute
                    ID = "CUSTOMLEARNING-sequence"
                };
                sequence.SiteCollections.Add(site);

                ph.Sequences.Add(sequence);

                // Add the packages to the tenant app catalog
                ph.Tenant.AppCatalog.Packages.Add(new Package() {
                    Action = PackageAction.UploadAndPublish,
                    Overwrite = true,
                    SkipFeatureDeployment = false,
                    Src = System.IO.Path.Combine(txtSppkgFolder.Text, "customlearning.sppkg")
                });

                // Install the package into the site
                template.ApplicationLifecycleManagement.Apps.Add(new OfficeDevPnP.Core.Framework.Provisioning.Model.App()
                {
                    AppId = "{apppackageid:Custom Learning for Office 365}",
                    Action = AppAction.Install,
                    SyncMode = SyncMode.Synchronously
                });

                // Add the storage entities
                ph.Tenant.StorageEntities.Add(new StorageEntity() { Key = "MicrosoftCustomLearningCdn", Value = "https://sharepoint.github.io/sp-custom-learning/v2/", Description = "CDN source for Microsoft Content" });
                ph.Tenant.StorageEntities.Add(new StorageEntity() { Key = "MicrosoftCustomLearningSite", Value = "{parameter:SiteUrl}", Description = "Custom Learning Site Collection" });
                ph.Tenant.StorageEntities.Add(new StorageEntity() { Key = "MicrosoftCustomLearningTelemetryOn", Value = "true", Description = "Custom Learning Telemetry Collection" });

                // Add the template
                ph.Templates.Add(template);

                if (tgExtractAsPnP.Checked)
                {
                    // Persist the tenant template
                    var fileSystemConnector = new FileSystemConnector(".", "");
                    XMLTemplateProvider provider = new XMLOpenXMLTemplateProvider(ptci.FileConnector as OpenXMLConnector)
                    {
                        Uri = templateNameXml
                    };
                    provider.Save(ph);                   

                    // Tenant level files need to be added to the package, insert the needed files in the already created package                    
                    ph.Connector = provider.Connector;
                    ProcessFiles(ph, templateNameXml, fileSystemConnector);
                }
                else
                {
                    XMLTemplateProvider provider = new XMLFileSystemTemplateProvider(".", "")
                    {
                        Uri = templateNameXml
                    };
                    provider.Save(ph);
                }

                lblMessages.Text = "Done!";
                lblOverview.Text = "Done!";
                MessageBox.Show($"Template extraction is done to {templateName}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(ex.ToDetailedString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnExtract.Enabled = true;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ProcessFiles(ProvisioningHierarchy template, string templateFileName, FileConnectorBase fileSystemConnector)
        {
            if (template.Tenant?.AppCatalog != null)
            {
                foreach (var app in template.Tenant.AppCatalog.Packages)
                {
                    AddFile(app.Src, template, fileSystemConnector);
                }
            }
            if (template.Tenant?.SiteScripts != null)
            {
                foreach (var siteScript in template.Tenant.SiteScripts)
                {
                    AddFile(siteScript.JsonFilePath, template, fileSystemConnector);
                }
            }
            if (template.Localizations != null && template.Localizations.Any())
            {
                foreach (var location in template.Localizations)
                {
                    AddFile(location.ResourceFile, template, fileSystemConnector);
                }
            }
            foreach (var tmpl in template.Templates)
            {
                if (tmpl.WebSettings != null && tmpl.WebSettings.SiteLogo != null)
                {
                    // is it a file?
                    var isFile = false;
                    try
                    {
                        using (var fileStream = fileSystemConnector.GetFileStream(tmpl.WebSettings.SiteLogo))
                        {
                            isFile = fileStream != null;
                        }
                        if (isFile)
                        {
                            AddFile(tmpl.WebSettings.SiteLogo, template, fileSystemConnector);
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                }                     
            }

        }

        private void AddFile(string sourceName, ProvisioningHierarchy hierarchy, FileConnectorBase fileSystemConnector)
        {
            using (var fs = fileSystemConnector.GetFileStream(sourceName))
            {
                var fileName = sourceName.IndexOf("\\") > 0 ? sourceName.Substring(sourceName.LastIndexOf("\\") + 1) : sourceName;
                var folderName = sourceName.IndexOf("\\") > 0 ? sourceName.Substring(0, sourceName.LastIndexOf("\\")) : "";
                hierarchy.Connector.SaveFileStream(fileName, folderName, fs);

                if (hierarchy.Connector is ICommitableFileConnector)
                {
                    ((ICommitableFileConnector)hierarchy.Connector).Commit();
                }
            }
        }

    }
}
