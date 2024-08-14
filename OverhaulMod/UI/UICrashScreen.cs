using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UICrashScreen : OverhaulUIBehaviour
    {
        public static bool HasSentReport;

        public override bool enableCursor => true;

        public override bool closeOnEscapeButtonPress => false;

        [UIElement("StackTrace")]
        private readonly Text m_stackTraceText;

        [UIElementAction(nameof(OnIgnoreCrashButtonClicked))]
        [UIElement("IgnoreCrashButton")]
        private readonly Button m_ignoreCrashButton;

        [UIElementAction(nameof(OnMainMenuButtonClicked))]
        [UIElement("MainMenuButton")]
        private readonly Button m_mainMenuButton;

        [UIElementAction(nameof(OnExitGameButtonClicked))]
        [UIElement("ExitGameButton")]
        private readonly Button m_exitGameButton;

        [UIElementAction(nameof(OnSendReportButtonClicked))]
        [UIElement("SendReportButton")]
        private readonly Button m_sendReportButton;

        [UIElementAction(nameof(OnIgnoreCrashesToggleChanged))]
        [UIElement("IgnoreCrashesToggle")]
        private readonly Toggle m_ignoreCrashesToggle;

        [UIElement("ExpandButton", typeof(UIElementExpandButton))]
        private readonly UIElementExpandButton m_expandButton;

        [UIElement("ScrollRect")]
        private readonly RectTransform m_stackTracePanel;

        protected override void OnInitialized()
        {
            UIElementExpandButton expandButton = m_expandButton;
            expandButton.rectTransform = m_stackTracePanel;
            expandButton.collapsedSize = new Vector2(-50f, 175f);
            expandButton.expandedSize = new Vector2(-50f, 350f);

            m_ignoreCrashesToggle.isOn = CrashPreventionManager.IgnoreCrashes;
            m_sendReportButton.interactable = !HasSentReport;
        }

        public void SetStackTraceText(string message)
        {
            m_stackTraceText.text = message;
        }

        public void OnIgnoreCrashButtonClicked()
        {
            Hide();

            if (ErrorManager.Instance)
                ErrorManager.Instance._hasCrashed = false;

            if (TimeManager.Instance)
                TimeManager.Instance.OnGameUnPaused();
        }

        public void OnMainMenuButtonClicked()
        {
            Hide();
            SceneTransitionManager.Instance.DisconnectAndExitToMainMenu();
        }

        public void OnExitGameButtonClicked()
        {
            Application.Quit();
        }

        public void OnSendReportButtonClicked()
        {
            HasSentReport = true;
            m_sendReportButton.interactable = false;
            ModWebhookManager.Instance.ExecuteCrashReportsWebhook(m_stackTraceText.text, delegate
            {
                ModUIUtils.MessagePopupOK("Report sent!", string.Empty, true);
            }, delegate (string error)
            {
                ModUIUtils.MessagePopupOK("Report send error", error, true);
            });
        }

        public void OnIgnoreCrashesToggleChanged(bool value)
        {
            CrashPreventionManager.IgnoreCrashes = value;
        }
    }
}
