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
    internal class CastleOptionsBehavior : CampaignBehaviorBase
    {
        private MoreSettlementActionSettings instance = GlobalSettings<MoreSettlementActionSettings>.Instance;
        private Random rng;

        public override void RegisterEvents()
        {
            rng = new Random();

            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(CastleMenuItems));
        }

        private void CastleMenuItems(CampaignGameStarter campaignGameStarter)
        {
            MainCastleMenu(campaignGameStarter);

            CaptureEnemyLordInFactionSettlement(campaignGameStarter);
            CastleTransferSettlementMenuOption(campaignGameStarter);
            CastleAbandonSettlementMenuOption(campaignGameStarter);
        }

        private void CaptureEnemyLordInFactionSettlement(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddPlayerLine("capture_lord", "lord_start", "close_window", "Guards seize {?CONVERSATION_NPC.GENDER}her{?}him{\\?}", new ConversationSentence.OnConditionDelegate(captureLordCondition), new ConversationSentence.OnConsequenceDelegate(captureLordConsequence));
        }

        private void captureLordConsequence()
        {
            TakePrisonerAction.Apply(PartyBase.MainParty, Hero.OneToOneConversationHero);
        }

        private bool captureLordCondition()
        {
            return Hero.OneToOneConversationHero.PartyBelongedTo == null && Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.Town != null && Settlement.CurrentSettlement.OwnerClan.MapFaction == Hero.MainHero.MapFaction && Hero.MainHero.MapFaction.IsAtWarWith(Hero.OneToOneConversationHero.MapFaction) && !Hero.OneToOneConversationHero.IsPrisoner;
        }

        private void MainCastleMenu(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("castle", "ruler_castle", "Ruler Action", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (Settlement.CurrentSettlement.OwnerClan != Hero.MainHero.Clan)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("ruler_castle_menu")), index: 1);

            campaignGameStarter.AddGameMenu("ruler_castle_menu", "You can issue special grants and decrees here", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("ruler_castle_menu", "ruler_castle_menu_leave", "Back", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => GameMenu.SwitchToMenu("castle")));
        }

        private void CastleAbandonSettlementMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_castle_menu", "ruler_castle_abandon", "Abandon Rulership of Castle", (GameMenuOption.OnConditionDelegate)(args =>
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
                InformationManager.ShowInquiry(new InquiryData("Abandon Castle", "Are you sure you want to abandon this castle?", true, true, "Yes", "No", (Action)(() => {
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
                    Hero NewHero2 = HeroCreator.CreateSpecialHero(template2, Settlement.CurrentSettlement, null, Hero.MainHero.Clan, rng.Next(25, 30));
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
                        field.SetValue((object)clan, rng.Next(2, 4));
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
                    GameMenu.SwitchToMenu("castle");
                })), true);
            }), index: 1); ; ;
        }

        private void CastleTransferSettlementMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("ruler_castle_menu", "ruler_castle_transfer", "Transfer Ownership of Castle", (GameMenuOption.OnConditionDelegate)(args =>
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
                SubModule.ExecuteActionOnNextTick((Action)(() => InformationManager.ShowInquiry(new InquiryData("", "Transfer the castle of " + Settlement.CurrentSettlement.Town.Name + " to the " + (args.Select<InquiryElement, Clan>((Func<InquiryElement, Clan>)(element => element.Identifier as Clan))).First<Clan>().Name + " clan?", true, true, "Continue", "Cancle", (Action)(() => TransferSettlementAction((args.Select<InquiryElement, Clan>((Func<InquiryElement, Clan>)(element => element.Identifier as Clan))).First<Clan>())), (Action)(() => InformationManager.HideInquiry())))));
            }), (Action<List<InquiryElement>>)null));
        }

        private void TransferSettlementAction(Clan clan)
        {
            ChangeOwnerOfSettlementAction.ApplyByBarter(clan.Leader, Settlement.CurrentSettlement);
            if (Settlement.CurrentSettlement.IsTown)
            {
                ChangeRelationAction.ApplyPlayerRelation(clan.Leader, 40);
            }
            else if (Settlement.CurrentSettlement.IsCastle)
            {
                ChangeRelationAction.ApplyPlayerRelation(clan.Leader, 20);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}