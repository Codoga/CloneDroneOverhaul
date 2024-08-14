using OverhaulMod.Engine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class DropdownWeaponVariantOptionData : Dropdown.OptionData
    {
        public WeaponVariant Value;

        public DropdownWeaponVariantOptionData(WeaponVariant value)
        {
            text = WeaponVariantManager.GetWeaponVariantString(value);
            Value = value;
        }
    }
}
