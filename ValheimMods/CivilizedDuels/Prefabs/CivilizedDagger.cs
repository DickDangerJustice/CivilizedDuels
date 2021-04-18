using CivilizedDuels.StatusEffects;
using HarmonyLib;
using JotunnLib.Entities;
using System;
using UnityEngine;

namespace CivilizedDuels.Prefabs
{
    public class CivilizedDagger : PrefabConfig
    {
        public CivilizedDagger() : base("CivilizedDagger", "KnifeCopper")
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
            item.m_itemData.m_shared.m_damages.m_slash = 0;
            item.m_itemData.m_shared.m_damages.m_pierce = 0;

            // set status effect
            item.m_itemData.m_shared.m_attackStatusEffect = Mod.StatusEffects["Challenged"];
        }
    }
}
