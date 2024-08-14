using ICSharpCode.SharpZipLib.Zip;
using OverhaulMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace OverhaulMod.Content
{
    public class ContentManager : Singleton<ContentManager>
    {
        public const string CONTENT_LIST_REPOSITORY_FILE = "ContentList.json";

        public const string CONTENT_INFO_FILE = "contentInfo.json";

        public const string EXTRAS_CONTENT_FOLDER_NAME = "Extras";

        public const string REALISTIC_SKYBOXES_CONTENT_FOLDER_NAME = "Realistic skyboxes";

        public const string GALLERY_CONTENT_FOLDER_NAME = "Behind the scenes";

        public const string CONTENT_DOWNLOAD_DONE_EVENT = "OverhaulContentDownloadDone";

        private Dictionary<string, float> m_downloadingFiles;

        private List<ContentInfo> m_installedContent;

        private List<object> m_loadingAddons;

        public bool isLoadingAnyContent
        {
            get
            {
                return m_loadingAddons != null && m_loadingAddons.Count != 0;
            }
        }

        private void Start()
        {
            m_downloadingFiles = new Dictionary<string, float>();
            m_loadingAddons = new List<object>();
            RefreshContent();
        }

        public void RefreshContent(bool force = false)
        {
            if (force)
                m_installedContent = null;

            _ = GetContent();
        }

        public bool DownloadContent(string name, string url, Action<string> callback)
        {
            if (m_downloadingFiles.ContainsKey(url))
            {
                _ = base.StartCoroutine(waitUntilContentIsDownloaded(url, callback));
                return false;
            }

            m_downloadingFiles.Add(url, 0f);

            string tempPath = Path.GetTempFileName();
            GoogleDriveManager.Instance.DownloadFile(url, tempPath, delegate (float progress)
            {
                if (!m_downloadingFiles.ContainsKey(url))
                    m_downloadingFiles.Add(url, progress);
                else
                    m_downloadingFiles[url] = progress;
            },
            delegate (string result)
            {
                _ = m_downloadingFiles.Remove(url);

                if (result != null)
                {
                    ModManagers.Instance.TriggerModContentLoadedEvent(result);
                    callback?.Invoke(result);
                    return;
                }

                RemoveContent(name);

                try
                {
                    string dest = Path.Combine(ModCore.addonsFolder, name);
                    if (!Directory.Exists(dest))
                        _ = Directory.CreateDirectory(dest);

                    FastZip fastZip = new FastZip();
                    fastZip.ExtractZip(tempPath, dest, null);
                }
                catch (Exception exc)
                {
                    ModManagers.Instance.TriggerModContentLoadedEvent(exc.ToString());
                    return;
                }

                ModManagers.Instance.TriggerModContentLoadedEvent(null);
                callback?.Invoke(null);

                RefreshContent(true);
            });
            return true;
        }

        public void DownloadContentList(out UnityWebRequest unityWebRequest, Action<ContentListInfo> callback, Action<string> errorCallback)
        {
            RepositoryManager.Instance.GetTextFile(CONTENT_LIST_REPOSITORY_FILE, delegate (string rawData)
            {
                ContentListInfo contentInfo = null;
                try
                {
                    contentInfo = ModJsonUtils.Deserialize<ContentListInfo>(rawData);
                    contentInfo.FixValues();
                }
                catch (Exception exc)
                {
                    errorCallback?.Invoke(exc.ToString());
                    return;
                }

                callback?.Invoke(contentInfo);
            }, errorCallback, out unityWebRequest, 15);
        }

        private IEnumerator waitUntilContentIsDownloaded(string name, Action<string> callback)
        {
            GlobalEventManager.Instance.AddEventListenerOnce(CONTENT_DOWNLOAD_DONE_EVENT, delegate (string error)
            {
                callback?.Invoke(error);
            });
            yield break;
        }

        public bool IsDownloading(string name)
        {
            return m_downloadingFiles.ContainsKey(name);
        }

        public float GetDownloadProgress(string name)
        {
            if (m_downloadingFiles.TryGetValue(name, out float progress))
                return progress;

            return -1f;
        }

        public void RemoveContent(string name)
        {
            if (HasContent(name))
                Directory.Delete(Path.Combine(ModCore.addonsFolder, name), true);
        }

        public bool HasContent(string contentName, bool quick = false)
        {
            if (quick)
            {
                string lower = contentName.ToLower();
                if (m_installedContent.IsNullOrEmpty())
                    return false;

                foreach (ContentInfo c in m_installedContent)
                    if (c.DisplayName.ToLower() == lower)
                        return true;
            }
            return Directory.Exists(Path.Combine(ModCore.addonsFolder, contentName));
        }

        public void SetContentIsLoading(object obj, bool value)
        {
            if (obj == null)
                return;

            if (value && !m_loadingAddons.Contains(obj))
                m_loadingAddons.Add(obj);
            else if (!value)
                _ = m_loadingAddons.Remove(obj);
        }

        /// <summary>
        /// Get content directory path
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public string GetContentPath(string contentName)
        {
            string path = Path.Combine(ModCore.addonsFolder, contentName);
            return !Directory.Exists(path) ? null : path;
        }

        public List<ContentInfo> GetContent()
        {
            if (m_installedContent != null)
                return m_installedContent;

            string[] folders = Directory.GetDirectories(ModCore.addonsFolder);
            if (folders.IsNullOrEmpty())
                return null;

            List<ContentInfo> list = new List<ContentInfo>();
            foreach (string folder in folders)
            {
                string addonInfoFilePath = Path.Combine(folder, CONTENT_INFO_FILE);
                if (!File.Exists(addonInfoFilePath))
                    continue;

                try
                {
                    ContentInfo contentInfo = ModJsonUtils.DeserializeStream<ContentInfo>(addonInfoFilePath);
                    contentInfo.FolderPath = folder;
                    list.Add(contentInfo);
                }
                catch
                {
                    continue;
                }
            }
            m_installedContent = list;
            return list;
        }
    }
}
