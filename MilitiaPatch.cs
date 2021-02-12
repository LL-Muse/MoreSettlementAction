using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using System;
using System.Linq;
namespace MoreSettlementAction
{
    [HarmonyPatch(typeof(DefaultSettlementMilitiaModel), "CalculateMilitiaChangeInternal")]
    class MilitiaPatch
    {
        private static void Postfix(Settlement settlement, ref ExplainedNumber __result)
        {
            if (settlement.IsVillage && settlement.Village.Hearth > 1000)
            {
                float penalty = -1 * ((settlement.Village.Hearth - 1000) / 500f);
                __result.Add(penalty, new TextObject("Large Village Corruption Penalty"));
            }
            else if (settlement.IsFortification && settlement.Prosperity > 10000)
            {
                float penalty = -1 * ((settlement.Town.Prosperity - 10000) / 1250f);
                if ((double)settlement.Town.Security > 75.0 && settlement.Town.InRebelliousState)
                    penalty *= 0.8f;
                __result.Add(penalty, new TextObject("Large Town Corruption Penalty"));
            }

                if (settlement.IsTown)
            {
                Town town = settlement.Town;
                foreach (var pair in NotableBehavior.RebelliousNotables)
                {
                    if (town.Settlement.Notables.Contains(pair.Key))
                    {
                        if (town.MapFaction == pair.Value)
                        {
                            float powerEffect = 0.5f + 0.5f * ((int)Math.Min(2, Math.Max(0, (pair.Key.Power / 100))));
                            __result.Add(powerEffect, new TextObject(pair.Key.Name.ToString() + " supporting the rebellion against " + pair.Value.Name.ToString()));
                        }
                    }
                    foreach (Village village in town.Villages)
                    {
                        if (village.Settlement.Notables.Contains(pair.Key))
                        {
                            float powerEffect = 0.5f + 0.5f * ((int)Math.Min(2, Math.Max(0, (pair.Key.Power / 100))));
                            __result.Add(powerEffect, new TextObject(pair.Key.Name.ToString() + " supporting the rebellion against " + pair.Value.Name.ToString()));
                        }
                    }
                }
            }
        }
    }
}