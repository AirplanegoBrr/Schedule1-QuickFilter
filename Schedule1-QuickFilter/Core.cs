using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Storage;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Schedule1_QuickFilter.Core), "Schedule1-QuickFilter", "1.0.0", "airplanegobrr", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace Schedule1_QuickFilter
{
    public class Core : MelonMod {
        public override void OnInitializeMelon() {
            LoggerInstance.Msg("Initialized.");
        }


        Il2CppSystem.Collections.Generic.List<ItemSlot> findSameOrEmptySlot(StorageEntity storage, ItemInstance item) {
            Il2CppSystem.Collections.Generic.List<ItemSlot> validSlots = new();

            LoggerInstance.Msg($"[FSOES] Storage has {storage.ItemSlots._size} slots");

            for (int i = 0; i < storage.ItemSlots._size; i++) {

                ItemSlot storageSlot = storage.ItemSlots[i];

                LoggerInstance.Msg($"[FSOES] Slot {i} has {storageSlot?.Quantity} of {storageSlot?.ItemInstance?.Name}");

                if (storageSlot.ItemInstance == null || (storageSlot.ItemInstance.Name == item.Name && storageSlot.ItemInstance.Quantity != storageSlot.ItemInstance.StackLimit)) {
                    LoggerInstance.Msg($"[FSOES] Adding slot {i} as it matches requirements.");
                    validSlots.Add(storageSlot);
                }
            }

            LoggerInstance.Msg($"[FSOES] Got {validSlots.Count} valid slots");

            return validSlots;
        }

        void addItemToStorage(ItemSlot from, StorageEntity to, bool filter) {
            if (from == null) return;
            if (to == null) return;

            // Check if the storage container has the item in it
            if (filter) {
                bool passed = false;
                foreach (var item in to.GetAllItems()) {
                    if (item.Name == from.ItemInstance?.Name) {
                        passed = true;
                    }
                }


                if (!passed) {
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

        public override void OnLateUpdate() {
            // Item mover
            if (Input.GetKeyUp(KeyCode.H)) {
                PlayerCamera cam = PlayerSingleton<PlayerCamera>.instance;

                RaycastHit hit;
                LayerMask mask = LayerMask.GetMask("Vehicle", "Default"); // or whichever layers you want to include

                if (cam.LookRaycast(10f, out hit, mask)) {
                    GameObject lookedAtObject = hit.collider.gameObject;

                    PlayerInventory plrInv = PlayerSingleton<PlayerInventory>.Instance;
                    StorageEntity plrStorage = plrInv.GetComponent<StorageEntity>();

                    Il2CppSystem.Collections.Generic.List<ItemSlot> plrItems = plrInv.GetAllInventorySlots();

                    LoggerInstance.Msg("[Raycast] Name is: " + lookedAtObject?.gameObject?.name);
                    LoggerInstance.Msg("[Raycast] Parent name is: " + lookedAtObject?.transform?.parent?.gameObject?.name);
                    LoggerInstance.Msg("[Raycast] Parent Parent name is: " + lookedAtObject?.transform?.parent?.parent?.gameObject?.name);

                    if (lookedAtObject.transform.parent.parent.gameObject.name == "DeliveryVehicles") { // Van
                        LoggerInstance.Msg("[Van] Van found! Using Van mode and pulling items");
                        GameObject van = lookedAtObject.transform.parent.gameObject;
                        StorageEntity vanInv = van.GetComponent<StorageEntity>();
                        Il2CppSystem.Collections.Generic.List<ItemSlot> vanItems = vanInv.ItemSlots;

                        // LoggerInstance.Msg($"[Van -> Player] Moving Item: { item.Name} | Quantity: {quantity}");

                        foreach (var vanItemSlot in vanItems) {
                            if (vanItemSlot?.ItemInstance == null) { continue; }

                            foreach (var playerItemSlot in plrItems) {
                                // TODO: Make this better as it wont fill old items
                                if (playerItemSlot?.ItemInstance != null) { continue; }

                                LoggerInstance.Msg("[Van] Found empty player slot!");
                                var copiedItem = vanItemSlot.ItemInstance?.GetCopy();
                                LoggerInstance.Msg("[Van] Got copy!");


                                if (copiedItem == null) {
                                    LoggerInstance.Msg("[Van] GetCopy() returned null — skipping this item.");
                                    break;
                                }

                                LoggerInstance.Msg($"[Van] [Van -> Player] Moving Item: {vanItemSlot.ItemInstance.Name} | Quantity: {vanItemSlot.Quantity}");

                                copiedItem.SetQuantity(vanItemSlot.Quantity);
                                playerItemSlot.SetStoredItem(copiedItem);
                                vanItemSlot.ItemInstance.RequestClearSlot();
                                break;
                            }
                        }

                    } else if (lookedAtObject.name == "Trigger") { // Shelf
                        LoggerInstance.Msg("[Shelf] Shelf found! Using shelf mode and pushing items");

                        GameObject shelf = lookedAtObject.transform.parent.gameObject;
                        StorageEntity shelfInv = shelf.GetComponent<StorageEntity>();

                        foreach (var playerSlot in plrItems) {

                            if (playerSlot != null && playerSlot.ItemInstance != null && shelfInv != null) {
                                addItemToStorage(playerSlot, shelfInv, true);
                            }

                        }
                    }
                }

            }
        }
    }
}