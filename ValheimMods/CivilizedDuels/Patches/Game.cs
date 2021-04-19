using CivilizedDuels.Services;
using HarmonyLib;
using UnityEngine;

namespace CivilizedDuels.Patches
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    static class Game_Start_Patch
    {
        static void Postfix()
        {
            Mod.WebSocketObject = new GameObject();
            Mod.WebSocketObject.AddComponent<WebSocketClient>();
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Logout))]
    static class Game_Logout_Patch
    {
        static void Postfix()
        {
            Object.Destroy(Mod.WebSocketObject);
        }
    }
}
