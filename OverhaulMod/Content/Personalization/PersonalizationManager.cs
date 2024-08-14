using ICSharpCode.SharpZipLib.Zip;
using OverhaulMod.Combat;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationManager : Singleton<PersonalizationManager>, IGameLoadListener
    {
        public const string ASSETS_VERSION_FILE = "customizationAssetsInfo.json";

        public const string REMOTE_ASSETS_VERSION_FILE = "customizationAssetsInfo_remote.json";

        public const string CUSTOMIZATION_ASSETS_FILE_DOWNLOADED_EVENT = "CustomizationAssetsFileDownloaded";

        public const string ITEM_EQUIPPED_OR_UNEQUIPPED_EVENT = "PersonalizationItemEquippedOrUnequipped";

        public const string USER_INFO_FILE = "PersonalizationUserInfo.json";

        public static readonly string[] SupportedBodyParts = new string[]
        {
            "Head",
            "Torso",
            "Spine",
            "ArmUpperR",
            "ArmLowerR",
            "HandR",
            "ArmUpperL",
            "ArmLowerL",
            "HandL",
            "LegUpperR",
            "LegLowerR",
            "FootR",
            "LegUpperL",
            "LegLowerL",
            "FootL",
        };

        private string m_assetsVersionFile;
        public string assetsVersionFile
        {
            get
            {
                if (m_assetsVersionFile == null)
                {
                    m_assetsVersionFile = Path.Combine(ModCore.customizationFolder, ASSETS_VERSION_FILE);
                }
                return m_assetsVersionFile;
            }
        }

        private string m_remoteAssetsVersionFile;
        public string remoteAssetsVersionFile
        {
            get
            {
                if (m_remoteAssetsVersionFile == null)
                {
                    m_remoteAssetsVersionFile = Path.Combine(ModCore.customizationFolder, REMOTE_ASSETS_VERSION_FILE);
                }
                return m_remoteAssetsVersionFile;
            }
        }

        public PersonalizationAssetsInfo localAssetsInfo { get; set; }

        public PersonalizationAssetsInfo remoteAssetsInfo { get; set; }

        public PersonalizationItemList itemList
        {
            get;
            private set;
        }

        public PersonalizationUserInfo userInfo
        {
            get;
            private set;
        }

        private UnityWebRequest m_webRequest;

        public override void Awake()
        {
            base.Awake();

            PersonalizationItemList personalizationItemList = new PersonalizationItemList();
            personalizationItemList.Load();
            itemList = personalizationItemList;

            loadUserInfoFile();
        }

        private void Start()
        {
            LoadLocalCustomizationAssetsVersion();

            ScheduledActionsManager scheduledActionsManager = ScheduledActionsManager.Instance;
            if (!scheduledActionsManager.ShouldExecuteAction(ScheduledActionType.RefreshExclusivePerks))
                LoadRemoteCustomizationAssetsVersion();
            else
                RefreshRemoteCustomizationAssetsVersion(null);
        }

        public void OnGameLoaded()
        {
            PersonalizationUserInfo userInfo = this.userInfo;
            if (userInfo != null)
            {
                userInfo.RefreshAllItemsVerification();
                SaveUserInfo();
            }
        }

        public void DownloadCustomizationFile(Action<bool> callback)
        {
            _ = downloadCustomizationFileCoroutine(callback).Run();
        }

        private IEnumerator downloadCustomizationFileCoroutine(Action<bool> callback)
        {
            RepositoryManager.Instance.GetCustomFile($"https://github.com/aTVCat/Overhaul-Mod-Content/raw/main/content/customization.zip", delegate (byte[] bytes)
            {
                m_webRequest = null;
                try
                {
                    if (!Directory.Exists(ModCore.customizationFolder))
                    {
                        _ = Directory.CreateDirectory(ModCore.customizationFolder);
                    }
                    else
                    {
                        foreach (string folder in Directory.GetDirectories(ModCore.customizationFolder))
                        {
                            Directory.Delete(folder, true);
                        }
                    }
                    /*
                    else
                    {
                        foreach (var directory in Directory.GetDirectories(ModCore.customizationFolder))
                        {
                            Directory.Delete(directory, true);
                        }
                    }*/

                    string tempFile = Path.GetTempFileName();
                    ModIOUtils.WriteBytes(bytes, tempFile);

                    FastZip fastZip = new FastZip();
                    fastZip.ExtractZip(tempFile, ModCore.customizationFolder, null);

                    if (remoteAssetsInfo != null)
                    {
                        ModJsonUtils.WriteStream(assetsVersionFile, remoteAssetsInfo);
                        localAssetsInfo = remoteAssetsInfo;
                    }
                    else
                    {
                        RefreshRemoteCustomizationAssetsVersion(delegate (bool result)
                        {
                            if (result)
                            {
                                ModJsonUtils.WriteStream(assetsVersionFile, remoteAssetsInfo);
                                localAssetsInfo = remoteAssetsInfo;
                            }
                        });
                    }

                    itemList.Load();
                    GlobalEventManager.Instance.Dispatch(CUSTOMIZATION_ASSETS_FILE_DOWNLOADED_EVENT);
                }
                catch (Exception)
                {
                    callback?.Invoke(false);
                    return;
                }
                callback?.Invoke(true);
            }, delegate
            {
                m_webRequest = null;
                callback?.Invoke(false);
            }, out UnityWebRequest unityWebRequest, -1);
            m_webRequest = unityWebRequest;
            yield break;
        }

        public float GetCustomizationFileDownloadProgress()
        {
            UnityWebRequest unityWebRequest = m_webRequest;
            if (unityWebRequest == null)
                return 0f;

            try
            {
                return unityWebRequest.downloadProgress;
            }
            catch
            {
                return 0;
            }
        }

        public void LoadLocalCustomizationAssetsVersion()
        {
            string path = assetsVersionFile;
            if (!File.Exists(path))
                localAssetsInfo = null;
            else
            {
                PersonalizationAssetsInfo personalizationAssetsInfo;
                try
                {
                    personalizationAssetsInfo = ModJsonUtils.DeserializeStream<PersonalizationAssetsInfo>(path);
                    personalizationAssetsInfo.FixValues();
                }
                catch
                {
                    personalizationAssetsInfo = new PersonalizationAssetsInfo();
                    personalizationAssetsInfo.FixValues();
                }
                localAssetsInfo = personalizationAssetsInfo;
            }
        }

        public void LoadRemoteCustomizationAssetsVersion()
        {
            string path = remoteAssetsVersionFile;
            if (!File.Exists(path))
                remoteAssetsInfo = null;
            else
            {
                PersonalizationAssetsInfo personalizationAssetsInfo;
                try
                {
                    personalizationAssetsInfo = ModJsonUtils.DeserializeStream<PersonalizationAssetsInfo>(path);
                    personalizationAssetsInfo.FixValues();
                }
                catch
                {
                    personalizationAssetsInfo = new PersonalizationAssetsInfo();
                    personalizationAssetsInfo.FixValues();
                }

                remoteAssetsInfo = personalizationAssetsInfo;
            }
        }

        public void RefreshRemoteCustomizationAssetsVersion(Action<bool> callback)
        {
            remoteAssetsInfo = null;

            ScheduledActionsManager scheduledActionsManager = ScheduledActionsManager.Instance;
            RepositoryManager.Instance.GetTextFile($"content/{ASSETS_VERSION_FILE}", delegate (string result)
            {
                PersonalizationAssetsInfo personalizationAssetsInfo;
                try
                {
                    personalizationAssetsInfo = ModJsonUtils.Deserialize<PersonalizationAssetsInfo>(result);
                    personalizationAssetsInfo.FixValues();
                }
                catch
                {
                    personalizationAssetsInfo = new PersonalizationAssetsInfo();
                    personalizationAssetsInfo.FixValues();
                }

                remoteAssetsInfo = personalizationAssetsInfo;
                ModJsonUtils.WriteStream(remoteAssetsVersionFile, personalizationAssetsInfo);
                scheduledActionsManager.SetActionExecuted(ScheduledActionType.RefreshCustomizationAssetsRemoteVersion);

                callback?.Invoke(true);
            }, delegate (string error)
            {
                callback?.Invoke(false);
            }, out _);
        }

        public PersonalizationAssetsState GetPersonalizationAssetsState()
        {
            PersonalizationAssetsInfo localInfo = localAssetsInfo;
            PersonalizationAssetsInfo remoteInfo = remoteAssetsInfo;
            if (localInfo == null)
                return PersonalizationAssetsState.NotInstalled;

            if (remoteInfo == null || remoteInfo.AssetsVersion <= localInfo.AssetsVersion)
                return PersonalizationAssetsState.Installed;

            return PersonalizationAssetsState.NeedUpdate;
        }

        public bool IsDownloadingCustomizationFile()
        {
            return m_webRequest != null;
        }

        public bool SetLocalAssetsVersion(string versionString)
        {
            if (!System.Version.TryParse(versionString, out System.Version version))
                return false;

            return SetLocalAssetsVersion(version);
        }

        public bool SetLocalAssetsVersion(System.Version version)
        {
            PersonalizationAssetsInfo personalizationAssetsInfo = localAssetsInfo;
            if (personalizationAssetsInfo == null)
            {
                personalizationAssetsInfo = new PersonalizationAssetsInfo
                {
                    AssetsVersion = version
                };
                personalizationAssetsInfo.FixValues();
                localAssetsInfo = personalizationAssetsInfo;
            }
            ModJsonUtils.WriteStream(assetsVersionFile, personalizationAssetsInfo);
            return true;
        }

        public System.Version GetLocalAssetsVersion()
        {
            return localAssetsInfo?.AssetsVersion;
        }

        private void loadUserInfoFile()
        {
            string path = Path.Combine(ModDataManager.userDataFolder, USER_INFO_FILE);

            PersonalizationUserInfo personalizationUserInfo;
            try
            {
                if (!File.Exists(path))
                {
                    personalizationUserInfo = new PersonalizationUserInfo();
                    personalizationUserInfo.FixValues();
                }
                else
                {
                    personalizationUserInfo = ModDataManager.Instance.DeserializeFile<PersonalizationUserInfo>(USER_INFO_FILE, false);
                    personalizationUserInfo.FixValues();
                }
            }
            catch
            {
                personalizationUserInfo = new PersonalizationUserInfo();
                personalizationUserInfo.FixValues();
            }

            /*if (personalizationUserInfo.DiscoveredItems.Count == 0)
                personalizationUserInfo.DiscoverAllItems();*/

            userInfo = personalizationUserInfo;
        }

        public void SaveUserInfo()
        {
            PersonalizationUserInfo personalizationUserInfo = userInfo;
            if (personalizationUserInfo != null)
            {
                ModDataManager.Instance.SerializeToFile(USER_INFO_FILE, personalizationUserInfo, false);
            }
        }

        public static bool IsWeaponCustomizationSupported(WeaponType weaponType)
        {
            return weaponType != WeaponType.None
&& (weaponType == WeaponType.Sword
                || weaponType == WeaponType.Bow
                || weaponType == WeaponType.Hammer
                || weaponType == WeaponType.Spear
                || weaponType == WeaponType.Shield
                || weaponType == ModWeaponsManager.SCYTHE_TYPE);
        }

        public void EquipItem(PersonalizationItemInfo item)
        {
            if (!item.IsCompatibleWithMod())
            {
                ModUIUtils.MessagePopupOK("Incompatible item!", $"This item is made for the new version of Overhaul mod.\nMake sure you're using the latest version of the mod.", 175f, true);
                return;
            }

            if (item.Category == PersonalizationCategory.WeaponSkins)
            {
                FirstPersonMover firstPersonMover = CharacterTracker.Instance.GetPlayerRobot();
                List<FirstPersonMover> clones = CloneManager.Instance._clones;

                List<FirstPersonMover> allPlayers = new List<FirstPersonMover>(clones);
                if (firstPersonMover)
                    allPlayers.Add(firstPersonMover);

                foreach (FirstPersonMover clone in allPlayers)
                {
                    if (!clone)
                        continue;

                    PersonalizationController personalizationController = clone.GetComponent<PersonalizationController>();
                    if (personalizationController)
                    {
                        personalizationController.EquipItem(item);
                    }
                }
            }
            else
            {

            }
        }

        public static void SetIsItemEquipped(PersonalizationItemInfo item, bool value)
        {
            string id = item.ItemID;
            switch (item.Category)
            {
                case PersonalizationCategory.WeaponSkins:
                    PersonalizationController.SetWeaponSkin(item.Weapon, id);
                    break;
            }

            GlobalEventManager.Instance.Dispatch(ITEM_EQUIPPED_OR_UNEQUIPPED_EVENT);
        }

        public static bool GetIsItemEquipped(PersonalizationItemInfo item)
        {
            string itemId = item.ItemID;
            switch (item.Category)
            {
                case PersonalizationCategory.WeaponSkins:
                    switch (item.Weapon)
                    {
                        case WeaponType.Sword:
                            return PersonalizationController.SwordSkin == itemId;
                        case WeaponType.Bow:
                            return PersonalizationController.BowSkin == itemId;
                        case WeaponType.Hammer:
                            return PersonalizationController.HammerSkin == itemId;
                        case WeaponType.Spear:
                            return PersonalizationController.SpearSkin == itemId;
                        case WeaponType.Shield:
                            return PersonalizationController.ShieldSkin == itemId;
                        case ModWeaponsManager.SCYTHE_TYPE:
                            return PersonalizationController.ScytheSkin == itemId;
                    }
                    return false;
            }
            return false;
        }
    }
}
