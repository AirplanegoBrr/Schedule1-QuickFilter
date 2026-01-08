using Il2CppScheduleOne.ItemFramework;
using MelonLoader;

namespace Schedule1_QuickFilter {
    public static class StorageUtils {
        public static ItemSlotList FindSameOrEmptySlot(ItemSlotList storageSlots, ItemInstance item) { // CHECK OLD CODE TO SEE IF THERE WAS A CHECK TO SEE IF "ITEM" EXISTED BEFORE CALLING THIS FUNCITON
            if (storageSlots == null)
                throw new ArgumentNullException(nameof(storageSlots));

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            ItemSlotList validSlots = new();
            ItemSlotList validEmptySlots = new();

            // TODO: Break into helper functions :P


            // This will make it so some slots are locked out from being used
            int maxSlotsToUse = storageSlots.Count;
            bool checkHasQuality = false;

            // Mixer/Packager
            if (storageSlots.Count == 3) {
                maxSlotsToUse = 2;
                checkHasQuality = true;
            }
            else if (storageSlots.Count == 2) {
                // Dryer
                maxSlotsToUse = 1;
                checkHasQuality = true;
            }

            MelonLogger.Msg($"[FSOES] Storage has {storageSlots._size} slots, filling {maxSlotsToUse}, Quality check: {checkHasQuality}");

            for (int i = 0; i < storageSlots._size; i++) {
                if (i >= maxSlotsToUse) continue;

                ItemSlot storageSlot = storageSlots[i];

                bool hardFilterPass = storageSlot.DoesItemMatchHardFilters(item);

                if (!hardFilterPass) {
                    MelonLogger.Msg($"[FSOES] Hard Filter does NOT accept item");
                    continue;
                }

                MelonLogger.Msg($"[FSOES] Slot {i} has {storageSlot?.Quantity} of {storageSlot?.ItemInstance?.Name}");

                if (storageSlot?.ItemInstance == null) {
                    // Check if we are doing a quality check AND check if this is the first slot
                    if (checkHasQuality && i == 0) {
                        MelonLogger.Msg("Running check!");
                        // Test if the item has Quality data
                        bool hasKey = JsonUtils.HasDataKey(item, "Quality");
                        bool isPackaged = JsonUtils.HasDataKey(item, "PackagingID");

                        // Check if we have a valid Quality AND that its NOT packaged.
                        if (hasKey && !isPackaged) {
                            MelonLogger.Msg("Check passed!");

                            validSlots.Add(storageSlot);
                            continue;
                        }
                        else {
                            MelonLogger.Msg("Check didnt pass!");

                            continue;
                        }
                    }
                    validEmptySlots.Add(storageSlot);
                    continue;
                }

                if (storageSlot.ItemInstance.Name == item.Name && storageSlot.ItemInstance.Quantity != storageSlot.ItemInstance.StackLimit) {
                    MelonLogger.Msg($"[FSOES] Adding slot {i} as it matches requirements.");

                    // Check if we are doing a quality check AND check if this is the first slot
                    if (checkHasQuality && i == 0) {
                        MelonLogger.Msg("Checking for quality");
                        // Test if the item has Quality data
                        bool hasKey = JsonUtils.HasDataKey(item, "Quality");
                        bool isPackaged = JsonUtils.HasDataKey(item, "PackagingID");

                        MelonLogger.Msg("Got values");


                        bool areSameQuality = JsonUtils.CheckJSONDataMatches(item, storageSlot.ItemInstance, "Quality");

                        // Check if we have a valid Quality AND that its NOT packaged.
                        if (hasKey && areSameQuality && !isPackaged) {
                            validSlots.Add(storageSlot);
                            continue;
                        }
                        else {
                            continue;
                        }
                    }

                    validSlots.Add(storageSlot);
                    continue;

                }
            }

            foreach (var slot in validEmptySlots) {
                validSlots.Add(slot);
            }

            MelonLogger.Msg($"[FSOES] Got {validSlots.Count} valid slots");

            return validSlots;
        }

