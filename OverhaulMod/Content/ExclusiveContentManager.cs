using OverhaulMod.Engine;
using OverhaulMod.Utils;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Content
{
    public class ExclusiveContentManager : Singleton<ExclusiveContentManager>
    {
        public const string REPOSITORY_FILE = "ExclusiveContentInfoList.json";

        public const string CONTENT_REFRESHED_EVENT = "ExclusiveContentRefreshed";

        public ExclusiveContentInfoList contentInfoList
        {
            get;
            private set;
        }

        public string error
        {
            get;
            private set;
        }

        public ulong localSteamId
        {
            get
            {
                return contentInfoList == null ? 0 : contentInfoList.LocalSteamID;
            }
        }

        public string localPlayFabId
        {
            get
            {
                return contentInfoList == null ? string.Empty : contentInfoList.LocalPlayFabID;
            }
        }

        public bool isRetrievingData
        {
            get;
            private set;
        }

        public override void Awake()
        {
            base.Awake();
            _ = ModActionUtils.RunCoroutine(retrieveDataOnStartCoroutine());
        }

        private IEnumerator retrieveDataOnStartCoroutine()
        {
            yield return null;
            RetrieveData(null, null, true); // load the local file

            while (!MultiplayerLoginManager.Instance.IsLoggedIntoPlayfab())
                yield return null;

            ScheduledActionsManager scheduledActionsManager = ScheduledActionsManager.Instance;
            if (!scheduledActionsManager.ShouldExecuteAction(ScheduledActionType.RefreshExclusivePerks))
                yield break;

            RetrieveData(delegate
            {
                scheduledActionsManager.SetActionExecuted(ScheduledActionType.RefreshExclusivePerks);
            }, delegate (string error)
            {
                this.error = error;
            }, false);
            yield break;
        }

        public void RetrieveData(Action doneCallback, Action<string> errorCallback, bool getInfoFromDisk = false)
        {
            isRetrievingData = true;
            contentInfoList = null;
            error = null;

            MultiplayerLoginManager loginManager = MultiplayerLoginManager.Instance;
            if (!loginManager || !loginManager.IsLoggedIntoPlayfab() || loginManager.IsBanned())
                getInfoFromDisk = true;

            if (getInfoFromDisk)
            {
                ModDataManager dataManager = ModDataManager.Instance;
                if (dataManager.FileExists(REPOSITORY_FILE, false))
                {
                    ExclusiveContentInfoList contentInfoList = null;
                    try
                    {
                        contentInfoList = dataManager.DeserializeFile<ExclusiveContentInfoList>(REPOSITORY_FILE, false);
                    }
                    catch (Exception exc)
                    {
                        isRetrievingData = false;
                        string s = exc.ToString();
                        error = s;
                        errorCallback?.Invoke(s);
                        return;
                    }
                    isRetrievingData = false;
                    this.contentInfoList = contentInfoList;
                    doneCallback?.Invoke();
                    GlobalEventManager.Instance.Dispatch(CONTENT_REFRESHED_EVENT);
                    ModFeatures.CacheValues();
                }
                isRetrievingData = false;
                return;
            }

            RepositoryManager repositoryManager = RepositoryManager.Instance;
            repositoryManager.GetTextFile(REPOSITORY_FILE, delegate (string contents)
            {
                ExclusiveContentInfoList contentInfoList = null;
                try
                {
                    contentInfoList = ModJsonUtils.Deserialize<ExclusiveContentInfoList>(contents);
                    contentInfoList.LocalSteamID = (ulong)SteamUser.GetSteamID();
                    contentInfoList.LocalPlayFabID = loginManager.GetLocalPlayFabID();
                    if (contentInfoList.List == null)
                        contentInfoList.List = new List<ExclusiveContentInfo>();

                    ExclusiveContentEditor.Save(contents);
                }
                catch (Exception exc)
                {
                    isRetrievingData = false;
                    string s = exc.ToString();
                    error = s;
                    errorCallback?.Invoke(s);
                    return;
                }
                isRetrievingData = false;
                this.contentInfoList = contentInfoList;
                doneCallback?.Invoke();
                ModFeatures.CacheValues();
            }, delegate (string error)
            {
                isRetrievingData = false;
                this.error = error;
                errorCallback?.Invoke(error);
            }, out _, 20);
        }

        public List<ExclusiveContentInfo> GetAllUnlockedContent()
        {
            List<ExclusiveContentInfo> list = new List<ExclusiveContentInfo>();
            if (contentInfoList != null && contentInfoList.List != null && contentInfoList.List.Count != 0)
                foreach (ExclusiveContentInfo info in contentInfoList.List)
                {
                    info.VerifyFields();
                    if (info.IsAvailable())
                        list.Add(info);
                }

            return list;
        }

        public List<ExclusiveContentInfo> GetAllContentOfType<T>() where T : ExclusiveContentBase
        {
            List<ExclusiveContentInfo> list = new List<ExclusiveContentInfo>();
            if (contentInfoList != null && contentInfoList.List != null && contentInfoList.List.Count != 0)
                foreach (ExclusiveContentInfo info in contentInfoList.List)
                {
                    info.VerifyFields();
                    if (info.Content != null && info.Content.GetType() == typeof(T))
                        list.Add(info);
                }

            return list;
        }

        public List<ExclusiveContentInfo> GetAllUnlockedContentOfType<T>() where T : ExclusiveContentBase
        {
            List<ExclusiveContentInfo> list = new List<ExclusiveContentInfo>();
            if (contentInfoList != null && contentInfoList.List != null && contentInfoList.List.Count != 0)
                foreach (ExclusiveContentInfo info in contentInfoList.List)
                {
                    info.VerifyFields();
                    if (info.IsAvailable() && info.Content != null && info.Content.GetType() == typeof(T))
                        list.Add(info);
                }

            return list;
        }

        public void GetOverrideRobotColor(FirstPersonMover firstPersonMover, Color currentColor, out Color newColor)
        {
            newColor = currentColor;
            foreach (ExclusiveContentInfo exclusiveContentInfo in GetAllContentOfType<ExclusiveContentColorOverride>())
                if (exclusiveContentInfo.Content is ExclusiveContentColorOverride colorOv && colorOv.GetOverrideRobotColor(firstPersonMover, currentColor, out newColor))
                    break;
        }

        public bool IsFeatureUnlocked(ModFeatures.FeatureType featureType)
        {
            foreach (ExclusiveContentInfo content in GetAllUnlockedContentOfType<ExclusiveContentFeatureUnlock>())
                if ((content.Content as ExclusiveContentFeatureUnlock).Feature == featureType)
                    return true;

            return false;
        }

        public bool IsLocalUserTheTester()
        {
            return !GetAllUnlockedContentOfType<ExclusiveContentTesterRole>().IsNullOrEmpty();
        }

        public bool IsLocalUserAbleToVerifyItems()
        {
            return !GetAllUnlockedContentOfType<ExclusiveContentItemsVerifierRole>().IsNullOrEmpty();
        }
    }
}
