using InternalModBot;
using ModBotWebsiteAPI;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIFeedbackMenu : OverhaulUIBehaviour
    {
        public const int CHARACTER_LIMIT = 500;

        [ModSetting(ModSettingsConstants.HAS_EVER_SENT_FEEDBACK, false)]
        public static bool HasEverSentFeedback;

        [ModSetting(ModSettingsConstants.FEEDBACK_MENU_RATE, 0)]
        public static int SavedRating;

        [ModSetting(ModSettingsConstants.FEEDBACK_MENU_IMPROVE_TEXT, null)]
        public static string SavedImproveText;

        [ModSetting(ModSettingsConstants.FEEDBACK_MENU_FAVORITE_TEXT, null)]
        public static string SavedFavoriteText;

        public static bool HasSentFeedback, HasLikedTheMod;

        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(On1RankClicked))]
        [UIElement("BadRank")]
        private readonly Button m_1rankButton;

        [UIElementAction(nameof(On2RankClicked))]
        [UIElement("MehRank")]
        private readonly Button m_2rankButton;

        [UIElementAction(nameof(On3RankClicked))]
        [UIElement("NeutralRank")]
        private readonly Button m_3rankButton;

        [UIElementAction(nameof(On4RankClicked))]
        [UIElement("GoodRank")]
        private readonly Button m_4rankButton;

        [UIElementAction(nameof(On5RankClicked))]
        [UIElement("SatisfiedRank")]
        private readonly Button m_5rankButton;

        [UIElementAction(nameof(OnSendButtonClicked))]
        [UIElement("SendButton")]
        private readonly Button m_sendButton;

        [UIElementAction(nameof(OnLikeButtonClicked))]
        [UIElement("LikeButton")]
        private readonly Button m_likeButton;

        [UIElementAction(nameof(OnExitGameButtonClicked))]
        [UIElement("ExitGameButton")]
        private readonly Button m_exitGameButton;

        [UIElement("LoadingIndicator", false)]
        private readonly GameObject m_loadingIndicator;

        [UIElementAction(nameof(OnImproveFieldChanged))]
        [UIElement("ImproveTextInputField")]
        private readonly InputField m_improveField;

        [UIElementAction(nameof(OnFavoriteFieldChanged))]
        [UIElement("FavouriteTextInputField")]
        private readonly InputField m_favoriteField;

        [UIElement("charLeftText_Improve")]
        private readonly Text m_improveFieldCharsLeftText;

        [UIElement("charLeftText_Favorite")]
        private readonly Text m_favoriteFieldCharsLeftText;

        public override bool hideTitleScreen => true;

        private bool m_refreshElementsNextFrame;

        private bool m_usedSavedSettings;

        private bool m_isSendingFeedback;

        private string m_charsLeftText;

        public int selectedRank
        {
            get;
            private set;
        }

        protected override void OnInitialized()
        {
            m_improveField.characterLimit = CHARACTER_LIMIT;
            m_favoriteField.characterLimit = CHARACTER_LIMIT;
        }

        public override void Update()
        {
            if (m_refreshElementsNextFrame)
            {
                m_refreshElementsNextFrame = false;
                refreshElements();
            }
        }

        public override void Show()
        {
            base.Show();

            m_charsLeftText = LocalizationManager.Instance.GetTranslatedString("charsleft");
            m_likeButton.interactable = !ModBotSignInUI._userName.IsNullOrEmpty() && !HasLikedTheMod;

            if (!m_usedSavedSettings)
            {
                m_usedSavedSettings = true;

                selectedRank = SavedRating;
                m_improveField.text = SavedImproveText;
                m_favoriteField.text = SavedFavoriteText;
            }

            refreshElementsNextFrame();

            m_improveFieldCharsLeftText.text = getCharLeftTextForField(m_improveField);
            m_favoriteFieldCharsLeftText.text = getCharLeftTextForField(m_favoriteField);
        }

        public override void Hide()
        {
            base.Hide();

            ModSettingsManager.SetIntValue(ModSettingsConstants.FEEDBACK_MENU_RATE, selectedRank);
            ModSettingsManager.SetStringValue(ModSettingsConstants.FEEDBACK_MENU_IMPROVE_TEXT, m_improveField.text);
            ModSettingsManager.SetStringValue(ModSettingsConstants.FEEDBACK_MENU_FAVORITE_TEXT, m_favoriteField.text);
            ModSettingsDataManager.Instance.Save();
        }

        private void refreshElementsNextFrame()
        {
            m_refreshElementsNextFrame = true;
        }

        private void refreshElements()
        {
            bool shouldBeInteractable = !HasSentFeedback && !m_isSendingFeedback;

            m_improveField.interactable = shouldBeInteractable;
            m_favoriteField.interactable = shouldBeInteractable;
            m_1rankButton.interactable = selectedRank != 1 && shouldBeInteractable;
            m_2rankButton.interactable = selectedRank != 2 && shouldBeInteractable;
            m_3rankButton.interactable = selectedRank != 3 && shouldBeInteractable;
            m_4rankButton.interactable = selectedRank != 4 && shouldBeInteractable;
            m_5rankButton.interactable = selectedRank != 5 && shouldBeInteractable;
            m_sendButton.interactable = shouldBeInteractable && selectedRank > 0 && selectedRank < 6 && !m_improveField.text.IsNullOrEmpty() && !m_improveField.text.IsNullOrWhiteSpace();

            m_loadingIndicator.SetActive(m_isSendingFeedback);
        }

        private void likeTheMod()
        {
            HasLikedTheMod = true;
            m_likeButton.interactable = false;
            API.Like("rAnDomPaTcHeS1", "true", delegate (JsonObject jsonObject)
            {
                ModUIUtils.MessagePopupOK("Successfully liked the mod!", "Thanks!", false);
            });
        }

        private string getCharLeftTextForField(InputField inputField)
        {
            return $"{inputField.characterLimit - inputField.text.Length} {m_charsLeftText}";
        }

        public void SetExitButtonVisible(bool value)
        {
            m_exitGameButton.gameObject.SetActive(value);
        }

        public void OnExitGameButtonClicked()
        {
            Hide();
            Application.Quit();
        }

        public void OnImproveFieldChanged(string text)
        {
            refreshElementsNextFrame();
            m_improveFieldCharsLeftText.text = getCharLeftTextForField(m_improveField);
        }

        public void OnFavoriteFieldChanged(string text)
        {
            refreshElementsNextFrame();
            m_favoriteFieldCharsLeftText.text = getCharLeftTextForField(m_favoriteField);
        }

        public void OnSendButtonClicked()
        {
            HasSentFeedback = true;
            m_isSendingFeedback = true;
            refreshElementsNextFrame();
            ModWebhookManager.Instance.ExecuteFeedbacksWebhook(selectedRank, m_improveField.text, m_favoriteField.text, delegate
            {
                m_isSendingFeedback = false;
                refreshElementsNextFrame();
                ModUIUtils.MessagePopupOK(LocalizationManager.Instance.GetTranslatedString("feedback_success_header"), LocalizationManager.Instance.GetTranslatedString("feedback_success_text"), true);

                if (!HasEverSentFeedback)
                {
                    ModSettingsManager.SetBoolValue(ModSettingsConstants.HAS_EVER_SENT_FEEDBACK, true);
                }
            }, delegate (string error)
            {
                m_isSendingFeedback = false;
                refreshElementsNextFrame();
                ModUIUtils.MessagePopupOK("Could not send the feedback", $"Error details:\n{error}\n\nTry again later", true);
            });
        }

        public void OnLikeButtonClicked()
        {
            likeTheMod();
        }

        public void On1RankClicked()
        {
            selectedRank = 1;
            refreshElementsNextFrame();
        }

        public void On2RankClicked()
        {
            selectedRank = 2;
            refreshElementsNextFrame();
        }

        public void On3RankClicked()
        {
            selectedRank = 3;
            refreshElementsNextFrame();
        }

        public void On4RankClicked()
        {
            selectedRank = 4;
            refreshElementsNextFrame();
        }

        public void On5RankClicked()
        {
            selectedRank = 5;
            refreshElementsNextFrame();
        }
    }
}
