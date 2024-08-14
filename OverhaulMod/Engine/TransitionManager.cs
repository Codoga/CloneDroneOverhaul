using OverhaulMod.Utils;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OverhaulMod.Engine
{
    public class TransitionManager : Singleton<TransitionManager>
    {
        [ModSetting(ModSettingsConstants.OVERHAUL_SCENE_TRANSITIONS, true)]
        public static bool OverhaulSceneTransitions;

        [ModSetting(ModSettingsConstants.OVERHAUL_NON_SCENE_TRANSITIONS, true)]
        public static bool OverhaulNonSceneTransitions;

        [ModSetting(ModSettingsConstants.TRANSITION_ON_STARTUP, true)]
        public static bool TransitionOnStartup;

        private TransitionBehaviour m_transitionBehaviour;

        public override void Awake()
        {
            base.Awake();
            if (TransitionOnStartup)
                DoTransition(null, Color.white, false, true, true, 0.1f);
        }

        public void DoNonSceneTransition(IEnumerator coroutine)
        {
            DoTransition(coroutine, Color.black, true, false);
        }

        public void DoTransition(IEnumerator coroutine, Color bgColor, bool showText, bool fadeOut, bool ignoreDeltaTime = false, float deltaTimeMultiplier = 15f, float waitBeforeFadeOut = 0.25f)
        {
            if (m_transitionBehaviour)
                return;

            GameObject gameObject = Instantiate(ModResources.Prefab(AssetBundleConstants.UI, "UI_Transition"), GameUIRoot.Instance.transform, false);
            RectTransform transform = gameObject.transform as RectTransform;
            transform.anchoredPosition = Vector2.zero;
            transform.localEulerAngles = Vector2.zero;
            transform.localScale = Vector2.one;
            transform.SetSiblingIndex(ModUIManager.Instance.GetSiblingIndex(ModUIManager.UILayer.BeforeCrashScreen));
            TransitionBehaviour transitionBehaviour = gameObject.AddComponent<TransitionBehaviour>();
            transitionBehaviour.fadeOut = fadeOut;
            transitionBehaviour.ignoreDeltaTime = ignoreDeltaTime;
            transitionBehaviour.deltaTimeMultiplier = deltaTimeMultiplier;
            transitionBehaviour.waitBeforeFadeOut = waitBeforeFadeOut;
            transitionBehaviour.SetBackgroundColor(bgColor);
            transitionBehaviour.SetTextVisible(showText);
            transitionBehaviour.RunCoroutine(coroutine);
            transitionBehaviour.Refresh();
            m_transitionBehaviour = transitionBehaviour;
        }

        public void EndTransition()
        {
            if (m_transitionBehaviour)
                m_transitionBehaviour.fadeOut = true;
        }

        public static IEnumerator SceneTransitionCoroutine(SceneTransitionManager sceneTransitionManager)
        {
            yield return new WaitForSecondsRealtime(0.25f);
            sceneTransitionManager._isExitingToMainMenu = true;
            GlobalEventManager.Instance.Dispatch("ExitingToMainMenu");
            sceneTransitionManager._isDisconnecting = true;
            SceneTransitionManager.LastDisconnectTime = Time.realtimeSinceStartup;
            SceneTransitionManager.LastDisconnectHadBoltRunning = false;
            if (BoltNetwork.IsConnected || BoltNetwork.IsRunning)
            {
                sceneTransitionManager._isDisconnecting = true;
                SceneTransitionManager.LastDisconnectHadBoltRunning = true;
                bool flag = false;
                object obj = SceneTransitionManager.mutex;
                lock (obj)
                {
                    if (!sceneTransitionManager._isBoltDisconnectInProgress)
                    {
                        flag = true;
                        sceneTransitionManager._isBoltDisconnectInProgress = true;
                    }
                }

                if (!flag)
                    yield break;
                try
                {
                    BoltLauncher.Shutdown();
                    yield break;
                }
                catch (Exception)
                {
                    sceneTransitionManager._isDisconnecting = false;
                    SceneManager.LoadScene("Gameplay");
                    yield break;
                }
            }
            sceneTransitionManager._isDisconnecting = false;
            SceneManager.LoadScene("Gameplay");
            yield break;
        }
    }
}
