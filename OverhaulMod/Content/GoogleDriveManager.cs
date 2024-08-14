using System;
using UnityEngine;

namespace OverhaulMod.Content
{
    /// <summary>
    /// An alternative for <see cref="RepositoryManager"/>
    /// </summary>
    public class GoogleDriveManager : Singleton<GoogleDriveManager>
    {
        public void DownloadFile(string url, string targetPath, Action<float> progressCallback, Action<string> resultCallback)
        {
            int lastFrameDownloadProgressWasDisplayed = 0;

            FileDownloader fileDownloader = new FileDownloader();
            fileDownloader.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                    resultCallback?.Invoke("Download cancelled");
                else if (e.Error != null)
                    resultCallback?.Invoke("Download failed: " + e.Error);
                else
                    resultCallback?.Invoke(null);

                fileDownloader.Dispose();
            };
            fileDownloader.DownloadProgressChanged += (sender, e) =>
            {
                if (lastFrameDownloadProgressWasDisplayed == Time.frameCount)
                    return;

                lastFrameDownloadProgressWasDisplayed = Time.frameCount;
                progressCallback?.Invoke(e.BytesReceived / (float)e.TotalBytesToReceive);
            };
            fileDownloader.DownloadFileAsync(url, targetPath);
        }
    }
}
