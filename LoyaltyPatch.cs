using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using System;
using System.Linq; 
namespace MoreSettlementAction
{
    [HarmonyPatch(typeof(DefaultSettlementLoyaltyModel), "CalculateLoyaltyChangeInternal")]
    class LoyaltyPatch
    {
        private static void Postfix(Town town, ref ExplainedNumber __result)
        {
            float veteranStrength = 0;
            foreach (var pair in VillageOptionsBehavior.VeteranParties)
            {
                if (town.Villages.Contains(pair.Key))
                {
                    foreach(TroopRosterElement troop in pair.Value.MemberRoster)
                    {
                        veteranStrength += Math.Max(1, troop.Character.Tier) * troop.Number;
                    }
                }
            }
            if(veteranStrength > 0)
            {
                float amount = 20f * (veteranStrength / town.Prosperity);
                __result.Add(amount, new TextObject("Loyal Veterans Settled in Countryside"));
            }

            foreach(SackSettlement sackSettlement in SackSettlementBehavior.sackSettlements)
            {
                if(town == sackSettlement.Town && CampaignTime.Now.ToYears - sackSettlement.SackTime.ToYears < 5 && town.OwnerClan.MapFaction == Hero.MainHero.Clan.MapFaction)
                {
                    __result.Add(-10, new TextObject(town.Name.ToString() + " sacked by " + Hero.MainHero.Clan.Name.ToString() + " on " + sackSettlement.SackTime.ToString() + " (lasts 5 years)"));
                }
            }

            Hero[] remove = new Hero[NotableBehavior.RebelliousNotables.Count];
            int index = 0;
            foreach (var pair in NotableBehavior.RebelliousNotables)
            {
                if (town.Settlement.Notables.Contains(pair.Key)) { 
                    if(town.MapFaction == pair.Value)
                    {
                        int powerEffect = 1 + (int) Math.Min(2,Math.Max(0,(pair.Key.Power / 100)));
                        __result.Add(-1 * powerEffect, new TextObject(pair.Key.Name.ToString() + " supporting the rebellion against " + pair.Value.Name.ToString()));
                    }
                    else
                    {
                        remove[index] = pair.Key;
                    }
                }
                foreach(Village village in town.Villages)
                {
                    if (village.Settlement.Notables.Contains(pair.Key))
                    {
                        if(village.Settlement.MapFaction == pair.Value)
                        {
                            int powerEffect = 1 + (int)Math.Min(2, Math.Max(0, (pair.Key.Power / 100)));
                            __result.Add(-1 * powerEffect, new TextObject(pair.Key.Name.ToString() + " supporting the rebellion against " + pair.Value.Name.ToString()));
                        }
                        else 
                        {
                            remove[index] = pair.Key;
                        }
                    }
                }
                if(pair.Key.HeroState == Hero.CharacterStates.Dead)
                {
                    remove[index] = pair.Key;
                }
                index++;
            }
            for(int i = 0; i < remove.Length; i++)
            {
                if (remove[i] != null)
                {
                    NotableBehavior.RebelliousNotables.Remove(remove[i]);
                }
            }
        }
    }
}