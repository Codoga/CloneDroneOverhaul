using HarmonyLib;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using OverhaulMod.Visuals;
using PicaVoxel;
using System;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(MechBodyPart))]
    internal static class MechBodyPart_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MechBodyPart.createNewVoxelBeingDestroyed), new Type[] { typeof(PicaVoxelPoint), typeof(FireSpreadDefinition), typeof(float) })]
        private static void createNewVoxelBeingDestroyed_Postfix(MechBodyPart __instance, ref VoxelBeingDestroyed __result, PicaVoxelPoint picaVoxelPoint, FireSpreadDefinition fireSpreadDefinition, float probabilityOfFireSpread)
        {
            if (FadingVoxelManager.EnableFading && fireSpreadDefinition != null && !__instance.IgnoreColorBurnForGlowingVoxels)
            {
                FadingVoxelManager manager = FadingVoxelManager.Instance;
                if (manager)
                {
                    __result.TimeToDestroy += manager.timeToDestroyOffset;
                    manager.AddFadingVoxel(picaVoxelPoint, __instance, __result.TimeToDestroy);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MechBodyPart.destroyVoxelAtPositionFromCut))]
        private static bool destroyVoxelAtPositionFromCut_Prefix(MechBodyPart __instance, PicaVoxelPoint picaVoxelPoint, Voxel? voxelAtPosition, Vector3 localPosition, Vector3 volumeWorldCenter, Vector3 impactDirectionWorld, FireSpreadDefinition fireSpreadDefinition, Frame currentFrame)
        {
            bool hasFire = fireSpreadDefinition != null;
            if (voxelAtPosition != null)
            {
                Vector3 vector = currentFrame.GetVoxelWorldPosition(picaVoxelPoint);
                Vector3 vector2 = (vector - volumeWorldCenter).normalized + impactDirectionWorld;
                if (hasFire)
                {
                    GlobalFireParticleSystem.Instance.SpawnSingleBig(vector, (5f * impactDirectionWorld) + (1f * vector2), 1f);

                    if (ParticleManager.EnableParticles && UnityEngine.Random.value < 0.1f)
                        ParticleManager.Instance.SpawnFireCutParticles(vector);
                }
                else
                {
                    VoxelParticleSystem.Instance.SpawnSingle(vector, voxelAtPosition.Value.Color, __instance.getVoxelParticleSize() * 0.75f, (3f * impactDirectionWorld) + (1f * vector2));

                    if (ParticleManager.EnableParticles && UnityEngine.Random.value < 0.1f)
                        ParticleManager.Instance.SpawnLaserCutParticles(vector);

                    if (FadingVoxelManager.EnableBurning)
                    {
                        FadingVoxelManager fadingVoxelManager = ModCache.fadingVoxelManager;
                        foreach (PicaVoxelPoint p in fadingVoxelManager.GetSurroundingPoints(picaVoxelPoint))
                        {
                            if (__instance.IsVoxelWaitingToBeDestroyed(p))
                                continue;

                            Voxel? vox = currentFrame.GetVoxelAtArrayPosition(p);
                            if (vox == null)
                                continue;

                            Color32 oldColor = vox.Value.Color;
                            Voxel theVox = vox.Value;
                            theVox.Color = new Color32(fadingVoxelManager.BurnColor(oldColor.r),
                                 fadingVoxelManager.BurnColor(oldColor.g),
                                 fadingVoxelManager.BurnColor(oldColor.b),
                                oldColor.a);
                            currentFrame.SetVoxelAtArrayPosition(p, theVox);
                        }
                    }
                }
            }

            AttackManager attackManager = ModCache.attackManager;
            Color c = hasFire ? attackManager.BodyOnFireColor : attackManager.HitColor;
            currentFrame.SetVoxelAtArrayPosition(picaVoxelPoint, new Voxel
            {
                Color = c,
                State = VoxelState.Active,
                Value = 1
            });

            __instance.DestroyVoxelAfterWait(picaVoxelPoint, fireSpreadDefinition);
            return false;
        }
    }
}
