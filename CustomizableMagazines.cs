using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Customizable Magazines", "Razor", "1.1.6-fixed")]
    [Description("Change the Magazines Around.")]
    public class CustomizableMagazines : RustPlugin
    {
        [PluginReference] private Plugin BetterLoot;
        #region Init/Unloading
        private bool debug = false;
        private static CustomizableMagazines Instance;
        private ItemModMagazine _itemModMagazine;
        private const string MagazineUse = "customizablemagazines.admin";

        private void Init()
        {
            Instance = this;
            permission.RegisterPermission(MagazineUse, this);

            if (configData.mags.Count <= 0)
            {
                configData.mags.Add(2892143123, new customMagazines("Extended Magazine 15%", 1.5f, 15f, new List<string> { "crate_basic", "crate_normal", "crate_normal_2" }));
                configData.mags.Add(2892142979, new customMagazines("Extended Magazine 30%", 1.75f, 30f, new List<string> { "crate_basic", "crate_normal", "crate_normal_2" }));
                configData.mags.Add(2892142846, new customMagazines("Extended Magazine 50%", 2.0f, 50f, new List<string> { "crate_basic", "crate_normal", "crate_normal_2" }));
                configData.mags.Add(2892142705, new customMagazines("Extended Magazine 100%", 3.0f, 100f, new List<string> { "crate_basic", "crate_normal", "crate_normal_2" }));
                SaveConfig();
            }
        }

        private void OnServerInitialized()
        {
            var magazineItemDef = ItemManager.FindItemDefinition("weapon.mod.extendedmags");
            if (magazineItemDef == null)
            {
                PrintError("Could not find item definition for weapon.mod.extendedmags");
                return;
            }

            _itemModMagazine = magazineItemDef.gameObject.GetComponent<ItemModMagazine>();
            if (_itemModMagazine == null)
            {
                _itemModMagazine = magazineItemDef.gameObject.AddComponent<ItemModMagazine>();
            }

            AddToItemDefinition(magazineItemDef, _itemModMagazine);

            if (BetterLoot != null)
                Puts("BetterLoot detected. Custom magazine injection will be delayed until after BetterLoot finishes populating containers.");
        }

        private void Unload()
        {
            var magazineItemDef = ItemManager.FindItemDefinition("weapon.mod.extendedmags");
            if (magazineItemDef != null && _itemModMagazine != null)
            {
                RemoveFromItemDefinition(magazineItemDef, _itemModMagazine);
                UnityEngine.Object.DestroyImmediate(_itemModMagazine);
            }

            _itemModMagazine = null;
            Instance = null;
        }
        #endregion

        #region ItemModMagazine
        private static void AddToItemDefinition(ItemDefinition itemDefinition, ItemMod itemMod)
        {
            if (itemDefinition == null || itemMod == null || itemDefinition.itemMods.Contains(itemMod))
                return;

            var length = itemDefinition.itemMods.Length;
            Array.Resize(ref itemDefinition.itemMods, length + 1);
            itemDefinition.itemMods[length] = itemMod;
        }

        private static void RemoveFromItemDefinition(ItemDefinition itemDefinition, ItemMod itemMod)
        {
            if (itemDefinition == null || itemMod == null || !itemDefinition.itemMods.Contains(itemMod))
                return;

            itemDefinition.itemMods = itemDefinition.itemMods.Where(mod => mod != itemMod).ToArray();
        }

        private class ItemModMagazine : ItemMod
        {
            public override void OnParentChanged(Item item)
            {
                if (Instance == null || item == null || item.parent == null || item.skin == 0UL)
                    return;

                if (!Instance.configData.mags.ContainsKey(item.skin))
                    return;

                var held = item.GetHeldEntity() as ProjectileWeaponMod;
                if (held == null)
                    return;

                SetupMagazine(held, item, item.skin);
            }
        }
        #endregion

        #region Hooks
        private readonly HashSet<string> _queuedContainers = new HashSet<string>();
        private readonly HashSet<string> _recentlyProcessedContainers = new HashSet<string>();
        private const float BetterLootDelay = 0.2f;
        private const float StandardDelay = 0.01f;

        private void OnLootSpawn(LootContainer container)
        {
            // On BetterLoot servers this hook may be short-circuited by BetterLoot returning non-null,
            // so do not rely on it there. Use OnItemAddedToContainer instead.
            if (BetterLoot != null)
                return;

            QueueConfiguredMagazine(container, StandardDelay);
        }

        private void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (BetterLoot == null || container == null)
                return;

            if (item == null || item.info == null)
                return;

            // Ignore the item we inject ourselves.
            if (item.info.itemid == 2005491391)
                return;

            var lootContainer = container.entityOwner as LootContainer;
            if (lootContainer == null)
                return;

            QueueConfiguredMagazine(lootContainer, BetterLootDelay);
        }

        private void QueueConfiguredMagazine(LootContainer container, float delay)
        {
            if (container == null)
                return;

            var key = GetContainerKey(container);
            if (string.IsNullOrEmpty(key))
                return;

            if (_queuedContainers.Contains(key) || _recentlyProcessedContainers.Contains(key))
                return;

            _queuedContainers.Add(key);
            timer.Once(delay, () =>
            {
                _queuedContainers.Remove(key);

                if (container == null || container.IsDestroyed)
                    return;

                TryAddConfiguredMagazine(container);
                _recentlyProcessedContainers.Add(key);
                timer.Once(5f, () => _recentlyProcessedContainers.Remove(key));
            });
        }

        private static string GetContainerKey(LootContainer container)
        {
            if (container == null)
                return string.Empty;

            if (container.net != null)
                return container.net.ID.Value.ToString();

            return container.GetInstanceID().ToString();
        }

        private void TryAddConfiguredMagazine(LootContainer container)
        {
            if (container == null || container.inventory == null)
                return;

            if (!configData.settings.randomMagazine)
                return;

            if (container.ShortPrefabName == "stocking_large_deployed" || container.ShortPrefabName == "stocking_small_deployed")
                return;

            string prefabName = container.ShortPrefabName ?? string.Empty;
            if (string.IsNullOrEmpty(prefabName))
                return;

            if (container.inventory.itemList.Any(existing => existing != null && existing.info != null && existing.info.itemid == 2005491391 && existing.skin != 0UL && configData.mags.ContainsKey(existing.skin)))
            {
                if (debug)
                    PrintWarning($"[LootRoll] Skipping {prefabName} because a custom magazine is already present.");
                return;
            }

            foreach (var entry in configData.mags)
            {
                var magConfig = entry.Value;
                if (magConfig == null || magConfig.LootContainers == null || magConfig.LootContainers.Count == 0)
                    continue;

                if (magConfig.SpawnChance <= 0f)
                    continue;

                bool matchesContainer = magConfig.LootContainers.Any(x =>
                    !string.IsNullOrEmpty(x) &&
                    string.Equals(x, prefabName, StringComparison.OrdinalIgnoreCase));

                if (!matchesContainer)
                    continue;

                bool success = RollSuccess(magConfig.SpawnChance);
                if (debug)
                    PrintWarning($"[LootRoll] prefab={prefabName}, skin={entry.Key}, name={magConfig.displayName}, chance={magConfig.SpawnChance}, success={success}");

                if (!success)
                    continue;

                if (container.inventory.itemList.Count >= container.inventory.capacity)
                    container.inventory.capacity++;

                var magazineItem = ItemManager.CreateByItemID(2005491391, 1, entry.Key);
                if (magazineItem == null)
                {
                    PrintError($"Failed to create extended magazine item for skin {entry.Key}");
                    continue;
                }

                magazineItem.name = magConfig.displayName;
                if (!magazineItem.MoveToContainer(container.inventory))
                {
                    magazineItem.Remove();
                    if (debug)
                        PrintWarning($"[LootRoll] Failed to move {magConfig.displayName} into container {prefabName}");
                    continue;
                }

                if (debug)
                    PrintWarning($"[LootRoll] Spawned {magConfig.displayName} in {prefabName} at {container.transform.position}");

                return;
            }
        }

        private static bool RollSuccess(float chance)
        {
            if (chance >= 100f)
                return true;

            if (chance <= 0f)
                return false;

            return UnityEngine.Random.Range(0f, 100f) <= chance;
        }

        private static void SetupMagazine(ProjectileWeaponMod mag, Item item, ulong configName)
        {
            if (mag == null || Instance == null || !Instance.configData.mags.ContainsKey(configName))
                return;

            customMagazines magconfig = Instance.configData.mags[configName];
            mag.magazineCapacity.scalar = magconfig.totalAmmo;
            mag.skinID = configName;
            item.skin = configName;
            mag.name = magconfig.displayName;
            item.name = magconfig.displayName;
            item.MarkDirty();
            mag.SendNetworkUpdateImmediate();
        }
        #endregion

        #region Class Definitions
        public class customMagazines
        {
            [JsonProperty(PropertyName = "Magazine Display Name")]
            public string displayName;

            [JsonProperty(PropertyName = "Ammo Multiplier 1.0 = default Gun")]
            public float totalAmmo;

            [JsonProperty(PropertyName = "Can Spawn In LootContainer types")]
            public List<string> LootContainers;

            [JsonProperty(PropertyName = "LootContainer Spawn Chance 1-100")]
            public float SpawnChance;

            public customMagazines(string displayName, float totalAmmo, float spawnChance, List<string> lootContainers)
            {
                this.displayName = displayName;
                this.totalAmmo = totalAmmo;
                this.LootContainers = lootContainers ?? new List<string>();
                this.SpawnChance = spawnChance;
            }
        }
        #endregion

        #region Configuration
        [JsonObject(MemberSerialization.OptIn)]
        private class ConfigData
        {
            [JsonProperty(PropertyName = "Settings")]
            public Settings settings { get; set; } = new Settings();

            [JsonProperty(PropertyName = "Magazine settings")]
            public Dictionary<ulong, customMagazines> mags { get; set; } = new Dictionary<ulong, customMagazines>();

            public class Settings
            {
                [JsonProperty("Enable Loot Container Spawns")]
                public bool randomMagazine { get; set; } = true;
            }

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber Version { get; set; }

            [JsonProperty(PropertyName = "Last Breaking Change")]
            public VersionNumber LastBreakingChange { get; set; } = new VersionNumber(1, 1, 6);
        }
        #endregion

        #region Configuration Handling
        private ConfigData configData;

        protected override void LoadConfig()
        {
            Instance = this;
            base.LoadConfig();

            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                    LoadDefaultConfig();
            }
            catch
            {
                PrintError("Your configuration file is invalid");
                UpdateConfig();
                return;
            }

            if (configData.Version == default(VersionNumber))
                configData.Version = Version;

            UpdateConfigVersion();
            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            configData = new ConfigData();
            configData.Version = Version;
        }

        protected override void SaveConfig() => Config.WriteObject(configData, true);

        private void UpdateConfig()
        {
            PrintWarning("Invalid config file detected! Backing up current and creating new config...");
            var outdatedConfig = Config.ReadObject<object>();
            Config.WriteObject(outdatedConfig, filename: $"{Name}.Backup");
            LoadDefaultConfig();
            PrintWarning("Config update completed!");
        }

        private void UpdateConfigVersion() => configData.Version = Version;
        #endregion

        #region Commands
        [ConsoleCommand("magazine")]
        private void CmdConsoleMagazine(ConsoleSystem.Arg args)
        {
            if (args?.Args == null || args.Args.Length < 2)
                return;

            string userID = args.Args[0];
            string portal = args.Args[1];

            BasePlayer player = null;
            ulong ids;
            ulong skinId;
            int total = 1;

            if (ulong.TryParse(userID, out ids))
                player = BasePlayer.FindByID(ids);

            if (!ulong.TryParse(portal, out skinId))
            {
                SendReply(args, "Incorrect SkinID format");
                return;
            }

            if (args.Args.Length >= 3 && !int.TryParse(args.Args[2], out total))
            {
                SendReply(args, "Amount not set correctly");
                return;
            }

            if (player != null)
            {
                string[] theItemConfig = args.Args.ToArray();
                GetTheMagazine(null, string.Empty, theItemConfig);
            }
            else
            {
                SendReply(args, "Player not found");
            }
        }

        [ChatCommand("magazine")]
        private void GetTheMagazine(BasePlayer player, string command, string[] args)
        {
            if (player == null)
            {
                ulong ids;
                if (args != null && args.Length > 0 && ulong.TryParse(args[0], out ids))
                {
                    player = BasePlayer.FindByID(ids);
                    if (player == null)
                        return;

                    args = args.Skip(1).ToArray();
                }
                else
                {
                    return;
                }
            }
            else if (player.net?.connection != null && !permission.UserHasPermission(player.UserIDString, MagazineUse))
            {
                SendReply(player, lang.GetMessage("NoPerm", this, player.UserIDString));
                return;
            }

            if (args == null || args.Length <= 0)
            {
                messagePlayer(player);
                return;
            }

            if (args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                messagePlayer(player);
                return;
            }

            ulong theItemConfig;
            if (!ulong.TryParse(args[0], out theItemConfig))
            {
                messagePlayer(player);
                return;
            }

            int total = 1;
            if (args.Length > 1 && !int.TryParse(args[1], out total))
                total = 1;

            if (!configData.mags.ContainsKey(theItemConfig))
            {
                messagePlayer(player);
                SendReply(player, lang.GetMessage("NoValidItem", this, player.UserIDString), theItemConfig);
                return;
            }

            GetMagazineItem(player, theItemConfig, total, true);
        }

        private void messagePlayer(BasePlayer player)
        {
            string configitems = "<color=#ce422b>Magazine Item List Usage /magazine <SkinID></color>\n\n";
            foreach (var key in configData.mags)
            {
                configitems += $"<color=#FFFF00>Item Skin</color>: {key.Key} <color=#FFFF00>Item Name:</color> {key.Value.displayName}\n";
            }
            SendReply(player, configitems);
        }

        private void GetMagazineItem(BasePlayer player, ulong skinID, int total, bool message)
        {
            customMagazines magconfig = configData.mags[skinID];
            var magazineItem = ItemManager.CreateByItemID(2005491391, total, skinID);
            if (magazineItem == null)
                return;

            magazineItem.name = magconfig.displayName;

            if (magazineItem.MoveToContainer(player.inventory.containerBelt, -1, true))
            {
                if (message) SendReply(player, lang.GetMessage("gaveProtector", this), magconfig.displayName);
                return;
            }

            if (magazineItem.MoveToContainer(player.inventory.containerMain, -1, true))
            {
                if (message) SendReply(player, lang.GetMessage("gaveProtector", this), magconfig.displayName);
                return;
            }

            Vector3 velocity = Vector3.zero;
            magazineItem.Drop(player.transform.position + new Vector3(0.5f, 1f, 0), velocity);
            if (message) SendReply(player, lang.GetMessage("droped", this), magconfig.displayName);
        }
        #endregion

        #region Messages
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoPerm"] = "<color=#ce422b>You lack the permissions to use this command!</color>",
                ["gaveProtector"] = "<color=#ce422b>You have just got a {0}!</color>",
                ["droped"] = "<color=#ce422b>Your inventory was full so I dropped your {0} on the ground!</color>",
                ["blocked"] = "<color=#ce422b>You are building blocked!</color>",
                ["NoValidItem"] = "That is not a valid config item {0}!",
                ["NoPlayer"] = "Player not found!",
                ["ammountNot"] = "Amount not set correctly",
                ["SkinIDformat"] = "Incorrect skinID format"
            }, this);
        }
        #endregion
    }
}
