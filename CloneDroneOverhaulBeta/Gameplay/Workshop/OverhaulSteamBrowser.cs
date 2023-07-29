﻿using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDOverhaul.Workshop
{
    public static class OverhaulSteamBrowser
    {
        public static readonly AppId_t AppId = new AppId_t(597170U);

        private static readonly List<ulong> m_KnownUserIDs = new List<ulong>();
        private static readonly List<PublishedFileId_t> m_DownloadingItems = new List<PublishedFileId_t>();

        #region Workshop Items

        public static void RequestItems(EUGCQuery rankBt, EUGCMatchingUGCType contentType, Action<OverhaulWorkshopRequestResult> completedCallback, ProgressInformation progressInfo, string searchText, string tags, int page, bool allowCache = false, bool getFullDescription = false, bool getInstalled = false, AccountID_t accountPublishedItems = default)
        {
            if (completedCallback == null)
                return;

            OverhaulWorkshopRequestResult requestResult = new OverhaulWorkshopRequestResult();
            ProgressInformation.SetProgress(progressInfo, 0f);

            UGCQueryHandle_t request;
            if (getInstalled)
            {
                request = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Subscribed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_SubscriptionDateDesc, AppId, AppId, (uint)page);
            }
            else
            {
                if(accountPublishedItems != default)
                {
                    request = SteamUGC.CreateQueryUserUGCRequest(accountPublishedItems, EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId, AppId, (uint)page);
                }
                else
                {
                    request = SteamUGC.CreateQueryAllUGCRequest(rankBt, contentType, AppId, AppId, (uint)page);
                }
            }

            if (request.IsInvalid())
            {
                Debug.LogWarning("[OverhaulMod] Request is invalid (Query: " + rankBt + ", Content type: " + contentType + ", Page: " + page + ", Cache: " + allowCache);
                requestResult.Error = true;
                completedCallback.Invoke(requestResult);
                return;
            }

            if (allowCache) _ = SteamUGC.SetAllowCachedResponse(request, 6);
            if (getFullDescription) _ = SteamUGC.SetReturnLongDescription(request, true);
            if (!string.IsNullOrEmpty(searchText)) _ = SteamUGC.SetSearchText(request, searchText);
            _ = SteamUGC.SetReturnAdditionalPreviews(request, true);
            _ = SteamUGC.AddRequiredTag(request, tags);

            SendUGCRequest(request, requestResult, completedCallback, progressInfo);
        }

        public static void RequestItems(EUGCQuery rank, EUGCMatchingUGCType contentType, Action<List<OverhaulWorkshopRequestResult>> completedCallback, ProgressInformation progressInfo, string searchText, string tags, int[] pages, bool cache = false, bool returnLongDescription = false)
        {
            List<OverhaulWorkshopRequestResult> results = new List<OverhaulWorkshopRequestResult>(pages.Length);
            List<ProgressInformation> progressInformations = new List<ProgressInformation>(pages.Length);
            ProgressInformation.SetProgress(progressInfo, 0f);

            foreach (int page in pages)
            {
                ProgressInformation progressInformation = new ProgressInformation();
                progressInformations.Add(progressInformation);

                RequestItems(rank, contentType, delegate (OverhaulWorkshopRequestResult result)
                {
                    ProgressInformation.SetProgress(progressInfo, (float)results.Count / pages.Length);
                    results.Add(result);
                }, progressInformation, searchText, tags, page, cache, returnLongDescription);
            }
        }

        public static void SendUGCRequest(UGCQueryHandle_t request, OverhaulWorkshopRequestResult requestResult, Action<OverhaulWorkshopRequestResult> completedCallback, ProgressInformation progressInfo)
        {
            if (completedCallback == null)
                return;

            if (requestResult == null)
                requestResult = new OverhaulWorkshopRequestResult();

            if (request.IsInvalid())
            {
                Debug.LogWarning("[OverhaulMod] Cannot send UGC request");
                requestResult.Error = true;
                completedCallback.Invoke(requestResult);
                return;
            }

            ProgressInformation.SetProgress(progressInfo, 0.4f);
            SteamAPICall_t apiCall = SteamUGC.SendQueryUGCRequest(request);
            if (apiCall.IsInvalid())
            {
                requestResult.Error = true;
                completedCallback.Invoke(requestResult);
                return;
            }

            CallResult<SteamUGCQueryCompleted_t> apiCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(delegate (SteamUGCQueryCompleted_t queryResult, bool bIOFailure)
            {
                if (bIOFailure)
                {
                    Debug.LogWarning("[OverhaulMod] APICallResult IO error!");
                    requestResult.Error = true;
                    completedCallback.Invoke(requestResult);
                    return;
                }

                requestResult.PageCount = (int)((queryResult.m_unTotalMatchingResults / 50U) + 1U);

                ProgressInformation.SetProgress(progressInfo, 0.7f);
                OverhaulWorkshopItem[] items = new OverhaulWorkshopItem[queryResult.m_unNumResultsReturned];

                int num = 0;
                while (num < (int)queryResult.m_unNumResultsReturned)
                {
                    if (SteamUGC.GetQueryUGCResult(queryResult.m_handle, (uint)num, out SteamUGCDetails_t steamUGCDetails_t) && steamUGCDetails_t.m_eResult == EResult.k_EResultOK && !steamUGCDetails_t.m_bBanned)
                    {
                        _ = SteamUGC.GetQueryUGCPreviewURL(queryResult.m_handle, (uint)num, out string url, 4096U);
                        items[num] = new OverhaulWorkshopItem(steamUGCDetails_t, url);

                        int previewCount = (int)SteamUGC.GetQueryUGCNumAdditionalPreviews(queryResult.m_handle, (uint)num);
                        if (previewCount == 0)
                        {
                            num++;
                            continue;
                        }

                        for (int i = 0; i < previewCount; i++)
                        {
                            bool apSuccess = SteamUGC.GetQueryUGCAdditionalPreview(queryResult.m_handle, (uint)num, (uint)i, out string itemURL, 512, out string name, 512, out EItemPreviewType type);
                            if (apSuccess && type == EItemPreviewType.k_EItemPreviewType_Image)
                                items[num].ItemAdditionalImages.Add(itemURL);
                        }
                    }
                    num++;
                }
                requestResult.QueryCompleted = queryResult;
                requestResult.ItemsReceived = items;
                ProgressInformation.SetProgress(progressInfo, 1f);
                completedCallback.Invoke(requestResult);
            });
            apiCallResult.Set(apiCall);
        }

        #endregion

        #region Users

        public static bool RequestInfoAboutUser(CSteamID userID)
        {
            if (m_KnownUserIDs.Contains(userID.m_SteamID))
                return false;

            bool result = SteamFriends.RequestUserInformation(userID, false);
            if (!m_KnownUserIDs.Contains(userID.m_SteamID))
                m_KnownUserIDs.Add(userID.m_SteamID);

            return result;
        }

        #endregion

        #region Items

        public static EItemState GetItemState(PublishedFileId_t id) => (EItemState)SteamUGC.GetItemState(id);
        public static bool IsSubscribedToItem(PublishedFileId_t id) => GetItemState(id).HasFlag(EItemState.k_EItemStateSubscribed);
        public static bool IsGoingToDownloadingItem(PublishedFileId_t id) => GetItemState(id).HasFlag(EItemState.k_EItemStateDownloadPending) || GetItemState(id).HasFlag(EItemState.k_EItemStateNeedsUpdate);
        public static bool IsDownloadingItem(PublishedFileId_t id) => GetItemState(id).HasFlag(EItemState.k_EItemStateDownloading);
        public static bool IsDownloadingItemInAnyWay(PublishedFileId_t id) => IsGoingToDownloadingItem(id) || IsDownloadingItem(id);
        public static bool IsItemInstalled(PublishedFileId_t id) => GetItemState(id).HasFlag(EItemState.k_EItemStateInstalled);

        public static float GetItemDownloadProgress(PublishedFileId_t id)
        {
            if (!IsDownloadingItem(id))
                return IsItemInstalled(id) ? 1f : 0f;
            _ = GetItemDownloadInfo(id, out _, out _, out float progress);
            return progress;
        }

        public static void SetItemVote(PublishedFileId_t id, bool up, Action<SetUserItemVoteResult_t, bool> callback = null)
        {
            SteamAPICall_t apiCall = SteamUGC.SetUserItemVote(id, up);
            CallResult<SetUserItemVoteResult_t> result = CallResult<SetUserItemVoteResult_t>.Create(delegate (SetUserItemVoteResult_t t, bool failure)
            {
                callback?.Invoke(t, failure);
                if (failure)
                {
                    Debug.LogWarning("[OverhaulMod] APICallResult (SetUserItemVoteResult_t) had IO error!");
                    return;
                }
            });
            result.Set(apiCall, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback">1 - Skipped, 2 - Up, 3 - Down, 4 - Fail</param>
        public static void GetItemVoteInfo(PublishedFileId_t id, Action<bool, bool, bool, bool> callback = null)
        {
            SteamAPICall_t apiCall = SteamUGC.GetUserItemVote(id);
            CallResult<GetUserItemVoteResult_t> result = CallResult<GetUserItemVoteResult_t>.Create(delegate (GetUserItemVoteResult_t t, bool failure)
            {
                callback?.Invoke(t.m_bVoteSkipped, t.m_bVotedUp, t.m_bVotedDown, failure);
                if (failure)
                {
                    Debug.LogWarning("[OverhaulMod] APICallResult (GetUserItemVoteResult_t) had IO error!");
                    return;
                }
            });
            result.Set(apiCall, null);
        }

        public static bool GetItemInstallInfo(PublishedFileId_t id, out ulong itemSizeOnDisk, out string folder, out uint lastUpdateTime) => SteamUGC.GetItemInstallInfo(id, out itemSizeOnDisk, out folder, 512, out lastUpdateTime);

        public static bool GetItemDownloadInfo(PublishedFileId_t id, out ulong downloaded, out ulong total, out float progress)
        {
            if (GetItemInstallInfo(id, out downloaded, out _, out _))
            {
                total = downloaded;
                progress = 1f;
                return false;
            }

            bool success = SteamUGC.GetItemDownloadInfo(id, out downloaded, out total);
            if (!success)
            {
                progress = 0f;
                return false;
            }

            progress = downloaded / (float)total;
            return true;
        }

        public static void UpdateItemDownloadInfo(PublishedFileId_t id, ProgressInformation progress) => ProgressInformation.SetProgress(progress, GetItemDownloadProgress(id));

        public static void SubscribeToItem(PublishedFileId_t id, Action<PublishedFileId_t, EResult> resultCallback = null)
        {
            if (!m_DownloadingItems.Contains(id))
                m_DownloadingItems.Add(id);

            CallResult<RemoteStorageSubscribePublishedFileResult_t> result = CallResult<RemoteStorageSubscribePublishedFileResult_t>.Create(delegate (RemoteStorageSubscribePublishedFileResult_t t, bool bIOFailure)
            {
                resultCallback?.Invoke(t.m_nPublishedFileId, t.m_eResult);
                _ = m_DownloadingItems.Remove(id);
                if (bIOFailure)
                {
                    Debug.LogWarning("[OverhaulMod] APICallResult (RemoteStorageSubscribePublishedFileResult_t) had IO error!");
                    return;
                }
            });
            SteamAPICall_t apiCall = SteamUGC.SubscribeItem(id);
            result.Set(apiCall, null);
        }

        public static void UnsubscribeFromItem(PublishedFileId_t id, Action<PublishedFileId_t, EResult> resultCallback = null)
        {
            if (!IsSubscribedToItem(id))
                return;

            CallResult<RemoteStorageUnsubscribePublishedFileResult_t> result = CallResult<RemoteStorageUnsubscribePublishedFileResult_t>.Create(delegate (RemoteStorageUnsubscribePublishedFileResult_t t, bool bIOFailure)
            {
                resultCallback?.Invoke(t.m_nPublishedFileId, t.m_eResult);
                if (bIOFailure)
                {
                    Debug.LogWarning("[OverhaulMod] APICallResult (RemoteStorageUnsubscribePublishedFileResult_t) had IO error!");
                    return;
                }
            });
            SteamAPICall_t apiCall = SteamUGC.UnsubscribeItem(id);
            result.Set(apiCall, null);
        }

        public static void MarkItemAsFavourite(PublishedFileId_t id, Action<PublishedFileId_t, EResult, bool> resultCallback = null)
        {
            CallResult<UserFavoriteItemsListChanged_t> result = CallResult<UserFavoriteItemsListChanged_t>.Create(delegate (UserFavoriteItemsListChanged_t t, bool bIOFailure)
            {
                resultCallback?.Invoke(t.m_nPublishedFileId, t.m_eResult, t.m_bWasAddRequest);
                if (bIOFailure)
                {
                    Debug.LogWarning("[OverhaulMod] APICallResult (UserFavoriteItemsListChanged_t) had IO error!");
                    return;
                }
            });
            SteamAPICall_t apiCall = SteamUGC.AddItemToFavorites(AppId, id);
            result.Set(apiCall, null);
        }

        public static void UnmarkItemAsFavourite(PublishedFileId_t id, Action<PublishedFileId_t, EResult, bool> resultCallback = null)
        {
            CallResult<UserFavoriteItemsListChanged_t> result = CallResult<UserFavoriteItemsListChanged_t>.Create(delegate (UserFavoriteItemsListChanged_t t, bool bIOFailure)
            {
                resultCallback?.Invoke(t.m_nPublishedFileId, t.m_eResult, t.m_bWasAddRequest);
                if (bIOFailure)
                {
                    Debug.LogWarning("[OverhaulMod] APICallResult (UserFavoriteItemsListChanged_t) had IO error!");
                    return;
                }
            });
            SteamAPICall_t apiCall = SteamUGC.RemoveItemFromFavorites(AppId, id);
            result.Set(apiCall, null);
        }

        #endregion

        #region Extensions

        public static bool IsInvalid(this UGCQueryHandle_t request, bool warn = true)
        {
            if (request == UGCQueryHandle_t.Invalid)
            {
                if (warn) Debug.LogWarning("[OverhaulMod] Request is invalid");
                return true;
            }
            return false;
        }

        public static bool IsInvalid(this SteamAPICall_t steamAPICall, bool warn = true)
        {
            if (steamAPICall == SteamAPICall_t.Invalid)
            {
                if (warn) Debug.LogWarning("[OverhaulMod] SteamAPICall is invalid");
                return true;
            }
            return false;
        }

        #endregion
    }
}
