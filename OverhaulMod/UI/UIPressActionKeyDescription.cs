using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPressActionKeyDescription : OverhaulUIBehaviour
    {
        [ModSetting(ModSettingsConstants.PAK_DESCRIPTION_BG, true)]
        public static bool EnableBG;

        [ModSetting(ModSettingsConstants.PAK_DESCRIPTION_FONT, 1)]
        public static int FontType;

        [ModSetting(ModSettingsConstants.PAK_DESCRIPTION_FONT_SIZE, 10)]
        public static int FontSize;

        [UIElement("BG")]
        private readonly RectTransform m_bg;

        [UIElement("BG", false)]
        private readonly GameObject m_bgObject;

        [UIElement("BG")]
        private readonly Image m_bgImage;

        [UIElement("BG")]
        private readonly CanvasGroup m_bgCanvasGroup;

        [UIElement("Text")]
        private readonly Text m_text;

        private BetterOutline m_textOutline;

        public override bool closeOnEscapeButtonPress => false;

        private float m_expandProgress;

        private bool m_show;

        private int m_siblingIndex;

        protected override void OnInitialized()
        {
            BetterOutline betterOutline = m_text.gameObject.AddComponent<BetterOutline>();
            betterOutline.effectColor = Color.black;
            betterOutline.effectDistance = Vector2.one * 1.25f;
            m_textOutline = betterOutline;

            ModSettingsManager.Instance.AddSettingValueChangedListener(refreshSettings, ModSettingsConstants.PAK_DESCRIPTION_BG);
            ModSettingsManager.Instance.AddSettingValueChangedListener(refreshSettings, ModSettingsConstants.PAK_DESCRIPTION_FONT);
            ModSettingsManager.Instance.AddSettingValueChangedListener(refreshSettings, ModSettingsConstants.PAK_DESCRIPTION_FONT_SIZE);

            refreshSettings(null);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            ModSettingsManager.Instance.RemoveSettingValueChangedListener(refreshSettings, ModSettingsConstants.PAK_DESCRIPTION_BG);
            ModSettingsManager.Instance.RemoveSettingValueChangedListener(refreshSettings, ModSettingsConstants.PAK_DESCRIPTION_FONT);
            ModSettingsManager.Instance.RemoveSettingValueChangedListener(refreshSettings, ModSettingsConstants.PAK_DESCRIPTION_FONT_SIZE);
        }

        public override void Update()
        {
            Text textComponent = m_text;

            RectTransform rt = m_bg;
            Vector2 sd = rt.sizeDelta;
            sd.x = Mathf.Lerp(0f, Mathf.Clamp(textComponent.preferredWidth + 10f, 100f, 200f), NumberUtils.EaseOutQuad(0f, 1f, m_expandProgress));
            sd.y = Mathf.Lerp(0f, textComponent.preferredHeight + 10f, NumberUtils.EaseOutQuad(0f, 1f, m_expandProgress));
            rt.sizeDelta = sd;

            m_bgObject.SetActive(m_expandProgress > 0f);
            if (!m_show && m_expandProgress == 0f)
            {
                if (!textComponent.text.IsNullOrEmpty())
                    textComponent.text = null;
            }

            m_expandProgress = Mathf.Clamp01(m_expandProgress + ((m_show ? 1f : -1f) * Time.unscaledDeltaTime * 7.5f));
        }

        public void ShowText(string text)
        {
            if (FontType == 0)
            {
                m_text.font = LocalizationManager.Instance.GetCurrentSubtitlesFont();
            }

            m_text.text = text;
            m_show = true;
        }

        public void HideText()
        {
            m_show = false;
        }

        public void SetSiblingIndex(bool last)
        {
            if (last)
            {
                m_siblingIndex = base.transform.GetSiblingIndex();
                base.transform.SetAsLastSibling();
            }
            else if (m_siblingIndex != 0)
            {
                base.transform.SetSiblingIndex(m_siblingIndex);
            }
        }

        private void refreshSettings(object obj)
        {
            m_textOutline.enabled = !EnableBG;
            m_bgImage.enabled = EnableBG;

            m_text.fontSize = FontSize;
            switch (FontType)
            {
                case 1:
                    m_text.font = ModResources.Font(AssetBundleConstants.UI, "3117-font");
                    break;
                case 2:
                    m_text.font = ModResources.Font(AssetBundleConstants.UI, "Piksieli-Prosto");
                    break;
                case 3:
                    m_text.font = ModResources.Font(AssetBundleConstants.UI, "Edit-Undo");
                    break;
                case 4:
                    m_text.font = ModResources.Font(AssetBundleConstants.UI, "OpenSans-Regular");
                    break;
                case 5:
                    m_text.font = ModResources.Font(AssetBundleConstants.UI, "OpenSans-ExtraBold");
                    break;
            }
        }
    }
}
