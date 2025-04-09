using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Storage;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(Schedule1_QuickFilter.Core), "Schedule1-QuickFilter", "1.1.0", "airplanegobrr", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

// TODO: Make shelf scans better
// We need to find the "StorageRack_Large(Clone)" in one of the parents
// Find the Storage entity in one of the parents idfk
// Lazy ill do it later! :D

namespace Schedule1_QuickFilter {
    public class Core : MelonMod {

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

        Il2CppSystem.Collections.Generic.List<ItemSlot> findSameOrEmptySlot(Il2CppSystem.Collections.Generic.List<ItemSlot> storageSlots, ItemInstance item) {
            Il2CppSystem.Collections.Generic.List<ItemSlot> validSlots = new();
            Il2CppSystem.Collections.Generic.List<ItemSlot> validEmptySlots = new();

            LoggerInstance.Msg($"[FSOES] Storage has {storageSlots._size} slots");

            for (int i = 0; i < storageSlots._size; i++) {

                ItemSlot storageSlot = storageSlots[i];

                LoggerInstance.Msg($"[FSOES] Slot {i} has {storageSlot?.Quantity} of {storageSlot?.ItemInstance?.Name}");

                if (storageSlot.ItemInstance == null) {
                    validEmptySlots.Add(storageSlot);
                    continue;
                }

                if (storageSlot.ItemInstance.Name == item.Name && storageSlot.ItemInstance.Quantity != storageSlot.ItemInstance.StackLimit) {
                    LoggerInstance.Msg($"[FSOES] Adding slot {i} as it matches requirements.");
                    validSlots.Add(storageSlot);
                    continue;

                }
            }

            foreach (var slot in validEmptySlots) {
                validSlots.Add(slot);
            }

            LoggerInstance.Msg($"[FSOES] Got {validSlots.Count} valid slots");

            return validSlots;
        }

        bool filterCheck(Il2CppSystem.Collections.Generic.List<ItemSlot> storage, ItemSlot item) {
            bool foundFilterItem = false;

            for (var slotItemIndex = 0; slotItemIndex < storage._size; slotItemIndex++) {
                ItemSlot slotItem = storage[slotItemIndex];
                if (slotItem?.ItemInstance != null && slotItem?.ItemInstance?.Name == item?.ItemInstance?.Name) {
                    foundFilterItem = true;
                }
            }
            return foundFilterItem;
        }

        void addItemToStorage(ItemSlot from, Il2CppSystem.Collections.Generic.List<ItemSlot> to, bool filter) {
            if (from == null || to == null) return;

            // Check if the storage container has the item in it
            if (filter) {
                bool foundItem = filterCheck(to, from);

                if (!foundItem) {
                    LoggerInstance.Msg($"[AITS] Item {from.ItemInstance?.Name} not allowed by filter.");
                    return; // Storage container doesn't have this item in it
                }
            }

            Il2CppSystem.Collections.Generic.List<ItemSlot> validSlots = findSameOrEmptySlot(to, from.ItemInstance);

            LoggerInstance.Msg($"[AITS] Found {validSlots.Count} valid slots");

            for (int slotIndex = 0; slotIndex < validSlots.Count; slotIndex++) {
                ItemSlot slot = validSlots[slotIndex];

                if (slot.ItemInstance == null && from.ItemInstance != null) {
                    LoggerInstance.Msg($"[AITS] Moving {from?.ItemInstance?.Quantity} of {from?.ItemInstance?.Name} to storage");
                    slot.InsertItem(from.ItemInstance);
                    from.ClearItemInstanceRequested();
                    return; // We are done.
                } else if (slot.ItemInstance?.Name == from.ItemInstance?.Name) {// Same item
                    // Find space left
                    int spaceInSlot = slot.ItemInstance.StackLimit - slot.Quantity;

                    // Get how many items we can fit in the slot
                    int ableToFit = Math.Min(from.Quantity, spaceInSlot);

                    if (ableToFit <= 0) {
                        LoggerInstance.Msg($"[AITS] Can't fit item! (PLEASE SCREENSHOT CONSOLE AS THIS SHOULDN'T HAPPEN!)");
                        continue;
                    }

                    // Get how many cound't fit
                    int leftOver = from.Quantity - ableToFit;

                    LoggerInstance.Msg($"[AITS] [Move info]\nLimit: {slot.ItemInstance.StackLimit}\nSlotCount: {slot.ItemInstance.Quantity}\nFromCount: {from.Quantity}\nSpaceLeft: {spaceInSlot}\nFit: {ableToFit}\nLeftover: {leftOver}");

                    LoggerInstance.Msg($"[AITS] Adding {ableToFit} {slot.ItemInstance.Name}");

                    // Add the items to the slot
                    slot.ItemInstance.ChangeQuantity(ableToFit);

                    from.ItemInstance.SetQuantity(leftOver);

                    if (from.Quantity == 0) {
                        from.ClearItemInstanceRequested();
                        return;
                    }
                }
            }
        }
        void grabVanPushShelfFilter(GameObject lookedAtObject, Il2CppSystem.Collections.Generic.List<ItemSlot> plrItems, bool filter) {
            if (lookedAtObject.transform.parent.parent.gameObject.name == "DeliveryVehicles") { // Van
                LoggerInstance.Msg("[Van] Van found! Using Van mode and pulling items");
                GameObject van = lookedAtObject.transform.parent.gameObject;
                StorageEntity vanInv = van.GetComponent<StorageEntity>();
                Il2CppSystem.Collections.Generic.List<ItemSlot> vanItems = vanInv.ItemSlots;

                // LoggerInstance.Msg($"[Van -> Player] Moving Item: { item.Name} | Quantity: {quantity}");

                foreach (var vanItemSlot in vanItems) {
                    if (vanItemSlot != null && vanItemSlot.ItemInstance != null && plrItems != null) {
                        addItemToStorage(vanItemSlot, plrItems, false);
                    }
                }


            } else if (lookedAtObject.name == "Trigger") { // Shelf, need more checks if they are looking at the real shelf tho
                LoggerInstance.Msg("[Shelf] Shelf found! Using shelf mode and pushing items");

                GameObject shelf = lookedAtObject.transform.parent.gameObject;
                StorageEntity shelfInv = shelf.GetComponent<StorageEntity>();

                for (var playerSlotIndex = 0; playerSlotIndex < plrItems._size; playerSlotIndex++) {
                    // Skip cash slot (8), 9 would be the credit card incase that becomes a slot for whatever reason
                    if (playerSlotIndex == 8 || playerSlotIndex == 9) { continue; }
                    ItemSlot playerSlot = plrItems[playerSlotIndex];

                    if (playerSlot != null && playerSlot.ItemInstance != null && shelfInv != null) {
                        LoggerInstance.Msg($"[GVPSF] Slot {playerSlotIndex} has {playerSlot?.ItemInstance?.Name}");

                        addItemToStorage(playerSlot, shelfInv?.ItemSlots, filter);
                    }

                }
            }
        }

