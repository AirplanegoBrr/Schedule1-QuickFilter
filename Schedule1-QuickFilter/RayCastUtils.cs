#nullable enable

using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Storage;
using MelonLoader;
using UnityEngine;

namespace Schedule1_QuickFilter {
    public static class RayCastUtils {
        public static RaycastHit? GetLookedAt() {
            PlayerCamera cam = PlayerSingleton<PlayerCamera>.instance;

            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("Vehicle", "Default");
            if (cam.LookRaycast(10f, out hit, mask)) return hit;
            return null;
        }

        public static ItemSlotList? GetItemSlots(Transform transform) {
            ItemSlotList itemSlots;

            var storageEntity = transform?.GetComponent<StorageEntity>();

            if (storageEntity == null) {
                var mixing = transform?.GetComponent<MixingStation>();
                if (mixing != null) {
                    MelonLogger.Msg("Found mixer!");
                    itemSlots = mixing.ItemSlots;
                    return itemSlots;
                }

                var packing = transform?.GetComponent<PackagingStation>();
                if (packing != null) {
                    MelonLogger.Msg("Found packer!");
                    itemSlots = packing.ItemSlots;

                    // Tyler.
                    // Why is the first slot index the packaging slot?
                    // Then the 2ed slot is the product?
                    // Why?
                    // Why tyler?

                    // why do you hate me?

                    var temp = itemSlots[0];
                    itemSlots[0] = itemSlots[1];
                    itemSlots[1] = temp;

                    return itemSlots;
                }

                var rack = transform?.GetComponent<DryingRack>();
                if (rack != null) {
                    MelonLogger.Msg("Found Drying Rack!");
                    itemSlots = rack.ItemSlots;
                    return itemSlots;
                }
            } else if (storageEntity?.ItemSlots != null) {
                itemSlots = storageEntity.ItemSlots;
                return itemSlots;
            }
            return null;
        }

        public static Transform? GetStorageObject(Transform transform) {
            var currentTransform = transform;
            int steps = 0;

            while (currentTransform != null && steps <= 3) {
                steps++;
                MelonLogger.Msg($"[Raycast] Checking: {currentTransform.gameObject.name}");

                if (GetItemSlots(currentTransform) != null) break;

                MelonLogger.Msg($"[Raycast] {currentTransform.gameObject.name} didn't have a valid Storage type");

                currentTransform = currentTransform.parent;
            }

            return currentTransform;
        }
    }
}