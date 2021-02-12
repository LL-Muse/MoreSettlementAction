using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using MCM.Abstractions.Settings.Base.Global;

namespace MoreSettlementAction
{
    internal class TownOptionsBehavior : CampaignBehaviorBase
    {
        private CampaignTime _startTime;
        private float _Duration;
        private MoreSettlementActionSettings instance = GlobalSettings<MoreSettlementActionSettings>.Instance;

        private bool GangFought = false;
        private MobileParty gangParty;
        private Hero gangLeader;

        private Dictionary<Town, CampaignTime> LastFestival = new Dictionary<Town, CampaignTime>();

        private Random rng;

        private int tools;
        private int inputcounter;
        private int outputcounter;

        public override void RegisterEvents()
        {
            rng = new Random();

            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(TownMenuItems));
            CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, new Action<MenuCallbackArgs>(this.OnGameMenuOpened));
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, new Action(HourlyTick));
        }

        private void HourlyTick()
        {
            MoveReleasedHeroesToHomeSettlement();
            FormParties();
        }

        private void FormParties()
        {
            if (instance.FormParties)
            {
                if (Hero.MainHero.HomeSettlement == null || !Hero.MainHero.HomeSettlement.IsTown)
                {
                    List<Settlement> settlements = new List<Settlement>();
                    foreach (Settlement settlement in Campaign.Current.Settlements)
                    {
                        if (settlement.Culture == Hero.MainHero.Culture && settlement.IsTown)
                        {
                            settlements.Add(settlement);
                        }
                    }
                    Settlement selected = settlements[rng.Next(0, settlements.Count - 1)];
                    FieldInfo field = Hero.MainHero.GetType().GetField("_homeSettlement", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if ((FieldInfo)null != field)
                    {
                        field.SetValue((object)Hero.MainHero, selected);
                    }
                }
                foreach (Hero hero in Hero.MainHero.Clan.Heroes)
                {
                    if (hero.HeroState == Hero.CharacterStates.Active && hero.PartyBelongedTo == null && !hero.IsOccupiedByAnEvent() && hero.GovernorOf == null && Campaign.Current.Models.ClanTierModel.GetPartyLimitForTier(Hero.MainHero.Clan, Hero.MainHero.Clan.Tier) > Hero.MainHero.Clan.WarParties.Count<MobileParty>())
                    {
                        hero.StayingInSettlementOfNotable = Hero.MainHero.HomeSettlement;
                        Hero.HeroLastSeenInformation lastseen = new Hero.HeroLastSeenInformation();
                        lastseen.IsNearbySettlement = false;
                        lastseen.LastSeenDate = CampaignTime.Now;
                        lastseen.LastSeenPlace = Hero.MainHero.HomeSettlement;
                        FieldInfo field = hero.GetType().GetField("_cachedLastSeenInformation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if ((FieldInfo)null != field)
                        {
                            field.SetValue((object)hero, lastseen);
                        }
                        FieldInfo field2 = hero.GetType().GetField("_lastSeenInformationKnownToPlayer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if ((FieldInfo)null != field2)
                        {
                            field2.SetValue((object)hero, lastseen);
                        }
                        Hero.MainHero.Clan.CreateNewMobilePartyAtPosition(hero, Hero.MainHero.HomeSettlement.GatePosition);
                        InformationManager.DisplayMessage(new InformationMessage(hero.Name.ToString() + " has begun mustering a new party near " + Hero.MainHero.HomeSettlement.Name.ToString()));
                    }
                }
            }
        }

        private void MoveReleasedHeroesToHomeSettlement()
        {
            if(Hero.MainHero.HomeSettlement == null || !Hero.MainHero.HomeSettlement.IsTown)
            {
                List<Settlement> settlements = new List<Settlement>();
                foreach(Settlement settlement in Campaign.Current.Settlements)
                {
                    if(settlement.Culture == Hero.MainHero.Culture && settlement.IsTown)
                    {
                        settlements.Add(settlement);
                    }
                }
                Settlement selected = settlements[rng.Next(0, settlements.Count - 1)];
                FieldInfo field = Hero.MainHero.GetType().GetField("_homeSettlement", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if ((FieldInfo)null != field)
                {
                    field.SetValue((object)Hero.MainHero, selected);
                }
            }
            foreach(Hero hero in Hero.MainHero.Clan.Heroes)
            {
                if(hero.HeroState == Hero.CharacterStates.Released && hero.CurrentSettlement != Hero.MainHero.HomeSettlement)
                {
                    hero.StayingInSettlementOfNotable = Hero.MainHero.HomeSettlement;
                    Hero.HeroLastSeenInformation lastseen = new Hero.HeroLastSeenInformation();
                    lastseen.IsNearbySettlement = false;
                    lastseen.LastSeenDate = CampaignTime.Now;
                    lastseen.LastSeenPlace = Hero.MainHero.HomeSettlement;
                    FieldInfo field = hero.GetType().GetField("_cachedLastSeenInformation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if ((FieldInfo)null != field)
                    {
                        field.SetValue((object)hero, lastseen);
                    }
                    FieldInfo field2 = hero.GetType().GetField("_lastSeenInformationKnownToPlayer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if ((FieldInfo)null != field2)
                    {
                        field2.SetValue((object)hero, lastseen);
                    }
                }
            }
        }

        private void TownMenuItems(CampaignGameStarter campaignGameStarter)
        {
            MainTownMenu(campaignGameStarter);

            TownHousingMenuOptions(campaignGameStarter);
            TownMilitiaMenuOptions(campaignGameStarter);
            TownFestivalMenuOptions(campaignGameStarter);
            TownPatrolMenuOptions(campaignGameStarter);
            TownSponsorTournamentMenuOptions(campaignGameStarter);
            TownCommericalCharterMenuOption(campaignGameStarter);
            TownGuildCharterMenuOption(campaignGameStarter);
            TownCrimeLordDealsMenuOption(campaignGameStarter);
            TownConscriptTroopsMenuOption(campaignGameStarter);
            TownNobleConscriptTroopsMenuOption(campaignGameStarter);
            TownTransferSettlementMenuOption(campaignGameStarter);
            TownAbandonSettlementMenuOption(campaignGameStarter);
            TownSetAsHomeMenuOption(campaignGameStarter);

            TownProductionMenuItems(campaignGameStarter, "brewery", "grain", "beer", inputcounter, outputcounter, instance.WorkshopBeerTradeXP, instance.WorkshopBeerBaseTime);
            TownProductionMenuItems(campaignGameStarter, "velvet_weavery", "cotton", "velvet", inputcounter, outputcounter, instance.WorkshopVelvetTradeXP, instance.WorkshopVelvetBaseTime);
            TownProductionMenuItems(campaignGameStarter, "pottery_shop", "clay", "pottery", inputcounter, outputcounter, instance.WorkshopPotteryTradeXP, instance.WorkshopPotteryBaseTime);
            TownProductionMenuItems(campaignGameStarter, "tannery", "hides", "leather", inputcounter, outputcounter, instance.WorkshopLeatherTradeXP, instance.WorkshopLeatherBaseTime);
            TownProductionMenuItems(campaignGameStarter, "linen_weavery", "flax", "linen", inputcounter, outputcounter, instance.WorkshopLinenTradeXP, instance.WorkshopLinenBaseTime);
            TownProductionMenuItems(campaignGameStarter, "wine_press", "grape", "wine", inputcounter, outputcounter, instance.WorkshopWineTradeXP, instance.WorkshopWineBaseTime);
            TownProductionMenuItems(campaignGameStarter, "olive_press", "olives", "oil", inputcounter, outputcounter, instance.WorkshopOilTradeXP, instance.WorkshopOilBaseTime);
            TownProductionMenuItems(campaignGameStarter, "silversmith", "silver", "jewelry", inputcounter, outputcounter, instance.WorkshopJewelryTradeXP, instance.WorkshopJewelryBaseTime);
        }

        private void TownSetAsHomeMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town", "town_set_as_home_settlement", "Set this town as your home settlement", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (Hero.MainHero.HomeSettlement == Settlement.CurrentSettlement)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                args.Tooltip = new TextObject("Released heroes of your clan will regroup at your clan's home settlement");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                GameMenu.SwitchToMenu("town_set_as_home_settlement_menu");
            }));

            campaignGameStarter.AddGameMenu("town_set_as_home_settlement_menu", "Move your home to this town?  Released heroes of your clan will regroup at your clan's home settlement", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("town_set_as_home_settlement_menu", "town_set_as_home_settlement_menu_confirm", "Confirm", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                FieldInfo field = Hero.MainHero.GetType().GetField("_homeSettlement", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if ((FieldInfo)null != field)
                {
                    field.SetValue((object)Hero.MainHero, Settlement.CurrentSettlement);
                }
                GameMenu.SwitchToMenu("town");
            }), index: 1);

            campaignGameStarter.AddGameMenuOption("town_set_as_home_settlement_menu", "town_set_as_home_settlement_menu_cancle", "Cancle", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                GameMenu.SwitchToMenu("town");
            }), index: 1);
        }

        private void MainTownMenu(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town", "build_town", "Town projects", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (!Settlement.CurrentSettlement.IsTown)
                    return false;
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                args.Tooltip = new TextObject("The town needs help with various projects");
                args.IsEnabled = true;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("build_town_menu")), index: 1);

            campaignGameStarter.AddGameMenu("build_town_menu", "You can help the town build some additional houses if you have " + instance.TownHousingHardwoodCost + " hardwood. The amount of time it will take will depend on your engineering skill on party size\nYou can help train militia.  The amount of time it will take will depend on you leadership level (Note: massive penalty at low leadership)\nYou can also arrange festivity.  This will cost food.  It will increase your charm skill, your relation with notables and the loyatly of the town.\nYou can also patrol the streets and clear them of gangs (Note: There is a chance to get ambushed by gangs)", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("build_town_menu", "town_workshop_work", "Work as a labor in the town workshop", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (!Settlement.CurrentSettlement.IsTown)
                    return false;
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("town_workshop_menu")), index: 1);

            campaignGameStarter.AddGameMenu("town_workshop_menu", "You can turn raw materials into finished goods at the workshop.  If you do not own the workshop in town need to make a particular good, the shop worker will charge you a fee of 1000 gold to use their equipment.  Tools will be needed in addition to the raw materials", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("town", "ruler_town", "Ruler Actions", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (Settlement.CurrentSettlement.OwnerClan != Hero.MainHero.Clan)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu; ;
                args.IsEnabled = true;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("ruler_town_menu")), index: 1);

            campaignGameStarter.AddGameMenu("ruler_town_menu", "You can issue special grants and decrees here", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("build_town_menu", "build_town_menu_leave", "Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("town")));

            campaignGameStarter.AddGameMenuOption("town_workshop_menu", "town_workshop_menu_leave", "Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("town")));

            campaignGameStarter.AddGameMenuOption("ruler_town_menu", "ruler_town_menu_leave", "Back", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("town")));
        }

        private void TownAbandonSettlementMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_town_menu", "ruler_town_abandon", "Abandon Rulership of Town", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                if (Settlement.CurrentSettlement.OwnerClan.Kingdom != null && Settlement.CurrentSettlement.OwnerClan.Kingdom.Leader != Hero.MainHero)
                {
                    args.Tooltip = new TextObject("You must be the faction leader to abandon a settlement");
                    args.IsEnabled = false;
                }

                args.Tooltip = new TextObject("A minor noble will take control of the settlement");

                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                InformationManager.ShowInquiry(new InquiryData("Abandon Town", "Are you sure you want to abandon this town?", true, true, "Yes", "No", (Action)(() => {
                    List<CharacterObject> source = new List<CharacterObject>();
                    CultureObject culture = Settlement.CurrentSettlement.Culture;
                    foreach (CharacterObject characterObject in CharacterObject.Templates.Where<CharacterObject>((Func<CharacterObject, bool>)(x => x.Occupation == Occupation.Lord)).ToList<CharacterObject>())
                    {
                        if (characterObject.Culture == culture)
                        {
                            source.Add(characterObject);
                        }
                    }
                    CharacterObject template = source[rng.Next(0, source.Count - 1)];
                    Hero NewHero = HeroCreator.CreateSpecialHero(template, Settlement.CurrentSettlement, null, Hero.MainHero.Clan, rng.Next(25, 30));
                    NewHero.ChangeState(Hero.CharacterStates.Active);
                    HeroCreationCampaignBehavior herocreationbehavior = new HeroCreationCampaignBehavior();
                    herocreationbehavior.DeriveSkillsFromTraits(NewHero, template);

                    List<CharacterObject> source2 = new List<CharacterObject>();
                    foreach (CharacterObject characterObject in CharacterObject.Templates.Where<CharacterObject>((Func<CharacterObject, bool>)(x => x.Occupation == Occupation.Lord)).ToList<CharacterObject>())
                    {
                        if (characterObject.Culture == culture)
                        {
                            source2.Add(characterObject);
                        }
                    }
                    CharacterObject template2 = source2[rng.Next(0, source2.Count - 1)];
                    template2.IsFemale = true;
                    Hero NewHero2 = HeroCreator.CreateSpecialHero(template2, Settlement.CurrentSettlement, null, Hero.MainHero.Clan, rng.Next(25,30));
                    NewHero2.ChangeState(Hero.CharacterStates.Active);
                    herocreationbehavior.DeriveSkillsFromTraits(NewHero2, template2);
                    template2.IsFemale = false;

                    Clan clan = MBObjectManager.Instance.CreateObject<Clan>();
                    Banner ClanBanner = Banner.CreateRandomClanBanner();
                    TextObject clanName = culture.ClanNameList[rng.Next(0, culture.ClanNameList.Count)];
                    clan.InitializeClan(clanName, clanName, culture, ClanBanner);
                    clan.SetLeader(NewHero);
                    FieldInfo field = clan.GetType().GetField("_tier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if ((FieldInfo)null != field)
                    {
                        field.SetValue((object)clan, rng.Next(2,4));
                    }

                    NewHero.Clan = clan;
                    NewHero.IsNoble = true;
                    MobileParty newMobileParty1 = clan.CreateNewMobileParty(NewHero);
                    newMobileParty1.ItemRoster.AddToCounts(DefaultItems.Grain, 10);
                    newMobileParty1.ItemRoster.AddToCounts(DefaultItems.Meat, 5);

                    NewHero2.Clan = clan;
                    NewHero2.IsNoble = true;
                    MobileParty newMobileParty2 = clan.CreateNewMobileParty(NewHero2);
                    newMobileParty2.ItemRoster.AddToCounts(DefaultItems.Grain, 10);
                    newMobileParty2.ItemRoster.AddToCounts(DefaultItems.Meat, 5);

                    ChangeOwnerOfSettlementAction.ApplyByKingDecision(NewHero, Settlement.CurrentSettlement);
                    clan.UpdateHomeSettlement(Settlement.CurrentSettlement);

                    MarriageAction.Apply(NewHero, NewHero2);
                    ChangeRelationAction.ApplyPlayerRelation(NewHero, 40);

                }), (Action)(() => {
                    GameMenu.SwitchToMenu("town");
                })), true);
            }), index: 1); ;
        }

        private void TownTransferSettlementMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_town_menu", "ruler_town_transfer", "Transfer Ownership of Town", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                if (Settlement.CurrentSettlement.OwnerClan.Kingdom == null)
                {
                    args.Tooltip = new TextObject("You must be in a kingdom to do this");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.OwnerClan.Kingdom.Clans.Count == 1)
                {
                    args.Tooltip = new TextObject("You are the only clan in your kingdom");
                    args.IsEnabled = false;
                }

                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                TransferSettlement();
            }), index: 1); ;
        }

        private void TransferSettlement()
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>();
            foreach (Clan clan in Settlement.CurrentSettlement.OwnerClan.Kingdom.Clans)
            {
                if (clan != Hero.MainHero.Clan && !clan.IsEliminated)
                    inquiryElements.Add(new InquiryElement((object)clan, clan.Name.ToString() + " -  tier " + clan.Tier, new ImageIdentifier(CharacterCode.CreateFrom((BasicCharacterObject)clan.Leader.CharacterObject))));
            }

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData("Settlement Transfer", "Select the clan you want to transfer the settlement to", inquiryElements, true, 1, "Continue", (string)null, (Action<List<InquiryElement>>)(args =>
            {
                List<InquiryElement> source = args;
                if (source != null && !source.Any<InquiryElement>())
                {
                    return;
                }
                    InformationManager.HideInquiry();
                    SubModule.ExecuteActionOnNextTick((Action)(() => InformationManager.ShowInquiry(new InquiryData("", "Transfer the town of " + Settlement.CurrentSettlement.Town.Name + " to the " + (args.Select<InquiryElement, Clan>((Func<InquiryElement, Clan>)(element => element.Identifier as Clan))).First<Clan>().Name + " clan?", true, true, "Continue", "Cancle", (Action)(() => TransferSettlementAction((args.Select<InquiryElement, Clan>((Func<InquiryElement, Clan>)(element => element.Identifier as Clan))).First<Clan>())), (Action)(() => InformationManager.HideInquiry())))));
                    }), (Action<List<InquiryElement>>)null));
        }

        private void TransferSettlementAction(Clan clan)
        {
            ChangeOwnerOfSettlementAction.ApplyByBarter(clan.Leader, Settlement.CurrentSettlement);
            if (Settlement.CurrentSettlement.IsTown)
            {
                ChangeRelationAction.ApplyPlayerRelation(clan.Leader, 40);
            }
            else if(Settlement.CurrentSettlement.IsCastle)
            {
                ChangeRelationAction.ApplyPlayerRelation(clan.Leader, 20);
            }
        }

        private void TownNobleConscriptTroopsMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_town_menu", "ruler_town_noble_conscription", "Forced Noble Conscription", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveTroops;
                if (Settlement.CurrentSettlement.Town.Loyalty < 25)
                {
                    args.Tooltip = new TextObject("The people of this town actively resist any attempts of forceful consription  (Loyalty too low)");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Town.Prosperity < 2000)
                {
                    args.Tooltip = new TextObject("The people of this town look starved and dieseased.  They would make terrible soldiers (Prosperity too low)");
                    args.IsEnabled = false;
                }

                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                InformationManager.ShowTextInquiry(new TextInquiryData("Forced Noble Conscription", "As ruler of this town it is within your rights to drag off some men to refill your levies.  Consripting members of the lower nobilty has greater impact on the loyalty and prosperity of your settlement than conscripting commoners.  You can consript up to " + (int)Settlement.CurrentSettlement.Town.Loyalty/5 + " men.  How many men do you want to conscript?", true, true, "Procede", "Cancel", (Action<string>)(s => NobleConscript(s)), (Action)null));
                GameMenu.SwitchToMenu("town");
            }), index: 1);
        }

        private void NobleConscript(string args)
        {
            try
            {
                int amount = Int32.Parse(args);
                if (amount > Settlement.CurrentSettlement.Town.Loyalty/5)
                {
                    InformationManager.ShowInquiry(new InquiryData("Error", "Input number was larger to max allowed", true, false, "Ok", "", (Action)null, (Action)null), true);
                    return;
                }
                else if (amount <= 0)
                {
                    InformationManager.ShowInquiry(new InquiryData("Error", "Input number must be a positive number", true, false, "Ok", "", (Action)null, (Action)null), true);
                    return;
                }
                Settlement.CurrentSettlement.Town.Loyalty -= 5*amount;
                Settlement.CurrentSettlement.Prosperity -= 5*amount;
                PartyBase.MainParty.MemberRoster.AddToCounts(Settlement.CurrentSettlement.Culture.EliteBasicTroop, amount);
            }
            catch (Exception e)
            {
                InformationManager.ShowInquiry(new InquiryData("Error", "Input was not a number", true, false, "Ok", "", (Action)null, (Action)null), true);
            }
        }

        private void TownConscriptTroopsMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_town_menu", "ruler_town_conscription", "Forced Conscription", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveTroops;
                if (Settlement.CurrentSettlement.Town.Loyalty < 25)
                {
                    args.Tooltip = new TextObject("The people of this town actively resist any attempts of forceful consription  (Loyalty too low)");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Town.Prosperity < 2000)
                {
                    args.Tooltip = new TextObject("The people of this town look starved and dieseased.  They would make terrible soldiers (Prosperity too low)");
                    args.IsEnabled = false;
                }

                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                InformationManager.ShowTextInquiry(new TextInquiryData("Forced Conscription", "As ruler of this town it is within your rights to drag off some men to refill your levies.  However such acts are not popular with the people and depriving the town of working age men will no doubt have an impact on the local economy.  You can consript up to " + (int)Settlement.CurrentSettlement.Town.Loyalty + " men.  How many men do you want to conscript?", true, true, "Procede", "Cancel", (Action<string>)(s => conscript(s)), (Action)null));
                GameMenu.SwitchToMenu("town");
            }), index: 1);
        }

        private void conscript(string args)
        {
            try{
                int amount = Int32.Parse(args);
                if(amount > Settlement.CurrentSettlement.Town.Loyalty)
                {
                    InformationManager.ShowInquiry(new InquiryData("Error", "Input number was larger to max allowed", true, false, "Ok", "", (Action)null, (Action)null), true);
                    return;
                }
                else if(amount <= 0)
                {
                    InformationManager.ShowInquiry(new InquiryData("Error", "Input number must be a positive number", true, false, "Ok", "", (Action)null, (Action)null), true);
                    return;
                }
                Settlement.CurrentSettlement.Town.Loyalty -= amount;
                Settlement.CurrentSettlement.Prosperity -= amount;
                PartyBase.MainParty.MemberRoster.AddToCounts(Settlement.CurrentSettlement.Culture.BasicTroop, amount);
            }
            catch(Exception e)
            {
                InformationManager.ShowInquiry(new InquiryData("Error", "Input was not a number", true, false, "Ok", "", (Action)null, (Action)null), true);
            }
        }

        private void TownSponsorTournamentMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("build_town_menu", "town_sponsor_tournament", "Sponsor a tournament in this town", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                if (Settlement.CurrentSettlement.Town.HasTournament)
                {
                    args.Tooltip = new TextObject("There is already an ongoing tournament in this town");
                    args.IsEnabled = false;
                }
                else if (Hero.MainHero.Gold < 10000)
                {
                    args.Tooltip = new TextObject("You need 10000 gold to sponsor a tournament");
                    args.IsEnabled = false;
                }
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                ITournamentManager tournamentManager = Campaign.Current.TournamentManager;
                tournamentManager.AddTournament(Campaign.Current.Models.TournamentModel.CreateTournament(Settlement.CurrentSettlement.Town));
                Hero.MainHero.Gold -= 10000;
                MoreSettlementActionHelper.GetCreditForHelping(5);
                GameMenu.SwitchToMenu("town");
            }), index: 1);
        }

        private void TownCrimeLordDealsMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_town_menu", "ruler_town_spawn_gang_leader", "Make backroom deal with local crime bosses", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods; 
                if(Hero.MainHero.Clan.Influence < 100)
                {
                    args.Tooltip = new TextObject("You need 100 influence to take this action");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Notables.Count >= 12) 
                {
                    args.Tooltip = new TextObject("There are too many notables in this settlement");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Town.Prosperity < 1000 * Settlement.CurrentSettlement.Notables.Count)
                {
                    args.Tooltip = new TextObject("The prosperity of the town is too low to suport another notable");
                    args.IsEnabled = false;
                }
                else
                {
                    args.Tooltip = new TextObject("Adds 1 gang leader notable to your settlement");
                    args.IsEnabled = true;
                }
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                Hero NewHero = HeroCreator.CreateHeroAtOccupation(Occupation.GangLeader, Settlement.CurrentSettlement);
                ChangeRelationAction.ApplyPlayerRelation(NewHero, 20);
                NewHero.SupporterOf = Hero.MainHero.Clan;
                Hero.MainHero.Clan.Influence -= 100;
                GameMenu.SwitchToMenu("town");
                }), index: 1);
        }

        private void TownGuildCharterMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_town_menu", "ruler_town_spawn_merchant", "Grant trading rights", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                if (Hero.MainHero.Clan.Influence < 100)
                {
                    args.Tooltip = new TextObject("You need 100 influence to take this action");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Notables.Count >= 12)
                {
                    args.Tooltip = new TextObject("There are too many notables in this settlement");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Town.Prosperity < 1000 * Settlement.CurrentSettlement.Notables.Count)
                {
                    args.Tooltip = new TextObject("The prosperity of the town is too low to suport another notable");
                    args.IsEnabled = false;
                }
                else
                {
                    args.Tooltip = new TextObject("Adds 1 merchannt notable to your settlement");
                    args.IsEnabled = true;
                }
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                Hero NewHero = HeroCreator.CreateHeroAtOccupation(Occupation.Merchant, Settlement.CurrentSettlement);
                NewHero.SupporterOf = Hero.MainHero.Clan;
                ChangeRelationAction.ApplyPlayerRelation(NewHero, 20);
                Hero.MainHero.Clan.Influence -= 100;
                GameMenu.SwitchToMenu("town");
            }), index: 1);
        }

        private void TownCommericalCharterMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_town_menu", "ruler_town_spawn_artisan", "Grant commercial charters", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                if (Hero.MainHero.Clan.Influence < 100)
                {
                    args.Tooltip = new TextObject("You need 100 influence to take this action");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Notables.Count >= 12)
                {
                    args.Tooltip = new TextObject("There are too many notables in this settlement");
                    args.IsEnabled = false;
                }
                else if (Settlement.CurrentSettlement.Town.Prosperity < 1000 * Settlement.CurrentSettlement.Notables.Count)
                {
                    args.Tooltip = new TextObject("The prosperity of the town is too low to suport another notable");
                    args.IsEnabled = false;
                }
                else
                {
                    args.Tooltip = new TextObject("Adds 1 artisan notable to your settlement");
                    args.IsEnabled = true;
                }
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                Hero NewHero = HeroCreator.CreateHeroAtOccupation(Occupation.Artisan, Settlement.CurrentSettlement);
                ChangeRelationAction.ApplyPlayerRelation(NewHero, 20);
                NewHero.SupporterOf = Hero.MainHero.Clan;
                Hero.MainHero.Clan.Influence -= 100;
                GameMenu.SwitchToMenu("town");
            }), index: 1);
        }

        private void TownProductionMenuItems(CampaignGameStarter campaignGameStarter, string WorkshopId, string input, string output, int InputCount, int OutputCount, int XPRate, float BaseTime)
        {
            {
                string workshopName = WorkshopId;
                if(WorkshopType.Find(WorkshopId) != null)
                {
                    workshopName = WorkshopType.Find(WorkshopId).Name.ToString().ToLower();
                }

                campaignGameStarter.AddGameMenuOption("town_workshop_menu", "town_workshop_menu_" + WorkshopId, "Have your men work in the " + workshopName, (GameMenuOption.OnConditionDelegate)(args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    if (!MoreSettlementActionHelper.TownHasWorkshop(Settlement.CurrentSettlement.Town, WorkshopId))
                    {
                        args.IsEnabled = false;
                    }
                    return true;
                }), (GameMenuOption.OnConsequenceDelegate)(args => Produce(input, output, instance.VillageCottonProductionTime)));

                campaignGameStarter.AddWaitGameMenu("town_workshop_production_" + output + "_wait", "Your party working in the " + workshopName, (OnInitDelegate)(args =>
                {
                    args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                    args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
                {
                    args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                    args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    return true;
                }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => ProduceEnd(WorkshopId, input, output, ref InputCount, ref OutputCount, ref tools, XPRate, BaseTime)), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);
                campaignGameStarter.AddWaitGameMenu("town_workshop_production_" + output + "2_wait", "Your party working in the " + workshopName, (OnInitDelegate)(args =>
                {
                    args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                    args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
                {
                    args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                    args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    return true;
                }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => Produce2End(WorkshopId, input, output, ref InputCount, ref OutputCount, ref tools, XPRate, BaseTime)), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

                campaignGameStarter.AddGameMenuOption("town_workshop_production_" + output + "_wait", "town_workshop_production_" + output + "_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }), (GameMenuOption.OnConsequenceDelegate)(args =>
                {
                    GameMenu.SwitchToMenu("town");
                }));

                campaignGameStarter.AddGameMenuOption("town_workshop_production_" + output + "2_wait", "town_workshop_production_" + output + "2_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }), (GameMenuOption.OnConsequenceDelegate)(args =>
                {
                    GameMenu.SwitchToMenu("town");
                }));
            }
        }

        private void Produce(string input, string output, float BaseProductionRate)
        {
            if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) == 0 && output != "tools")
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You need tools to produce " + output, true, false, "OK", "", (Action)null, (Action)null), true);
                return;
            }

            if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>(input)) == 0)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You need " + input + " to produce " + output, true, false, "OK", "", (Action)null, (Action)null), true);
                return;
            }

            int TradeSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);
            _startTime = CampaignTime.Now;
            _Duration = BaseProductionRate / ((100 + TradeSkill) / 100);
            GameMenu.SwitchToMenu("town_workshop_production_" + output + "_wait");
        }

        private void Produce2(string input, string output, float BaseProductionRate)
        {
            if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) == 0 && output != "tools")
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You need tools to produce " + output, true, false, "OK", "", (Action)null, (Action)null), true);
                return;
            }

            if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>(input)) == 0)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Materials", "You need " + input + " to produce " + output, true, false, "OK", "", (Action)null, (Action)null), true);
                return;
            }

            int TradeSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);
            _startTime = CampaignTime.Now;
            _Duration = BaseProductionRate / ((100 + TradeSkill) / 100);
            GameMenu.SwitchToMenu("town_workshop_production_" + output + "2_wait");
        }

        private void ProduceEnd(string workshopId, string input, string output, ref int InputCount, ref int OutputCount, ref int tools, int BaseXP, float BaseProductionRate)
        {
            int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
            int partyTradeSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Trade);

            InputCount = 0;
            OutputCount = 0;
            tools = 0;
            ProductionTick(partySize + (partyTradeSum / 100), workshopId, input, output, ref InputCount, ref OutputCount, ref tools, BaseXP, false);
            Produce2(input, output, BaseProductionRate);
        }

        private void Produce2End(string workshopId, string input, string output, ref int InputCount, ref int OutputCount, ref int tools, int BaseXP, float BaseProductionRate)
        {
            int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
            int partyTradeSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Trade);

            InputCount = 0;
            OutputCount = 0;
            tools = 0;
            ProductionTick(partySize + (partyTradeSum / 100), workshopId, input, output, ref InputCount, ref OutputCount, ref tools, BaseXP, false);
            Produce(input, output, BaseProductionRate);
        }

        private void ProductionTick(int unit, string workshopId, string input, string output , ref int InputCount, ref int OutputCount, ref int tools, int baseExp, bool paid)
        {
            if (unit == 0)
            {
                InformationManager.DisplayMessage(new InformationMessage("Your party produce " + InputCount + " " + input + " from " + OutputCount + " " + output + " using " + tools + " tools in the process"));
                return;
            }
            else if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) == 0 && output != "tools")
            {
                InformationManager.ShowInquiry(new InquiryData("Out of tools", "You used up all your tools", true, false, "OK", "", (Action)null, (Action)null), true);
                GameMenu.SwitchToMenu("town");
                return;
            }
            else if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>(input)) == 0)
            {
                InformationManager.ShowInquiry(new InquiryData("Out of " + input, "You used up all your " + input, true, false, "OK", "", (Action)null, (Action)null), true);
                GameMenu.SwitchToMenu("town");
                return;
            }
            else
            {
                if ((MoreSettlementActionHelper.OwnsWorkshop(Settlement.CurrentSettlement.Town, workshopId) == 0) && !paid)
                {
                    if (Hero.MainHero.Gold < 1000)
                    {
                        InformationManager.ShowInquiry(new InquiryData("Insufficent funds", "You 1000 gold to produce goods in a workshop you do not own", true, false, "OK", "", (Action)null, (Action)null), true);
                        GameMenu.SwitchToMenu("town");
                        paid = true;
                        return;
                    }
                    else
                    {
                        Hero.MainHero.Gold -= 1000;
                    }
                }

                foreach (var notable in Settlement.CurrentSettlement.Notables)
                {
                    if (!notable.IsGangLeader)
                    {
                        notable.AddPower(instance.VillageNotablePowerGainPerGoodProduced);
                    }
                }

                InputCount++;
                OutputCount++;

                PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>(input), -1);
                PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>(output), 1);

                if (rng.Next(0, 100) < instance.ToolBreakChance)
                {
                    PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("tools"), -1);
                    tools++;
                }
                Hero.MainHero.AddSkillXp(DefaultSkills.Trade, baseExp / 10);
                
                ProductionTick(unit - 1, workshopId, input, output, ref InputCount, ref OutputCount, ref tools, baseExp, paid);
            }
        }

        private void TownHousingMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("build_town_menu", "build_town_housing_menu_start", "Start building houses", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => BuildTownHousing()));

            campaignGameStarter.AddWaitGameMenu("build_town_housing_wait", "Your party is helping the locals construct new houses", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => BuildTownHousingEnd()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddWaitGameMenu("build_town_housing2_wait", "Your party is helping the locals construct new houses", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => BuildTownHousing2End()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("build_town_housing_wait", "build_town_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped builing before the job was done."));
                GameMenu.SwitchToMenu("town");
            }));

            campaignGameStarter.AddGameMenuOption("build_town_housing2_wait", "build_town2_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped builing before the job was done."));
                GameMenu.SwitchToMenu("town");
            }));
        }

        private void TownMilitiaMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("build_town_menu", "town_militia_menu_start", "Start training militia", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if(Settlement.CurrentSettlement.Town.Loyalty < 50)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => TownMilitia()));

            campaignGameStarter.AddWaitGameMenu("town_militia_wait", "You are helping the locals train militia.  (1 militia added every time the bar fills up)", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => TownMilitiaEnd()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddWaitGameMenu("town_militia2_wait", "You are helping the locals train militia.  (1 militia added every time the bar fills up)", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => TownMilitia2End()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("town_militia_wait", "town_militia_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("town");
            }));
            campaignGameStarter.AddGameMenuOption("town_militia2_wait", "town_militia2_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("town");
            }));
        }

        private void TownFestivalMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("build_town_menu", "arrange_festivity_start", "Arrange a festival", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => TownFestival()));

            campaignGameStarter.AddWaitGameMenu("town_festival_wait", "Your party is arranging feasts and entertainment in the town square", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => TownFestivalEnd()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("town_festival_wait", "town_festival_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("town");
            }));
        }

        private void TownPatrolMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("build_town_menu", "town_patorl_start", "Patorl Town", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => TownPatrol()));

            campaignGameStarter.AddWaitGameMenu("town_patrol_wait", "Your party is patroling the town streets for criminals", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => TownPatrolEnd()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("town_patrol_wait", "town_patrol_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("town");
            }));

            campaignGameStarter.AddGameMenu("town_patrol_ambush", "You are ambushed by a gang of thugs.  You can either rally the few party members near you to withstand the onslaught or try to lure the out of town where the rest of your army is waiting  (Lure success is base on your tactics level)", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("town_patrol_ambush", "town_patrol_attack", "Make your stand here", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => PatrolGangFight(true)));

            campaignGameStarter.AddGameMenuOption("town_patrol_ambush", "town_patrol_lure", "Attempt to lure them out", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => PatrolGangFight(Hero.MainHero.GetSkillValue(DefaultSkills.Tactics) < rng.Next(1, 200))));
        }

        private void BuildTownHousing()
        {
            int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"));
            if (itemNumber < instance.TownHousingHardwoodCost)
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
                _Duration = Math.Max((instance.TownHousingBaseBuildTime / (partySize + (partyEngineeringSum / 100))) / ((100 + engineeringSkill) / 100), instance.BuildProjectMinTime);
                GameMenu.SwitchToMenu("build_town_housing_wait");
            }
        }

        private void BuildTownHousing2()
        {
            int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"));
            if (itemNumber < instance.TownHousingHardwoodCost)
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
                _Duration = Math.Max((instance.TownHousingBaseBuildTime / (partySize + (partyEngineeringSum / 100))) / ((100 + engineeringSkill) / 100), instance.BuildProjectMinTime);
                GameMenu.SwitchToMenu("build_town_housing2_wait");
            }
        }

        private void BuildTownHousingEnd()
        {
            if (Settlement.CurrentSettlement.IsTown)
            {
                Settlement.CurrentSettlement.Prosperity += instance.TownHousingProsperity;
            }

            Hero.MainHero.Gold += instance.TownHousingGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Engineering, instance.TownHousingEngineeringXp, true);
            MoreSettlementActionHelper.GetCreditForHelping(instance.TownHousingRelations);
            PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), -1 * instance.TownHousingHardwoodCost);
            InformationManager.DisplayMessage(new InformationMessage("Houses Built.  You have " + PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood")) + " hardwood left"));
            BuildTownHousing2();
        }

        private void BuildTownHousing2End()
        {
            if (Settlement.CurrentSettlement.IsTown)
            {
                Settlement.CurrentSettlement.Prosperity += instance.TownHousingProsperity;
            }

            Hero.MainHero.Gold += instance.TownHousingGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Engineering, instance.TownHousingEngineeringXp, true);
            MoreSettlementActionHelper.GetCreditForHelping(instance.TownHousingRelations);
            PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), -1 * instance.TownHousingHardwoodCost);
            InformationManager.DisplayMessage(new InformationMessage("Houses Built.  You have " + PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood")) + " hardwood left"));
            BuildTownHousing();
        }

        private void TownMilitia()
        {
            _startTime = CampaignTime.Now;
            int leadershipSkillSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Leadership);
            if (leadershipSkillSum == 0)
            {
                leadershipSkillSum++;
            }
            _Duration = instance.MilitiaBaseTime / (leadershipSkillSum / 100);
            GameMenu.SwitchToMenu("town_militia_wait");
        }

        private void TownMilitia2()
        {
            _startTime = CampaignTime.Now;
            int leadershipSkillSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Leadership);
            if (leadershipSkillSum == 0)
            {
                leadershipSkillSum++;
            }
            _Duration = instance.MilitiaBaseTime / (leadershipSkillSum / 100);
            GameMenu.SwitchToMenu("town_militia2_wait");
        }

        private void TownMilitiaEnd()
        {
            if (Settlement.CurrentSettlement.IsTown)
            {
                Settlement.CurrentSettlement.Militia++;
            }

            Hero.MainHero.Gold += instance.MilitiaGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Leadership, instance.MilitiaLeadershipXp, true);

            MoreSettlementActionHelper.GetCreditForHelping(instance.MilitiaRelations);
            TownMilitia2();
        }

        private void TownMilitia2End()
        {
            if (Settlement.CurrentSettlement.IsTown)
            {
                Settlement.CurrentSettlement.Militia++;
            }

            Hero.MainHero.Gold += instance.MilitiaGold;
            MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Leadership, instance.MilitiaLeadershipXp, true);

            MoreSettlementActionHelper.GetCreditForHelping(instance.MilitiaRelations);
            TownMilitia();
        }

        private void TownFestival()
        {
            CampaignTime lastFestivalTime;
            if (LastFestival.TryGetValue(Settlement.CurrentSettlement.Town, out lastFestivalTime))
            {
                if(CampaignTime.Now.ToDays - lastFestivalTime.ToDays < 21)
                {
                    InformationManager.ShowInquiry(new InquiryData("Festival Recently", "Last festival was on " + lastFestivalTime.ToString() + "\n Must wait at least 21 days before another festival can be organized in this town", true, false, "Okay", "", (Action)null, (Action)null ));
                    GameMenu.SwitchToMenu("town");
                    return;
                }
                LastFestival.Remove(Settlement.CurrentSettlement.Town);
            }
            int FoodAmount = MoreSettlementActionHelper.GetFoodAmount(PartyBase.MainParty);
            if (FoodAmount < instance.TownFestivalFood)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Food", "You do not possess the resources required.\n" + "You have:\n" + FoodAmount.ToString() + " food and you need " + instance.TownFestivalFood + " to host a festival\n(Note: livestock not counted in inventory food amount)", true, false, "OK", "", (Action)null, (Action)null), true);
            }
            else
            {
                _startTime = CampaignTime.Now;
                _Duration = instance.TownFestivalBaseTime;
                GameMenu.SwitchToMenu("town_festival_wait");
            }
        } 

        private void TownFestivalEnd()
        {
            if (Settlement.CurrentSettlement.IsTown)
            {
                Settlement.CurrentSettlement.Town.Loyalty += instance.TownFestivalLoyalty;
            }

            LastFestival.Add(Settlement.CurrentSettlement.Town, CampaignTime.Now);
            MoreSettlementActionHelper.ConsumeFood(instance.TownFestivalFood, PartyBase.MainParty);
            Hero.MainHero.AddSkillXp(DefaultSkills.Charm, instance.TownFestivalCharmXp);
            MoreSettlementActionHelper.GetCreditForHelping(PartyBase.MainParty.ItemRoster.FoodVariety);
            GameMenu.SwitchToMenu("town");
        }

        private void TownPatrol()
        {
            _startTime = CampaignTime.Now;
            _Duration = instance.TownPatrolTime;
            GameMenu.SwitchToMenu("town_patrol_wait");
        }

        private void TownPatrolEnd()
        {
            if(rng.Next(0,100) <= instance.TownPatrolGangEncounterPercentage)
            {
                GameMenu.SwitchToMenu("town_patrol_ambush");
            }
            else
            {
                Hero.MainHero.Gold += instance.TownPatrolGold;
                MoreSettlementActionHelper.SkillXPToAllPartyHeroes(DefaultSkills.Leadership, instance.TownPatrolLeadershipXP, true);
                foreach (Hero notable in Settlement.CurrentSettlement.Notables)
                {
                    if (!notable.IsGangLeader)
                    {
                        ChangeRelationAction.ApplyPlayerRelation(notable, instance.TownPatrolRelations);
                        notable.AddPower(instance.TownPatrolRelations);
                    }
                    else
                    {
                        ChangeRelationAction.ApplyPlayerRelation(notable, -1 * instance.TownPatrolRelations);
                        notable.AddPower(-1 * instance.TownPatrolRelations);
                    }
                }    
                GameMenu.SwitchToMenu("town");
            }
        }

        private void OnGameMenuOpened(MenuCallbackArgs args)
        {
            
            if (Campaign.Current.GameMenuManager.NextLocation != null || !(GameStateManager.Current.ActiveState is MapState))
                return;
            if ((args.MenuContext.GameMenu.StringId == "town"))
            {
                if (GangFought && PlayerEncounter.Battle != null)
                {
                    if (PlayerEncounter.Battle.WinningSide == PlayerEncounter.Battle.PlayerSide)
                    {
                        PlayerEncounter.Current.FinalizeBattle();
                        Settlement.CurrentSettlement.Town.Security += instance.TownPatrolFightSecurity;
                        foreach (Hero notable in Settlement.CurrentSettlement.Notables)
                        {
                            if (!notable.IsGangLeader)
                            {
                                ChangeRelationAction.ApplyPlayerRelation(notable, instance.TownPatrolFightRelation);
                                notable.AddPower(instance.TownPatrolFightRelation);
                            }
                            Hero.MainHero.Gold += instance.TownPatrolFightGold;
                            Hero.MainHero.AddSkillXp(DefaultSkills.Leadership, instance.TownPatrolFightLeadership);
                        }
                        InformationManager.ShowInquiry(new InquiryData("Battle Won", "You have defeated the gang of " + gangLeader.Name.ToString(), true, false, "OK", "", (() => {
                            PlayerEncounter.LeaveSettlement();
                            PlayerEncounter.Finish();
                            Campaign.Current.SaveHandler.SignalAutoSave();
                        }), (Action)null), true);
                    }
                    else
                    {
                        Settlement.CurrentSettlement.Town.Security -= instance.TownPatrolFightSecurity;
                        InformationManager.ShowInquiry(new InquiryData("Battle Lost", "You have been defeated by the gang of " + gangLeader.Name.ToString() + "\nYou are release after taking all the money in your coin purse", true, false, "OK", "", (()=> {
                            PlayerEncounter.LeaveSettlement();
                            PlayerEncounter.Finish();
                            Campaign.Current.SaveHandler.SignalAutoSave();
                        }), (Action)null), true);
                        Hero.MainHero.Gold -= Math.Min(Hero.MainHero.Gold, instance.MaxGoldFromLosingScriptBattle);
                    }
                    ChangeRelationAction.ApplyPlayerRelation(gangLeader, -1 * instance.TownPatrolFightRelation);
                    gangLeader.AddPower(-1 * instance.TownPatrolFightRelation);
                    PlayerEncounter.LeaveEncounter = true;
                    if (gangParty != null)
                    {
                        gangParty.RemoveParty();
                    }
                    GangFought = false;
                }
            }
        }

        private MobileParty CreatePatrolGangParty()
        {
            Hero GangLeader = RandomGangLeaderInSettlement();
            MobileParty GangParty = MBObjectManager.Instance.CreateObject<MobileParty>("patrol_encounter_gang_party");

            GangParty.InitializeMobileParty(new TroopRoster(GangParty.Party), new TroopRoster(GangParty.Party), Settlement.CurrentSettlement.GatePosition, 1f);
            GangParty.SetCustomName(new TextObject(GangLeader.Name.ToString() + "'s gang"));
            GangParty.HomeSettlement = Settlement.CurrentSettlement;
            GangParty.Party.Owner = GangLeader;
            GangParty.MemberRoster.AddToCounts(CharacterObject.All.First<CharacterObject>((Func<CharacterObject, bool>)(x => x.StringId == "gangleader_bodyguard_" + GangParty.HomeSettlement.Culture.StringId)), rng.Next(instance.TownPatrolMinGangSize, instance.TownPatrolMaxGangSize), true);
            gangParty = GangParty;
            GangParty.IsVisible = false;
            EnterSettlementAction.ApplyForParty(GangParty, GangParty.HomeSettlement);
            return GangParty;
        }

        private void PatrolGangFight(bool insde)
        {
            GangFought = true;
            int upgradeLevel = Settlement.CurrentSettlement.Town.GetWallLevel();
            MobileParty GangParty = CreatePatrolGangParty();
            gangLeader = GangParty.Party.Owner;
            if (GangParty.CurrentSettlement == null)
            {
                GangParty.CurrentSettlement = Settlement.CurrentSettlement;
            }
                
            PlayerEncounter.RestartPlayerEncounter(GangParty.Party, PartyBase.MainParty, false);
            PlayerEncounter.Current.ForceAlleyFight = true;
            PlayerEncounter.StartBattle();
            int num = GangParty.MemberRoster.TotalManCount;

            if (insde)
            {
                CampaignMission.OpenAlleyFightMission(Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center").GetSceneName(upgradeLevel), upgradeLevel);
            }
            else
            {
                CampaignMission.OpenBattleMissionWhileEnteringSettlement(Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center").GetSceneName(upgradeLevel), upgradeLevel, Hero.MainHero.PartyBelongedTo.Party.MemberRoster.TotalHealthyCount, num);
                Hero.MainHero.AddSkillXp(DefaultSkills.Tactics, instance.TownPatrolLureTacticsXP);
            }
            GameMenu.ActivateGameMenu("town");
        }

        private Hero RandomGangLeaderInSettlement()
        {
            List<Hero> GangLeaders = new List<Hero>();
            foreach (Hero notable in Settlement.CurrentSettlement.Notables)
            {
                if (notable.IsGangLeader)
                {
                    GangLeaders.Add(notable);
                }
            }
            return GangLeaders[rng.Next(0, GangLeaders.Count)];
        }

        private void TakeMenuAction()
        {
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<Dictionary<Town, CampaignTime>>("_last_festival_time", ref LastFestival);
        }
    }
}
