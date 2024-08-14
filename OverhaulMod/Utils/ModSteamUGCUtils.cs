using OverhaulMod.Content;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Utils
{
    public static class ModSteamUGCUtils
    {
        public const uint cchURLSize = 4096U;
        public const uint cchFolderSize = 4096U;

        /// <summary>
        /// Amount of pages from last query execution
        /// </summary>
        public static int pageCount
        {
            get;
            private set;
        }

        public static float GetItemDownloadProgress(PublishedFileId_t workshopItemId)
        {
            if (SteamUGC.GetItemDownloadInfo(workshopItemId, out ulong downloaded, out ulong total))
            {
                float a = downloaded;
                float b = total;

                bool isAIncorrectNumber = float.IsInfinity(a) || float.IsNaN(a);
                bool isBIncorrectNumber = float.IsInfinity(b) || float.IsNaN(b);
                if (isAIncorrectNumber && isBIncorrectNumber)
                    return -1f;
                else
                {
                    if (isAIncorrectNumber)
                        a = 0f;

                    if (isBIncorrectNumber)
                        b = 1f;
                }

                return a / Mathf.Max(0.01f, b);
            }
            return -1f;
        }

        public static bool IsItemInstalled(PublishedFileId_t workshopItemId)
        {
            return SteamUGC.GetItemInstallInfo(workshopItemId, out _, out _, cchFolderSize, out _);
        }

        public static EItemState GetItemState(PublishedFileId_t workshopItemId)
        {
            return (EItemState)SteamUGC.GetItemState(workshopItemId);
        }

        public static void SubscribeItem(PublishedFileId_t workshopItemId, Action<RemoteStorageSubscribePublishedFileResult_t, bool> callback)
        {
            CallResult<RemoteStorageSubscribePublishedFileResult_t> callResult = new CallResult<RemoteStorageSubscribePublishedFileResult_t>();
            callResult.Set(SteamUGC.SubscribeItem(workshopItemId), delegate (RemoteStorageSubscribePublishedFileResult_t t, bool ioError)
            {
                if (callback != null)
                {
                    try
                    {
                        callback?.Invoke(t, ioError);
                    }
                    catch { }
                }
                callResult?.Dispose();
            });
        }

        public static void UnsubscribeItem(PublishedFileId_t workshopItemId, Action<RemoteStorageUnsubscribePublishedFileResult_t, bool> callback)
        {
            CallResult<RemoteStorageUnsubscribePublishedFileResult_t> callResult = new CallResult<RemoteStorageUnsubscribePublishedFileResult_t>();
            callResult.Set(SteamUGC.UnsubscribeItem(workshopItemId), delegate (RemoteStorageUnsubscribePublishedFileResult_t t, bool ioError)
            {
                if (callback != null)
                {
                    try
                    {
                        callback?.Invoke(t, ioError);
                    }
                    catch { }
                }
                callResult?.Dispose();
            });
        }

        public static bool UpdateItem(PublishedFileId_t workshopItemId, Action<DownloadItemResult_t> callback)
        {
            Callback<DownloadItemResult_t> cb = null;
            cb = new Callback<DownloadItemResult_t>(delegate (DownloadItemResult_t t)
            {
                if (callback != null)
                {
                    try
                    {
                        callback?.Invoke(t);
                    }
                    catch { }
                }
                cb?.Dispose();
            }, false);
            return SteamUGC.DownloadItem(workshopItemId, true);
        }

        public static void GetUserVote(PublishedFileId_t workshopItemId, Action<WorkshopItemVote> callback)
        {
            CallResult<GetUserItemVoteResult_t> callResult = new CallResult<GetUserItemVoteResult_t>();
            callResult.Set(SteamUGC.GetUserItemVote(workshopItemId), delegate (GetUserItemVoteResult_t t, bool ioError)
            {
                WorkshopItemVote workshopItemVote = new WorkshopItemVote(t, ioError);
                if (callback != null)
                {
                    try
                    {
                        callback?.Invoke(workshopItemVote);
                    }
                    catch { }
                }
                callResult?.Dispose();
            });
        }

        public static void SetUserVote(PublishedFileId_t workshopItemId, bool upVote, Action<SetUserItemVoteResult_t, bool> callback)
        {
            CallResult<SetUserItemVoteResult_t> callResult = new CallResult<SetUserItemVoteResult_t>();
            callResult.Set(SteamUGC.SetUserItemVote(workshopItemId, upVote), delegate (SetUserItemVoteResult_t t, bool ioError)
            {
                if (callback != null)
                {
                    try
                    {
                        callback?.Invoke(t, ioError);
                    }
                    catch { }
                }
                callResult?.Dispose();
            });
        }

        public static void AddItemToFavorites(PublishedFileId_t workshopItemId, Action<UserFavoriteItemsListChanged_t, bool> callback)
        {
            CallResult<UserFavoriteItemsListChanged_t> callResult = new CallResult<UserFavoriteItemsListChanged_t>();
            callResult.Set(SteamUGC.AddItemToFavorites(SteamUtils.GetAppID(), workshopItemId), delegate (UserFavoriteItemsListChanged_t t, bool ioError)
            {
                if (callback != null)
                {
                    try
                    {
                        callback?.Invoke(t, ioError);
                    }
                    catch { }
                }
                callResult?.Dispose();
            });
        }

        /// <summary>
        /// Execute <see cref="SteamUGC.CreateQueryAllUGCRequest(EUGCQuery, EUGCMatchingUGCType, AppId_t, AppId_t, uint)"/>
        /// </summary>
        /// <param name="rankBy"></param>
        /// <param name="itemsType"></param>
        /// <param name="page"></param>
        /// <param name="errorCallback"></param>
        /// <param name="debugCallback"></param>
        public static bool GetAllWorkshopItems(EUGCQuery rankBy, int page, RequestParameters requestParameters, Action<List<WorkshopItem>> callback, Action<string> errorCallback, Action<bool, int, int> debugCallback)
        {
            page = Mathf.Max(1, page);
            if (requestParameters == null)
                return false;

            UGCQueryHandle_t queryHandle = createAllWorkshopItemsRequest(rankBy, requestParameters.GetItemType(), page);
            requestParameters.ConfigureQuery(queryHandle);
            requestWorkshopItems(queryHandle, delegate (SteamUGCQueryCompleted_t queryResult, bool io)
            {
                onQueryCallback(true, queryResult, io, callback, errorCallback, debugCallback);
            }, errorCallback);
            return true;
        }

        public static bool GetWorkshopItem(PublishedFileId_t publishedFileId, Action<WorkshopItem> callback, Action<string> errorCallback, Action<bool, int, int> debugCallback)
        {
            UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[] { publishedFileId }, 1);
            _ = SteamUGC.SetReturnChildren(queryHandle, true);
            _ = SteamUGC.SetReturnLongDescription(queryHandle, true);
            requestWorkshopItems(queryHandle, delegate (SteamUGCQueryCompleted_t queryResult, bool io)
            {
                onQueryCallback(false, queryResult, io, delegate (List<WorkshopItem> list)
                {
                    if (list != null && list.Count == 1)
                        callback?.Invoke(list[0]);
                    else
                        errorCallback?.Invoke($"Item not found. ({(list == null ? -1 : list.Count)})");
                }, errorCallback, debugCallback);
            }, errorCallback);
            return true;
        }

        public static bool GetWorkshopItems(PublishedFileId_t[] publishedFileIds, Action<List<WorkshopItem>> callback, Action<string> errorCallback, Action<bool, int, int> debugCallback)
        {
            UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(publishedFileIds, (uint)publishedFileIds.Length);
            _ = SteamUGC.SetReturnChildren(queryHandle, true);
            requestWorkshopItems(queryHandle, delegate (SteamUGCQueryCompleted_t queryResult, bool io)
            {
                onQueryCallback(false, queryResult, io, callback, errorCallback, debugCallback);
            }, errorCallback);
            return true;
        }

        public static bool GetWorkshopUserItemList(CSteamID steamId, int page, EUserUGCList userList, EUserUGCListSortOrder sortOrder, RequestParameters requestParameters, Action<List<WorkshopItem>> callback, Action<string> errorCallback, Action<bool, int, int> debugCallback)
        {
            page = Mathf.Max(1, page);
            if (requestParameters == null || steamId == default)
                return false;

            UGCQueryHandle_t queryHandle = createWorkshopItemsByUserRequest(steamId, page, userList, requestParameters.GetItemType(), sortOrder);
            requestParameters.ConfigureQuery(queryHandle);
            requestWorkshopItems(queryHandle, delegate (SteamUGCQueryCompleted_t queryResult, bool io)
            {
                onQueryCallback(true, queryResult, io, callback, errorCallback, debugCallback);
            }, errorCallback);
            return true;
        }

        private static void onQueryCallback(bool setPageCount, SteamUGCQueryCompleted_t queryResult, bool ioError, Action<List<WorkshopItem>> callback, Action<string> errorCallback, Action<bool, int, int> debugInfo)
        {
            if (!checkQueryResultForErrors(queryResult, ioError, errorCallback))
            {
                releaseQueryHandle(queryResult.m_handle);
                return;
            }

            if (setPageCount)
                setPageAmount(queryResult.m_unTotalMatchingResults);

            callback?.Invoke(getQueryItems(queryResult));
            debugInfo?.Invoke(queryResult.m_bCachedData, (int)queryResult.m_unNumResultsReturned, (int)queryResult.m_unTotalMatchingResults);
            releaseQueryHandle(queryResult.m_handle);
        }

        private static List<WorkshopItem> getQueryItems(SteamUGCQueryCompleted_t queryResult)
        {
            List<WorkshopItem> items = new List<WorkshopItem>();
            long numResultsReturned = (long)(ulong)queryResult.m_unNumResultsReturned;
            if (numResultsReturned <= 0)
                return items;

            uint index = 0;
            while (index < numResultsReturned)
            {
                if (SteamUGC.GetQueryUGCResult(queryResult.m_handle, index, out SteamUGCDetails_t ugcDetails) && ugcDetails.m_eResult == EResult.k_EResultOK && !ugcDetails.m_bBanned)
                {
                    UGCQueryHandle_t handle = queryResult.m_handle;
                    EWorkshopFileType itemType = ugcDetails.m_eFileType;
                    PublishedFileId_t itemId = ugcDetails.m_nPublishedFileId;
                    CSteamID itemAuthor = (CSteamID)ugcDetails.m_ulSteamIDOwner;

                    bool shouldRunUserInfoCoroutine = false;
                    string authorName = string.Empty;
                    if (SteamFriends.RequestUserInformation(itemAuthor, false))
                    {
                        shouldRunUserInfoCoroutine = true;
                    }
                    else
                        authorName = SteamFriends.GetFriendPersonaName(itemAuthor);

                    List<WorkshopItemPreview> additionalPreviews = null;
                    uint apCount = SteamUGC.GetQueryUGCNumAdditionalPreviews(handle, index);
                    if (apCount != 0)
                    {
                        additionalPreviews = new List<WorkshopItemPreview>();
                        for (int i = 0; i < apCount; i++)
                            if (SteamUGC.GetQueryUGCAdditionalPreview(handle, index, (uint)i, out string apUrl, cchURLSize, out _, 4096U, out EItemPreviewType previewType))
                            {
                                additionalPreviews.Add(new WorkshopItemPreview(apUrl, previewType));
                            }
                    }

                    bool getChildrenError;
                    PublishedFileId_t[] children = null;
                    if (itemType == EWorkshopFileType.k_EWorkshopFileTypeCollection)
                    {
                        children = new PublishedFileId_t[ugcDetails.m_unNumChildren];
                        getChildrenError = SteamUGC.GetQueryUGCChildren(handle, index, children, (uint)children.Length);
                    }
                    else
                        getChildrenError = false;

                    if (!SteamUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumUniqueWebsiteViews, out ulong viewCount))
                        viewCount = 0L;

                    if (!SteamUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumSubscriptions, out ulong subscriberCount))
                        subscriberCount = 0L;

                    if (!SteamUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumFavorites, out ulong favoriteCount))
                        favoriteCount = 0L;

                    bool previewUrlError = !SteamUGC.GetQueryUGCPreviewURL(handle, index, out string previewUrl, cchURLSize);
                    bool installInfoError = SteamUGC.GetItemInstallInfo(itemId, out _, out string folder, cchFolderSize, out _);
                    WorkshopItem item = new WorkshopItem()
                    {
                        Name = ugcDetails.m_rgchTitle,
                        Description = ugcDetails.m_rgchDescription,
                        Author = authorName,
                        Tags = ugcDetails.m_rgchTags.IsNullOrEmpty() ? null : ugcDetails.m_rgchTags.Split(','),
                        ItemID = itemId,
                        AuthorID = itemAuthor,
                        Votes = (int)(ugcDetails.m_unVotesUp + ugcDetails.m_unVotesDown),
                        UpVotes = (int)ugcDetails.m_unVotesUp,
                        DownVotes = (int)ugcDetails.m_unVotesDown,
                        Views = (long)viewCount,
                        Subscribers = (long)subscriberCount,
                        Favorites = (long)favoriteCount,
                        Rating = ugcDetails.m_flScore,
                        Children = children,
                        PreviewURL = previewUrl,
                        AdditionalPreviews = additionalPreviews,
                        ItemType = itemType,
                        Folder = folder,
                        Size = ugcDetails.m_nFileSize / 1024f / 1024f,
                        PostDate = DateTimeOffset.FromUnixTimeSeconds(ugcDetails.m_rtimeCreated).DateTime.AddHours(15),
                        UpdateDate = DateTimeOffset.FromUnixTimeSeconds(ugcDetails.m_rtimeUpdated).DateTime.AddHours(15),
                        PreviewURLError = previewUrlError,
                        InstallInfoError = installInfoError,
                        GetChildrenError = getChildrenError,
                    };

                    if (shouldRunUserInfoCoroutine)
                        _ = ModActionUtils.RunCoroutine(requestUserInformationCoroutine(item, itemAuthor));

                    items.Add(item);
                }
                index++;
            }
            return items;
        }

        private static bool checkQueryResultForErrors(SteamUGCQueryCompleted_t queryResult, bool ioError, Action<string> errorCallback)
        {
            if (ioError)
            {
                errorCallback?.Invoke("I/O Error.");
                return false;
            }

            EResult result = queryResult.m_eResult;
            if (result != EResult.k_EResultOK)
            {
                errorCallback?.Invoke("Error: " + result + ".");
                return false;
            }
            return true;
        }

        private static UGCQueryHandle_t createAllWorkshopItemsRequest(EUGCQuery rankBy, EUGCMatchingUGCType itemsType, int page)
        {
            AppId_t appId = SteamUtils.GetAppID();
            return SteamUGC.CreateQueryAllUGCRequest(rankBy, itemsType, appId, appId, (uint)page);
        }

        private static UGCQueryHandle_t createWorkshopItemsByUserRequest(CSteamID accountId, int page, EUserUGCList userList, EUGCMatchingUGCType itemsType, EUserUGCListSortOrder sortOrder)
        {
            AppId_t appId = SteamUtils.GetAppID();
            return SteamUGC.CreateQueryUserUGCRequest(accountId.GetAccountID(), userList, itemsType, sortOrder, appId, appId, (uint)page);
        }

        private static void requestWorkshopItems(UGCQueryHandle_t queryHandle, Action<SteamUGCQueryCompleted_t, bool> callback, Action<string> errorCallback)
        {
            bool done = false;
            CallResult<SteamUGCQueryCompleted_t> callResult = CallResult<SteamUGCQueryCompleted_t>.Create(null);
            callResult.Set(SteamUGC.SendQueryUGCRequest(queryHandle), delegate (SteamUGCQueryCompleted_t c, bool io)
            {
                done = true;
                callback?.Invoke(c, io);
                callResult?.Dispose();
            });

            DelegateScheduler.Instance.Schedule(delegate
            {
                if (callResult != null && !callResult.m_bDisposed)
                    callResult.Dispose();

                if (!done)
                    errorCallback?.Invoke("Request timeout.");
            }, 20f);
        }

        private static void releaseQueryHandle(UGCQueryHandle_t queryHandle)
        {
            _ = SteamUGC.ReleaseQueryUGCRequest(queryHandle);
        }

        private static void setPageAmount(uint matchingResults)
        {
            pageCount = (int)((matchingResults / 50U) + 1U);
        }

        private static IEnumerator requestUserInformationCoroutine(WorkshopItem workshopItem, CSteamID steamId)
        {
            float time = Time.unscaledTime + 2f;
            while (Time.unscaledTime < time)
                yield return null;

            if (!workshopItem.IsDisposed())
                workshopItem.Author = SteamFriends.GetFriendPersonaName(steamId);

            yield break;
        }

        public class RequestParameters
        {
            private List<string> m_requiredTags;

            private string m_searchText;

            private bool m_enableCaching;

            private bool m_returnChildren;

            private bool m_returnPreviews;

            private bool m_returnLongDescription;

            private EUGCMatchingUGCType m_itemType;

            public static RequestParameters Create(EUGCMatchingUGCType itemType)
            {
                return new RequestParameters()
                {
                    m_itemType = itemType
                };
            }

            public void EnableCaching()
            {
                m_enableCaching = true;
            }

            public void ReturnChildren()
            {
                m_returnChildren = true;
            }

            public void ReturnPreviews()
            {
                m_returnPreviews = true;
            }

            public void ReturnLongDescription()
            {
                m_returnLongDescription = true;
            }

            public void RequireTags(List<string> tags)
            {
                m_requiredTags = tags;
            }

            public void SearchText(string text)
            {
                m_searchText = text;
            }

            public void ConfigureQuery(UGCQueryHandle_t queryHandle)
            {
                if (m_requiredTags != null && m_requiredTags.Count != 0)
                    foreach (string tag in m_requiredTags)
                        _ = SteamUGC.AddRequiredTag(queryHandle, tag);

                if (!m_searchText.IsNullOrEmpty())
                    _ = SteamUGC.SetSearchText(queryHandle, m_searchText);

                if (m_enableCaching)
                    _ = SteamUGC.SetAllowCachedResponse(queryHandle, 5);

                if (m_returnChildren)
                    _ = SteamUGC.SetReturnChildren(queryHandle, true);

                if (m_returnPreviews)
                    _ = SteamUGC.SetReturnAdditionalPreviews(queryHandle, true);

                if (m_returnLongDescription)
                    _ = SteamUGC.SetReturnLongDescription(queryHandle, true);
            }

            public bool IsCachingEnabled()
            {
                return m_enableCaching;
            }

            public EUGCMatchingUGCType GetItemType()
            {
                return m_itemType;
            }
        }
    }
}
