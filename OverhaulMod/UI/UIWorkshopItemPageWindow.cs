using OverhaulMod.Content;
using OverhaulMod.Utils;
using Steamworks;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIWorkshopItemPageWindow : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnAuthorProfileButtonClicked))]
        [UIElement("AuthorProfileButton")]
        private readonly Button m_authorProfileButton;

        [UIElementAction(nameof(OnAuthorLevelsButtonClicked))]
        [UIElement("AuthorLevelsButton")]
        private readonly Button m_authorLevelsButton;

        [UIElementAction(nameof(OnShareButtonClicked))]
        [UIElement("ShareButton")]
        private readonly Button m_shareButton;

        [UIElementAction(nameof(OnSteamPageButtonClicked))]
        [UIElement("SteamPageButton")]
        private readonly Button m_steamPageButton;

        [UIElement("ItemTitle")]
        private readonly Text m_itemTitleText;

        [UIElement("ItemDescription")]
        private readonly Text m_itemDescriptionText;

        [UIElement("ItemAuthor")]
        private readonly Text m_itemAuthorText;

        [UIElement("ItemMainPreview")]
        private readonly RawImage m_itemPreviewImage;

        [UIElementAction(nameof(OnPreviewClicked))]
        [UIElement("ItemMainPreview")]
        private readonly Button m_itemPreviewButton;

        [UIElementAction(nameof(OnVoteUpButtonClicked))]
        [UIElement("UpVoteButton")]
        private readonly Button m_voteUpButton;

        [UIElementAction(nameof(OnVoteDownButtonClicked))]
        [UIElement("DownVoteButton")]
        private readonly Button m_voteDownButton;

        [UIElementAction(nameof(OnFavoriteButtonClicked))]
        [UIElement("AddToFavouritesButton")]
        private readonly Button m_favoriteButton;

        [UIElementAction(nameof(OnSubscribeButtonClicked))]
        [UIElement("SubscribeButton")]
        private readonly Button m_subscribeButton;

        [UIElementAction(nameof(OnUnsubscribeButtonClicked))]
        [UIElement("UnsubscribeButton")]
        private readonly Button m_unsubscribeButton;

        [UIElementAction(nameof(OnPlayButtonClicked))]
        [UIElement("PlayButton")]
        private readonly Button m_playButton;

        [ShowTooltipOnHighLight("erase progress", 1.5f, true)]
        [UIElementAction(nameof(OnEraseProgressButtonClicked))]
        [UIElement("DeleteProgressButton")]
        private readonly Button m_eraseProgressButton;

        [UIElementAction(nameof(OnUpdateButtonClicked))]
        [UIElement("UpdateButton")]
        private readonly Button m_updateButton;

        [UIElement("LBSMessage", false)]
        private readonly GameObject m_battleRoyaleMessage;

        [UIElement("EndlessMessage", false)]
        private readonly GameObject m_endlessModeMessage;

        [UIElement("FavouriteGlow", false)]
        private readonly GameObject m_favoriteButtonGlowObject;

        [UIElement("LoadingIndicator", false)]
        private readonly GameObject m_loadingIndicatorObject;

        [UIElement("NotEnoughRatingsText", false)]
        private readonly GameObject m_notEnoughRatingsTextObject;

        [UIElement("Stars", false)]
        private readonly GameObject m_starsObject;

        [UIElement("LoadingIndicatorText")]
        private readonly Text m_loadingIndicatorText;

        [UIElement("ItemAuthorAvatar")]
        private readonly RawImage m_authorAvatarImage;

        [UIElement("StarsFill")]
        private readonly Image m_ratingFillImage;

        [UIElement("UniqueVisitorsDetailText")]
        private readonly Text m_visitorsText;

        [UIElement("SubscribersDetailText")]
        private readonly Text m_subscribersText;

        [UIElement("FavouritesDetailText")]
        private readonly Text m_favoritesText;

        [UIElement("FileSizeDetailText")]
        private readonly Text m_fileSizeText;

        [UIElement("PostTimeDetailText")]
        private readonly Text m_postTimeText;

        [UIElement("UpdateTimeDetailText")]
        private readonly Text m_updateTimeText;

        [UIElement("TagsText")]
        private readonly Text m_tagsText;

        [UIElement("ItemImageDisplay", false)]
        private readonly ModdedObject m_additionalPreviewDisplayPrefab;

        [UIElement("ItemImageContainer")]
        private readonly Transform m_additionalPreviewDisplayContainer;

        [UIElement("NamePanel")]
        private readonly RectTransform m_namePanel;

        [UIElement("Name")]
        private readonly RectTransform m_nameHolder;

        /*
        [UIElement("Panel", typeof(UIElementMouseEventsComponent))]
        private readonly UIElementMouseEventsComponent m_panel;*/

        private string m_authorProfileLink, m_itemLink;
        private CSteamID m_authorId;

        private string m_previewLink;
        private Texture2D m_previewTexture, m_authorAvatarTexture;

        private UnityWebRequest m_webRequest;

        private UIElementShowTooltipOnHightLight m_tooltipOnHightLight;

        private WorkshopItem m_workshopItem;

        private bool m_refreshDisplaysNextFrame;

        private float m_makeButtonsInteractableInTime;

        private float m_timeLeftToRefreshDisplays;

        private float m_timeLeftToResumeTicker;
        private float m_tickerProgress;
        private bool m_tickerIsGoingLeft;

        public float EaseMultiplier;

        public UIWorkshopBrowser browserUI
        {
            get;
            set;
        }

        public bool isImageViewerShown
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            UIElementShowTooltipOnHightLight tooltipOnHightLight = m_itemTitleText.gameObject.AddComponent<UIElementShowTooltipOnHightLight>();
            tooltipOnHightLight.tooltipShowDuration = 2f;
            tooltipOnHightLight.InitializeElement();
            m_tooltipOnHightLight = tooltipOnHightLight;

            EaseMultiplier = 50f;
        }

        public override void Show()
        {
            base.Show();
            m_tickerProgress = 0f;
            m_tickerIsGoingLeft = false;
            m_timeLeftToResumeTicker = 1f;

            RectTransform nameHolder = m_nameHolder;
            Vector2 vector = nameHolder.anchoredPosition;
            vector.x = 0f;
            nameHolder.anchoredPosition = vector;
        }

        public override void Update()
        {
            base.Update();

            float d = Time.unscaledDeltaTime;
            if (m_timeLeftToResumeTicker > 0f)
                m_timeLeftToResumeTicker -= d;

            if (m_refreshDisplaysNextFrame)
            {
                m_refreshDisplaysNextFrame = false;

                refreshManagementDisplays(m_workshopItem);
            }

            if (m_makeButtonsInteractableInTime > 0f)
                m_makeButtonsInteractableInTime -= d;

            if (m_makeButtonsInteractableInTime <= 0f)
            {
                m_makeButtonsInteractableInTime = -1f;
                m_subscribeButton.interactable = true;
                m_unsubscribeButton.interactable = true;
                m_updateButton.interactable = true;
            }

            m_timeLeftToRefreshDisplays -= d;
            if (m_timeLeftToRefreshDisplays <= 0f)
            {
                m_refreshDisplaysNextFrame = true;
                m_timeLeftToRefreshDisplays = 0.1f;
            }

            RectTransform namePanel = m_namePanel;
            RectTransform nameHolder = m_nameHolder;
            float preferredWidth = LayoutUtility.GetPreferredWidth(m_itemTitleText.rectTransform);
            float xa = 0f;
            float xb = Mathf.Min(namePanel.rect.width - preferredWidth, 0f);

            if (xb != 0f && m_timeLeftToResumeTicker <= 0f)
            {
                float xbPositive = -xb;
                float toAdd = d * (1f / Mathf.Clamp(xbPositive, 100f, 600f)) * EaseMultiplier;

                if (m_tickerIsGoingLeft)
                {
                    m_tickerProgress -= toAdd;
                    if (m_tickerProgress <= 0f)
                    {
                        m_tickerIsGoingLeft = false;
                        m_tickerProgress = 0f;
                    }
                }
                else
                {
                    m_tickerProgress += toAdd;
                    if (m_tickerProgress >= 1f)
                    {
                        m_tickerIsGoingLeft = true;
                        m_tickerProgress = 1f;
                    }
                }

                Vector2 vector = nameHolder.anchoredPosition;
                vector.x = Mathf.Lerp(xa, xb, NumberUtils.EaseInOutCubic(0f, 1f, m_tickerProgress));
                nameHolder.anchoredPosition = vector;
            }
            else
            {
                Vector2 vector = nameHolder.anchoredPosition;
                vector.x = 0f;
                nameHolder.anchoredPosition = vector;
            }

            /*
            if (Input.GetMouseButtonDown(0) && !m_panel.isMouseOverElement && !isImageViewerShown)
            {
                Hide();
            }*/
        }

        public override void OnDisable()
        {
            dispose();
        }

        public void Populate(WorkshopItem workshopItem)
        {
            dispose();
            if (workshopItem == null || workshopItem.IsDisposed())
                return;

            bool isChallengeOrAdventure = workshopItem.IsChallengeOrAdventure();
            string path = DataRepository.Instance.GetFullPath($"ChallengeData{workshopItem.ItemID}", false);
            m_workshopItem = workshopItem;

            m_playButton.interactable = true;

            m_eraseProgressButton.gameObject.SetActive(isChallengeOrAdventure);
            m_eraseProgressButton.interactable = isChallengeOrAdventure && File.Exists(path);

            m_itemTitleText.text = workshopItem.Name;
            m_itemDescriptionText.text = workshopItem.Description;

            if (!workshopItem.Author.IsNullOrEmpty() && workshopItem.Author != "[unknown]")
                m_itemAuthorText.text = $"{LocalizationManager.Instance.GetTranslatedString("workshop_leveldetails_author")} {workshopItem.Author.AddColor(Color.white)}";
            else
                m_itemAuthorText.text = $"{LocalizationManager.Instance.GetTranslatedString("workshop_leveldetails_author")} {workshopItem.AuthorID.ToString().AddColor(Color.white)}";

            m_battleRoyaleMessage.SetActive(workshopItem.IsLastBotStandingLevel());
            m_endlessModeMessage.SetActive(workshopItem.IsEndlessLevel());

            m_itemLink = $"https://steamcommunity.com/sharedfiles/filedetails/?id={workshopItem.ItemID}";
            m_authorProfileLink = $"https://steamcommunity.com/profiles/{workshopItem.AuthorID}";
            m_authorId = workshopItem.AuthorID;

            m_tooltipOnHightLight.tooltipText = workshopItem.Name;

            getMainPreview(workshopItem);
            getAuthorAvatar(workshopItem);
            populateDetails(workshopItem);
            populateAdditionalPreviews(workshopItem);
            refreshManagementDisplays(workshopItem);
            refreshUserVote(workshopItem);
        }

        public void SetFavoriteButtonInteractable(bool value)
        {
            m_favoriteButton.interactable = value;
            m_favoriteButtonGlowObject.SetActive(!value);
        }

        private void getMainPreview(WorkshopItem workshopItem)
        {
            m_itemPreviewImage.gameObject.SetActive(false);

            string link = workshopItem.PreviewURL;
            m_previewLink = link;

            UIWorkshopItemPageWindow itemPageWindow = this;
            RepositoryManager.Instance.GetCustomTexture(link, delegate (Texture2D texture)
            {
                if (!itemPageWindow || link != m_previewLink)
                {
                    if (texture)
                        Destroy(texture);

                    return;
                }

                m_previewTexture = texture;
                m_itemPreviewImage.gameObject.SetActive(true);
                m_itemPreviewImage.texture = texture;
                m_itemPreviewImage.rectTransform.sizeDelta = new Vector2(Mathf.Min(145f * (texture.width / (float)texture.height), 257.7778f), 145f);
            }, null, out m_webRequest, 60);
        }

        private void getAuthorAvatar(WorkshopItem workshopItem)
        {
            m_authorAvatarImage.gameObject.SetActive(false);

            int handle = SteamFriends.GetMediumFriendAvatar(workshopItem.AuthorID);
            if (SteamUtils.GetImageSize(handle, out uint width, out uint height))
            {
                byte[] bytes = new byte[width * height * 4];
                if (SteamUtils.GetImageRGBA(handle, bytes, bytes.Length))
                {
                    try
                    {
                        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                        texture.LoadRawTextureData(bytes);
                        texture.Apply();
                        m_authorAvatarTexture = texture;
                        m_authorAvatarImage.texture = texture;
                        m_authorAvatarImage.gameObject.SetActive(true);
                    }
                    catch
                    {
                        m_authorAvatarImage.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void populateDetails(WorkshopItem workshopItem)
        {
            string tagsText = "-";
            string[] tagsArray = workshopItem.Tags;
            if (!tagsArray.IsNullOrEmpty())
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                for (int i = 0; i < tagsArray.Length; i++)
                {
                    _ = stringBuilder.Append(tagsArray[i]);
                    if (i < tagsArray.Length - 1)
                        _ = stringBuilder.Append(", ");
                }
                tagsText = stringBuilder.ToString();
            }

            string postTimeText = $"{workshopItem.PostDate.ToShortDateString()}, {workshopItem.PostDate.ToShortTimeString()}";
            string updateTimeText = $"{workshopItem.UpdateDate.ToShortDateString()}, {workshopItem.UpdateDate.ToShortTimeString()}";
            string sizeText = $"{Mathf.Round(workshopItem.Size * 100f) / 100f} MBs";

            float rating = Mathf.Ceil(workshopItem.Rating * 5f);

            m_ratingFillImage.fillAmount = rating / 5f;
            m_notEnoughRatingsTextObject.SetActive(rating < 1f);
            m_starsObject.SetActive(rating >= 1f);
            m_visitorsText.text = workshopItem.Views.ToString();
            m_subscribersText.text = workshopItem.Subscribers.ToString();
            m_favoritesText.text = workshopItem.Favorites.ToString();
            m_postTimeText.text = postTimeText;
            m_updateTimeText.text = updateTimeText;
            m_fileSizeText.text = sizeText;
            m_tagsText.text = tagsText;
        }

        private void populateAdditionalPreviews(WorkshopItem workshopItem)
        {
            if (m_additionalPreviewDisplayContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_additionalPreviewDisplayContainer);

            if (workshopItem.AdditionalPreviews.IsNullOrEmpty())
                return;

            foreach (WorkshopItemPreview preview in workshopItem.AdditionalPreviews.OrderBy(f => f.PreviewType != EItemPreviewType.k_EItemPreviewType_YouTubeVideo))
            {
                if (preview.URL.IsNullOrEmpty())
                    continue;

                ModdedObject moddedObject = Instantiate(m_additionalPreviewDisplayPrefab, m_additionalPreviewDisplayContainer);
                moddedObject.gameObject.SetActive(true);
                UIElementWorkshopItemPreviewDisplay workshopItemPreviewDisplay = moddedObject.gameObject.AddComponent<UIElementWorkshopItemPreviewDisplay>();
                workshopItemPreviewDisplay.isVideo = preview.PreviewType != EItemPreviewType.k_EItemPreviewType_Image;
                workshopItemPreviewDisplay.link = preview.URL;
                workshopItemPreviewDisplay.imageViewerOpenedCallback = onImageViewerOpened;
                workshopItemPreviewDisplay.imageViewerClosedCallback = onImageViewerClosed;
                workshopItemPreviewDisplay.InitializeElement();
            }
        }

        private void refreshManagementDisplays(WorkshopItem workshopItem)
        {
            if (workshopItem == null || workshopItem.IsDisposed())
                return;

            EItemState itemState = ModSteamUGCUtils.GetItemState(workshopItem.ItemID);
            bool installed = ModSteamUGCUtils.IsItemInstalled(workshopItem.ItemID);
            bool subscribed = itemState.HasFlag(EItemState.k_EItemStateSubscribed);
            bool downloading = itemState.HasFlag(EItemState.k_EItemStateDownloading) || itemState.HasFlag(EItemState.k_EItemStateDownloadPending);
            bool needsUpdate = itemState.HasFlag(EItemState.k_EItemStateNeedsUpdate);
            bool allowPlayingFromThere = workshopItem.IsChallengeOrAdventure();

            m_subscribeButton.gameObject.SetActive(!subscribed && !downloading);
            m_unsubscribeButton.gameObject.SetActive(subscribed);
            m_playButton.gameObject.SetActive(allowPlayingFromThere && installed && subscribed && !downloading);
            m_updateButton.gameObject.SetActive(installed && subscribed && !downloading && needsUpdate);

            m_loadingIndicatorObject.SetActive(downloading || needsUpdate);
            if (m_loadingIndicatorObject.activeSelf)
                m_loadingIndicatorText.text = $"{LocalizationManager.Instance.GetTranslatedString("downloading...")}  {(Mathf.RoundToInt(Mathf.Clamp01(ModSteamUGCUtils.GetItemDownloadProgress(workshopItem.ItemID)) * 100f).ToString() + "%").AddColor(Color.white)}";
        }

        private void refreshUserVote(WorkshopItem workshopItem)
        {
            WorkshopItem item = workshopItem;
            if (item == null || item.IsDisposed())
                return;

            m_voteUpButton.gameObject.SetActive(false);
            m_voteDownButton.gameObject.SetActive(false);
            SetFavoriteButtonInteractable(true);

            ModSteamUGCUtils.GetUserVote(item.ItemID, delegate (WorkshopItemVote workshopItemVote)
            {
                WorkshopItem item2 = m_workshopItem;
                if (item != item2 || item2 == null || item2.IsDisposed())
                    return;

                m_voteUpButton.gameObject.SetActive(true);
                m_voteDownButton.gameObject.SetActive(true);

                if (!workshopItemVote.HasVoted)
                {
                    m_voteUpButton.interactable = true;
                    m_voteDownButton.interactable = true;
                    return;
                }
                m_voteUpButton.interactable = !workshopItemVote.VoteValue;
                m_voteDownButton.interactable = workshopItemVote.VoteValue;
            });
        }

        private void onImageViewerOpened()
        {
            isImageViewerShown = true;
        }

        private void onImageViewerClosed()
        {
            isImageViewerShown = false;
        }

        private void dispose()
        {
            m_workshopItem = null;

            if (m_webRequest != null)
            {
                try
                {
                    m_webRequest.Abort();
                    m_webRequest = null;
                }
                catch { }
            }

            Texture2D mp = m_previewTexture;
            if (mp)
                Destroy(mp);

            Texture2D aat = m_authorAvatarTexture;
            if (aat)
                Destroy(aat);
        }

        public void OnPreviewClicked()
        {
            Texture2D texture = m_previewTexture;
            if (!texture)
                return;

            onImageViewerOpened();
            ModUIUtils.ImageViewer(texture, base.transform, onImageViewerClosed);
        }

        public void OnVoteUpButtonClicked()
        {
            WorkshopItem item = m_workshopItem;
            if (item == null || item.IsDisposed())
                return;

            m_voteUpButton.interactable = false;
            ModSteamUGCUtils.SetUserVote(item.ItemID, true, delegate (SetUserItemVoteResult_t t, bool ioError)
            {
                WorkshopItem item2 = m_workshopItem;
                if (item != item2 || item2 == null || item2.IsDisposed())
                    return;

                if (ioError || t.m_eResult != EResult.k_EResultOK)
                    ModUIUtils.MessagePopupOK("Vote error", $"Error code: {t.m_eResult} (ioError: {ioError})", 150f, true);
                else
                {
                    m_voteDownButton.interactable = t.m_bVoteUp;
                    m_voteUpButton.interactable = !t.m_bVoteUp;
                    return;
                }

                m_voteUpButton.interactable = true;
            });
        }

        public void OnVoteDownButtonClicked()
        {
            WorkshopItem item = m_workshopItem;
            if (item == null || item.IsDisposed())
                return;

            m_voteDownButton.interactable = false;
            ModSteamUGCUtils.SetUserVote(item.ItemID, false, delegate (SetUserItemVoteResult_t t, bool ioError)
            {
                WorkshopItem item2 = m_workshopItem;
                if (item != item2 || item2 == null || item2.IsDisposed())
                    return;

                if (ioError || t.m_eResult != EResult.k_EResultOK)
                    ModUIUtils.MessagePopupOK("Vote error", $"Error code: {t.m_eResult} (ioError: {ioError})", 150f, true);
                else
                {
                    m_voteDownButton.interactable = t.m_bVoteUp;
                    m_voteUpButton.interactable = !t.m_bVoteUp;
                    return;
                }

                m_voteDownButton.interactable = true;
            });
        }

        public void OnFavoriteButtonClicked()
        {
            ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("workshop_favorite_this_item_header"), string.Empty, 125f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
            {
                WorkshopItem item = m_workshopItem;
                if (item == null || item.IsDisposed())
                    return;

                ModSteamUGCUtils.AddItemToFavorites(item.ItemID, delegate (UserFavoriteItemsListChanged_t t, bool ioError)
                {
                    WorkshopItem item2 = m_workshopItem;
                    if (item != item2 || item2 == null || item2.IsDisposed())
                        return;

                    if (ioError || t.m_eResult != EResult.k_EResultOK)
                    {
                        ModUIUtils.MessagePopupOK("Mark item as favorite error", $"Error code:{t.m_eResult} (ioError: {ioError})", 150f, true);
                    }
                    else
                    {
                        SetFavoriteButtonInteractable(false);
                        return;
                    }

                    SetFavoriteButtonInteractable(true);
                });

                SetFavoriteButtonInteractable(false);
            });
        }

        public void OnSubscribeButtonClicked()
        {
            WorkshopItem item = m_workshopItem;
            if (item == null || item.IsDisposed())
                return;

            ModSteamUGCUtils.SubscribeItem(item.ItemID, delegate (RemoteStorageSubscribePublishedFileResult_t t, bool ioError)
            {
                WorkshopItem item2 = m_workshopItem;
                if (item != item2 || item2 == null || item2.IsDisposed())
                    return;

                if (ioError || t.m_eResult != EResult.k_EResultOK)
                    ModUIUtils.MessagePopupOK("Subscription error", $"Error code: {t.m_eResult} (ioError: {ioError})", 150f, true);
                else
                    m_refreshDisplaysNextFrame = true;
            });
            m_refreshDisplaysNextFrame = true;

            m_makeButtonsInteractableInTime = 2f;
            m_subscribeButton.interactable = false;
        }

        public void OnUnsubscribeButtonClicked()
        {
            WorkshopItem item = m_workshopItem;
            if (item == null || item.IsDisposed())
                return;

            ModSteamUGCUtils.UnsubscribeItem(item.ItemID, delegate (RemoteStorageUnsubscribePublishedFileResult_t t, bool ioError)
            {
                WorkshopItem item2 = m_workshopItem;
                if (item != item2 || item2 == null || item2.IsDisposed())
                    return;

                if (ioError || t.m_eResult != EResult.k_EResultOK)
                    ModUIUtils.MessagePopupOK("Unsubscription error", $"Error code: {t.m_eResult} (ioError: {ioError})", 150f, true);
                else
                    m_refreshDisplaysNextFrame = true;
            });
            m_refreshDisplaysNextFrame = true;

            m_makeButtonsInteractableInTime = 2f;
            m_unsubscribeButton.interactable = false;
        }

        public void OnPlayButtonClicked()
        {
            WorkshopItem item = m_workshopItem;
            if (item == null || item.IsDisposed())
                return;

            if (SteamUGC.GetItemInstallInfo(item.ItemID, out _, out string folder, ModSteamUGCUtils.cchFolderSize, out _))
                item.Folder = folder;

            if (!WorkshopChallengeManager.Instance.StartChallengeFromWorkshop(item.ToSteamWorkshopItem()))
            {
                ModUIUtils.MessagePopupOK("Incompatible game version!", "This item was made on newer version of the game.\nTo become able to play this level, update the game.", true);
                m_playButton.interactable = false;
            }
            else
            {
                Hide();
                browserUI.Hide();
            }
        }

        public void OnEraseProgressButtonClicked()
        {
            WorkshopItem item = m_workshopItem;
            if (item == null || item.IsDisposed())
                return;

            string path = DataRepository.Instance.GetFullPath($"ChallengeData{item.ItemID}", false);
            ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("reset_progress_header"), LocalizationManager.Instance.GetTranslatedString("action_cannot_be_undone"), 125f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
            {
                if (File.Exists(path))
                    try
                    {
                        File.Delete(path);
                        m_eraseProgressButton.interactable = false;
                    }
                    catch (Exception exception)
                    {
                        ModUIUtils.MessagePopupOK("Error", $"Could not erase save file.\nDetails: {exception}");
                    }
            });
        }

        public void OnOptionsButtonClicked()
        {

        }

        public void OnSetLevelOnTitleScreenButtonClicked()
        {

        }

        public void OnUpdateButtonClicked()
        {
            WorkshopItem item = m_workshopItem;
            if (item == null || item.IsDisposed())
                return;

            _ = ModSteamUGCUtils.UpdateItem(item.ItemID, delegate (DownloadItemResult_t t)
            {
                WorkshopItem item2 = m_workshopItem;
                if (item != item2 || item2 == null || item2.IsDisposed())
                    return;

                if (t.m_unAppID == SteamUtils.GetAppID() && t.m_nPublishedFileId == item2.ItemID)
                {
                    if (t.m_eResult != EResult.k_EResultOK)
                        ModUIUtils.MessagePopupOK("Update error", $"Error code: {t.m_eResult}", 150f, true);
                    else
                        m_refreshDisplaysNextFrame = true;
                }
            });
            m_refreshDisplaysNextFrame = true;

            m_makeButtonsInteractableInTime = 2f;
            m_updateButton.interactable = false;
        }

        public void OnSteamPageButtonClicked()
        {
            string link = m_itemLink;
            if (SteamManager.Instance && SteamManager.Instance.Initialized && SteamUtils.IsOverlayEnabled())
                SteamFriends.ActivateGameOverlayToWebPage(link);
            else
                Application.OpenURL(link);
        }

        public void OnShareButtonClicked()
        {
            GUIUtility.systemCopyBuffer = m_itemLink;
            ModUIUtils.MessagePopupOK(LocalizationManager.Instance.GetTranslatedString("workshop_link_copied"), string.Empty, false);
        }

        public void OnAuthorProfileButtonClicked()
        {
            string link = m_authorProfileLink;
            if (SteamManager.Instance && SteamManager.Instance.Initialized && SteamUtils.IsOverlayEnabled())
                SteamFriends.ActivateGameOverlayToWebPage(link);
            else
                Application.OpenURL(link);
        }

        public void OnAuthorLevelsButtonClicked()
        {
            UIWorkshopBrowser bui = browserUI;
            bui.searchLevelsByUser = m_authorId;
            bui.searchUserList = EUserUGCList.k_EUserUGCList_Published;
            bui.sourceType = 1;
            bui.browseCollections = false;
            bui.browseChildrenOfCollection = default;
            bui.Populate();
            Hide();
        }
    }
}
