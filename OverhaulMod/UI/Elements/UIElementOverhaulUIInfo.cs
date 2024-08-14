using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementOverhaulUIInfo : OverhaulUIBehaviour
    {
        [UIElement("Name")]
        private readonly Text m_uiNameText;

        [UIElement("MissingPreviewText", false)]
        private readonly Text m_missingPreviewText;

        [UIElement("TextBG")]
        private readonly RectTransform m_textBG;

        private RawImage m_image;

        private Texture2D m_loadedImage;

        private UnityWebRequest m_webRequest;

        public string PreviewFile;

        private bool m_destroyed;

        protected override void OnInitialized()
        {
            m_image = GetComponent<RawImage>();
            LoadPreview();
        }

        public override void OnDestroy()
        {
            m_destroyed = true;

            Texture2D texture = m_loadedImage;
            if (texture)
            {
                Destroy(texture);
            }

            UnityWebRequest webRequest = m_webRequest;
            if (webRequest != null)
            {
                try
                {
                    webRequest.Abort();
                }
                catch { }
            }
        }

        public void LoadPreview()
        {
            _ = base.StartCoroutine(loadPreviewCoroutine());
        }

        private IEnumerator loadPreviewCoroutine()
        {
            yield return null;

            RectTransform rectTransform = m_textBG;
            Vector2 sd = rectTransform.sizeDelta;
            sd.x = m_uiNameText.preferredWidth + 10f;
            rectTransform.sizeDelta = sd;

            using (UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture($"file://{PreviewFile}"))
            {
                m_webRequest = unityWebRequest;
                yield return unityWebRequest.SendWebRequest();
                if (m_destroyed)
                    yield break;

                if (unityWebRequest.isDone && !unityWebRequest.isHttpError && !unityWebRequest.isNetworkError)
                {
                    Texture2D texture = (unityWebRequest.downloadHandler as DownloadHandlerTexture).texture;
                    m_loadedImage = texture;
                    m_image.texture = texture;
                }
                else
                {
                    m_missingPreviewText.gameObject.SetActive(true);
                    m_missingPreviewText.text = $"Missing preview file:\n{PreviewFile}";
                }
            }
            yield break;
        }
    }
}
