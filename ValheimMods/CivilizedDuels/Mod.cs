using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using System.Collections.Generic;
using CivilizedDuels.StatusEffects;
using BepInEx.Configuration;
using Jotunn.Utils;
using System.Reflection;
using Jotunn.Configs;
using CivilizedDuels.Services;

namespace CivilizedDuels
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Mod : BaseUnityPlugin
    {
        public const string PluginGUID = "dickdangerjustice.CivilizedDuels";
        public const string PluginName = "Civilized Duels";
        public const string PluginVersion = "1.0.0";

        private AssetBundle slappingFishBundle;
        private GameObject slappingFish;
        private ButtonConfig escapeChallenge;
        private CustomStatusEffect challengedEffect;

        private readonly Harmony harmony = new Harmony("dickdangerjustice.CivilizedDuels");
        //public static Dictionary<string, StatusEffect> StatusEffects = new Dictionary<string, StatusEffect>();
        public static GameObject WebSocketObject;
        public static ConfigEntry<string> WebSocketEndpoint;
        public static ConfigEntry<string> SiteUrl;

        void Awake()
        {
            // Load, create and init your custom mod stuff
            CreateConfigValues();
            LoadAssets();
            AddInputs();
            AddStatusEffects();
            AddMockedItems();

            harmony.PatchAll();
        }

        private void Update()
        {
            // Since our Update function in our BepInEx mod class will load BEFORE Valheim loads,
            // we need to check that ZInput is ready to use first.
            if (ZInput.instance != null)
            {
                // Check if our button is pressed. This will only return true ONCE, right after our button is pressed.
                if (ZInput.GetButtonDown("EscapeChallenge"))
                {
                    if (Player.m_localPlayer && Player.m_localPlayer.m_intro)
                    {
                        Player.m_localPlayer.m_intro = false;
                        var hitData = new HitData();
                        hitData.m_damage.m_damage = 99999f;
                        Player.m_localPlayer.Damage(hitData);
                        var client = WebSocketObject.GetComponent<WebSocketClient>();
                        client.Send("forceQuit");
                    }
                }
            }
        }

        // Create some sample configuration values to check server sync
        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;

            // Add server config which gets pushed to all clients connecting and can only be edited by admins
            // In local/single player games the player is always considered the admin
            WebSocketEndpoint = Config.Bind("Server config", "WebSocketEndpoint", "wss://civilized-duels.herokuapp.com", new ConfigDescription("Web Socket Endpoint", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
            SiteUrl = Config.Bind("Server config", "SiteUrl", "https://serene-johnson-5519cc.netlify.app", new ConfigDescription("Site Url", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));

            // Add a client side custom input key
            Config.Bind("Client config", "EscapeChallenge", KeyCode.P, new ConfigDescription("Terminate a challenge as a loss."));
        }

        // Various forms of asset loading
        private void LoadAssets()
        {
            //Load embedded resources
            Jotunn.Logger.LogInfo($"Embedded resources: {string.Join(",", Assembly.GetExecutingAssembly().GetManifestResourceNames())}");
            slappingFishBundle = AssetUtils.LoadAssetBundleFromResources("slappingfish", Assembly.GetExecutingAssembly());
            slappingFish = slappingFishBundle.LoadAsset<GameObject>("Assets/CustomItems/SlappingFish/SlappingFish.prefab");
        }

        // Add custom key bindings
        private void AddInputs()
        {
            // Add key bindings backed by a config value
            escapeChallenge = new ButtonConfig
            {
                Name = "EscapeChallenge",
                Key = (KeyCode)Config["Client config", "EscapeChallenge"].BoxedValue,
                HintToken = "Terminate a challenge as a loss."
            };
            InputManager.Instance.AddButton(PluginGUID, escapeChallenge);
        }

        // Add new status effects
        private void AddStatusEffects()
        {
            StatusEffect effect = ScriptableObject.CreateInstance<SE_Challenged>();
            effect.name = "Challenged";

            challengedEffect = new CustomStatusEffect(effect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(challengedEffect);
        }

        // Implementation of assets using mocks, adding recipe's manually without the config abstraction
        private void AddMockedItems()
        {
            Jotunn.Logger.LogInfo("test items");
            if (!slappingFish) Jotunn.Logger.LogWarning($"Failed to load asset from bundle: {slappingFishBundle}");
            else
            {
                Jotunn.Logger.LogInfo("test items 2");
                // Create and add a custom item
                CustomItem CI = new CustomItem(slappingFish, true);
                CI.ItemDrop.m_itemData.m_shared.m_attackStatusEffect = challengedEffect.StatusEffect;
                CI.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 0;
                CI.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 0;
                ItemManager.Instance.AddItem(CI);

                //Create and add a custom recipe
                Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
                recipe.m_item = slappingFish.GetComponent<ItemDrop>();
                recipe.m_craftingStation = Mock<CraftingStation>.Create("piece_workbench");
                var ingredients = new List<Piece.Requirement>
                {
                    MockRequirement.Create("Wood", 1),
                };
                recipe.m_resources = ingredients.ToArray();
                CustomRecipe CR = new CustomRecipe(recipe, true, true);
                ItemManager.Instance.AddRecipe(CR);

                //Enable BoneReorder
                BoneReorder.ApplyOnEquipmentChanged();
            }
            slappingFishBundle.Unload(false);
        }
    }
}
