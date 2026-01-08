using System.Text.Json;
using Il2CppScheduleOne.ItemFramework;

namespace Schedule1_QuickFilter {
    public static class JsonUtils {

        public static bool HasDataKey(ItemInstance item1, string key) {
            string rawData1 = item1?.GetItemData()?.GetJson(false);

            if (string.IsNullOrEmpty(rawData1)) return false;

            using var jsonDoc1 = JsonDocument.Parse(rawData1);

            if (!jsonDoc1.RootElement.TryGetProperty(key, out var prop1Check))
                return false;

            if (prop1Check.ValueKind != JsonValueKind.String)
                return false;

            return !string.IsNullOrEmpty(prop1Check.GetString());
        }

        public static bool CheckJSONDataMatches(ItemInstance item1, ItemInstance item2, string key) {
            // Get json
            string rawData1 = item1.GetItemData().GetJson(false);
            string rawData2 = item2.GetItemData().GetJson(false);

            // Get C# json data
            var jsonDoc1 = JsonDocument.Parse(rawData1);
            var jsonDoc2 = JsonDocument.Parse(rawData2);

            bool has1Check = jsonDoc1.RootElement.TryGetProperty(key, out var prop1Check);
            bool has2Check = jsonDoc1.RootElement.TryGetProperty(key, out var prop2Check);

            // One has, other doesn't
            if (has1Check != has2Check) {
                return false;
            }

            if (has1Check && has2Check && prop1Check.GetString().ToLower() != prop2Check.GetString().ToLower()) {
                return false;  // Mismatch
            }

            return true;
        }
    }
}
