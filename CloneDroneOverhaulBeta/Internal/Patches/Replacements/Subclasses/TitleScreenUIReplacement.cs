﻿using CDOverhaul.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace CDOverhaul.Patches
{
    public class TitleScreenUIReplacement : ReplacementBase
    {
        private Transform m_ButtonsTransform;
        private Transform m_SpawnedPanel;

        private RectTransform m_MultiplayerNEWButtonTransform;

        private Text m_SettingsText;
        private Text m_BugReportText;
        private Text m_AboutOverhaulText;

        private Vector3 m_OriginalAchievementsNewHintPosition;
        private Transform m_AchievementsNewHint;

        private Transform m_MessagePanel;
        public TitleScreenMessagePanel MessagePanelComponent;

        public override void Replace()
        {
            base.Replace();
            TitleScreenUI target = GameUIRoot.Instance.TitleScreenUI;

            m_ButtonsTransform = TransformUtils.FindChildRecursive(target.transform, "BottomButtons");
            if (m_ButtonsTransform == null)
            {
                SuccessfullyPatched = false;
                return;
            }

            m_MultiplayerNEWButtonTransform = TransformUtils.FindChildRecursive(target.transform, "MultiplayerButton_NEW") as RectTransform;
            if (m_MultiplayerNEWButtonTransform == null)
            {
                SuccessfullyPatched = false;
                return;
            }
            m_MultiplayerNEWButtonTransform.localPosition = new Vector3(0, -85f, 0);

            m_AchievementsNewHint = TransformUtils.FindChildRecursive(target.transform, "AchievementsNewHint");
            if (m_AchievementsNewHint == null)
            {
                SuccessfullyPatched = false;
                return;
            }

            m_MessagePanel = TransformUtils.FindChildRecursive(target.transform, "TitleScreenMessagePanel");
            if (m_MessagePanel == null)
            {
                SuccessfullyPatched = false;
                return;
            }
            MessagePanelComponent = m_MessagePanel.GetComponent<TitleScreenMessagePanel>();

            m_OriginalAchievementsNewHintPosition = m_AchievementsNewHint.localPosition;
            m_AchievementsNewHint.localPosition = new Vector3(70f, -185f, 0f);

            GameObject panel = OverhaulMod.Core.CanvasController.GetHUDPrefab("TitleScreenUI_Buttons");
            if (panel == null)
            {
                SuccessfullyPatched = false;
                return;
            }
            m_SpawnedPanel = GameObject.Instantiate(panel, m_ButtonsTransform).transform;
            m_SpawnedPanel.SetAsFirstSibling();
            m_SpawnedPanel.gameObject.SetActive(true);

            ModdedObject moddedObject = m_SpawnedPanel.GetComponent<ModdedObject>();
            moddedObject.GetObject<Button>(0).onClick.AddListener(OverhaulController.GetController<ParametersMenu>().Show);
            m_SettingsText = moddedObject.GetObject<Text>(1);
            moddedObject.GetObject<Button>(2).onClick.AddListener(OverhaulController.GetController<OverhaulSurveyUI>().Show);
            m_BugReportText = moddedObject.GetObject<Text>(3);
            moddedObject.GetObject<Button>(4).onClick.AddListener(OverhaulController.GetController<AboutOverhaulMenu>().Show);
            m_AboutOverhaulText = moddedObject.GetObject<Text>(5);
            moddedObject.GetObject<Transform>(6).gameObject.SetActive(OverhaulVersion.IsDebugBuild);
            moddedObject.GetObject<Button>(6).onClick.AddListener(OverhaulController.GetController<OverhaulLocalizationEditor>().Show);

            m_ButtonsTransform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            m_ButtonsTransform.localPosition = new Vector3(0, -158f, 0);

            _ = OverhaulEventsController.AddEventListener(GlobalEvents.UILanguageChanged, localizeTexts, true);
            localizeTexts();

            Transform joinPublicMatchButtonTransform = target.BattleRoyaleMenu.JoinRandomButton.transform;
            joinPublicMatchButtonTransform.localPosition = new Vector3(-57f, -150f, 0);

            Transform createPrivateModdedLobbyButtonTransform = Object.Instantiate(joinPublicMatchButtonTransform, joinPublicMatchButtonTransform.parent);
            createPrivateModdedLobbyButtonTransform.localPosition = new Vector3(57f, -150f, 0);
            Button createPrivateModdedLobbyButton = createPrivateModdedLobbyButtonTransform.GetComponent<Button>();
            createPrivateModdedLobbyButton.interactable = OverhaulFeatureAvailabilitySystem.ImplementedInBuild.IsCustomMultiplayerTestEnabled;
            LocalizedTextField localizedTextFieldCreatePrivateModdedLobby = createPrivateModdedLobbyButtonTransform.GetComponentInChildren<LocalizedTextField>();
            if (localizedTextFieldCreatePrivateModdedLobby)
            {
                Object.Destroy(localizedTextFieldCreatePrivateModdedLobby);
                Text textFieldCreatePrivateModdedLobby = createPrivateModdedLobbyButtonTransform.GetComponentInChildren<Text>();
                textFieldCreatePrivateModdedLobby.text = "Modded game" + (!OverhaulFeatureAvailabilitySystem.ImplementedInBuild.IsCustomMultiplayerTestEnabled ? " (Coming soon)" : string.Empty);
            }

            SuccessfullyPatched = true;
        }

        private void localizeTexts()
        {
            if (!SuccessfullyPatched || m_BugReportText == null || m_SettingsText == null || OverhaulLocalizationController.Error)
            {
                return;
            }

            m_BugReportText.text = OverhaulLocalizationController.Localization.GetTranslation("TitleScreen_BugReport");
            m_SettingsText.text = OverhaulLocalizationController.Localization.GetTranslation("TitleScreen_Settings");
        }

        public override void Cancel()
        {
            base.Cancel();
            if (SuccessfullyPatched)
            {
                OverhaulEventsController.RemoveEventListener(GlobalEvents.UILanguageChanged, localizeTexts, true);
                m_ButtonsTransform.localScale = Vector3.one;
                m_ButtonsTransform.localPosition = new Vector3(0, -195.5f, 0);
                m_MultiplayerNEWButtonTransform.localPosition = new Vector3(0, -87.8241f, 0);
                m_AchievementsNewHint.localPosition = m_OriginalAchievementsNewHintPosition;

                if (m_SpawnedPanel != null)
                {
                    GameObject.Destroy(m_SpawnedPanel.gameObject);
                }
            }
        }
    }
}
