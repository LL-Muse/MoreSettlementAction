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
using MCM.Abstractions.Settings.Base.Global;
using System.Linq;
using System.Reflection;
namespace MoreSettlementAction
{
    internal class SlaveEstateBehavior : CampaignBehaviorBase
    {
        public static Dictionary<Village, SlaveEstate> SlaveEstates = new Dictionary<Village, SlaveEstate>();
        public static bool ClanPartiesEnslaveBandits = false;
        public static bool ToolDelivery = false;

        private CampaignTime _startTime;
        private float _Duration;
        private float _upgrade_cost;
        private SlaveEstateBuilding _upgrade_type;

        private Random rng;
        private MoreSettlementActionSettings instance = GlobalSettings<MoreSettlementActionSettings>.Instance; 
        public override void RegisterEvents()
        {
            rng = new Random();
            CampaignEvents.DailyTickEvent.AddNonSerializedListener((object)this, new Action(this.DailyTick));
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, new Action(this.HourlyTick));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(SlaveEstateMenuItems));
        }

        enum SlaveEstateBuilding
        {
            Surgeon,
            Overseer,
            ToolRepairer,
        }

        private void SlaveEstateMenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("village", "slave_village", "Slave Estates", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveTroops;
                args.IsEnabled = true;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                if (!SlaveEstates.ContainsKey(Settlement.CurrentSettlement.Village))
                {
                    GameMenu.SwitchToMenu("slave_village_buy_menu");
                }
                else
                {
                    GameMenu.SwitchToMenu("slave_village_manage_menu");
                }
            }), index: 1);

            campaignGameStarter.AddGameMenu("slave_village_buy_menu", "You can buy an estate from one of the wealthy landlord for " + instance.SlaveEstateCost + " gold. The main source labor is captured bandits used as force labor", (OnInitDelegate)(args => { }),GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("slave_village_buy_menu", "slave_village_buy_menu_buy", "Buy the estate", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (Hero.MainHero.Gold < instance.SlaveEstateCost)
                {
                    args.Tooltip = new TextObject("You need to " + instance.SlaveEstateCost + " gold");
                    args.IsEnabled = false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                Hero.MainHero.Gold -= instance.SlaveEstateCost;
                SlaveEstates.Add(Settlement.CurrentSettlement.Village, new SlaveEstate());
                GameMenu.SwitchToMenu("slave_village_manage_menu");
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_buy_menu", "slave_village_buy_menu_view_all", "View Summary of All Owned Estates", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if(SlaveEstates.Count == 0)
                {
                    args.IsEnabled = false;
                    args.Tooltip = new TextObject("You do not own any estates");
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                viewEstates();
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_buy_menu", "slave_village_buy_menu_leave", "Back", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenu("slave_village_manage_menu", "You can manage your estate from here.  If the clan party enslave bandits options is turn on all clan parties will send captured bandits to one of your owned slave estates instead of holding on to them for ransom.  If the tool delivery option is turn on, slaves estates that run out of tools will start using the stockpile of tools found in the tool maker's workshop given that you own one and tools are avalible in the tool makers' inventory. Note that a delivery fee of 5 gold will be applied for each tool delivered.  Only bandit prisoners can be use as slave until the forced labour perk (steward 200) is unlocked", (OnInitDelegate)(args => { }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_manage_menu_manage_slaves", "Manage Slaves", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Escape;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                PartyScreenLogic partyScreenLogic = new PartyScreenLogic();
                try
                {
                    FieldInfo field1 = PartyScreenManager.Instance.GetType().GetField("_currentMode", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field1 != (FieldInfo)null)
                    {
                        field1.SetValue((object)PartyScreenManager.Instance, (object)PartyScreenMode.Normal);
                    }
                    TroopRoster PrisonerTroopRoster = TroopRoster.CreateDummyTroopRoster();
                    TroopRoster GuardTroopRoster = TroopRoster.CreateDummyTroopRoster();
                    SlaveEstate slaveEstate;
                    if (SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate))
                    {
                        if( slaveEstate.Guards == null)
                        {
                            slaveEstate.Guards = TroopRoster.CreateDummyTroopRoster();
                        }
                        foreach (TroopRosterElement prisoner in slaveEstate.Prisoners)
                        {
                            PrisonerTroopRoster.AddToCounts(prisoner.Character, prisoner.Number);
                        }
                        foreach (TroopRosterElement guard in slaveEstate.Guards)
                        {
                            GuardTroopRoster.AddToCounts(guard.Character, guard.Number);
                        }
                        partyScreenLogic.Initialize(GuardTroopRoster, PrisonerTroopRoster, MobileParty.MainParty, true, new TextObject("Guards and Slaves"), 10000, new PartyPresentationDoneButtonDelegate(ManageDone), new TextObject("Manage Slaves"));
                        partyScreenLogic.InitializeTrade(PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable);
                        partyScreenLogic.SetTroopTransferableDelegate(new PartyScreenLogic.IsTroopTransferableDelegate(TroopTransferableDelegate));
                        PartyState state = Game.Current.GameStateManager.CreateState<PartyState>();
                        state.InitializeLogic(partyScreenLogic);
                        FieldInfo field2 = PartyScreenManager.Instance.GetType().GetField("_partyScreenLogic", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (field2 != (FieldInfo)null)
                        {
                            field2.SetValue((object)PartyScreenManager.Instance, (object)partyScreenLogic);
                        }
                        Game.Current.GameStateManager.PushState((GameState)state);
                    }
                }
                catch (Exception ex)
                {
                }
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_building", "Upgrade Buildings", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("slave_village_building_menu");
            }));

            campaignGameStarter.AddGameMenu("slave_village_building_menu", "Buildings can be upgraded to a max level of 3.  Upgrading to level 1 costs 200 hardwood and 2000 manhours, upgrading to level to level 2 costs 300 hardwood and 3000 manhours, and upgrading to level 3 costs 500 hardwood and 5000 manhours.  Each unit in your party will perform 1 manhour of work every hours so upgrading building is much faster with larger parties.  The surgeon house decrease daily slave death rate, the overseers house increases daily slave output, and the tool repair workshop decreases daily tool break chance", (OnInitDelegate)(args => {
                SlaveEstate slaveEstate;
                if(!SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate))
                {
                    return;
                }
                MBTextManager.SetTextVariable("SURGEONLEVEL", "(Current Level: " + slaveEstate.SurgeonLevel.ToString() + ")", false);
                MBTextManager.SetTextVariable("OVERSEERLEVEL", "(Current Level: " + slaveEstate.OverseerLevel.ToString() + ")", false);
                MBTextManager.SetTextVariable("TOOLREPAIRLEVEL", "(Current Level: " + slaveEstate.ToolRepairLevel.ToString() + ")", false);
            }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("slave_village_building_menu", "slave_village_building_surgeon", "Upgrade Surgeon's House {SURGEONLEVEL}", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                SlaveEstate slaveEstate;
                if (SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate) && slaveEstate.SurgeonLevel < 3)
                {
                    int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"));
                    if (slaveEstate.SurgeonLevel == 0)
                    {
                        if(itemNumber < 200)
                        {
                            args.IsEnabled = false;
                            args.Tooltip = new TextObject("You do not have the 200 hardwood to upgrade this building.  This building currently provides no bonuses.");
                        }
                        else
                        {
                            args.Tooltip = new TextObject("This building currently provides no bonuses");
                        }
                    }
                        
                    if (slaveEstate.SurgeonLevel == 1)
                    {
                        if (itemNumber < 300)
                        {
                            args.IsEnabled = false;
                            args.Tooltip = new TextObject("You do not have the 300 hardwood to upgrade this building.  This building provides a 20% decrease to slave death rate.");
                        }
                        else
                        {
                            args.Tooltip = new TextObject("This building provides a 20% decrease to slave death rate");
                        }
                    }
                    if (slaveEstate.SurgeonLevel == 2)
                    {
                        if (itemNumber < 500)
                        {
                            args.IsEnabled = false;
                            args.Tooltip = new TextObject("You do not have the 500 hardwood to upgrade this building.  This building provides a 40% decrease to slave death rate.");
                        }
                        else
                        {
                            args.Tooltip = new TextObject("This building provides a 40% decrease to slave death rate");
                        }
                    }
                }
                else
                {
                    args.Tooltip = new TextObject("This building is already max level.  It currently provides a 60% decrease to slave death rate");
                    args.IsEnabled = false;
                }
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                SlaveEstate slaveEstate;
                _upgrade_type = SlaveEstateBuilding.Surgeon;
                SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate);
                _startTime = CampaignTime.Now;
                int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
                int engineeringSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Engineering);
                int partyEngineeringSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Engineering);
                if(slaveEstate.SurgeonLevel == 0)
                {
                    _Duration = 2000;
                    _upgrade_cost = 200;
                }
                else if (slaveEstate.SurgeonLevel == 1)
                {
                    _Duration = 3000;
                    _upgrade_cost = 300;
                }
                if (slaveEstate.SurgeonLevel == 2)
                {
                    _Duration = 5000;
                    _upgrade_cost = 500;
                }
                _Duration = _Duration / ((100 + engineeringSkill)/100 * (partySize + (partyEngineeringSum / 100)));
                GameMenu.SwitchToMenu("slave_estate_upgrade_wait");
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_building_menu", "slave_village_building_overseer", "Upgrade Overseer's House {OVERSEERLEVEL}", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                SlaveEstate slaveEstate;
                if (SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate) && slaveEstate.OverseerLevel < 3)
                {
                    int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"));
                    if (slaveEstate.OverseerLevel == 0)
                    {
                        if (itemNumber < 200)
                        {
                            args.IsEnabled = false;
                            args.Tooltip = new TextObject("You do not have the 200 hardwood to upgrade this building.  This building currently provides no bonuses.");
                        }
                        else
                        {
                            args.Tooltip = new TextObject("This building currently provides no bonuses");
                        }
                    }

                    if (slaveEstate.OverseerLevel == 1)
                    {
                        if (itemNumber < 300)
                        {
                            args.IsEnabled = false;
                            args.Tooltip = new TextObject("You do not have the 300 hardwood to upgrade this building.  This building provides a increase to slave output");
                        }
                        else
                        {
                            args.Tooltip = new TextObject("This building provides a 20% increase to slave output");
                        }
                    }
                    if (slaveEstate.OverseerLevel == 2)
                    {
                        if (itemNumber < 500)
                        {
                            args.IsEnabled = false;
                            args.Tooltip = new TextObject("You do not have the 500 hardwood to upgrade this building.  This building provides a 40% increase to slave output");
                        }
                        else
                        {
                            args.Tooltip = new TextObject("This building provides a 40% increase to slave output");
                        }
                    }
                }
                else
                {
                    args.Tooltip = new TextObject("This building is already max level.  It currently provides a 60% increase to slave output");
                    args.IsEnabled = false;
                }
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                SlaveEstate slaveEstate;
                _upgrade_type = SlaveEstateBuilding.Overseer;
                SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate);
                _startTime = CampaignTime.Now;
                int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
                int engineeringSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Engineering);
                int partyEngineeringSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Engineering);
                if (slaveEstate.OverseerLevel == 0)
                {
                    _Duration = 2000;
                    _upgrade_cost = 200;
                }
                else if (slaveEstate.OverseerLevel == 1)
                {
                    _Duration = 3000;
                    _upgrade_cost = 300;
                }
                if (slaveEstate.OverseerLevel == 2)
                {
                    _Duration = 5000;
                    _upgrade_cost = 500;
                }
                _Duration = _Duration / ((100 + engineeringSkill) / 100 * (partySize + (partyEngineeringSum / 100)));
                GameMenu.SwitchToMenu("slave_estate_upgrade_wait");
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_building_menu", "slave_village_building_toolrepair", "Upgrade Tool repair workshop {TOOLREPAIRLEVEL}", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                SlaveEstate slaveEstate;
                if (SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate) && slaveEstate.ToolRepairLevel < 3)
                {
                    int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"));
                    if (slaveEstate.ToolRepairLevel == 0)
                    {
                        if (itemNumber < 200)
                        {
                            args.IsEnabled = false;
                            args.Tooltip = new TextObject("You do not have the 200 hardwood to upgrade this building.  This building currently provides no bonuses.");
                        }
                        else
                        {
                            args.Tooltip = new TextObject("This building currently provides no bonuses");
                        }
                    }

                    if (slaveEstate.ToolRepairLevel == 1)
                    {
                        if (itemNumber < 300)
                        {
                            args.IsEnabled = false;
                            args.Tooltip = new TextObject("You do not have the 300 hardwood to upgrade this building.  This building provides a 20% decrease to tool break rate.");
                        }
                        else
                        {
                            args.Tooltip = new TextObject("This building provides a 20% decrease to tool break rate");
                        }
                    }
                    if (slaveEstate.ToolRepairLevel == 2)
                    {
                        if (itemNumber < 500)
                        {
                            args.IsEnabled = false;
                            args.Tooltip = new TextObject("You do not have the 500 hardwood to upgrade this building.  This building provides a 40% decrease to tool break rate");
                        }
                        else
                        {
                            args.Tooltip = new TextObject("This building provides a 40% decrease to tool break rate");
                        }
                    }
                }
                else
                {
                    args.Tooltip = new TextObject("This building is already max level.  It currently provides a 60% decrease to tool break rate");
                    args.IsEnabled = false;
                }
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                SlaveEstate slaveEstate;
                _upgrade_type = SlaveEstateBuilding.ToolRepairer;
                SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate);
                _startTime = CampaignTime.Now;
                int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
                int engineeringSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Engineering);
                int partyEngineeringSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Engineering);
                if (slaveEstate.ToolRepairLevel == 0)
                {
                    _Duration = 2000;
                    _upgrade_cost = 200;
                }
                else if (slaveEstate.ToolRepairLevel == 1)
                {
                    _Duration = 3000;
                    _upgrade_cost = 300;
                }
                if (slaveEstate.ToolRepairLevel == 2)
                {
                    _Duration = 5000;
                    _upgrade_cost = 500;
                }
                _Duration = _Duration / ((100 + engineeringSkill) / 100 * (partySize + (partyEngineeringSum / 100)));
                GameMenu.SwitchToMenu("slave_estate_upgrade_wait");
            }));

            campaignGameStarter.AddWaitGameMenu("slave_estate_upgrade_wait", "Your party is upgrading your slave estate", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => {
                SlaveEstate slaveEstate;
                SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate);
                PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), (int)(-1 * _upgrade_cost));
                if (_upgrade_type == SlaveEstateBuilding.Surgeon)
                {
                    slaveEstate.SurgeonLevel++;
                    InformationManager.DisplayMessage(new InformationMessage("Surgeon's house in the slave estate at " + Settlement.CurrentSettlement.Village.Name.ToString() + " has been upgraded to level " + slaveEstate.SurgeonLevel.ToString()));
                }
                if (_upgrade_type == SlaveEstateBuilding.Overseer)
                {
                    slaveEstate.OverseerLevel++;
                    InformationManager.DisplayMessage(new InformationMessage("Overseer's house in the slave estate at " + Settlement.CurrentSettlement.Village.Name.ToString() + " has been upgraded to level " + slaveEstate.OverseerLevel.ToString()));
                }
                if (_upgrade_type == SlaveEstateBuilding.ToolRepairer)
                {
                    slaveEstate.ToolRepairLevel++;
                    InformationManager.DisplayMessage(new InformationMessage("Tool repair workshop in the slave estate at " + Settlement.CurrentSettlement.Village.Name.ToString() + " has been upgraded to level " + slaveEstate.ToolRepairLevel.ToString()));
                }
                GameMenu.SwitchToMenu("slave_village_building_menu");
            }), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("slave_estate_upgrade_wait", "slave_estate_upgrade_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped builing before the job was done."));
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_building_menu", "slave_village_building_menu_leave", "Back", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("slave_village_manage_menu"); 
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_manage_menu_manage_stockpile", "Manage Stockpile", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                SlaveEstate slaveEstate;
                if (SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate))
                {
                    InventoryManager.OpenScreenAsStash(slaveEstate.StockPile);
                }
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_manage_menu_view_all", "View Summary of All Owned Estates", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (SlaveEstates.Count == 0)
                {
                    args.IsEnabled = false;
                    args.Tooltip = new TextObject("You do not own any estates");
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                viewEstates();
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_manage_menu_sell", "Sell Slave Estate", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.ShowInquiry(new InquiryData("Sell Slave Estate", "Sell slave estate for " + (0.8 * instance.SlaveEstateCost).ToString("0") + " gold", true, true, "Sell", "Cancle", (Action) (() => {
                    Hero.MainHero.Gold += (int) (instance.SlaveEstateCost * 0.8);
                    SlaveEstates.Remove(Settlement.CurrentSettlement.Village);
                    GameMenu.SwitchToMenu("village");
                }),(Action) (() => { 
                })));
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_manage_menu_enslave_on", "Clan parties enslave bandits : off", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (ClanPartiesEnslaveBandits)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Escape;
                args.Tooltip = new TextObject("click to turn on");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                ClanPartiesEnslaveBandits = true;
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_manage_menu_enslave_off", "Clan parties enslave bandits : on", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (!ClanPartiesEnslaveBandits)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Escape;
                args.Tooltip = new TextObject("click to turn off");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                ClanPartiesEnslaveBandits = false;
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_manage_menu_tool_delivery_on", "Tool Delivery : off", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (ToolDelivery)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                args.Tooltip = new TextObject("click to turn on");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                ToolDelivery = true;
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_manage_menu_tool_delivery_off", "Tool Delivery : on", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (!ToolDelivery)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                args.Tooltip = new TextObject("click to turn off");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                ToolDelivery = false;
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("slave_village_manage_menu", "slave_village_manage_menu_leave", "Back", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("village");
            }));

        }

        private void viewEstates()
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>();
            foreach(var pair in SlaveEstates)
            {
                SlaveEstate slaveEstate = pair.Value;
                int index = slaveEstate.StockPile.FindIndexOfItem(slaveEstate.PrimaryProduction);
                int productcount = (index == -1 ? 0 : slaveEstate.StockPile.GetElementCopyAtIndex(index).Amount);
                inquiryElements.Add(new InquiryElement((object)slaveEstate, slaveEstate.Village.Name.ToString() + " : " + prisonerCount(slaveEstate.Prisoners) + " slaves, stockpile : " + productcount, new ImageIdentifier(slaveEstate.PrimaryProduction)));
            }
            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData("Owned Slave Estates", "", inquiryElements, true, 1, "Logs", (string)null, (Action<List<InquiryElement>>)(args =>
            {
                (args.Select<InquiryElement, SlaveEstate>((Func<InquiryElement, SlaveEstate>)(element => element.Identifier as SlaveEstate))).First<SlaveEstate>().Display();
            }), (Action<List<InquiryElement>>)null));
        }

        private bool TroopTransferableDelegate(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
        {
            if (character.IsHero)
            {
                return false;
            }

            if (type == PartyScreenLogic.TroopType.Member)
            {
                return true;
            } 
            else if (type == PartyScreenLogic.TroopType.Prisoner)
            {
                return character.Occupation == Occupation.Bandit || Hero.MainHero.GetPerkValue(DefaultPerks.Steward.ForcedLabor);
            }
            return false;
        }

        private bool ManageDone(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster,
          bool isForced, List<MobileParty> leftParties = null, List<MobileParty> rightParties = null)
        {
            SlaveEstate slaveEstate;
            SlaveEstates.TryGetValue(Settlement.CurrentSettlement.Village, out slaveEstate);
            slaveEstate.Prisoners = leftPrisonRoster;
            slaveEstate.Guards = leftMemberRoster;

            if(3 * (CalculateStrength(leftMemberRoster) + (Settlement.CurrentSettlement.MilitaParty == null ? 0 : CalculateStrength(Settlement.CurrentSettlement.MilitaParty.MemberRoster))) < CalculateStrength(leftPrisonRoster))
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficent Guards", "The combined strength of the guards and milita (" + Math.Floor(3 * (CalculateStrength(leftMemberRoster) + (Settlement.CurrentSettlement.MilitaParty == null ? 0 : CalculateStrength(Settlement.CurrentSettlement.MilitaParty.MemberRoster)))).ToString() + ") is too weak to prevent a slave uprising (slave strength: " + Math.Floor(CalculateStrength(leftPrisonRoster)).ToString() + ")  Consider leaving more troops as guards", true, false, "Okay", "", (Action)null, (Action)null));
            }

            return true;
        }
        private float CalculateStrength(TroopRoster troops)
        {
            float ret = 0.0f;
            foreach (TroopRosterElement troop in troops)
            {
                 ret += (float)(troop.Number - troop.WoundedNumber) * Campaign.Current.Models.MilitaryPowerModel.GetTroopPowerBasedOnContext(troop.Character);
            }
            return ret;
        }
        private void DailyTick()
        {
            SlaveEstateDailyProduction();
            SlaveRebellion();
        }

        private void SlaveRebellion()
        {
            foreach (var pair in SlaveEstates)
            {
                Village village = pair.Key;
                SlaveEstate slaveEstate = pair.Value;
                if (slaveEstate.Guards == null)
                {
                    slaveEstate.Guards = TroopRoster.CreateDummyTroopRoster();
                }
                if (3 * (CalculateStrength(slaveEstate.Guards) + (village.Settlement.MilitaParty == null ? 0 : CalculateStrength(village.Settlement.MilitaParty.MemberRoster))) < CalculateStrength(slaveEstate.Prisoners))
                {
                    StartSlaveRebellion(village, slaveEstate);
                }
            }
        }

        private void StartSlaveRebellion(Village village, SlaveEstate slaveEstate)
        {
            MobileParty SlaveParty = MBObjectManager.Instance.CreateObject<MobileParty>("slave_revolt_" + village.Name.ToString().ToLower());

            SlaveParty.InitializeMobileParty(new TroopRoster(SlaveParty.Party), new TroopRoster(SlaveParty.Party), village.Settlement.Position2D, 1f);
            SlaveParty.SetCustomName(new TextObject(village.Name.ToString() + " slave revolt"));
            SlaveParty.HomeSettlement = village.Settlement;
            CharacterObject leader = slaveEstate.Prisoners.GetRandomElement().Character;
            slaveEstate.Prisoners.AddToCounts(leader, -1);
            Hero leaderHero = HeroCreator.CreateSpecialHero(leader, faction: Clan.BanditFactions.First<Clan>(), bornSettlement: village.Settlement, age: (int)leader.Age) ;
            leaderHero.NeverBecomePrisoner = true;
            leaderHero.AlwaysDie = true;
            leaderHero.SetSkillValue(DefaultSkills.Leadership, 150);
            leaderHero.SetSkillValue(DefaultSkills.Steward, 1000);
            leaderHero.SetPerkValue(DefaultPerks.Leadership.VeteransRespect, true);
            SlaveParty.Party.Owner = leaderHero;
            SlaveParty.MemberRoster.AddToCounts(leaderHero.CharacterObject, 1);
            int partyMaxSize = rng.Next(75, 175);
            while(SlaveParty.MemberRoster.TotalManCount < partyMaxSize && slaveEstate.Prisoners.TotalManCount > 0)
            {
                CharacterObject randomSlave = slaveEstate.Prisoners.GetRandomElement().Character;
                slaveEstate.Prisoners.AddToCounts(randomSlave, -1);
                SlaveParty.MemberRoster.AddToCounts(randomSlave, 1);
            }

            InformationManager.ShowInquiry(new InquiryData("Slave Revolt", "The slave at your estate in the village of " + village.Name.ToString() + " have revolted.  They are lead by a former slave named " + leaderHero.Name.ToString() + ".  Their party numbers " + SlaveParty.MemberRoster.TotalManCount.ToString() + " escaped slave.  The rebellious slaves are currently raiding the village of " + village.Name.ToString(), true, false, "Not Good", "", (Action)null, (Action)null));

            SlaveParty.SetMoveRaidSettlement(village.Settlement);
            SlaveParty.Ai.SetAIState(AIState.Raiding);
        }

        private void HourlyTick() 
        { 
            ClanPartySendBanditsToSlaveEstate();
        }

        private void ClanPartySendBanditsToSlaveEstate()
        {
            if (!ClanPartiesEnslaveBandits || SlaveEstates.Count < 1)
            {
                return;
            }
            List<SlaveEstate> posibileTarget = new List<SlaveEstate>();
            foreach(var pair in SlaveEstates)
            {
                posibileTarget.Add(pair.Value);
            }
            foreach(MobileParty party in Hero.MainHero.Clan.WarParties)
            {
                if (party != MobileParty.MainParty && HasBanditPrisoners(party))
                {
                    TransferBanditSlavesFromWarpartyToSlaveEstate(party, posibileTarget[rng.Next(0, posibileTarget.Count - 1)]);
                }
            }
        }

        private void TransferBanditSlavesFromWarpartyToSlaveEstate(MobileParty party, SlaveEstate slaveEstate)
        {
            int count = 0;
            foreach(TroopRosterElement prison in party.PrisonRoster)
            {
                if(prison.Character.Occupation == Occupation.Bandit)
                {
                    count += prison.Number;
                    slaveEstate.Prisoners.AddToCounts(prison.Character, prison.Number);
                    party.PrisonRoster.AddToCounts(prison.Character, -1 * prison.Number);
                }
            }
            InformationManager.DisplayMessage(new InformationMessage(party.Leader.Name.ToString() + " has transfered " + count + " slaves to the slave estate at " + slaveEstate.Village.Name.ToString()));
        }

        private bool HasBanditPrisoners(MobileParty party)
        {
            foreach(TroopRosterElement prisoner in party.PrisonRoster)
            {
                if(prisoner.Character.Occupation == Occupation.Bandit)
                {
                    return true;
                }
            }
            return false;
        }

        private void SlaveEstateDailyProduction()
        {
            foreach (var pair in SlaveEstates)
            {
                SlaveEstate slaveEstate = pair.Value;
                ItemRoster UpdatedStockPile = new ItemRoster();
                TroopRoster UpdatedPrisoners = TroopRoster.CreateDummyTroopRoster();
                foreach (TroopRosterElement prisoner in slaveEstate.Prisoners)
                {
                    UpdatedPrisoners.AddToCounts(prisoner.Character, prisoner.Number);
                }
                foreach (ItemRosterElement item in slaveEstate.StockPile)
                {
                    UpdatedStockPile.AddToCounts(item.EquipmentElement.Item, item.Amount);
                }
                Tuple<int, int> tuple = UpdateStockPile(slaveEstate, ref UpdatedStockPile, slaveEstate.Village, prisonerCount(UpdatedPrisoners));
                int unitsProduced = tuple.Item1;
                int toolsUsed = tuple.Item2;
                if (slaveEstate.PrimaryProduction.Type == ItemObject.ItemTypeEnum.Horse)
                {
                    produceHorses(slaveEstate, ref UpdatedStockPile, unitsProduced);
                }
                else if (slaveEstate.PrimaryProduction.Type == ItemObject.ItemTypeEnum.Animal)
                {
                    produceLivestock(slaveEstate, ref UpdatedStockPile, unitsProduced);
                }
                else
                {
                    UpdatedStockPile.AddToCounts(slaveEstate.PrimaryProduction, unitsProduced);
                }

                Hero.MainHero.AddSkillXp(DefaultSkills.Trade, instance.SlaveEstateTradeXP * unitsProduced);
                int slavesDied = PrisonerDeathes(slaveEstate, ref UpdatedPrisoners);
                slaveEstate.Prisoners = UpdatedPrisoners;
                slaveEstate.StockPile = UpdatedStockPile;
                slaveEstate.AddToLogs(slavesDied, unitsProduced, toolsUsed);
            }
        }

        private void produceLivestock(SlaveEstate slaveEstate, ref ItemRoster UpdatedStockPile, int unitsProduced)
        {
            if (unitsProduced == 0)
            {
                return;
            }
            if (UpdatedStockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("grain")) >= 1 && slaveEstate.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("hog"))
            {
                UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), -1);
                UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hog"), 1);
                produceLivestock(slaveEstate, ref UpdatedStockPile, unitsProduced - 1);
            }
            else if (UpdatedStockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("grain")) >= 2 && slaveEstate.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("sheep"))
            {
                UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), -2);
                UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("sheep"), 1);
                produceLivestock(slaveEstate, ref UpdatedStockPile, unitsProduced - 1);
            }
            else if (UpdatedStockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("grain")) >= 4 && slaveEstate.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("cow"))
            {
                UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), -4);
                UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("cow"), 1);
                produceLivestock(slaveEstate, ref UpdatedStockPile, unitsProduced - 1);
            }
            else
            {
                UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), 1);
                produceLivestock(slaveEstate, ref UpdatedStockPile, unitsProduced - 1);
            }
        }

        private void produceHorses(SlaveEstate slaveEstate, ref ItemRoster UpdatedStockPile, int unitsProduced)
        {
            if(unitsProduced == 0)
            {
                return;
            }
            if (UpdatedStockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("grain")) >= 5)
            {
                UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), -5);
                int RNGResult = rng.Next(0, 100);
                if (RNGResult >= 90)
                {
                    if(slaveEstate.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("aserai_horse"))
                    {
                        UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("t2_aserai_horse"), 1);
                    }
                    else if (slaveEstate.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("battania_horse"))
                    {
                        UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("t2_battania_horse"), 1);
                    }
                    else if (slaveEstate.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("empire_horse"))
                    {
                        UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("t2_empire_horse"), 1);
                    }
                    else if (slaveEstate.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("khuzait_horse"))
                    {
                        UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("t2_khuzait_horse"), 1);
                    }
                    else if (slaveEstate.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse"))
                    {
                        UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("t2_vlandia_horse"), 1);
                    }
                    else if (slaveEstate.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("sturgia_horse"))
                    {
                        UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("t2_sturgia_horse"), 1);
                    }
                    else 
                    {
                        UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse"), 1);
                    }
                }
                else if(RNGResult >= 65)
                {
                    UpdatedStockPile.AddToCounts(slaveEstate.PrimaryProduction, 1);
                }
                else if(RNGResult >= 30)
                {
                    UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse"), 1);
                }
                else
                {
                    UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("mule"), 1);
                }
                produceHorses(slaveEstate, ref UpdatedStockPile, unitsProduced - 1);
            }
            else
            {
                UpdatedStockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), 1);
                produceHorses(slaveEstate, ref UpdatedStockPile, unitsProduced - 1);
            }
        }

        private Tuple<int,int> UpdateStockPile(SlaveEstate slaveEstate, ref ItemRoster stockpile, Village village, int workUnit)
        {
            int count = 0;
            int tools_count = 0;
            int tools_deliver = 0;
            ItemObject tools = MBObjectManager.Instance.GetObject<ItemObject>("tools");
            for (int i = 0; i < workUnit; i++)
            {
                if(stockpile.FindIndexOfItem(tools) != -1 && stockpile.GetElementCopyAtIndex(stockpile.FindIndexOfItem(tools)).Amount > 0)
                {
                    count++;
                    if (rng.Next(0, 100) > 20 * slaveEstate.OverseerLevel)
                    {
                        count++;
                    }
                    if (rng.Next(0, 100) < instance.ToolBreakChance)
                    {
                        if (rng.Next(0, 100) > 20 * slaveEstate.ToolRepairLevel)
                        {
                            stockpile.AddToCounts(tools, -1);
                            tools_count++;
                        }
                    }
                }
                else if (ToolDelivery && HasToolsInToolmakerWorkshop() && Hero.MainHero.Gold >= 5)
                {
                    deliverTool();
                    stockpile.AddToCounts(tools, 1);
                    tools_deliver++;
                    Hero.MainHero.Gold -= 5;
                    if (rng.Next(0, 100) < instance.ToolBreakChance)
                    {
                        if(rng.Next(0,100) > 20 * slaveEstate.ToolRepairLevel)
                        {
                            stockpile.AddToCounts(tools, -1);
                            tools_count++;
                        }
                    }
                    count++;
                    if (rng.Next(0, 100) > 20 * slaveEstate.OverseerLevel)
                    {
                        count++;
                    }
                }
                else
                {
                    if(rng.Next(0,100) < 15)
                    {
                        count++;
                        if (rng.Next(0, 100) > 20 * slaveEstate.OverseerLevel)
                        {
                            count++;
                        }
                    }
                }
            }
            if(tools_deliver > 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(tools_deliver + " units of tools were delievered to the slave estate at " + village.Name.ToString() + " at a cost of " + (tools_deliver * 5) + " <img src=\"Icons\\Coin@2x\">gold"));
            }
            return new Tuple<int,int>(count, tools_count);
        }

        private void deliverTool()
        {
            foreach (var pair in ToolMakerBehavior.ToolMakers)
            {
                if (pair.Value.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) >= 1)
                {
                    pair.Value.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("tools"), -1);
                    return;
                }
            }
        }

        private bool HasToolsInToolmakerWorkshop()
        {
            foreach(var pair in ToolMakerBehavior.ToolMakers)
            {
                if (pair.Value.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("tools")) >= 1)
                {
                    return true;
                }
            }
            return false;
        }

        private int prisonerCount(TroopRoster prisoners)
        {
            int ret = 0;
            foreach (TroopRosterElement prisoner in prisoners)
            {
                ret += prisoner.Number;
            }
            return ret;
        }

        private int PrisonerDeathes(SlaveEstate slaveEstate ,ref TroopRoster prisoners)
        {
            int deathes = 0;
            foreach(TroopRosterElement prisoner in prisoners)
            {
                for(int i = 0; i < prisoner.Number; i++)
                {
                    if(rng.Next(0,100) < instance.SlaveDailyDeathRate / Math.Max(1, prisoner.Character.Tier))
                    {
                        if(rng.Next(0,100) >= slaveEstate.SurgeonLevel * 20)
                        {
                            prisoners.AddToCounts(prisoner.Character, -1);
                            deathes++;
                        }
                    }
                }
            }
            return deathes;
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<Dictionary<Village, SlaveEstate>>("_slave_estates", ref SlaveEstateBehavior.SlaveEstates);
            dataStore.SyncData<bool>("_clan_party_enslave_bandits", ref ClanPartiesEnslaveBandits);
            dataStore.SyncData<bool>("_slave_estate_tool_delivery", ref ToolDelivery);
        }
    }
}
