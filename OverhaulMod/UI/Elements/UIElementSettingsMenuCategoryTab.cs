using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OverhaulMod.UI.Elements
{
    public class UIElementSettingsMenuCategoryTab : UIElementTab, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
    {
        public const float EXTRA_HEIGHT = 7f;

        [UIElement("Text")]
        private readonly Text m_text;

        [UIElement("SubcategoriesContainer")]
        private readonly RectTransform m_subcategoriesContainer;

        [UIElement("SubcategoriesContainer", false)]
        private readonly GameObject m_subcategoriesContainerObject;

        [UIElement("SubcategoryDisplay", false)]
        private readonly Text m_subcategoryDisplay;

        public string LocalizationID;

        private bool m_mouseIn;

        private float m_expandProgress, m_height;

        protected override void OnInitialized()
        {
            m_height = 0f;
            m_text.text = LocalizationID.IsNullOrEmpty() ? tabId : LocalizationManager.Instance.GetTranslatedString(LocalizationID);

            switch (tabId)
            {
                case "Home":
                    InitializeSubcategoryDisplay("Game interface", false);
                    InitializeSubcategoryDisplay("Multiplayer settings", false);
                    break;
                case "Gameplay":
                    InitializeSubcategoryDisplay("Gameplay settings", false);
                    InitializeSubcategoryDisplay("Twitch", false);
                    break;
                case "Sounds":
                    InitializeSubcategoryDisplay("Volume", false);
                    InitializeSubcategoryDisplay("Filters", false);
                    break;
                case "Multiplayer":
                    InitializeSubcategoryDisplay("Multiplayer settings", false);
                    InitializeSubcategoryDisplay("Player", false);
                    break;
                case "Advanced":
                    InitializeSubcategoryDisplay("Mod-bot", false);
                    InitializeSubcategoryDisplay("Transitions", false);
                    InitializeSubcategoryDisplay("Rich presence", false);
                    InitializeSubcategoryDisplay("Multiplayer settings", false);
                    InitializeSubcategoryDisplay("Misc.", false);
                    InitializeSubcategoryDisplay("Reset settings", false);
                    break;
                case "Graphics":
                    InitializeSubcategoryDisplay("Window", true);
                    InitializeSubcategoryDisplay("Render", true);
                    InitializeSubcategoryDisplay("Post effects", true);
                    if (ModFeatures.IsEnabled(ModFeatures.FeatureType.MoreImageEffects)) InitializeSubcategoryDisplay("Color blindness mode", true);
                    InitializeSubcategoryDisplay("Camera", false);
                    InitializeSubcategoryDisplay("Environment", false);
                    InitializeSubcategoryDisplay("Voxel engine", false);
                    InitializeSubcategoryDisplay("Garbage", false);
                    break;
                case "Interface":
                    InitializeSubcategoryDisplay("Game interface", false);
                    InitializeSubcategoryDisplay("Energy bar enhancements", false);
                    InitializeSubcategoryDisplay("Labels", false);
                    break;
            }
        }

        public override void Update()
        {
            RectTransform rt = m_subcategoriesContainer;
            Vector2 sd = rt.sizeDelta;
            sd.y = Mathf.Lerp(0f, m_height + EXTRA_HEIGHT, NumberUtils.EaseOutQuad(0f, 1f, m_expandProgress));
            rt.sizeDelta = sd;

            m_subcategoriesContainerObject.SetActive(m_expandProgress > 0f);

            m_expandProgress = Mathf.Clamp01(m_expandProgress + ((m_mouseIn ? 1f : -1f) * Time.unscaledDeltaTime * 7.5f));
        }

        public void InitializeSubcategoryDisplay(string text, bool subHeader)
        {
            m_height += 20f + EXTRA_HEIGHT;
            Text text1 = Instantiate(m_subcategoryDisplay, m_subcategoriesContainer);
            text1.gameObject.SetActive(true);
            text1.text = LocalizationManager.Instance.GetTranslatedString($"settings_{(subHeader ? "subheader" : "header")}_{text.ToLower().Replace(' ', '_')}");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_mouseIn = m_height != 0f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_mouseIn = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_mouseIn = false;
        }
    }
}