        public static bool AddItemToStorage(ItemSlot from, ItemSlotList to, bool filter) {
            if (from == null || to == null) return true;

            // Check if the storage container has the item in it
            if (filter) {
                bool foundItem = Utils.FilterCheck(to, from);

                if (!foundItem) {
                    MelonLogger.Msg($"[AITS] Item {from.ItemInstance?.Name} not allowed by filter.");
                    return true; // Storage container doesn't have this item in it
                }
            }

            MelonLogger.Msg("[AITS] Finding valid slots");

            ItemSlotList validSlots = FindSameOrEmptySlot(to, from.ItemInstance);

            MelonLogger.Msg($"[AITS] Found {validSlots.Count} valid slots");

            for (int slotIndex = 0; slotIndex < validSlots.Count; slotIndex++) {
                ItemSlot slot = validSlots[slotIndex];

                try {
                    MelonLogger.Msg($"[AITS] {slotIndex} is slot {slot?.SlotIndex} in the storage container");
                }
                catch (Exception) { }

                // if (slotIndex == maxSlotsToCheck) { return false; }

                if (slot.ItemInstance == null && from.ItemInstance != null) {
                    MelonLogger.Msg($"[AITS] Moving {from?.ItemInstance?.Quantity} of {from?.ItemInstance?.Name} to storage slot {slotIndex}");
                    slot.InsertItem(from.ItemInstance);
                    from.ClearItemInstanceRequested();
                    return true; // We are done.
                }
                else if (slot.ItemInstance?.Name == from.ItemInstance?.Name) {// Same item
                    // Find space left
                    int spaceInSlot = slot.ItemInstance.StackLimit - slot.Quantity;

                    bool isSame = Utils.CheckIfSameItem(slot.ItemInstance, from.ItemInstance);
                    if (!isSame) {
                        continue;
                    }

                    // Get how many items we can fit in the slot
                    int ableToFit = Math.Min(from.Quantity, spaceInSlot);

                    if (ableToFit <= 0) {
                        MelonLogger.Msg($"[AITS] Can't fit item! (PLEASE SCREENSHOT CONSOLE AS THIS SHOULDN'T HAPPEN!)");
                        continue;
                    }

                    // Get how many cound't fit
                    int leftOver = from.Quantity - ableToFit;

                    MelonLogger.Msg($"[AITS] [Move info]\nSlotIndex: {slotIndex}\nLimit: {slot.ItemInstance.StackLimit}\nSlotCount: {slot.ItemInstance.Quantity}\nFromCount: {from.Quantity}\nSpaceLeft: {spaceInSlot}\nFit: {ableToFit}\nLeftover: {leftOver}");

                    MelonLogger.Msg($"[AITS] Adding {ableToFit} {slot.ItemInstance.Name}");

                    var canStack = slot.ItemInstance.CanStackWith(from.ItemInstance);
                    var canStack2 = slot.ItemInstance.CanStackWith(from.ItemInstance, false);

                    MelonLogger.Msg($"[AITS-Debug] CanStackWith: {canStack} {canStack2}");


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

        public static void VanMode(ItemSlotList ItemSlots, ItemSlotList plrItems) {
            MelonLogger.Msg("[Van] Van found! Using Van mode and pulling items");

            foreach (var vanItemSlot in ItemSlots) {
                if (vanItemSlot != null && vanItemSlot.ItemInstance != null && plrItems != null) {
                    bool keepAdding = AddItemToStorage(vanItemSlot, plrItems, false);
                    if (!keepAdding) return;
                }
            }
        }

        public static void ContainerMode(ItemSlotList itemSlots, ItemSlotList plrItems, bool filter) {
            MelonLogger.Msg("[ContainerMode] Container found! Using containter mode and pushing items");

            for (var playerSlotIndex = 0; playerSlotIndex < plrItems._size; playerSlotIndex++) {
                // Skip cash slot (8), 9 would be the credit card incase that becomes a slot for whatever reason
                if (playerSlotIndex == 8 || playerSlotIndex == 9) { continue; }
                ItemSlot playerSlot = plrItems[playerSlotIndex];

                if (playerSlot == null && playerSlot.ItemInstance == null && itemSlots == null) continue;
                MelonLogger.Msg($"[GVPSF] Slot {playerSlotIndex} has {playerSlot?.ItemInstance?.Name}");

                bool keepAdding = AddItemToStorage(playerSlot, itemSlots, filter);
                if (!keepAdding) return;
            }
        }

        public static void GrabVanPushShelfFilter(string storageName, string storageParentName, ItemSlotList itemSlots, ItemSlotList plrItems, bool filter) {
            if (storageParentName == "DeliveryVehicles") { // Van
                VanMode(itemSlots, plrItems);
            }
            else if (storageName.Contains("StorageRack") || storageName.Contains("DryingRack") || storageName.Contains("PackagingStation") || storageName.Contains("MixingStation")) {
                ContainerMode(itemSlots, plrItems, filter);
            }
        }

        public static void GrabItems(ItemSlotList itemSlots, ItemSlotList plrItems) {
            if (itemSlots == null) return;
            MelonLogger.Msg("[GS] Storage found! Pulling items");

            foreach (var shelfItemSlot in itemSlots) {
                if (shelfItemSlot != null && shelfItemSlot.ItemInstance != null && plrItems != null) {
                    bool keepAdding = AddItemToStorage(shelfItemSlot, plrItems, false);
                    if (!keepAdding) return;
                }
            }

        }

    }
}
