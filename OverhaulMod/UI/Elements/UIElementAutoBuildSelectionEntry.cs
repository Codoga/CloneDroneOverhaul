using OverhaulMod.Engine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OverhaulMod.UI
{
    public class UIElementAutoBuildSelectionEntry : OverhaulUIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
    {
        [UIElement("ButtonBG", false)]
        private readonly GameObject m_buttonBGObject;

        public AutoBuildInfo BuildInfo;

        public UIAutoBuildSelectionMenu Menu;

        public void SetBGActive(bool value)
        {
            m_buttonBGObject.SetActive(value);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Menu.SelectEntry(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Menu.SelectEntry(null);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }
    }
}
