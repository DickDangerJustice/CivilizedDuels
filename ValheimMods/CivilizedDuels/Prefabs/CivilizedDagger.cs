using CivilizedDuels.StatusEffects;
using HarmonyLib;
using JotunnLib.Entities;
using UnityEngine;

namespace ExampleMagicBox.Prefabs
{
    public class CivilizedDagger : PrefabConfig
    {
        public CivilizedDagger() : base("CivilizedDagger", "DaggerBronze")
        {
            // Nothing to do here
            // "Prefab" wil be set for us automatically after this is called
        }

        public override void Register()
        {
            // Configure item drop
            ItemDrop item = Prefab.GetComponent<ItemDrop>();
            item.m_itemData.m_shared.m_name = "Civilized Dagger";
            item.m_itemData.m_shared.m_description = "Start civilized duels";

            // set challenged status effect
            item.m_itemData.m_shared.m_attackStatusEffect = ScriptableObject.CreateInstance<SE_Challenged>();
        }
    }
}
