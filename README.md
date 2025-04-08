# 📦 Schedule1-QuickFilter
### by airplanegobrr

---

## 📝 Overview
**Schedule1-QuickFilter** is a quality-of-life mod for *Schedule I* that streamlines your item management workflow. With a single key press (`H`), you can quickly transfer items between your inventory and storage units such as shelves and delivery vans, with smart filtering, stacking, and automatic quantity handling. Say goodbye to tedious item dragging—just look, press, and go.

Perfect for players who want smoother gameplay, faster item organization, and fewer headaches during pickups and deliveries.

---

## 🌍 **This Mod is Fully Open Source!**

Want to understand how it works? Make your own tweaks? Contribute improvements?

### 👉👉👉 [Check it out on GitHub](https://github.com/AirplanegoBrr/Schedule1-QuickFilter) 👈👈👈

> Built by a player, **for players** — no hidden code, no obfuscation, just clean and transparent modding. Unlike other mods!

---

## 💡 Features

### 🔄 Quick Item Transfer (`H` Key)
- Transfers items **to shelves** when looking at a shelf.
- Transfers items **from vans** when looking at a delivery vehicle.
- Activates only when you press `H`, so it's fully manual and non-intrusive.

---

### 📥 Shelf Mode (Push to Shelf)
- Triggered when looking at a **storage shelf**.
- Scans your player inventory for items that:
  - Match items already present in the shelf.
  - Have empty or stackable slots available in the shelf.
- Automatically moves items from your inventory into matching shelf slots.
- Items that don’t match existing shelf contents are **ignored** to prevent storage clutter.

---

### 🚚 Van Mode (Pull from Van)
- Triggered when looking at a **delivery van**.
- Transfers items **from the van’s inventory to your player inventory**.
- Automatically:
  - Detects empty player slots.
  - Copies the item from the van.
  - Sets the correct quantity.
  - Clears the slot in the van.
- Stops at the first available player slot—won’t stack for now (by design).

---

## 🧠 Intelligent Slot Matching

### `findSameOrEmptySlot(storage, item)`
- Searches a storage container’s slots for:
  - Empty slots, or
  - Slots with the **same item** that aren't at stack limit.
- Returns a list of valid slots for insertion.

### `addItemToStorage(fromSlot, toStorage, filter)`
- Handles the actual transfer logic.
- Uses filtering:
  - If enabled, only allows transfer of items already present in the destination.
- Automatically stacks into existing items or inserts into empty slots.
- Handles partial transfers when slots can't hold the full quantity.

---

## 🛠️ Debug Logging
The mod includes **extensive logging** for debugging and transparency. You’ll see console messages about:
- What you're looking at
- Slot contents and transfer status
- Filtering and quantity math
- Leftovers, errors, and warnings

---

## 🎮 Controls

| Key | Function |
|-----|----------|
| `H` | Transfers items between player and shelf/van based on what you're looking at |

---

## 🔧 Installation

1. Make sure you have **MelonLoader** installed for *Schedule I*.
2. Drop the mod `.dll` into your `Mods`
3. Launch the game and look for `[Schedule1-QuickFilter] Initialized` in the MelonLoader console to confirm it's working.

---

## 📢 Planned Features

- 💼 Configurable keybind

---

## 🙏 Credits

- Developed by **airplanegobrr**
- Built using **MelonLoader** and `Il2CppScheduleOne` internals
- Special thanks to *@Piximental* for his insight of how to mod the game.

---

## 💬 Support & Feedback

Have a bug to report? Feature idea? Want to contribute?  
> 📬 Message me on NexusMods  
> 🛠 Or hop on the GitHub repo and open an issue or pull request:

### 👉 [https://github.com/AirplanegoBrr/Schedule1-QuickFilter](https://github.com/AirplanegoBrr/Schedule1-QuickFilter)

---

