using OverhaulMod.Content;
using OverhaulMod.Utils;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementPlayerInfoDisplay : OverhaulUIBehaviour
    {
        [UIElement("PlayerIcon")]
        public RawImage m_icon;

        private bool m_showIcon;

        private UnityWebRequest m_webRequest;

        private Texture2D m_texture;

        protected override void OnInitialized()
        {
            m_icon.color = new Color(1f, 1f, 1f, 0f);
            m_icon.enabled = false;
        }

        public override void Update()
        {
            Color color = m_icon.color;
            color.a = Mathf.Lerp(color.a, m_showIcon ? 1f : 0f, Time.unscaledDeltaTime * 12.5f);
            m_icon.color = color;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            UnityWebRequest unityWebRequest = m_webRequest;
            if (unityWebRequest != null)
            {
                try
                {
                    unityWebRequest.Abort();
                }
                catch
                {

                }
            }

            Texture2D texture = m_texture;
            if (texture)
            {
                Destroy(texture);
            }
        }

        public void LoadRobotHead(int characterModelIndex, int favouriteColorIndex)
        {
            if (!base.gameObject.activeInHierarchy || !base.enabled)
                return;

            string contentPath = ContentManager.Instance.GetContentPath(ContentManager.EXTRAS_CONTENT_FOLDER_NAME);
            if (contentPath.IsNullOrEmpty())
                return;

            if (characterModelIndex < 0 || characterModelIndex >= MultiplayerCharacterCustomizationManager.Instance.CharacterModels.Count)
                characterModelIndex = 0;

            string filename = $"{MultiplayerCharacterCustomizationManager.Instance.CharacterModels[characterModelIndex].Name}_{favouriteColorIndex}.png".Replace("Bow 2", "Bow 1");
            if (filename.Contains("Business Bot")) filename = "Business Bot_0.png";
            if (filename.Contains("Emperor")) filename = "Emperor_0.png";
            if (filename.Contains("Sword 5")) filename = "Sword 5_0.png";

            string path = Path.Combine(contentPath, "playerIcons", filename);
            if (!File.Exists(path))
                return;

            _ = base.StartCoroutine(loadIconCoroutine(path));
        }

        private IEnumerator loadIconCoroutine(string path)
        {
            using (UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture($"file://{path}"))
            {
                m_webRequest = unityWebRequest;
                yield return unityWebRequest.SendWebRequest();
                m_webRequest = null;
                if (!unityWebRequest.isHttpError && !unityWebRequest.isNetworkError && unityWebRequest.isDone)
                {
                    Texture2D texture = (unityWebRequest.downloadHandler as DownloadHandlerTexture).texture;
                    m_texture = texture;
                    m_icon.texture = texture;
                    m_icon.enabled = true;
                    m_showIcon = true;
                }
            }
            yield break;
        }
    }
}
