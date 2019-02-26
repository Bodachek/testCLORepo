using CLO365.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLO365
{ 
    public class JSONOperations
    {
        public bool fnLoadModelsFromJSON(ref List<PlaylistsModel> objPlaylistList, ref List<AssetsModel> objAssetsList, ref MetadataModel objMetadataModel)
        {
            bool blnStatus = false;
            string strAssets = string.Empty;
            string strMetadata = string.Empty;
            string strPlaylists = string.Empty;
            SessionData objSessionData = null;
            objSessionData = new SessionData();
            try
            {
                strAssets = File.ReadAllText(Path.Combine(objSessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.assetjsonFilename));
                strMetadata = File.ReadAllText(Path.Combine(objSessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.metadatajsonFilename));
                strPlaylists = File.ReadAllText(Path.Combine(objSessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.playlistjsonFilename));

                objAssetsList = JsonConvert.DeserializeObject<List<AssetsModel>>(strAssets);
                objMetadataModel = JsonConvert.DeserializeObject<MetadataModel>(strMetadata);
                objPlaylistList = JsonConvert.DeserializeObject<List<PlaylistsModel>>(strPlaylists);
                blnStatus = true;

            }
            catch (Exception ex) { blnStatus = false; }

            return blnStatus;
        }

        public bool fnLoadProductionModelsFromJSON(ref List<PlaylistsModel> objPlaylistList, ref List<AssetsModel> objAssetsList, ref MetadataModel objMetadataModel)
        {
            bool blnStatus = false;
            string strAssets = string.Empty;
            string strMetadata = string.Empty;
            string strPlaylists = string.Empty;
            SessionData objSessionData = null;
            objSessionData = new SessionData();
            try
            {
                strAssets = File.ReadAllText(Path.Combine(objSessionData.ProductionLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.assetjsonFilename));
                strMetadata = File.ReadAllText(Path.Combine(objSessionData.ProductionLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.metadatajsonFilename));
                strPlaylists = File.ReadAllText(Path.Combine(objSessionData.ProductionLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.playlistjsonFilename));

                objAssetsList = JsonConvert.DeserializeObject<List<AssetsModel>>(strAssets);
                objMetadataModel = JsonConvert.DeserializeObject<MetadataModel>(strMetadata);
                objPlaylistList = JsonConvert.DeserializeObject<List<PlaylistsModel>>(strPlaylists);
                blnStatus = true;

            }
            catch (Exception ex) { blnStatus = false; }

            return blnStatus;
        }

        public void fnSavePlaylistLocalRepoFile(List<PlaylistsModel> objPlaylistList)
        {
            string strPlayList = string.Empty;
            SessionData objSessionData = null;
            try
            {
                objSessionData = new SessionData();
                strPlayList = JsonConvert.SerializeObject(objPlaylistList, Formatting.Indented);
                File.WriteAllText(Path.Combine(objSessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.playlistjsonFilename), strPlayList);
            }
            catch (Exception ex) { }
            finally
            {
                strPlayList = null;
            }
        }

        public void fnSaveAssetLocalRepoFile(List<AssetsModel> objAssetsList)
        {
            string strAssetList = string.Empty;
            SessionData objSessionData = null;
            try
            {
                objSessionData = new SessionData();
                strAssetList = JsonConvert.SerializeObject(objAssetsList, Formatting.Indented);
                File.WriteAllText(Path.Combine(objSessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.assetjsonFilename), strAssetList);
            }
            catch (Exception ex) { }
            finally
            {
                strAssetList = null;
            }
        }

        public void fnSaveMetadataLocalRepoFile(MetadataModel objMetadataModel)
        {
            string strMetaList = string.Empty;
            SessionData objSessionData = null;
            try
            {
                objSessionData = new SessionData();
                strMetaList = JsonConvert.SerializeObject(objMetadataModel, Formatting.Indented);
                File.WriteAllText(Path.Combine(objSessionData.StagingLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.metadatajsonFilename), strMetaList);
            }
            catch (Exception ex) { }
            finally
            {
                strMetaList = null;
            }
        }


        public void fnSavePlaylistProductionLocalRepoFile(List<PlaylistsModel> objPlaylistList)
        {
            string strPlayList = string.Empty;
            SessionData objSessionData = null;
            try
            {
                objSessionData = new SessionData();
                strPlayList = JsonConvert.SerializeObject(objPlaylistList, Formatting.Indented);
                File.WriteAllText(Path.Combine(objSessionData.ProductionLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.playlistjsonFilename), strPlayList);
            }
            catch (Exception ex) { }
            finally
            {
                strPlayList = null;
            }
        }

        public void fnSaveAssetProductionLocalRepoFile(List<AssetsModel> objAssetsList)
        {
            string strAssetList = string.Empty;
            SessionData objSessionData = null;
            try
            {
                objSessionData = new SessionData();
                strAssetList = JsonConvert.SerializeObject(objAssetsList, Formatting.Indented);
                File.WriteAllText(Path.Combine(objSessionData.ProductionLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.assetjsonFilename), strAssetList);
            }
            catch (Exception ex) { }
            finally
            {
                strAssetList = null;
            }
        }

        public void fnSaveMetadataProductionLocalRepoFile(MetadataModel objMetadataModel)
        {
            string strMetaList = string.Empty;
            SessionData objSessionData = null;
            try
            {
                objSessionData = new SessionData();
                strMetaList = JsonConvert.SerializeObject(objMetadataModel, Formatting.Indented);
                File.WriteAllText(Path.Combine(objSessionData.ProductionLocalRepositoryPath, GetSetConfig.gitFolderName, GetSetConfig.metadatajsonFilename), strMetaList);
            }
            catch (Exception ex) { }
            finally
            {
                strMetaList = null;
            }
        }
    }
}
