using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns;
using MCM.Abstractions.Settings.Base.Global;
using System.Linq;

namespace MoreSettlementAction
{

    internal class VillageOptionsBehavior : CampaignBehaviorBase
    {
        public static Dictionary<Village, MobileParty> VeteranParties = new Dictionary<Village, MobileParty>();

        private CampaignTime _startTime;
        private float _Duration;

        private int harvestcount;
        private int tools;

        Random rng;

        private MoreSettlementActionSettings instance = GlobalSettings<MoreSettlementActionSettings>.Instance;
        public override void RegisterEvents() {

            rng = new Random();
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(VillageMenuItems));
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, new Action(this.HourlyTick));

        }

        private void HourlyTick()
        {
            RemoveVeteranParties();
            AddFoodToVeteranParties();
        }

        private void AddFoodToVeteranParties()
        {
            foreach (var pair in VeteranParties)
            {
                if (pair.Value.Food < 1000)
                {
                    pair.Value.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), 1000000);
                }
                if (pair.Value != null && pair.Value.HomeSettlement != null)
                {
                    pair.Value.CurrentSettlement = pair.Value.HomeSettlement;
                    pair.Value.SetMoveGoToSettlement(pair.Value.HomeSettlement);
                    pair.Value.Ai.SetAIState(AIState.VisitingVillage);
                }
            }
        }

        private void RemoveVeteranParties()
        {
            List<MobileParty> parties = new List<MobileParty>();
            foreach(var pair in VeteranParties)
            {
                if(pair.Key.Settlement.OwnerClan != Hero.MainHero.Clan)
                {
                    parties.Add(pair.Value);
                }
            }
            MobileParty[] removeParties = parties.ToArray();
            for(int i = 0; i < removeParties.Length; i++)
            {
                removeParties[i].RemoveParty();
            }
        }

        private void VillageMenuItems(CampaignGameStarter campaignGameStarter)
        {
            MainVillageMenu(campaignGameStarter);

            VillageHousingMenuItems(campaignGameStarter);
            VillageClearingMenuItems(campaignGameStarter);
            VillageMilitiaMenuItems(campaignGameStarter);
            VillageLandlordMenuItems(campaignGameStarter);
            VillageHerdsmenMenuItems(campaignGameStarter);

            VillageSellBanditSlavesMenuItems(campaignGameStarter);
            VillageReturnPeasantsMenuItems(campaignGameStarter);
            VillageSettleVeteransMenuItems(campaignGameStarter);

            VillageProductionMenuItems(campaignGameStarter, "grain", "wheat fields", harvestcount, instance.VillageGrainAthleticsXP, instance.VillageGrainProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "flax", "flax fields", harvestcount, instance.VillageFlaxAthleticsXP, instance.VillageFlaxProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "fish", "fishing galley", harvestcount, instance.VillageFishAthleticsXP, instance.VillageFishProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "fur", "hunting forests", harvestcount, instance.VillageFurAthleticsXP, instance.VillageFurProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "hardwood", "hardwood forests", harvestcount, instance.VillageWoodAthleticsXP, instance.VillageWoodProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "clay", "clay pits", harvestcount, instance.VillageClayAthleticsXP, instance.VillageClayProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "salt", "salt mine", harvestcount, instance.VillageSaltAthleticsXP, instance.VillageSaltProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "iron", "iron mine", harvestcount, instance.VillageIronAthleticsXP, instance.VillageIronProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "grape", "vineyard", harvestcount, instance.VillageGrapeAthleticsXP, instance.VillageGrapeProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "olives", "olive orchard", harvestcount, instance.VillageOlivesAthleticsXP, instance.VillageOlivesProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "date_fruit", "date orchard", harvestcount, instance.VillageDatesAthleticsXP, instance.VillageDatesProductionTime);
            VillageProductionMenuItems(campaignGameStarter, "cotton", "cotton fields", harvestcount, instance.VillageCottonAthleticsXP, instance.VillageCottonProductionTime);
        }

        private void VillageSettleVeteransMenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_village_menu", "village_settle_veterans", "Settle veterans", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("village_settle_veterans_menu")));

            campaignGameStarter.AddGameMenu("village_settle_veterans_menu", "You can allow some of your troops to retire in this village, however it would cost 200 gold per veteran to buy them a plot of land to settle on.  Veterans will protect their new homes increasing the defensibility of this village to raids and towns and castles with a ton of your veterans settled in the countryside would think twice about revolting against you", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("village_settle_veterans_menu", "village_settle_veterans_interface", "Settle veterans", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                PartyScreenManager.OpenScreenWithCondition(new PartyScreenLogic.IsTroopTransferableDelegate(IsTroopTransferable3), new PartyPresentationDoneButtonConditionDelegate(DoneButtonCondition3), new PartyPresentationDoneButtonDelegate(OnDoneClicked3), PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, new TextObject("Retire Veterans"), 10000, false);
            }));

            campaignGameStarter.AddGameMenuOption("village_settle_veterans_menu", "village_settle_veterans_back", "Back", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("village")));
        }

        private bool OnDoneClicked3(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, List<MobileParty> leftParties, List<MobileParty> rigthParties)
        {
            MobileParty party;
            if(leftMemberRoster.Count == 0)
            {
                return false;
            }

            if(!VeteranParties.TryGetValue(Settlement.CurrentSettlement.Village, out party) || party == null || party.MemberRoster.Count == 0)
            {
                party = MBObjectManager.Instance.CreateObject<MobileParty>("Veterans of " + Settlement.CurrentSettlement.Village.Name.ToString());
                party.InitializeMobileParty(new TroopRoster(party.Party), new TroopRoster(party.Party), Settlement.CurrentSettlement.Position2D, 1f);
                party.SetCustomName(new TextObject("Veterans of " + Settlement.CurrentSettlement.Village.Name.ToString()));
                party.HomeSettlement = Settlement.CurrentSettlement;
                party.Party.Owner = Hero.MainHero;
                if (party == null || party.MemberRoster.Count == 0)
                {
                    VeteranParties.Remove(Settlement.CurrentSettlement.Village);
                }
                VeteranParties.Add(Settlement.CurrentSettlement.Village, party);
            }

            int troop_count = 0;

            foreach (TroopRosterElement troopRosterElement in leftMemberRoster)
            {
                troop_count += troopRosterElement.Number;
                party.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number);
            }
            Hero.MainHero.Gold -= troop_count * 200;
            party.CurrentSettlement = party.HomeSettlement;
            party.SetMoveGoToSettlement(party.HomeSettlement);
            party.Ai.SetAIState(AIState.VisitingVillage);
            
            return true;
        }

        private Tuple<bool, string> DoneButtonCondition3(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
        {
            int troop_count = 0;

            foreach (TroopRosterElement troopRosterElement in leftMemberRoster)
            {
                troop_count += troopRosterElement.Number;
            }

            if (troop_count * 200 > Hero.MainHero.Gold)
            {
                return new Tuple<bool, string>(false, "You can not afford to buy all the land needed to settle your veterans");
            }
            return new Tuple<bool, string>(true, "");
        }

        private bool IsTroopTransferable3(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
        {
            return MobileParty.MainParty.MemberRoster.Contains(character) && !character.IsHero;
        }

        private void VillageSellBanditSlavesMenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("village", "sell_slave_village", "Sell bandits into slavery", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                args.Tooltip = new TextObject("They deserve it");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => PartyScreenManager.OpenScreenWithCondition(new PartyScreenLogic.IsTroopTransferableDelegate(IsTroopTransferable), new PartyPresentationDoneButtonConditionDelegate(this.DoneButtonCondition), new PartyPresentationDoneButtonDelegate(this.OnDoneClicked), PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.Transferable, new TextObject("Local Landlords"), 10000, false)), index: 1);
        }

        private Tuple<bool, string> DoneButtonCondition(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
        {
                return new Tuple<bool, string>(true, "");
        }

        private bool OnDoneClicked(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, List<MobileParty> leftParties, List<MobileParty> rigthParties)
        {
            int gold = 0;
            int troopcount = 0;
            foreach (TroopRosterElement troopRosterElement in leftPrisonRoster)
            {
                gold += Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(troopRosterElement.Character, Hero.MainHero) * troopRosterElement.Number;
                troopcount += troopRosterElement.Number;
            }
            Settlement.CurrentSettlement.Village.Hearth += troopcount;
            foreach(Hero notable in Settlement.CurrentSettlement.Notables)
            {
                if(notable.CharacterObject.Occupation == Occupation.RuralNotable)
                {
                    notable.AddPower(troopcount / 5);
                    ChangeRelationAction.ApplyPlayerRelation(notable, troopcount / 20);
                }
            }
            Hero.MainHero.Gold += gold;
            return true;
        }

        private bool IsTroopTransferable(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
        {
            return MobileParty.MainParty.PrisonRoster.Contains(character) && character.Occupation == Occupation.Bandit;
        }

        private void VillageReturnPeasantsMenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("village", "return_peasants_village", "Return peasants their families", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => PartyScreenManager.OpenScreenWithCondition(new PartyScreenLogic.IsTroopTransferableDelegate(IsTroopTransferable2), new PartyPresentationDoneButtonConditionDelegate(DoneButtonCondition2), new PartyPresentationDoneButtonDelegate(OnDoneClicked2), PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, new TextObject("Local Villagers"), 10000, false)), index: 1);
        }

        private bool OnDoneClicked2(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, List<MobileParty> leftParties, List<MobileParty> rigthParties)
        {
            int gold = 0;
            int troopcount = 0;
            foreach (TroopRosterElement troopRosterElement in leftPrisonRoster)
            {
                gold += Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(troopRosterElement.Character, Hero.MainHero) * troopRosterElement.Number;
                troopcount += troopRosterElement.Number;
            }
            foreach (TroopRosterElement troopRosterElement in leftMemberRoster)
            {
                gold += Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(troopRosterElement.Character, Hero.MainHero) * troopRosterElement.Number;
                troopcount += troopRosterElement.Number;
            }
            Settlement.CurrentSettlement.Village.Hearth += troopcount;
            foreach (Hero notable in Settlement.CurrentSettlement.Notables)
            {
                if (notable.CharacterObject.Occupation == Occupation.Headman)
                {
                    notable.AddPower(troopcount / 2);
                    ChangeRelationAction.ApplyPlayerRelation(notable, troopcount / 5);
                }
            }
            Hero.MainHero.Gold += gold;
            return true;
        }

        private Tuple<bool, string> DoneButtonCondition2(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
        {
            return new Tuple<bool, string>(true, "");
        }

        private bool IsTroopTransferable2(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
        {
            return (MobileParty.MainParty.PrisonRoster.Contains(character) || MobileParty.MainParty.MemberRoster.Contains(character)) && character.Culture == Settlement.CurrentSettlement.Culture && character.Name.ToString().Contains("Peasant");
        }

        private void MainVillageMenu(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("village", "build_village", "Village projects", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (!Settlement.CurrentSettlement.IsVillage || Settlement.CurrentSettlement.IsRaided)
                    return false;
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                args.Tooltip = new TextObject("The village needs help with various projects");
                args.IsEnabled = true;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("build_village_menu")), index: 1);

            campaignGameStarter.AddGameMenu("build_village_menu", "You can help the village build some additional houses if you have " + instance.VillageHousingHardwoodCost + " hardwood. The amount of time it will take will depend on your engineering skill on party size \nYou can help the village clear some land if you have " + instance.VillageClearingToolCost + " tool.  The amount of time it will take will depend on your athletic skill on party size\nYou can help the village train additional militia.  The amount of time it will take will depend on you leadership level (Note: massive penalty at low leadership)", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenu("village_production_menu", "Goods Production\n If your clan owns the village, the goods produced will be added directly to your inventory, otherwise the notable will pay you wage depending on amount produced and the goods will be added to the village stockpile", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("village_production_menu", "village_production_leave", "Back", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("build_village_menu");
            }));

            campaignGameStarter.AddGameMenuOption("build_village_menu", "village_production", "Work as a laborer", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("village_production_menu")));

            campaignGameStarter.AddGameMenuOption("village", "rule_village", "Ruler actions", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (Settlement.CurrentSettlement.OwnerClan != Hero.MainHero.Clan)
                {
                    return false;
                }

                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                args.IsEnabled = true;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("ruler_village_menu")), index: 1);

            campaignGameStarter.AddGameMenu("ruler_village_menu", "You can issue special grants and decrees here", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("ruler_village_menu", "ruler_village_menu_leave", "Back", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("village")));
        }

        private void VillageHerdsmenMenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_village_menu", "ruler_village_spawn_landlord", "Grant landed estates", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                if (Hero.MainHero.Clan.Influence < 100)
                {
                    args.Tooltip = new TextObject("You need 100 influence to take this action");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Notables.Count >= 5)
                {
                    args.Tooltip = new TextObject("There are too many notables in this settlement");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Village.Hearth < 500 * Settlement.CurrentSettlement.Notables.Count)
                {
                    args.Tooltip = new TextObject("The hearths of the village is too low to suport another notable");
                    args.IsEnabled = false;
                }
                else
                {
                    args.Tooltip = new TextObject("Adds 1 land owner notable to your settlement");
                    args.IsEnabled = true;
                }
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                List<CharacterObject> source = new List<CharacterObject>();
                CultureObject culture = Settlement.CurrentSettlement.Culture;
                foreach (CharacterObject characterObject in CharacterObject.Templates.Where<CharacterObject>((Func<CharacterObject, bool>)(x => x.Occupation == Occupation.RuralNotable)).ToList<CharacterObject>())
                {
                    if (characterObject.Culture == culture)
                    {
                        source.Add(characterObject);
                    }    
                }
                CharacterObject template = source[rng.Next(0, source.Count - 1)];
                Hero NewHero = HeroCreator.CreateSpecialHero(template, Settlement.CurrentSettlement, null, Hero.MainHero.Clan, (int)template.Age);
                NewHero.ChangeState(Hero.CharacterStates.Active);
                HeroCreationCampaignBehavior herocreationbehavior = new HeroCreationCampaignBehavior();
                herocreationbehavior.DeriveSkillsFromTraits(NewHero, template);
                NewHero.StayingInSettlementOfNotable = Settlement.CurrentSettlement;
                CampaignEventDispatcher.Instance.OnHeroCreated(NewHero, false);
                ChangeRelationAction.ApplyPlayerRelation(NewHero, 20);
                Hero.MainHero.Clan.Influence -= 100;
                GameMenu.SwitchToMenu("village");
            }), index: 1);
        }

        private void VillageLandlordMenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_village_menu", "ruler_village_spawn_heardmen", "Grant grazing rights", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                if (Hero.MainHero.Clan.Influence < 100)
                {
                    args.Tooltip = new TextObject("You need 100 influence to take this action");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Notables.Count >= 5)
                {
                    args.Tooltip = new TextObject("There are too many notables in this settlement");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Village.Hearth < 500 * Settlement.CurrentSettlement.Notables.Count)
                {
                    args.Tooltip = new TextObject("The hearths of the village is too low to suport another notable");
                    args.IsEnabled = false;
                }
                else
                {
                    args.Tooltip = new TextObject("Adds 1 heardman notable to your settlement");
                    args.IsEnabled = true;
                }
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                List<CharacterObject> source = new List<CharacterObject>();
                CultureObject culture = Settlement.CurrentSettlement.Culture;
                foreach (CharacterObject characterObject in CharacterObject.Templates.Where<CharacterObject>((Func<CharacterObject, bool>)(x => x.Occupation == Occupation.Headman)).ToList<CharacterObject>())
                {
                    if (characterObject.Culture == culture)
                    {
                        source.Add(characterObject);
                    }
                }
                CharacterObject template = source[rng.Next(0, source.Count - 1)];
                Hero NewHero = HeroCreator.CreateSpecialHero(template, Settlement.CurrentSettlement, null, Hero.MainHero.Clan, (int)template.Age);
                NewHero.ChangeState(Hero.CharacterStates.Active);
                HeroCreationCampaignBehavior herocreationbehavior = new HeroCreationCampaignBehavior();
                herocreationbehavior.DeriveSkillsFromTraits(NewHero, template);
                NewHero.StayingInSettlementOfNotable = Settlement.CurrentSettlement;
                CampaignEventDispatcher.Instance.OnHeroCreated(NewHero, false);
                ChangeRelationAction.ApplyPlayerRelation(NewHero, 20);
                Hero.MainHero.Clan.Influence -= 100;
                GameMenu.SwitchToMenu("village");
            }), index: 1);
        }

        private void VillageProductionMenuItems(CampaignGameStarter campaignGameStarter, string good, string work_location_name, int ResourceCount, int XPRate, float BaseTime)
        {

            {
                campaignGameStarter.AddGameMenuOption("village_production_menu", "village_production_" + good, "Have your men work the " + work_location_name, (GameMenuOption.OnConditionDelegate)(args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    if (!Settlement.CurrentSettlement.Village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>(good)))
                    {
                        args.IsEnabled = false;
                    }
                    return true;
                }), (GameMenuOption.OnConsequenceDelegate)(args => Produce(good, instance.VillageCottonProductionTime)));
                campaignGameStarter.AddWaitGameMenu("village_production_" + good + "_wait", "Your party working the " + work_location_name, (OnInitDelegate)(args =>
                {
                    args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                    args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
                {
                    args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                    args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    return true;
                }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => ProduceEnd(good, ref ResourceCount, ref tools, XPRate, BaseTime)), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);
                campaignGameStarter.AddWaitGameMenu("village_production_" + good + "2_wait", "Your party working the " + work_location_name, (OnInitDelegate)(args =>
                {
                    args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                    args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
                {
                    args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                    args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    return true;
                }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => Produce2End(good, ref ResourceCount, ref tools, XPRate, BaseTime)), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

                campaignGameStarter.AddGameMenuOption("village_production_" + good + "_wait", "village_production_" + good +"_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }), (GameMenuOption.OnConsequenceDelegate)(args =>
                {
                    GameMenu.SwitchToMenu("village");
                }));

                campaignGameStarter.AddGameMenuOption("village_production_" + good + "2_wait", "village_production_" + good + "2_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }), (GameMenuOption.OnConsequenceDelegate)(args =>
                {
                    GameMenu.SwitchToMenu("village");
                }));
            }
        }

        private void VillageHousingMenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("build_village_menu", "build_village_housing_menu_start", "Start building houses", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => BuildVillageHousing()));

            campaignGameStarter.AddGameMenuOption("build_village_menu", "build_village_housing_menu_leave", "Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("village")));

            campaignGameStarter.AddWaitGameMenu("build_village_housing_wait", "Your party is helping the locals construct new houses", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => BuildVillageHousingEnd()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddWaitGameMenu("build_village_housing2_wait", "Your party is helping the locals construct new houses", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => BuildVillageHousingEnd2()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("build_village_housing_wait", "build_village_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped builing before the job was done."));
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("build_village_housing2_wait", "build_village2_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped builing before the job was done."));
                GameMenu.SwitchToMenu("village");
            }));
        }

        private void VillageClearingMenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("build_village_menu", "village_clearing_menu_start", "Start clearing land", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Mission;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => VillageClearing()));

            campaignGameStarter.AddWaitGameMenu("village_clearing_wait", "Your party is helping the locals clear addition land for cultivation", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => VillageClearingEnd()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddWaitGameMenu("village_clearing2_wait", "Your party is helping the locals clear addition land for cultivation", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => VillageClearing2End()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("village_clearing_wait", "village_clearing_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped clearing land before the job was done."));
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("village_clearing2_wait", "village_clearing2_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped clearing land before the job was done."));
                GameMenu.SwitchToMenu("village");
            }));
        }

        private void VillageMilitiaMenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("build_village_menu", "village_militia_menu_start", "Start training militia", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => VillageMilitia()));

            campaignGameStarter.AddWaitGameMenu("village_militia_wait", "You are helping the locals train additional militia (1 extra militia is added every time the bar is filled up)", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => VillageMilitiaEnd()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddWaitGameMenu("village_militia2_wait", "You are helping the locals train additional militia (1 extra militia is added every time the bar is filled up)", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => VillageMilitia2End()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("village_militia_wait", "village_militia_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped training militia."));
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("village_militia2_wait", "village_militia2_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped training militia."));
                GameMenu.SwitchToMenu("village");
            }));
        }

        private void BuildVillageHousing()
        {
            int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"));
            if (itemNumber < instance.VillageHousingHardwoodCost)
            {
                GameMenu.SwitchToMenu("village");
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You do not possess the resources required.\n" + "You have:\n" + itemNumber.ToString() + " hardwood", true, false, "OK", "", (Action)null, (Action)null), true);
            }
            else
            {
                _startTime = CampaignTime.Now;
                int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
                int engineeringSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Engineering);
                int partyEngineeringSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Engineering);
                _Duration = Math.Max((instance.VillageHousingBaseBuildTime / (partySize + (partyEngineeringSum / 100))) / ((100 + engineeringSkill) / 100), instance.BuildProjectMinTime);
                GameMenu.SwitchToMenu("build_village_housing_wait");
            }
        }

        private void BuildVillageHousing2()
        {
            int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"));
            if (itemNumber < instance.VillageHousingHardwoodCost)
            {
                GameMenu.SwitchToMenu("village");
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You do not possess the resources required.\n" + "You have:\n" + itemNumber.ToString() + " hardwood", true, false, "OK", "", (Action)null, (Action)null), true);
            }
            else
            {
                _startTime = CampaignTime.Now;
                int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
                int engineeringSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Engineering);
                int partyEngineeringSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Engineering);
                _Duration = Math.Max((instance.VillageHousingBaseBuildTime / (partySize + (partyEngineeringSum / 100))) / ((100 + engineeringSkill) / 100), instance.BuildProjectMinTime);
                GameMenu.SwitchToMenu("build_village_housing2_wait");
            }
        }

        private void BuildVillageHousingEnd()
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                Settlement.CurrentSettlement.Village.Hearth += instance.VillageHousingHearth;
            }

            Hero.MainHero.Gold += instance.VillageHousingGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Engineering, instance.VillageHousingEngineeringXp, true);
            MoreSettlementActionHelper.GetCreditForHelping(instance.VillageHousingRelations);
            PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), -1 * instance.VillageHousingHardwoodCost);
            InformationManager.DisplayMessage(new InformationMessage("Houses Built.  You have " + PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood")) + " hardwood left"));
            BuildVillageHousing2();
        }

        private void BuildVillageHousingEnd2()
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                Settlement.CurrentSettlement.Village.Hearth += instance.VillageHousingHearth;
            }

            Hero.MainHero.Gold += instance.VillageHousingGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Engineering, instance.VillageHousingEngineeringXp, true);
            MoreSettlementActionHelper.GetCreditForHelping(instance.VillageHousingRelations);
            PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), -1 * instance.VillageHousingHardwoodCost);
            InformationManager.DisplayMessage(new InformationMessage("Houses Built.  You have " + PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood")) + " hardwood left"));
            BuildVillageHousing();
        }

        private void VillageClearing()
        {
            int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools"));
            if (itemNumber < instance.VillageClearingToolCost)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You do not possess the resources required.\n" + "You have:\n" + itemNumber.ToString() + " tools", true, false, "OK", "", (Action)null, (Action)null), true);
            }
            else
            {
                _startTime = CampaignTime.Now;
                int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
                int athleticsSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Athletics);
                int partyAthleticsingSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Engineering);
                _Duration = Math.Max((instance.VillageHousingBaseBuildTime / (partySize + (partyAthleticsingSum / 100))) / ((100 + athleticsSkill) / 100), instance.BuildProjectMinTime);
                GameMenu.SwitchToMenu("village_clearing_wait");
            }
        }

        private void VillageClearing2()
        {
            int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools"));
            if (itemNumber < instance.VillageClearingToolCost)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You do not possess the resources required.\n" + "You have:\n" + itemNumber.ToString() + " tools", true, false, "OK", "", (Action)null, (Action)null), true);
            }
            else
            {
                _startTime = CampaignTime.Now;
                int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
                int athleticsSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Athletics);
                int partyAthleticsingSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Engineering);
                _Duration = Math.Max((instance.VillageHousingBaseBuildTime / (partySize + (partyAthleticsingSum / 100))) / ((100 + athleticsSkill) / 100), instance.BuildProjectMinTime);
                GameMenu.SwitchToMenu("village_clearing2_wait");
            }
        }

        private void VillageClearingEnd()
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                Settlement.CurrentSettlement.Village.Hearth += instance.VillageClearingHearth;
            }

            Hero.MainHero.Gold += instance.VillageClearingGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Athletics, instance.VillageClearingAthleticXp, true);
            MoreSettlementActionHelper.GetCreditForHelping(instance.VillageClearingRelations);
            PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("tools"), -1 * instance.VillageClearingToolCost);
            InformationManager.DisplayMessage(new InformationMessage("Land Cleared.  You have " + PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) + " tools left"));
            VillageClearing2();
        }

        private void VillageClearing2End()
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                Settlement.CurrentSettlement.Village.Hearth += instance.VillageClearingHearth;
            }

            Hero.MainHero.Gold += instance.VillageClearingGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Athletics, instance.VillageClearingAthleticXp, true);
            MoreSettlementActionHelper.GetCreditForHelping(instance.VillageClearingRelations);
            PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("tools"), -1 * instance.VillageClearingToolCost);
            InformationManager.DisplayMessage(new InformationMessage("Land Cleared.  You have " + PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) + " tools left"));
            VillageClearing();
        }

        private void VillageMilitia()
        {
            _startTime = CampaignTime.Now;
            int leadershipSkillSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Leadership);
            if (leadershipSkillSum == 0)
            {
                leadershipSkillSum++;
            }
            _Duration = instance.MilitiaBaseTime / (leadershipSkillSum / 100);
            GameMenu.SwitchToMenu("village_militia_wait");
        }

        private void VillageMilitia2()
        {
            _startTime = CampaignTime.Now;
            int leadershipSkillSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Leadership);
            if (leadershipSkillSum == 0)
            {
                leadershipSkillSum++;
            }
            _Duration = instance.MilitiaBaseTime / (leadershipSkillSum / 100);
            GameMenu.SwitchToMenu("village_militia2_wait");
        }

        private void VillageMilitiaEnd()
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                Settlement.CurrentSettlement.Village.Owner.Settlement.Militia++;
            }

            Hero.MainHero.Gold += instance.MilitiaGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Leadership, instance.MilitiaLeadershipXp, true);
            MoreSettlementActionHelper.GetCreditForHelping(instance.MilitiaRelations);
            VillageMilitia2();
        }

        private void VillageMilitia2End()
        {
            if (Settlement.CurrentSettlement.IsVillage)
            {
                Settlement.CurrentSettlement.Village.Owner.Settlement.Militia++;
            }

            Hero.MainHero.Gold += instance.MilitiaGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Leadership, instance.MilitiaLeadershipXp, true);
            MoreSettlementActionHelper.GetCreditForHelping(instance.MilitiaRelations);
            VillageMilitia();
        }

        private void Produce(string type, float BaseProductionRate)
        {
            if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) == 0)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You need tools to produce raw materials", true, false, "OK", "", (Action)null, (Action)null), true);
                return;
            }

            if (Settlement.CurrentSettlement.Village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>(type)))
            {
                int athleticsSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Athletics);
                _startTime = CampaignTime.Now;
                _Duration = BaseProductionRate / ((100 + athleticsSkill) / 100);
                GameMenu.SwitchToMenu("village_production_" + type + "_wait");
            }
            else
            {
                InformationManager.ShowInquiry(new InquiryData("Production not possible", "This village does not produce " + type, true, false, "OK", "", (Action)null, (Action)null), true);
            }
        }

        private void Produce2(string type, float BaseProductionRate)
        {
            if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) == 0)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You need tools to produce raw materials", true, false, "OK", "", (Action)null, (Action)null), true);
                return;
            }
            if (Settlement.CurrentSettlement.Village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>(type)))
            {
                int athleticsSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Athletics);
                _startTime = CampaignTime.Now;
                _Duration = BaseProductionRate / ((100 + athleticsSkill) / 100);
                GameMenu.SwitchToMenu("village_production_" + type + "2_wait");
            }
            else
            {
                InformationManager.ShowInquiry(new InquiryData("Production not possible", "This village does not produce " + type, true, false, "OK", "", (Action)null, (Action)null), true);
            }
        }

        private void ProduceEnd(string type, ref int ResourceCount, ref int tools, int BaseXP, float BaseProductionRate)
        {
            int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
            int partyAthleticsSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Athletics);
            ResourceCount = 0;
            tools = 0;
            ProductionTick(partySize + (partyAthleticsSum/100), type, ref ResourceCount, ref tools);
            Produce2(type, BaseProductionRate);
        }

        private void Produce2End(string type, ref int ResourceCount, ref int tools, int BaseXP, float BaseProductionRate)
        {
            int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
            int partyAthleticsSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Athletics);
            ResourceCount = 0;
            tools = 0;
            ProductionTick(partySize + (partyAthleticsSum / 100), type, ref ResourceCount, ref tools);
            Produce(type, BaseProductionRate);
        }

        private void ProductionTick(int unit, string type, ref int ResourceCount, ref int tools)
        {
            if (unit == 0)
            {
                InformationManager.DisplayMessage(new InformationMessage("Your party harvested " + ResourceCount + " " + type + " using " + tools + " tools in the process"));
                return;
            }
            else if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) == 0)
            {
                InformationManager.ShowInquiry(new InquiryData("Out of tools", "You used up all your tools", true, false, "OK", "", (Action)null, (Action)null), true);
                GameMenu.SwitchToMenu("village");
            }
            else
            {
                if(Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan)
                {
                    PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>(type), 1);
                }
                else
                {
                    Settlement.CurrentSettlement.ItemRoster.Add(new ItemRosterElement(MBObjectManager.Instance.GetObject<ItemObject>(type), 1));
                    Hero.MainHero.Gold += instance.VillageGoldPerUnitPayment;
                }
                
                foreach(var notable in Settlement.CurrentSettlement.Notables)
                {
                    notable.AddPower(instance.VillageNotablePowerGainPerGoodProduced);
                }

                ResourceCount++;
                if (rng.Next(0, 100) < instance.ToolBreakChance)
                {
                    PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("tools"), -1);
                    tools++;
                }
                ProductionTick(unit - 1, type, ref ResourceCount, ref tools);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<Dictionary<Village, MobileParty>>("_village_veteran_party", ref VeteranParties);
        }
    }
}
