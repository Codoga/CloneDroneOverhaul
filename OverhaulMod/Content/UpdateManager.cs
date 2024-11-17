using ICSharpCode.SharpZipLib.Zip;
using InternalModBot;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OverhaulMod.Content
{
    public class UpdateManager : Singleton<UpdateManager>
    {
        public const string REPOSITORY_FILE = "UpdateInfo.json";

        [ModSetting(ModSettingsConstants.CHECK_FOR_UPDATES_ON_STARTUP, true)]
        public static bool CheckForUpdatesOnStartup;

        [ModSetting(ModSettingsConstants.NOTIFY_ABOUT_NEW_VERSION_FROM_BRANCH, UpdateInfoList.RELEASE_BRANCH)]
        public static string NotifyAboutNewVersionFromBranch;

        [ModSetting(ModSettingsConstants.SAVED_NEW_VERSION, null, ModSetting.Tag.IgnoreExport)]
        public static string SavedNewVersion;

        [ModSetting(ModSettingsConstants.SAVED_NEW_VERSION_BRANCH, null, ModSetting.Tag.IgnoreExport)]
        public static string SavedNewVersionBranch;

        [ModSetting(ModSettingsConstants.UPDATES_LAST_CHECKED_DATE, null, ModSetting.Tag.IgnoreExport)]
        public static string UpdatesLastCheckedDate;

        public static readonly List<Dropdown.OptionData> BranchesForTestersDropdownOptions = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData() { text = "Release" },
            new Dropdown.OptionData() { text = "Testing" },
            new Dropdown.OptionData() { text = "Canary" },
        };

        public static readonly List<Dropdown.OptionData> BranchesDropdownOptions = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData() { text = "Release" },
            new Dropdown.OptionData() { text = "Testing" },
        };

        public static System.Version downloadedVersion
        {
            get;
            set;
        }

        private readonly UpdateInfoList m_downloadedUpdateInfoList;

        private void Start()
        {
            if (!CheckForUpdatesOnStartup)
            {
                downloadedVersion = ModBuildInfo.version;
                return;
            }

            ScheduledActionsManager scheduledActionsManager = ScheduledActionsManager.Instance;
            if (!scheduledActionsManager.ShouldExecuteAction(ScheduledActionType.RefreshModUpdates))
                return;

            _ = ModActionUtils.RunCoroutine(retrieveDataOnStartCoroutine());
        }

        private IEnumerator retrieveDataOnStartCoroutine()
        {
            DownloadUpdateInfoFile(delegate
            {
                ScheduledActionsManager.Instance.SetActionExecuted(ScheduledActionType.RefreshModUpdates);
            }, null);
            yield break;
        }

        public bool ShouldHighlightUpdatesButton()
        {
            return downloadedVersion > ModBuildInfo.version;
        }

        public void DownloadUpdateInfoFile(Action<UpdateInfoList> callback, Action<string> errorCallback)
        {
            RepositoryManager.Instance.GetTextFile(REPOSITORY_FILE, delegate (string content)
            {
                UpdateInfoList updateInfoList = null;
                try
                {
                    updateInfoList = ModJsonUtils.Deserialize<UpdateInfoList>(content);
                    updateInfoList.FixValues();

                    string n = NotifyAboutNewVersionFromBranch;
                    if (!n.IsNullOrEmpty())
                    {
                        if (updateInfoList.Branches.ContainsKey(n))
                        {
                            UpdateInfo updateInfo = updateInfoList.Branches[n];
                            if (updateInfo.CanBeInstalledByLocalUser() && updateInfo.IsNewerBuild())
                            {
                                ModSettingsManager.SetStringValue(ModSettingsConstants.SAVED_NEW_VERSION, updateInfo.ModVersion.ToString());
                                ModSettingsManager.SetStringValue(ModSettingsConstants.SAVED_NEW_VERSION_BRANCH, n);
                            }
                        }
                    }
                    ModSettingsManager.SetStringValue(ModSettingsConstants.UPDATES_LAST_CHECKED_DATE, DateTime.Now.ToString());
                    ModSettingsDataManager.Instance.Save();
                }
                catch (Exception exc)
                {
                    errorCallback?.Invoke(exc.ToString());
                    return;
                }
                callback?.Invoke(updateInfoList);
            }, errorCallback, out _);
        }

        public void DownloadBuildFromSource(string source, string directoryName, Action callback, Action<string> errorCallback, out UnityWebRequest unityWebRequest)
        {
            unityWebRequest = null;
            string directoryPath = Path.Combine(ModsManager.Instance.ModFolderPath, directoryName);
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.Delete(directoryPath, true);
                }
                catch (Exception exc)
                {
                    errorCallback?.Invoke(exc.ToString());
                    return;
                }
            }

            RepositoryManager.Instance.GetCustomFile(source, delegate (byte[] bytes)
            {
                string tempFile = Path.GetTempFileName();
                ModFileUtils.WriteBytes(bytes, tempFile);

                FastZip fastZip = new FastZip();
                fastZip.ExtractZip(tempFile, directoryPath, null);
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                try
                {
                    prepareCurrentBuildForAnUpdate();
                }
                catch (Exception exc)
                {
                    errorCallback?.Invoke("Prepare build error: " + exc);
                    return;
                }

                callback?.Invoke();
            }, errorCallback, out unityWebRequest, -1);
        }

        public string GetBranchDescription(int index)
        {
            switch (index)
            {
                case 0:
                    return LocalizationManager.Instance.GetTranslatedString("update_branch_release_description");
                case 1:
                    return LocalizationManager.Instance.GetTranslatedString("update_branch_testing_description");
                case 2:
                    return LocalizationManager.Instance.GetTranslatedString("update_branch_canary_description");
            }
            return LocalizationManager.Instance.GetTranslatedString("update_branch_unknown_description");
        }

        public List<Dropdown.OptionData> GetAvailableBranches()
        {
            return ExclusiveContentManager.Instance.IsLocalUserTheTester() ? BranchesForTestersDropdownOptions : BranchesDropdownOptions;
        }

        private void prepareCurrentBuildForAnUpdate()
        {
            string modFolder = ModCore.instance.ModInfo.FolderPath;
            string filePath = Path.Combine(modFolder, "ModInfo.json");
            string backupFilePath = Path.Combine(modFolder, "ModInfo.json.bak");
            if (File.Exists(filePath))
            {
                if (File.Exists(backupFilePath))
                    File.Delete(backupFilePath);

                File.Move(filePath, backupFilePath);
            }
        }
    }
}
