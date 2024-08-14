using OverhaulMod.Content.Personalization;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPersonalizationEditorVerificationMenu : OverhaulUIBehaviour
    {
        public const string ALL_WEAPON_VARIANTS_PRESENT_COLOR = "#262626";
        public const string ALL_WEAPON_VARIANTS_NOT_PRESENT_COLOR = "#802020";
        public const string WEAPON_VARIANT_NOT_PRESENT_COLOR = "#998523";

        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnSendButtonClicked))]
        [UIElement("SendButton")]
        private readonly Button m_sendButton;

        [UIElement("SendButtonText")]
        private readonly Text m_sendButtonText;

        [UIElement("StatusText")]
        private readonly Text m_statusText;

        [UIElement("LoadingIndicator", false)]
        private readonly GameObject m_loadingIndicator;

        [UIElement("ScrollRect")]
        private readonly Image m_scrollRectBG;

        [UIElement("WeaponVariantDisplay", false)]
        private readonly ModdedObject m_weaponVariantDisplay;

        [UIElement("Content")]
        private readonly Transform m_container;

        private bool m_currentItemIsNotFullyCompleted;

        private bool m_currentItemIsFullyIncomplete;

        public override void Show()
        {
            base.Show();
            PersonalizationEditorManager personalizationEditorManager = PersonalizationEditorManager.Instance;
            if (!personalizationEditorManager)
                return;

            PersonalizationItemInfo personalizationItemInfo = personalizationEditorManager.currentEditingItemInfo;
            if (personalizationItemInfo == null)
                return;

            RefreshItemCompletion(personalizationEditorManager.currentEditingRoot);
            RefreshButtonAndStatusText(personalizationItemInfo);
        }

        public void RefreshItemCompletion(PersonalizationEditorObjectBehaviour objectBehaviour)
        {
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            m_currentItemIsFullyIncomplete = true;
            m_currentItemIsNotFullyCompleted = false;
            string bgColor = ALL_WEAPON_VARIANTS_PRESENT_COLOR;

            PersonalizationItemVerificationManager personalizationItemVerificationManager = PersonalizationItemVerificationManager.Instance;
            foreach (WeaponVariant weaponVariant in typeof(WeaponVariant).GetEnumValues())
            {
                if (weaponVariant == WeaponVariant.None)
                    continue;

                bool value = personalizationItemVerificationManager.DoesWeaponSkinSupportWeaponVariant(objectBehaviour, weaponVariant, out bool weaponDoesHaveThisVariant);
                if (weaponDoesHaveThisVariant)
                {
                    ModdedObject moddedObject = Instantiate(m_weaponVariantDisplay, m_container);
                    moddedObject.gameObject.SetActive(true);
                    moddedObject.GetObject<Text>(0).text = WeaponVariantManager.GetWeaponVariantString(weaponVariant);
                    moddedObject.GetObject<GameObject>(1).SetActive(value);
                    moddedObject.GetObject<GameObject>(2).SetActive(!value);
                    if (!value)
                    {
                        m_currentItemIsNotFullyCompleted = true;
                        moddedObject.transform.SetAsFirstSibling();
                        bgColor = WEAPON_VARIANT_NOT_PRESENT_COLOR;
                    }
                    else
                    {
                        m_currentItemIsFullyIncomplete = false;
                    }
                }
            }

            if (m_currentItemIsFullyIncomplete)
            {
                bgColor = ALL_WEAPON_VARIANTS_NOT_PRESENT_COLOR;
            }

            m_scrollRectBG.color = ModParseUtils.TryParseToColor(bgColor, Color.gray);
        }

        public void RefreshButtonAndStatusText(PersonalizationItemInfo personalizationItemInfo)
        {
            if (!personalizationItemInfo.IsSentForVerification && !personalizationItemInfo.IsVerified)
            {
                m_statusText.text = "You haven't uploaded this item yet.";
            }
            else if (personalizationItemInfo.IsSentForVerification && !personalizationItemInfo.IsVerified)
            {
                m_statusText.text = "This item is being verified...";
            }
            else if (personalizationItemInfo.IsSentForVerification && personalizationItemInfo.IsVerified)
            {
                m_statusText.text = "This item's update is being verified...";
            }
            else
            {
                m_statusText.text = "This item is verified!\nYou can update it if you have made changes to it.";
            }

            if (personalizationItemInfo.IsVerified)
            {
                if (personalizationItemInfo.IsSentForVerification)
                {
                    m_sendButtonText.text = "Reupload update";
                }
                else
                {
                    m_sendButtonText.text = "Update item";
                }
            }
            else if (personalizationItemInfo.IsSentForVerification)
            {
                m_sendButtonText.text = "Reupload item";
            }
            else
            {
                m_sendButtonText.text = "Upload item";
            }

            m_sendButton.interactable = !m_currentItemIsFullyIncomplete && personalizationItemInfo != null && !personalizationItemInfo.ReuploadedTheItem;
        }

        public bool CanExit()
        {
            return m_exitButton.interactable;
        }

        public void OnSendButtonClicked()
        {
            if (m_currentItemIsNotFullyCompleted)
            {
                ModUIUtils.MessagePopup(true, "Item not fully completed", "Your weapon skin doesn't support some of possible weapon variants.\nDo you still want to send the item to verification?", 150f, MessageMenu.ButtonLayout.EnableDisableButtons, null, "Yes", "No", null, sendItemToVerification);
                return;
            }
            sendItemToVerification();
        }

        private void sendItemToVerification()
        {
            PersonalizationEditorManager personalizationEditorManager = PersonalizationEditorManager.Instance;
            if (!personalizationEditorManager)
                return;

            PersonalizationItemInfo personalizationItemInfo = personalizationEditorManager.currentEditingItemInfo;
            if (personalizationItemInfo == null)
                return;

            if (personalizationItemInfo.IsVerified)
                personalizationItemInfo.Version++;

            if (!personalizationEditorManager.SaveItem(out string error2, true))
            {
                UIPersonalizationEditor.instance.ShowSaveErrorMessage(error2);
                return;
            }

            m_exitButton.interactable = false;
            m_sendButton.interactable = false;
            m_loadingIndicator.SetActive(true);

            PersonalizationItemVerificationManager.Instance.SendItemToVerification(personalizationItemInfo, delegate
            {
                if (personalizationItemInfo.IsSentForVerification)
                    personalizationItemInfo.ReuploadedTheItem = true;

                personalizationItemInfo.IsSentForVerification = true;
                _ = personalizationEditorManager.SaveItem(out _, true);

                m_exitButton.interactable = true;
                m_loadingIndicator.SetActive(false);

                RefreshButtonAndStatusText(personalizationItemInfo);
                ModUIUtils.MessagePopupOK("Success", "It usually takes few hours or days to verify items.\nI'm not a robot :D", true);
            }, delegate (string error)
            {
                m_exitButton.interactable = true;
                m_sendButton.interactable = true;
                m_loadingIndicator.SetActive(true);
                ModUIUtils.MessagePopupOK("Could not send item to verification", $"If the error doesn't get fixed, it'll probably get fixed in the next mod update.\n\nError details:\n{error}", 200f, true);
            });
        }
    }
}
