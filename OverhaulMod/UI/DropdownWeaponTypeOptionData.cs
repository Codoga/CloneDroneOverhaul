using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class DropdownWeaponTypeOptionData : Dropdown.OptionData
    {
        public WeaponType Weapon;

        public DropdownWeaponTypeOptionData(WeaponType weaponType)
        {
            if (weaponType == Combat.ModWeaponsManager.SCYTHE_TYPE)
                text = "Scythe";
            else
                text = weaponType.ToString();

            Weapon = weaponType;
        }
    }
}
