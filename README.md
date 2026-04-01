# 🔫 Customizable Extended Magazines

A fully configurable extended magazine system for **Rust (Oxide/uMod)** that allows you to control magazine sizes, spawn chances, and crate distribution.

---

## ✨ Features

- 🔧 Fully configurable magazine bonuses (15%, 30%, 50%, 100% etc.)
- 📦 Control **which crates** magazines spawn in
- 🎯 Adjustable **spawn chances per magazine**
- ⚙️ Easy JSON config editing
- 🧩 Supports custom crate types
- 🔁 Auto-generates default config if missing

---

## 📦 Installation

1. Download the plugin:
   ```
   CustomizableExtendedMagazines.cs
   ```

2. Place into:
   ```
   /oxide/plugins/
   ```

3. Reload plugin:
   ```
   oxide.reload CustomizableExtendedMagazines
   ```

4. Config will generate at:
   ```
   /oxide/config/CustomizableExtendedMagazines.json
   ```

---

## ⚙️ Configuration

Example config:

```json
"mags": {
  "2892143123": {
    "Name": "Extended Magazine 15%",
    "Multiplier": 1.5,
    "Capacity Bonus": 15,
    "Containers": ["crate_basic", "crate_normal"]
  },
  "2892142979": {
    "Name": "Extended Magazine 30%",
    "Multiplier": 1.75,
    "Capacity Bonus": 30,
    "Containers": ["crate_basic", "crate_normal"]
  }
}
```

---

### 🧠 Field Breakdown

| Field | Description |
|------|------------|
| `Name` | Display name of the magazine |
| `Multiplier` | Weapon capacity multiplier |
| `Capacity Bonus` | % increase in ammo |
| `Containers` | Crates the item can spawn in |

---

## 📦 Supported Crates

Common crate types:
- `crate_basic`
- `crate_normal`
- `crate_normal_2`
- `crate_elite`
- `crate_tools`
- `crate_military`

You can add custom containers depending on your server mods.

---

## 🎮 How It Works

- Magazines are injected into loot tables
- Spawn based on configured containers
- Apply capacity bonuses when used
- Fully server-side (no client mods required)

---

## ⚠️ Notes

- If your config doesn’t match the code defaults, **only existing entries will load**
- New magazines must exist in **both config and code defaults**
- Restart or reload plugin after config edits

---

## 🛠️ Troubleshooting

### Magazines not spawning?
- Check container names match exactly
- Ensure config is valid JSON
- Verify plugin loaded successfully:
  ```
  oxide.reload CustomizableExtendedMagazines
  ```

### Config not updating?
- Delete config file and reload plugin to regenerate

---

## 🚀 Future Ideas

- Weighted spawn chances per crate
- Tier-based magazine drops
- UI display for bonus stats
- Integration with loot table plugins

---

## 📌 Credits

- Plugin: Custom Extended Magazines
- Maintained by: https://github.com/milestorme

---

## 💬 Support

If you run into issues or want new features:
- Open a GitHub issue
- Or modify the config/code to suit your server

---

## ⭐ Enjoy

Make your loot system more dynamic and rewarding.
