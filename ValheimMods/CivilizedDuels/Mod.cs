using BepInEx;
using HarmonyLib;
using JotunnLib.Entities;
using JotunnLib.Managers;
using Shared;
using System;
using UnityEngine;
using CivilizedDuels.Services;
using CivilizedDuels.Prefabs;
using System.Collections.Generic;
using CivilizedDuels.StatusEffects;

namespace CivilizedDuels
{
    [BepInPlugin("dickdangerjustice.CivilizedDuels", "Civilized Duels", "1.0.0")]
    [BepInDependency(JotunnLib.JotunnLib.ModGuid)]
    public class Mod : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("dickdangerjustice.CivilizedDuels");
        public static Dictionary<string, StatusEffect> StatusEffects = new Dictionary<string, StatusEffect>();
        public static GameObject WebSocketObject;

        void Awake()
        {
            // add status effects
            var challenged = ScriptableObject.CreateInstance(typeof(SE_Challenged)) as SE_Challenged;
            // TODO: should this be set on every instance? Consider
            challenged.name = "Challenged";
            StatusEffects["Challenged"] = challenged;

            PrefabManager.Instance.PrefabRegister += RegisterPrefabs;
            ObjectManager.Instance.ObjectRegister += InitObjects;
            harmony.PatchAll();
        }

        //void OnDestroy()
        //{
        //    harmony.UnpatchSelf();
        //}

        private void RegisterPrefabs(object sender, EventArgs e)
        {

            PrefabManager.Instance.RegisterPrefab(new CivilizedDagger());
        }

        private void InitObjects(object sender, EventArgs e)
        {
            foreach (var statusEffect in StatusEffects.Values)
            {
                // register status effect
                if (ObjectDB.instance.GetStatusEffect(statusEffect.name) == null)
                {
                    Debug.Log($"Registered status effect: {statusEffect.name}");
                    ObjectDB.instance.m_StatusEffects.Add(statusEffect);
                }
            }

            // Add civilized dagger as an item
            ObjectManager.Instance.RegisterItem("CivilizedDagger");

            // Add a sample recipe for the example sword
            ObjectManager.Instance.RegisterRecipe(new RecipeConfig()
            {
                // Name of the recipe (defaults to "Recipe_YourItem")
                Name = "Recipe_CivilizedDagger",

                // Name of the prefab for the crafted item
                Item = "CivilizedDagger",

                // Name of the prefab for the crafting station we wish to use
                // Can set this to null or leave out if you want your recipe to be craftable in your inventory
                CraftingStation = "piece_workbench",

                RepairStation = "piece_workbench",

                // List of requirements to craft your item
                Requirements = new PieceRequirementConfig[]
                {
                    new PieceRequirementConfig()
                    {
                        // Prefab name of requirement
                        Item = "Wood",

                        // Amount required
                        Amount = 1
                    }
                }
            });
        }
    }
}
