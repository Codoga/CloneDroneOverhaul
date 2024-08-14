 using OverhaulMod.Content.Personalization;
using OverhaulMod.UI.Elements;
using OverhaulMod.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPersonalizationEditor : OverhaulUIBehaviour
    {
        private static List<UIElementPersonalizationEditorDropdown.OptionData> s_fileOptions, s_viewOptions, s_windowOptions, s_helpOptions;

        [UIElementAction(nameof(OnExitButtonClicked))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnSelectItemButtonClicked))]
        [UIElement("SelectItemButton")]
        private readonly Button m_selectItemButton;

        [UIElementAction(nameof(OnSaveButtonClicked))]
        [UIElement("SaveButton")]
        private readonly Button m_saveButton;

        [UIElementAction(nameof(OnSendToVerificationButtonClicked))]
        [UIElement("SendToVerificationButton")]
        private readonly Button m_sendToVerificationButton;

        [UIElementAction(nameof(OnPlaytestButtonClicked))]
        [UIElement("PlaytestButton")]
        private readonly Button m_playtestButton;

        [UIElement("DeveloperPanel", false)]
        private readonly RectTransform m_developerPanel;

        [UIElement("ToolBar")]
        public RectTransform ToolBarTransform;

        [UIElement("InspectorWindow", typeof(UIElementPersonalizationEditorInspector), false)]
        public readonly UIElementPersonalizationEditorInspector Inspector;

        [UIElement("BottomBar", typeof(UIElementPersonalizationEditorUtilitiesPanel), false)]
        public readonly UIElementPersonalizationEditorUtilitiesPanel Utilities;

        [UIElement("ObjectPropertiesWindow", typeof(UIElementPersonalizationEditorPropertiesPanel), false)]
        public readonly UIElementPersonalizationEditorPropertiesPanel PropertiesPanel;

        [UIElement("Dropdown", typeof(UIElementPersonalizationEditorDropdown), false)]
        public readonly UIElementPersonalizationEditorDropdown Dropdown;

        [UIElement("Notification", typeof(UIElementPersonalizationEditorNotification), false)]
        public readonly UIElementPersonalizationEditorNotification Notification;

        [UIElement("GuideWindow", typeof(UIElementPersonalizationEditorGuideWindow), false)]
        public readonly UIElementPersonalizationEditorGuideWindow GuideWindow;

        [UIElementAction(nameof(OnFileButtonClicked))]
        [UIElement("FileButton")]
        private readonly Button m_toolbarFileButton;

        [UIElementAction(nameof(OnViewButtonClicked))]
        [UIElement("ViewButton")]
        private readonly Button m_toolbarViewButton;

        [UIElementAction(nameof(OnWindowButtonClicked))]
        [UIElement("WindowButton")]
        private readonly Button m_toolbarWindowButton;

        [UIElementAction(nameof(OnHelpButtonClicked))]
        [UIElement("HelpButton")]
        private readonly Button m_toolbarHelpButton;

        [UIElementAction(nameof(OnUploadButtonClicked))]
        [UIElement("UploadButton")]
        private readonly Button m_toolbarUploadButton;

        public string InspectorWindowID, DeveloperWindowID, ObjectPropertiesWindowID;

        public override bool enableCursor => true;

        public override bool closeOnEscapeButtonPress => false;

        public static UIPersonalizationEditor instance
        {
            get;
            private set;
        }

        protected override void OnInitialized()
        {
            instance = this;
            s_fileOptions = new List<UIElementPersonalizationEditorDropdown.OptionData>()
            {
                new UIElementPersonalizationEditorDropdown.OptionData("Open", "Redirect-16x16", instance.OnSelectItemButtonClicked),
                new UIElementPersonalizationEditorDropdown.OptionData("Save (Ctrl+S)", "Save16x16", instance.OnSaveButtonClicked),
                new UIElementPersonalizationEditorDropdown.OptionData(true),
                new UIElementPersonalizationEditorDropdown.OptionData("Exit", "Exit-V2-16x16", instance.OnExitButtonClicked),
            };

            s_viewOptions = new List<UIElementPersonalizationEditorDropdown.OptionData>()
            {
                new UIElementPersonalizationEditorDropdown.OptionData("Welcome message", "Redirect-16x16", PersonalizationEditorManager.Instance.WelcomeMessage),
            };

            s_windowOptions = new List<UIElementPersonalizationEditorDropdown.OptionData>()
            {
                new UIElementPersonalizationEditorDropdown.OptionData("Show item info editor", "Redirect-16x16", ShowInspector),
                new UIElementPersonalizationEditorDropdown.OptionData("Show object editor", "Redirect-16x16", ShowObjectProperties),
                new UIElementPersonalizationEditorDropdown.OptionData(true),
                new UIElementPersonalizationEditorDropdown.OptionData("Show item moderator", "Redirect-16x16", ShowItemModerator),
            };

            s_helpOptions = new List<UIElementPersonalizationEditorDropdown.OptionData>
            {
                new UIElementPersonalizationEditorDropdown.OptionData("Welcome message", "Redirect-16x16", PersonalizationEditorManager.Instance.WelcomeMessage),
                new UIElementPersonalizationEditorDropdown.OptionData("Guide: Introduction", "Redirect-16x16", DropdownGuide),
            };
            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.PersonalizationEditorTutorialVideo)) s_helpOptions.Add(new UIElementPersonalizationEditorDropdown.OptionData("Tutorial video", "Redirect-16x16", PersonalizationEditorManager.Instance.TutorialVideo));
            s_helpOptions.Add(new UIElementPersonalizationEditorDropdown.OptionData("About", "Redirect-16x16", OnAboutButtonClicked));

            m_toolbarWindowButton.interactable = false;
            m_toolbarUploadButton.interactable = false;

            DelegateScheduler.Instance.Schedule(delegate
            {
                Guide();
            }, 3f);
        }

        public override void Update()
        {
            if (InputManager.Instance.GetKeyMode() != KeyMode.EditingInputField)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
                {
                    OnSaveButtonClicked();
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            instance = null;
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public void DropdownGuide()
        {
            Dropdown.Hide();
            Guide();
        }

        public void Guide()
        {
            GuideWindow.StartGuide(PersonalizationEditorGuideManager.Instance.GetGuide("getting_started"));
        }

        public void ShowNotification(string header, string text, Color baseColor, float duration = 7f)
        {
            Notification.ShowNotification(header, text, baseColor, duration);
        }

        public void ShowErrorNotification(string header, string text, float duration = 7f)
        {
            Notification.ShowNotification(header, text, UIElementPersonalizationEditorNotification.ErrorColor, duration);
        }

        public void ShowSaveErrorMessage(string message)
        {
            ShowErrorNotification("Could not save the item", message, 15f);
        }

        public void ShowEverything()
        {
            m_toolbarWindowButton.interactable = true;
            m_toolbarUploadButton.interactable = true;
            ShowInspector();
            ShowObjectProperties();
            ShowItemModerator();
        }

        public void ShowInspector()
        {
            ModUIManager.WindowManager windowManager = ModUIManager.Instance.windowManager;
            if (InspectorWindowID == null)
                InspectorWindowID = windowManager.Window(base.transform, Inspector.transform, "Edit item info", Vector2.one * -1f, (Vector2.right * -250f) + (Vector2.up * 220f));
            else
                windowManager.ShowWindow(InspectorWindowID);

            ModUIManager.WindowBehaviour windowBehaviour = windowManager.GetWindow(InspectorWindowID);
            windowBehaviour.transform.localScale = Vector3.one * 0.85f;
        }

        public void ShowObjectProperties()
        {
            ModUIManager.WindowManager windowManager = ModUIManager.Instance.windowManager;
            if (ObjectPropertiesWindowID == null)
                ObjectPropertiesWindowID = windowManager.Window(base.transform, PropertiesPanel.transform, "Edit object", Vector2.one * -1f, (Vector2.right * 250f) + (Vector2.up * 220f));
            else
                windowManager.ShowWindow(ObjectPropertiesWindowID);

            ModUIManager.WindowBehaviour windowBehaviour = windowManager.GetWindow(ObjectPropertiesWindowID);
            windowBehaviour.transform.localScale = Vector3.one * 0.85f;
        }

        public void ShowItemModerator()
        {
            TryShowItemModerator(false);
        }

        public void TryShowItemModerator(bool withMessage)
        {
            if (PersonalizationEditorManager.Instance.canVerifyItems)
            {
                ModUIManager.WindowManager windowManager = ModUIManager.Instance.windowManager;
                if (DeveloperWindowID == null)
                    DeveloperWindowID = windowManager.Window(base.transform, m_developerPanel, "Item moderator", Vector2.one * -1f, Vector2.up * -120f);
                else
                    windowManager.ShowWindow(DeveloperWindowID);

                ModUIManager.WindowBehaviour windowBehaviour = windowManager.GetWindow(DeveloperWindowID);
                windowBehaviour.transform.localScale = Vector3.one * 0.85f;
            }
            else if (withMessage)
            {
                ModUIUtils.MessagePopupOK("Item moderator is not available", "This panel is made for developers.", 150f, true);
            }
        }

        public void OnExitButtonClicked()
        {
            Dropdown.Hide();
            ModUIUtils.MessagePopup(true, "Exit editor?", "Make sure you have saved your progress.", 150f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes, exit", "No", null, SceneTransitionManager.Instance.DisconnectAndExitToMainMenu, null);
        }

        public void OnSelectItemButtonClicked()
        {
            Dropdown.Hide();
            _ = ModUIConstants.ShowPersonalizationEditorItemsBrowser(base.transform);
        }

        public void OnSaveButtonClicked()
        {
            Dropdown.Hide();
            if (!PersonalizationEditorManager.Instance.SaveItem(out string error))
                ShowSaveErrorMessage(error);
            else
                ShowNotification("Success", $"Saved the item ({PersonalizationEditorManager.Instance.currentEditingItemInfo.Name})", UIElementPersonalizationEditorNotification.SuccessColor);
        }

        public void OnPlaytestButtonClicked()
        {
            Utilities.SetAnimationToggleOn();
            PersonalizationEditorManager.Instance.EnterPlaytestMode();
        }

        public void OnSendToVerificationButtonClicked()
        {
            Dropdown.Hide();
            _ = ModUIConstants.ShowPersonalizationEditorVerificationMenu(base.transform);
        }

        public void OnUploadButtonClicked()
        {
            Dropdown.Hide();
            _ = ModUIConstants.ShowPersonalizationEditorVerificationMenu(base.transform);
        }

        public void OnAboutButtonClicked()
        {
            Dropdown.Hide();
            _ = ModUIConstants.ShowPersonalizationEditorAboutDialog(base.transform);
        }

        public void OnFileButtonClicked()
        {
            Dropdown.ShowWithOptions(s_fileOptions, m_toolbarFileButton.transform as RectTransform);
        }

        public void OnViewButtonClicked()
        {
            Dropdown.ShowWithOptions(s_viewOptions, m_toolbarViewButton.transform as RectTransform);
        }

        public void OnWindowButtonClicked()
        {
            Dropdown.ShowWithOptions(s_windowOptions, m_toolbarWindowButton.transform as RectTransform);
        }

        public void OnHelpButtonClicked()
        {
            Dropdown.ShowWithOptions(s_helpOptions, m_toolbarHelpButton.transform as RectTransform);
        }
    }
}
