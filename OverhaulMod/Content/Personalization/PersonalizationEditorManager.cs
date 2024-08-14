using ICSharpCode.SharpZipLib.Zip;
using OverhaulMod.Combat;
using OverhaulMod.Engine;
using OverhaulMod.UI;
using OverhaulMod.UI.Elements;
using OverhaulMod.Utils;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationEditorManager : Singleton<PersonalizationEditorManager>
    {
        public const string ITEM_INFO_FILE = "itemInfo.json";

        public const string ITEM_META_DATA_FILE = "metaData.json";

        public const string EDITOR_STARTED_EVENT = "PersonalizationEditorStarted";

        public const string OBJECT_EDITED_EVENT = "PersonalizationEditorObjectEdited";

        public const string PRESET_PREVIEW_CHANGED_EVENT = "PersonalizationEditorPresetPreviewChanged";

        public readonly GameData GameData = new GameData();

        private bool m_hasConfiguredGameData;

        private GreatSwordPreviewController m_greatSwordPreviewController;

        private FirstPersonMover m_bot;

        private PersonalizationEditorCamera m_camera;

        public PersonalizationController currentPersonalizationController
        {
            get;
            set;
        }

        public PersonalizationItemInfo currentEditingItemInfo
        {
            get;
            set;
        }

        public PersonalizationEditorObjectBehaviour currentEditingRoot
        {
            get;
            set;
        }

        public string currentEditingItemFolder
        {
            get
            {
                return currentEditingItemInfo.FolderPath;
            }
        }

        private bool m_originalModelsEnabled;
        public bool originalModelsEnabled
        {
            get
            {
                return m_originalModelsEnabled;
            }
            set
            {
                m_originalModelsEnabled = value;
                RefreshGreatswordPreview();
            }
        }

        private WeaponVariant m_previewPresetKey;
        public WeaponVariant previewPresetKey
        {
            get
            {
                return m_previewPresetKey;
            }
            set
            {
                m_previewPresetKey = value;
                RefreshGreatswordPreview();
            }
        }

        private string m_editorId;
        public string editorId
        {
            get
            {
                if (!SteamManager.Instance || !SteamManager.Instance.Initialized)
                    return null;

                if (m_editorId == null)
                {
                    m_editorId = SteamUser.GetSteamID().ToString();
                }
                return m_editorId;
            }
        }

        public bool canVerifyItems
        {
            get
            {
                return ExclusiveContentManager.Instance.IsLocalUserAbleToVerifyItems();
            }
        }

        public bool canEditNonOwnItems
        {
            get
            {
                return ExclusiveContentManager.Instance.IsLocalUserAbleToVerifyItems();
            }
        }

        public bool canEditItemSpecialInfo
        {
            get
            {
                return true;
            }
        }

        public static bool IsInEditor()
        {
            return GameFlowManager.Instance._gameMode == (GameMode)2500;
        }

        public void StartEditorGameMode(bool noTransition = false)
        {
            if (noTransition || !TransitionManager.OverhaulNonSceneTransitions)
            {
                _ = base.StartCoroutine(startEditorGameModeCoroutine(false));
                return;
            }
            TransitionManager.Instance.DoNonSceneTransition(startEditorGameModeCoroutine(true));
        }

        private IEnumerator startEditorGameModeCoroutine(bool useTransitionManager)
        {
            if (useTransitionManager)
                yield return new WaitForSecondsRealtime(0.25f);

            yield return null;

            if (!m_hasConfiguredGameData)
            {
                GameData gameData = GameData;
                gameData.HumanFacts = HumanFactsManager.Instance.GetRandomFactSet();
                gameData.PlayerUpgrades = new Dictionary<UpgradeType, int>()
                {
                    { UpgradeType.SwordUnlock, 1 },
                    { UpgradeType.BowUnlock, 1 },
                    { UpgradeType.Hammer, 3 },
                    { UpgradeType.SpearUnlock, 1 },
                    { UpgradeType.Dash, 1 },
                    { UpgradeType.EnergyCapacity, 2 },
                    { UpgradeType.EnergyRecharge, 2 },
                };
                m_hasConfiguredGameData = true;
            }

            currentEditingItemInfo = null;
            currentEditingRoot = null;
            previewPresetKey = WeaponVariant.Normal;
            originalModelsEnabled = false;

            GameFlowManager.Instance._gameMode = (GameMode)2500;

            LevelManager.Instance.CleanUpLevelThisFrame();
            GameFlowManager.Instance.HideTitleScreen(false);

            GameDataManager.Instance.SaveHighScoreDataWithoutModifyingIt();
            CacheManager.Instance.CreateOrClearInstance();
            GarbageManager.Instance.DestroyAllGarbage();

            PersonalizationEditorTemplateManager.Instance.LoadTemplates();

            SingleplayerServerStarter.Instance.StartServerThenCall(delegate
            {
                UIPersonalizationEditor editorUi = ModUIConstants.ShowPersonalizationEditorUI();

                LevelEditorLevelData levelEditorLevelData = null;
                try
                {
                    levelEditorLevelData = ModJsonUtils.DeserializeStream<LevelEditorLevelData>(Path.Combine(ModCore.dataFolder, "levels/personalizationEditorLevel.json"));
                }
                catch
                {
                }

                _ = base.StartCoroutine(spawnLevelCoroutine(useTransitionManager, levelEditorLevelData));
            });
            yield break;
        }

        public FirstPersonMover GetBot()
        {
            return m_bot;
        }

        public void RefreshGreatswordPreview()
        {
            if (m_greatSwordPreviewController)
                m_greatSwordPreviewController.SetPreviewActivate(originalModelsEnabled && (previewPresetKey == WeaponVariant.NormalMultiplayer || previewPresetKey == WeaponVariant.OnFireMultiplayer));
        }

        public List<Dropdown.OptionData> GetConditionOptions()
        {
            if (ModAdvancedCache.TryGet("DropdownShowConditionOptions", out List<Dropdown.OptionData> list))
                return list;

            list = new List<Dropdown.OptionData>
            {
                new DropdownWeaponVariantOptionData(WeaponVariant.Normal),
                new DropdownWeaponVariantOptionData(WeaponVariant.OnFire),
                new DropdownWeaponVariantOptionData(WeaponVariant.NormalMultiplayer),
                new DropdownWeaponVariantOptionData(WeaponVariant.OnFireMultiplayer)
            };
            ModAdvancedCache.Add("DropdownShowConditionOptions", list);
            return list;
        }

        public List<Dropdown.OptionData> GetConditionOptionsDependingOnEditingWeapon(bool includeNone = false)
        {
            WeaponType weaponType = currentEditingItemInfo.Weapon;

            List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
            if (includeNone)
                list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.None));

            list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.Normal));

            if (weaponType == WeaponType.Sword)
            {
                list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.OnFire));
                list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.NormalMultiplayer));
                list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.OnFireMultiplayer));
            }
            else if (weaponType == WeaponType.Hammer || weaponType == WeaponType.Spear || weaponType == ModWeaponsManager.SCYTHE_TYPE)
            {
                list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.OnFire));
            }

            return list;
        }

        public bool CreateItem(string directoryName, string name, bool usePersistentFolder, PersonalizationItemInfo templateSource, out PersonalizationItemInfo personalizationItem)
        {
            string rootDirectory = usePersistentFolder ? ModCore.customizationPersistentFolder : ModCore.customizationFolder;
            string directoryPath = Path.Combine(rootDirectory, directoryName);
            string filesDirectoryPath = Path.Combine(directoryPath, "files");

            personalizationItem = null;
            if (Directory.Exists(directoryPath))
                return false;
            else
                _ = Directory.CreateDirectory(directoryPath);

            if (!Directory.Exists(filesDirectoryPath))
                _ = Directory.CreateDirectory(filesDirectoryPath);

            bool useTemplate = true;
            if (templateSource != null)
            {
                try
                {
                    personalizationItem = ModJsonUtils.Deserialize<PersonalizationItemInfo>(ModJsonUtils.Serialize(templateSource));

                    personalizationItem.Name = name;
                    personalizationItem.Description = "No description provided.";
                    personalizationItem.IsVerified = false;
                    personalizationItem.EditorID = Instance.editorId;
                    personalizationItem.ItemID = Guid.NewGuid().ToString();
                    personalizationItem.FolderPath = directoryPath;
                    personalizationItem.RootFolderPath = rootDirectory;
                    personalizationItem.RootFolderName = usePersistentFolder ? ModCore.CUSTOMIZATION_PERSISTENT_FOLDER_NAME : ModCore.CUSTOMIZATION_FOLDER_NAME;
                    personalizationItem.IsPersistentAsset = usePersistentFolder;
                    personalizationItem.MetaData = new PersonalizationItemMetaData()
                    {
                        CustomizationSystemVersion = PersonalizationItemMetaData.CurrentCustomizationSystemVersion,
                    };
                }
                catch
                {
                    useTemplate = false;
                }
            }
            else
                useTemplate = false;

            if (!useTemplate)
            {
                personalizationItem = new PersonalizationItemInfo()
                {
                    Name = name,
                    Description = "No description provided.",
                    IsVerified = false,
                    Category = PersonalizationCategory.WeaponSkins,
                    EditorID = Instance.editorId,
                    ItemID = Guid.NewGuid().ToString(),
                    FolderPath = directoryPath,
                    RootFolderPath = rootDirectory,
                    RootFolderName = usePersistentFolder ? ModCore.CUSTOMIZATION_PERSISTENT_FOLDER_NAME : ModCore.CUSTOMIZATION_FOLDER_NAME,
                    IsPersistentAsset = usePersistentFolder,
                    MetaData = new PersonalizationItemMetaData()
                    {
                        CustomizationSystemVersion = PersonalizationItemMetaData.CurrentCustomizationSystemVersion,
                    }
                };
            }

            personalizationItem.FixValues();
            personalizationItem.SetAuthor(SteamFriends.GetPersonaName());
            PersonalizationManager.Instance.itemList.Items.Add(personalizationItem);

            ModJsonUtils.WriteStream(Path.Combine(directoryPath, ITEM_INFO_FILE), personalizationItem);
            ModJsonUtils.WriteStream(Path.Combine(directoryPath, ITEM_META_DATA_FILE), personalizationItem.MetaData);
            return true;
        }

        public void EditItem(PersonalizationItemInfo personalizationItemInfo, string folder)
        {
            currentEditingItemInfo = personalizationItemInfo;

            UIElementPersonalizationEditorUtilitiesPanel utils = UIPersonalizationEditor.instance.Utilities;
            utils.Show();
            utils.SetConditionOptions(GetConditionOptionsDependingOnEditingWeapon());

            UIPersonalizationEditor.instance.Inspector.Populate(personalizationItemInfo);
            SpawnRootObject();

            UIPersonalizationEditor.instance.ShowNotification("Success", $"Loaded the item ({personalizationItemInfo.Name})", UIElementPersonalizationEditorNotification.SuccessColor);
        }

        public bool SaveItem(out string error, bool ignoreDevPanel = false)
        {
            if (currentEditingItemInfo == null)
            {
                error = "Editing item info is NULL";
                return false;
            }

            if (!currentEditingRoot)
            {
                error = "Editing item is NULL";
                return false;
            }

            string folder = currentEditingItemFolder;
            if (folder.IsNullOrEmpty())
            {
                error = "Could not find folder";
                return false;
            }

            if (!Directory.Exists(folder))
                _ = Directory.CreateDirectory(folder);

            PersonalizationItemMetaData personalizationItemMetaData = currentEditingItemInfo.MetaData;
            if (personalizationItemMetaData == null)
            {
                personalizationItemMetaData = new PersonalizationItemMetaData
                {
                    CustomizationSystemVersion = PersonalizationItemMetaData.CurrentCustomizationSystemVersion
                };
            }

            UIPersonalizationEditor.instance.Inspector.ApplyValues(ignoreDevPanel);
            SerializeRoot();
            try
            {
                ModJsonUtils.WriteStream(Path.Combine(folder, ITEM_INFO_FILE), currentEditingItemInfo);
                ModJsonUtils.WriteStream(Path.Combine(folder, ITEM_META_DATA_FILE), personalizationItemMetaData);
            }
            catch (Exception exc)
            {
                error = exc.ToString();
                return false;
            }
            error = null;
            return true;
        }

        public void SerializeRoot()
        {
            currentEditingItemInfo.RootObject = currentEditingRoot.Serialize();
        }

        public void SerializeRotAndRespawnBot()
        {
            SerializeRoot();
            BoltNetwork.Destroy(m_bot.gameObject);
            SpawnBot(true);
        }

        public void SpawnBot(bool spawnEditingItem)
        {
            _ = base.StartCoroutine(spawnBotCoroutine(spawnEditingItem));
        }

        private IEnumerator spawnBotCoroutine(bool spawnEditingItem)
        {
            PersonalizationController personalizationController = currentPersonalizationController;
            if (personalizationController)
            {
                Destroy(personalizationController.gameObject);
            }

            GameObject spawnPoint = new GameObject();
            spawnPoint.transform.position = Vector3.zero;

            CloneSpawningData cloneSpawningData = new CloneSpawningData(spawnPoint.transform, true, false, UIPersonalizationEditor.instance.Utilities.GetFavoriteColor(), null);

            var cloneSpawner = GameFlowManager.Instance._cloneSpawner;
            cloneSpawner.UseSkinInSingleplayer = false;

            FirstPersonMover bot = cloneSpawner.SpawnClone(cloneSpawningData);
            bot._upgradeCollection._upgradeLevels = new Dictionary<UpgradeType, int>();
            bot._upgradeCollection.AddUpgradeIfMissing(UpgradeType.SwordUnlock, 1);
            bot._upgradeCollection.AddUpgradeIfMissing(UpgradeType.BowUnlock, 1);
            bot._upgradeCollection.AddUpgradeIfMissing(UpgradeType.Hammer, 3);
            bot._upgradeCollection.AddUpgradeIfMissing(UpgradeType.SpearUnlock, 1);
            bot._upgradeCollection.AddUpgradeIfMissing(UpgradeType.EnergyCapacity, 2);
            bot._upgradeCollection.AddUpgradeIfMissing(UpgradeType.Dash, 1);
            bot._upgradeCollection.AddUpgradeIfMissing(ModUpgradesManager.SCYTHE_UNLOCK_UPGRADE, 1);
            bot.transform.eulerAngles = Vector3.up * 90f;
            if (bot._playerCamera)
                bot._playerCamera.gameObject.SetActive(false);

            m_bot = bot;
            m_greatSwordPreviewController = bot.gameObject.AddComponent<GreatSwordPreviewController>();

            DelegateScheduler.Instance.Schedule(delegate
            {
                BoltEntity boltEntity = bot.GetComponent<BoltEntity>();
                if (boltEntity)
                {
                    bot._hasEverHadLocalControl = false;
                    bot._hasLocalControl = false;
                    boltEntity.ReleaseControl();
                }

                if (spawnEditingItem)
                {
                    bot.SetEquippedWeaponType(currentEditingItemInfo.Weapon, false);
                    SpawnRootObject();
                }

            }, 0.2f);

            Destroy(spawnPoint);
            yield break;
        }

        private IEnumerator spawnLevelCoroutine(bool useTransitionManager, LevelEditorLevelData levelEditorLevelData)
        {
            yield return null;

            if (levelEditorLevelData != null)
            {
                GameObject level = new GameObject();
                LevelManager.Instance._currentLevelHidesTheArena = true;
                _ = LevelEditorDataManager.Instance.DeserializeInto(level.transform, levelEditorLevelData).MoveNext();
            }
            else
            {
                LevelManager.Instance._currentLevelHidesTheArena = false;
            }
            ArenaLiftManager.Instance.SetToArena();
            GlobalEventManager.Instance.Dispatch(GlobalEvents.LevelSpawned);
            SpawnBot(false);
            GlobalEventManager.Instance.Dispatch(EDITOR_STARTED_EVENT);

            GameObject cameraObject = Instantiate(PlayerCameraManager.Instance.DefaultGameCameraPrefab.gameObject);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(-2.5f, 3f, 3f);
            cameraObject.transform.eulerAngles = new Vector3(5f, 120f, 0f);
            m_camera = cameraObject.AddComponent<PersonalizationEditorCamera>();

            if (useTransitionManager)
            {
                yield return new WaitForSecondsRealtime(1f);
                TransitionManager.Instance.EndTransition();
            }
            WelcomeMessage();

            yield break;
        }

        public void SpawnRootObject()
        {
            PersonalizationItemInfo info = currentEditingItemInfo;
            if (info == null)
                return;

            PersonalizationEditorObjectInfo rootInfo = info.RootObject;
            if (rootInfo == null)
            {
                rootInfo = new PersonalizationEditorObjectInfo()
                {
                    Name = "Root",
                    Path = "Empty",
                    IsRoot = true,
                    Children = new List<PersonalizationEditorObjectInfo>(),
                    PropertyValues = new Dictionary<string, object>()
                };
                currentEditingItemInfo.RootObject = rootInfo;
            }

            PersonalizationController personalizationController = currentPersonalizationController;
            if (!personalizationController)
                return;

            personalizationController.DestroyAllItems();
            personalizationController.RefreshWeaponRenderers();
            currentEditingRoot = personalizationController.SpawnItem(currentEditingItemInfo);
            PersonalizationEditorObjectManager.Instance.SetCurrentRootNextUniqueIndex(rootInfo.NextUniqueIndex);
        }

        public void EnterPlaytestMode()
        {
            FirstPersonMover firstPersonMover = m_bot;
            if (firstPersonMover)
            {
                firstPersonMover.GetComponent<BoltEntity>().TakeControl();

                firstPersonMover.SetCameraHolderEnabled(true);
                firstPersonMover.SetPlayerCameraEnabled(true);
                firstPersonMover.SetCameraAnimatorEnabled(true);

                m_camera.gameObject.SetActive(false);

                UIPersonalizationEditor.instance.Hide();
                ModUIConstants.ShowPersonalizationEditorPlaytestHUD();
            }
        }

        public void ExitPlaytestMode()
        {
            FirstPersonMover firstPersonMover = m_bot;
            if (firstPersonMover)
            {
                firstPersonMover.SetCameraAnimatorEnabled(false);
                firstPersonMover.SetPlayerCameraEnabled(true);
                firstPersonMover.SetCameraHolderEnabled(true);
                firstPersonMover.ResetInputKeys();
                firstPersonMover.InstantlySetTorsoTiltX(0f);
                firstPersonMover.SetIsJumpingBools(false);
                firstPersonMover.SetIsMovingBools(false);

                PersonalizationEditorCamera camera = m_camera;
                camera.transform.position = new Vector3(-2.5f, 3f, 3f);
                camera.transform.eulerAngles = new Vector3(5f, 120f, 0f);
                camera.gameObject.SetActive(true);

                UIPersonalizationEditor.instance.Show();

                base.StartCoroutine(exitPlaytestModeCoroutine(firstPersonMover));
            }
        }

        private IEnumerator exitPlaytestModeCoroutine(FirstPersonMover firstPersonMover)
        {
            int ffc = ModTime.fixedFrameCount;
            while (ModTime.fixedFrameCount < ffc + 2)
                yield return null;

            if (firstPersonMover)
            {
                firstPersonMover.SetEquippedWeaponType(currentEditingItemInfo.Weapon, false);
                firstPersonMover.GetComponent<BoltEntity>().ReleaseControl();
                firstPersonMover.transform.position = Vector3.zero;
                firstPersonMover.transform.eulerAngles = Vector3.up * 90f;
                firstPersonMover.stopAirCleavingIfActive();
            }

            yield break;
        }

        public void ExportItem(PersonalizationItemInfo personalizationItemInfo, out string destination, string overrideDirectoryPath = null, string overrideFn = null)
        {
            string fn = overrideFn.IsNullOrEmpty() ? $"PersonalizationItem_{personalizationItemInfo.ItemID.ToString().Replace("-", string.Empty)}.zip" : overrideFn;
            string folder = overrideDirectoryPath.IsNullOrEmpty() ? ModDataManager.savesFolder : overrideDirectoryPath;
            destination = Path.Combine(folder, fn);

            if (File.Exists(destination))
                File.Delete(destination);

            FastZip fastZip = new FastZip();
            fastZip.CreateZip(destination, personalizationItemInfo.FolderPath, true, null);
        }

        public void WelcomeMessage()
        {
            UIPersonalizationEditor.instance.Dropdown.Hide();
            ModUIUtils.MessagePopupOK("Welcome to customization editor!", "Here you can make weapon skins, accessories and pets." +
                "\n\n<color=#FFFFFF><size=14>HOW TO MAKE STUFF?</size></color>" +
                "\nTo open or create a project, click on 'File' at the top left and click on 'Open'." +
                "\n\n<color=#FFFFFF><size=14>HOW TO UPLOAD STUFF?</size></color>" +
                "\nTo upload your project, click on 'File' at the top left and click on 'Upload'." +
                "\nOnce you upload an item, you'll have to wait until it's verified and when it is, customization assets will get an update." +
                "\n\n<color=#FFCB23>This editor is still in development, so you can experience issues while editing!</color>", 400f, true);
        }

        public void TutorialVideo()
        {
            UIPersonalizationEditor.instance.Dropdown.Hide();
            Application.OpenURL("https://youtu.be/xdbdb-WizSo"); // todo: make tutorial video and add it here
        }

        public List<ColorPairFloat> GetColorPairsFromString(string dataString)
        {
            if (dataString.IsNullOrEmpty())
                return null;

            string[] split = dataString.Split('|');
            if (split.IsNullOrEmpty())
                return null;

            List<ColorPairFloat> list = new List<ColorPairFloat>();
            foreach (string oldAndNewColorsString in split)
            {
                if (oldAndNewColorsString.IsNullOrEmpty())
                    continue;

                string[] oldAndNewColors = oldAndNewColorsString.Split('-');
                if (oldAndNewColors.Length == 2)
                {
                    Color a = ModParseUtils.TryParseToColor(oldAndNewColors[0], Color.white);
                    Color b = ModParseUtils.TryParseToColor(oldAndNewColors[1], Color.white);
                    list.Add(new ColorPairFloat(a, b));
                }
            }
            return list;
        }

        public string GetStringFromColorPairs(List<ColorPairFloat> colorPairs)
        {
            if (colorPairs.IsNullOrEmpty())
                return null;

            int index = 0;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (ColorPairFloat cp in colorPairs)
            {
                string colorA = ColorUtility.ToHtmlStringRGBA(cp.ColorA);
                string colorB = ColorUtility.ToHtmlStringRGBA(cp.ColorB);
                string colorsString = $"{colorA}-{colorB}".Replace("#", string.Empty);
                _ = stringBuilder.Append(colorsString);
                if (index + 1 != colorPairs.Count)
                    _ = stringBuilder.Append('|');

                index++;
            }
            return stringBuilder.ToString();
        }
    }
}
