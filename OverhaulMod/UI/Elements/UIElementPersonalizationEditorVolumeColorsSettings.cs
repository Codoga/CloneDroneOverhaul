using OverhaulMod.Content.Personalization;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementPersonalizationEditorVolumeColorsSettings : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_closeButton;

        [UIElement("ReplaceColorDisplay", false)]
        private readonly ModdedObject m_replaceColorDisplay;

        [UIElement("Content")]
        private readonly Transform m_container;

        [UIElementAction(nameof(OnCopyColorsButtonClicked))]
        [UIElement("CopyColorsButton")]
        private readonly Button m_copyColorsButton;

        [UIElementAction(nameof(OnPasteColorsButtonClicked))]
        [UIElement("PasteColorsButton")]
        private readonly Button m_pasteColorsButton;

        private VolumeSettingsPreset m_volumeSettingsPreset;

        private List<ColorPairFloat> m_colorPairs;

        private Dictionary<string, FavoriteColorSettings> m_favoriteColorSettings;

        public Action<string> onColorChanged
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            m_colorPairs = new List<ColorPairFloat>();
            m_pasteColorsButton.interactable = false;
        }

        public void Populate(VolumeSettingsPreset volumeSettingsPreset)
        {
            m_volumeSettingsPreset = volumeSettingsPreset;
            m_colorPairs = PersonalizationEditorManager.Instance.GetColorPairsFromString(volumeSettingsPreset.ColorReplacements);
            m_favoriteColorSettings = volumeSettingsPreset.ReplaceWithFavoriteColors;
            populate(m_colorPairs, m_favoriteColorSettings);
        }

        private void populate(List<ColorPairFloat> list, Dictionary<string, FavoriteColorSettings> replaceWithFavoriteColors)
        {
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            if (!list.IsNullOrEmpty())
            {
                int index = 0;
                foreach (ColorPairFloat cp in list)
                {
                    int i = index;
                    ModdedObject colorPairDisplay = Instantiate(m_replaceColorDisplay, m_container);
                    colorPairDisplay.gameObject.SetActive(true);
                    UIElementPersonalizationEditorColorPairDisplay editorColorPairDisplay = colorPairDisplay.gameObject.AddComponent<UIElementPersonalizationEditorColorPairDisplay>();
                    editorColorPairDisplay.InitializeElement();
                    editorColorPairDisplay.returnNewPair = false;
                    editorColorPairDisplay.colorPair = cp;
                    editorColorPairDisplay.favoriteColorSettings = replaceWithFavoriteColors;
                    editorColorPairDisplay.colorPickerTransform = UIPersonalizationEditor.instance.transform;
                    editorColorPairDisplay.onValueChanged.AddListener(onColorChangedCallback);
                    editorColorPairDisplay.onFavoriteColorSettingsChanged.AddListener(delegate
                    {
                        GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                    });

                    index++;
                }
            }
        }

        private void onColorChangedCallback(ColorPairFloat colorPairFloat)
        {
            onColorChanged?.Invoke(PersonalizationEditorManager.Instance.GetStringFromColorPairs(m_colorPairs));
        }

        public void OnCopyColorsButtonClicked()
        {
            if (m_colorPairs == null || m_favoriteColorSettings == null)
                return;

            m_pasteColorsButton.interactable = true;
            PersonalizationEditorCopyPasteManager.Instance.CopyColorSettings(m_colorPairs, m_favoriteColorSettings);
        }

        public void OnPasteColorsButtonClicked()
        {
            List<ColorPairFloat> originalColors = m_colorPairs;
            Dictionary<string, FavoriteColorSettings> originalFavoriteColors = m_favoriteColorSettings;

            if (originalColors == null || originalFavoriteColors == null)
                return;

            m_pasteColorsButton.interactable = false;

            PersonalizationEditorCopyPasteManager.Instance.PasteColorSettings(out List<ColorPairFloat> colorPairs, out Dictionary<string, FavoriteColorSettings> favoriteColors);

            foreach (ColorPairFloat colorPairFloatA in originalColors)
            {
                foreach (ColorPairFloat colorPairFloatB in colorPairs)
                {
                    if (colorPairFloatA.ColorA == colorPairFloatB.ColorA)
                        colorPairFloatA.ColorB = colorPairFloatB.ColorB;
                }
            }

            originalFavoriteColors.Clear();
            foreach (KeyValuePair<string, FavoriteColorSettings> kv in favoriteColors)
                originalFavoriteColors.Add(kv.Key, kv.Value.Clone());

            onColorChangedCallback(null);
            populate(originalColors, originalFavoriteColors);
        }
    }
}
