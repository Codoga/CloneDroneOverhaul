using OverhaulMod.Engine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIEndlessGameLossWindow : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(OnRestartButtonClicked))]
        [UIElement("RestartButton")]
        private readonly Button m_restartButton;

        [UIElementAction(nameof(OnMainMenuClicked))]
        [UIElement("MainMenuButton")]
        private readonly Button m_mainMenuButton;

        public override bool enableCursor => true;
        public override bool closeOnEscapeButtonPress => false;

        public void OnRestartButtonClicked()
        {
            Hide();

            if (!TransitionManager.OverhaulNonSceneTransitions)
            {
                GameFlowManager.Instance.ResetGameAndSpawnHuman(true);
                return;
            }
            TransitionManager.Instance.DoNonSceneTransition(resetGameCoroutine());
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
