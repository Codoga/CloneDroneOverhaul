using OverhaulMod.Content.Personalization;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class DropdownPersonalizationItemInfo : Dropdown.OptionData
    {
        public PersonalizationItemInfo ItemInfo;

        public DropdownPersonalizationItemInfo(PersonalizationItemInfo itemInfo)
        {
            ItemInfo = itemInfo;

            string txt = itemInfo.Name.Replace("template_", string.Empty);
            string upperName = txt[0].ToString().ToUpper();
            txt = txt.Remove(0, 1).Insert(0, upperName);
            text = txt;
        }
    }
}
