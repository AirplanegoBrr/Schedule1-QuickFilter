using Il2CppScheduleOne.ItemFramework;
using MelonLoader;

namespace Schedule1_QuickFilter {
    public static class Utils {

        /// <summary>
        /// Makes sure the Quality and the Packaging match on both items
        /// </summary>
        public static bool CheckIfSameItem(ItemInstance item1, ItemInstance item2) {

            bool sameQuality = JsonUtils.CheckJSONDataMatches(item1, item2, "Quality");
            if (!sameQuality) return false;

            bool samePacking = JsonUtils.CheckJSONDataMatches(item1, item2, "PackagingID");
            if (!samePacking) return false;

            return true;
        }

        /// <summary>
        /// Checks whether at least one slot in the given storage matches the specified item.
        /// Works by checking both existing items and the player-applied slot filters.
        /// </summary>
        /// <param name="storage">The list of ItemSlot objects to check against.</param>
        /// <param name="item">The ItemSlot containing the ItemInstance to match.</param>
        /// <returns>
        /// True if at least one slot matches the item (either by name or by passing player filters), false otherwise.
        /// </returns>
        public static bool FilterCheck(ItemSlotList storage, ItemSlot item) {
            bool foundFilterItem = false;

            for (var slotItemIndex = 0; slotItemIndex < storage._size; slotItemIndex++) {
                ItemSlot slotItem = storage[slotItemIndex];

                var playerDoesMatch = slotItem.DoesItemMatchPlayerFilters(item.ItemInstance);
                if (playerDoesMatch) {
                    MelonLogger.Msg($"[FSOES] Player Filter does accept item");
                    foundFilterItem = true;
                }

                if (slotItem?.ItemInstance != null && slotItem?.ItemInstance?.Name == item?.ItemInstance?.Name) {
                    foundFilterItem = true;
                }
            }
            return foundFilterItem;
        }
    }
}
