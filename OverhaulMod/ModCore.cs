﻿using ModLibrary;
using ModLibrary.YieldInstructions;
using OverhaulMod.Combat;
using OverhaulMod.Patches.Addons;
using OverhaulMod.Utils;
using OverhaulMod.Visuals;
using System;
using System.Collections;
using UnityEngine;

namespace OverhaulMod
{
    [MainModClass]
    public class ModCore : Mod
    {
        public static event Action GameInitialized;
        public static event Action ContentDownloaded;

        public static ModCore instance { get; private set; }

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
                    s_savesFolder = folder + "saves/";
                }
                ModIOUtils.CreateFolderIfNotCreated(s_savesFolder);
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
                    s_assetsFolder = folder + "assets/";
                }
                return s_assetsFolder;
            }
        }

        private static string s_dataFolder;
        public static string dataFolder
        {
            get
            {
                if (s_dataFolder == null)
                {
                    s_dataFolder = assetsFolder + "data/";
                }
                return s_dataFolder;
            }
        }


        private static string s_contentFolder;
        public static string contentFolder
        {
            get
            {
                if (s_contentFolder == null)
                {
                    s_contentFolder = assetsFolder + "content/";
                }
                return s_contentFolder;
            }
        }

        private bool m_hasAddedListeners;

        public override void OnModEnabled()
        {
            instance = this;

            addEventListeners();
        }

        public override void OnModLoaded()
        {
            instance = this;

            ModLoader.Load();
            GameAddon.Load();

            addEventListeners();
        }

        public override void OnModDeactivated()
        {
            instance = null;
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
            owner.RefreshModWeaponModels();
        }

        private IEnumerator waitUntilCharacterModelInitialization(FirstPersonMover firstPersonMover)
        {
            yield return new WaitForCharacterModelInitialization(firstPersonMover);
            yield return new WaitUntil(() => !firstPersonMover || firstPersonMover.gameObject.activeInHierarchy);
            yield return new WaitForEndOfFrame();
            if (firstPersonMover && firstPersonMover.IsAlive())
            {
                ModWeaponsManager.Instance.AddWeaponsToRobot(firstPersonMover);
            }
            yield return new WaitForEndOfFrame();
            if (firstPersonMover && firstPersonMover.IsAlive())
            {
                firstPersonMover.RefreshModWeaponModels();

                if (firstPersonMover.IsAttachedAndAlive())
                {
                    if (ModFeatures.IsEnabled(ModFeatures.FeatureType.WeaponBag))
                        _ = firstPersonMover.gameObject.AddComponent<RobotWeaponBag>();
                }
            }
            yield break;
        }

        private void addEventListeners()
        {
            if (m_hasAddedListeners)
            {
                return;
            }
            m_hasAddedListeners = true;

            GlobalEventManager.Instance.AddEventListenerOnce(GlobalEvents.GameInitializtionCompleted, onGameInitialized);
        }

        private void onGameInitialized()
        {
            Debug.LogWarning("GAME INITIALIZED");
            GameInitialized?.Invoke();
        }

        public static void DispatchContentLoadedEvent()
        {
            ContentDownloaded?.Invoke();
        }
    }
}