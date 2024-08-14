using DiscordWebhook;
using Newtonsoft.Json;
using OverhaulMod.Content.Personalization;
using Steamworks;
using System;
using System.Net;
using UnityEngine;

namespace OverhaulMod
{
    public class ModWebhookManager : Singleton<ModWebhookManager>
    {
        private const string START_ENABLED_EMOJI = "<:star_enabled:1196444610751373434>";
        private const string START_DISABLED_EMOJI = "<:star_disabled:1196444266772316182>";

        public const string FeedbacksWebhookURL = "https://discord.com/api/webhooks/1124285317768290454/QuXjaAywp5eRXT2a5BfOtYGFS9h2eHb8giuze3yxLkZ1Y7m7m2AOTfxf9hB4IeCIkTk5";
        public const string SurveysWebhookURL = "https://discord.com/api/webhooks/1197656266848342057/66RNDd0uzzEHWfMG-tJFxgLciQfrMryHEcm7h6m7YQYwu5vUtDfhIEImH_SuVNCl29Hb";
        public const string VerificationRequestsWebhookURL = "https://discord.com/api/webhooks/1206265503836930098/XhMcbjEETktOqZlbW1CHY7kQWscJzG-Nk65Q8bvghHfnMFMk7A2_KZWx7CiTG554YsSG";
        public const string CrashReportsWebhookURL = "https://discord.com/api/webhooks/1106574827806019665/n486TzxFbaF6sMmbqg2CUHKGN1o15UpR9AUJAmi5c7sdIwI1jeXpTReD4jtZ3U76PzWS";

        public static string ErrorReportText;

        public void ExecuteCrashReportsWebhook(string text, Action successCallback, Action<string> errorCallback)
        {
            int color = int.Parse("d63a51", System.Globalization.NumberStyles.HexNumber);
            string deviceInfo = $"- **OS:** {SystemInfo.operatingSystem}\n- **CPU:** {SystemInfo.processorType}\n * {SystemInfo.processorCount}/{SystemInfo.processorFrequency}\n- **GPU:** {SystemInfo.graphicsDeviceName}\n * {SystemInfo.graphicsMemorySize} MBs\n- **Memory:** {SystemInfo.systemMemorySize} MBs";
            string gameMode = "N/A";
            try
            {
                gameMode = GameFlowManager.Instance._gameMode.ToString();
            }
            catch { }

            string levelId = "N/A";
            try
            {
                levelId = LevelManager.Instance.GetCurrentLevelID();
            }
            catch { }

            string gameVer = "N/A";
            try
            {
                gameVer = VersionNumberManager.Instance.GetVersionString();
            }
            catch { }

            string lang = "N/A";
            try
            {
                lang = LocalizationManager.Instance.GetCurrentLanguageCode();
            }
            catch { }

            string time = "N/A";
            try
            {
                time = Time.unscaledTime.ToString();
            }
            catch { }

            if (text.Length > 1490)
                text = text.Remove(1490);

            text = $"```{text}```";

            WebhookObject obj1 = new WebhookObject()
            {
                content = $"## __Game crashed. v{ModBuildInfo.version}__",
                embeds = new Embed[]
                {
                    new Embed()
                    {
                        title = "**Stack trace**",
                        description = text,
                        color = color,
                    },

                    new Embed()
                    {
                        title = "**Details**",
                        description = $"{gameMode}, {levelId}, {gameVer}, {lang}, {time}\n{deviceInfo}",
                        color = color,
                    },
                },
            };
            UploadMessageWithWebhook(obj1, CrashReportsWebhookURL, successCallback, errorCallback);
        }

        public void ExecuteFeedbacksWebhook(int rank, string improveText, string likedText, Action successCallback, Action<string> errorCallback)
        {
            rank = Mathf.Clamp(rank, 1, 5);
            string userInfo = $"- **User:** {SteamFriends.GetPersonaName()} [[Profile]](<https://steamcommunity.com/profiles/{SteamUser.GetSteamID()}>)";
            string deviceInfo = $"- **OS:** {SystemInfo.operatingSystem}\n- **CPU:** {SystemInfo.processorType}\n * {SystemInfo.processorCount}/{SystemInfo.processorFrequency}\n- **GPU:** {SystemInfo.graphicsDeviceName}\n * {SystemInfo.graphicsMemorySize} MBs\n- **Memory:** {SystemInfo.systemMemorySize} MBs";
            string rankText = string.Empty;
            for (int i = 0; i < rank; i++)
            {
                rankText += START_ENABLED_EMOJI;
                rankText += " ";
            }
            for (int i = 0; i < (5 - rank); i++)
            {
                rankText += START_DISABLED_EMOJI;
                rankText += " ";
            }

            int color = int.Parse("32a852", System.Globalization.NumberStyles.HexNumber);
            WebhookObject obj1 = new WebhookObject()
            {
                content = $"## __Feedback. v{ModBuildInfo.version}__ {rankText}",
                embeds = new Embed[]
                {
                    new Embed()
                    {
                        title = "**Improve**",
                        description = improveText,
                        color = color,
                    },

                    new Embed()
                    {
                        title = "**Favorite**",
                        description = likedText,
                        color = color,
                    },

                    new Embed()
                    {
                        title = "**Details**",
                        description = $"{userInfo}\n{deviceInfo}",
                        color = color,
                    },
                },
            };
            UploadMessageWithWebhook(obj1, FeedbacksWebhookURL, successCallback, errorCallback);
        }

