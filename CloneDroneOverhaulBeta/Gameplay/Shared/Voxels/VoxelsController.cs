﻿using PicaVoxel;
using UnityEngine;

namespace CDOverhaul.Shared
{
    public class VoxelsController : ModController
    {
        public static float ColorBurnMultipler;

        [OverhaulSetting("Gameplay.Voxels.Make laser burn voxels", true, false, "Cutting robots with normal sword would leave nearby voxels burnt")]
        public static bool MakeLaserBurnVoxels;

        [OverhaulSetting("Gameplay.Voxels.TES12345TMake laser burn voxels", true, false, "12345671332289", null, null, "Gameplay.Voxels.Make laser burn voxels")]
        public static bool Test2;

        [OverhaulSetting("Gameplay.Voxels.TESTMake laser burn voxels", true, false, "123456789", null, null, "Gameplay.Voxels.Make laser burn voxels")]
        public static bool Test;

        public override void Initialize()
        {
            ColorBurnMultipler = AttackManager.Instance.FireBurnColorMultiplier;

            HasAddedEventListeners = true;
            IsInitialized = true;
        }

        public static void OnVoxelDestroy(MechBodyPart bodyPart, PicaVoxelPoint picaVoxelPoint, Voxel? voxelAtPosition, Vector3 impactDirectionWorld, FireSpreadDefinition fireSpreadDefinition, Frame currentFrame)
        {
            if (!MakeLaserBurnVoxels)
            {
                return;
            }

            foreach (PicaVoxelPoint p in GetSurroundingPoints(picaVoxelPoint))
            {
                if (!bodyPart.IsVoxelWaitingToBeDestroyed(p))
                {
                    Voxel? vox = currentFrame.GetVoxelAtArrayPosition(p);
                    if (vox != null)
                    {
                        Voxel newVox = vox.Value;
                        Color32 color = new Color32((byte)Mathf.RoundToInt(vox.Value.Color.r * ColorBurnMultipler), (byte)Mathf.RoundToInt(vox.Value.Color.g * ColorBurnMultipler), (byte)Mathf.RoundToInt(vox.Value.Color.b * ColorBurnMultipler), vox.Value.Color.a);
                        newVox.Color = color;
                        currentFrame.SetVoxelAtArrayPosition(p, newVox);
                    }
                }
            }
        }

        public static PicaVoxelPoint GetOffsetPoint(in PicaVoxelPoint picaVoxelPoint, in int OffX, in int OffY, in int OffZ)
        {
            return new PicaVoxelPoint(picaVoxelPoint.X + OffX, picaVoxelPoint.Y + OffY, picaVoxelPoint.Z + OffZ);
        }

        public static PicaVoxelPoint[] GetSurroundingPoints(in PicaVoxelPoint picaVoxelPoint)
        {
            PicaVoxelPoint x1 = GetOffsetPoint(picaVoxelPoint, 1, 0, 0);
            PicaVoxelPoint x2 = GetOffsetPoint(picaVoxelPoint, -1, 0, 0);
            PicaVoxelPoint y1 = GetOffsetPoint(picaVoxelPoint, 0, 1, 0);
            PicaVoxelPoint y2 = GetOffsetPoint(picaVoxelPoint, 0, -1, 0);
            PicaVoxelPoint z1 = GetOffsetPoint(picaVoxelPoint, 0, 0, 1);
            PicaVoxelPoint z2 = GetOffsetPoint(picaVoxelPoint, 0, 0, -1);
            return new PicaVoxelPoint[] { x1, x2, y1, y2, z1, z2 };
        }
    }
}