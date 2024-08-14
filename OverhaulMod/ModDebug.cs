using OverhaulMod.Utils;
using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace OverhaulMod
{
    public static class ModDebug
    {
        private static int s_lastFrameDownloadProgressWasDisplayed;

        public static bool forceDisableCursor
        {
            get;
            set;
        }

        public static void Log(object obj, bool logInReleaseBuilds = false)
        {
            if (ModBuildInfo.debug || logInReleaseBuilds)
                Debug.Log(obj);
        }

        public static void LogWarning(object obj, bool logInReleaseBuilds = false)
        {
            if (ModBuildInfo.debug || logInReleaseBuilds)
                Debug.LogWarning(obj);
        }

        public static void LogError(object obj, bool logInReleaseBuilds = false)
        {
            if (ModBuildInfo.debug || logInReleaseBuilds)
                Debug.LogError(obj);
        }

        public static void LogException(Exception obj, bool logInReleaseBuilds = false)
        {
            if (ModBuildInfo.debug || logInReleaseBuilds)
                Debug.LogException(obj);
        }

        public static void AddAntialiasingEffect(Camera camera)
        {
            Antialiasing antialiasing = camera.gameObject.AddComponent<Antialiasing>();
            antialiasing.dlaaShader = Shader.Find("Hidden/DLAA");
            antialiasing.nfaaShader = Shader.Find("Hidden/NFAA");
            antialiasing.ssaaShader = Shader.Find("Hidden/SSAA");
            antialiasing.shaderFXAAII = Shader.Find("Hidden/FXAA II");
            antialiasing.shaderFXAAIII = Shader.Find("Hidden/FXAA III (Console)");
            antialiasing.shaderFXAAPreset2 = Shader.Find("Hidden/FXAA Preset 2");
            antialiasing.shaderFXAAPreset3 = Shader.Find("Hidden/FXAA Preset 3");
            antialiasing.mode = AAMode.FXAA2;
        }

        public static void AddDepthOfFieldEffect(Camera camera)
        {
            _ = camera.gameObject.AddComponent<DepthOfField>();
        }

        public static void MessagePopupTest()
        {
            string desc = string.Empty;
            for (int i = 0; i < 75; i++)
            {
                desc += "very long string ";
            }

            ModUIUtils.MessagePopupOK("hmm", desc, 175f, true);
        }

        public static void UpdatePopupTest()
        {
            ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("update_available_header"), string.Format(LocalizationManager.Instance.GetTranslatedString("update_available_description"), new Version(10, 10, 10, 10)), 150f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
            {
                UI.UIUpdatesWindow window = ModUIConstants.ShowUpdatesWindow();
                window.SelectBranchAndSearchForUpdates(1);
            });
        }

        public static void FileDownloadTest()
        {
            FileDownloader fileDownloader = new FileDownloader();
            fileDownloader.DownloadProgressChanged += onDownloadProgress;
            fileDownloader.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                    ModDebug.Log("Download cancelled");
                else if (e.Error != null)
                    ModDebug.Log("Download failed: " + e.Error);
                else
                    ModDebug.Log("Download completed");
            };
            fileDownloader.DownloadFileAsync("https://drive.google.com/file/d/1T_sWBJdpXe74dZrtN7pPZXjT4sVgq7-o/view?usp=drive_link", "D:\\CloneDroneBeta.zip");
        }

        private static void onDownloadProgress(object sender, FileDownloader.DownloadProgress e)
        {
            if (s_lastFrameDownloadProgressWasDisplayed == Time.frameCount)
                return;

            s_lastFrameDownloadProgressWasDisplayed = Time.frameCount;
            ModDebug.Log("Progress changed " + e.BytesReceived + " " + e.TotalBytesToReceive);
        }
    }
}
