using CivilizedDuels.Services;
using HarmonyLib;
using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CivilizedDuels.Patches
{
    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    static class Character_RPC_Damage_Patch
    {
        static bool Prefix(long sender, HitData hit, Character __instance)
        {
            if (__instance.IsDebugFlying() || !__instance.m_nview.IsOwner() || __instance.GetHealth() <= 0f || __instance.IsDead() || __instance.IsTeleporting() || __instance.InCutscene() || (hit.m_dodgeable && __instance.IsDodgeInvincible()))
            {
                return false;
            }

            Character attacker = hit.GetAttacker();
            if ((hit.HaveAttacker() && attacker == null) || (__instance.IsPlayer() && !__instance.IsPVPEnabled() && attacker != null && attacker.IsPlayer()))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(hit.m_statusEffect))
            {
                if (hit.m_statusEffect.StartsWith("Challenged"))
                {
                    if (__instance.IsPlayer() && attacker != null && attacker.IsPlayer())
                    {
                        ((Player)__instance).m_intro = true;
                        var webSocketClient = Mod.WebSocketObject.GetComponent<WebSocketClient>();

                        StatusEffect statusEffect = __instance.m_seman.GetStatusEffect(hit.m_statusEffect);
                        if (statusEffect == null)
                        {
                            statusEffect = __instance.m_seman.AddStatusEffect(hit.m_statusEffect);
                        }
                        if (statusEffect != null)
                        {
                            // start game in here
                            statusEffect.SetAttacker(attacker);
                        }
                        Debug.Log("Challenge open for user id: " + hit.m_attacker.userID);
                        //dynamic message = new JObject();
                        var isAttacker = hit.m_attacker.userID == ZDOMan.instance.GetMyID();

                        var gameId = !isAttacker ? Guid.NewGuid().ToString() : hit.m_statusEffect.Split(':').Last();
                        var message = new Message
                        {
                            type = "connectValheim",
                            isWhite = isAttacker,
                            gameId = gameId
                        };

                        //message.type = "connectValheim";
                        //message.gameId = ZDOMan.instance.GetMyID().ToString();
                        webSocketClient.Send(JsonMapper.ToJson(message));

                        if (!isAttacker)
                        {
                            //webSocketClient.Send("Challenged by id: " + hit.m_attacker.userID);
                            hit.m_statusEffect = $"Challenged:{gameId}";
                            attacker.Damage(hit);
                        }

                        Application.OpenURL($"{Mod.SiteUrl.Value}/game/{gameId}?isWhite={isAttacker}");
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