        public void ExecuteSurveysWebhook(string selectedVariant, string newsTitle, Action successCallback, Action<string> errorCallback)
        {
            int color = int.Parse("32a852", System.Globalization.NumberStyles.HexNumber);
            string userInfo = $"- **User:** {SteamFriends.GetPersonaName()} [[Profile]](<https://steamcommunity.com/profiles/{SteamUser.GetSteamID()}>)";
            WebhookObject obj1 = new WebhookObject()
            {
                content = $"## __Survey answer. v{ModBuildInfo.version}__ {newsTitle}",
                embeds = new Embed[]
                {
                    new Embed()
                    {
                        title = "**Selected answer**",
                        description = selectedVariant,
                        color = color,
                    },

                    new Embed()
                    {
                        title = "**Details**",
                        description = $"{userInfo}",
                        color = color,
                    },
                },
            };
            UploadMessageWithWebhook(obj1, SurveysWebhookURL, successCallback, errorCallback);
        }

        public void ExecuteVerificationRequestWebhook(string zipPath, PersonalizationItemInfo personalizationItem, Action successCallback, Action<string> errorCallback)
        {
            int color = int.Parse("32a852", System.Globalization.NumberStyles.HexNumber);
            string userInfo = $"- **User:** {SteamFriends.GetPersonaName()} [[Profile]](<https://steamcommunity.com/profiles/{ModUserInfo.localPlayerSteamID}>)\n- **PlayFab ID:** {ModUserInfo.localPlayerPlayFabID}";
            string itemInfo = $"- **Name:** {personalizationItem.Name}\n- **Author:** {personalizationItem.GetAuthorsString()}\n- **Category:** {personalizationItem.Category}\n- **Weapon:** {personalizationItem.Weapon}\n- **Version:** {personalizationItem.Version}";

            WebhookObject obj1 = new WebhookObject()
            {
                content = $"## __{(personalizationItem.IsVerified ? "An item to update" : "New item to verify")}. v{ModBuildInfo.version}__{(personalizationItem.IsSentForVerification ? "\n# REUPLOAD" : string.Empty)}\nid: {personalizationItem.ItemID}",
                embeds = new Embed[]
                {
                    new Embed()
                    {
                        title = "**Information**",
                        description = itemInfo,
                        color = color,
                    },

                    new Embed()
                    {
                        title = "**Details**",
                        description = $"{userInfo}",
                        color = color,
                    },
                },
            };

            UploadMessageWithWebhook(obj1, VerificationRequestsWebhookURL, delegate
            {
                UploadFileWithWebhook(zipPath, VerificationRequestsWebhookURL, successCallback, errorCallback);
            }, errorCallback);
        }

        public void ExecuteTestUploadWebhook(string filePath, Action successCallback, Action<string> errorCallback)
        {
            UploadFileWithWebhook(filePath, FeedbacksWebhookURL, successCallback, errorCallback);
        }

        public async void UploadMessageWithWebhook(WebhookObject webhookObject, string url, Action successCallback, Action<string> errorCallback)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    System.Threading.Tasks.Task<string> task = webClient.UploadStringTaskAsync(new Uri(url), "POST", JsonConvert.SerializeObject(webhookObject));
                    _ = await task;
                    if (task.Exception != null)
                    {
                        ModDebug.LogWarning(task.Exception);
                        errorCallback?.Invoke(task.Exception.ToString());
                    }
                    else
                    {
                        successCallback?.Invoke();
                    }
                }
            }
            catch (Exception exc)
            {
                ModDebug.LogWarning(exc);
                errorCallback?.Invoke(exc.ToString());
            }
        }

        public async void UploadFileWithWebhook(string filePath, string url, Action successCallback, Action<string> errorCallback)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    System.Threading.Tasks.Task<byte[]> task = webClient.UploadFileTaskAsync(new Uri(url), "POST", filePath);
                    _ = await task;
                    if (task.Exception != null)
                    {
                        ModDebug.LogWarning(task.Exception);
                        errorCallback?.Invoke(task.Exception.ToString());
                    }
                    else
                    {
                        successCallback?.Invoke();
                    }
                }
            }
            catch (Exception exc)
            {
                ModDebug.LogWarning(exc);
                errorCallback?.Invoke(exc.ToString());
            }
        }
    }
}