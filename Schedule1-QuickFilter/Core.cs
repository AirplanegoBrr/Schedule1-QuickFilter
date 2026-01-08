global using ItemSlotList = Il2CppSystem.Collections.Generic.List<Il2CppScheduleOne.ItemFramework.ItemSlot>;

using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Schedule1_QuickFilter.Core), "Schedule1-QuickFilter", "1.4.0", "airplanegobrr", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace Schedule1_QuickFilter {
    public class Core: MelonMod {

        KeyCode moveFilteredKey = KeyCode.H;
        KeyCode moveKey = KeyCode.Y;
        KeyCode grabKey = KeyCode.B;

        public override void OnInitializeMelon() {
            LoggerInstance.Msg("Loaded QuickFilter by APGB!.");

            string derivedCategoryName = "QuickMoveFilter";
            MelonPreferences_Category settingsCategory = MelonPreferences.CreateCategory(derivedCategoryName);

            var moveFilteredKeybind = settingsCategory.CreateEntry("MovePushFilterd", "H", "[Quick Move] Move filtered");
            var moveKeybind = settingsCategory.CreateEntry("MovePush", "Y", "[Quick Move] Move");
            var grabKeybind = settingsCategory.CreateEntry("GrabShelf", "B", "[Quick Move] Grab shelf");

            // Try parsing each config key to the Key enum (from Unity.InputSystem if you're using Input System package)
            if (Enum.TryParse<KeyCode>(moveFilteredKeybind.Value, true, out KeyCode parsedFilteredKey))
                moveFilteredKey = parsedFilteredKey;

            if (Enum.TryParse<KeyCode>(moveKeybind.Value, true, out KeyCode parsedMoveKey))
                moveKey = parsedMoveKey;

            if (Enum.TryParse<KeyCode>(grabKeybind.Value, true, out KeyCode parsedGrabKey))
                grabKey = parsedGrabKey;

            MelonPreferences.Save();
        }

        public override void OnLateUpdate() {
            // Item mover

            bool grabVanPushShelfFilterKey = Input.GetKeyUp(moveFilteredKey);
            bool grabVanPushShelfKey = Input.GetKeyUp(moveKey);
            bool grabShelfKey = Input.GetKeyUp(grabKey);

            if (!(grabVanPushShelfFilterKey || grabVanPushShelfKey || grabShelfKey)) { return; }
            LoggerInstance.Msg("Valid key pressed!");

            RaycastHit? testHit = RayCastUtils.GetLookedAt();

            if (!testHit.HasValue) return;
            RaycastHit hit = testHit.Value;

            GameObject lookedAtObject = hit.collider.gameObject;

            if (lookedAtObject == null) {
                LoggerInstance.Msg("[Raycast] lookedAtObject is null");
                return;
            }

            Transform transform = RayCastUtils.GetStorageObject(lookedAtObject.transform);
            ItemSlotList itemSlots = RayCastUtils.GetItemSlots(transform);

            if (itemSlots == null) {
                MelonLogger.Msg($"[Raycast] No valid storage type was found.");

                return;
            }

            PlayerInventory plrInv = PlayerSingleton<PlayerInventory>.Instance;
            ItemSlotList plrItems = plrInv.GetAllInventorySlots();

            if (grabVanPushShelfFilterKey) {
                StorageUtils.GrabVanPushShelfFilter(transform.gameObject.name, transform.parent.gameObject.name, itemSlots, plrItems, true);
            } else if (grabVanPushShelfKey) {
                StorageUtils.GrabVanPushShelfFilter(transform.gameObject.name, transform.parent.gameObject.name, itemSlots, plrItems, false);
            } else if (grabShelfKey) {
                StorageUtils.GrabItems(itemSlots, plrItems);
                // Grab items from shelf
            }
        }
    }
}