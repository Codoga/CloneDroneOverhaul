using OverhaulMod.Content;
using OverhaulMod.Utils;
using System.Collections;
using System.IO;
using UnityEngine;

namespace OverhaulMod.Patches.Behaviours
{
    internal class SkyboxesPatchBehaviour : GamePatchBehaviour
    {
        public const string OVERHAUL_DEFAULT_SKYBOXES = "overhaul_default_skyboxes";

        public override void Patch()
        {
            _ = ModActionUtils.RunCoroutine(patchCoroutine());
        }

        public override void UnPatch()
        {
            ModAdvancedCache.Remove("Chapter4Skybox_Rework");
            ModAdvancedCache.Remove("Chapter5Skybox_Rework");
        }

        private IEnumerator patchCoroutine()
        {
            yield return null;
            yield return null;

            SkyBoxManager.Instance.LevelConfigurableSkyboxes[8].SetColor("_Tint", new Color(0.6f, 0.73f, 2f, 1f));
            if (ContentManager.Instance.HasContent(ContentManager.EXTRAS_CONTENT_FOLDER_NAME))
            {
                string path = Path.Combine(ModCore.addonsFolder, ContentManager.EXTRAS_CONTENT_FOLDER_NAME);
                ModResources.LoadBundleAsync(OVERHAUL_DEFAULT_SKYBOXES, delegate (bool result)
                {
                    if (ModAdvancedCache.TryGet("Chapter4Skybox_Rework", out Material material1))
                    {
                        replaceSkyboxMaterial(material1, 2);
                    }
                    else
                    {
                        ModResources.LoadAsync(OVERHAUL_DEFAULT_SKYBOXES, "Chapter4Skybox", delegate (Material material)
                        {
                            ModAdvancedCache.Add("Chapter4Skybox_Rework", material);
                            replaceSkyboxMaterial(material, 2);
                        }, path);
                    }

                    if (ModAdvancedCache.TryGet("Chapter5Skybox_Rework", out Material material2))
                    {
                        replaceSkyboxMaterial(material2, 7);
                    }
                    else
                    {
                        ModResources.LoadAsync(OVERHAUL_DEFAULT_SKYBOXES, "Chapter5Skybox", delegate (Material material)
                        {
                            ModAdvancedCache.Add("Chapter5Skybox_Rework", material);
                            replaceSkyboxMaterial(material, 7);
                        }, path);
                    }
                }, path);
            }
            yield break;
        }

        private void replaceSkyboxMaterial(Material material, int index)
        {
            Material[] array = SkyBoxManager.Instance.LevelConfigurableSkyboxes;
            Material og = array[index];
            if (og != material && RenderSettings.skybox == og)
            {
                RenderSettings.skybox = material;
            }
            material.name = og.name;
            array[index] = material;
        }
    }
}
