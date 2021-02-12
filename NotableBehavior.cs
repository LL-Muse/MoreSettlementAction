using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Reflection;


namespace MoreSettlementAction
{
    internal class NotableBehavior : CampaignBehaviorBase
    {

        public static Dictionary<Hero, IFaction> RebelliousNotables = new Dictionary<Hero, IFaction>();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(MenuOption));
        }

        private void MenuOption(CampaignGameStarter GameStarter)
        {
            TownArmSeperatists(GameStarter);
            AddDialouge(GameStarter);
        }

        private void AddDialouge(CampaignGameStarter gameStarter)
        {
            gameStarter.AddPlayerLine("clan_support_1", "hero_main_options", "clan_support_1_exit", "I would like you to support my clan", new ConversationSentence.OnConditionDelegate(town_notable_support_condition), (ConversationSentence.OnConsequenceDelegate)null);

            gameStarter.AddDialogLine("clan_support_2", "clan_support_1_exit", "clan_support_2_exit", "If you want my support, I need your support as well", (ConversationSentence.OnConditionDelegate)null, (ConversationSentence.OnConsequenceDelegate)null);

            gameStarter.AddPlayerLine("clan_support_3a", "clan_support_2_exit", "clan_support_3_exit", "I will provide you with my political support", new ConversationSentence.OnConditionDelegate(town_notable_political_bribe_condition), new ConversationSentence.OnConsequenceDelegate(town_notable_political_bribe_consequence));

            gameStarter.AddPlayerLine("clan_support_3b", "clan_support_2_exit", "clan_support_3_exit", "I will provide you with my finicial support", new ConversationSentence.OnConditionDelegate(town_notable_cash_bribe_condition), new ConversationSentence.OnConsequenceDelegate(town_notable_cash_bribe_consequence));

            gameStarter.AddPlayerLine("clan_support_3c", "clan_support_2_exit", "close_window", "Never mind", (ConversationSentence.OnConditionDelegate) null, (ConversationSentence.OnConsequenceDelegate) null );

            gameStarter.AddDialogLine("clan_support_4", "clan_support_3_exit", "hero_pretalk", "You have my support[rb:positive][rf:happy]", (ConversationSentence.OnConditionDelegate)null, (ConversationSentence.OnConsequenceDelegate)null);

            gameStarter.AddPlayerLine("notable_rebellion_town_conversation_1", "hero_main_options", "notable_rebellion_town_conversation_1_exit", "I need your help in overthrowing {FACTION_NAME}", new ConversationSentence.OnConditionDelegate(town_notable_rebellion_condition), (ConversationSentence.OnConsequenceDelegate)null);

            gameStarter.AddDialogLine("notable_rebellion_town_conversation_2", "notable_rebellion_town_conversation_1_exit", "notable_rebellion_town_conversation_2_exit", "The people of {TOWN_NAME} have suffered under the rule of {FACTION_NAME}, but I am putting my life in danger by doing this.", new ConversationSentence.OnConditionDelegate(rebellion_2_condition), (ConversationSentence.OnConsequenceDelegate)null);

            gameStarter.AddPlayerLine("notable_rebellion_town_conversation_3a", "notable_rebellion_town_conversation_2_exit", "notable_rebellion_town_conversation_3_exit", "I will provide you with additional political support", new ConversationSentence.OnConditionDelegate(town_notable_political_bribe_condition), new ConversationSentence.OnConsequenceDelegate(town_notable_political_bribe_consequence));

            gameStarter.AddPlayerLine("notable_rebellion_town_conversation_3b", "notable_rebellion_town_conversation_2_exit", "notable_rebellion_town_conversation_3_exit", "I will provide you with additional finicial support", new ConversationSentence.OnConditionDelegate(town_notable_cash_bribe_condition), new ConversationSentence.OnConsequenceDelegate(town_notable_cash_bribe_consequence));

            gameStarter.AddPlayerLine("notable_rebellion_town_conversation_3c", "notable_rebellion_town_conversation_2_exit", "close_window", "Never Mind", new ConversationSentence.OnConditionDelegate(town_notable_rebellion_condition), (ConversationSentence.OnConsequenceDelegate)null);

            gameStarter.AddDialogLine("notable_rebellion_town_conversation_4", "notable_rebellion_town_conversation_3_exit", "hero_pretalk", "I will begin building a resistance network.  We will strike once we are ready[rb:positive][rf:happy]", (ConversationSentence.OnConditionDelegate) null, new ConversationSentence.OnConsequenceDelegate(town_notable_rebellion_consquence));

            gameStarter.AddPlayerLine("notable_rebellion_ask_1", "hero_main_options", "notable_rebellion_ask_1_exit", "How are the preperations for the rebellion?", new ConversationSentence.OnConditionDelegate(town_notable_rebellion_ask_condition), (ConversationSentence.OnConsequenceDelegate)null);

            gameStarter.AddDialogLine("notable_rebellion_ask_2a", "notable_rebellion_ask_1_exit", "hero_pretalk", "There are still many loyal to the regime.  We need more time to prepare  (current loyalty : {LOYALTY} > 25)", new ConversationSentence.OnConditionDelegate(town_notable_rebellion_ask_2a_condition), (ConversationSentence.OnConsequenceDelegate)null);

            gameStarter.AddDialogLine("notable_rebellion_ask_2b", "notable_rebellion_ask_1_exit", "hero_pretalk", "The forces in the garrison are too strong.  We need more time to prepare  (Rebel Strength: {REBEL} < Garrison Strength : {GARRISON})", new ConversationSentence.OnConditionDelegate(town_notable_rebellion_ask_2b_condition), (ConversationSentence.OnConsequenceDelegate)null);

            gameStarter.AddDialogLine("notable_rebellion_ask_2c", "notable_rebellion_ask_1_exit", "hero_pretalk", "Preparations are done.  Our men are stroming the keep right now", new ConversationSentence.OnConditionDelegate(town_notable_rebellion_ask_2c_condition), new ConversationSentence.OnConsequenceDelegate(triggerRebellion));
        }

        private void triggerRebellion()
        {
            object[] args = new object[1];
            args[0] = (Settlement.CurrentSettlement.IsVillage) ? (object) Settlement.CurrentSettlement.Village.Bound : (object) Settlement.CurrentSettlement;
            RebellionsCampaignBehavior rebellions = new RebellionsCampaignBehavior();
            rebellions.GetType().GetTypeInfo().GetDeclaredMethod("StartRebellionEvent").Invoke(rebellions, args);
        }

        private bool town_notable_rebellion_ask_2c_condition()
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                Town town = Settlement.CurrentSettlement.Village.Bound.Town;
                float militia = town.Settlement.Militia;
                MobileParty garrisonParty = town.GarrisonParty;
                float num = garrisonParty != null ? garrisonParty.Party.TotalStrength : 0.0f;
                foreach (MobileParty party in town.Settlement.Parties)
                {
                    if (party.IsLordParty && FactionManager.IsAlliedWithFaction(party.MapFaction, town.Settlement.MapFaction))
                        num += party.Party.TotalStrength;
                }
                return (double)militia >= (double)num * 1.6 && town.Loyalty <= (double)Campaign.Current.Models.SettlementLoyaltyModel.RebelliousStateStartLoyaltyThreshold;
            }
            else
            {
                Town town = Settlement.CurrentSettlement.Town;
                float militia = town.Settlement.Militia;
                MobileParty garrisonParty = town.GarrisonParty;
                float num = garrisonParty != null ? garrisonParty.Party.TotalStrength : 0.0f;
                foreach (MobileParty party in town.Settlement.Parties)
                {
                    if (party.IsLordParty && FactionManager.IsAlliedWithFaction(party.MapFaction, town.Settlement.MapFaction))
                        num += party.Party.TotalStrength;
                }
                return (double)militia >= (double)num * 1.6 && town.Loyalty <= (double)Campaign.Current.Models.SettlementLoyaltyModel.RebelliousStateStartLoyaltyThreshold;
            }
        }

        private bool town_notable_rebellion_ask_2b_condition()
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                Town town = Settlement.CurrentSettlement.Village.Bound.Town;
                float militia = town.Settlement.Militia;
                MobileParty garrisonParty = town.GarrisonParty;
                float num = garrisonParty != null ? garrisonParty.Party.TotalStrength : 0.0f;
                foreach (MobileParty party in town.Settlement.Parties)
                {
                    if (party.IsLordParty && FactionManager.IsAlliedWithFaction(party.MapFaction, town.Settlement.MapFaction))
                        num += party.Party.TotalStrength;
                }
                MBTextManager.SetTextVariable("REBEL", new TextObject(Math.Floor(militia).ToString()), false);
                MBTextManager.SetTextVariable("GARRISON", new TextObject(Math.Floor(1.6 * num).ToString()), false);
                return (double)militia < (double)num * 1.6;
            }
            else
            {
                Town town = Settlement.CurrentSettlement.Town;
                float militia = town.Settlement.Militia;
                MobileParty garrisonParty = town.GarrisonParty;
                float num = garrisonParty != null ? garrisonParty.Party.TotalStrength : 0.0f;
                foreach (MobileParty party in town.Settlement.Parties)
                {
                    if (party.IsLordParty && FactionManager.IsAlliedWithFaction(party.MapFaction, town.Settlement.MapFaction))
                        num += party.Party.TotalStrength;
                }
                MBTextManager.SetTextVariable("REBEL", new TextObject(Math.Floor(militia).ToString()), false);
                MBTextManager.SetTextVariable("GARRISON", new TextObject(Math.Floor(1.6 * num).ToString()), false);
                return (double)militia < (double)num * 1.6;
            }
        }

        private bool town_notable_rebellion_ask_2a_condition()
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                MBTextManager.SetTextVariable("LOYALTY", new TextObject(Settlement.CurrentSettlement.Village.Bound.Town.Loyalty.ToString()), false);
                return Settlement.CurrentSettlement.Village.Bound.Town.Loyalty > (double)Campaign.Current.Models.SettlementLoyaltyModel.RebelliousStateStartLoyaltyThreshold;
            }
            else
            {
                MBTextManager.SetTextVariable("LOYALTY", new TextObject(Settlement.CurrentSettlement.Town.Loyalty.ToString()), false);
                return Settlement.CurrentSettlement.Town.Loyalty > (double) Campaign.Current.Models.SettlementLoyaltyModel.RebelliousStateStartLoyaltyThreshold;
            }
            
        }

        private bool town_notable_rebellion_ask_condition()
        {
            return RebelliousNotables.ContainsKey(Hero.OneToOneConversationHero);
        }

        private bool rebellion_2_condition()
        {
            if (Settlement.CurrentSettlement != null)
            {
                if (Settlement.CurrentSettlement.IsTown)
                {
                    MBTextManager.SetTextVariable("TOWN_NAME", Settlement.CurrentSettlement.Town.Name.ToString(), false);
                }
                if (Settlement.CurrentSettlement.IsVillage)
                {
                    MBTextManager.SetTextVariable("TOWN_NAME", Settlement.CurrentSettlement.Village.Name.ToString(), false);
                }
                MBTextManager.SetTextVariable("FACTION_NAME", Settlement.CurrentSettlement.MapFaction.ToString(), false);
            }
            return true;
        }

        private void town_notable_cash_bribe_consequence()
        {
            int required = (int) ((Hero.MainHero.GetPerkValue(DefaultPerks.Charm.Diplomacy) ? 0.85 : 1.0f) * (Hero.MainHero.Culture == Hero.OneToOneConversationHero.Culture ? 0.9f : 1.1f) * (10000 + 5000 * (int)Math.Min(2, Math.Max(0, (Hero.OneToOneConversationHero.Power / 100)))));
            Hero.MainHero.Gold -= required;
            InformationManager.DisplayMessage(new InformationMessage(required + " <img src=\"Icons\\Coin@2x\">gold spent giving finical support to " + Hero.OneToOneConversationHero.Name.ToString()));
            ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, 20);
            Hero.OneToOneConversationHero.AddPower(25);
            Hero.OneToOneConversationHero.SupporterOf = Hero.MainHero.Clan;
        }

        private bool town_notable_cash_bribe_condition()
        {
            int required = (int) ((Hero.MainHero.GetPerkValue(DefaultPerks.Charm.Diplomacy) ? 0.85 : 1.0f) * (Hero.MainHero.Culture == Hero.OneToOneConversationHero.Culture ? 0.9f : 1.1f) * (10000 + 5000 * (int)Math.Min(2, Math.Max(0, (Hero.OneToOneConversationHero.Power / 100)))));
            return Hero.MainHero.Gold >= required;
        }

        private void town_notable_political_bribe_consequence()
        {
            int required = (int) ((Hero.MainHero.GetPerkValue(DefaultPerks.Charm.Diplomacy) ? 0.85 : 1.0f) * (Hero.MainHero.Culture == Hero.OneToOneConversationHero.Culture ? 0.9f : 1.1f) * (100 + 50 * (int)Math.Min(2, Math.Max(0, (Hero.OneToOneConversationHero.Power / 100)))));
            Hero.MainHero.Clan.Influence -= required;
            ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, 20);
            Hero.OneToOneConversationHero.AddPower(25);
            Hero.OneToOneConversationHero.SupporterOf = Hero.MainHero.Clan;
            InformationManager.DisplayMessage(new InformationMessage(required + GameTexts.FindText("str_html_influence_icon").ToString() + " influence spent giving political support to " + Hero.OneToOneConversationHero.Name.ToString()));
        }

        private bool town_notable_political_bribe_condition()
        {
            int required = (int) ((Hero.MainHero.GetPerkValue(DefaultPerks.Charm.Diplomacy) ? 0.85 : 1.0f) * (Hero.MainHero.Culture == Hero.OneToOneConversationHero.Culture ? 0.9f : 1.1f) * (100 + 50 * (int)Math.Min(2, Math.Max(0, (Hero.OneToOneConversationHero.Power / 100)))));
            return Hero.MainHero.Clan.Influence >= required;
        }

        private bool town_notable_support_condition()
        {
            return  (Hero.OneToOneConversationHero.SupporterOf == null || (Hero.OneToOneConversationHero.SupporterOf != Hero.MainHero.Clan && Hero.OneToOneConversationHero.GetRelation(Hero.MainHero) > Hero.OneToOneConversationHero.GetRelation(Hero.OneToOneConversationHero.SupporterOf.Leader))) && Hero.OneToOneConversationHero.GetRelation(Hero.MainHero) > 10 && Hero.OneToOneConversationHero.IsNotable;
        }

        private void town_notable_rebellion_consquence()
        {
            RebelliousNotables.Add(Hero.OneToOneConversationHero, Settlement.CurrentSettlement.MapFaction);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.OneToOneConversationHero, Settlement.CurrentSettlement.OwnerClan.Leader, -100);
            if(Settlement.CurrentSettlement.MapFaction.Leader != Settlement.CurrentSettlement.OwnerClan.Leader)
            {
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.OneToOneConversationHero, Settlement.CurrentSettlement.MapFaction.Leader, -100);
            }
        }

        private bool town_notable_rebellion_condition()
        {
            if (Settlement.CurrentSettlement != null)
            {
                MBTextManager.SetTextVariable("FACTION_NAME", Settlement.CurrentSettlement.MapFaction.Name, false);
            }

            IFaction faction;
            
            return Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.IsNotable && Hero.OneToOneConversationHero.SupporterOf == Hero.MainHero.Clan && Settlement.CurrentSettlement != null && Hero.MainHero.MapFaction != Settlement.CurrentSettlement.MapFaction && Hero.OneToOneConversationHero.Culture != Settlement.CurrentSettlement.MapFaction.Culture && Hero.OneToOneConversationHero.Culture != Settlement.CurrentSettlement.OwnerClan.Culture && !RebelliousNotables.TryGetValue(Hero.OneToOneConversationHero, out faction);
        }

        private void TownArmSeperatists(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town", "town_arm_seperatist", "Smuggle Weapons to Angry Seperatists", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (Settlement.CurrentSettlement.Town.Loyalty > 50 || (Settlement.CurrentSettlement.OwnerClan.Culture == Settlement.CurrentSettlement.Culture) || (Settlement.CurrentSettlement.OwnerClan.Kingdom != null && Settlement.CurrentSettlement.OwnerClan.Kingdom.Culture == Settlement.CurrentSettlement.Culture) || Settlement.CurrentSettlement.MapFaction == Hero.MainHero.MapFaction)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Raid;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => ArmSeperatists()));
        }

        private void ArmSeperatists()
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>();
            foreach (ItemRosterElement item in PartyBase.MainParty.ItemRoster)
            {
                if (isWeapon(item.EquipmentElement.Item))
                {
                    inquiryElements.Add(new InquiryElement((object)new Tuple<EquipmentElement, int>(item.EquipmentElement, item.Amount), (item.EquipmentElement.ItemModifier == null ? item.EquipmentElement.Item.Name.ToString() : (item.EquipmentElement.ItemModifier.Name.ToString()) + item.EquipmentElement.Item.Name.ToString()) + " - " + item.Amount, new ImageIdentifier(item.EquipmentElement.Item)));
                }
            }
            if (inquiryElements.Count < 1)
            {
                InformationManager.ShowInquiry(new InquiryData("No weapons in inventory", "", true, false, "OK", "", (Action)null, (Action)null), true);
                GameMenu.SwitchToMenu("town");
            }
            else
            {
                InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData("", "Select Weapons to give seperatists", inquiryElements, true, 1000, "Continue", (string)null, (Action<List<InquiryElement>>)(args =>
                {
                    List<InquiryElement> source = args;
                    if (source != null && !source.Any<InquiryElement>())
                        return;
                    InformationManager.HideInquiry();
                    float WeaponsValue = 0;
                    IEnumerable<Tuple<EquipmentElement, int>> selected = args.Select<InquiryElement, Tuple<EquipmentElement, int>>((Func<InquiryElement, Tuple<EquipmentElement, int>>)(element => element.Identifier as Tuple<EquipmentElement, int>));
                    foreach (Tuple<EquipmentElement, int> pair in selected)
                    {
                        WeaponsValue += pair.Item2 * (2 + Math.Min((int)pair.Item1.Item.Tierf, 6));
                        PartyBase.MainParty.ItemRoster.Remove(new ItemRosterElement(pair.Item1, pair.Item2));
                    }
                    Settlement.CurrentSettlement.Militia += WeaponsValue / 2;
                    float loyaltyChange = Math.Min(100 * (WeaponsValue / Math.Max(Settlement.CurrentSettlement.Town.Prosperity, 1)), Settlement.CurrentSettlement.Town.Loyalty);
                    Settlement.CurrentSettlement.Town.Loyalty -= loyaltyChange;
                    ChangeRelationAction.ApplyPlayerRelation(Settlement.CurrentSettlement.OwnerClan.Leader, (int)(-1 * loyaltyChange));
                    ChangeCrimeRatingAction.Apply(Settlement.CurrentSettlement.MapFaction, loyaltyChange);
                    Hero.MainHero.AddSkillXp(DefaultSkills.Roguery, WeaponsValue * 10);
                }), (Action<List<InquiryElement>>)null));
            }
        }

        private bool isWeapon(ItemObject item)
        {
            return (item.Type == ItemObject.ItemTypeEnum.Arrows || item.Type == ItemObject.ItemTypeEnum.Bolts || item.Type == ItemObject.ItemTypeEnum.Bow || item.Type == ItemObject.ItemTypeEnum.Crossbow || item.Type == ItemObject.ItemTypeEnum.Musket || item.Type == ItemObject.ItemTypeEnum.OneHandedWeapon || item.Type == ItemObject.ItemTypeEnum.Pistol || item.Type == ItemObject.ItemTypeEnum.Polearm || item.Type == ItemObject.ItemTypeEnum.Thrown || item.Type == ItemObject.ItemTypeEnum.TwoHandedWeapon || item.Type == ItemObject.ItemTypeEnum.Bullets);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<Dictionary<Hero, IFaction>>("_rebellious_notables", ref RebelliousNotables);
        }
    }
}