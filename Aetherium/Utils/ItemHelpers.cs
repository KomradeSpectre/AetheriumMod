using RoR2;
using System;
using UnityEngine;

namespace Aetherium.Utils
{
    internal class ItemHelpers
    {
        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj)
        {
            MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = meshes[i].material,
                    renderer = meshes[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }

        public static string OrderManifestLoreFormatter(string deviceName, string estimatedDelivery, string sentTo, string trackingNumber, string devicePickupDesc, string shippingMethod, string orderDetails)
        {
            string[] Manifest =
            {
                $"<align=left>Estimated Delivery:<indent=70%>Sent To:</indent></align>",
                $"<align=left>{estimatedDelivery}<indent=70%>{sentTo}</indent></align>",
                "",
                $"<indent=1%><style=cIsDamage><size=125%><u>  Shipping Details:\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0</u></size></style></indent>",
                "",
                $"<indent=2%>-Order: <style=cIsUtility>{deviceName}</style></indent>",
                $"<indent=4%><style=cStack>Tracking Number:  {trackingNumber}</style></indent>",
                "",
                $"<indent=2%>-Order Description: {devicePickupDesc}</indent>",
                "",
                $"<indent=2%>-Shipping Method: <style=cIsHealth>{shippingMethod}</style></indent>",
                "",
                "",
                "",
                $"<indent=2%>-Order Details: {orderDetails}</indent>",
                "",
                "",
                "",
                "<style=cStack>Delivery being brought to you by the brand new </style><style=cIsUtility>Orbital Drop-Crate System (TM)</style>. <style=cStack><u>No refunds.</u></style>"
            };
            return String.Join("\n", Manifest);
        }
    }
}