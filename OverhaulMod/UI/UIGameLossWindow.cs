﻿using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIGameLossWindow : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(OnRestartButtonClicked))]
        [UIElement("RestartButton")]
        private readonly Button m_restartButton;

        [UIElementAction(nameof(OnSoftRestartButtonClicked))]
        [UIElement("SoftRestartButton")]
        private readonly Button m_softRestartButton;

        [UIElementAction(nameof(OnMainMenuClicked))]
        [UIElement("MainMenuButton")]
        private readonly Button m_mainMenuButton;

        public override bool enableCursor => true;

        public void OnRestartButtonClicked()
        {
            Hide();
            TransitionManager.Instance.DoNonSceneTransition(resetGameCoroutine());
        }

        public void OnSoftRestartButtonClicked()
        {
            Hide();
            QuickResetManager.Instance.RestartGame();
        }

        public void OnMainMenuClicked()
        {
            Hide();
            SceneTransitionManager.Instance.DisconnectAndExitToMainMenu();
        }

        private IEnumerator resetGameCoroutine()
        {
            yield return new WaitForSecondsRealtime(0.25f);
            GameFlowManager gameFlowManager = GameFlowManager.Instance;
            gameFlowManager.ResetGameAndSpawnHuman(true);
            while (gameFlowManager._isAsyncLoadingInProgress)
                yield return null;

            yield return new WaitForSecondsRealtime(0.1f);
            TransitionManager.Instance.EndTransition();
            yield break;
        }
    }
}