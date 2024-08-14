using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Visuals.Environment
{
    public class WeatherInfo
    {
        public string Name;

        public bool IsDefault;

        public float EmissionRate;

        public KeyValuePair<string, string> BundleAndAsset;

        public GameObject GetVFXPrefab()
        {
            return IsDefault ? null : ModResources.Prefab(BundleAndAsset.Key, BundleAndAsset.Value);
        }
    }
}
