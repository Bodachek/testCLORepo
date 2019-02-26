using CLO365.Models;
using Microsoft.VisualBasic;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CLO365.Forms
{
    public partial class frmMain : Form
    {
        #region Static Variables

        private SessionData _SessionData = new SessionData();
        private JSONOperations _JSONOperations = new JSONOperations();
        private List<AssetsModel> _AssetsList = null;
        private MetadataModel _Metadata = null;
        private List<PlaylistsModel> _PlaylistList = null;
        
        private string _SelectedItemType = "";        
        private string _SelectedItemId = "";
        private string _SelectedMetaItemType = "tech";
        private string _SelectedMetaItemValue = "";

        private bool _IsNewPlaylistAssetItem = false;
        private bool _IsNewMetadataItem = false;        
        private bool hasClickedAddAssets = false;

        private PlaylistsModel _SelectedPlaylist = null;
        private AssetsModel _SelectedAsset = null;

        private Technology _SelectedTechnology = null;
        private Category _SelectedCategory = null;
        private SubCategory _SelectedSubCategory = null;
        private SubCategorySubj1 _SelectedSubCategorySubjectLevel1 = null;
        private SubCategorySubj2 _SelectedSubCategorySubjectLevel2 = null;
        private Audience _SelectedAudience = null;
        private string _SelectedSubject = string.Empty;
        private string _SelectedSource = string.Empty;
        private string _SelectedLevel = string.Empty;
        private string _SelectedStatusTag = string.Empty;

        #endregion

        #region Constructor
        public frmMain()
        {
            InitializeComponent();
            toolTip_Main.SetToolTip(this.button_AddAsset, "Add an asset item");
            toolTip_Main.SetToolTip(this.button_addPlaylist, "Add playlist item");
            toolTip_Main.SetToolTip(this.button_AddMeta, "Add metadata item");
            toolTip_Main.SetToolTip(this.button_EditMetadata, "Edit current item");
            toolTip_Main.SetToolTip(this.button_EditProperties, "Edit current item");
            toolTip_Main.SetToolTip(this.button_RemoveItem, "Delete current item");
            toolTip_Main.SetToolTip(this.button_SaveAsset, "Save changes");
            toolTip_Main.SetToolTip(this.button_Save_Prop, "Save changes");
            toolTip_Main.SetToolTip(this.button_SaveMetaData, "Save changes");
            toolTip_Main.SetToolTip(this.button_CancelMetaData, "Cancel changes");
            toolTip_Main.SetToolTip(this.button_Cancel_Prop, "Cancel changes");
            toolTip_Main.SetToolTip(this.button_Meta_Browse, "Browse image");
            toolTip_Main.SetToolTip(this.button_BrowseImage, "Browse image");
            toolTip_Main.SetToolTip(this.btnPullFiles, "Download latest files");
            toolTip_Main.SetToolTip(this.btnPushFiles, "Upload changes to staging");
            toolTip_Main.SetToolTip(this.btnEditMetadata, "Edit metadata items");
            toolTip_Main.SetToolTip(this.btnEditPlaylist, "Edit playlist and asset items");
            toolTip_Main.SetToolTip(this.btnRefresh, "Refresh window");
            toolTip_Main.SetToolTip(this.button_MoveAssetUp, "Move asset item up");
            toolTip_Main.SetToolTip(this.button_MoveAssetDown, "Move asset item down");
            toolTip_Main.SetToolTip(this.button_ClearCatFilter, "Clear category filter");
            toolTip_Main.SetToolTip(this.button_ClearTechFilter, "Clear technology filter");
        }

        #endregion

        #region Form Load

        private void frmMain_2_Load(object sender, EventArgs e)
        {
            fnEnableDisableEditMetadata();
            if (_JSONOperations.fnLoadModelsFromJSON(ref _PlaylistList, ref _AssetsList, ref _Metadata))
            {
                fnFillFilterbyCategoryCombobox();
                fnFillFilterbyTechnologyCombobox();
                //fnLoadPlaylistTree();
            }
            else
            {
                fnValidateJSONs();
                fnEnableScreen();
            }
        }

        private void fnEnableDisableEditMetadata()
        {
            try
            {
                if (_SessionData.UserRole.ToLower() == "admin" || _SessionData.UserRole.ToLower() == "write") { btnEditMetadata.Visible = true; }
                else { btnEditMetadata.Visible = false; }
            }
            catch { }
        }

        #endregion

        #region GIT Operations       
        private void btnPullFiles_Click(object sender, EventArgs e)
        {
            GITOperations objGITOperations = null;
            bool blnStatus = false;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                objGITOperations = new GITOperations();
                blnStatus = objGITOperations.fnPullFiles();
                if (blnStatus)
                {
                    if (_JSONOperations.fnLoadModelsFromJSON(ref _PlaylistList, ref _AssetsList, ref _Metadata))
                    {
                        fnEnableEditPlaylistWindow();
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(Properties.Resources.MsgPullFilesSuccess, Properties.Resources.TitlePull);
                    }
                    else
                    {
                        fnValidateJSONs();
                        fnEnableScreen();
                    }                    
                }
                else
                {
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show(Properties.Resources.MsgGenericError, Properties.Resources.TitlePull);
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(ex.Message, Properties.Resources.TitlePull);
            }
            finally
            {
                objGITOperations = null;
            }
        }
                
        #region Push Files
        private void btnPushFiles_Click(object sender, EventArgs e)
        {
            fnPushFiles();
        }

        private void fnPushFiles()
        {            
            string strCommitMessage = string.Empty;
            bool blnCommitStatus = false;
            GITOperations objGITOperations = null;
            GenericStatus objGenericStatus = null;

            try
            {
                objGITOperations = new GITOperations();
                frmCommit _commitForm = new frmCommit();
                _commitForm.ShowDialog();
                strCommitMessage = _commitForm.CommitMsg;
                if (!string.IsNullOrEmpty(strCommitMessage))
                {
                    Cursor.Current = Cursors.WaitCursor;
                    objGITOperations = new GITOperations();
                    if (objGITOperations.fnIsLocalRepoHasUncommittedChanges())
                        blnCommitStatus = objGITOperations.fnCommitFiles(strCommitMessage);
                    //if (blnCommitStatus)
                    //{
                    objGenericStatus = objGITOperations.fnPushFiles();
                    if (objGenericStatus.IsSuccess == true)
                    {
                        _SelectedItemType = "";
                        _SelectedMetaItemType = "";
                        _SelectedPlaylist = null;
                        //fnLoadPlaylistTree();
                        fnFillFilterbyCategoryCombobox();
                        fnFillFilterbyTechnologyCombobox();
                        fnLoadMetadataTree();
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(Properties.Resources.MsgPushedSuccessfully, Properties.Resources.TitlePush);
                    }
                    else
                    {
                        //MessageBox.Show("Push Ex: " + objGenericStatus.Exception.Message + objGenericStatus.Exception.InnerException.Message);
                        if (objGenericStatus.Exception.Message.Contains("not present locally"))
                        {
                            fnMergeFiles();
                        }
                    }
                    //}
                    //else
                    //    MessageBox.Show(Properties.Resources.MsgCommitFailed);
                }
                else
                    return;
            }
            catch (Exception ex) { }
            finally
            {
                objGITOperations = null;
                objGenericStatus = null;
            }
        }

        private void fnMergeFiles()
        {
            GITOperations objGITOperations = null;
            bool blnStatus = false;
            try
            {
                Cursor.Current = Cursors.Default;
                var confirmResult = MessageBox.Show(Properties.Resources.MsgConfirmToPullonMergeException, Properties.Resources.TitleConfirmToPullonMergeException, MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    objGITOperations = new GITOperations();
                    objGITOperations.fnPullFiles();
                    if (fnValidateJSONs() == true)
                    {
                        blnStatus = _JSONOperations.fnLoadModelsFromJSON(ref _PlaylistList, ref _AssetsList, ref _Metadata);
                        if (blnStatus)
                        {
                            fnPushFiles();
                        }                        
                    }
                    else
                    {
                        fnEnableScreen();
                    }
                }
            }
            catch (Exception ex) { }
            finally { objGITOperations = null; }
        }

        private bool fnValidateJSONs()
        {
            bool blnStatus = true;
            string filenames = string.Empty;
            string prodFilenames = string.Empty;
            string strAssets = string.Empty;
            string strMetadata = string.Empty;
            string strPlaylists = string.Empty;
            string strProdAssets = string.Empty;
            string strProdMetadata = string.Empty;
            string strProdPlaylists = string.Empty;

            try
            {
                strAssets = File.ReadAllText(Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.assetjsonFilename));
                strMetadata = File.ReadAllText(Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.metadatajsonFilename));
                strPlaylists = File.ReadAllText(Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.playlistjsonFilename));

                if(File.Exists(Path.Combine(_SessionData.ProductionLocalRepositoryPath, GetSetConfig.playlistjsonFilename)))
                {
                    strProdAssets = File.ReadAllText(Path.Combine(_SessionData.ProductionLocalRepositoryPath, GetSetConfig.assetjsonFilename));
                    strProdMetadata = File.ReadAllText(Path.Combine(_SessionData.ProductionLocalRepositoryPath, GetSetConfig.metadatajsonFilename));
                    strProdPlaylists = File.ReadAllText(Path.Combine(_SessionData.ProductionLocalRepositoryPath, GetSetConfig.playlistjsonFilename));
                }

                if (strAssets.Contains("<<<<<"))
                    filenames = GetSetConfig.assetjsonFilename + " ";
                if (strMetadata.Contains("<<<<<"))
                    filenames = filenames + GetSetConfig.metadatajsonFilename + " ";
                if (strPlaylists.Contains("<<<<<"))
                    filenames = filenames + GetSetConfig.playlistjsonFilename + " ";
                if (strProdAssets.Contains("<<<<<"))
                    prodFilenames = GetSetConfig.assetjsonFilename + " ";
                if (strProdMetadata.Contains("<<<<<"))
                    prodFilenames = prodFilenames + GetSetConfig.metadatajsonFilename + " ";
                if (strProdPlaylists.Contains("<<<<<"))
                    prodFilenames = prodFilenames + GetSetConfig.playlistjsonFilename + " ";
                if (filenames != "" || prodFilenames != "")
                {
                    string conflictMsg = string.Empty;
                    if (filenames != "")
                        conflictMsg = "Staging repo files " + filenames;
                    if (prodFilenames != "")
                    {
                        if(filenames == "")
                            conflictMsg = conflictMsg + " Production repo files " + prodFilenames;
                        else
                            conflictMsg = conflictMsg + " and Production repo files " + prodFilenames;
                    }
                    blnStatus = false;
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show(conflictMsg + Properties.Resources.MsgFilesContainsMergeConflict, Properties.Resources.TitleError);
                }               
            }
            catch
            { }
            finally
            {
                filenames = null;
                strAssets = null;
                strMetadata = null;
                strPlaylists = null;
            }

            return blnStatus;
        }

        #endregion

        #endregion

        #region TreeNode Selection Events

        //>>Playlist/Asset 
        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {            
            if (fnIsDataModified() == true)
            {
                var confirmResult = MessageBox.Show(Properties.Resources.MsgChangesNotSaved, Properties.Resources.TitleChangesNotSaved, MessageBoxButtons.YesNoCancel);
                if (confirmResult == DialogResult.Yes)
                {
                    if (_SelectedItemType == "playlist")
                    {
                        fnUpdatePlaylist();
                    }
                    else
                    {
                        fnUpdateAssetItem();
                    }
                }
                else if (confirmResult == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                treeView1.SelectedNode.SelectedImageIndex = treeView1.SelectedNode.ImageIndex; 
                fnPostUpdates(sender);
            }
            catch (Exception ex) { }
        }

        private void fnPostUpdates(object sender)
        {
            propertyEditPanel.Enabled = false;
            _IsNewPlaylistAssetItem = false;
            if (btnEditMetadata.Visible == true)
            {
                button_RemoveItem.Visible = true;
                propertyEditPanel.Visible = true;
            }            
            TreeView tree = (TreeView)sender;
            var tagSplits = tree.SelectedNode.Tag.ToString().Split(',');
            _SelectedItemType = tagSplits[0];
            _SelectedItemId = tagSplits[1];
            fnLoadSelectedTreeNode(tagSplits[0], tagSplits[1]);
            if (_SelectedItemType == "playlist")
            {
                _SelectedPlaylist = _PlaylistList.FirstOrDefault(o => o.Id == _SelectedItemId);
            }
            else if (_SelectedItemType == "asset")
            {
                _SelectedAsset = _AssetsList.FirstOrDefault(o => o.Id == _SelectedItemId);
            }
        }               

        //>> Metadata
        private void treeView2_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (fnIsMetadataModified() == true)
            {
                var confirmResult = MessageBox.Show(Properties.Resources.MsgChangesNotSaved, Properties.Resources.TitleChangesNotSaved, MessageBoxButtons.YesNoCancel);
                if (confirmResult == DialogResult.Yes)
                {
                    fnUpdateMetadata();
                }
                else if (confirmResult == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            button_AddMeta.Visible = true;
            if(btnEditPlaylist.Visible == true)
                button_RemoveItem.Visible = false;
            pictureBox_Metadata.ImageLocation = null;
            fnManageMetadataSelction();
        }


        #endregion

        #region Playlist

        #region Add
        private void button_addPlaylist_Click(object sender, EventArgs e)
        {
            propertyEditPanel.Enabled = true;
            button_RemoveItem.Visible = false;
            _IsNewPlaylistAssetItem = true;
            hasClickedAddAssets = false;
            pictureBox_Playlist.ImageLocation = "";
            fnLoadSelectedTreeNode("playlist", "");
        }

        private void fnAddPlaylistTreeNode(PlaylistsModel playModel)
        {
            try
            {
                TreeNode trvRoot = new TreeNode();
                trvRoot.Text = playModel.Title;
                trvRoot.ImageIndex = 0;
                trvRoot.Tag = "playlist," + playModel.Id;
                treeView1.Nodes.Add(trvRoot);
                if (playModel.Assets != null)
                {
                    foreach (string assets in playModel.Assets)
                    {
                        var assetName = _AssetsList.FirstOrDefault(o => o.Id == assets);
                        TreeNode assetNode = new TreeNode();
                        assetNode.Text = assetName.Title;
                        assetNode.Tag = "asset," + assets;
                        trvRoot.Nodes.Add(assetNode);
                    }
                }
                _SelectedItemType = "playlist";
                _SelectedPlaylist = playModel;
                treeView1.SelectedNode = trvRoot;
            }
            catch (Exception ex) { }
        }

        #endregion

        #region Edit
        private void btnEditPlaylist_Click(object sender, EventArgs e)
        {
            fnEnableEditPlaylistWindow();
        }

        private void fnEnableEditPlaylistWindow()
        {
            btnEditPlaylist.Visible = false;
            btnEditMetadata.Visible = true;
            btnEditMetadata.Focus();
            panel_TreeView1.Visible = true;
            panel_TreeView2.Visible = false;
            propertyEditPanel.Visible = true;
            panelEditMetadata.Visible = false;
            button_AddAsset.Visible = true;
            button_addPlaylist.Visible = true;
            button_EditProperties.Visible = true;
            button_AddMeta.Visible = false;
            button_EditMetadata.Visible = false;
            button_RemoveItem.Visible = true;
            _SelectedItemType = "";
            _SelectedMetaItemType = "";
            _SelectedPlaylist = null;

            //fnLoadPlaylistTree();
            fnFillFilterbyCategoryCombobox();
            fnFillFilterbyTechnologyCombobox();
        }

        private void fnUpdatePlaylist()
        {
            if (string.IsNullOrEmpty(textBox_Id.Text.Trim())) { MessageBox.Show(Properties.Resources.MsgIdCannotbeEmpty, Properties.Resources.TitleError); return; }
            if (string.IsNullOrEmpty(textBox_Title.Text.Trim())) { MessageBox.Show(Properties.Resources.MsgTitleCannotbeEmpty, Properties.Resources.TitleError); return; }

            PlaylistsModel objPlaylist = null;
            try
            {
                if (!_IsNewPlaylistAssetItem) { objPlaylist = _PlaylistList.FirstOrDefault(o => o.Id == _SelectedItemId); } else { objPlaylist = new PlaylistsModel(); }
                var playListwithNewId = _PlaylistList.FirstOrDefault(o => o.Id == textBox_Id.Text);
                if (playListwithNewId != null && playListwithNewId.Id != objPlaylist.Id) { MessageBox.Show(Properties.Resources.MsgIdExistsAlready, Properties.Resources.TitleError); return; }
                //>> Load playlist properties
                objPlaylist.Id = textBox_Id.Text;
                objPlaylist.Title = textBox_Title.Text.Trim().Replace("\r\n", " ");
                objPlaylist.Image = textBox_Image.Text;
                objPlaylist.Description = textBox_CommonDesc.Text;
                if (!_IsNewPlaylistAssetItem) { treeView1.SelectedNode.Text = textBox_Title.Text; }
                if (comboBox_StatusTag.SelectedItem != null) { objPlaylist.StatusTag = (comboBox_StatusTag.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_StatusTag.SelectedItem.ToString()); }
                if (comboBox_Level.SelectedItem != null) { objPlaylist.Level = comboBox_Level.SelectedItem.ToString(); }
                if (comboBox_Audience.SelectedItem != null) { objPlaylist.Audience = comboBox_Audience.SelectedItem.ToString(); }
                if (comboBox_Technology.SelectedItem != null) { objPlaylist.Technology = comboBox_Technology.SelectedItem.ToString(); }
                if (comboBox_Subject.SelectedItem != null) { objPlaylist.Subject = (comboBox_Subject.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Subject.SelectedItem.ToString()); }
                if (comboBox_Category.SelectedItem != null) { objPlaylist.Category = comboBox_Category.SelectedItem.ToString(); }
                //if (comboBox_SubCategory.SelectedItem != null) { objPlaylist.SubCategory = comboBox_SubCategory.SelectedItem.ToString(); }
                if (comboBox_Source.SelectedItem != null) { objPlaylist.Source = comboBox_Source.SelectedItem.ToString(); }
                if (comboBox_SubCategory.SelectedItem != null)
                {
                    string catId = string.Empty;
                    string subCateg = string.Empty;
                    bool canExitLoop = false;
                    string selectedSubcatValue = comboBox_SubCategory.SelectedItem.ToString().Trim();
                    var subcats1 = _Metadata.Categories.FirstOrDefault(o => o.Name == comboBox_Category.SelectedItem.ToString());
                    if (subcats1.SubCategories.Count > 0)
                    {
                        foreach (var subcat in subcats1.SubCategories)
                        {
                            if (!canExitLoop)
                            {
                                if (selectedSubcatValue == subcat.Name)
                                {
                                    catId = subcat.Id;
                                    subCateg = subcat.Name;
                                    canExitLoop = true;
                                }
                                if (subcat.SubCategories.Count > 0)
                                {
                                    foreach (var subcatsubj1 in subcat.SubCategories)
                                    {
                                        if (!canExitLoop)
                                        {
                                            if (selectedSubcatValue == subcatsubj1.Name)
                                            {
                                                catId = subcatsubj1.Id;
                                                subCateg = subcat.Name;
                                                canExitLoop = true;
                                            }
                                            if (subcatsubj1.SubCategories.Count > 0)
                                            {
                                                foreach (var subcatsubj2 in subcatsubj1.SubCategories)
                                                {
                                                    if (!canExitLoop)
                                                    {
                                                        if (selectedSubcatValue == subcatsubj2.Name)
                                                        {
                                                            catId = subcatsubj2.Id;
                                                            subCateg = subcat.Name;
                                                            canExitLoop = true;
                                                        }
                                                    }
                                                    else
                                                        break;
                                                }
                                            }
                                        }
                                        else
                                            break;
                                    }
                                }
                            }
                            else
                                break;
                        }
                    }

                    objPlaylist.SubCategory = subCateg;
                    objPlaylist.CatId = catId;
                }
                //>> Add new playlist node
                if (_IsNewPlaylistAssetItem) { _PlaylistList.Add(objPlaylist); fnAddPlaylistTreeNode(objPlaylist); MessageBox.Show(Properties.Resources.MsgPlaylistAddedSuccessfully, Properties.Resources.TitleSuccess); }
                //>> Update existing playlist node
                else { treeView1.SelectedNode.Tag = "playlist," + textBox_Id.Text; MessageBox.Show(String.Format(Properties.Resources.MsgPlaylistUpdatedSuccessfully,objPlaylist.Title, Properties.Resources.TitleSuccess)); }
                propertyEditPanel.Enabled = false;
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                
            }
            catch { }
            finally { objPlaylist = null; }
        }

        #endregion

        #endregion

        #region Assets
                
        private void button_SaveAsset_Click_1(object sender, EventArgs e)
        {
            fnUpdateAssetItem();
        }

        private void button_AddAsset_Click(object sender, EventArgs e)
        {
            propertyEditPanel.Enabled = true;
            button_RemoveItem.Visible = false;
            hasClickedAddAssets = true;
            _IsNewPlaylistAssetItem = true;
            fnLoadSelectedTreeNode("asset", "");
        }

        private void fnUpdateAssetItem()
        {
            //>> Required field validation
            if (string.IsNullOrEmpty(textBox_Id.Text.Trim())) { MessageBox.Show(Properties.Resources.MsgIdCannotbeEmpty, Properties.Resources.TitleError); return; }
            if (string.IsNullOrEmpty(textBox_Title.Text.Trim())) { MessageBox.Show(Properties.Resources.MsgTitleCannotbeEmpty, Properties.Resources.TitleError); return; }
            if (string.IsNullOrEmpty(textBox_Url.Text.Trim())) { MessageBox.Show(Properties.Resources.MsgUrlCannotbeEmpty, Properties.Resources.TitleError); return; }

            AssetsModel objAssetModel = null;
            try
            {
                //>> Existing ID validation
                if (!_IsNewPlaylistAssetItem) { objAssetModel = _AssetsList.FirstOrDefault(o => o.Id == _SelectedItemId); }
                else { objAssetModel = new AssetsModel(); }
                var assetwithNewId = _AssetsList.FirstOrDefault(o => o.Id == textBox_Id.Text);
                if (assetwithNewId != null && assetwithNewId.Id != objAssetModel.Id) { MessageBox.Show(Properties.Resources.MsgIdExistsAlready, Properties.Resources.TitleError); return; }

                //>> Load properties
                objAssetModel.Id = textBox_Id.Text.Trim();
                objAssetModel.Title = textBox_Title.Text.Trim().Replace("\r\n", " ");
                objAssetModel.Description = textBox_CommonDesc.Text.Trim();
                objAssetModel.Url = textBox_Url.Text.Trim();
                objAssetModel.MediaId = textBox_MediaId.Text.Trim();              
                if (!_IsNewPlaylistAssetItem) { treeView1.SelectedNode.Text = textBox_Title.Text; }
                if (comboBox_StatusTag.SelectedItem != null) { objAssetModel.StatusTag = (comboBox_StatusTag.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_StatusTag.SelectedItem.ToString()); }
                if (comboBox_Level.SelectedItem != null) { objAssetModel.Level = comboBox_Level.SelectedItem.ToString(); }
                if (comboBox_Audience.SelectedItem != null) { objAssetModel.Audience = comboBox_Audience.SelectedItem.ToString(); }
                if (comboBox_Technology.SelectedItem != null) { objAssetModel.Technology = comboBox_Technology.SelectedItem.ToString(); }
                if (comboBox_Subject.SelectedItem != null) { objAssetModel.Subject = (comboBox_Subject.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Subject.SelectedItem.ToString()); }
                if (comboBox_Source.SelectedItem != null) { objAssetModel.Source = comboBox_Source.SelectedItem.ToString(); }
                if (comboBox_Category.SelectedItem != null) { objAssetModel.Category = comboBox_Category.SelectedItem.ToString(); }
                //if (comboBox_SubCategory.SelectedItem != null) { objAssetModel.SubCategory = comboBox_SubCategory.SelectedItem.ToString(); }
                if (comboBox_SubCategory.SelectedItem != null)
                {
                    string catId = string.Empty;
                    string subCateg = string.Empty;
                    bool canExitLoop = false;
                    string selectedSubcatValue = comboBox_SubCategory.SelectedItem.ToString().Trim();
                    var subcats1 = _Metadata.Categories.FirstOrDefault(o => o.Name == comboBox_Category.SelectedItem.ToString());
                    if (subcats1.SubCategories.Count > 0)
                    {
                        foreach (var subcat in subcats1.SubCategories)
                        {
                            if (!canExitLoop)
                            {
                                if (selectedSubcatValue == subcat.Name)
                                {
                                    catId = subcat.Id;
                                    subCateg = subcat.Name;
                                    canExitLoop = true;
                                }
                                if (subcat.SubCategories.Count > 0)
                                {
                                    foreach (var subcatsubj1 in subcat.SubCategories)
                                    {
                                        if (!canExitLoop)
                                        {
                                            if (selectedSubcatValue == subcatsubj1.Name)
                                            {
                                                catId = subcatsubj1.Id;
                                                subCateg = subcat.Name;
                                                canExitLoop = true;
                                            }
                                            if (subcatsubj1.SubCategories.Count > 0)
                                            {
                                                foreach (var subcatsubj2 in subcatsubj1.SubCategories)
                                                {
                                                    if (!canExitLoop)
                                                    {
                                                        if (selectedSubcatValue == subcatsubj2.Name)
                                                        {
                                                            catId = subcatsubj2.Id;
                                                            subCateg = subcat.Name;
                                                            canExitLoop = true;
                                                        }
                                                    }
                                                    else
                                                        break;
                                                }
                                            }
                                        }
                                        else
                                            break;
                                    }
                                }
                            }
                            else
                                break;
                        }
                    }

                    objAssetModel.SubCategory = subCateg;
                    objAssetModel.CatId = catId;
                }
                if (_IsNewPlaylistAssetItem) //>> Add new Assests node
                {
                    string parentPlaylistName = string.Empty;
                    if (_SelectedItemType == "playlist")
                    {
                        PlaylistsModel playlistModell = _PlaylistList.FirstOrDefault(o => o.Id == _SelectedItemId);
                        parentPlaylistName = playlistModell.Title;
                        playlistModell.Assets.Add(textBox_Id.Text);
                    }
                    else
                    {
                        string idofPlaylist = treeView1.SelectedNode.Parent.Tag.ToString().Split(',')[1];
                        PlaylistsModel playlistModell = _PlaylistList.FirstOrDefault(o => o.Id == idofPlaylist);
                        parentPlaylistName = playlistModell.Title;
                        playlistModell.Assets.Add(textBox_Id.Text);
                    }
                    _AssetsList.Add(objAssetModel);
                    fnAddAssetTreeNode(objAssetModel);
                                       

                    MessageBox.Show(String.Format(Properties.Resources.MsgAssetAddedSuccessfully, parentPlaylistName), Properties.Resources.TitleSuccess);
                }
                else //>> Update existing Assets node
                {
                    string idofPlaylist = treeView1.SelectedNode.Parent.Tag.ToString().Split(',')[1];
                    PlaylistsModel playlistModell = _PlaylistList.FirstOrDefault(o => o.Id == idofPlaylist);
                    playlistModell.Assets.Remove(_SelectedItemId);
                    playlistModell.Assets.Add(textBox_Id.Text);
                    treeView1.SelectedNode.Tag = "asset," + textBox_Id.Text;
                    MessageBox.Show(String.Format(Properties.Resources.MsgAssetUpdatedSuccessfully, objAssetModel.Title), Properties.Resources.TitleSuccess);
                }
                propertyEditPanel.Enabled = false;
                button_MoveAssetUp.Enabled = true;
                button_MoveAssetDown.Enabled = true;
                _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);            
            }
            catch (Exception ex) { }
            finally { objAssetModel = null; }
        }

        private void fnAddAssetTreeNode(AssetsModel assetModel)
        {
            try
            {
                TreeNode trvRoot = new TreeNode();
                trvRoot.Text = assetModel.Title;
                trvRoot.Tag = "asset," + assetModel.Id;
                trvRoot.ImageIndex = 1;
                if (_SelectedItemType == "playlist") { treeView1.SelectedNode.Nodes.Add(trvRoot); }
                else { treeView1.SelectedNode.Parent.Nodes.Add(trvRoot); }
                _SelectedItemType = "asset";
                _SelectedAsset = assetModel;
                treeView1.SelectedNode = trvRoot;
            }
            catch (Exception ex) { }
        }

        #endregion

        #region MetaData

        private void btnEditMetadata_Click(object sender, EventArgs e)
        {
            btnEditPlaylist.Visible = true;
            btnEditMetadata.Visible = false;
            button_MoveAssetUp.Visible = false;
            button_MoveAssetDown.Visible = false;
            btnEditPlaylist.Focus();
            panel_TreeView1.Visible = false;
            panel_TreeView2.Visible = true;
            propertyEditPanel.Visible = false;
            panelEditMetadata.Visible = true;
            button_AddAsset.Visible = false;
            button_addPlaylist.Visible = false;
            button_EditProperties.Visible = false;
            button_RemoveItem.Visible = false;

            _SelectedItemType = "";
            _SelectedMetaItemType = "";
            _SelectedPlaylist = null;

            fnLoadMetadataTree();
        }

        private void button_EditMetadata_Click(object sender, EventArgs e)
        {
            _IsNewMetadataItem = false;
            panelEditMetadata.Enabled = true;
        }

        private void button_Meta_Browse_Click_1(object sender, EventArgs e)
        {
            try
            {
               
                switch (_SelectedMetaItemType)
                {
                    case "tech":
                        fnLoadImage(GetSetConfig.repoImagePathTechnology, true);
                        break;

                    case "cate":
                        fnLoadImage(GetSetConfig.repoImagePathCategory, true);
                        break;

                    case "subc":
                        fnLoadImage(GetSetConfig.repoImagePathCategory, true);
                        break;

                    case "subcsubj1":
                        fnLoadImage(GetSetConfig.repoImagePathCategory, true);
                        break;

                    case "subcsubj2":
                        fnLoadImage(GetSetConfig.repoImagePathCategory, true);
                        break;

                    case "audi":
                        fnLoadImage(GetSetConfig.repoImagePathAudience, true);
                        break;
                }
            }
            catch (Exception ex) { }
        }
       
        private void button_AddMeta_Click(object sender, EventArgs e)
        {
            _IsNewMetadataItem = true;
            panelEditMetadata.Visible = true;
            panelEditMetadata.Enabled = true;
            textBox_MetaId.Text = Guid.NewGuid().ToString();
            textBox_Meta_Name.Text = "";
            textBox_Meta_Image.Text = "";
            if(comboBox_Meta_Security.Items.Count > 0)
                comboBox_Meta_Security.SelectedIndex = 0;
            pictureBox_Metadata.ImageLocation = "";
            switch (_SelectedMetaItemType)
            {
                case "tech":
                    if (string.IsNullOrEmpty(_SelectedMetaItemValue))
                    {
                        label_Meta_Heading.Text = "Technology";
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                    }
                    else
                    {
                        label_Meta_Heading.Text = "Subject";
                        fnDisableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                    }
                    break;

                case "subj":
                    label_Meta_Heading.Text = "Subject";
                    fnDisableMetadataItems();
                    fnDisableSecurityItem();
                    fnDisableCategoryItems();
                    break;

                case "cate":
                    _SelectedCategory = null;
                    if (string.IsNullOrEmpty(_SelectedMetaItemValue))
                    {
                        label_Meta_Heading.Text = "Category";
                        fnEnableMetadataItems();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        comboBox_Meta_Security.Visible = true;
                    }
                    else
                    {
                        label_Meta_Heading.Text = "Sub Category";
                        fnEnableMetadataItems();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        comboBox_Meta_Security.Visible = true;
                    }
                    break;

                case "subc":
                    label_Meta_Heading.Text = "Subject";
                    fnEnableMetadataItems();
                    fnEnableCategoryItems();
                    label_Meta_Security.Visible = true;
                    comboBox_Meta_Security.Visible = true;
                    break;

                case "audi":
                    label_Meta_Heading.Text = "Audience";
                    fnEnableMetadataItems();
                    fnDisableSecurityItem();
                    fnDisableCategoryItems();
                    break;

                case "sour":
                    label_Meta_Heading.Text = "Source";
                    fnDisableMetadataItems();
                    fnDisableSecurityItem();
                    fnDisableCategoryItems();
                    break;

                case "leve":
                    label_Meta_Heading.Text = "Level";
                    fnDisableMetadataItems();
                    fnDisableSecurityItem();
                    fnDisableCategoryItems();
                    break;

                case "subcsubj1":
                    label_Meta_Heading.Text = "Subject";
                    fnEnableMetadataItems();
                    fnEnableCategoryItems();
                    label_Meta_Security.Visible = true;
                    comboBox_Meta_Security.Visible = true;
                    break;

                case "subcsubj2":
                    label_Meta_Heading.Text = "Subject";
                    fnEnableMetadataItems();
                    fnEnableCategoryItems();
                    label_Meta_Security.Visible = true;
                    comboBox_Meta_Security.Visible = true;
                    break;

                case "stat":
                    label_Meta_Heading.Text = "Status Tag";
                    fnDisableMetadataItems();
                    fnDisableSecurityItem();
                    fnDisableCategoryItems();
                    break;
            }
        }

        private void button_CancelMetaData_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(Properties.Resources.MsgConfirmCancelClick, Properties.Resources.TitleConfirmCancelClick, MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                _IsNewMetadataItem = false;
                panelEditMetadata.Enabled = false;
                switch (_SelectedMetaItemType)
                {
                    case "tech":
                        _SelectedTechnology = null;
                        break;
                    case "subj":
                        _SelectedSubject = null;
                        break;
                    case "cate":
                        _SelectedCategory = null;
                        break;
                    case "subc":
                        _SelectedSubCategory = null;
                        break;
                    case "subcsubj1":
                        _SelectedSubCategorySubjectLevel1 = null;
                        break;
                    case "subcsubj2":
                        _SelectedSubCategorySubjectLevel2 = null;
                        break;
                    case "audi":
                        _SelectedAudience = null;
                        break;
                    case "sour":
                        _SelectedSource = null;
                        break;
                    case "leve":
                        _SelectedLevel = null;
                        break;
                    case "stat":
                        _SelectedStatusTag = null;
                        break;
                }
            }
        }

        private void button_SaveMetaData_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox_Meta_Name.Text.Trim().Replace("\r\n", " "))) { fnUpdateMetadata(); }
            else { MessageBox.Show(Properties.Resources.MsgNameCannotbeEmpty, Properties.Resources.TitleError); }
        }

        private void fnAddMetadataTreeNode(string tag, string name)
        {
            try
            {
                TreeNode trvNode = new TreeNode();
                trvNode.Text = name;
                trvNode.Tag = tag;
                if (treeView2.SelectedNode != null)
                {
                    //if (treeView2.SelectedNode.Tag.ToString().Split(',')[1] == string.Empty) { treeView2.SelectedNode.Nodes.Add(trvNode); }
                    //else { treeView2.SelectedNode.Parent.Nodes.Add(trvNode); }
                    if((_SelectedMetaItemType == "subcsubj2" || _SelectedMetaItemType == "subj" || _SelectedMetaItemType == "audi" || _SelectedMetaItemType == "audi" || _SelectedMetaItemType == "sour" || _SelectedMetaItemType == "leve" || _SelectedMetaItemType == "stat") && !string.IsNullOrEmpty(_SelectedMetaItemValue))
                        treeView2.SelectedNode.Parent.Nodes.Add(trvNode);
                    else
                        treeView2.SelectedNode.Nodes.Add(trvNode);
                }
                else { treeView2.Nodes[0].Nodes.Add(trvNode); }
                treeView2.SelectedNode = trvNode;
            }
            catch (Exception ex) { }
        }

        private void fnUpdateTechnologyMetadata(string[] metaNames)
        {
            try
            {
                Technology techModel = null;
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (!_IsNewMetadataItem) { techModel = _Metadata.Technologies.FirstOrDefault(o => o.Name == metaNames[1]); }
                else { techModel = new Technology(); }
                var TechwithNewName = _Metadata.Technologies.FirstOrDefault(o => o.Name == metaName);
                if (TechwithNewName != null && TechwithNewName.Name != techModel.Name) { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); return; }
                techModel.Name = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                techModel.Image = textBox_Meta_Image.Text;
                if (!_IsNewMetadataItem)
                {
                    var assetsWithTech = _AssetsList.Where(o => o.Technology == metaNames[1]);
                    foreach (var item in assetsWithTech) { item.Technology = techModel.Name; }
                    var playlistsWithTech = _PlaylistList.Where(o => o.Technology == metaNames[1]);
                    foreach (var item in playlistsWithTech) { item.Technology = techModel.Name; }
                    treeView2.SelectedNode.Tag = "tech," + techModel.Name;
                    treeView2.SelectedNode.Text = techModel.Name.Trim();
                    _SelectedTechnology = techModel;
                    MessageBox.Show(Properties.Resources.MsgTechUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                }
                else
                {
                    techModel.Subjects = new List<object>();
                    _Metadata.Technologies.Add(techModel);
                    _SelectedTechnology = techModel;
                    fnAddMetadataTreeNode("tech," + techModel.Name, techModel.Name);                    
                    MessageBox.Show(Properties.Resources.MsgTechAddedSuccessfully, Properties.Resources.TitleSuccess);
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
            }
            catch { }
        }

        private void fnUpdateSubjectMetadata(string[] metaNames)
        {
            try
            {
                Technology techModel = null;
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (_IsNewMetadataItem && _SelectedMetaItemType == "tech")
                    techModel = _Metadata.Technologies.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Tag.ToString().Split(new[] { ',' }, 2)[1]);
                else
                    techModel = _Metadata.Technologies.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]);
                if (!_IsNewMetadataItem && metaNames[1] != textBox_Meta_Name.Text.Trim())
                {
                    if (!techModel.Subjects.Contains(textBox_Meta_Name.Text.Trim()))
                    {
                        techModel.Subjects.Add(metaName);
                        var assetsWithSubj = _AssetsList.Where(o => o.Subject == metaNames[1]);
                        foreach (var item in assetsWithSubj) { item.Subject = textBox_Meta_Name.Text.Trim(); }
                        var playlistsWithSubj = _PlaylistList.Where(o => o.Subject == metaNames[1]);
                        foreach (var item in playlistsWithSubj) { item.Subject = textBox_Meta_Name.Text.Trim(); }
                        treeView2.SelectedNode.Tag = "subj," + textBox_Meta_Name.Text.Trim();
                        treeView2.SelectedNode.Text = textBox_Meta_Name.Text.Trim();
                        _SelectedSubject = textBox_Meta_Name.Text.Trim();
                        MessageBox.Show(Properties.Resources.MsgSubjectUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                    }
                    else { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); }
                }
                else
                {
                    if (!techModel.Subjects.Contains(metaName))
                    {
                        techModel.Subjects.Add(metaName);
                        _SelectedSubject = metaName;
                        _SelectedTechnology = null;
                        fnAddMetadataTreeNode("subj," + metaName, metaName);
                        MessageBox.Show(Properties.Resources.MsgSubjectAddedSuccessfully);
                    }
                    else { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); }
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
            }
            catch { }
        }

        private void fnUpdateCategoryMetadata(string[] metaNames)
        {

            Category catModel = null;
            try
            {
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (!_IsNewMetadataItem) { catModel = _Metadata.Categories.FirstOrDefault(o => o.Name == metaNames[1]); }
                else { catModel = new Category(); }
                var catwithNewName = _Metadata.Categories.FirstOrDefault(o => o.Name == metaName);
                if (catwithNewName != null && catwithNewName.Name != catModel.Name) { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); return; }
                catModel.Id = textBox_MetaId.Text.Trim();
                catModel.Name = metaName;
                catModel.Image = textBox_Meta_Image.Text;
                catModel.Security = (comboBox_Meta_Security.SelectedItem == null ? String.Empty : (comboBox_Meta_Security.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Meta_Security.SelectedItem.ToString()));
                catModel.Technology = (comboBox_MetaTechnology.SelectedItem == null ? String.Empty : (comboBox_MetaTechnology.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_MetaTechnology.SelectedItem.ToString()));
                catModel.Type = "Category";
                catModel.SubCategories = new List<SubCategory>();

                if (!_IsNewMetadataItem)
                {
                    var assetsWitCat = _AssetsList.Where(o => o.Category == metaNames[1]);
                    foreach (var item in assetsWitCat) { item.Category = catModel.Name; }
                    var playlistsWithCat = _PlaylistList.Where(o => o.Category == metaNames[1]);
                    foreach (var item in playlistsWithCat) { item.Technology = catModel.Name; }
                    treeView2.SelectedNode.Tag = "cate," + catModel.Name;
                    treeView2.SelectedNode.Text = catModel.Name;
                    _SelectedCategory = catModel;
                    MessageBox.Show(Properties.Resources.MsgCategoryUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                }
                else
                {
                    catModel.SubCategories = new List<SubCategory>();
                    _Metadata.Categories.Add(catModel);
                    _SelectedCategory = catModel;
                    fnAddMetadataTreeNode("cate," + catModel.Name, catModel.Name);
                    MessageBox.Show(Properties.Resources.MsgCategoryAddedSuccessfully, Properties.Resources.TitleSuccess);
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
            }
            catch { }
            finally { catModel = null; }
        }

        private void fnUpdateSubCategoryMetadata(string[] metaNames)
        {
            SubCategory subcatModel = null;
            try
            {
                Category catModel = null;
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (_IsNewMetadataItem)
                    catModel = _Metadata.Categories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Tag.ToString().Split(new[] { ',' }, 2)[1]);
                else
                    catModel = _Metadata.Categories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]);
                if (!_IsNewMetadataItem) { subcatModel = catModel.SubCategories.FirstOrDefault(o => o.Name == metaNames[1]); }
                else { subcatModel = new SubCategory(); }
                var subcatwithNewName = catModel.SubCategories.FirstOrDefault(o => o.Name == metaName);
                if (subcatwithNewName != null && subcatwithNewName.Name != subcatModel.Name) { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); return; }
                subcatModel.Id = textBox_MetaId.Text.Trim();
                subcatModel.Name = metaName;
                subcatModel.Image = textBox_Meta_Image.Text.Trim();
                subcatModel.Type = "SubCategory";
                subcatModel.SubCategories = new List<SubCategorySubj1>();
                subcatModel.Security = (comboBox_Meta_Security.SelectedItem == null ? String.Empty : (comboBox_Meta_Security.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Meta_Security.SelectedItem.ToString()));
                subcatModel.Technology = (comboBox_MetaTechnology.SelectedItem == null ? String.Empty : (comboBox_MetaTechnology.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_MetaTechnology.SelectedItem.ToString()));
                if (!_IsNewMetadataItem)
                {
                    var assetsWithsubcat = _AssetsList.Where(o => o.SubCategory == metaNames[1]);
                    foreach (var item in assetsWithsubcat) { item.SubCategory = subcatModel.Name; }
                    var playlistsWithsubcat = _PlaylistList.Where(o => o.SubCategory == metaNames[1]);
                    foreach (var item in playlistsWithsubcat) { item.SubCategory = subcatModel.Name; }
                    treeView2.SelectedNode.Tag = "subc," + subcatModel.Name;
                    treeView2.SelectedNode.Text = subcatModel.Name.Trim();
                    _SelectedSubCategory = subcatModel;
                    MessageBox.Show(Properties.Resources.MsgSubCategoryUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                }
                else
                {
                    subcatModel.SubCategories = new List<SubCategorySubj1>();
                    catModel.SubCategories.Add(subcatModel);
                    _SelectedSubCategory = subcatModel;
                    fnAddMetadataTreeNode("subc," + subcatModel.Name, subcatModel.Name);
                    MessageBox.Show(Properties.Resources.MsgSubCategoryAddedSuccessfully, Properties.Resources.TitleSuccess);
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
            }
            catch(Exception ex) { }
            finally { subcatModel = null; }
        }

        private void fnUpdateSubCategorySubjectLevel1Metadata(string[] metaNames)
        {
            SubCategorySubj1 subcatSubj1Model = null;
            try
            {
                SubCategory subcatModel = null;
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (_IsNewMetadataItem)
                    subcatModel = _Metadata.Categories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]).SubCategories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Tag.ToString().Split(new[] { ',' }, 2)[1]);
                else
                    subcatModel = _Metadata.Categories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]).SubCategories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]);
                if (!_IsNewMetadataItem) { subcatSubj1Model = subcatModel.SubCategories.FirstOrDefault(o => o.Name == metaNames[1]); }
                else { subcatSubj1Model = new SubCategorySubj1(); }
                var subcatwithNewName = subcatModel.SubCategories.FirstOrDefault(o => o.Name == metaName);
                if (subcatwithNewName != null && subcatwithNewName.Name != subcatSubj1Model.Name) { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); return; }

                subcatSubj1Model.Name = metaName;
                subcatSubj1Model.Image = textBox_Meta_Image.Text.Trim();
                subcatSubj1Model.Id = textBox_MetaId.Text.Trim();
                subcatSubj1Model.Security = (comboBox_Meta_Security.SelectedItem == null ? String.Empty : (comboBox_Meta_Security.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Meta_Security.SelectedItem.ToString()));
                subcatSubj1Model.Technology = (comboBox_MetaTechnology.SelectedItem == null ? String.Empty : (comboBox_MetaTechnology.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_MetaTechnology.SelectedItem.ToString()));
                subcatSubj1Model.Type = "Subject";
                subcatSubj1Model.SubCategories = new List<SubCategorySubj2>();                
                if (!_IsNewMetadataItem)
                {
                    treeView2.SelectedNode.Tag = "subcsubj1," + subcatSubj1Model.Name;
                    treeView2.SelectedNode.Text = subcatSubj1Model.Name.Trim();
                    panelEditMetadata.Enabled = false;
                    _SelectedSubCategorySubjectLevel1 = subcatSubj1Model;
                    MessageBox.Show(Properties.Resources.MsgSubjectUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                }
                else
                {
                    subcatSubj1Model.SubCategories = new List<SubCategorySubj2>();
                    subcatModel.SubCategories.Add(subcatSubj1Model);
                    _SelectedSubCategorySubjectLevel1 = subcatSubj1Model;
                    fnAddMetadataTreeNode("subcsubj1," + subcatSubj1Model.Name, subcatSubj1Model.Name);
                    panelEditMetadata.Enabled = false;
                    MessageBox.Show(Properties.Resources.MsgSubjectAddedSuccessfully, Properties.Resources.TitleSuccess);
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);

            }
            catch { }
            finally { subcatSubj1Model = null; }
        }

        private void fnUpdateSubCategorySubjectLevel2Metadata(string[] metaNames)
        {
            SubCategorySubj2 subcatSubj2Model = null;

            try
            {
                SubCategorySubj1 subcatModel = null;
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (_IsNewMetadataItem && _SelectedMetaItemType == "subcsubj1")
                    subcatModel = _Metadata.Categories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]).SubCategories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]).SubCategories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Tag.ToString().Split(new[] { ',' }, 2)[1]);
                else
                    subcatModel = _Metadata.Categories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]).SubCategories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]).SubCategories.FirstOrDefault(o => o.Name == treeView2.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1]);
                if (!_IsNewMetadataItem) { subcatSubj2Model = subcatModel.SubCategories.FirstOrDefault(o => o.Name == metaNames[1]); }
                else { subcatSubj2Model = new SubCategorySubj2(); }
                var subcatwithNewName = subcatModel.SubCategories.FirstOrDefault(o => o.Name == metaName);
                if (subcatwithNewName != null && subcatwithNewName.Name != subcatSubj2Model.Name) { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); return; }

                subcatSubj2Model.Name = metaName;
                subcatSubj2Model.Image = textBox_Meta_Image.Text.Trim();
                subcatSubj2Model.Id = textBox_MetaId.Text.Trim();
                subcatSubj2Model.Security = (comboBox_Meta_Security.SelectedItem == null ? String.Empty : comboBox_Meta_Security.SelectedItem.ToString());
                subcatSubj2Model.Type = "Subject";
                subcatSubj2Model.Security = (comboBox_Meta_Security.SelectedItem == null ? String.Empty : (comboBox_Meta_Security.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Meta_Security.SelectedItem.ToString()));
                subcatSubj2Model.Technology = (comboBox_MetaTechnology.SelectedItem == null ? String.Empty : (comboBox_MetaTechnology.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_MetaTechnology.SelectedItem.ToString()));
                if (!_IsNewMetadataItem)
                {
                    treeView2.SelectedNode.Tag = "subcsubj2," + subcatSubj2Model.Name;
                    treeView2.SelectedNode.Text = subcatSubj2Model.Name.Trim();
                    panelEditMetadata.Enabled = false;
                    _SelectedSubCategorySubjectLevel2 = subcatSubj2Model;
                    MessageBox.Show(Properties.Resources.MsgSubjectUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                }
                else
                {
                    subcatModel.SubCategories.Add(subcatSubj2Model);
                    _SelectedSubCategorySubjectLevel2 = subcatSubj2Model;
                    fnAddMetadataTreeNode("subcsubj2," + subcatSubj2Model.Name, subcatSubj2Model.Name);
                    panelEditMetadata.Enabled = false;
                    MessageBox.Show(Properties.Resources.MsgSubCategoryAddedSuccessfully, Properties.Resources.TitleSuccess);
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);
            }
            catch { }
            finally { subcatSubj2Model = null; }
        }

        private void fnUpdateAudienceMetadata(string[] metaNames)
        {
            Audience audModel = null;
            try
            {
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (!_IsNewMetadataItem) { audModel = _Metadata.Audiences.FirstOrDefault(o => o.Name == metaNames[1]); }
                else { audModel = new Audience(); }
                var audwithNewName = _Metadata.Audiences.FirstOrDefault(o => o.Name == metaName);
                if (audwithNewName != null && audwithNewName.Name != audModel.Name) { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); return; }

                audModel.Name = metaName;
                audModel.Image = textBox_Meta_Image.Text.Trim();

                if (!_IsNewMetadataItem)
                {
                    var assetsWithAud = _AssetsList.Where(o => o.Audience == metaNames[1]);
                    foreach (var item in assetsWithAud) { item.Audience = audModel.Name; }
                    var playlistsWithAud = _PlaylistList.Where(o => o.Audience == metaNames[1]);
                    foreach (var item in playlistsWithAud) { item.Audience = audModel.Name; }
                    treeView2.SelectedNode.Tag = "audi," + audModel.Name;
                    treeView2.SelectedNode.Text = audModel.Name;
                    _SelectedAudience = audModel;
                    MessageBox.Show(Properties.Resources.MsgAudienceUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                }
                else
                {
                    _Metadata.Audiences.Add(audModel);
                    _SelectedAudience = audModel;
                    fnAddMetadataTreeNode("audi," + audModel.Name, audModel.Name);
                    MessageBox.Show(Properties.Resources.MsgAudienceAddedSuccessfully, Properties.Resources.TitleSuccess);
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
            }
            catch { }
            finally { audModel = null; }
        }

        private void fnUpdateSourceMetadata(string[] metaNames)
        {
            try
            {
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (!_IsNewMetadataItem && metaNames[1] != metaName)
                {
                    if (!_Metadata.Sources.Contains(metaName))
                    {
                        _Metadata.Sources.Add(metaName);
                        var assetsWithSour = _AssetsList.Where(o => o.Source == metaNames[1]);
                        foreach (var item in assetsWithSour) { item.Source = metaName; }
                        var playlistsWithSour = _PlaylistList.Where(o => o.Source == metaNames[1]);
                        foreach (var item in playlistsWithSour) { item.Source = metaName; }
                        treeView2.SelectedNode.Tag = "sour," + metaName;
                        treeView2.SelectedNode.Text = metaName;
                        _SelectedSource = metaName;
                        MessageBox.Show(Properties.Resources.MsgSourceUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                    }
                    else { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); }
                }
                else
                {
                    if (!_Metadata.Sources.Contains(metaName))
                    {
                        _Metadata.Sources.Add(metaName);
                        _SelectedSource = metaName;
                        fnAddMetadataTreeNode("sour," + metaName, metaName);
                        MessageBox.Show(Properties.Resources.MsgSourceAddedSuccessfully, Properties.Resources.TitleSuccess);
                    }
                    else { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); }
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
            }
            catch { }
        }

        private void fnUpdateLevelMetadata(string[] metaNames)
        {
            try
            {
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (!_IsNewMetadataItem && metaNames[1] != metaName)
                {
                    if (!_Metadata.Levels.Contains(metaName))
                    {
                        _Metadata.Levels.Add(metaName);
                        var assetsWithSour = _AssetsList.Where(o => o.Level == metaNames[1]);
                        foreach (var item in assetsWithSour) { item.Level = metaName; }
                        var playlistsWithSour = _PlaylistList.Where(o => o.Level == metaNames[1]);
                        foreach (var item in playlistsWithSour) { item.Level = metaName; }
                        treeView2.SelectedNode.Tag = "leve," + metaName;
                        treeView2.SelectedNode.Text = metaName;
                        _SelectedLevel = metaName;
                        MessageBox.Show(Properties.Resources.MsgLevelUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                    }
                    else { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); }
                }
                else
                {
                    if (!_Metadata.Levels.Contains(metaName))
                    {
                        _Metadata.Levels.Add(metaName);
                        _SelectedLevel = metaName;
                        fnAddMetadataTreeNode("leve," + metaName, metaName);
                        MessageBox.Show(Properties.Resources.MsgLevelAddedSuccessfully, Properties.Resources.TitleSuccess);
                    }
                    else { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); }
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
            }
            catch { }
        }

        private void fnUpdateStatusTagMetadata(string[] metaNames)
        {
            try
            {
                string metaName = textBox_Meta_Name.Text.Trim().Replace("\r\n", " ");
                if (!_IsNewMetadataItem && metaNames[1] != metaName)
                {
                    if (!_Metadata.StatusTag.Contains(metaName))
                    {
                        _Metadata.StatusTag.Add(metaName);
                        var assetsWithStat = _AssetsList.Where(o => o.StatusTag == metaNames[1]);
                        foreach (var item in assetsWithStat) { item.StatusTag = metaName; }
                        var playlistsWithStat = _PlaylistList.Where(o => o.StatusTag == metaNames[1]);
                        foreach (var item in playlistsWithStat) { item.StatusTag = metaName; }
                        treeView2.SelectedNode.Tag = "stat," + metaName;
                        treeView2.SelectedNode.Text = metaName;
                        _SelectedStatusTag = metaName;
                        MessageBox.Show(Properties.Resources.MsgLevelUpdatedSuccessfully, Properties.Resources.TitleSuccess);
                    }
                    else { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); }
                }
                else
                {
                    if (!_Metadata.StatusTag.Contains(metaName))
                    {
                        _Metadata.StatusTag.Add(metaName);
                        _SelectedStatusTag = metaName;
                        fnAddMetadataTreeNode("stat," + metaName, metaName);
                        MessageBox.Show(Properties.Resources.MsgLevelAddedSuccessfully, Properties.Resources.TitleSuccess);
                    }
                    else { MessageBox.Show(Properties.Resources.MsgNameExistsAlready, Properties.Resources.TitleError); }
                }
                _JSONOperations.fnSaveMetadataLocalRepoFile(_Metadata);
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
            }
            catch { }
        }

        private void fnUpdateMetadata()
        {
            try
            {
                var metaNames = treeView2.SelectedNode.Tag.ToString().Split(new[] { ',' }, 2);
                switch (_SelectedMetaItemType)
                {
                    case "tech":                        
                        if(!string.IsNullOrEmpty(metaNames[1]) && _IsNewMetadataItem)
                            fnUpdateSubjectMetadata(metaNames);
                        else
                            fnUpdateTechnologyMetadata(metaNames);
                        break;
                    case "subj":
                        fnUpdateSubjectMetadata(metaNames);
                        break;
                    case "cate":
                        if (!string.IsNullOrEmpty(metaNames[1]) && _IsNewMetadataItem)
                            fnUpdateSubCategoryMetadata(metaNames);
                        else
                            fnUpdateCategoryMetadata(metaNames);
                        break;
                    case "subc":
                        if(_IsNewMetadataItem)
                            fnUpdateSubCategorySubjectLevel1Metadata(metaNames);
                        else
                            fnUpdateSubCategoryMetadata(metaNames);
                        break;
                    case "subcsubj1":
                        if (_IsNewMetadataItem)
                            fnUpdateSubCategorySubjectLevel2Metadata(metaNames);
                        else
                            fnUpdateSubCategorySubjectLevel1Metadata(metaNames);
                        break;
                    case "subcsubj2":
                        fnUpdateSubCategorySubjectLevel2Metadata(metaNames);
                        break;
                    case "audi":
                        fnUpdateAudienceMetadata(metaNames);
                        break;
                    case "sour":
                        fnUpdateSourceMetadata(metaNames);
                        break;
                    case "leve":
                        fnUpdateLevelMetadata(metaNames);
                        break;
                    case "stat":
                        fnUpdateStatusTagMetadata(metaNames);
                        break;
                }

            }
            catch (Exception ex) { }
        }


        #endregion

        #region Node Properties Events
   
        private void button_BrowseImage_Click_1(object sender, EventArgs e)
        {
            fnLoadImage(GetSetConfig.repoImagePathPlaylist, false);
        }
             
        private void comboBox_Technology_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                ComboBox _combo_Box = (ComboBox)sender;
                if (_combo_Box.SelectedItem != null)
                {
                    var techno = _Metadata.Technologies.FirstOrDefault(o => o.Name == _combo_Box.SelectedItem.ToString());
                    comboBox_Subject.DataSource = null;
                    comboBox_Subject.Items.Clear();
                    if (techno.Subjects.Count > 0) { comboBox_Subject.DataSource = techno.Subjects; comboBox_Subject.SelectedIndex = 0; }
                }
            }
            catch (Exception ex) { }
        }

        private void comboBox_Category_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                ComboBox _combo_Box = (ComboBox)sender;
                if (_combo_Box.SelectedItem != null)
                {
                    var subcats = _Metadata.Categories.FirstOrDefault(o => o.Name == _combo_Box.SelectedItem.ToString());
                    comboBox_SubCategory.DataSource = null;
                    comboBox_SubCategory.Items.Clear();
                    //if (!hasClickedAddAssets)
                    //{
                    //    if (subcats.SubCategories.Count > 0)
                    //    {
                    //        foreach (var subcat in subcats.SubCategories)
                    //            comboBox_SubCategory.Items.Add(subcat.Name);
                    //        comboBox_SubCategory.SelectedIndex = 0;
                    //    }
                    //}
                    //else
                    //{
                    if (subcats.SubCategories.Count > 0)
                    {
                        foreach (var subcat in subcats.SubCategories)
                        {
                            comboBox_SubCategory.Items.Add(subcat.Name);
                            if (subcat.SubCategories.Count > 0)
                            {
                                foreach (var subcatsubj1 in subcat.SubCategories)
                                {
                                    comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel1Intent + subcatsubj1.Name);
                                    if (subcatsubj1.SubCategories.Count > 0)
                                    {
                                        foreach (var subcatsubj2 in subcatsubj1.SubCategories)
                                        {
                                            comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel2Intent + subcatsubj2.Name);
                                        }
                                    }
                                }
                            }
                        }
                        comboBox_SubCategory.SelectedIndex = 0;
                    }
                    //}
                }
            }
            catch (Exception ex) { }
        }

        private void button_RemoveItem_Click(object sender, EventArgs e)
        {
            try
            {
                var confirmResult = MessageBox.Show(Properties.Resources.MsgConfirmDelete, Properties.Resources.TitleConfirmDelete, MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    var tagSplits = treeView1.SelectedNode.Tag.ToString().Split(',');
                    switch (tagSplits[0])
                    {
                        case "playlist":
                            fnRemovePlaylistItem(tagSplits);
                            break;
                        case "asset":
                            fnRemoveAssetItem(tagSplits);
                            break;
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void fnRemovePlaylistItem(string[] tagSplits)
        {
            try
            {
                PlaylistsModel playlist = _PlaylistList.FirstOrDefault(o => o.Id == tagSplits[1]);
                if (playlist != null)
                {
                    _PlaylistList.Remove(playlist);
                    foreach (string assettt in playlist.Assets)
                    {
                        var playlistsWithaAssettt = _PlaylistList.Where(o => o.Assets.Contains(assettt)).ToList();
                        if (playlistsWithaAssettt.Count == 0)
                        {
                            var corresAssetObj = _AssetsList.FirstOrDefault(o => o.Id == assettt);
                            _AssetsList.Remove(corresAssetObj);
                        }
                    }
                    string removingPlaylistName = playlist.Title;                    
                    treeView1.SelectedNode.Nodes.Clear();
                    treeView1.Nodes.Remove(treeView1.SelectedNode);
                    _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                    _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
                    MessageBox.Show(String.Format(Properties.Resources.MsgPlaylistDeletionSuccessfull, removingPlaylistName), Properties.Resources.TitleSuccess);
                }
            }
            catch { }
        }

        private void fnRemoveAssetItem(string[] tagSplits)
        {
            try
            {
                AssetsModel asset = _AssetsList.FirstOrDefault(o => o.Id == tagSplits[1]);
                string removingAssetName = asset.Title;
                if (asset != null)
                {
                    var playlistsWithaAsset = _PlaylistList.Where(o => o.Assets.Contains(tagSplits[1])).ToList();
                    foreach (var plays in playlistsWithaAsset) { plays.Assets.Remove(tagSplits[1]); }
                    _AssetsList.Remove(asset);
                    treeView1.Nodes.Remove(treeView1.SelectedNode);
                    _JSONOperations.fnSaveAssetLocalRepoFile(_AssetsList);
                    _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
                    MessageBox.Show(String.Format(Properties.Resources.MsgAssetDeletionSuccessfull, removingAssetName), Properties.Resources.TitleSuccess);
                }
            }
            catch { }
        }

        private void button_EditProperties_Click(object sender, EventArgs e)
        {
            propertyEditPanel.Enabled = true;
            panelEditMetadata.Enabled = true;
            button_MoveAssetUp.Enabled = false;
            button_MoveAssetDown.Enabled = false;
        }
                
        private void button_Save_Prop_Click_1(object sender, EventArgs e)
        {
            fnUpdatePlaylist();
        }

        private void button_Cancel_Prop_Click_1(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(Properties.Resources.MsgConfirmCancelClick, Properties.Resources.TitleConfirmCancelClick, MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                _IsNewPlaylistAssetItem = false;
                propertyEditPanel.Enabled = false;
                _SelectedPlaylist = null;
                _SelectedAsset = null;
                button_MoveAssetUp.Enabled = true;
                button_MoveAssetDown.Enabled = true;
            }
        }               

        private void btnResetRepo_Click(object sender, EventArgs e)
        {
            bool blnStatus = _JSONOperations.fnLoadModelsFromJSON(ref _PlaylistList, ref _AssetsList, ref _Metadata);
            if (blnStatus)
            {
                //>> Disable Screen
                tableLayoutPanelMain.Enabled = true;
                btnPullFiles.Enabled = true;
                btnPushFiles.Enabled = true;
                btnEditMetadata.Enabled = true;
                btnEditPlaylist.Enabled = true;
                button_MoveAssetDown.Enabled = true;
                button_MoveAssetUp.Enabled = true;
                button_PublishtoProduction.Enabled = true;
                btnRefresh.Visible = false;

                //fnLoadPlaylistTree();
                fnFillFilterbyCategoryCombobox();
                fnFillFilterbyTechnologyCombobox();
                fnLoadMetadataTree();
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void comboBox_FilterByCate_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedCategoryItemId = (comboBox_FilterByCate.SelectedItem == null ? String.Empty : ((KeyValuePair<string, string>)comboBox_FilterByCate.SelectedItem).Key);
            string strSelectedCategoryItemValue = (comboBox_FilterByCate.SelectedItem == null ? String.Empty : ((KeyValuePair<string, string>)comboBox_FilterByCate.SelectedItem).Value);
            string strSelectedTechnologyItem = (comboBox_FilterByTech.SelectedItem == null ? String.Empty : comboBox_FilterByTech.SelectedItem.ToString());
            fnLoadPlaylistTree(fnFilterPlaylistbyCategoryandTechnology(strSelectedCategoryItemId, strSelectedCategoryItemValue, strSelectedTechnologyItem));
        }

        private void comboBox_FilterByTech_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedCategoryItemId = (comboBox_FilterByCate.SelectedItem == null ? String.Empty : ((KeyValuePair<string, string>)comboBox_FilterByCate.SelectedItem).Key);
            string strSelectedCategoryItemValue = (comboBox_FilterByCate.SelectedItem == null ? String.Empty : ((KeyValuePair<string, string>)comboBox_FilterByCate.SelectedItem).Value);
            string strSelectedTechnologyItem = (comboBox_FilterByTech.SelectedItem == null ? String.Empty : comboBox_FilterByTech.SelectedItem.ToString());
            fnLoadPlaylistTree(fnFilterPlaylistbyCategoryandTechnology(strSelectedCategoryItemId, strSelectedCategoryItemValue, strSelectedTechnologyItem));
        }

        private void button_ClearCatFilter_Click(object sender, EventArgs e)
        {
            comboBox_FilterByCate.SelectedIndex = 0;
        }

        private void button_ClearTechFilter_Click(object sender, EventArgs e)
        {
            comboBox_FilterByTech.SelectedIndex = 0;
        }

        private void button_MoveAssetUp_Click(object sender, EventArgs e)
        {
            fnMoveAssetNodeUp();
        }

        private void button_MoveAssetDown_Click(object sender, EventArgs e)
        {
            fnMoveAssetNodeDown();
        }

        #endregion

        #region Methods

        private void fnLoadPlaylistTree(List<PlaylistsModel> playlistModelObj)
        {
            string catchExce = string.Empty;
            try
            {
                treeView1.Nodes.Clear();
                foreach (PlaylistsModel playModel in playlistModelObj)
                {
                    catchExce = playModel.Title;
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.Text = playModel.Title;
                    trvRoot.Tag = "playlist," + playModel.Id;
                    trvRoot.ImageIndex = 0;
                    treeView1.Nodes.Add(trvRoot);
                    if (playModel.Assets != null)
                    {
                        foreach (string assets in playModel.Assets)
                        {
                            catchExce = assets;
                            var assetName = _AssetsList.FirstOrDefault(o => o.Id == assets);
                            TreeNode assetNode = new TreeNode();
                            assetNode.Text = assetName.Title;
                            assetNode.Tag = "asset," + assets;
                            assetNode.ImageIndex = 1;
                            trvRoot.Nodes.Add(assetNode);
                        }
                    }
                }
                treeView1.SelectedNode = treeView1.Nodes[0];
            }
            catch (Exception ex)
            { }
        }

        private void fnLoadMetadataTree()
        {
            try
            {
                treeView2.Nodes.Clear();

                TreeNode trvRootTech = new TreeNode();
                trvRootTech.Text = "Technologies";
                trvRootTech.Tag = "tech,";
                treeView2.Nodes.Add(trvRootTech);

                foreach (var techModel in _Metadata.Technologies)
                {
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.Text = techModel.Name;
                    trvRoot.Tag = "tech," + techModel.Name;
                    trvRootTech.Nodes.Add(trvRoot);
                    if (techModel.Subjects != null)
                    {
                        foreach (string subj in techModel.Subjects)
                        {
                            TreeNode subjNode = new TreeNode();
                            subjNode.Text = subj;
                            subjNode.Tag = "subj," + subj;
                            trvRoot.Nodes.Add(subjNode);
                        }
                    }
                }

                TreeNode trvRootCateg = new TreeNode();
                trvRootCateg.Text = "Categories";
                trvRootCateg.Tag = "cate,";
                treeView2.Nodes.Add(trvRootCateg);

                foreach (var categModel in _Metadata.Categories)
                {
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.Text = categModel.Name;
                    trvRoot.Tag = "cate," + categModel.Name;
                    trvRootCateg.Nodes.Add(trvRoot);
                    if (categModel.SubCategories != null)
                    {
                        foreach (var subcat in categModel.SubCategories)
                        {
                            TreeNode subNode = new TreeNode();
                            subNode.Text = subcat.Name;
                            subNode.Tag = "subc," + subcat.Name;
                            trvRoot.Nodes.Add(subNode);
                            foreach (var subcatsubj1 in subcat.SubCategories)
                            {
                                TreeNode subNode1 = new TreeNode();
                                subNode1.Text = subcatsubj1.Name;
                                subNode1.Tag = "subcsubj1," + subcatsubj1.Name;
                                subNode.Nodes.Add(subNode1);
                                foreach (var subcatsubj2 in subcatsubj1.SubCategories)
                                {
                                    TreeNode subNode2 = new TreeNode();
                                    subNode2.Text = subcatsubj2.Name;
                                    subNode2.Tag = "subcsubj2," + subcatsubj2.Name;
                                    subNode1.Nodes.Add(subNode2);
                                }
                            }
                        }
                    }
                }

                TreeNode trvRootAud = new TreeNode();
                trvRootAud.Text = "Role";
                trvRootAud.Tag = "audi,";
                treeView2.Nodes.Add(trvRootAud);
                foreach (var audModel in _Metadata.Audiences)
                {
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.Text = audModel.Name;
                    trvRoot.Tag = "audi," + audModel.Name;
                    trvRootAud.Nodes.Add(trvRoot);
                }

                TreeNode trvRootSou = new TreeNode();
                trvRootSou.Text = "Sources";
                trvRootSou.Tag = "sour,";
                treeView2.Nodes.Add(trvRootSou);
                foreach (var souModel in _Metadata.Sources)
                {
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.Text = souModel;
                    trvRoot.Tag = "sour," + souModel;
                    trvRootSou.Nodes.Add(trvRoot);
                }

                TreeNode trvRootLevel = new TreeNode();
                trvRootLevel.Text = "Levels";
                trvRootLevel.Tag = "leve,";
                treeView2.Nodes.Add(trvRootLevel);
                foreach (var level in _Metadata.Levels)
                {
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.Text = level;
                    trvRoot.Tag = "leve," + level;
                    trvRootLevel.Nodes.Add(trvRoot);
                }

                TreeNode trvRootStatusTag = new TreeNode();
                trvRootStatusTag.Text = "Status Tag";
                trvRootStatusTag.Tag = "stat,";
                treeView2.Nodes.Add(trvRootStatusTag);
                foreach (var statTag in _Metadata.StatusTag)
                {
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.Text = statTag;
                    trvRoot.Tag = "stat," + statTag;
                    trvRootStatusTag.Nodes.Add(trvRoot);
                }

                treeView2.SelectedNode = treeView2.Nodes[0];
            }
            catch (Exception ex)
            {

            }
        }

        private void fnEnableScreen()
        {
            tableLayoutPanelMain.Enabled = false;
            btnPullFiles.Enabled = false;
            btnPushFiles.Enabled = false;
            btnEditMetadata.Enabled = false;
            btnEditPlaylist.Enabled = false;
            button_MoveAssetDown.Enabled = false;
            button_MoveAssetUp.Enabled = false;
            button_PublishtoProduction.Enabled = false;
            button_MoveAssetUp.Visible = false;
            btnRefresh.Visible = true;
        }

        private void fnLoadImage(string repoImagePath, bool isMeta)
        {
            try
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = false;
                dialog.Filters.Add(new CommonFileDialogFilter("All Image Files", "*.png;*.jpeg;*.gif;*.jpg;*.bmp;*.tiff;*.tif"));
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var fileName = dialog.FileName;
                    using (Bitmap img = new Bitmap(dialog.FileName))
                    {
                        var imageSize = GetSetConfig.imageSizeLimit.Split(',');
                        if (img.Width <= Convert.ToInt16(imageSize[0]) && img.Height <= Convert.ToInt16(imageSize[1]))
                        {
                            if (isMeta)
                            {
                                textBox_Meta_Image.Text = repoImagePath + Path.GetFileName(fileName);
                                pictureBox_Metadata.ImageLocation = dialog.FileName;
                            }
                            else
                            {
                                textBox_Image.Text = repoImagePath + Path.GetFileName(fileName);
                                pictureBox_Playlist.ImageLocation = dialog.FileName;
                            }
                            File.Copy(fileName, Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, repoImagePath, Path.GetFileName(fileName)), true);
                            File.Copy(fileName, Path.Combine(_SessionData.ProductionLocalRepositoryPath, GetSetConfig.gitFolderName, repoImagePath, Path.GetFileName(fileName)), true);
                        }
                        else { MessageBox.Show(Properties.Resources.MsgImageFileSizeExceeds, Properties.Resources.TitleError); }
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void fnLoadSelectedTreeNode(string type, string id)
        {
            try
            {
                switch (type)
                {
                    case "playlist":
                        fnLoadSelectedPlaylist(type, id);
                        break;
                    case "asset":
                        fnLoadSelectedAsset(type, id);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex) { }
        }

        private void fnLoadSelectedPlaylist(string type, string id)
        {
            PlaylistsModel playlist = null;
            try
            {
                fnResetEditFormForPlaylist();
                //>> ID, Title and Image
                textBox_Id.Text = ""; textBox_Title.Text = ""; textBox_CommonDesc.Text = ""; textBox_Image.Text = "";
                if (!string.IsNullOrEmpty(id))
                {
                    playlist = _PlaylistList.FirstOrDefault(o => o.Id == id);
                    label_PlaylistHeading.Text = "Playlist: " + playlist.Title;
                    textBox_Id.Text = playlist.Id;
                    textBox_Title.Text = playlist.Title;
                    textBox_Image.Text = playlist.Image;
                    textBox_CommonDesc.Text = playlist.Description;
                }
                else
                {
                    label_PlaylistHeading.Text = "Playlist";
                    textBox_Id.Text = Guid.NewGuid().ToString();
                }
                //>> Level
                comboBox_Level.DataSource = _Metadata.Levels;
                if (playlist != null && !string.IsNullOrEmpty(playlist.Level)) { comboBox_Level.SelectedItem = playlist.Level; }
                else { comboBox_Level.SelectedIndex = 0; }
                //>> Audience
                foreach (var aud in _Metadata.Audiences) { comboBox_Audience.Items.Add(aud.Name); }
                if (playlist != null && !string.IsNullOrEmpty(playlist.Audience)) { comboBox_Audience.SelectedItem = playlist.Audience; }
                else { comboBox_Audience.SelectedIndex = 0; }
                //>> Technologies
                foreach (var tec in _Metadata.Technologies) { comboBox_Technology.Items.Add(tec.Name); }
                if (playlist != null && !string.IsNullOrEmpty(playlist.Technology)) { comboBox_Technology.SelectedItem = playlist.Technology; }
                else { comboBox_Technology.SelectedIndex = 0; }
                //>> Subject
                comboBox_Subject.DataSource = null;
                comboBox_Subject.Items.Clear();
                comboBox_Subject.Items.Add(Properties.Resources.StrSelectAnyComboOption);
                if (playlist != null)
                {
                    var techno = _Metadata.Technologies.FirstOrDefault(o => o.Name == playlist.Technology);                    
                    if (techno.Subjects.Count > 0)
                    {
                        foreach (string subject in techno.Subjects) { comboBox_Subject.Items.Add(subject); }
                        if (!string.IsNullOrEmpty(playlist.Subject)) { comboBox_Subject.SelectedItem = playlist.Subject; }
                        else { comboBox_Subject.SelectedIndex = 0; }
                    }
                    else { comboBox_Subject.SelectedIndex = 0; }
                }
                else
                {
                    var techno = _Metadata.Technologies[0];
                    if (techno.Subjects.Count > 0)
                    {
                        foreach (string subject in techno.Subjects) { comboBox_Subject.Items.Add(subject); }
                        comboBox_Subject.SelectedIndex = 0;
                    }
                    else { comboBox_Subject.SelectedIndex = 0; }
                }
                //>> Categories
                foreach (var cat in _Metadata.Categories) { comboBox_Category.Items.Add(cat.Name); }
                if (playlist != null && !string.IsNullOrEmpty(playlist.Category)) { comboBox_Category.SelectedItem = playlist.Category; }
                else { comboBox_Category.SelectedIndex = 0; }
                //>> SubCategories
                comboBox_SubCategory.DataSource = null;
                comboBox_SubCategory.Items.Clear();
                if (playlist != null)
                {
                    var subcats1 = _Metadata.Categories.FirstOrDefault(o => o.Name == playlist.Category);
                    string catIdName = string.Empty;
                    string selectableCatIdName = string.Empty;
                    if (subcats1.SubCategories.Count > 0)
                    {
                        foreach (var subcat in subcats1.SubCategories)
                        {
                            comboBox_SubCategory.Items.Add(subcat.Name);
                            if (subcat.Id == playlist.CatId)
                            {
                                catIdName = subcat.Name;
                                selectableCatIdName = catIdName;
                            }
                            if (subcat.SubCategories.Count > 0)
                            {
                                foreach (var subcatsubj1 in subcat.SubCategories)
                                {
                                    comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel1Intent + subcatsubj1.Name);
                                    if (subcatsubj1.Id == playlist.CatId)
                                    {
                                        catIdName = subcatsubj1.Name;
                                        selectableCatIdName = Properties.Resources.StrSubjectLevel1Intent + catIdName;
                                    }
                                    if (subcatsubj1.SubCategories.Count > 0)
                                    {
                                        foreach (var subcatsubj2 in subcatsubj1.SubCategories)
                                        {
                                            comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel2Intent + subcatsubj2.Name);
                                            if (subcatsubj2.Id == playlist.CatId)
                                            {
                                                catIdName = subcatsubj2.Name;
                                                selectableCatIdName = Properties.Resources.StrSubjectLevel2Intent + catIdName;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(playlist.SubCategory))
                        {
                            comboBox_SubCategory.SelectedItem = selectableCatIdName;
                        }
                    }
                }
                else
                {
                    var subcats1 = _Metadata.Categories[0];
                    if (subcats1.SubCategories.Count > 0)
                    {
                        if (subcats1.SubCategories.Count > 0)
                        {
                            foreach (var subcat in subcats1.SubCategories)
                            {
                                comboBox_SubCategory.Items.Add(subcat.Name);
                                if (subcat.SubCategories.Count > 0)
                                {
                                    foreach (var subcatsubj1 in subcat.SubCategories)
                                    {
                                        comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel1Intent + subcatsubj1.Name);
                                        if (subcatsubj1.SubCategories.Count > 0)
                                        {
                                            foreach (var subcatsubj2 in subcatsubj1.SubCategories)
                                            {
                                                comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel2Intent + subcatsubj2.Name);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        comboBox_SubCategory.SelectedIndex = 0;
                    }
                }
                //>> Source
                comboBox_Source.DataSource = _Metadata.Sources;
                if (playlist != null && !string.IsNullOrEmpty(playlist.Source)) { comboBox_Source.SelectedItem = playlist.Source; }
                else { comboBox_Source.SelectedIndex = 0; }
                //>> Image Preview
                try { pictureBox_Playlist.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, playlist.Image); } catch { }
                //>> Status Tag
                comboBox_StatusTag.Items.Clear();
                comboBox_StatusTag.Items.Add(Properties.Resources.StrSelectAnyComboOption);
                foreach(string tagName in _Metadata.StatusTag) { comboBox_StatusTag.Items.Add(tagName); }
                if (playlist != null && !string.IsNullOrEmpty(playlist.StatusTag)) { comboBox_StatusTag.SelectedItem = playlist.StatusTag; }
                else { comboBox_StatusTag.SelectedIndex = 0; }
                if (_IsNewPlaylistAssetItem) { comboBox_StatusTag.SelectedIndex = 1; }
            }
            catch (Exception ex) { }
        }

        private void fnResetEditFormForPlaylist()
        {
            //label_CommonDesc.Text = "Image";
            
            label_Assets.Visible = false;
            textBox_Url.Visible = false;
            label_Image.Visible = true;
            textBox_Image.Visible = true;
            label_ImageLimits.Visible = true;
            //label_StatusTag.Visible = false;
            //comboBox_StatusTag.Visible = false;
            label_MediaId.Visible = false;
            textBox_MediaId.Visible = false;
            button_Save_Prop.Visible = true;
            button_SaveAsset.Visible = false;
            
            pictureBox_Playlist.Visible = true;
            button_BrowseImage.Visible = true;

            button_MoveAssetDown.Visible = false;
            button_MoveAssetUp.Visible = false;

            hasClickedAddAssets = false;

            fnResetComboBoxes();
        }

        private void fnResetEditFormForAsset()
        {
            //label_CommonDesc.Text = "Description";
            label_Assets.Visible = true;
            textBox_Url.Visible = true;
            label_Image.Visible = false;
            textBox_Image.Visible = false;
            label_ImageLimits.Visible = false;
            //label_StatusTag.Visible = true;
            //comboBox_StatusTag.Visible = true;
            label_MediaId.Visible = true;
            textBox_MediaId.Visible = true;
            button_Save_Prop.Visible = false;
            button_SaveAsset.Visible = true;
            pictureBox_Playlist.Visible = false;
            button_BrowseImage.Visible = false;

            button_MoveAssetDown.Visible = true;
            button_MoveAssetUp.Visible = true;

            fnResetComboBoxes();
        }

        private void fnResetComboBoxes()
        {
            comboBox_Level.DataSource = null;
            comboBox_Audience.DataSource = null;
            comboBox_Technology.DataSource = null;
            comboBox_Subject.DataSource = null;
            comboBox_Category.DataSource = null;
            comboBox_SubCategory.DataSource = null;
            comboBox_Source.DataSource = null;

            comboBox_Level.Items.Clear();
            comboBox_Audience.Items.Clear();
            comboBox_Technology.Items.Clear();
            comboBox_Subject.Items.Clear();
            comboBox_Category.Items.Clear();
            comboBox_SubCategory.Items.Clear();
            comboBox_Source.Items.Clear();
        }

        private void fnLoadSelectedAsset(string type, string id)
        {
            AssetsModel asset = null;
            PlaylistsModel currPlaylitsItem = null;

            try
            {
                fnResetEditFormForAsset();

                if (_SelectedItemType == "playlist")
                {
                    currPlaylitsItem = _PlaylistList.FirstOrDefault(o => o.Id == _SelectedItemId);
                }
                else
                {
                    string idofPlaylist = treeView1.SelectedNode.Parent.Tag.ToString().Split(',')[1];
                    currPlaylitsItem = _PlaylistList.FirstOrDefault(o => o.Id == idofPlaylist);
                }

                //>> ID, Title and Image
                textBox_Id.Text = ""; textBox_Title.Text = ""; textBox_CommonDesc.Text = ""; textBox_MediaId.Text = "";
                if (!string.IsNullOrEmpty(id))
                {
                    asset = _AssetsList.FirstOrDefault(o => o.Id == id);
                    label_PlaylistHeading.Text = "Asset: " + asset.Title;
                    textBox_Id.Text = asset.Id;
                    textBox_Title.Text = asset.Title;
                    textBox_CommonDesc.Text = asset.Description;
                    textBox_MediaId.Text = asset.MediaId;
                }
                else
                {
                    label_PlaylistHeading.Text = "Asset";
                    textBox_Id.Text = Guid.NewGuid().ToString();
                }

                //>> Level
                comboBox_Level.DataSource = _Metadata.Levels;
                if (asset != null && !string.IsNullOrEmpty(asset.Level)) { comboBox_Level.SelectedItem = asset.Level; }
                else { comboBox_Level.SelectedItem = currPlaylitsItem.Level; }
                //>> Audiences
                foreach (var aud in _Metadata.Audiences) { comboBox_Audience.Items.Add(aud.Name); }
                if (asset != null && !string.IsNullOrEmpty(asset.Audience)) { comboBox_Audience.SelectedItem = asset.Audience; }
                else { comboBox_Audience.SelectedItem = currPlaylitsItem.Audience; }
                //>> Technologies
                foreach (var tec in _Metadata.Technologies) { comboBox_Technology.Items.Add(tec.Name); }
                if (asset != null && !string.IsNullOrEmpty(asset.Technology)) { comboBox_Technology.SelectedItem = asset.Technology; }
                else { comboBox_Technology.SelectedItem = currPlaylitsItem.Technology; }
                //> Subject
                comboBox_Subject.DataSource = null;
                comboBox_Subject.Items.Clear();
                comboBox_Subject.Items.Add(Properties.Resources.StrSelectAnyComboOption);
                if (asset != null)
                {
                    var techno1 = _Metadata.Technologies.FirstOrDefault(o => o.Name == asset.Technology);
                    if (techno1.Subjects.Count > 0)
                    {
                        foreach (string subject in techno1.Subjects) { comboBox_Subject.Items.Add(subject); }
                        if (!string.IsNullOrEmpty(asset.Subject)) { comboBox_Subject.SelectedItem = asset.Subject; }
                        else { comboBox_Subject.SelectedIndex = 0; }
                    }
                    else { comboBox_Subject.SelectedIndex = 0; }
                }
                else
                {
                    var techno1 = _Metadata.Technologies[0];
                    if (techno1.Subjects.Count > 0)
                    {
                        foreach (string subject in techno1.Subjects) { comboBox_Subject.Items.Add(subject); }
                        comboBox_Subject.SelectedItem = currPlaylitsItem.Subject;
                    }
                    else { comboBox_Subject.SelectedIndex = 0; }
                }
                //>> Categories
                foreach (var cat in _Metadata.Categories) { comboBox_Category.Items.Add(cat.Name); }
                if (asset != null && !string.IsNullOrEmpty(asset.Category)) { comboBox_Category.SelectedItem = asset.Category; }
                else { comboBox_Category.SelectedItem = currPlaylitsItem.Category; }
                //>> SubCategories
                comboBox_SubCategory.DataSource = null;
                comboBox_SubCategory.Items.Clear();
                if (asset != null)
                {
                    var subcats1 = _Metadata.Categories.FirstOrDefault(o => o.Name == asset.Category);
                    string catIdName = string.Empty;
                    string selectableCatIdName = string.Empty;
                    if (subcats1.SubCategories.Count > 0)
                    {
                        foreach (var subcat in subcats1.SubCategories)
                        {
                            comboBox_SubCategory.Items.Add(subcat.Name);
                            if (subcat.Id == asset.CatId)
                            {
                                catIdName = subcat.Name;
                                selectableCatIdName = catIdName;
                            }
                            if (subcat.SubCategories.Count > 0)
                            {
                                foreach (var subcatsubj1 in subcat.SubCategories)
                                {
                                    comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel1Intent + subcatsubj1.Name);
                                    if (subcatsubj1.Id == asset.CatId)
                                    {
                                        catIdName = subcatsubj1.Name;
                                        selectableCatIdName = Properties.Resources.StrSubjectLevel1Intent + catIdName;
                                    }
                                    if (subcatsubj1.SubCategories.Count > 0)
                                    {
                                        foreach (var subcatsubj2 in subcatsubj1.SubCategories)
                                        {
                                            comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel2Intent + subcatsubj2.Name);
                                            if (subcatsubj2.Id == asset.CatId)
                                            {
                                                catIdName = subcatsubj2.Name;
                                                selectableCatIdName = Properties.Resources.StrSubjectLevel2Intent + catIdName;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(asset.SubCategory))
                        {
                            comboBox_SubCategory.SelectedItem = selectableCatIdName;
                        }
                    }
                }
                else
                {
                    var subcats1 = _Metadata.Categories.FirstOrDefault(o => o.Name == currPlaylitsItem.Category); ;
                    if (subcats1.SubCategories.Count > 0)
                    {
                        if (subcats1.SubCategories.Count > 0)
                        {
                            foreach (var subcat in subcats1.SubCategories)
                            {
                                comboBox_SubCategory.Items.Add(subcat.Name);
                                if (subcat.SubCategories.Count > 0)
                                {
                                    foreach (var subcatsubj1 in subcat.SubCategories)
                                    {
                                        comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel1Intent + subcatsubj1.Name);
                                        if (subcatsubj1.SubCategories.Count > 0)
                                        {
                                            foreach (var subcatsubj2 in subcatsubj1.SubCategories)
                                            {
                                                comboBox_SubCategory.Items.Add(Properties.Resources.StrSubjectLevel2Intent + subcatsubj2.Name);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        comboBox_SubCategory.SelectedItem = currPlaylitsItem.SubCategory;
                    }
                }
                //>> Source
                comboBox_Source.DataSource = _Metadata.Sources;
                if (asset != null && !string.IsNullOrEmpty(asset.Source)) { comboBox_Source.SelectedItem = asset.Source; }
                else { comboBox_Source.SelectedItem = currPlaylitsItem.Source; }
                //>> Status Tag
                comboBox_StatusTag.Items.Clear();
                comboBox_StatusTag.Items.Add(Properties.Resources.StrSelectAnyComboOption);
                foreach (string tagName in _Metadata.StatusTag) { comboBox_StatusTag.Items.Add(tagName); }
                if (asset != null && !string.IsNullOrEmpty(asset.StatusTag)) { comboBox_StatusTag.SelectedItem = asset.StatusTag; }
                else { if (currPlaylitsItem != null && !string.IsNullOrEmpty(currPlaylitsItem.StatusTag)) { comboBox_StatusTag.SelectedItem = currPlaylitsItem.StatusTag; } else { comboBox_StatusTag.SelectedIndex = 0; } }
                if (_IsNewPlaylistAssetItem) { comboBox_StatusTag.SelectedIndex = 1; }
                //>> URL
                if (asset != null) { textBox_Url.Text = asset.Url; }
                else { textBox_Url.Text = ""; }
            }
            catch (Exception ex) { }
        }

        private void fnManageMetadataSelction()
        {
            try
            {
                panelEditMetadata.Enabled = false;
                _IsNewPlaylistAssetItem = false;
                if (!string.IsNullOrEmpty(treeView2.SelectedNode.Tag.ToString()))
                {
                    string[] tagSplit = treeView2.SelectedNode.Tag.ToString().Split(',');
                    _SelectedMetaItemType = tagSplit[0];
                    _SelectedMetaItemValue = tagSplit[1];
                    if (!string.IsNullOrEmpty(tagSplit[1])) { fnLoadSelectedMetadata(tagSplit); }
                    else
                    {
                        panelEditMetadata.Visible = false;
                        button_EditMetadata.Visible = false;
                        switch (tagSplit[0])
                        {
                            case "tech":
                                if (string.IsNullOrEmpty(tagSplit[1]))
                                {
                                    button_AddMeta.Text = "        Add Technology";
                                    _SelectedTechnology = null;
                                }
                                else
                                {
                                    button_AddMeta.Text = "        Add Subject";
                                    _SelectedSubject = null;
                                }
                                
                                break;

                            case "subj":
                                _SelectedSubject = null;
                                button_AddMeta.Text = "        Add Subject";
                                break;

                            case "cate":
                                _SelectedCategory = null;
                                if (string.IsNullOrEmpty(tagSplit[1]))
                                {
                                    button_AddMeta.Text = "        Add Category";
                                }
                                else
                                {
                                    button_AddMeta.Text = "     Add Subcategory";
                                    _SelectedSubCategory = null;
                                }
                                break;

                            case "subc":
                                _SelectedSubCategory = null;
                                button_AddMeta.Text = "     Add Subcategory";
                                break;

                            case "audi":
                                _SelectedAudience = null;
                                button_AddMeta.Text = "        Add Role";
                                break;

                            case "sour":
                                _SelectedSource = null;
                                button_AddMeta.Text = "        Add Source";
                                break;

                            case "leve":
                                _SelectedLevel = null;
                                button_AddMeta.Text = "        Add Level";
                                break;

                            case "subcsubj1":
                                _SelectedSubCategorySubjectLevel1 = null;
                                button_AddMeta.Text = "     Add Subcategory";
                                break;

                            case "subcsubj2":
                                _SelectedSubCategorySubjectLevel2 = null;
                                button_AddMeta.Text = "     Add Subcategory";
                                break;

                            case "stat":
                                _SelectedStatusTag = null;
                                button_AddMeta.Text = "      Add Status Tag";
                                break;
                        }
                    }
                }
                else { panelEditMetadata.Visible = false; }
               
            }
            catch (Exception ex) { }
        }

        private void fnLoadSelectedMetadata(string[] tagSplit)
        {
            try
            {
                panelEditMetadata.Visible = true;
                button_EditMetadata.Visible = true;
                comboBox_MetaTechnology.Items.Clear();
                var metaNames = treeView2.SelectedNode.Tag.ToString().Split(new[] { ',' }, 2);
                switch (tagSplit[0])
                {
                    case "tech":
                        if(string.IsNullOrEmpty(metaNames[1]))
                            button_AddMeta.Text = "        Add Technology";
                        else
                            button_AddMeta.Text = "        Add Subject";
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        var metaTechObj = _Metadata.Technologies.FirstOrDefault(o => o.Name == metaNames[1]);
                        _SelectedTechnology = metaTechObj;                        
                        label_Meta_Heading.Text = "Technology: " + metaTechObj.Name;
                        textBox_Meta_Name.Text = metaTechObj.Name;
                        textBox_Meta_Image.Text = metaTechObj.Image;
                        try
                        { if (!string.IsNullOrEmpty(metaTechObj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaTechObj.Image); } }
                        catch { }
                        break;
                    case "subj":
                        button_AddMeta.Text = "        Add Subject";
                        fnDisableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        label_Meta_Heading.Text = "Subject: " + metaNames[1];
                        _SelectedSubject = metaNames[1];
                        textBox_Meta_Name.Text = metaNames[1];
                        break;
                    case "cate":
                        if (string.IsNullOrEmpty(metaNames[1]))
                            button_AddMeta.Text = "        Add Category";
                        else
                            button_AddMeta.Text = "     Add Subcategory";
                        fnEnableMetadataItems();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        comboBox_Meta_Security.Visible = true;
                        var metaCateObj = _Metadata.Categories.FirstOrDefault(o => o.Name == metaNames[1]);
                        _SelectedCategory = metaCateObj;
                        label_Meta_Heading.Text = "Category: " + metaCateObj.Name;
                        textBox_Meta_Name.Text = metaCateObj.Name;
                        textBox_Meta_Image.Text = metaCateObj.Image;
                        fnLoadSecurityComboItems();
                        if (!string.IsNullOrEmpty(metaCateObj.Security)) { comboBox_Meta_Security.SelectedItem = metaCateObj.Security; }
                        else { comboBox_Meta_Security.SelectedIndex = 0; }
                        textBox_MetaId.Text = metaCateObj.Id;
                        comboBox_MetaTechnology.Items.Add(Properties.Resources.StrSelectAnyComboOption);
                        foreach (var tec in _Metadata.Technologies)
                            comboBox_MetaTechnology.Items.Add(tec.Name);

                        if (!string.IsNullOrEmpty(metaCateObj.Technology))
                            comboBox_MetaTechnology.SelectedItem = metaCateObj.Technology;
                        else { comboBox_MetaTechnology.SelectedIndex = 0; }
                        try { if (!string.IsNullOrEmpty(metaCateObj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaCateObj.Image); } }
                        catch { }
                        break;
                    case "subc":
                        button_AddMeta.Text = "     Add Subcategory";
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        comboBox_Meta_Security.Visible = true;
                        string categName = treeView2.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        var metaSubcObj = _Metadata.Categories.FirstOrDefault(o => o.Name == categName).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        _SelectedSubCategory = metaSubcObj;
                        label_Meta_Heading.Text = "Sub Category: " + metaSubcObj.Name;
                        textBox_Meta_Name.Text = metaSubcObj.Name;
                        textBox_Meta_Image.Text = metaSubcObj.Image;
                        textBox_MetaId.Text = metaSubcObj.Id;
                        fnLoadSecurityComboItems();
                        if (!string.IsNullOrEmpty(metaSubcObj.Security)) { comboBox_Meta_Security.SelectedItem = metaSubcObj.Security; }
                        else { comboBox_Meta_Security.SelectedIndex = 0; }
                        comboBox_MetaTechnology.Items.Add(Properties.Resources.StrSelectAnyComboOption);
                        foreach (var tec in _Metadata.Technologies)
                            comboBox_MetaTechnology.Items.Add(tec.Name);

                        if (!string.IsNullOrEmpty(metaSubcObj.Technology))
                            comboBox_MetaTechnology.SelectedItem = metaSubcObj.Technology;
                        else { comboBox_MetaTechnology.SelectedIndex = 0; }
                        try { if (!string.IsNullOrEmpty(metaSubcObj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaSubcObj.Image); } }
                        catch { }
                        break;
                    case "subcsubj1":
                        button_AddMeta.Text = "     Add Subcategory";
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        comboBox_Meta_Security.Visible = true;
                        string categName1 = treeView2.SelectedNode.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        string subcategName1 = treeView2.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        var metaSubcSubj1Obj = _Metadata.Categories.FirstOrDefault(o => o.Name == categName1).SubCategories.FirstOrDefault(o => o.Name == subcategName1).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        _SelectedSubCategorySubjectLevel1 = metaSubcSubj1Obj;
                        textBox_Meta_Name.Text = metaSubcSubj1Obj.Name;
                        fnLoadSecurityComboItems();
                        if (!string.IsNullOrEmpty(metaSubcSubj1Obj.Security)) { comboBox_Meta_Security.SelectedItem = metaSubcSubj1Obj.Security; }
                        else { comboBox_Meta_Security.SelectedIndex = 0; }
                        label_Meta_Heading.Text = "Subject: " + metaSubcSubj1Obj.Name;
                        textBox_Meta_Image.Text = metaSubcSubj1Obj.Image;
                        textBox_MetaId.Text = metaSubcSubj1Obj.Id;
                        comboBox_MetaTechnology.Items.Add(Properties.Resources.StrSelectAnyComboOption);
                        foreach (var tec in _Metadata.Technologies)
                            comboBox_MetaTechnology.Items.Add(tec.Name);

                        if (!string.IsNullOrEmpty(metaSubcSubj1Obj.Technology))
                            comboBox_MetaTechnology.SelectedItem = metaSubcSubj1Obj.Technology;
                        else { comboBox_MetaTechnology.SelectedIndex = 0; }
                        try { if (!string.IsNullOrEmpty(metaSubcSubj1Obj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaSubcSubj1Obj.Image); } }
                        catch { }
                        break;
                    case "subcsubj2":
                        button_AddMeta.Text = "     Add Subcategory";
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        comboBox_Meta_Security.Visible = true;
                        string categName2 = treeView2.SelectedNode.Parent.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        string subcategName2 = treeView2.SelectedNode.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        string subjlevel1Name = treeView2.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        var metaSubcSubj2Obj = _Metadata.Categories.FirstOrDefault(o => o.Name == categName2).SubCategories.FirstOrDefault(o => o.Name == subcategName2).SubCategories.FirstOrDefault(o => o.Name == subjlevel1Name).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        _SelectedSubCategorySubjectLevel2 = metaSubcSubj2Obj;
                        textBox_Meta_Name.Text = metaSubcSubj2Obj.Name;
                        fnLoadSecurityComboItems();
                        if (!string.IsNullOrEmpty(metaSubcSubj2Obj.Security)) { comboBox_Meta_Security.SelectedItem = metaSubcSubj2Obj.Security; }
                        else { comboBox_Meta_Security.SelectedIndex = 0; }
                        label_Meta_Heading.Text = "Subject: " + metaSubcSubj2Obj.Name;
                        textBox_Meta_Image.Text = metaSubcSubj2Obj.Image;
                        textBox_MetaId.Text = metaSubcSubj2Obj.Id;
                        comboBox_MetaTechnology.Items.Add(Properties.Resources.StrSelectAnyComboOption);
                        foreach (var tec in _Metadata.Technologies)
                            comboBox_MetaTechnology.Items.Add(tec.Name);

                        if (!string.IsNullOrEmpty(metaSubcSubj2Obj.Technology))
                            comboBox_MetaTechnology.SelectedItem = metaSubcSubj2Obj.Technology;
                        else { comboBox_MetaTechnology.SelectedIndex = 0; }
                        try { if (!string.IsNullOrEmpty(metaSubcSubj2Obj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaSubcSubj2Obj.Image); } }
                        catch { }
                        break;
                    case "audi":
                        button_AddMeta.Text = "        Add Role";
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        var metaAudiObj = _Metadata.Audiences.FirstOrDefault(o => o.Name == metaNames[1]);
                        _SelectedAudience = metaAudiObj;
                        label_Meta_Heading.Text = "Role: " + metaAudiObj.Name;
                        textBox_Meta_Name.Text = metaAudiObj.Name;
                        textBox_Meta_Image.Text = metaAudiObj.Image;
                        try { if (!string.IsNullOrEmpty(metaAudiObj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaAudiObj.Image); } }
                        catch { }
                        break;
                    case "sour":
                        button_AddMeta.Text = "        Add Source";
                        fnDisableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        label_Meta_Heading.Text = "Source: " + metaNames[1];
                        _SelectedSource = metaNames[1];
                        textBox_Meta_Name.Text = metaNames[1];
                        break;
                    case "leve":
                        button_AddMeta.Text = "        Add Level";
                        fnDisableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        label_Meta_Heading.Text = "Level: " + metaNames[1];
                        _SelectedLevel = metaNames[1];
                        textBox_Meta_Name.Text = metaNames[1];
                        break;
                    case "stat":
                        button_AddMeta.Text = "      Add Status Tag";
                        fnDisableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        label_Meta_Heading.Text = "Status Tag: " + metaNames[1];
                        _SelectedStatusTag = metaNames[1];
                        textBox_Meta_Name.Text = metaNames[1];
                        break;
                }
            }
            catch (Exception ex) { }
        }

        private void fnLoadSecurityComboItems()
        {
            comboBox_Meta_Security.Items.Clear();
            comboBox_Meta_Security.Items.Add(Properties.Resources.StrSelectAnyComboOption);
            comboBox_Meta_Security.Items.Add("Owners");
            comboBox_Meta_Security.Items.Add("Members");
            comboBox_Meta_Security.Items.Add("Visitors");
        }

        private void fnEnableMetadataItems()
        {
            label_Meta_Image.Visible = true;
            textBox_Meta_Image.Visible = true;
            button_Meta_Browse.Visible = true;
            pictureBox_Metadata.Visible = true;
            
        }

        private void fnDisableMetadataItems()
        {
            label_Meta_Image.Visible = false;
            textBox_Meta_Image.Visible = false;
            button_Meta_Browse.Visible = false;
            pictureBox_Metadata.Visible = false;
        }

        private void fnDisableSecurityItem()
        {
            label_Meta_Security.Visible = false;
            comboBox_Meta_Security.Visible = false;
        }

        private void fnEnableCategoryItems()
        {
            label_MetaId.Visible = true;
            textBox_MetaId.Visible = true;
            label_MetaTechnology.Visible = true;
            comboBox_MetaTechnology.Visible = true;
        }

        private void fnDisableCategoryItems()
        {
            label_MetaId.Visible = false;
            textBox_MetaId.Visible = false;
            label_MetaTechnology.Visible = false;
            comboBox_MetaTechnology.Visible = false;
        }

        private void fnFillFilterbyCategoryCombobox()
        {
            comboBox_FilterByCate.DataSource = null;
            comboBox_FilterByCate.Items.Clear();
            Dictionary<string, string> comboItems = new Dictionary<string, string>();
            comboItems.Add("", Properties.Resources.StrSelectAnyComboOption);
            //comboBox_FilterByCate.Items.Add(Properties.Resources.StrSelectAnyComboOption);
            foreach (var cat in _Metadata.Categories) {
                //comboBox_FilterByCate.Items.Add(cat.Name);
                comboItems.Add(cat.Id, cat.Name);
                if (cat.SubCategories.Count > 0)
                {
                    foreach (var subcat in cat.SubCategories)
                    {
                        //comboBox_FilterByCate.Items.Add(Properties.Resources.StrSubjectLevel1Intent + subcat.Name);
                        comboItems.Add(subcat.Id, Properties.Resources.StrSubjectLevel1Intent + subcat.Name);
                        if (subcat.SubCategories.Count > 0)
                        {
                            foreach (var subcatsubj1 in subcat.SubCategories)
                            {
                                //comboBox_FilterByCate.Items.Add(Properties.Resources.StrSubjectLevel2Intent + subcatsubj1.Name);
                                comboItems.Add(subcatsubj1.Id, Properties.Resources.StrSubjectLevel2Intent + subcatsubj1.Name);
                                if (subcatsubj1.SubCategories.Count > 0)
                                {
                                    foreach (var subcatsubj2 in subcatsubj1.SubCategories)
                                        //comboBox_FilterByCate.Items.Add(Properties.Resources.StrSubjectLevel3Intent + subcatsubj2.Name);
                                        comboItems.Add(subcatsubj2.Id, Properties.Resources.StrSubjectLevel3Intent + subcatsubj2.Name);
                                }
                            }
                        }
                    }                    
                }
            }
            comboBox_FilterByCate.DataSource = new BindingSource(comboItems, null);
            comboBox_FilterByCate.DisplayMember = "Value";
            comboBox_FilterByCate.ValueMember = "Key";
            comboBox_FilterByCate.SelectedIndex = 0;
        }

        private void fnFillFilterbyTechnologyCombobox()
        {
            comboBox_FilterByTech.Items.Clear();
            comboBox_FilterByTech.Items.Add(Properties.Resources.StrSelectAnyComboOption);
            foreach (var tec in _Metadata.Technologies) { comboBox_FilterByTech.Items.Add(tec.Name); }
            comboBox_FilterByTech.SelectedIndex = 0;
        }
        
        private List<PlaylistsModel> fnFilterPlaylistbyCategoryandTechnology(string categoryId, string categoryName, string technology)
        {
            List<PlaylistsModel> resultPlaylists = null;
            try
            {
                if (categoryName != Properties.Resources.StrSelectAnyComboOption && technology == Properties.Resources.StrSelectAnyComboOption)
                {
                    if(categoryName.Contains(Properties.Resources.StrSubjectLevel2Intent))
                        resultPlaylists = _PlaylistList.Where(o => o.CatId == categoryId).ToList();
                    else if (categoryName.Contains(Properties.Resources.StrSubjectLevel1Intent))
                        resultPlaylists = _PlaylistList.Where(o => o.SubCategory == categoryName.Trim()).ToList();
                    else
                        resultPlaylists = _PlaylistList.Where(o => o.Category == categoryName.Trim()).ToList();
                }
                else if (categoryName == Properties.Resources.StrSelectAnyComboOption && technology != Properties.Resources.StrSelectAnyComboOption)
                    resultPlaylists = _PlaylistList.Where(o => o.Technology == technology).ToList();
                else if (categoryName != Properties.Resources.StrSelectAnyComboOption && technology != Properties.Resources.StrSelectAnyComboOption)
                {
                    //resultPlaylists = _PlaylistList.Where(o => o.Technology == technology && o.Category == categoryName).ToList();
                    if (categoryName.Contains(Properties.Resources.StrSubjectLevel2Intent))
                        resultPlaylists = _PlaylistList.Where(o => o.Technology == technology && o.CatId == categoryId).ToList();
                    else if (categoryName.Contains(Properties.Resources.StrSubjectLevel1Intent))
                        resultPlaylists = _PlaylistList.Where(o => o.Technology == technology && o.SubCategory == categoryName.Trim()).ToList();
                    else
                        resultPlaylists = _PlaylistList.Where(o => o.Technology == technology && o.Category == categoryName.Trim()).ToList();
                }
                else
                    resultPlaylists = _PlaylistList;
            }
            catch(Exception ex) { }
            return resultPlaylists;
        }

        private void fnMoveAssetNodeUp()
        {
            TreeNode tv = new TreeNode();
            TreeNode selectedNodeToMove = new TreeNode();
            try
            {
                TreeNode parent = treeView1.SelectedNode.Parent;
                TreeNode node = this.treeView1.SelectedNode.Clone() as TreeNode;
                if (parent != null)
                {
                    int index = -1;
                    for (int j = 0; j < parent.Nodes.Count; j++)
                    {
                        if (this.treeView1.SelectedNode == parent.Nodes[j])
                        {
                            index = j;
                            break;
                        }
                    }
                    if (index > 0)
                    {
                        this.treeView1.BeginUpdate();
                        this.treeView1.SelectedNode.Parent.Nodes.Insert(index - 1, node);
                        this.treeView1.SelectedNode.Parent.Nodes.RemoveAt(index + 1);
                        this.treeView1.EndUpdate();
                        this.treeView1.SelectedNode = node;
                        this.treeView1.Select();
                        fnChangeAssetOrderinPlaylist(true);
                    }
                }
            }

            catch (Exception ex)
            {
            }
        }

        private void fnMoveAssetNodeDown()
        {
            TreeNode tv = new TreeNode();
            TreeNode selectedNodeToMove = new TreeNode();
            try
            {
                TreeNode parent = treeView1.SelectedNode.Parent;
                TreeNode node = this.treeView1.SelectedNode.Clone() as TreeNode;
                selectedNodeToMove = treeView1.SelectedNode;
                if (parent != null)
                {
                    int index = -1;
                    for (int j = 0; j < parent.Nodes.Count; j++)
                    {
                        if (this.treeView1.SelectedNode == parent.Nodes[j])
                        {
                            index = j;
                            break;
                        }
                    }
                    if (parent.Nodes.Count > index + 1)
                    {
                        this.treeView1.BeginUpdate();
                        this.treeView1.SelectedNode.Parent.Nodes.RemoveAt(index);
                        this.treeView1.SelectedNode.Parent.Nodes.Insert(index + 1, node);
                        this.treeView1.EndUpdate();
                        this.treeView1.SelectedNode = node;
                        this.treeView1.Select();
                        fnChangeAssetOrderinPlaylist(false);
                    }
                }
            }

            catch (Exception ex)
            {

            }
        }

        private void fnChangeAssetOrderinPlaylist(bool isMoveUp)
        {
            try
            {
                string currentPlaylistId = treeView1.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                var assetsofCurrentPlaylist = _PlaylistList.FirstOrDefault(o => o.Id == currentPlaylistId).Assets;
                int index = assetsofCurrentPlaylist.IndexOf(_SelectedItemId);
                if (isMoveUp)
                {                    
                    var old = assetsofCurrentPlaylist[index - 1];
                    assetsofCurrentPlaylist[index - 1] = assetsofCurrentPlaylist[index];
                    assetsofCurrentPlaylist[index] = old;
                }
                else
                {
                    var old = assetsofCurrentPlaylist[index + 1];
                    assetsofCurrentPlaylist[index + 1] = assetsofCurrentPlaylist[index];
                    assetsofCurrentPlaylist[index] = old;   
                }
                _JSONOperations.fnSavePlaylistLocalRepoFile(_PlaylistList);
            }
            catch(Exception ex) { }
        }

        #endregion

        #region compareobjects
        private bool fnIsDataModified()
        {
            bool blnStatus = false;

            if (_SelectedItemType == "playlist" && _SelectedPlaylist != null)
            {
                if ((_SelectedPlaylist.Id == null ? String.Empty : _SelectedPlaylist.Id) != textBox_Id.Text.Trim()
                    || (_SelectedPlaylist.Title == null ? String.Empty : _SelectedPlaylist.Title) != textBox_Title.Text.Trim()
                    || (_SelectedPlaylist.Image == null ? String.Empty : _SelectedPlaylist.Image) != textBox_Image.Text.Trim()
                    || (_SelectedPlaylist.Description == null ? String.Empty : _SelectedPlaylist.Description) != textBox_CommonDesc.Text.Trim()
                    || (_SelectedPlaylist.Level == null ? String.Empty : _SelectedPlaylist.Level) != (comboBox_Level.SelectedItem == null ? String.Empty : comboBox_Level.SelectedItem.ToString())
                    || (_SelectedPlaylist.Audience == null ? String.Empty : _SelectedPlaylist.Audience) != (comboBox_Audience.SelectedItem == null ? String.Empty : comboBox_Audience.SelectedItem.ToString())
                    || (_SelectedPlaylist.Technology == null ? String.Empty : _SelectedPlaylist.Technology) != (comboBox_Technology.SelectedItem == null ? String.Empty : comboBox_Technology.SelectedItem.ToString())
                    || (_SelectedPlaylist.Subject == null ? String.Empty : _SelectedPlaylist.Subject) != (comboBox_Subject.SelectedItem == null ? String.Empty : (comboBox_Subject.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Subject.SelectedItem.ToString()))
                    || (_SelectedPlaylist.Category == null ? String.Empty : _SelectedPlaylist.Category) != (comboBox_Category.SelectedItem == null ? String.Empty : comboBox_Category.SelectedItem.ToString())
                    || (_SelectedPlaylist.StatusTag == null ? String.Empty : _SelectedPlaylist.StatusTag) != (comboBox_StatusTag.SelectedItem == null ? String.Empty : (comboBox_StatusTag.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_StatusTag.SelectedItem.ToString()))
                    || (_SelectedPlaylist.Source == null ? String.Empty : _SelectedPlaylist.Source) != (comboBox_Source.SelectedItem == null ? String.Empty : comboBox_Source.SelectedItem.ToString()))
                {
                    blnStatus = true;
                }
            }
            else if(_SelectedItemType == "asset" && _SelectedAsset != null)
            {
                if ((_SelectedAsset.Id == null ? String.Empty : _SelectedAsset.Id) != textBox_Id.Text.Trim()
                   || (_SelectedAsset.Title == null ? String.Empty : _SelectedAsset.Title) != textBox_Title.Text.Trim()
                   || (_SelectedAsset.Description == null ? String.Empty : _SelectedAsset.Description) != textBox_CommonDesc.Text.Trim()
                   || (_SelectedAsset.MediaId == null ? String.Empty : _SelectedAsset.MediaId) != textBox_MediaId.Text.Trim()
                   || (_SelectedAsset.Url == null ? String.Empty : _SelectedAsset.Url) != textBox_Url.Text.Trim()
                   || (_SelectedAsset.Level == null ? String.Empty : _SelectedAsset.Level) != (comboBox_Level.SelectedItem == null ? String.Empty : comboBox_Level.SelectedItem.ToString())
                   || (_SelectedAsset.Audience == null ? String.Empty : _SelectedAsset.Audience) != (comboBox_Audience.SelectedItem == null ? String.Empty : comboBox_Audience.SelectedItem.ToString())
                   || (_SelectedAsset.Technology == null ? String.Empty : _SelectedAsset.Technology) != (comboBox_Technology.SelectedItem == null ? String.Empty : comboBox_Technology.SelectedItem.ToString())
                   || (_SelectedAsset.Subject == null ? String.Empty : _SelectedAsset.Subject) != (comboBox_Subject.SelectedItem == null ? String.Empty : (comboBox_Subject.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Subject.SelectedItem.ToString()))
                   || (_SelectedAsset.Category == null ? String.Empty : _SelectedAsset.Category) != (comboBox_Category.SelectedItem == null ? String.Empty : comboBox_Category.SelectedItem.ToString())
                   || (_SelectedAsset.StatusTag == null ? String.Empty : _SelectedAsset.StatusTag) != (comboBox_StatusTag.SelectedItem == null ? String.Empty : (comboBox_StatusTag.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_StatusTag.SelectedItem.ToString()))
                   || (_SelectedAsset.Source == null ? String.Empty : _SelectedAsset.Source) != (comboBox_Source.SelectedItem == null ? String.Empty : comboBox_Source.SelectedItem.ToString()))
                {
                    blnStatus = true;
                }
            }

            return blnStatus;
        }

        private bool fnIsMetadataModified()
        {
            bool blnStatus = false;
            if (_SelectedMetaItemType == "tech" && _SelectedTechnology != null)
            {
                if ((_SelectedTechnology.Name == null ? String.Empty : _SelectedTechnology.Name) != textBox_Meta_Name.Text.Trim()
                   || (_SelectedTechnology.Image == null ? String.Empty : _SelectedTechnology.Image) != textBox_Meta_Image.Text.Trim())
                {
                    blnStatus = true;
                }
            }
            else if (_SelectedMetaItemType == "subj" && !string.IsNullOrEmpty(_SelectedSubject))
            {
                if (_SelectedSubject != textBox_Meta_Name.Text.Trim())
                    blnStatus = true;
            }
            else if(_SelectedMetaItemType == "cate" && _SelectedCategory != null)
            {
                if ((_SelectedCategory.Name == null ? String.Empty : _SelectedCategory.Name) != textBox_Meta_Name.Text.Trim()
                   || (_SelectedCategory.Image == null ? String.Empty : _SelectedCategory.Image) != textBox_Meta_Image.Text.Trim()
                   || (_SelectedCategory.Security == null ? String.Empty : _SelectedCategory.Security) != (comboBox_Meta_Security.SelectedItem == null ? String.Empty : (comboBox_Meta_Security.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Meta_Security.SelectedItem.ToString()))
                   || (_SelectedCategory.Technology == null ? String.Empty : _SelectedCategory.Technology) != (comboBox_MetaTechnology.SelectedItem == null ? String.Empty : (comboBox_MetaTechnology.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_MetaTechnology.SelectedItem.ToString())))
                {
                    blnStatus = true;
                }
            }
            else if (_SelectedMetaItemType == "subc" && _SelectedSubCategory != null)
            {
                if ((_SelectedSubCategory.Name == null ? String.Empty : _SelectedSubCategory.Name) != textBox_Meta_Name.Text.Trim()
                   || (_SelectedSubCategory.Image == null ? String.Empty : _SelectedSubCategory.Image) != textBox_Meta_Image.Text.Trim()
                   || (_SelectedSubCategory.Security == null ? String.Empty : _SelectedSubCategory.Security) != (comboBox_Meta_Security.SelectedItem == null ? String.Empty : (comboBox_Meta_Security.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Meta_Security.SelectedItem.ToString()))
                   || (_SelectedSubCategory.Technology == null ? String.Empty : _SelectedSubCategory.Technology) != (comboBox_MetaTechnology.SelectedItem == null ? String.Empty : (comboBox_MetaTechnology.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_MetaTechnology.SelectedItem.ToString())))
                {
                    blnStatus = true;
                }
            }
            else if (_SelectedMetaItemType == "subcsubj1" && _SelectedSubCategorySubjectLevel1 != null)
            {
                if ((_SelectedSubCategorySubjectLevel1.Name == null ? String.Empty : _SelectedSubCategorySubjectLevel1.Name) != textBox_Meta_Name.Text.Trim()
                   || (_SelectedSubCategorySubjectLevel1.Image == null ? String.Empty : _SelectedSubCategorySubjectLevel1.Image) != textBox_Meta_Image.Text.Trim()
                   || (_SelectedSubCategorySubjectLevel1.Security == null ? String.Empty : _SelectedSubCategorySubjectLevel1.Security) != (comboBox_Meta_Security.SelectedItem == null ? String.Empty : (comboBox_Meta_Security.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Meta_Security.SelectedItem.ToString()))
                   || (_SelectedSubCategorySubjectLevel1.Technology == null ? String.Empty : _SelectedSubCategorySubjectLevel1.Technology) != (comboBox_MetaTechnology.SelectedItem == null ? String.Empty : (comboBox_MetaTechnology.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_MetaTechnology.SelectedItem.ToString())))
                {
                    blnStatus = true;
                }
            }
            else if (_SelectedMetaItemType == "subcsubj2" && _SelectedSubCategorySubjectLevel2 != null)
            {
                if ((_SelectedSubCategorySubjectLevel2.Name == null ? String.Empty : _SelectedSubCategorySubjectLevel2.Name) != textBox_Meta_Name.Text.Trim()
                   || (_SelectedSubCategorySubjectLevel2.Image == null ? String.Empty : _SelectedSubCategorySubjectLevel2.Image) != textBox_Meta_Image.Text.Trim()
                   || (_SelectedSubCategorySubjectLevel2.Security == null ? String.Empty : _SelectedSubCategorySubjectLevel2.Security) != (comboBox_Meta_Security.SelectedItem == null ? String.Empty : (comboBox_Meta_Security.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_Meta_Security.SelectedItem.ToString()))
                   || (_SelectedSubCategorySubjectLevel2.Technology == null ? String.Empty : _SelectedSubCategorySubjectLevel2.Technology) != (comboBox_MetaTechnology.SelectedItem == null ? String.Empty : (comboBox_MetaTechnology.SelectedItem.ToString() == Properties.Resources.StrSelectAnyComboOption ? "" : comboBox_MetaTechnology.SelectedItem.ToString())))
                {
                    blnStatus = true;
                }
            }
            else if (_SelectedMetaItemType == "audi" && _SelectedAudience != null)
            {
                if ((_SelectedAudience.Name == null ? String.Empty : _SelectedAudience.Name) != textBox_Meta_Name.Text.Trim()
                   || (_SelectedAudience.Image == null ? String.Empty : _SelectedAudience.Image) != textBox_Meta_Image.Text.Trim())
                {
                    blnStatus = true;
                }
            }
            else if (_SelectedMetaItemType == "sour" && !string.IsNullOrEmpty(_SelectedSource))
            {
                if (_SelectedSource != textBox_Meta_Name.Text.Trim())
                    blnStatus = true;
            }
            else if (_SelectedMetaItemType == "leve" && !string.IsNullOrEmpty(_SelectedLevel))
            {
                if (_SelectedLevel != textBox_Meta_Name.Text.Trim())
                    blnStatus = true;
            }
            else if (_SelectedMetaItemType == "stat" && !string.IsNullOrEmpty(_SelectedStatusTag))
            {
                if (_SelectedStatusTag != textBox_Meta_Name.Text.Trim())
                    blnStatus = true;
            }
            return blnStatus;
        }

        #endregion

        #region PublishtoProduction
        private void button_PublishtoProduction_Click(object sender, EventArgs e)
        {
            fnPublishToProduction();
        }

        public void fnPublishToProduction()
        {
            string strCommitMessage = string.Empty;
            bool blnCommitStatus = false;
            GITOperations objGITOperations = null;
            GenericStatus objGenericStatus = null;
            SessionData objSessionData = null;

            try
            {
                var confirmResult = MessageBox.Show(Properties.Resources.MsgConfirmPullonPushtoProd, Properties.Resources.TitleConfirmPullonPushtoProd, MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.No)
                    return;
                objSessionData = new SessionData();
                objGITOperations = new GITOperations();
                Cursor.Current = Cursors.WaitCursor;
                bool doesProdFilesExists = objGITOperations.fnIsaValidRepo(objSessionData.ProductionLocalRepositoryPath);                
                if (doesProdFilesExists)
                    objGITOperations.fnPullProductionFiles();
                else
                    objGITOperations.fnCloneGITProductionRepository();
                objGITOperations.fnPullFiles();
                Cursor.Current = Cursors.Default;
                frmPublishtoProduction _frmPublishtoProd = new frmPublishtoProduction();
                //_frmPublishtoProd.MdiParent = this;
                _frmPublishtoProd.Show();
            }
            catch (Exception ex) { }
            finally
            {
                objGITOperations = null;
                objGenericStatus = null;
                objSessionData = null;
            }
        }

        public void fnRefreshWindowAfterPublishtoProduction()
        {
            if (_JSONOperations.fnLoadModelsFromJSON(ref _PlaylistList, ref _AssetsList, ref _Metadata))
            {
                _SelectedItemType = "";
                _SelectedMetaItemType = "";
                _SelectedPlaylist = null;
                fnFillFilterbyCategoryCombobox();
                fnFillFilterbyTechnologyCombobox();
                fnLoadMetadataTree();
            }
            else
            {
                fnValidateJSONs();
                fnEnableScreen();
            }            
        }

        #endregion

    }
}