        public void grabShelf(GameObject lookedAtObject, Il2CppSystem.Collections.Generic.List<ItemSlot> plrItems) {
            if (lookedAtObject.name == "Trigger") { // Shelf, need more checks if they are looking at the real shelf tho
                LoggerInstance.Msg("[GS][Shelf] Shelf found! Using shelf mode and pulling items");

                GameObject shelf = lookedAtObject.transform.parent.gameObject;
                StorageEntity shelfInv = shelf.GetComponent<StorageEntity>();
                Il2CppSystem.Collections.Generic.List<ItemSlot> shelfItems = shelfInv.ItemSlots;


                foreach (var shelfItemSlot in shelfItems) {
                    if (shelfItemSlot != null && shelfItemSlot.ItemInstance != null && plrItems != null) {
                        addItemToStorage(shelfItemSlot, plrItems, false);
                    }
                }
            }
        }
        public override void OnLateUpdate() {
            // Item mover

            bool grabVanPushShelfFilterKey = Input.GetKeyUp(moveFilteredKey);
            bool grabVanPushShelfKey = Input.GetKeyUp(moveKey);
            bool grabShelfKey = Input.GetKeyUp(grabKey);

            if (!(grabVanPushShelfFilterKey || grabVanPushShelfKey || grabShelfKey)) { return; }
            LoggerInstance.Msg("Valid key pressed!");

            PlayerCamera cam = PlayerSingleton<PlayerCamera>.instance;

            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("Vehicle", "Default");

            if (cam.LookRaycast(10f, out hit, mask)) {
                GameObject lookedAtObject = hit.collider.gameObject;

                PlayerInventory plrInv = PlayerSingleton<PlayerInventory>.Instance;

                Il2CppSystem.Collections.Generic.List<ItemSlot> plrItems = plrInv.GetAllInventorySlots();

                LoggerInstance.Msg($"[Raycast] Name is: {lookedAtObject?.gameObject?.name}");
                LoggerInstance.Msg($"[Raycast] Parent name is: {lookedAtObject?.transform?.parent?.gameObject?.name}");
                LoggerInstance.Msg($"[Raycast] Parent Parent name is: {lookedAtObject?.transform?.parent?.parent?.gameObject?.name}");

                if (grabVanPushShelfFilterKey) {
                    grabVanPushShelfFilter(lookedAtObject, plrItems, true);
                } else if (grabVanPushShelfKey) {
                    grabVanPushShelfFilter(lookedAtObject, plrItems, false);
                } else if (grabShelfKey) {
                    grabShelf(lookedAtObject, plrItems);
                    // Grab items from shelf
                }

            }
        }
    }
}