using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace MoreSettlementAction
{
    [HarmonyPatch(typeof(Village), "HearthLevel")]
    class HearthLevelPatch
    {
        private static void Postfix(ref Village __instance, ref int __result)
        {
            int amount = (int)__instance.Hearth;
            int level = 0;

            while (amount >= (level+1)*100)
            {
                level++;
                amount -= (level * 100);
            }
            __result = Math.Max(level-1,0);
        }
    }
}