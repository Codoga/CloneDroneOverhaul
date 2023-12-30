﻿using CDOverhaul.DevTools;
using CDOverhaul.Patches;
using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CDOverhaul.HUD
{
    public class UIErrorScreen : UIController
    {
        private static bool s_HasSentReport;

        [UIElementReferenceAttribute("ErrorMessage")]
        private readonly Text m_ErrorText;
        [UIElementReferenceAttribute("stacktraceText")]
        private readonly Text m_StackTraceText;

        [UIElementActionReference(nameof(IgnoreCrash))]
        [UIElementReferenceAttribute("IgnoreCrash")]
        private readonly Button m_IgnoreCrash;
        [UIElementActionReference(nameof(GoToMainMenu))]
        [UIElementReferenceAttribute("MainMenu")]
        private readonly Button m_MainMenu;
        [UIElementActionReference(nameof(ExitGame))]
        [UIElementReferenceAttribute("ExitGame")]
        private readonly Button m_ExitGame;

        [UIElementActionReference(nameof(OpenStackTrace))]
        [UIElementReferenceAttribute("ViewStacktrace")]
        private readonly Button m_ViewStackTrace;
        [UIElementActionReference(nameof(TriggerScreenshot))]
        [UIElementReferenceAttribute("Screenshot")]
        private readonly Button m_MakeScreenshot;

        [UIElementActionReference(nameof(SendReport))]
        [UIElementReferenceAttribute("SendReport")]
        private readonly Button m_SendReport;
        [UIElementReferenceAttribute("SendReport")]
        private readonly CanvasGroup m_SendReportCanvasGroup;

        [UIElementReferenceAttribute("StackTrace")]
        private readonly GameObject m_StackTraceWindow;
        [UIElementActionReference(nameof(CloseStackTrace))]
        [UIElementReferenceAttribute("CloseStackTraceButton")]
        private readonly Button m_CloseStackTrace;

        private bool m_IsScreenshotting;

        protected override bool HideTitleScreen() => true;

        public override void Show()
        {
            base.Show();
            SetSendReportButtonInteractable(!s_HasSentReport);
        }

        public override void OnGetArguments(object[] args)
        {
            string logString = args[0] as string;
            string stackTrace = args[1] as string;

            m_ErrorText.text = logString;
            m_StackTraceText.text = logString + " " + stackTrace;
        }

        public void SetSendReportButtonInteractable(bool value)
        {
            m_SendReport.interactable = value;
            m_SendReportCanvasGroup.alpha = value ? 1f : 0.25f;
        }

        public void OpenStackTrace()
        {
            m_StackTraceWindow.SetActive(true);
        }

        public void CloseStackTrace()
        {
            m_StackTraceWindow.SetActive(false);
        }

        public void IgnoreCrash()
        {
            OverhaulDebugActions.IgnoreCrash();
            Hide();
        }

        public void GoToMainMenu()
        {
            SceneTransitionManager.Instance.DisconnectAndExitToMainMenu();
            Hide();
        }

        public void ExitGame()
        {
            OverhaulTransitionManager.reference.DoTransition(delegate
            {
                Application.Quit();
            });
        }

        public void SendReport()
        {
            if (s_HasSentReport)
                return;

            if (string.IsNullOrEmpty(ErrorManager_Patch.Report))
                return;

            OverhaulWebhooks.ExecuteCrashReportsWebhook(ErrorManager_Patch.Report);
            s_HasSentReport = true;
            SetSendReportButtonInteractable(false);
        }

        public void TriggerScreenshot()
        {
            if (m_IsScreenshotting || !SteamAPI.IsSteamRunning())
                return;

            m_IsScreenshotting = true;
            _ = StartCoroutine(triggerScreenshotCoroutine());
        }

        private IEnumerator triggerScreenshotCoroutine()
        {
            OpenStackTrace();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            SteamScreenshots.TriggerScreenshot();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            CloseStackTrace();
            m_IsScreenshotting = false;
        }
    }
}