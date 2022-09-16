﻿using PicaVoxel;
using UnityEngine;

namespace CloneDroneOverhaul.Patching
{
    public class BodyPartPatcher
    {
        public static void OnBodyPartStart(MechBodyPart __instance)
        {
            //GarbageTarget target = __instance.GetComponentInParent<GarbageTarget>();
            //if(target == null || target.State != GarbageState.NotGarbage)
            //{
            //    return;
            //}
            Voxel[] voxels = __instance.GetMyVolume.GetCurrentFrame().Voxels;
            ReplaceVoxelColor replaceVoxels = __instance.GetComponent<ReplaceVoxelColor>();
            for (int i = 0; i < voxels.Length; i++)
            {
                Color normalCol = voxels[i].Color;

                float num = UnityEngine.Random.Range(0.95f, 1.00f);
                bool shouldUpdateColor = false;
                if (replaceVoxels == null)
                {
                    shouldUpdateColor = voxels[i].Color.a == 255;
                }
                else if (replaceVoxels != null && replaceVoxels.Old != normalCol)
                {
                    shouldUpdateColor = true;
                }

                if (shouldUpdateColor)
                {
                    Color32 color = new Color32((byte)Mathf.RoundToInt(voxels[i].Color.r * num), (byte)Mathf.RoundToInt(voxels[i].Color.g * num),
                       (byte)Mathf.RoundToInt(voxels[i].Color.b * num), voxels[i].Color.a);
                    voxels[i].Color = color;
                }
            }

            Singleton<CacheManager>.Instance.GetVolume(__instance.transform).GetCurrentFrame().UpdateAllChunks();
        }

        public static void OnVoxelCut(MechBodyPart __instance, PicaVoxelPoint picaVoxelPoint, Voxel? voxelAtPosition, Vector3 localPosition, Vector3 volumeWorldCenter, Vector3 impactDirectionWorld, FireSpreadDefinition fireSpreadDefinition, Frame currentFrame)
        {
            try
            {
                Vector3 voxelWorldPosition = currentFrame.GetVoxelWorldPosition(picaVoxelPoint);
                if (fireSpreadDefinition == null)
                {
                    Vector3 a = (voxelWorldPosition - volumeWorldCenter).normalized + impactDirectionWorld;
                    VoxelParticleSystem.Instance.SpawnSingle(voxelWorldPosition, voxelAtPosition.Value.Color, __instance.GetVoxelSize() * 0.75f, 1f * a);
                    VoxelParticleSystem.Instance.SpawnSingle(voxelWorldPosition, voxelAtPosition.Value.Color, __instance.GetVoxelSize() * 0.75f, (3f * impactDirectionWorld) + (1f * a));
                }

                OverhaulMain.Visuals.EmitBodyPartCutVFX(voxelWorldPosition, fireSpreadDefinition != null);
            }
            catch(System.Exception ex)
            {
                CloneDroneOverhaul.UI.Notifications.Notification notif = new UI.Notifications.Notification();
                notif.SetUp("Nullpoint : BodyPartPatcher - OnVoxelCut", ex.Message, 5, new UnityEngine.Vector2(400, 700), new UnityEngine.Color(1f, 0.1559941f, 0.1792453f, 0.6f), new UI.Notifications.Notification.NotificationButton[] { });
            }
        }

        public static void OnBodyPartDamaged(BaseBodyPart part)
        {
            if (part is MindSpaceBodyPart)
            {
                OverhaulMain.Visuals.EmitMSBodyPartDamage(part.transform.position);
            }
        }

        public static void AddMechBodyPartComponent(GameObject obj)
        {
            MechBodyPart part = obj.AddComponent<MechBodyPart>();

        }
    }
}
