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

namespace MoreSettlementAction
{
    internal class SackSettlementBehavior : CampaignBehaviorBase
    {

        public static List<SackSettlement> sackSettlements = new List<SackSettlement>();
        private float ProsperityBeforeRazed = 0;
        private Random rng;

        public void UpdateSackSettlement()
        {
            bool hasTown = false;
            foreach(SackSettlement sackSettlement in sackSettlements)
            {
                if (sackSettlement.Town == Settlement.CurrentSettlement.Town)
                {
                    sackSettlement.SackTime = CampaignTime.Now;
                    hasTown = true;
                }
            }
            if (!hasTown)
            {
                sackSettlements.Add(new SackSettlement());
            }
        }
        public override void RegisterEvents()
        {
            rng = new Random();

            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(MenuOption)); 
        }

        private void MenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu("sack_settlement", "Sacking will cause the settlement to lose 1 building level for each building and the settlement to lose half of its prosperity.  Razing the settlement will cause the settlement to lose all buildings and reset prosperity to 0.  A large amount of wealth is gained by sacking and an even larger amount is gained by razing.  Looted settlements will have loyalty problems for years to come", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("sack_settlement", "sack_settlement_sack", "Sack it", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Raid;
                args.IsEnabled = true;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                UpdateSackSettlement();
                Settlement.CurrentSettlement.Town.Loyalty = 0.0f;
                float prosperitySacked = Settlement.CurrentSettlement.Prosperity / 2.0f;
                Settlement.CurrentSettlement.Prosperity -= prosperitySacked;
                float buildingRazeCost = 0.0f;
                foreach(Building building in Settlement.CurrentSettlement.Town.Buildings)
                {
                    if(building.CurrentLevel > 0)
                    {
                        building.CurrentLevel--;
                        buildingRazeCost += building.GetConstructionCost();
                    }
                    if (building.GetConstructionCost() == 0)
                    {
                        building.CurrentLevel++;
                    }
                }
                int goldSacked = (int) (5 * buildingRazeCost + 50 * prosperitySacked);
                Hero.MainHero.Gold += goldSacked;
                InformationManager.DisplayMessage(new InformationMessage("Your troops looted " + goldSacked.ToString() + " <img src=\"Icons\\Coin@2x\">gold"));
                if (Settlement.CurrentSettlement.IsTown)
                {
                    GameMenu.SwitchToMenu("town");
                }
                else if (Settlement.CurrentSettlement.IsCastle)
                {
                    GameMenu.SwitchToMenu("castle");
                }
            }), index: 1);

            campaignGameStarter.AddGameMenuOption("sack_settlement", "sack_settlement_raze", "Raze it", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                args.IsEnabled = true;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                UpdateSackSettlement();
                Settlement.CurrentSettlement.Town.Loyalty = 0.0f;
                float prosperitySacked = Settlement.CurrentSettlement.Prosperity;
                ProsperityBeforeRazed = Settlement.CurrentSettlement.Prosperity;
                Settlement.CurrentSettlement.Prosperity = 0;
                float buildingRazeCost = 0.0f;
                foreach (Building building in Settlement.CurrentSettlement.Town.Buildings)
                {
                    while (building.CurrentLevel > 0)
                    {
                        building.CurrentLevel--;
                        buildingRazeCost += building.GetConstructionCost();
                    }
                    if(building.GetConstructionCost() == 0)
                    {
                        building.CurrentLevel++;
                    }
                }
                int goldSacked = (int)(5 * buildingRazeCost + 50 * prosperitySacked);
                Hero.MainHero.Gold += goldSacked;
                InformationManager.DisplayMessage(new InformationMessage("Your troops looted " + goldSacked.ToString() + " <img src=\"Icons\\Coin@2x\">gold"));
                GameMenu.SwitchToMenu("sack_settlement_captives");
            }), index: 2);

            campaignGameStarter.AddGameMenuOption("sack_settlement", "sack_settlement_occupy", "Never Mind", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                args.IsEnabled = true;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                GameMenu.SwitchToMenu("sack_settlement_captives");
                if (Settlement.CurrentSettlement.IsTown)
                {
                    GameMenu.SwitchToMenu("town");
                }
                else if (Settlement.CurrentSettlement.IsCastle)
                {
                    GameMenu.SwitchToMenu("castle");
                }
            }), index: 3);

            campaignGameStarter.AddGameMenu("sack_settlement_captives", "With the Settlement reduced to rubble, the question comes to mind of what to do with the displaced locals.  Those that are still alives.", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("sack_settlement_captives", "sack_settlement_captives_enslave", "Send them to the slave markets", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveTroops;
                args.IsEnabled = true;
                args.Tooltip = new TextObject("gain gold");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                int gold = (int) (ProsperityBeforeRazed * 10);
                Hero.MainHero.Gold += gold;
                InformationManager.DisplayMessage(new InformationMessage("The citizens of " + Settlement.CurrentSettlement.Name.ToString() + " sold for " + gold.ToString() + " <img src=\"Icons\\Coin@2x\">gold on the slave market"));
                if (Settlement.CurrentSettlement.IsTown)
                {
                    GameMenu.SwitchToMenu("town");
                }
                else if (Settlement.CurrentSettlement.IsCastle)
                {
                    GameMenu.SwitchToMenu("castle");
                }
            }), index: 1);

            campaignGameStarter.AddGameMenuOption("sack_settlement_captives", "sack_settlement_captives_murder", "Make them into piles of skulls", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                args.IsEnabled = true;
                args.Tooltip = new TextObject("gain renown");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                int renownGained = (int) ProsperityBeforeRazed / 50;
                Hero.MainHero.Clan.Renown += renownGained;
                InformationManager.DisplayMessage(new InformationMessage("News of your gruelsome deeds spread through out all of Calradia (" + renownGained + " renown gained)"));
                if (Settlement.CurrentSettlement.IsTown)
                {
                    GameMenu.SwitchToMenu("town");
                }
                else if (Settlement.CurrentSettlement.IsCastle)
                {
                    GameMenu.SwitchToMenu("castle");
                }
            }), index: 1);

            campaignGameStarter.AddGameMenuOption("sack_settlement_captives", "sack_settlement_captives_resettle", "Resettle the useful ones in one of my towns", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                bool ownsTowns = false;
                foreach(Fief fief in Hero.MainHero.Clan.Fiefs)
                {
                    if (fief.Settlement.IsTown)
                    {
                        ownsTowns = true;
                    }
                }
                args.IsEnabled = ownsTowns;
                if (ownsTowns)
                {
                    args.Tooltip = new TextObject("gain prosperity in an owned town");
                }
                else
                {
                    args.Tooltip = new TextObject("your clan does not own any towns");
                }
                
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                List<Town> towns = new List<Town>();
                foreach (Fief fief in Hero.MainHero.Clan.Fiefs)
                {
                    if (fief.Settlement.IsTown)
                    {
                        towns.Add(fief.Settlement.Town);
                    }
                }
                Town Selected = towns[rng.Next(0, towns.Count - 1)];
                int prosperityGain = (int) (ProsperityBeforeRazed / 10);
                Selected.Settlement.Prosperity += prosperityGain;
                InformationManager.DisplayMessage(new InformationMessage("The Town of " + Selected.Name.ToString() + " gained " + prosperityGain + " prosperity from the specialists resettled there"));
                if (Settlement.CurrentSettlement.IsTown)
                {
                    GameMenu.SwitchToMenu("town");
                }
                else if (Settlement.CurrentSettlement.IsCastle)
                {
                    GameMenu.SwitchToMenu("castle");
                }
            }), index: 1);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<SackSettlement>>("_sacked_settlements", ref sackSettlements);
        }
    }
}