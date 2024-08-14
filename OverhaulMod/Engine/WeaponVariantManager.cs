using OverhaulMod.Combat;

namespace OverhaulMod.Engine
{
    public class WeaponVariantManager
    {
        public static void GetWeaponVariant(FirstPersonMover firstPersonMover, WeaponType weaponType, out bool isOnFire, out bool isGreatSword)
        {
            isGreatSword = false;
            isOnFire = false;
            switch (weaponType)
            {
                case WeaponType.Sword:
                    isGreatSword = GameModeManager.UsesMultiplayerSpeedMultiplier();
                    isOnFire = firstPersonMover.HasUpgrade(UpgradeType.FireSword);
                    break;
                case WeaponType.Hammer:
                    isOnFire = firstPersonMover.HasUpgrade(UpgradeType.FireHammer);
                    break;
                case WeaponType.Spear:
                    isOnFire = firstPersonMover.HasUpgrade(UpgradeType.FireSpear);
                    break;
                case ModWeaponsManager.SCYTHE_TYPE:
                    isOnFire = firstPersonMover.HasUpgrade(ModUpgradesManager.SCYTHE_FIRE_UPGRADE);
                    break;
            }
        }

        public static void GetWeaponVariant(FirstPersonMover firstPersonMover, WeaponType weaponType, out WeaponVariant showConditions)
        {
            GetWeaponVariant(firstPersonMover, weaponType, out bool of, out bool gs);
            if (!of && !gs)
            {
                showConditions = WeaponVariant.Normal;
            }
            else if (of && !gs)
            {
                showConditions = WeaponVariant.OnFire;
            }
            else if (!of && gs)
            {
                showConditions = WeaponVariant.NormalMultiplayer;
            }
            else if (of && gs)
            {
                showConditions = WeaponVariant.OnFireMultiplayer;
            }
            else
            {
                showConditions = WeaponVariant.None;
            }
        }

        public static string GetWeaponVariantString(WeaponVariant weaponVariant)
        {
            switch (weaponVariant)
            {
                case WeaponVariant.Normal:
                    return "Normal";
                case WeaponVariant.OnFire:
                    return "Fire";
                case WeaponVariant.NormalMultiplayer:
                    return "Normal (Multiplayer)";
                case WeaponVariant.OnFireMultiplayer:
                    return "Fire (Multiplayer)";
            }
            return weaponVariant.ToString();
        }
    }
}
