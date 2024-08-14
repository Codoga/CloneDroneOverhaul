using ModLibrary;
using OverhaulMod.Combat;
using OverhaulMod.Content.Personalization;
using OverhaulMod.Engine;
using OverhaulMod.Patches.Behaviours;
using OverhaulMod.Utils;
using OverhaulMod.Visuals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OverhaulMod
{
    [MainModClass]
    public class ModCore : Mod
    {
        public const string CUSTOMIZATION_FOLDER_NAME = "customization";

        public const string CUSTOMIZATION_PERSISTENT_FOLDER_NAME = "customizationPersistent";

        [ModSetting(ModSettingsConstants.ENABLE_TITLE_BAR_OVERHAUL, true)]
        public static bool EnableTitleBarOverhaul;

        [ModSetting(ModSettingsConstants.SHOW_SPEAKER_NAME, true)]
        public static bool ShowSpeakerName;

        [ModSetting(ModSettingsConstants.STREAMER_MODE, true)]
        public static bool StreamerMode;

        public static readonly string LoremIpsumText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus ut viverra diam, nec rhoncus dui. Suspendisse quis rutrum dolor.";

        public static bool EnterCustomizationEditor;

        public static event Action GameInitialized;
        public static event Action<bool> ModStateChanged;
        public static event Action<Camera, Camera> OnCameraSwitched;

        public static ModCore instance { get; private set; }

        public static bool isEnabled
        {
            get;
            private set;
        }

        private static string s_folder;
        public static string folder
        {
            get
            {
                ModCore modCore = instance;
                if (modCore == null)
                {
                    return null;
                }

                if (s_folder == null)
                {
                    s_folder = modCore.ModInfo.FolderPath;
                }
                return s_folder;
            }
        }

        private static string s_savesFolder;
        public static string savesFolder
        {
            get
            {
                if (s_savesFolder == null)
                {
                    s_savesFolder = $"{Path.Combine(modDataFolder, "saves")}/";
                }
                return s_savesFolder;
            }
        }

        private static string s_assetsFolder;
        public static string assetsFolder
        {
            get
            {
                if (s_assetsFolder == null)
                {
                    s_assetsFolder = $"{Path.Combine(folder, "assets")}/";
                }
                return s_assetsFolder;
            }
        }

        private static string s_examplesFolder;
        public static string examplesFolder
        {
            get
            {
                if (s_examplesFolder == null)
                {
                    s_examplesFolder = $"{Path.Combine(folder, "examples")}/";
                }
                return s_examplesFolder;
            }
        }

        private static string s_dataFolder;
        public static string dataFolder
        {
            get
            {
                if (s_dataFolder == null)
                {
                    s_dataFolder = $"{Path.Combine(assetsFolder, "data")}/";
                }
                return s_dataFolder;
            }
        }

        private static string s_modFolder;
        public static string modDataFolder
        {
            get
            {
                if (s_modFolder == null)
                {
                    s_modFolder = $"{Path.Combine(Application.persistentDataPath, "OverhaulMod")}/";
                }
                return s_modFolder;
            }
        }

        private static string s_contentFolder;
        public static string contentFolder
        {
            get
            {
                if (s_contentFolder == null)
                {
                    s_contentFolder = $"{Path.Combine(modDataFolder, "content")}/";
                }
                return s_contentFolder;
            }
        }

        private static string s_addonsFolder;
        public static string addonsFolder
        {
            get
            {
                if (s_addonsFolder == null)
                {
                    s_addonsFolder = $"{Path.Combine(contentFolder, "addons")}/";
                }
                return s_addonsFolder;
            }
        }

        private static string s_customizationFolder;
        public static string customizationFolder
        {
            get
            {
                if (s_customizationFolder == null)
                {
                    s_customizationFolder = $"{Path.Combine(contentFolder, CUSTOMIZATION_FOLDER_NAME)}/";
                }
                return s_customizationFolder;
            }
        }

        private static string s_persistentCustomizationFolder;
        public static string customizationPersistentFolder
        {
            get
            {
                if (s_persistentCustomizationFolder == null)
                {
                    s_persistentCustomizationFolder = $"{Path.Combine(contentFolder, CUSTOMIZATION_PERSISTENT_FOLDER_NAME)}/";
                }
                return s_persistentCustomizationFolder;
            }
        }

        private static string s_textureFolder;
        public static string texturesFolder
        {
            get
            {
                if (s_textureFolder == null)
                {
                    s_textureFolder = $"{Path.Combine(assetsFolder, "textures")}/";
                }
                return s_textureFolder;
            }
        }

        private static string s_editorTexturesFolder;
        public static string editorTexturesFolder
        {
            get
            {
                if (s_editorTexturesFolder == null)
                {
                    s_editorTexturesFolder = $"{Path.Combine(texturesFolder, "editor")}/";
                }
                return s_editorTexturesFolder;
            }
        }

        public override void OnModEnabled()
        {
            instance = this;
            isEnabled = true;

            ModLoader.Load();
            GamePatchBehaviour.Load();

            //TriggerGameInitializedEvent();
            TriggerModStateChangedEvent(true);

            ModSpecialUtils.SetTitleBarStateDependingOnSettings();

            if (EnterCustomizationEditor)
            {
                EnterCustomizationEditor = false;
                if (PersonalizationEditorManager.Instance)
                    PersonalizationEditorManager.Instance.StartEditorGameMode(true);
            }
        }

        public override void OnModLoaded()
        {
            instance = this;
            GlobalEventManager.Instance.AddEventListenerOnce(GlobalEvents.GameInitializtionCompleted, TriggerGameInitializedEvent);
            ModLoader.Load();
        }

        public override void OnModDeactivated()
        {
            instance = null;
            isEnabled = false;

            GameUIRoot gameUIRoot = ModCache.gameUIRoot;
            if (gameUIRoot)
            {
                ModUIManager modUIManager = ModUIManager.Instance;
                if (modUIManager)
                {
                    if (modUIManager.IsUIVisible(AssetBundleConstants.UI, ModUIConstants.UI_PAUSE_MENU))
                    {
                        _ = modUIManager.Hide(AssetBundleConstants.UI, ModUIConstants.UI_PAUSE_MENU);
                        if (gameUIRoot.EscMenu)
                            gameUIRoot.EscMenu.Show();
                    }
                }
            }

            TriggerModStateChangedEvent(false);

            ModSpecialUtils.SetTitleBarStateDependingOnSettings();

            GamePatchBehaviour.Unload();
            ModLoader.Unload();
        }

        public override void OnClientConnectedToServer()
        {
            PersonalizationMultiplayerManager.Instance.SendPlayerCustomizationDataEvent(false);
            if (GameModeManager.Is(GameMode.EndlessCoop) || GameModeManager.Is(GameMode.CoopChallenge))
                _ = waitThenFixArenaLiftInCoop().Run();

            ArenaRemodelManager.Instance.PatchVanillaParts(false);
        }

        public override void OnLevelEditorStarted()
        {
            ArenaRemodelManager.Instance.SetUpperInteriorActive(false);
            PostEffectsManager.Instance.RefreshCameraPostEffects();
        }

        public override void OnMultiplayerEventReceived(GenericStringForModdingEvent moddedEvent)
        {
            PersonalizationMultiplayerManager.Instance.OnEvent(moddedEvent);
        }

        public override UnityEngine.Object OnResourcesLoad(string path)
        {
            return LevelEditorPatch.Patch.GetResourceObject(path);
        }

        public override void OnFirstPersonMoverSpawned(FirstPersonMover firstPersonMover)
        {
            _ = ModActionUtils.RunCoroutine(waitUntilCharacterModelInitialization(firstPersonMover));
        }

        public override void OnUpgradesRefreshed(FirstPersonMover owner, UpgradeCollection upgrades)
        {
            owner.addWeaponToEquipppedIfHasUpgradeAndModelPresent(ModUpgradesManager.SCYTHE_UNLOCK_UPGRADE, ModWeaponsManager.SCYTHE_TYPE);
            owner.RefreshModWeaponModels();

            RobotInventory robotInventory = ModComponentCache.GetRobotInventory(owner.transform);
            if (robotInventory)
            {
                robotInventory.OnUpgradesRefreshed(upgrades);
            }
        }

        public override void AfterUpgradesRefreshed(FirstPersonMover owner, UpgradeCollection upgrades)
        {
            PersonalizationController personalizationController = owner.GetComponent<PersonalizationController>();
            if (personalizationController)
            {
                personalizationController.RespawnSkinsIfRequired();
            }
        }

        public override void OnLanguageChanged(string newLanguageID, Dictionary<string, string> localizationDictionary)
        {
            if (newLanguageID.IsNullOrEmpty() || localizationDictionary.IsNullOrEmpty())
                return;

            ModLocalizationManager manager = ModLocalizationManager.Instance;
            if (manager)
            {
                manager.PopulateTranslationDictionary(ref localizationDictionary, newLanguageID);
            }
        }

        private IEnumerator waitUntilCharacterModelInitialization(FirstPersonMover firstPersonMover)
        {
            yield return new WaitUntil(() => (!firstPersonMover || firstPersonMover.gameObject.activeInHierarchy) && firstPersonMover.HasCharacterModel());
            for (int i = 0; i < 3; i++)
                yield return null;

            if (!firstPersonMover || !firstPersonMover.IsAlive())
                yield break;

            if (firstPersonMover.IsAttachedAndAlive())
            {
                _ = firstPersonMover.gameObject.AddComponent<PersonalizationController>();

                if (ModFeatures.IsEnabled(ModFeatures.FeatureType.WeaponBag) && !firstPersonMover.IsMindSpaceCharacter && ((!GameModeManager.IsMultiplayerDuel() && !GameModeManager.IsBattleRoyale()) || firstPersonMover.IsMainPlayer()))
                    _ = firstPersonMover.gameObject.AddComponent<RobotWeaponBag>();

                RobotInventory robotInventory = firstPersonMover.gameObject.AddComponent<RobotInventory>();
            }

            yield return new WaitUntil(() => !firstPersonMover || firstPersonMover._playerCamera);
            if (firstPersonMover)
                CameraManager.Instance.AddControllers(firstPersonMover._playerCamera, firstPersonMover);

            yield break;
        }

        private IEnumerator waitThenFixArenaLiftInCoop()
        {
            yield return new WaitForSeconds(3f);
            if (!ArenaLiftManager.Instance)
                yield break;

            ArenaLift lift = ArenaLiftManager.Instance.Lift;
            if (lift && (lift._state == null || lift._stateHolder == null))
            {
                foreach (MovingPlatformStateHolder sh in Resources.FindObjectsOfTypeAll<MovingPlatformStateHolder>())
                {
                    BoltEntity boltEntity = sh.GetComponent<BoltEntity>();
                    if (!boltEntity || !boltEntity.IsAttached)
                        continue;

                    if (sh.state.UniqueIndex == lift.GetUniqueIndex())
                    {
                        lift._state = sh.state;
                        lift._stateHolder = sh;
                        break;
                    }
                }
            }
            yield break;
        }

        public static void TriggerGameInitializedEvent()
        {
            GameInitialized?.Invoke();
        }

        public static void TriggerModStateChangedEvent(bool enabled)
        {
            ModStateChanged?.Invoke(enabled);
        }

        public static void TriggerOnCameraSwitchedEvent(Camera oldCamera, Camera newCamera)
        {
            OnCameraSwitched?.Invoke(oldCamera, newCamera);
        }

        public static IEnumerator PushDownIfAboveGroundCoroutine_Patch(FirstPersonMover firstPersonMover)
        {
            while (firstPersonMover && !firstPersonMover.gameObject.activeInHierarchy)
                yield return null;

            yield return new WaitForSeconds(0.05f);
            if (!firstPersonMover || !firstPersonMover._characterController || firstPersonMover.IsRidingOtherCharacter() || GameFlowManager.Instance.IsInEditorMode())
                yield break;

            if (Physics.SphereCast(new Ray(firstPersonMover.transform.position + firstPersonMover.CenterOfCharacterOffset, -Vector3.up), firstPersonMover.CollisionRadius, out RaycastHit raycastHit, 20f, PhysicsManager.GetEnvironmentLayerMask(), QueryTriggerInteraction.Ignore))
            {
                float num = firstPersonMover.transform.position.y - raycastHit.point.y;
                if (num > 0f)
                {
                    firstPersonMover.transform.position += new Vector3(0f, -num, 0f);
                }
            }
            yield break;
        }
    }
}
