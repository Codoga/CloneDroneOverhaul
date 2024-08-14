using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIInformationSelectWindow : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnOverhaulInfoButtonClicked))]
        [UIElement("OverhaulInfoButton")]
        private readonly Button m_overhaulInfoButton;

        [UIElementAction(nameof(OnModBotInfoButtonClicked))]
        [UIElement("ModBotInfo")]
        private readonly Button m_modBotInfoButton;

        [UIElementAction(nameof(OnCloneDroneInfoButtonClicked))]
        [UIElement("CloneDroneInfoButton")]
        private readonly Button m_cloneDroneInfoButton;

        [UIElementAction(nameof(OnGalleryButtonClicked))]
        [UIElement("GalleryButton")]
        private readonly Button m_galleryButton;

        public override bool hideTitleScreen => true;

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public void OnOverhaulInfoButtonClicked()
        {
            _ = ModUIConstants.ShowOverhaulModInfoMenu(base.transform);
        }

        public void OnModBotInfoButtonClicked()
        {
            Application.OpenURL("https://modbot.org/credits.html");
        }

        public void OnCloneDroneInfoButtonClicked()
        {
            Hide();
            ModCache.titleScreenUI.OnCreditsButtonClicked();
        }

        public void OnGalleryButtonClicked()
        {
            _ = ModUIConstants.ShowDevelopmentGallery(base.transform);
        }
    }
}
