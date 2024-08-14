using OverhaulMod.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverhaulMod.Content.Personalization
{
    public class CVMModelPreset
    {
        public string CvmFilePath;

        public WeaponType Weapon;

        public WeaponVariant Variant;

        public CVMModelPreset()
        {

        }

        public CVMModelPreset(bool defaultValues)
        {
            if (defaultValues)
            {
                Weapon = WeaponType.Sword;
                Variant = WeaponVariant.Normal;
            }
        }
    }
}
