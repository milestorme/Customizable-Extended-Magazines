# 🔫 Customizable Extended Magazines

A fully configurable extended magazine system for **Rust (Oxide/uMod)** that allows you to control magazine bonuses, spawn chances, and crate distribution.

---

## ✨ Features

* 🔧 Configure magazine bonuses as **extra % over vanilla extended mags**
* 📦 Control **which crates** magazines spawn in
* 🎯 Adjustable **spawn chances per magazine**
* ⚙️ Clean, modern JSON config
* 🧩 Supports custom crate types
* 🔁 Automatic config migration from older versions

---

## 📦 Installation

1. Download the plugin:

   ```
   CustomizableMagazines.cs
   ```

2. Place into:

   ```
   /oxide/plugins/
   ```

3. Reload plugin:

   ```
   oxide.reload CustomizableMagazines
   ```

4. Config will generate at:

   ```
   /oxide/config/CustomizableMagazines.json
   ```

---

## ⚙️ Configuration

### Example config

```json
"Magazine settings": {
  "2892143123": {
    "Magazine Display Name": "Extended Magazine 15%",
    "Extra Capacity Over Vanilla Extended Mag (0.15 = +15%)": 0.15,
    "Can Spawn In LootContainer types": [
      "crate_basic",
      "crate_tools",
      "crate_normal",
      "crate_normal_2"
    ],
    "LootContainer Spawn Chance 1-100": 50.0
  }
}
```

---

## 🧠 How Capacity Works

Rust’s **vanilla extended magazine already provides ~+25% capacity**.

This plugin adds **extra capacity on top of that**.

### Formula

```
Final Capacity = Base Weapon Capacity × (1.25 + Config Value)
```

---

### 📊 Examples

| Config Value | Label | Final Total     |
| ------------ | ----- | --------------- |
| `0.15`       | 15%   | **+40% total**  |
| `0.30`       | 30%   | **+55% total**  |
| `0.50`       | 50%   | **+75% total**  |
| `1.00`       | 100%  | **+125% total** |

---

### ⚠️ Important

* Config values are **NOT multipliers**
* They represent **extra % added on top of vanilla extended mags**
* Example:

  * `0.15` = +15% over vanilla → total becomes +40%

---

## 📦 Supported Crates

Common crate types:

* `crate_basic`
* `crate_normal`
* `crate_normal_2`
* `crate_elite`
* `crate_tools`
* `crate_military`
* `supply_drop`
* `codelockedhackablecrate`

You can add any custom container names used by your server.

---

## 🎮 How It Works

* Custom magazines are injected into loot containers
* Spawn chances and locations are configurable
* When attached, they override the magazine capacity
* Uses per-skin logic (does not affect vanilla mags)

---

## ⚠️ Notes

* The in-game inspect panel may still show **+25%**

  * This is a **Rust UI limitation**
  * Actual capacity is correctly applied in gameplay
* Move or reattach magazines to refresh their stats after changes

---

## 🛠️ Troubleshooting

### Magazines not spawning?

* Verify container names match exactly
* Check spawn chance values > 0
* Reload plugin:

  ```
  oxide.reload CustomizableMagazines
  ```

### Config issues?

* Delete config and reload plugin to regenerate
* Ensure values like `0.15`, `0.30`, etc. are used (not 1.5)

---

## 🚀 Future Ideas

* Custom UI display for real capacity values
* Tier-based loot balancing
* Per-weapon scaling
* Integration with BetterLoot / LootTable APIs

---

## 📌 Credits

* Plugin: Customizable Extended Magazines
* Maintained by: https://github.com/milestorme

---

## ⭐ Enjoy

Make your loot system more dynamic and rewarding.
