using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Localization;
using System;
using System.Linq;
using Helpers;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.ObjectSystem;

namespace MoreSettlementAction
{
    [HarmonyPatch(typeof(RebellionsCampaignBehavior), "StartRebellionEvent")]
    class PlayerRebellionPatch
    {
        private static bool Prefix(Settlement settlement)
        {
          
            bool hadPlayerSupport = false;
            List<Hero> NotablesSupportingReveolt = new List<Hero>();
            if (settlement.IsVillage)
            {
                return true;
            }
            Town town = settlement.Town;
            foreach (var pair in NotableBehavior.RebelliousNotables)
            {
                if (town.Settlement.Notables.Contains(pair.Key))
                {
                    if (town.MapFaction == pair.Value)
                    {
                        hadPlayerSupport = true;
                        NotablesSupportingReveolt.Add(pair.Key);
                    }
                }
                foreach (Village village in town.Villages)
                {
                    if (village.Settlement.Notables.Contains(pair.Key))
                    {
                        hadPlayerSupport = true;
                        NotablesSupportingReveolt.Add(pair.Key);
                    }
                }
            }

            if (hadPlayerSupport)
            {
                InformationManager.ShowInquiry(new InquiryData("Rebellion", "Your supporters have started an uprising in " + settlement.Town.Name.ToString() + ".  Do you want to take direct control of the settlement or let them elect their own leader?  Taking control of the settlement will lead to a war with the previous owners" , true, true, "take it", "let them decide", (Action)(() => {
                    DeclareWarAction.ApplyDeclareWarOverSettlement( settlement.MapFaction, Hero.MainHero.MapFaction);
                    ChangeOwnerOfSettlementAction.ApplyByRebellion(Hero.MainHero, settlement);
                }), (Action)(() => {

                    Random rng = new Random();
                    Hero newLeader = NotablesSupportingReveolt[rng.Next(0, NotablesSupportingReveolt.Count - 1)];
                    FieldInfo field2 = newLeader.GetType().GetField("Occupation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if ((FieldInfo)null != field2)
                    {
                        field2.SetValue((object)newLeader, Occupation.Lord);
                    }

                    Clan clan = MBObjectManager.Instance.CreateObject<Clan>();
                    Banner ClanBanner = Banner.CreateRandomClanBanner();
                    TextObject clanName = newLeader.Culture.ClanNameList[rng.Next(0, newLeader.Culture.ClanNameList.Count)];
                    clan.InitializeClan(clanName, clanName, newLeader.Culture, ClanBanner);
                    clan.SetLeader(newLeader);
                    FieldInfo field = clan.GetType().GetField("_tier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if ((FieldInfo)null != field)
                    {
                        field.SetValue((object)clan, rng.Next(2, 4));
                    }

                    newLeader.Clan = clan;
                    newLeader.IsNoble = true;
                    MobileParty newMobileParty1 = clan.CreateNewMobileParty(newLeader);
                    newMobileParty1.ItemRoster.AddToCounts(DefaultItems.Grain, 10);
                    newMobileParty1.ItemRoster.AddToCounts(DefaultItems.Meat, 5);

                    List<Equipment> Lordgear = new List<Equipment>();
                    foreach(Hero hero in Campaign.Current.Heroes)
                    {
                        if(hero.Culture == newLeader.Culture && hero.IsNoble && hero.IsFemale == newLeader.IsFemale)
                        {
                            Lordgear.Add(hero.BattleEquipment);
                        }
                    }
                    EquipmentHelper.AssignHeroEquipmentFromEquipment(newLeader, Lordgear[rng.Next(0, Lordgear.Count - 1)]);

                    clan.UpdateHomeSettlement(settlement);

                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(newLeader, settlement.OwnerClan.Leader, -200);
                    if(settlement.OwnerClan.Leader != settlement.MapFaction.Leader)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(newLeader, settlement.MapFaction.Leader, -200);
                    }
                    DeclareWarAction.Apply(settlement.MapFaction, newLeader.MapFaction);
                    ChangeOwnerOfSettlementAction.ApplyByRebellion(newLeader, settlement);
                })));
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}