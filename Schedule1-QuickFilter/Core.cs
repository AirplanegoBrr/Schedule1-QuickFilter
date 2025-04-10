using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Storage;
using MelonLoader;
using UnityEngine;
using System.Text.Json;
using Il2CppScheduleOne.ObjectScripts;
using System.Linq.Expressions;

[assembly: MelonInfo(typeof(Schedule1_QuickFilter.Core), "Schedule1-QuickFilter", "1.3.0", "airplanegobrr", null)]
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

            // This will make it so some slots are locked out from being used
            int maxSlotsToUse = storageSlots.Count;

            // Mixer/Packager
            if (storageSlots.Count == 3) {
                maxSlotsToUse = 2;
            } else if (storageSlots.Count == 2) {
                // Dryer
                maxSlotsToUse = 1;
            }

            for (int i = 0; i < storageSlots._size; i++) {
                if (i >= maxSlotsToUse) continue;

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

        bool checkIfSameItem(ItemInstance item1, ItemInstance item2) {
            // Get json
            string rawData1 = item1.GetItemData().GetJson(false);
            string rawData2 = item2.GetItemData().GetJson(false);

            // Get C# json data
            var jsonDoc1 = JsonDocument.Parse(rawData1);
            var jsonDoc2 = JsonDocument.Parse(rawData2);

            // Check "Quality"
            bool hasQuality1 = jsonDoc1.RootElement.TryGetProperty("Quality", out var qualityProp1);
            bool hasQuality2 = jsonDoc2.RootElement.TryGetProperty("Quality", out var qualityProp2);

            if (hasQuality1 != hasQuality2) {
                return false;
            }

            if (hasQuality1 && qualityProp1.GetString().ToLower() != qualityProp2.GetString().ToLower()) {
                return false;  // Quality mismatch
            }

            // Check "PackagingID"
            bool hasPacking1 = jsonDoc1.RootElement.TryGetProperty("PackagingID", out var packingProp1);
            bool hasPacking2 = jsonDoc2.RootElement.TryGetProperty("PackagingID", out var packingProp2);

            if (hasPacking1 != hasPacking2) {
                return false;
            }

            if (hasPacking1 && packingProp1.GetString().ToLower() != packingProp2.GetString().ToLower()) {
                return false;  // PackagingID mismatch
            }

            // If no mismatches were found, return true
            return true;
        }

        bool addItemToStorage(ItemSlot from, Il2CppSystem.Collections.Generic.List<ItemSlot> to, bool filter) {
            if (from == null || to == null) return true;

            // Check if the storage container has the item in it
            if (filter) {
                bool foundItem = filterCheck(to, from);

                if (!foundItem) {
                    LoggerInstance.Msg($"[AITS] Item {from.ItemInstance?.Name} not allowed by filter.");
                    return true; // Storage container doesn't have this item in it
                }
            }

            Il2CppSystem.Collections.Generic.List<ItemSlot> validSlots = findSameOrEmptySlot(to, from.ItemInstance);
           
            LoggerInstance.Msg($"[AITS] Found {validSlots.Count} valid slots");

            for (int slotIndex = 0; slotIndex < validSlots.Count; slotIndex++) {
                ItemSlot slot = validSlots[slotIndex];
                
                try { 
                    LoggerInstance.Msg($"[AITS] {slotIndex} is slot {slot?.SlotIndex} in the storage container");
                } catch (Exception) {}
                
                // if (slotIndex == maxSlotsToCheck) { return false; }

                if (slot.ItemInstance == null && from.ItemInstance != null) {
                    LoggerInstance.Msg($"[AITS] Moving {from?.ItemInstance?.Quantity} of {from?.ItemInstance?.Name} to storage slot {slotIndex}");
                    slot.InsertItem(from.ItemInstance);
                    from.ClearItemInstanceRequested();
                    return true; // We are done.
                } else if (slot.ItemInstance?.Name == from.ItemInstance?.Name) {// Same item
                    // Find space left
                    int spaceInSlot = slot.ItemInstance.StackLimit - slot.Quantity;

                    bool isSame = checkIfSameItem(slot.ItemInstance, from.ItemInstance);
                    if (!isSame) {
                        continue;
                    }

                    // Get how many items we can fit in the slot
                    int ableToFit = Math.Min(from.Quantity, spaceInSlot);

                    if (ableToFit <= 0) {
                        LoggerInstance.Msg($"[AITS] Can't fit item! (PLEASE SCREENSHOT CONSOLE AS THIS SHOULDN'T HAPPEN!)");
                        continue;
                    }

                    // Get how many cound't fit
                    int leftOver = from.Quantity - ableToFit;

                    LoggerInstance.Msg($"[AITS] [Move info]\nSlotIndex: {slotIndex}\nLimit: {slot.ItemInstance.StackLimit}\nSlotCount: {slot.ItemInstance.Quantity}\nFromCount: {from.Quantity}\nSpaceLeft: {spaceInSlot}\nFit: {ableToFit}\nLeftover: {leftOver}");

                    LoggerInstance.Msg($"[AITS] Adding {ableToFit} {slot.ItemInstance.Name}");

                    // Add the items to the slot
                    slot.ItemInstance.ChangeQuantity(ableToFit);

                    from.ItemInstance.SetQuantity(leftOver);

                    if (from.Quantity == 0) {
                        from.ClearItemInstanceRequested();
                        return true;
                    }
                }
            }
            return true;
        }
        void grabVanPushShelfFilter(String storageName, String storageParentName, Il2CppSystem.Collections.Generic.List<ItemSlot> ItemSlots, Il2CppSystem.Collections.Generic.List<ItemSlot> plrItems, bool filter) {
            if (storageParentName == "DeliveryVehicles") { // Van
                LoggerInstance.Msg("[Van] Van found! Using Van mode and pulling items");

                // LoggerInstance.Msg($"[Van -> Player] Moving Item: { item.Name} | Quantity: {quantity}");

                foreach (var vanItemSlot in ItemSlots) {
                    if (vanItemSlot != null && vanItemSlot.ItemInstance != null && plrItems != null) {
                        bool keepAdding = addItemToStorage(vanItemSlot, plrItems, false);
                        if (!keepAdding) return;
                    }
                }


            } else if (storageName.Contains("StorageRack") || storageName.Contains("DryingRack") || storageName.Contains("PackagingStation") || storageName.Contains("MixingStation")) { // Shelf, need more checks if they are looking at the real shelf tho
                LoggerInstance.Msg("[Container] Container found! Using containter mode and pushing items");

                for (var playerSlotIndex = 0; playerSlotIndex < plrItems._size; playerSlotIndex++) {
                    // Skip cash slot (8), 9 would be the credit card incase that becomes a slot for whatever reason
                    if (playerSlotIndex == 8 || playerSlotIndex == 9) { continue; }
                    ItemSlot playerSlot = plrItems[playerSlotIndex];

                    if (playerSlot != null && playerSlot.ItemInstance != null && ItemSlots != null) {
                        LoggerInstance.Msg($"[GVPSF] Slot {playerSlotIndex} has {playerSlot?.ItemInstance?.Name}");

                        bool keepAdding = addItemToStorage(playerSlot, ItemSlots, filter);
                        if (!keepAdding) return;
                    }

                }
            }
        }

        public void grabItems(Il2CppSystem.Collections.Generic.List<ItemSlot> itemSlots, Il2CppSystem.Collections.Generic.List<ItemSlot> plrItems) {
            if (itemSlots == null) return;
            LoggerInstance.Msg("[GS] Storage found! Pulling items");

            foreach (var shelfItemSlot in itemSlots) {
                if (shelfItemSlot != null && shelfItemSlot.ItemInstance != null && plrItems != null) {
                    bool keepAdding = addItemToStorage(shelfItemSlot, plrItems, false);
                    if (!keepAdding) return;
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
                Il2CppSystem.Collections.Generic.List<ItemSlot> itemSlots = null;


                if (lookedAtObject == null) {
                    LoggerInstance.Msg("[Raycast] lookedAtObject is null");
                    return;
                }

                Transform currentTransform = lookedAtObject.transform;
                int steps = 0;

                // If the transform isn't null and we havm't gotten past 3 steps
                while (currentTransform != null && steps <= 3) {

                    steps++;

                    LoggerInstance.Msg($"[Raycast] Checking: {currentTransform?.gameObject?.name}");

                    if (currentTransform == null) {
                        LoggerInstance.Msg("[Raycast] currentTransform is null");
                        break;
                    }

                    var storageEntity = currentTransform?.GetComponent<StorageEntity>();

                    if (storageEntity == null) {
                        var mixing = currentTransform?.GetComponent<MixingStation>();
                        if (mixing != null) {
                            LoggerInstance.Msg("Found mixer!");
                            itemSlots = mixing.ItemSlots;
                            break;
                            // mixing.ItemSlots
                        }

                        var packing = currentTransform?.GetComponent<PackagingStation>();
                        if (packing != null) {
                            LoggerInstance.Msg("Found packer!");
                            itemSlots = packing.ItemSlots;
                            break;
                            // mixing.ItemSlots
                        }

                        var rack = currentTransform?.GetComponent<DryingRack>();
                        if (rack != null) {
                            LoggerInstance.Msg("Found Drying Rack!");
                            itemSlots = rack.ItemSlots;
                            break;
                            // mixing.ItemSlots
                        }
                    }

                    if (storageEntity != null) {
                        itemSlots = storageEntity?.ItemSlots;
                        break; // Stop as soon as we find the first valid StorageEntity
                    }

                    LoggerInstance.Msg($"[Raycast] {currentTransform?.gameObject?.name} didn't have a valid Storage type");

                    // Move to the parent of the current transform
                    currentTransform = currentTransform?.parent;
                }

                if (currentTransform == null || itemSlots == null) {
                    LoggerInstance.Msg($"[Raycast] No valid storage type was found.");

                    return;
                }


                PlayerInventory plrInv = PlayerSingleton<PlayerInventory>.Instance;
                Il2CppSystem.Collections.Generic.List<ItemSlot> plrItems = plrInv.GetAllInventorySlots();

                if (grabVanPushShelfFilterKey) {
                    grabVanPushShelfFilter(currentTransform.gameObject.name, currentTransform.parent.gameObject.name, itemSlots, plrItems, true);
                } else if (grabVanPushShelfKey) {
                    grabVanPushShelfFilter(currentTransform.gameObject.name, currentTransform.parent.gameObject.name, itemSlots, plrItems, false);
                } else if (grabShelfKey) {
                    grabItems(itemSlots, plrItems);
                    // Grab items from shelf
                }

            }
        }
    }
}