using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CLO365
{
    class GITOperations
    {
        public GenericStatus fnCloneGITRepository(string path)
        {
            GenericStatus _GenericStatus = new GenericStatus();
            try
            {
                var cloneOptions = new CloneOptions { BranchName = "master", Checkout = true };
                cloneOptions.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = Utility.fnConvertFromBase64String(GetSetConfig.gitPat), Password = "" };
                var cloneResult = Repository.Clone(GetSetConfig.gitRepoUrl, path, cloneOptions);
                _GenericStatus.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _GenericStatus.Exception = ex;
            }
            return _GenericStatus;
        }

        public GenericStatus fnCloneGITProductionRepository()
        {
            GenericStatus _GenericStatus = new GenericStatus();
            SessionData _sessionData = new SessionData();
            try
            {
                var cloneOptions = new CloneOptions { BranchName = "master", Checkout = true };
                cloneOptions.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = Utility.fnConvertFromBase64String(GetSetConfig.gitPat), Password = "" };
                var cloneResult = Repository.Clone(GetSetConfig.gitProdRepoUrl, _sessionData.ProductionLocalRepositoryPath, cloneOptions);
                _GenericStatus.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _GenericStatus.Exception = ex;
            }
            return _GenericStatus;
        }

        public bool fnPullFiles()
        {
            bool blnStatus = false;
            SessionData objSessionData = null;

            try
            {
                objSessionData = new SessionData();
                string strUserName = objSessionData.UserName;
                using (var objRepository = new Repository(objSessionData.StagingLocalRepositoryPath))
                {
                    LibGit2Sharp.PullOptions options = new LibGit2Sharp.PullOptions();
                    options.FetchOptions = new FetchOptions();
                    options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = Utility.fnConvertFromBase64String(GetSetConfig.gitPat),
                            Password = ""
                        });

                    var signature = new LibGit2Sharp.Signature(
                        new Identity(strUserName, strUserName), DateTimeOffset.Now);

                    Commands.Pull(objRepository, signature, options);
                    blnStatus = true;
                }
            }
            catch (Exception ex)
            {
                blnStatus = false;
            }
            finally
            {
                objSessionData = null;
            }
            return blnStatus;
        }

        public bool fnPullProductionFiles()
        {
            bool blnStatus = false;
            SessionData objSessionData = null;

            try
            {
                objSessionData = new SessionData();
                string strUserName = objSessionData.UserName;
                using (var objRepository = new Repository(objSessionData.ProductionLocalRepositoryPath))
                {
                    LibGit2Sharp.PullOptions options = new LibGit2Sharp.PullOptions();
                    options.FetchOptions = new FetchOptions();
                    options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = Utility.fnConvertFromBase64String(GetSetConfig.gitPat),
                            Password = ""
                        });
                    var signature = new LibGit2Sharp.Signature(
                        new Identity(strUserName, strUserName), DateTimeOffset.Now);

                    Commands.Pull(objRepository, signature, options);
                    blnStatus = true;
                }
            }
            catch (Exception ex)
            {
                blnStatus = false;
            }
            finally
            {
                objSessionData = null;
            }
            return blnStatus;
        }

        public bool fnCommitFiles(string Message)
        {
            bool blnCommitStatus = false;
            string strUserName = string.Empty;
            string strLocalRepositoryPath = string.Empty;
            SessionData objSessionData = null;

            try
            {
                objSessionData = new SessionData();
                if (string.IsNullOrEmpty(Message))
                    Message = Properties.Resources.MsgAutoCommitMsg + objSessionData.UserName + " on: " + DateTime.Now.ToString();
                objSessionData = new SessionData();
                strUserName = objSessionData.UserName;
                strLocalRepositoryPath = objSessionData.StagingLocalRepositoryPath;
                using (var _LocalRepoPath = new Repository(strLocalRepositoryPath))
                {
                    Commands.Stage(_LocalRepoPath, Path.Combine(strLocalRepositoryPath, GetSetConfig.gitFolderName));
                    _LocalRepoPath.Commit(Message, new Signature(strUserName, strUserName, DateTimeOffset.Now),
                    new LibGit2Sharp.Signature(new Identity(strUserName, strUserName), DateTimeOffset.Now));
                    blnCommitStatus = true;
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                strUserName = null;
                strLocalRepositoryPath = null;
                objSessionData = null;
            }

            return blnCommitStatus;
        }

        public bool fnCommitProductionFiles(string Message)
        {
            bool blnCommitStatus = false;
            string strUserName = string.Empty;
            string strLocalRepositoryPath = string.Empty;
            SessionData objSessionData = null;

            try
            {
                objSessionData = new SessionData();
                if (string.IsNullOrEmpty(Message))
                    Message = Properties.Resources.MsgAutoCommitMsg + objSessionData.UserName + " on: " + DateTime.Now.ToString();
                objSessionData = new SessionData();
                strUserName = objSessionData.UserName;
                strLocalRepositoryPath = objSessionData.ProductionLocalRepositoryPath;
                using (var _LocalRepoPath = new Repository(strLocalRepositoryPath))
                {
                    Commands.Stage(_LocalRepoPath, Path.Combine(strLocalRepositoryPath, GetSetConfig.gitFolderName));
                    _LocalRepoPath.Commit(Message, new Signature(strUserName, strUserName, DateTimeOffset.Now),
                    new LibGit2Sharp.Signature(new Identity(strUserName, strUserName), DateTimeOffset.Now));
                    blnCommitStatus = true;
                }
            }
            catch (Exception ex) { }
            finally
            {
                strUserName = null;
                strLocalRepositoryPath = null;
                objSessionData = null;
            }

            return blnCommitStatus;
        }

        public bool fnIsLocalRepoHasUncommittedChanges()
        {
            bool blnStatus = false;
            SessionData objUser = null;
            try
            {
                objUser = new SessionData();
                using (var repo = new Repository(objUser.StagingLocalRepositoryPath))
                {
                    RepositoryStatus status = repo.RetrieveStatus();
                    blnStatus = status.IsDirty;
                }
            }
            catch (Exception ex)
            {

            }
            return blnStatus;
        }

        public bool fnIsProductionLocalRepoHasUncommittedChanges()
        {
            bool blnStatus = false;
            SessionData objUser = null;
            try
            {
                objUser = new SessionData();
                using (var repo = new Repository(objUser.ProductionLocalRepositoryPath))
                {
                    RepositoryStatus status = repo.RetrieveStatus();
                    blnStatus = status.IsDirty;
                }
            }
            catch (Exception ex)
            {

            }
            return blnStatus;
        }

        public bool fnIsaValidRepo(string repoPath)
        {
            bool blnStatus = false;
            try
            {
                blnStatus = Repository.IsValid(repoPath);
            }
            catch (Exception ex)
            {

            }
            return blnStatus;
        }

        public GenericStatus fnPushFiles()
        {
            SessionData objSessionData = null;
            GenericStatus objGenericStatus = null;
            string strCommitMessage = string.Empty;

            try
            {
                objSessionData = new SessionData();
                objGenericStatus = new GenericStatus();
                bool blnIsLocalRepoHasUncommittedChanges = fnIsLocalRepoHasUncommittedChanges();
                if (blnIsLocalRepoHasUncommittedChanges)
                {
                    strCommitMessage = Properties.Resources.MsgAutoCommitMsg + objSessionData.UserName + " on: " + DateTime.Now.ToString();
                    fnCommitFiles(strCommitMessage);
                }

                using (var objRepository = new Repository(objSessionData.StagingLocalRepositoryPath))
                {
                    LibGit2Sharp.PushOptions options = new LibGit2Sharp.PushOptions();
                    options.CredentialsProvider = new CredentialsHandler(
                        (_url, _user, _cred) =>
                            new UsernamePasswordCredentials()
                            { 
                                Username = Utility.fnConvertFromBase64String(GetSetConfig.gitPat),
                                Password = ""
                            });
                    objRepository.Network.Push(objRepository.Branches["master"], options);
                    objGenericStatus.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                objGenericStatus.Exception = ex;
            }
            finally
            {
                objSessionData = null;
            }

            return objGenericStatus;
        }


        public GenericStatus fnPublishToProduction()
        {
            SessionData objSessionData = null;
            GenericStatus objGenericStatus = null;
            string strCommitMessage = string.Empty;
            try
            {
                objSessionData = new SessionData();
                objGenericStatus = new GenericStatus();

                #region oldPushtoProdOperation
                //using (var objRepository = new Repository(objSessionData.LocalRepositoryPath))
                //{
                //    var prodBrach = objRepository.Network.Remotes[GetSetConfig.gitProdRemoteName];
                //    if (prodBrach == null)
                //    {
                //        Remote remote = objRepository.Network.Remotes[GetSetConfig.gitProdRemoteName];
                //        Remote updatedremote = objRepository.Network.Remotes.Add(GetSetConfig.gitProdRemoteName, GetSetConfig.gitProdRepoUrl);
                //    }
                //}

                //using (var repo = new Repository(objSessionData.LocalRepositoryPath))
                //{                    
                //    Remote remote = repo.Network.Remotes[GetSetConfig.gitProdRemoteName];
                //    var options = new PushOptions();
                //    options.CredentialsProvider = new CredentialsHandler(
                //        (_url, _user, _cred) =>
                //            new UsernamePasswordCredentials()
                //            {
                //                Username = Utility.fnConvertFromBase64String(GetSetConfig.gitPat),
                //                Password = ""
                //            });
                //    repo.Network.Push(remote, @"+refs/heads/master", options);
                //    objGenericStatus.IsSuccess = true;
                //}
                #endregion

                using (var objRepository = new Repository(objSessionData.ProductionLocalRepositoryPath))
                {
                    LibGit2Sharp.PushOptions options = new LibGit2Sharp.PushOptions();
                    options.CredentialsProvider = new CredentialsHandler(
                        (_url, _user, _cred) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = Utility.fnConvertFromBase64String(GetSetConfig.gitPat),
                                Password = ""
                            });
                    objRepository.Network.Push(objRepository.Branches["master"], options);
                    objGenericStatus.IsSuccess = true;
                }

            }
            catch (Exception ex) { objGenericStatus.Exception = ex; }
            finally
            {
                objSessionData = null;
            }
            return objGenericStatus;
        }
    }

    public class GenericStatus
    {
        public GenericStatus()
        {
            this.Exception = null;
            this.IsSuccess = false;
        }
        public Exception Exception { get; set; }
        public bool IsSuccess { get; set; }

    }
}
