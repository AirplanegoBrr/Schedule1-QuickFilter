# 📦 Schedule1-QuickFilter

### by airplanegobrr


Info video below: (click picture)

[![Info video](https://img.youtube.com/vi/nQxGSbygMoU/0.jpg)](https://youtu.be/nQxGSbygMoU)


GIF example:

![GIF Example](https://github.com/AirplanegoBrr/Schedule1-QuickFilter/blob/master/assests/using.gif?raw=true)

---

## 📝 Overview
**Schedule1-QuickFilter** is a quality-of-life mod for *Schedule I* that streamlines your item management workflow.

You can quickly transfer items between your inventory and storage units such as shelves and delivery vans, with smart filtering, stacking, and automatic quantity handling.

Say goodbye to tedious item dragging—just look, press, and go.

Perfect for players who want smoother gameplay, faster item organization, and fewer headaches during pickups and deliveries.

---

## 🌍 **This Mod is Fully Open Source!**

Want to understand how it works? Make your own tweaks? Contribute improvements?

### 👉👉👉 [Check it out on GitHub](https://github.com/AirplanegoBrr/Schedule1-QuickFilter) 👈👈👈


> Built by a player, **for players** — no *hidden code* 🫣, no *obfuscation* 🥸, just clean 🧹 and transparent 🪟 modding. **Unlike other mods!**

---

## 💬 Discord

Want help? Want to chat? Want to learn about modding?

Join my [Discord](M6A2eK7)!

(If there is none, I'd like to make an Unofficial Schedule 1 Discord for everyone!)

---

## 💡 Features


### ⭐ Works on anything!
- Works on Cars, Shelfs, Mixer, Pallets, Everything! (Let me know if anything is missing!)

### 🔄 Quick Filtered Item Transfer (**H** Key) [MovePushFilterd]
- Transfers items **to shelves** when looking at a shelf, only adding items that match whats already in the self!
- Transfers items **from vans** when looking at a delivery vehicle.

### 🔄 Quick Item Transfer (**Y** Key) [MovePush]
- Transfers items **to shelves** when looking at a shelf, adding ALL items, no filtering!
- Transfers items **from vans** when looking at a delivery vehicle.

### ➡️ Quick pull Item Transfer (**B** Key) [GrabShelf]
- Quickly transfers all items from shelf into your invetory!

### ⌨️ Changeable keybinds using **MelonPreferences**!
- Easily Edit your key binds using a CFG file or [Ingame menus](https://www.nexusmods.com/schedule1/mods/397)
- File at: **Schedule I/UserData/MelonPreferences.cfg** Look for **[QuickMoveFilter]**

---

## 🎮 Controls

| Key | Function |
|-----|----------|
| **H** | Transfers items between player and shelf/van only putting items that are already in the shelf |
| **Y** | Transfers items between player and shelf/van pulls ALL items regardless of type. |
| **B** | Transfers items between shelf and player |

---

## 🔧 Installation

1. Make sure you have **MelonLoader** installed for *Schedule I*.
2. Drop the mod **.dll** into your **Mods**
3. Launch the game and look for **[Schedule1-QuickFilter] Initialized** in the MelonLoader console to confirm it's working.

---

## 📢 Planned Features

- 🤔 More support for other invs (Cars, Pallets, ETC)

---

## 💬 Support & Feedback

Have a bug to report? Feature idea? Want to contribute?
- 📬 Message me on **NexusMods** or Discord **@airplanegobrr**
- 🛠 Or hop on the GitHub repo and open an issue or pull request:

### 👉 [https://github.com/AirplanegoBrr/Schedule1-QuickFilter](https://github.com/AirplanegoBrr/Schedule1-QuickFilter) 👈

---

# 🤓Nerd info

Want to know more about the under lying functions? This is the place!


---

## 🧠 Intelligent Slot Matching

Now with better, wider support for more invs! Player or Enties!

### **findSameOrEmptySlot(storage, item)**
- Searches a storage container’s slots for:
  - Empty slots, or
  - Slots with the **same item** that aren't at stack limit.
- Returns a list of valid slots for insertion.

### **addItemToStorage(fromSlot, toStorage, filter)**
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

## 🙏 Credits

- Developed by **airplanegobrr**
- Built using **MelonLoader** and **Il2CppScheduleOne** internals
- Special thanks to *@Piximental* and *@Prowiler* for there ideas and help.

---

