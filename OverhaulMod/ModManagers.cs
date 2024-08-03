﻿using OverhaulMod.Engine;
using OverhaulMod.UI;
using OverhaulMod.Utils;
using System.Collections;
using UnityEngine;

namespace OverhaulMod
{
    public class ModManagers : Singleton<ModManagers>
    {
        [ModSetting(ModSettingsConstants.SHOW_MOD_SETUP_SCREEN_ON_START, true)]
        public static bool ShowModSetupScreenOnStart;

        public override void Awake()
        {
            base.Awake();

            ModCore.GameInitialized += onGameInitialized;

            if (!ModBuildInfo.debug)
                return;

            _ = base.gameObject.AddComponent<UIDeveloperMenu>();
        }

        private void FixedUpdate()
        {
            ModTime.hasFixedUpdated = true;
            ModTime.fixedFrameCount++;
        }

        private void LateUpdate()
        {
            ModTime.hasFixedUpdated = false;
        }

        private void OnDestroy()
        {
            ModCore.GameInitialized -= onGameInitialized;
        }

        private void onGameInitialized()
        {
            TriggerGameLoadedEvent();
            ShowUI();
        }

        public void ShowUI()
        {
            _ = ModActionUtils.RunCoroutine(showUICoroutine());
        }

        private IEnumerator showUICoroutine()
        {
            yield return null;
            yield return null;
            if (GameModeManager.IsOnTitleScreen() && !BoltNetwork.IsRunning)
            {
                if (ModFeatures.IsEnabled(ModFeatures.FeatureType.TitleScreenRework))
                    _ = ModUIConstants.ShowTitleScreenRework();

                if (ShowModSetupScreenOnStart)
                    _ = ModUIConstants.ShowSettingsMenuRework(true);
                else
                    ModUIUtils.ShowChangelogIfRequired(2f);
            }

            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.SubtitleTextFieldRework))
                _ = ModUIConstants.ShowSubtitleTextFieldRework();

            _ = ModUIConstants.ShowCinematicEffects();
            yield break;
        }

        public void TriggerModLoadedEvent()
        {
            foreach (MonoBehaviour behaviour in base.GetComponents<MonoBehaviour>())
            {
                if (behaviour is IModLoadListener modLoadListener)
                    modLoadListener.OnModLoaded();
            }
        }

        public void TriggerModContentLoadedEvent(string errorString)
        {
            GlobalEventManager.Instance.Dispatch(Content.ContentManager.CONTENT_DOWNLOAD_DONE_EVENT, errorString);
        }

        public void TriggerGameLoadedEvent()
        {
            foreach (MonoBehaviour behaviour in base.GetComponents<MonoBehaviour>())
            {
                if (behaviour is IGameLoadListener gameLoadListener)
                    gameLoadListener.OnGameLoaded();
            }
        }

        public static T NewSingleton<T>() where T : Singleton<T>
        {
            return Instance.gameObject.AddComponent<T>();
        }

        public static T NewBoltSingleton<T>() where T : BoltGlobalEventListenerSingleton<T>
        {
            return Instance.gameObject.AddComponent<T>();
        }
    }
}
