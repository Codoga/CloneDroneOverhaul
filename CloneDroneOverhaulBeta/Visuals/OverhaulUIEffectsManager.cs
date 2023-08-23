﻿using CDOverhaul.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CDOverhaul.Visuals
{
    public class OverhaulUIEffectsManager : OverhaulManager<OverhaulUIEffectsManager>
    {
        public GameObject canvasObject
        {
            get;
            private set;
        }

        public DitheringUIEffect dithering
        {
            get;
            private set;
        }

        protected override void OnAssetsLoaded()
        {
            canvasObject = Instantiate(OverhaulAssetsController.GetAsset("UIImageEffects", OverhaulAssetPart.Part2, false));
            DontDestroyOnLoad(canvasObject);

            ModdedObject moddedObject = canvasObject.GetComponent<ModdedObject>();
            dithering = moddedObject.GetObject<Transform>(0).gameObject.AddComponent<DitheringUIEffect>();
        }
    }
}