using OverhaulMod.Content;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIUpdatesWindow : OverhaulUIBehaviour
    {
        private static UpdateInfoList s_updateInfoList;

        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnEditorButtonClicked))]
        [UIElement("EditorButton")]
        private readonly Button m_editorButton;

        [UIElementAction(nameof(OnCheckForUpdatesButtonClicked))]
        [UIElement("CheckForUpdatesButton")]
        private readonly Button m_checkForUpdatesButton;

        [UIElementAction(nameof(OnInGameUpdateButtonClicked))]
        [UIElement("UpdateButton")]
        private readonly Button m_inGameUpdateButton;

        [UIElementAction(nameof(OnBranchChanged))]
        [UIElement("BranchDropdown")]
        private readonly Dropdown m_branchDropdown;


        [UIElementAction(nameof(OnCheckForUpdatesOnStartupToggleChanged))]
        [UIElement("CheckForUpdateOnStartupToggle")]
        private readonly Toggle m_checkForUpdatesOnStartupToggle;

        [UIElement("LoadingIndicator", false)]
        private readonly GameObject m_loadingIndicator;

        [UIElement("ChangelogText")]
        private readonly Text m_changelogText;

        [UIElement("VersionText")]
        private readonly Text m_versionText;

        [UIElement("BranchDescription")]
        private readonly Text m_branchDescription;

        [UIElement("ProgressBar", false)]
        private readonly GameObject m_progressBar;

        [UIElement("ProgressBarFill")]
        private readonly Image m_progressBarFill;

        private UnityWebRequest m_webRequest;
        private string m_downloadSource, m_directoryName;

        public override bool hideTitleScreen => true;

        public bool disallowClosing
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            int branch = (ModBuildInfo.debug || ExclusiveContentManager.Instance.IsLocalUserTheTester()) ? 2 : 0;

            m_checkForUpdatesOnStartupToggle.isOn = ModSettingsManager.GetBoolValue(ModSettingsConstants.CHECK_FOR_UPDATES_ON_STARTUP);
            m_inGameUpdateButton.interactable = false;
            m_branchDropdown.options = UpdateManager.Instance.GetAvailableBranches();
            m_branchDropdown.value = branch;

            OnBranchChanged(branch);
            ClearVersionAndChangelogTexts();
        }

        public override void Show()
        {
            base.Show();
            m_branchDropdown.options = UpdateManager.Instance.GetAvailableBranches();
            m_editorButton.gameObject.SetActive(ModUserInfo.isDeveloper);
        }

        public override void Hide()
        {
            if (disallowClosing)
                return;

            base.Hide();
        }

        public override void Update()
        {
            if (m_webRequest != null)
            {
                try
                {
                    m_progressBarFill.fillAmount = m_webRequest.downloadProgress;
                }
                catch
                {
                }
            }
            else
                m_progressBarFill.fillAmount = 0f;
        }

        public void SelectBranchAndSearchForUpdates(int value)
        {
            m_branchDropdown.value = value;
            OnCheckForUpdatesButtonClicked();
        }

        public void SetUIInteractable(bool value)
        {
            m_branchDropdown.interactable = value;
            m_checkForUpdatesButton.interactable = value;
            m_loadingIndicator.SetActive(!value);
        }

        private void onCheckedForUpdates(UpdateInfoList updateInfoList)
        {
            if (s_updateInfoList == null)
                s_updateInfoList = updateInfoList;

            SetUIInteractable(true);
            UpdateInfo updateInfo = null;
            switch (m_branchDropdown.value)
            {
                case 0:
                    updateInfo = updateInfoList.ModBotRelease;
                    break;
                case 1:
                    updateInfo = updateInfoList.GitHubRelease;
                    break;
                case 2:
                    updateInfo = updateInfoList.InternalRelease;
                    break;
            }

            if (updateInfo == null || !updateInfo.IsNewerBuild())
            {
                m_versionText.text = LocalizationManager.Instance.GetTranslatedString("no updates found");
                return;
            }
            m_directoryName = $"OverhaulMod_{updateInfo.ModVersion}";
            m_downloadSource = updateInfo.DownloadLink;
            m_inGameUpdateButton.interactable = true;

            SetVersionAndChangelogTexts($"<color=#5f9ded>></color>  {LocalizationManager.Instance.GetTranslatedString("update available:")} {updateInfo.ModVersion}", $"Changelog:\n{updateInfo.Changelog}");
        }

        private void onFailedToCheckUpdates(string error)
        {
            SetVersionAndChangelogTexts(LocalizationManager.Instance.GetTranslatedString("error:").AddColor(Color.yellow), $"{error}".AddColor(Color.yellow));
            SetUIInteractable(true);
        }

        private void onInstalledNewBuild()
        {
            m_progressBar.SetActive(false);
            _ = ModUIConstants.ShowRestartRequiredScreen(false);
        }

        private void onFailedToInstallNewBuild(string error)
        {
            disallowClosing = false;
            m_exitButton.interactable = true;
            m_progressBar.SetActive(false);
            SetUIInteractable(true);

            ModUIUtils.MessagePopupOK(LocalizationManager.Instance.GetTranslatedString("installation error"), error, 200f, true);
        }

        public void SetVersionAndChangelogTexts(string version, string changelog)
        {
            m_changelogText.text = changelog;
            m_versionText.text = version;
        }

        public void ClearVersionAndChangelogTexts()
        {
            SetVersionAndChangelogTexts(string.Empty, string.Empty);
        }

        public void OnCheckForUpdatesButtonClicked()
        {
            m_inGameUpdateButton.interactable = false;
            SetUIInteractable(false);
            ClearVersionAndChangelogTexts();

            if (s_updateInfoList != null)
            {
                DelegateScheduler.Instance.Schedule(delegate
                {
                    onCheckedForUpdates(s_updateInfoList);
                }, 0.2f);
                return;
            }
            UpdateManager.Instance.DownloadUpdateInfoFile(onCheckedForUpdates, onFailedToCheckUpdates);
        }

        public void OnBranchChanged(int index)
        {
            index = Mathf.Clamp(index, 0, m_branchDropdown.options.Count - 1);
            m_branchDescription.text = $"{LocalizationManager.Instance.GetTranslatedString("selected branch:")} \"{m_branchDropdown.options[index].text}\".\n{UpdateManager.Instance.GetBranchDescription(index)}";
        }

        public void OnInGameUpdateButtonClicked()
        {
            ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("update the mod in-game?"), LocalizationManager.Instance.GetTranslatedString("ingame_update_desc"), 200f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", LocalizationManager.Instance.GetTranslatedString("update"), LocalizationManager.Instance.GetTranslatedString("cancel"), null, onConfirmedInGameUpdate);
        }

        private void onConfirmedInGameUpdate()
        {
            disallowClosing = true;
            m_exitButton.interactable = false;
            m_inGameUpdateButton.interactable = false;
            m_progressBar.SetActive(true);
            SetUIInteractable(false);
            ClearVersionAndChangelogTexts();

            UpdateManager.Instance.DownloadBuildFromSource(m_downloadSource, m_directoryName, onInstalledNewBuild, onFailedToInstallNewBuild, out m_webRequest);
        }

        public void OnEditorButtonClicked()
        {
            _ = ModUIConstants.ShowUpdateInfoEditor(base.transform);
        }

        public void OnCheckForUpdatesOnStartupToggleChanged(bool value)
        {
            ModSettingsManager.SetBoolValue(ModSettingsConstants.CHECK_FOR_UPDATES_ON_STARTUP, value, true);
        }
    }
}
