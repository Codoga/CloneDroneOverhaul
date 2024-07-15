﻿using OverhaulMod.Content;
using OverhaulMod.Utils;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIDevelopmentGallery : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElement("ImageDisplay", false)]
        private readonly ModdedObject m_imageDisplay;

        [UIElement("Content")]
        private readonly Transform m_imageDisplaysContainer;

        public override bool hideTitleScreen => true;

        public override void Show()
        {
            base.Show();
            Populate();
        }

        public override void Hide()
        {
            base.Hide();
            if (m_imageDisplaysContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_imageDisplaysContainer);
        }

        public void Populate()
        {
            if (m_imageDisplaysContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_imageDisplaysContainer);

            string directory = Path.Combine(ModCore.addonsFolder, ContentManager.GALLERY_CONTENT_FOLDER_NAME);
            if (!Directory.Exists(directory))
            {
                ModUIUtils.MessagePopupOK("\"Behind The Scenes\" add-on not installed", "Install this add-on to make this menu work.", true);
                return;
            }

            string[] images = Directory.GetFiles(directory, "*.jpg");
            if (images.IsNullOrEmpty())
                return;

            foreach (string imageFilePath in images)
            {
                ModdedObject moddedObject = Instantiate(m_imageDisplay, m_imageDisplaysContainer);
                moddedObject.gameObject.SetActive(true);
                UIElementGalleryImage galleryImage = moddedObject.gameObject.AddComponent<UIElementGalleryImage>();
                galleryImage.filePath = imageFilePath;
                galleryImage.InitializeElement();
            }
        }
    }
}
