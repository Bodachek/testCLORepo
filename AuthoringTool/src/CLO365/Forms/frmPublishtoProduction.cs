using CLO365.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CLO365.Forms
{
    public partial class frmPublishtoProduction : Form
    {
        #region Static Variables
        private SessionData _SessionData = new SessionData();
        private JSONOperations _JSONOperations = new JSONOperations();

        private List<AssetsModel> _AssetsListProd = null;
        private MetadataModel _MetadataProd = null;
        private List<PlaylistsModel> _PlaylistListProd = null;

        private List<AssetsModel> _AssetsListStag = null;
        private MetadataModel _MetadataStag = null;
        private List<PlaylistsModel> _PlaylistListStag = null;

        private List<ProductionComparisonModel> _ChangedPlaylistTags = new List<ProductionComparisonModel>();
        private List<ProductionComparisonModel> _ChangedMetadataTags = new List<ProductionComparisonModel>();
        private ProductionComparisonModel _SelectedPlaylistTag = new ProductionComparisonModel();
        private ProductionComparisonModel _SelectedMetadataTag = new ProductionComparisonModel();

        private string _SelectedTree = "playlist";
        private string _SelectedItemType = "";
        private string _SelectedItemId = "";
        private string _SelectedMetaItemType = "tech";
        private string _SelectedMetaItemValue = "";
        private int isAcceptedRejectedAll = 0;
        #endregion

        #region constructor
        public frmPublishtoProduction()
        {
            InitializeComponent();
            labelDeletedColor.BackColor = Color.OrangeRed;
            labelEditColor.BackColor = Color.Yellow;
            labelNewColor.BackColor = Color.LightGreen;
        }

        #endregion

        #region formLoad
        private void frmPublishtoProduction_Load(object sender, EventArgs e)
        {
            if (_JSONOperations.fnLoadProductionModelsFromJSON(ref _PlaylistListProd, ref _AssetsListProd, ref _MetadataProd))
            {
                if(_JSONOperations.fnLoadModelsFromJSON(ref _PlaylistListStag, ref _AssetsListStag, ref _MetadataStag))
                {
                    fnLoadPlaylistTree(_PlaylistListStag);
                    fnLoadMetadataTree();
                    fnFillFileCombobox();
                    fnUpdatePendingItemsCount();
                    fnManageAcceptRejectAllButtons();
                }
            }
        }

        #endregion

        #region loadtrees
        private void fnLoadPlaylistTree(List<PlaylistsModel> playlistModelObj)
        {
            try
            {
                treeViewCompareJsonPlaylist.Nodes.Clear();
                List<PlaylistsModel> deletedPlaylists = _PlaylistListProd.Where(p => !_PlaylistListStag.Any(p2 => p2.Id == p.Id)).ToList();
                foreach (PlaylistsModel playModel in playlistModelObj)
                {
                    var objProdPlaylistModel = _PlaylistListProd.FirstOrDefault(o => o.Id == playModel.Id);
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.ImageIndex = 0;
                    if (objProdPlaylistModel == null)
                    {
                        trvRoot.ImageIndex = 8;
                        trvRoot.BackColor = Color.LightGreen;
                        _ChangedPlaylistTags.Add(new ProductionComparisonModel { Tag = "playlist," + playModel.Id, IsAcceptedorRejected = false });
                    }
                    else
                    {
                        bool blnStatus = fnComparePlaylistJsonObjects(playModel, objProdPlaylistModel);
                        if (blnStatus)
                        {
                            trvRoot.ImageIndex = 8;
                            trvRoot.BackColor = Color.Yellow;
                            _ChangedPlaylistTags.Add(new ProductionComparisonModel { Tag = "playlist," + playModel.Id, IsAcceptedorRejected = false });
                        }
                    }
                    trvRoot.Text = playModel.Title;
                    trvRoot.Tag = "playlist," + playModel.Id;
                    treeViewCompareJsonPlaylist.Nodes.Add(trvRoot);
                    if (playModel.Assets != null)
                    {
                        foreach (string assets in playModel.Assets)
                        {
                            var assetName = _AssetsListStag.FirstOrDefault(o => o.Id == assets);
                            var assetProd = _AssetsListProd.FirstOrDefault(o => o.Id == assets);
                            TreeNode assetNode = new TreeNode();
                            assetNode.Text = assetName.Title;
                            assetNode.Tag = "asset," + assets;
                            assetNode.ImageIndex = 1;
                            trvRoot.Nodes.Add(assetNode);
                            if (assetProd == null)
                            {
                                assetNode.ImageIndex = 8;
                                assetNode.BackColor = Color.LightGreen;
                                _ChangedPlaylistTags.Add(new ProductionComparisonModel { Tag = "asset," + assets, IsAcceptedorRejected = false });
                            }
                            else
                            {
                                bool blnStatus = fnCompareAssetJsonObjects(assetName, assetProd);
                                if (blnStatus)
                                {
                                    assetNode.ImageIndex = 8;
                                    assetNode.BackColor = Color.Yellow;
                                    _ChangedPlaylistTags.Add(new ProductionComparisonModel { Tag = "asset," + assets, IsAcceptedorRejected = false });
                                }
                            }                            
                        }
                        if (objProdPlaylistModel != null)
                        { 
                            foreach (string assets in objProdPlaylistModel.Assets)
                            {
                                var assetProd = _AssetsListProd.FirstOrDefault(o => o.Id == assets);
                                if (playModel.Assets != null)
                                {
                                    if (!playModel.Assets.Contains(assets))
                                    {
                                        TreeNode assetNode = new TreeNode();
                                        assetNode.BackColor = Color.OrangeRed;
                                        assetNode.Text = assetProd.Title;
                                        assetNode.Tag = "asset," + assets;
                                        assetNode.ImageIndex = 8;
                                        trvRoot.Nodes.Add(assetNode);
                                        _ChangedPlaylistTags.Add(new ProductionComparisonModel { Tag = "asset," + assets, IsAcceptedorRejected = false });
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (PlaylistsModel playModel in deletedPlaylists)
                {
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.BackColor = Color.OrangeRed;
                    trvRoot.ImageIndex = 8;
                    trvRoot.Text = playModel.Title;
                    trvRoot.Tag = "playlist," + playModel.Id;
                    treeViewCompareJsonPlaylist.Nodes.Add(trvRoot);
                    _ChangedPlaylistTags.Add(new ProductionComparisonModel { Tag = "playlist," + playModel.Id, IsAcceptedorRejected = false });
                    if (playModel.Assets != null)
                    {
                        foreach (string assets in playModel.Assets)
                        {
                            var assetName = _AssetsListProd.FirstOrDefault(o => o.Id == assets);
                            TreeNode assetNode = new TreeNode();
                            assetNode.BackColor = Color.OrangeRed;
                            assetNode.Text = assetName.Title;
                            assetNode.Tag = "asset," + assets;
                            assetNode.ImageIndex = 8;
                            trvRoot.Nodes.Add(assetNode);
                            _ChangedPlaylistTags.Add(new ProductionComparisonModel { Tag = "asset," + assets, IsAcceptedorRejected = false });
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void fnLoadMetadataTree()
        {
            try
            {                  
                treeViewCompareJsonMetadata.Nodes.Clear();
                TreeNode trvRootTech = new TreeNode();
                trvRootTech.Text = "Technologies";
                trvRootTech.Tag = "tech,";
                trvRootTech.ImageIndex = 20;
                treeViewCompareJsonMetadata.Nodes.Add(trvRootTech);

                for(int i = 0; i < _MetadataStag.Technologies.Count; i++)
                {
                    var techModel = _MetadataStag.Technologies[i];
                    Technology prodTechModel = null;
                    TreeNode trvRoot = new TreeNode();
                    if (i < _MetadataProd.Technologies.Count)
                    {
                        prodTechModel = _MetadataProd.Technologies[i];
                        bool blnStatus = fnCompareTechnologyJsonObjects(techModel, prodTechModel);
                        if (blnStatus)
                        {
                            trvRoot.BackColor = Color.Yellow;
                            _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "tech," + techModel.Name, IsAcceptedorRejected = false });
                        }
                    }
                    else
                    {
                        trvRoot.BackColor = Color.LightGreen;
                        _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "tech," + techModel.Name, IsAcceptedorRejected = false });
                    }
                    trvRoot.Text = techModel.Name;
                    trvRoot.Tag = "tech," + techModel.Name;
                    trvRoot.ImageIndex = 20;
                    trvRootTech.Nodes.Add(trvRoot);
                    if (techModel.Subjects != null)
                    {
                        for(int j=0; j<techModel.Subjects.Count; j++)
                        {
                            string subj = techModel.Subjects[j].ToString();
                            string subjProd = "";                            
                            TreeNode subjNode = new TreeNode();
                            subjNode.Text = subj;
                            subjNode.Tag = "subj," + subj;
                            subjNode.ImageIndex = 20;
                            trvRoot.Nodes.Add(subjNode);
                            if (prodTechModel != null)
                            {
                                if (j < prodTechModel.Subjects.Count)
                                {
                                    subjProd = prodTechModel.Subjects[j].ToString();
                                    bool blnStatusSubj = fnCompareMetaStringJsonObjects(subj, subjProd);
                                    if (blnStatusSubj)
                                    {
                                        subjNode.BackColor = Color.Yellow;
                                        if(subjNode.Parent.BackColor != Color.LightGreen)
                                            subjNode.Parent.BackColor = Color.Yellow;
                                        _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "subj," + subj, IsAcceptedorRejected = false });
                                    }
                                }
                                else
                                {
                                    subjNode.BackColor = Color.LightGreen;
                                    if(subjNode.Parent.BackColor != Color.LightGreen)
                                        subjNode.Parent.BackColor = Color.Yellow;
                                    _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "subj," + subj, IsAcceptedorRejected = false });
                                }
                            }                            
                        }
                    }
                }

                TreeNode trvRootCateg = new TreeNode();
                trvRootCateg.Text = "Categories";
                trvRootCateg.Tag = "cate,";
                trvRootCateg.ImageIndex = 20;
                treeViewCompareJsonMetadata.Nodes.Add(trvRootCateg);

                for (int i = 0; i < _MetadataStag.Categories.Count; i++)
                {
                    var categModel = _MetadataStag.Categories[i];
                    Category prodcategModel = null;
                    TreeNode trvRoot = new TreeNode();
                    trvRoot.Text = categModel.Name;
                    trvRoot.Tag = "cate," + categModel.Name;
                    trvRoot.ImageIndex = 20;
                    trvRootCateg.Nodes.Add(trvRoot);
                    if (i < _MetadataProd.Categories.Count)
                    {
                        prodcategModel = _MetadataProd.Categories[i];
                        bool blnStatus = fnCompareCategoryJsonObjects(categModel, prodcategModel);
                        if (blnStatus)
                        {
                            trvRoot.BackColor = Color.Yellow;
                            _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "cate," + categModel.Name, IsAcceptedorRejected = false });
                        }
                    }
                    else
                    {
                        trvRoot.BackColor = Color.LightGreen;
                        _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "cate," + categModel.Name, IsAcceptedorRejected = false });
                    }
                    
                    if (categModel.SubCategories != null)
                    {
                        for (int j=0; j < categModel.SubCategories.Count; j++)
                        {
                            var subcat = categModel.SubCategories[j];
                            SubCategory prodSubcat = null;
                            TreeNode subNode = new TreeNode();
                            subNode.Text = subcat.Name;
                            subNode.Tag = "subc," + subcat.Name;
                            subNode.ImageIndex = 20;
                            trvRoot.Nodes.Add(subNode);
                            if (prodcategModel != null)
                            {
                                if (j < prodcategModel.SubCategories.Count)
                                {
                                    prodSubcat = prodcategModel.SubCategories[j];
                                    bool blnStatusSubcat = fnCompareSubCategoryJsonObjects(subcat, prodSubcat);
                                    if (blnStatusSubcat)
                                    {
                                        subNode.BackColor = Color.Yellow;
                                        if(subNode.Parent.BackColor != Color.LightGreen)
                                            subNode.Parent.BackColor = Color.Yellow;
                                        _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "subc," + subcat.Name, IsAcceptedorRejected = false });
                                    }
                                }
                                else
                                {
                                    subNode.BackColor = Color.LightGreen;
                                    if(subNode.Parent.BackColor != Color.LightGreen)
                                        subNode.Parent.BackColor = Color.Yellow;
                                    _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "subc," + subcat.Name, IsAcceptedorRejected = false });
                                }
                            }
                            
                            for(int k=0; k<subcat.SubCategories.Count; k++)
                            {
                                var subcatsubj1 = subcat.SubCategories[k];
                                SubCategorySubj1 prodSubcatsubj1 = null;
                                TreeNode subNode1 = new TreeNode();
                                subNode1.Text = subcatsubj1.Name;
                                subNode1.Tag = "subcsubj1," + subcatsubj1.Name;
                                subNode1.ImageIndex = 20;
                                subNode.Nodes.Add(subNode1);
                                if (prodcategModel != null)
                                {
                                    if(prodSubcat != null)
                                    {
                                        if(k < prodSubcat.SubCategories.Count)
                                        {
                                            prodSubcatsubj1 = prodSubcat.SubCategories[k];
                                            bool blnStatusSubcatSub1 = fnCompareSubCatSubjLevel1JsonObjects(subcatsubj1, prodSubcatsubj1);
                                            if (blnStatusSubcatSub1)
                                            {
                                                subNode1.BackColor = Color.Yellow;
                                                if(subNode1.Parent.BackColor != Color.LightGreen)
                                                    subNode1.Parent.BackColor = Color.Yellow;
                                                if(subNode1.Parent.Parent.BackColor != Color.LightGreen)
                                                    subNode1.Parent.Parent.BackColor = Color.Yellow;
                                                _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "subcsubj1," + subcatsubj1.Name, IsAcceptedorRejected = false });
                                            }
                                        }
                                        else
                                        {
                                            subNode1.BackColor = Color.LightGreen;
                                            if (subNode1.Parent.BackColor != Color.LightGreen)
                                                subNode1.Parent.BackColor = Color.Yellow;
                                            if (subNode1.Parent.Parent.BackColor != Color.LightGreen)
                                                subNode1.Parent.Parent.BackColor = Color.Yellow;
                                            _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "subcsubj1," + subcatsubj1.Name, IsAcceptedorRejected = false });
                                        }
                                    }
                                }
                                
                                for(int l=0; l<subcatsubj1.SubCategories.Count; l++)
                                {
                                    var subcatsubj2 = subcatsubj1.SubCategories[l];
                                    SubCategorySubj2 prodSubcatsubj2 = null;
                                    TreeNode subNode2 = new TreeNode();
                                    subNode2.Text = subcatsubj2.Name;
                                    subNode2.Tag = "subcsubj2," + subcatsubj2.Name;
                                    subNode2.ImageIndex = 20;
                                    subNode1.Nodes.Add(subNode2);
                                    if (prodcategModel != null)
                                    {
                                        if (prodSubcat != null)
                                        {
                                            if (prodSubcatsubj1 != null)
                                            {
                                                if (l < prodSubcatsubj1.SubCategories.Count)
                                                {
                                                    prodSubcatsubj2 = prodSubcatsubj1.SubCategories[l];
                                                    bool blnStatusSubcatSub2 = fnCompareSubCatSubjLevel2JsonObjects(subcatsubj2, prodSubcatsubj2);
                                                    if (blnStatusSubcatSub2)
                                                    {
                                                        subNode2.BackColor = Color.Yellow;
                                                        if(subNode2.Parent.BackColor != Color.LightGreen)
                                                            subNode2.Parent.BackColor = Color.Yellow;
                                                        if(subNode2.Parent.Parent.BackColor != Color.LightGreen)
                                                            subNode2.Parent.Parent.BackColor = Color.Yellow;
                                                        if(subNode2.Parent.Parent.Parent.BackColor != Color.LightGreen)
                                                            subNode2.Parent.Parent.Parent.BackColor = Color.Yellow;
                                                        _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "subcsubj2," + subcatsubj2.Name, IsAcceptedorRejected = false });
                                                    }
                                                }
                                                else
                                                {
                                                    subNode2.BackColor = Color.LightGreen;
                                                    if (subNode2.Parent.BackColor != Color.LightGreen)
                                                        subNode2.Parent.BackColor = Color.Yellow;
                                                    if (subNode2.Parent.Parent.BackColor != Color.LightGreen)
                                                        subNode2.Parent.Parent.BackColor = Color.Yellow;
                                                    if (subNode2.Parent.Parent.Parent.BackColor != Color.LightGreen)
                                                        subNode2.Parent.Parent.Parent.BackColor = Color.Yellow;
                                                    _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "subcsubj2," + subcatsubj2.Name, IsAcceptedorRejected = false });
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                            }
                        }
                    }
                }

                TreeNode trvRootAud = new TreeNode();
                trvRootAud.Text = "Role";
                trvRootAud.Tag = "audi,";
                trvRootAud.ImageIndex = 20;
                treeViewCompareJsonMetadata.Nodes.Add(trvRootAud);
                for(int i =0; i<_MetadataStag.Audiences.Count; i++)
                {
                    var audModel = _MetadataStag.Audiences[i];
                    Audience prodAudModel = null;
                    TreeNode trvRoot = new TreeNode();
                    if (i < _MetadataProd.Audiences.Count)
                    {
                        prodAudModel = _MetadataProd.Audiences[i];
                        bool blnStatus = fnCompareAudienceJsonObjects(audModel, prodAudModel);
                        if (blnStatus)
                        {
                            trvRoot.BackColor = Color.Yellow;
                            _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "audi," + audModel.Name, IsAcceptedorRejected = false });
                        }
                    }
                    else
                    {
                        trvRoot.BackColor = Color.LightGreen;
                        _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "audi," + audModel.Name, IsAcceptedorRejected = false });
                    }
                    trvRoot.Text = audModel.Name;
                    trvRoot.Tag = "audi," + audModel.Name;
                    trvRoot.ImageIndex = 20;
                    trvRootAud.Nodes.Add(trvRoot);
                }

                TreeNode trvRootSou = new TreeNode();
                trvRootSou.Text = "Sources";
                trvRootSou.Tag = "sour,";
                trvRootSou.ImageIndex = 20;
                treeViewCompareJsonMetadata.Nodes.Add(trvRootSou);

                for(int i=0; i< _MetadataStag.Sources.Count; i++)
                {
                    var souModel = _MetadataStag.Sources[i];
                    string prodSouModel = "";
                    TreeNode trvRoot = new TreeNode();
                    if (i < _MetadataProd.Sources.Count)
                    {
                        prodSouModel = _MetadataProd.Sources[i];
                        bool blnStatus = fnCompareMetaStringJsonObjects(souModel, prodSouModel);
                        if (blnStatus)
                        {
                            trvRoot.BackColor = Color.Yellow;
                            _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "sour," + souModel, IsAcceptedorRejected = false });
                        }
                    }
                    else
                    {
                        trvRoot.BackColor = Color.LightGreen;
                        _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "sour," + souModel, IsAcceptedorRejected = false });
                    }
                    trvRoot.Text = souModel;
                    trvRoot.Tag = "sour," + souModel;
                    trvRoot.ImageIndex = 20;
                    trvRootSou.Nodes.Add(trvRoot);
                }

                TreeNode trvRootLevel = new TreeNode();
                trvRootLevel.Text = "Levels";
                trvRootLevel.Tag = "leve,";
                trvRootLevel.ImageIndex = 20;
                treeViewCompareJsonMetadata.Nodes.Add(trvRootLevel);

                for(int i =0; i<_MetadataStag.Levels.Count; i++)
                {
                    var level = _MetadataStag.Levels[i];
                    string prodLevel = "";
                    TreeNode trvRoot = new TreeNode();
                    if (i < _MetadataProd.Levels.Count)
                    {
                        prodLevel = _MetadataProd.Levels[i];
                        bool blnStatus = fnCompareMetaStringJsonObjects(level, prodLevel);
                        if (blnStatus)
                        {
                            trvRoot.BackColor = Color.Yellow;
                            _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "leve," + level, IsAcceptedorRejected = false });
                        }
                    }
                    else
                    {
                        trvRoot.BackColor = Color.LightGreen;
                        _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "leve," + level, IsAcceptedorRejected = false });
                    }
                    trvRoot.Text = level;
                    trvRoot.Tag = "leve," + level;
                    trvRoot.ImageIndex = 20;
                    trvRootLevel.Nodes.Add(trvRoot);
                }

                TreeNode trvRootStatusTag = new TreeNode();
                trvRootStatusTag.Text = "Status Tag";
                trvRootStatusTag.Tag = "stat,";
                trvRootStatusTag.ImageIndex = 20;
                treeViewCompareJsonMetadata.Nodes.Add(trvRootStatusTag);

                for(int i = 0; i < _MetadataStag.StatusTag.Count; i++)
                {
                    var statTag = _MetadataStag.StatusTag[i];
                    string prodstatTag = "";
                    TreeNode trvRoot = new TreeNode();
                    if (i < _MetadataProd.StatusTag.Count)
                    {
                        prodstatTag = _MetadataProd.StatusTag[i];
                        bool blnStatus = fnCompareMetaStringJsonObjects(statTag, prodstatTag);
                        if (blnStatus)
                        {
                            trvRoot.BackColor = Color.Yellow;
                            _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "stat," + statTag, IsAcceptedorRejected = false });
                        }
                    }
                    else
                    {
                        trvRoot.BackColor = Color.LightGreen;
                        _ChangedMetadataTags.Add(new ProductionComparisonModel { Tag = "stat," + statTag, IsAcceptedorRejected = false });
                    }
                    trvRoot.Text = statTag;
                    trvRoot.Tag = "stat," + statTag;
                    trvRoot.ImageIndex = 20;
                    trvRootStatusTag.Nodes.Add(trvRoot);
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region compareObjects
        private bool fnComparePlaylistJsonObjects(PlaylistsModel objStagingModel, PlaylistsModel objProdModel)
        {
            bool blnStatus = false;            
            if ((objStagingModel.Id == null ? String.Empty : objStagingModel.Id) != (objProdModel.Id == null ? String.Empty : objProdModel.Id)
                || (objStagingModel.Title == null ? String.Empty : objStagingModel.Title) != (objProdModel.Title == null ? String.Empty : objProdModel.Title)
                || (objStagingModel.Image == null ? String.Empty : objStagingModel.Image) != (objProdModel.Image == null ? String.Empty : objProdModel.Image)
                || (objStagingModel.Description == null ? String.Empty : objStagingModel.Description) != (objProdModel.Description == null ? String.Empty : objProdModel.Description)
                || (objStagingModel.Level == null ? String.Empty : objStagingModel.Level) != (objProdModel.Level == null ? String.Empty : objProdModel.Level)
                || (objStagingModel.Audience == null ? String.Empty : objStagingModel.Audience) != (objProdModel.Audience == null ? String.Empty : objProdModel.Audience)
                || (objStagingModel.Technology == null ? String.Empty : objStagingModel.Technology) != (objProdModel.Technology == null ? String.Empty : objProdModel.Technology)
                || (objStagingModel.Subject == null ? String.Empty : objStagingModel.Subject) != (objProdModel.Subject == null ? String.Empty : objProdModel.Subject)
                || (objStagingModel.Category == null ? String.Empty : objStagingModel.Category) != (objProdModel.Category == null ? String.Empty : objProdModel.Category)
                || (objStagingModel.SubCategory == null ? String.Empty : objStagingModel.SubCategory) != (objProdModel.SubCategory == null ? String.Empty : objProdModel.SubCategory)
                || (objStagingModel.CatId == null ? String.Empty : objStagingModel.CatId) != (objProdModel.CatId == null ? String.Empty : objProdModel.CatId)
                || (objStagingModel.StatusTag == null ? String.Empty : objStagingModel.StatusTag) != (objProdModel.StatusTag == null ? String.Empty : objProdModel.StatusTag)
                || (objStagingModel.Source == null ? String.Empty : objStagingModel.Source) != (objProdModel.Source == null ? String.Empty : objProdModel.Source))
            {
                blnStatus = true;
            }
            if(blnStatus == false)
            {
                if (!objStagingModel.Assets.SequenceEqual(objProdModel.Assets))
                    blnStatus = true;
            }
            return blnStatus;
        }

        private bool fnCompareAssetJsonObjects(AssetsModel objStagingModel, AssetsModel objProdModel)
        {
            bool blnStatus = false;
            if ((objStagingModel.Id == null ? String.Empty : objStagingModel.Id) != (objProdModel.Id == null ? String.Empty : objProdModel.Id)
                || (objStagingModel.Title == null ? String.Empty : objStagingModel.Title) != (objProdModel.Title == null ? String.Empty : objProdModel.Title)
                || (objStagingModel.Description == null ? String.Empty : objStagingModel.Description) != (objProdModel.Description == null ? String.Empty : objProdModel.Description)
                || (objStagingModel.Level == null ? String.Empty : objStagingModel.Level) != (objProdModel.Level == null ? String.Empty : objProdModel.Level)
                || (objStagingModel.Audience == null ? String.Empty : objStagingModel.Audience) != (objProdModel.Audience == null ? String.Empty : objProdModel.Audience)
                || (objStagingModel.Technology == null ? String.Empty : objStagingModel.Technology) != (objProdModel.Technology == null ? String.Empty : objProdModel.Technology)
                || (objStagingModel.Subject == null ? String.Empty : objStagingModel.Subject) != (objProdModel.Subject == null ? String.Empty : objProdModel.Subject)
                || (objStagingModel.Category == null ? String.Empty : objStagingModel.Category) != (objProdModel.Category == null ? String.Empty : objProdModel.Category)
                || (objStagingModel.SubCategory == null ? String.Empty : objStagingModel.SubCategory) != (objProdModel.SubCategory == null ? String.Empty : objProdModel.SubCategory)
                || (objStagingModel.CatId == null ? String.Empty : objStagingModel.CatId) != (objProdModel.CatId == null ? String.Empty : objProdModel.CatId)
                || (objStagingModel.StatusTag == null ? String.Empty : objStagingModel.StatusTag) != (objProdModel.StatusTag == null ? String.Empty : objProdModel.StatusTag)
                || (objStagingModel.Url == null ? String.Empty : objStagingModel.Url) != (objProdModel.Url == null ? String.Empty : objProdModel.Url)
                || (objStagingModel.MediaId == null ? String.Empty : objStagingModel.MediaId) != (objProdModel.MediaId == null ? String.Empty : objProdModel.MediaId)
                || (objStagingModel.Source == null ? String.Empty : objStagingModel.Source) != (objProdModel.Source == null ? String.Empty : objProdModel.Source))
            {
                blnStatus = true;
            }
            return blnStatus;
        }

        private bool fnCompareTechnologyJsonObjects(Technology objStagingModel, Technology objProdModel)
        {
            bool blnStatus = false;
            if ((objStagingModel.Name == null ? String.Empty : objStagingModel.Name) != (objProdModel.Name == null ? String.Empty : objProdModel.Name)
                   || (objStagingModel.Image == null ? String.Empty : objStagingModel.Image) != (objProdModel.Image == null ? String.Empty : objProdModel.Image))
            {
                blnStatus = true;
            }
            return blnStatus;
        }

        private bool fnCompareMetaStringJsonObjects(string objStagingModel, string objProdModel)
        {
            bool blnStatus = false;
            if (objStagingModel != objProdModel)
                blnStatus = true;
            return blnStatus;
        }

        private bool fnCompareCategoryJsonObjects(Category objStagingModel, Category objProdModel)
        {
            bool blnStatus = false;
            if ((objStagingModel.Name == null ? String.Empty : objStagingModel.Name) != (objProdModel.Name == null ? String.Empty : objProdModel.Name)
                   || (objStagingModel.Image == null ? String.Empty : objStagingModel.Image) != (objProdModel.Image == null ? String.Empty : objProdModel.Image)
                   || (objStagingModel.Security == null ? String.Empty : objStagingModel.Security) != (objProdModel.Security == null ? String.Empty : objProdModel.Security)
                   || (objStagingModel.Technology == null ? String.Empty : objStagingModel.Technology) != (objProdModel.Technology == null ? String.Empty : objProdModel.Technology))
            {
                blnStatus = true;
            }            
            return blnStatus;
        }

        private bool fnCompareSubCategoryJsonObjects(SubCategory objStagingModel, SubCategory objProdModel)
        {
            bool blnStatus = false;
            if ((objStagingModel.Name == null ? String.Empty : objStagingModel.Name) != (objProdModel.Name == null ? String.Empty : objProdModel.Name)
                   || (objStagingModel.Image == null ? String.Empty : objStagingModel.Image) != (objProdModel.Image == null ? String.Empty : objProdModel.Image)
                   || (objStagingModel.Security == null ? String.Empty : objStagingModel.Security) != (objProdModel.Security == null ? String.Empty : objProdModel.Security)
                   || (objStagingModel.Technology == null ? String.Empty : objStagingModel.Technology) != (objProdModel.Technology == null ? String.Empty : objProdModel.Technology))
            {
                blnStatus = true;
            }
            return blnStatus;
        }

        private bool fnCompareSubCatSubjLevel1JsonObjects(SubCategorySubj1 objStagingModel, SubCategorySubj1 objProdModel)
        {
            bool blnStatus = false;
            if ((objStagingModel.Name == null ? String.Empty : objStagingModel.Name) != (objProdModel.Name == null ? String.Empty : objProdModel.Name)
                   || (objStagingModel.Image == null ? String.Empty : objStagingModel.Image) != (objProdModel.Image == null ? String.Empty : objProdModel.Image)
                   || (objStagingModel.Security == null ? String.Empty : objStagingModel.Security) != (objProdModel.Security == null ? String.Empty : objProdModel.Security)
                   || (objStagingModel.Technology == null ? String.Empty : objStagingModel.Technology) != (objProdModel.Technology == null ? String.Empty : objProdModel.Technology))
            {
                blnStatus = true;
            }
            return blnStatus;
        }

        private bool fnCompareSubCatSubjLevel2JsonObjects(SubCategorySubj2 objStagingModel, SubCategorySubj2 objProdModel)
        {
            bool blnStatus = false;
            if ((objStagingModel.Name == null ? String.Empty : objStagingModel.Name) != (objProdModel.Name == null ? String.Empty : objProdModel.Name)
                   || (objStagingModel.Image == null ? String.Empty : objStagingModel.Image) != (objProdModel.Image == null ? String.Empty : objProdModel.Image)
                   || (objStagingModel.Security == null ? String.Empty : objStagingModel.Security) != (objProdModel.Security == null ? String.Empty : objProdModel.Security)
                   || (objStagingModel.Technology == null ? String.Empty : objStagingModel.Technology) != (objProdModel.Technology == null ? String.Empty : objProdModel.Technology))
            {
                blnStatus = true;
            }
            return blnStatus;
        }

        private bool fnCompareAudienceJsonObjects(Audience objStagingModel, Audience objProdModel)
        {
            bool blnStatus = false;
            if ((objStagingModel.Name == null ? String.Empty : objStagingModel.Name) != (objProdModel.Name == null ? String.Empty : objProdModel.Name)
                   || (objStagingModel.Image == null ? String.Empty : objStagingModel.Image) != (objProdModel.Image == null ? String.Empty : objProdModel.Image))
            {
                blnStatus = true;
            }
            return blnStatus;
        }

        #endregion
               
        #region events
        private void comboBox_SelectFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox_SelectFile.SelectedItem != null)
            {
                checkBox1.Checked = false;
                _SelectedMetadataTag.Tag = null;
                _SelectedPlaylistTag.Tag = null;
                if (comboBox_SelectFile.SelectedItem.ToString() == "Playlists/Assets")
                {
                    _SelectedTree = "playlist";
                    treeViewCompareJsonPlaylist.SelectedNode = treeViewCompareJsonPlaylist.Nodes[0];
                    treeViewCompareJsonPlaylist.Visible = true;
                    treeViewCompareJsonMetadata.Visible = false;
                    propertyEditPanel.Visible = true;
                    panelEditMetadata.Visible = false;                    
                }
                else
                {
                    _SelectedTree = "metadata";
                    treeViewCompareJsonMetadata.SelectedNode = treeViewCompareJsonMetadata.Nodes[0];
                    treeViewCompareJsonPlaylist.Visible = false;
                    treeViewCompareJsonMetadata.Visible = true;
                    propertyEditPanel.Visible = false;                    
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox _checkBox = (CheckBox)sender;
            if (_checkBox.Checked == true)
            {
                treeViewCompareJsonMetadata.ExpandAll();
                treeViewCompareJsonPlaylist.ExpandAll();
            }
            else
            {
                treeViewCompareJsonMetadata.CollapseAll();
                treeViewCompareJsonPlaylist.CollapseAll();
            }
        }

        private void treeViewCompareJsonPlaylist_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            treeViewCompareJsonPlaylist.SelectedNode.SelectedImageIndex = treeViewCompareJsonPlaylist.SelectedNode.ImageIndex;
            var tagSplits = tree.SelectedNode.Tag.ToString().Split(',');
            _SelectedItemType = tagSplits[0];
            _SelectedItemId = tagSplits[1];
            fnIsSelectedPlaylistNodeisChangedNode(tree.SelectedNode.Tag.ToString());
            fnLoadSelectedTreeNode(tagSplits[0], tagSplits[1]);
        }
        
        private void treeViewCompareJsonMetadata_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            treeViewCompareJsonMetadata.SelectedNode.SelectedImageIndex = treeViewCompareJsonMetadata.SelectedNode.ImageIndex;
            pictureBox_Metadata.ImageLocation = null;
            fnIsSelectedMetadataNodeisChangedNode(tree.SelectedNode.Tag.ToString());
            fnManageMetadataSelction();
        }

        private void button_NextChange_Click(object sender, EventArgs e)
        {
            fnSelectNextChange();
        }

        private void button_PreviousChange_Click(object sender, EventArgs e)
        {
            fnSelectPreviousChange();
        }

        private void button_Accept_Click(object sender, EventArgs e)
        {
            fnAcceptChange();
        }

        private void button_Reject_Click(object sender, EventArgs e)
        {
            fnRejectChange();
        }

        private void button_AcceptAll_Click(object sender, EventArgs e)
        {
            isAcceptedRejectedAll = 1;
            button_Accept.Enabled = false;
            button_AcceptAll.Enabled = false;
            button_Reject.Enabled = false;
            button_RejectAll.Enabled = false;
            fnSetIsAcceptedorRejectedtoTrueforAll();
            fnChangeNodeIconsonAcceptAll();
            fnUpdatePendingItemsCount();
        }

        private void button_RejectAll_Click(object sender, EventArgs e)
        {
            isAcceptedRejectedAll = -1;
            button_Accept.Enabled = false;
            button_AcceptAll.Enabled = false;
            button_Reject.Enabled = false;
            button_RejectAll.Enabled = false;
            fnSetIsAcceptedorRejectedtoTrueforAll();
            fnChangeNodeIconsonRejectAll();
            fnUpdatePendingItemsCount();
        }

        #endregion

        #region loadMetadata

        private void fnManageMetadataSelction()
        {
            try
            {
                panelEditMetadata.Enabled = false;
                if (!string.IsNullOrEmpty(treeViewCompareJsonMetadata.SelectedNode.Tag.ToString()))
                {
                    string[] tagSplit = treeViewCompareJsonMetadata.SelectedNode.Tag.ToString().Split(',');
                    _SelectedMetaItemType = tagSplit[0];
                    _SelectedMetaItemValue = tagSplit[1];
                    if (!string.IsNullOrEmpty(tagSplit[1])) { fnLoadSelectedMetadata(tagSplit); }
                    else
                    {
                        panelEditMetadata.Visible = false;                        
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
                textBox_MetaTech.Text = "";
                var metaNames = treeViewCompareJsonMetadata.SelectedNode.Tag.ToString().Split(new[] { ',' }, 2);
                switch (tagSplit[0])
                {
                    case "tech":                        
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        var metaTechObj = _MetadataStag.Technologies.FirstOrDefault(o => o.Name == metaNames[1]);
                        if(metaTechObj == null)
                            metaTechObj = _MetadataProd.Technologies.FirstOrDefault(o => o.Name == metaNames[1]);
                        label_Meta_Heading.Text = "Technology: " + metaTechObj.Name;
                        textBox_Meta_Name.Text = metaTechObj.Name;
                        textBox_Meta_Image.Text = metaTechObj.Image;
                        try
                        { if (!string.IsNullOrEmpty(metaTechObj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaTechObj.Image); } }
                        catch { }
                        break;
                    case "subj":
                        fnDisableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        label_Meta_Heading.Text = "Subject: " + metaNames[1];
                        textBox_Meta_Name.Text = metaNames[1];
                        break;
                    case "cate":                        
                        fnEnableMetadataItems();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        textBox_MetaSecurity.Visible = true;
                        var metaCateObj = _MetadataStag.Categories.FirstOrDefault(o => o.Name == metaNames[1]);
                        if(metaCateObj == null)
                            metaCateObj = _MetadataProd.Categories.FirstOrDefault(o => o.Name == metaNames[1]);
                        label_Meta_Heading.Text = "Category: " + metaCateObj.Name;
                        textBox_Meta_Name.Text = metaCateObj.Name;
                        textBox_Meta_Image.Text = metaCateObj.Image;
                        textBox_MetaSecurity.Text = metaCateObj.Security;
                        textBox_MetaId.Text = metaCateObj.Id;
                        textBox_MetaTech.Text = metaCateObj.Technology;
                        try { if (!string.IsNullOrEmpty(metaCateObj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaCateObj.Image); } }
                        catch { }
                        break;
                    case "subc":
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        textBox_MetaSecurity.Visible = true;
                        string categName = treeViewCompareJsonMetadata.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        var metaSubcObj = _MetadataStag.Categories.FirstOrDefault(o => o.Name == categName).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        if(metaSubcObj == null)
                            metaSubcObj = _MetadataProd.Categories.FirstOrDefault(o => o.Name == categName).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        label_Meta_Heading.Text = "Sub Category: " + metaSubcObj.Name;
                        textBox_Meta_Name.Text = metaSubcObj.Name;
                        textBox_Meta_Image.Text = metaSubcObj.Image;
                        textBox_MetaId.Text = metaSubcObj.Id;
                        textBox_MetaSecurity.Text = metaSubcObj.Security;
                        textBox_MetaTech.Text = metaSubcObj.Technology;
                        try { if (!string.IsNullOrEmpty(metaSubcObj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaSubcObj.Image); } }
                        catch { }
                        break;
                    case "subcsubj1":
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        textBox_MetaSecurity.Visible = true;
                        string categName1 = treeViewCompareJsonMetadata.SelectedNode.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        string subcategName1 = treeViewCompareJsonMetadata.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        var metaSubcSubj1Obj = _MetadataStag.Categories.FirstOrDefault(o => o.Name == categName1).SubCategories.FirstOrDefault(o => o.Name == subcategName1).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        textBox_Meta_Name.Text = metaSubcSubj1Obj.Name;
                        textBox_MetaSecurity.Text = metaSubcSubj1Obj.Security;
                        label_Meta_Heading.Text = "Subject: " + metaSubcSubj1Obj.Name;
                        textBox_Meta_Image.Text = metaSubcSubj1Obj.Image;
                        textBox_MetaId.Text = metaSubcSubj1Obj.Id;
                        textBox_MetaTech.Text = metaSubcSubj1Obj.Technology;
                        try { if (!string.IsNullOrEmpty(metaSubcSubj1Obj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaSubcSubj1Obj.Image); } }
                        catch { }
                        break;
                    case "subcsubj2":
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnEnableCategoryItems();
                        label_Meta_Security.Visible = true;
                        textBox_MetaSecurity.Visible = true;
                        string categName2 = treeViewCompareJsonMetadata.SelectedNode.Parent.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        string subcategName2 = treeViewCompareJsonMetadata.SelectedNode.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        string subjlevel1Name = treeViewCompareJsonMetadata.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        var metaSubcSubj2Obj = _MetadataStag.Categories.FirstOrDefault(o => o.Name == categName2).SubCategories.FirstOrDefault(o => o.Name == subcategName2).SubCategories.FirstOrDefault(o => o.Name == subjlevel1Name).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        textBox_Meta_Name.Text = metaSubcSubj2Obj.Name;
                        textBox_MetaSecurity.Text = metaSubcSubj2Obj.Security;
                        label_Meta_Heading.Text = "Subject: " + metaSubcSubj2Obj.Name;
                        textBox_Meta_Image.Text = metaSubcSubj2Obj.Image;
                        textBox_MetaId.Text = metaSubcSubj2Obj.Id;
                        textBox_MetaTech.Text = metaSubcSubj2Obj.Technology;
                        try { if (!string.IsNullOrEmpty(metaSubcSubj2Obj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaSubcSubj2Obj.Image); } }
                        catch { }
                        break;
                    case "audi":
                        fnEnableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        var metaAudiObj = _MetadataStag.Audiences.FirstOrDefault(o => o.Name == metaNames[1]);
                        label_Meta_Heading.Text = "Role: " + metaAudiObj.Name;
                        textBox_Meta_Name.Text = metaAudiObj.Name;
                        textBox_Meta_Image.Text = metaAudiObj.Image;
                        try { if (!string.IsNullOrEmpty(metaAudiObj.Image)) { pictureBox_Metadata.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, metaAudiObj.Image); } }
                        catch { }
                        break;
                    case "sour":
                        fnDisableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        label_Meta_Heading.Text = "Source: " + metaNames[1];
                        textBox_Meta_Name.Text = metaNames[1];
                        break;
                    case "leve":
                        fnDisableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        label_Meta_Heading.Text = "Level: " + metaNames[1];
                        textBox_Meta_Name.Text = metaNames[1];
                        break;
                    case "stat":
                        fnDisableMetadataItems();
                        fnDisableSecurityItem();
                        fnDisableCategoryItems();
                        label_Meta_Heading.Text = "Status Tag: " + metaNames[1];
                        textBox_Meta_Name.Text = metaNames[1];
                        break;
                }
            }
            catch (Exception ex) { }
        }        

        private void fnEnableMetadataItems()
        {
            label_Meta_Image.Visible = true;
            textBox_Meta_Image.Visible = true;
            pictureBox_Metadata.Visible = true;

        }

        private void fnDisableMetadataItems()
        {
            label_Meta_Image.Visible = false;
            textBox_Meta_Image.Visible = false;
            pictureBox_Metadata.Visible = false;
        }

        private void fnDisableSecurityItem()
        {
            label_Meta_Security.Visible = false;
            textBox_MetaSecurity.Visible = false;
        }

        private void fnEnableCategoryItems()
        {
            label_MetaId.Visible = true;
            textBox_MetaId.Visible = true;
            label_MetaTechnology.Visible = true;
            textBox_MetaTech.Visible = true;
        }

        private void fnDisableCategoryItems()
        {
            label_MetaId.Visible = false;
            textBox_MetaId.Visible = false;
            label_MetaTechnology.Visible = false;
            textBox_MetaTech.Visible = false;
        }       

        #endregion

        #region LoadAssetPlaylist
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
                    playlist = _PlaylistListStag.FirstOrDefault(o => o.Id == id);
                    if(playlist == null)
                        playlist = _PlaylistListProd.FirstOrDefault(o => o.Id == id);
                    label_PlaylistHeading.Text = "Playlist: " + playlist.Title;
                    textBox_Id.Text = playlist.Id;
                    textBox_Title.Text = playlist.Title;
                    textBox_Image.Text = playlist.Image;
                    textBox_CommonDesc.Text = playlist.Description;
                }               
                //>> Level
                textBox_Level.Text = playlist.Level;
                //>> Audience
                textBox_Role.Text = playlist.Audience;
                //>> Technologies
                textBox_tech.Text = playlist.Technology;
                //>> Subject
                textBox_Subject.Text = playlist.Subject;
                //>> Categories
                textBox_Category.Text = playlist.Category;
                //>> SubCategories
                textBox_Subcat.Text = playlist.SubCategory;
                //>> Source
                textBox_Source.Text = playlist.Source;
                //>> Image Preview
                try { pictureBox_Playlist.ImageLocation = Path.Combine(_SessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, playlist.Image); } catch { }
                //>> Status Tag
                textBox_SourceTag.Text = playlist.StatusTag;

            }
            catch (Exception ex) { }
        }

        private void fnResetEditFormForPlaylist()
        {
            label_Assets.Visible = false;
            textBox_Url.Visible = false;
            label_Image.Visible = true;
            textBox_Image.Visible = true;
            label_MediaId.Visible = false;
            textBox_MediaId.Visible = false;
            pictureBox_Playlist.Visible = true;
        }

        private void fnResetEditFormForAsset()
        {
            label_Assets.Visible = true;
            textBox_Url.Visible = true;
            label_Image.Visible = false;
            textBox_Image.Visible = false;
            label_MediaId.Visible = true;
            textBox_MediaId.Visible = true;
            pictureBox_Playlist.Visible = false;
        }        

        private void fnLoadSelectedAsset(string type, string id)
        {
            AssetsModel asset = null;
            try
            {
                fnResetEditFormForAsset();
                
                //>> ID, Title and Image
                textBox_Id.Text = ""; textBox_Title.Text = ""; textBox_CommonDesc.Text = ""; textBox_MediaId.Text = "";
                if (!string.IsNullOrEmpty(id))
                {
                    asset = _AssetsListStag.FirstOrDefault(o => o.Id == id);
                    if(asset == null)
                        asset = _AssetsListProd.FirstOrDefault(o => o.Id == id);
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
                textBox_Level.Text = asset.Level;
                //>> Audiences
                textBox_Role.Text = asset.Audience;
                //>> Technologies
                textBox_tech.Text = asset.Technology;
                //> Subject
                textBox_Subject.Text = asset.Subject;
                //>> Categories
                textBox_Category.Text = asset.Category;
                //>> SubCategories
                textBox_Subcat.Text = asset.SubCategory;
                //>> Source
                textBox_Source.Text = asset.Source;
                //>> Status Tag
                textBox_SourceTag.Text = asset.StatusTag;
                //>> URL
                textBox_Url.Text = asset.Url;
            }
            catch (Exception ex) { }
        }

        #endregion

        #region publishtoproduction
        private void button_Proceed_Click(object sender, EventArgs e)
        {
            fnPublishtoProduction();
        }

        private void fnPublishtoProduction()
        {
            string strCommitMessage = string.Empty;
            bool blnCommitStatus = false;
            GITOperations objGITOperations = null;
            GenericStatus objGenericStatus = null;
            JSONOperations objJsonOperations = null;
            try
            {
                int pendingPlaylistCount = 0;
                int pendingMetaCount = 0;
                try { pendingPlaylistCount = _ChangedPlaylistTags.Where(p => p.IsAcceptedorRejected == false).Count(); }
                catch { }
                try { pendingMetaCount = _ChangedMetadataTags.Where(p => p.IsAcceptedorRejected == false).Count(); }
                catch { }
                if (pendingPlaylistCount == 0 && pendingMetaCount == 0)
                {
                    //frmCommit _commitForm = new frmCommit();
                    //_commitForm.ShowDialog();
                    //strCommitMessage = _commitForm.CommitMsg;
                    //if (!string.IsNullOrEmpty(strCommitMessage))
                    //{

                    objGITOperations = new GITOperations();
                    objJsonOperations = new JSONOperations();
                    Cursor.Current = Cursors.WaitCursor;

                    if(isAcceptedRejectedAll == 1)
                    {
                        _JSONOperations.fnLoadModelsFromJSON(ref _PlaylistListStag, ref _AssetsListStag, ref _MetadataStag);
                    }
                    else if(isAcceptedRejectedAll == -1)
                    {
                        _JSONOperations.fnLoadProductionModelsFromJSON(ref _PlaylistListStag, ref _AssetsListStag, ref _MetadataStag);
                    }

                    objJsonOperations.fnSaveAssetLocalRepoFile(_AssetsListStag);
                    objJsonOperations.fnSaveAssetProductionLocalRepoFile(_AssetsListStag);
                    objJsonOperations.fnSavePlaylistLocalRepoFile(_PlaylistListStag);
                    objJsonOperations.fnSavePlaylistProductionLocalRepoFile(_PlaylistListStag);
                    objJsonOperations.fnSaveMetadataLocalRepoFile(_MetadataStag);
                    objJsonOperations.fnSaveMetadataProductionLocalRepoFile(_MetadataStag);

                    //fnUdateJsonstoLocalProductionFiles();
                    if (objGITOperations.fnIsProductionLocalRepoHasUncommittedChanges())
                        blnCommitStatus = objGITOperations.fnCommitProductionFiles(strCommitMessage);
                    if(objGITOperations.fnIsLocalRepoHasUncommittedChanges())
                        blnCommitStatus = objGITOperations.fnCommitFiles(strCommitMessage);
                    objGITOperations.fnPushFiles();
                    objGenericStatus = objGITOperations.fnPublishToProduction();
                    if (objGenericStatus.IsSuccess == true)
                    {
                        if (System.Windows.Forms.Application.OpenForms["frmMain"] != null)
                        {
                            (System.Windows.Forms.Application.OpenForms["frmMain"] as frmMain).fnRefreshWindowAfterPublishtoProduction();
                        }
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(Properties.Resources.MsgPushedtoProductionSuccessfully, Properties.Resources.TitleSuccess);
                        this.Close();
                    }
                    else
                    {
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(Properties.Resources.MsgGenericError, Properties.Resources.TitleError);
                    }
                    //}
                    //else
                    //    return;
                }
                else
                    MessageBox.Show(Properties.Resources.MsgAllItemsNotReviwed, Properties.Resources.TitleError);
            }
            catch (Exception ex) { }
            finally
            {
                objGITOperations = null;
                objGenericStatus = null;
                objJsonOperations = null;
            }
        }

        private void fnUdateJsonstoLocalProductionFiles()
        {
            SessionData _sessionData = null;
            try
            {
                _sessionData = new SessionData();
                File.Copy(Path.Combine(_sessionData.StagingLocalRepositoryPath, GetSetConfig.assetjsonFilename), Path.Combine(_sessionData.ProductionLocalRepositoryPath, GetSetConfig.assetjsonFilename), true);
                File.Copy(Path.Combine(_sessionData.StagingLocalRepositoryPath, GetSetConfig.playlistjsonFilename), Path.Combine(_sessionData.ProductionLocalRepositoryPath, GetSetConfig.playlistjsonFilename), true);
                File.Copy(Path.Combine(_sessionData.StagingLocalRepositoryPath, GetSetConfig.metadatajsonFilename), Path.Combine(_sessionData.ProductionLocalRepositoryPath, GetSetConfig.metadatajsonFilename), true);
            }
            catch (Exception ex)
            {

            }
            finally { _sessionData = null; }
        }

        #endregion

        #region methods
        private void fnFillFileCombobox()
        {
            comboBox_SelectFile.Items.Clear();
            comboBox_SelectFile.Items.Add("Playlists/Assets");
            comboBox_SelectFile.Items.Add("Metadata");
            comboBox_SelectFile.SelectedIndex = 0;
        }

        private void fnSelectNextChange()
        {
            try
            {
                if (_SelectedTree == "playlist")
                {
                    if (_SelectedPlaylistTag.Tag == null)
                    {
                        var nextTag = _ChangedPlaylistTags[0];
                        treeViewCompareJsonPlaylist.SelectedNode = fnFindPlaylistTreeNodefromTag(nextTag.Tag);
                    }
                    else
                    {
                        var currentTagItem = _ChangedPlaylistTags.FirstOrDefault(o => o.Tag == _SelectedPlaylistTag.Tag);
                        var nextTag = _ChangedPlaylistTags[(_ChangedPlaylistTags.IndexOf(currentTagItem) + 1)];
                        treeViewCompareJsonPlaylist.SelectedNode = fnFindPlaylistTreeNodefromTag(nextTag.Tag);
                    }
                }
                else
                {
                    if (_SelectedMetadataTag.Tag == null)
                    {
                        var nextTag = _ChangedMetadataTags[0];
                        treeViewCompareJsonMetadata.SelectedNode = fnFindMetadataTreeNodefromTag(nextTag.Tag);
                    }
                    else
                    {
                        var currentTagItem = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == _SelectedMetadataTag.Tag);
                        var nextTag = _ChangedMetadataTags[(_ChangedMetadataTags.IndexOf(currentTagItem) + 1)];
                        treeViewCompareJsonMetadata.SelectedNode = fnFindMetadataTreeNodefromTag(nextTag.Tag);
                    }
                }
            }
            catch(Exception ex)
            { }
        }
        
        private void fnSelectPreviousChange()
        {
            try
            {
                if (_SelectedTree == "playlist")
                {
                    if (_SelectedPlaylistTag.Tag == null)
                    {
                        var previousTag = _ChangedPlaylistTags[0];                        
                        treeViewCompareJsonPlaylist.SelectedNode = fnFindPlaylistTreeNodefromTag(previousTag.Tag);
                    }
                    else
                    {
                        var currentTagItem = _ChangedPlaylistTags.FirstOrDefault(o => o.Tag == _SelectedPlaylistTag.Tag);
                        var previousTag = _ChangedPlaylistTags[(_ChangedPlaylistTags.IndexOf(currentTagItem) - 1)];
                        treeViewCompareJsonPlaylist.SelectedNode = fnFindPlaylistTreeNodefromTag(previousTag.Tag);
                    }
                }
                else
                {
                    if (_SelectedMetadataTag.Tag == null)
                    {
                        var previousTag = _ChangedMetadataTags[0];
                        treeViewCompareJsonMetadata.SelectedNode = fnFindMetadataTreeNodefromTag(previousTag.Tag);
                    }
                    else
                    {
                        var currentTagItem = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == _SelectedMetadataTag.Tag);
                        var previousTag = _ChangedMetadataTags[(_ChangedMetadataTags.IndexOf(currentTagItem) - 1)];
                        treeViewCompareJsonMetadata.SelectedNode = fnFindMetadataTreeNodefromTag(previousTag.Tag);
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void fnIsSelectedPlaylistNodeisChangedNode(string tag)
        {
            try
            {
                if (_ChangedPlaylistTags.Count > 0)
                {
                    var selectedNodeinChangedNode = _ChangedPlaylistTags.FirstOrDefault(o => o.Tag == tag);
                    if (selectedNodeinChangedNode != null)
                    {
                        //_SelectedPlaylistTag = selectedNodeinChangedNode;
                        _SelectedPlaylistTag.Tag = selectedNodeinChangedNode.Tag;
                        _SelectedPlaylistTag.IsAcceptedorRejected = selectedNodeinChangedNode.IsAcceptedorRejected;
                        if (selectedNodeinChangedNode.IsAcceptedorRejected == false)
                        {
                            button_Accept.Enabled = true;
                            button_Reject.Enabled = true;
                        }                   
                    }
                    else
                    {
                        _SelectedPlaylistTag.Tag = null;
                        button_Accept.Enabled = false;
                        button_Reject.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void fnIsSelectedMetadataNodeisChangedNode(string tag)
        {
            try
            {
                if (_ChangedMetadataTags.Count > 0)
                {
                    var selectedNodeinChangedNode = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == tag);
                    if (selectedNodeinChangedNode != null) 
                    {
                        _SelectedMetadataTag.Tag = selectedNodeinChangedNode.Tag;
                        _SelectedMetadataTag.IsAcceptedorRejected = selectedNodeinChangedNode.IsAcceptedorRejected;
                        if (selectedNodeinChangedNode.IsAcceptedorRejected == false)
                        {
                            button_Accept.Enabled = true;
                            button_Reject.Enabled = true;
                        }                  
                    }
                    else
                    {
                        _SelectedMetadataTag.Tag = null;
                        button_Accept.Enabled = false;
                        button_Reject.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private TreeNode fnFindPlaylistTreeNodefromTag(string tag)
        {
            TreeNode itemNode = null;
            foreach (TreeNode node in treeViewCompareJsonPlaylist.Nodes)
            {
                itemNode = fnFindTreeNodeinChilds(tag, node);
                if (itemNode != null) break;
            }
            return itemNode;
        }

        private TreeNode fnFindMetadataTreeNodefromTag(string tag)
        {
            TreeNode itemNode = null;
            foreach (TreeNode node in treeViewCompareJsonMetadata.Nodes)
            {
                itemNode = fnFindTreeNodeinChilds(tag, node);
                if (itemNode != null) break;
            }
            return itemNode;
        }

        private TreeNode fnFindTreeNodeinChilds(string itemId, TreeNode rootNode)
        {
            if (rootNode.Tag.Equals(itemId)) return rootNode;
            foreach (TreeNode node in rootNode.Nodes)
            {
                if (node.Tag.Equals(itemId)) return node;
                TreeNode next = fnFindTreeNodeinChilds(itemId, node);
                if (next != null) return next;
            }
            return null;
        }

        private void fnAcceptChange()
        {            
            if (_SelectedTree == "playlist")
            {
                string currentTag = treeViewCompareJsonPlaylist.SelectedNode.Tag.ToString();
                var currentSelectedCompareObject = _ChangedPlaylistTags.FirstOrDefault(o => o.Tag == currentTag);
                currentSelectedCompareObject.IsAcceptedorRejected = true;
                fnUpdatePendingItemsCount();
                button_Accept.Enabled = false;
                button_Reject.Enabled = false;
                if (_SelectedItemType == "playlist")
                    treeViewCompareJsonPlaylist.SelectedNode.ImageIndex = treeViewCompareJsonPlaylist.SelectedNode.SelectedImageIndex = 6;
                else
                    treeViewCompareJsonPlaylist.SelectedNode.ImageIndex = treeViewCompareJsonPlaylist.SelectedNode.SelectedImageIndex = 6;
            }
            else
            {
                string currentTag = treeViewCompareJsonMetadata.SelectedNode.Tag.ToString();
                var currentSelectedCompareObject = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == currentTag);
                currentSelectedCompareObject.IsAcceptedorRejected = true;
                fnUpdatePendingItemsCount();
                button_Accept.Enabled = false;
                button_Reject.Enabled = false;
                treeViewCompareJsonMetadata.SelectedNode.ImageIndex = treeViewCompareJsonMetadata.SelectedNode.SelectedImageIndex = 6;
            }
            fnManageAcceptRejectAllButtons();
        }

        private void fnRejectChange()
        {
            if (_SelectedTree == "playlist")
            {
                string currentTag = treeViewCompareJsonPlaylist.SelectedNode.Tag.ToString();
                var currentSelectedCompareObject = _ChangedPlaylistTags.FirstOrDefault(o => o.Tag == currentTag);
                currentSelectedCompareObject.IsAcceptedorRejected = true;
                fnUpdatePendingItemsCount();
                button_Accept.Enabled = false;
                button_Reject.Enabled = false;
                fnRejectPlaylistAssetsChanges();
            }
            else
            {
                string currentTag = treeViewCompareJsonMetadata.SelectedNode.Tag.ToString();
                var currentSelectedCompareObject = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == currentTag);
                currentSelectedCompareObject.IsAcceptedorRejected = true;
                fnUpdatePendingItemsCount();
                button_Accept.Enabled = false;
                button_Reject.Enabled = false;
                treeViewCompareJsonMetadata.SelectedNode.ImageIndex = treeViewCompareJsonMetadata.SelectedNode.SelectedImageIndex = 7;
                fnRejectMetadataChanges();
            }
            fnManageAcceptRejectAllButtons();
        }

        private void fnRejectPlaylistAssetsChanges()
        {
            try
            {
                if (_SelectedItemType == "playlist")
                {
                    treeViewCompareJsonPlaylist.SelectedNode.ImageIndex = treeViewCompareJsonPlaylist.SelectedNode.SelectedImageIndex = 7;
                    if (treeViewCompareJsonPlaylist.SelectedNode.BackColor == Color.Yellow)
                    {
                        var selectedStagingItem = _PlaylistListStag.FirstOrDefault(o => o.Id == _SelectedItemId);
                        var selectedProductionItem = _PlaylistListProd.FirstOrDefault(o => o.Id == _SelectedItemId);
                        var index = _PlaylistListStag.IndexOf(selectedStagingItem);
                        if (index != -1)
                            _PlaylistListStag[index] = selectedProductionItem;
                        fnLoadSelectedPlaylist(_SelectedItemType, _SelectedItemId);
                    }
                    else if (treeViewCompareJsonPlaylist.SelectedNode.BackColor == Color.LightGreen)
                    {
                        PlaylistsModel playlist = _PlaylistListStag.FirstOrDefault(o => o.Id == _SelectedItemId);
                        if (playlist != null)
                        {
                            _PlaylistListStag.Remove(playlist);
                            var changedComaprisonPlaylistObj = _ChangedPlaylistTags.FirstOrDefault(o => o.Tag == "playlist," + playlist.Id);
                            _ChangedPlaylistTags.Remove(changedComaprisonPlaylistObj);
                            foreach (string assettt in playlist.Assets)
                            {
                                var playlistsWithaAssettt = _PlaylistListStag.Where(o => o.Assets.Contains(assettt)).ToList();
                                if (playlistsWithaAssettt.Count == 0)
                                {
                                    var corresAssetObj = _AssetsListStag.FirstOrDefault(o => o.Id == assettt);
                                    _AssetsListStag.Remove(corresAssetObj);
                                    var changedComaprisonAssetObj = _ChangedPlaylistTags.FirstOrDefault(o => o.Tag == "asset," + assettt);
                                    _ChangedPlaylistTags.Remove(changedComaprisonAssetObj);
                                }
                            }
                            treeViewCompareJsonPlaylist.SelectedNode.Nodes.Clear();
                            treeViewCompareJsonPlaylist.Nodes.Remove(treeViewCompareJsonPlaylist.SelectedNode);
                        }
                    }
                    else if (treeViewCompareJsonPlaylist.SelectedNode.BackColor == Color.OrangeRed)
                    {
                        var selectedProductionItem = _PlaylistListProd.FirstOrDefault(o => o.Id == _SelectedItemId);
                        _PlaylistListStag.Add(selectedProductionItem);
                        foreach (var assets in selectedProductionItem.Assets)
                        {
                            var corresAssetObj = _AssetsListProd.FirstOrDefault(o => o.Id == assets);
                            _AssetsListStag.Add(corresAssetObj);
                            var changedComaprisonAssetObj = _ChangedPlaylistTags.FirstOrDefault(o => o.Tag == "asset," + assets);
                            changedComaprisonAssetObj.IsAcceptedorRejected = true;
                        }
                    }
                }
                else
                {
                    treeViewCompareJsonPlaylist.SelectedNode.ImageIndex = treeViewCompareJsonPlaylist.SelectedNode.SelectedImageIndex = 7;
                    if (treeViewCompareJsonPlaylist.SelectedNode.BackColor == Color.Yellow)
                    {
                        var selectedStagingItem = _AssetsListStag.FirstOrDefault(o => o.Id == _SelectedItemId);
                        var selectedProductionItem = _AssetsListProd.FirstOrDefault(o => o.Id == _SelectedItemId);
                        var index = _AssetsListStag.IndexOf(selectedStagingItem);
                        if (index != -1)
                            _AssetsListStag[index] = selectedProductionItem;
                        fnLoadSelectedAsset(_SelectedItemType, _SelectedItemId);
                    }
                    else if (treeViewCompareJsonPlaylist.SelectedNode.BackColor == Color.LightGreen)
                    {
                        var corresAssetObj = _AssetsListStag.FirstOrDefault(o => o.Id == _SelectedItemId);
                        _AssetsListStag.Remove(corresAssetObj);
                        var changedComaprisonAssetObj = _ChangedPlaylistTags.FirstOrDefault(o => o.Tag == "asset," + _SelectedItemId);
                        _ChangedPlaylistTags.Remove(changedComaprisonAssetObj);
                        treeViewCompareJsonPlaylist.Nodes.Remove(treeViewCompareJsonPlaylist.SelectedNode);
                    }
                    else if (treeViewCompareJsonPlaylist.SelectedNode.BackColor == Color.OrangeRed)
                    {
                        var corresAssetObj = _AssetsListProd.FirstOrDefault(o => o.Id == _SelectedItemId);
                        _AssetsListStag.Add(corresAssetObj);
                        var corressPlaylistObj = _PlaylistListStag.FirstOrDefault(o => o.Id == treeViewCompareJsonPlaylist.SelectedNode.Tag.ToString().Split(',')[1]);
                        corressPlaylistObj.Assets.Add(corresAssetObj.Id);
                    }
                }
                fnUpdatePendingItemsCount();
            }
            catch { }
        }

        private void fnRejectMetadataChanges()
        {
            try
            {
                int usingStagingPlaylistCount = 0;
                int usingStagingAssetCount = 0;
                var metaNames = treeViewCompareJsonMetadata.SelectedNode.Tag.ToString().Split(new[] { ',' }, 2);
                switch (metaNames[0])
                {
                    case "tech":
                        var metaTechObj = _MetadataStag.Technologies.FirstOrDefault(o => o.Name == metaNames[1]);
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.Yellow)
                        {                            
                            var metaTechProdObj = _MetadataProd.Technologies.FirstOrDefault(o => o.Name == metaNames[1]);
                            var index = _MetadataStag.Technologies.IndexOf(metaTechObj);
                            if (index != -1)
                                _MetadataStag.Technologies[index] = metaTechProdObj;
                            fnLoadSelectedMetadata(metaNames);
                        }
                        else if(treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.Technology == metaNames[1]).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.Technology == metaNames[1]).Count();
                            if(usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                _MetadataStag.Technologies.Remove(metaTechObj);
                                var changedComaprisonMetaTechObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaTechObj);
                                foreach (string subj in metaTechObj.Subjects)
                                {
                                    var changedComaprisonMetaObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == "subj," + subj);
                                    _ChangedMetadataTags.Remove(changedComaprisonMetaObj);
                                }
                                treeViewCompareJsonMetadata.SelectedNode.Nodes.Clear();
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);
                        }                                           
                        break;
                    case "subj":                        
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            string techName = treeViewCompareJsonMetadata.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                            var metaTechSubjObj = _MetadataStag.Technologies.FirstOrDefault(o => o.Name == techName);
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.Subject == metaNames[1]).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.Subject == metaNames[1]).Count();                            
                            if (usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                var changedComaprisonMetaSubjObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaSubjObj);
                                metaTechSubjObj.Subjects.Remove(metaNames[1]);                                
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);

                        }
                        break;
                    case "cate":
                        var metaCateObj = _MetadataStag.Categories.FirstOrDefault(o => o.Name == metaNames[1]);
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.Yellow)
                        {                            
                            var metaCateProdObj = _MetadataProd.Categories.FirstOrDefault(o => o.Name == metaNames[1]);
                            var index = _MetadataStag.Categories.IndexOf(metaCateObj);
                            if (index != -1)
                                _MetadataStag.Categories[index] = metaCateProdObj;
                            fnLoadSelectedMetadata(metaNames);
                        }
                        else if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.Category == metaNames[1]).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.Category == metaNames[1]).Count();
                            if (usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                _MetadataStag.Categories.Remove(metaCateObj);
                                var changedComaprisonMetaCatjObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaCatjObj);
                                foreach (var subcat in metaCateObj.SubCategories)
                                {
                                    var changedComaprisonMetaSubCatObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == "subc," + subcat.Name);
                                    _ChangedMetadataTags.Remove(changedComaprisonMetaSubCatObj);
                                    foreach(var subcatsublevel1 in subcat.SubCategories)
                                    {
                                        var changedComaprisonMetaSubCatSubj1Obj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == "subcsubj1," + subcatsublevel1.Name);
                                        _ChangedMetadataTags.Remove(changedComaprisonMetaSubCatSubj1Obj);
                                        foreach (var subcatsublevel2 in subcatsublevel1.SubCategories)
                                        {
                                            var changedComaprisonMetaSubCatSubj2Obj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == "subcsubj2," + subcatsublevel2.Name);
                                            _ChangedMetadataTags.Remove(changedComaprisonMetaSubCatSubj2Obj);
                                        }
                                    }
                                }
                                treeViewCompareJsonMetadata.SelectedNode.Nodes.Clear();
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);

                        }
                        break;
                    case "subc":
                        string categName = treeViewCompareJsonMetadata.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        var metaSubcObj = _MetadataStag.Categories.FirstOrDefault(o => o.Name == categName).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.Yellow)
                        {                            
                            var metaSubcProdObj = _MetadataProd.Categories.FirstOrDefault(o => o.Name == categName).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                            metaSubcObj.Image = metaSubcProdObj.Image;
                            metaSubcObj.Security = metaSubcProdObj.Security;
                            metaSubcObj.Technology = metaSubcProdObj.Technology;
                            fnLoadSelectedMetadata(metaNames);
                        }
                        else if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.SubCategory == metaNames[1]).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.SubCategory == metaNames[1]).Count();
                            if (usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                _MetadataStag.Categories.FirstOrDefault(o => o.Name == categName).SubCategories.Remove(metaSubcObj);
                                var changedComaprisonMetaCatjObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaCatjObj);                               
                                foreach (var subcatsublevel1 in metaSubcObj.SubCategories)
                                {
                                    var changedComaprisonMetaSubCatSubj1Obj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == "subcsubj1," + subcatsublevel1.Name);
                                    _ChangedMetadataTags.Remove(changedComaprisonMetaSubCatSubj1Obj);
                                    foreach (var subcatsublevel2 in subcatsublevel1.SubCategories)
                                    {
                                        var changedComaprisonMetaSubCatSubj2Obj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == "subcsubj2," + subcatsublevel2.Name);
                                        _ChangedMetadataTags.Remove(changedComaprisonMetaSubCatSubj2Obj);
                                    }
                                }
                                treeViewCompareJsonMetadata.SelectedNode.Nodes.Clear();
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);

                        }
                        break;
                    case "subcsubj1":
                        string categName1 = treeViewCompareJsonMetadata.SelectedNode.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        string subcategName1 = treeViewCompareJsonMetadata.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        var metaSubcSubj1Obj = _MetadataStag.Categories.FirstOrDefault(o => o.Name == categName1).SubCategories.FirstOrDefault(o => o.Name == subcategName1).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.Yellow)
                        {                            
                            var metaSubcSubj1ProdObj = _MetadataProd.Categories.FirstOrDefault(o => o.Name == categName1).SubCategories.FirstOrDefault(o => o.Name == subcategName1).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                            metaSubcSubj1Obj.Image = metaSubcSubj1ProdObj.Image;
                            metaSubcSubj1Obj.Security = metaSubcSubj1ProdObj.Security;
                            metaSubcSubj1Obj.Technology = metaSubcSubj1ProdObj.Technology;
                            fnLoadSelectedMetadata(metaNames);
                        }
                        else if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.CatId == metaSubcSubj1Obj.Id).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.CatId == metaSubcSubj1Obj.Id).Count();
                            if (usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                _MetadataStag.Categories.FirstOrDefault(o => o.Name == categName1).SubCategories.FirstOrDefault(o => o.Name == subcategName1).SubCategories.Remove(metaSubcSubj1Obj);
                                var changedComaprisonMetaCatjObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaCatjObj);
                                foreach (var subcatsublevel2 in metaSubcSubj1Obj.SubCategories)
                                {
                                    var changedComaprisonMetaSubCatSubj2Obj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == "subcsubj2," + subcatsublevel2.Name);
                                    _ChangedMetadataTags.Remove(changedComaprisonMetaSubCatSubj2Obj);
                                }
                                treeViewCompareJsonMetadata.SelectedNode.Nodes.Clear();
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);

                        }
                        break;
                    case "subcsubj2":
                        string categName2 = treeViewCompareJsonMetadata.SelectedNode.Parent.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        string subcategName2 = treeViewCompareJsonMetadata.SelectedNode.Parent.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        string subjlevel1Name = treeViewCompareJsonMetadata.SelectedNode.Parent.Tag.ToString().Split(new[] { ',' }, 2)[1];
                        var metaSubcSubj2Obj = _MetadataStag.Categories.FirstOrDefault(o => o.Name == categName2).SubCategories.FirstOrDefault(o => o.Name == subcategName2).SubCategories.FirstOrDefault(o => o.Name == subjlevel1Name).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.Yellow)
                        {
                            var metaSubcSubj2ProdObj = _MetadataProd.Categories.FirstOrDefault(o => o.Name == categName2).SubCategories.FirstOrDefault(o => o.Name == subcategName2).SubCategories.FirstOrDefault(o => o.Name == subjlevel1Name).SubCategories.FirstOrDefault(o => o.Name == metaNames[1]);
                            metaSubcSubj2Obj.Image = metaSubcSubj2ProdObj.Image;
                            metaSubcSubj2Obj.Security = metaSubcSubj2ProdObj.Security;
                            metaSubcSubj2Obj.Technology = metaSubcSubj2ProdObj.Technology;
                            fnLoadSelectedMetadata(metaNames);
                        }
                        else if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.CatId == metaSubcSubj2Obj.Id).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.CatId == metaSubcSubj2Obj.Id).Count();
                            if (usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                _MetadataStag.Categories.FirstOrDefault(o => o.Name == categName2).SubCategories.FirstOrDefault(o => o.Name == subcategName2).SubCategories.FirstOrDefault(o => o.Name == subjlevel1Name).SubCategories.Remove(metaSubcSubj2Obj);
                                var changedComaprisonMetaCatjObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaCatjObj);                                
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);

                        }
                        break;
                    case "audi":
                        var metaAudiObj = _MetadataStag.Audiences.FirstOrDefault(o => o.Name == metaNames[1]);
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.Yellow)
                        {                            
                            var metaAudiProdObj = _MetadataProd.Audiences.FirstOrDefault(o => o.Name == metaNames[1]);
                            metaAudiObj.Image = metaAudiProdObj.Image;
                            fnLoadSelectedMetadata(metaNames);
                        }
                        else if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.Audience == metaNames[1]).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.Audience == metaNames[1]).Count();
                            if (usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                _MetadataStag.Audiences.Remove(metaAudiObj);
                                var changedComaprisonMetaTechObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaTechObj);                                
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);

                        }
                        break;
                    case "sour":
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.Source == metaNames[1]).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.Source == metaNames[1]).Count();                            
                            if (usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                _MetadataStag.Sources.Remove(metaNames[1]);
                                var changedComaprisonMetaObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaObj);
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);
                        }
                        break;
                    case "leve":
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.Level == metaNames[1]).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.Level == metaNames[1]).Count();
                            if (usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                _MetadataStag.Levels.Remove(metaNames[1]);
                                var changedComaprisonMetaObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaObj);
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);

                        }
                        break;
                    case "stat":
                        if (treeViewCompareJsonMetadata.SelectedNode.BackColor == Color.LightGreen)
                        {
                            usingStagingPlaylistCount = _PlaylistListStag.Where(p => p.StatusTag == metaNames[1]).Count();
                            usingStagingAssetCount = _AssetsListStag.Where(p => p.StatusTag == metaNames[1]).Count();
                            if (usingStagingPlaylistCount == 0 && usingStagingAssetCount == 0)
                            {
                                _MetadataStag.StatusTag.Remove(metaNames[1]);
                                var changedComaprisonMetaObj = _ChangedMetadataTags.FirstOrDefault(o => o.Tag == treeViewCompareJsonMetadata.SelectedNode.Tag.ToString());
                                _ChangedMetadataTags.Remove(changedComaprisonMetaObj);
                                treeViewCompareJsonMetadata.Nodes.Remove(treeViewCompareJsonMetadata.SelectedNode);
                            }
                            else
                                MessageBox.Show(String.Format(Properties.Resources.MsgMetadatainUseCannotReject, usingStagingPlaylistCount, usingStagingAssetCount), Properties.Resources.TitleError);

                        }
                        break;
                }
                fnUpdatePendingItemsCount();
            }
            catch { }
        }

        private void fnUpdatePendingItemsCount()
        {
            int pendingPlaylistCount = 0;
            int pendingMetaCount = 0;
            try { pendingPlaylistCount = _ChangedPlaylistTags.Where(p => p.IsAcceptedorRejected == false).Count(); }
            catch { }
            try { pendingMetaCount = _ChangedMetadataTags.Where(p => p.IsAcceptedorRejected == false).Count(); }
            catch { }
            label_PendindPlaylistCount.Text = pendingPlaylistCount.ToString();
            label_PendingMetadataCount.Text = pendingMetaCount.ToString();
        }

        private void fnManageAcceptRejectAllButtons()
        {
            int pendingPlaylistCount = 0;
            int pendingMetaCount = 0;
            try { pendingPlaylistCount = _ChangedPlaylistTags.Where(p => p.IsAcceptedorRejected == false).Count(); }
            catch { }
            try { pendingMetaCount = _ChangedMetadataTags.Where(p => p.IsAcceptedorRejected == false).Count(); }
            catch { }
            if(!(pendingPlaylistCount == 0 && pendingMetaCount == 0))
            {
                button_AcceptAll.Enabled = true;
                button_RejectAll.Enabled = true;
            }
            else
            {
                button_AcceptAll.Enabled = false;
                button_RejectAll.Enabled = false;
            }
        }
            
        private void fnSetIsAcceptedorRejectedtoTrueforAll()
        {
            foreach (ProductionComparisonModel _prodComModel in _ChangedMetadataTags)
                _prodComModel.IsAcceptedorRejected = true;
            foreach (ProductionComparisonModel _prodComModel in _ChangedPlaylistTags)
                _prodComModel.IsAcceptedorRejected = true;
        }

        private void fnChangeNodeIconsonAcceptAll()
        {
            try
            {
                foreach (ProductionComparisonModel _prodComModel in _ChangedMetadataTags)
                {
                    TreeNode treeNode = fnFindMetadataTreeNodefromTag(_prodComModel.Tag);
                    treeNode.ImageIndex = 6;
                }
                foreach (ProductionComparisonModel _prodComModel in _ChangedPlaylistTags)
                {
                    string type = _prodComModel.Tag.Split(',')[0];
                    if (type == "playlist")
                    {
                        TreeNode treeNode = fnFindPlaylistTreeNodefromTag(_prodComModel.Tag);
                        treeNode.ImageIndex = 6;
                    }
                    else
                    {
                        TreeNode treeNode = fnFindPlaylistTreeNodefromTag(_prodComModel.Tag);
                        treeNode.ImageIndex = 6;
                    }                    
                }
            }
            catch(Exception ex) { }
        }

        private void fnChangeNodeIconsonRejectAll()
        {
            try
            {
                foreach (ProductionComparisonModel _prodComModel in _ChangedMetadataTags)
                {
                    TreeNode treeNode = fnFindMetadataTreeNodefromTag(_prodComModel.Tag);
                    treeNode.ImageIndex = 7;
                }
                foreach (ProductionComparisonModel _prodComModel in _ChangedPlaylistTags)
                {
                    string type = _prodComModel.Tag.Split(',')[0];
                    if (type == "playlist")
                    {
                        TreeNode treeNode = fnFindPlaylistTreeNodefromTag(_prodComModel.Tag);
                        treeNode.ImageIndex = 7;
                    }
                    else
                    {
                        TreeNode treeNode = fnFindPlaylistTreeNodefromTag(_prodComModel.Tag);
                        treeNode.ImageIndex = 7;
                    }
                }
            }
            catch (Exception ex) { }
        }
            
        #endregion        
    }
}
