﻿using CivilizedDuels.Services;
using HarmonyLib;
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
                if (hit.m_statusEffect == "Challenged")
                {
                    if (__instance.IsPlayer() && attacker != null && attacker.IsPlayer())
                    {
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
                        var webSocketClient = Mod.WebSocketObject.GetComponent<WebSocketClient>();
                        if (hit.m_attacker.userID != ZDOMan.instance.GetMyID())
                        {
                            webSocketClient.Send("Challenged by id: " + hit.m_attacker.userID);
                            attacker.Damage(hit);
                        } else
                        {
                            webSocketClient.Send("Sent challenge as id: " + hit.m_attacker.userID);
                        }
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
