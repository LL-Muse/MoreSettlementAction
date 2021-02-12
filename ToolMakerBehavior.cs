using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.ObjectSystem;
using TaleWorlds.Localization;
using System.Reflection;
using System.Linq;

namespace MoreSettlementAction
{
    internal class ToolMakerBehavior : CampaignBehaviorBase
    {
        public static Dictionary<Town, ToolMaker> ToolMakers = new Dictionary<Town, ToolMaker>();
        public static bool ImportMaterials = false;

        private float _Duration;
        private CampaignTime _startTime;
        private float _upgrade_cost;

        private int woodImport = 0;
        private int ironImport = 0;

        Random rng;
        public override void RegisterEvents()
        {
            rng = new Random();
            CampaignEvents.DailyTickEvent.AddNonSerializedListener((object)this, new Action(this.DailyTick));
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, new Action(this.HourlyTick));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(ToolMakerMenuItems));
        }

        private void HourlyTick()
        {
            Town[] remove = new Town[ToolMakers.Count];
            int index = 0;
            foreach(var pair in ToolMakers)
            {
                if(MoreSettlementActionHelper.OwnsWorkshop(pair.Key, "smithy") == 0)
                {
                    remove[index] = pair.Key;
                }
                index++;
            }
            for(int i = 0; i < remove.Length; i++)
            {
                if(remove[i] != null)
                {
                    ToolMakers.Remove(remove[i]);
                }
            }
        }

        private void ToolMakerMenuItems(CampaignGameStarter campaignGameStarter)
        { 
            campaignGameStarter.AddGameMenuOption("town", "town_tool_maker", "Visit tool maker at your smithy", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (MoreSettlementActionHelper.OwnsWorkshop(Settlement.CurrentSettlement.Town, "smithy") == 0)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                args.IsEnabled = true;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => {
                GameMenu.SwitchToMenu("town_tool_maker_menu");
            }), index: 1);

            campaignGameStarter.AddGameMenu("town_tool_maker_menu", "The tool maker can turn various metal ingots into tools.  Smeltable items in the stockpile will be melted down if there are no ingots avalible.  Hardwood in stockpile will be refined into charcoal when there is not enough charcoal.  If the import material option is turned on iron ore and hardwood will be imported from owned slave estates producing iron ore and hardwood when none is left in the inventory of the tool maker but is avalible at the slave estates.  The import cost is 5 gold per unit delievered.  Building can be upgraded, with each level increasing building base production by 50%", (OnInitDelegate)(args => {
                ToolMaker toolMaker;
                if(!ToolMakers.TryGetValue(Settlement.CurrentSettlement.Town, out toolMaker))
                {
                    return;
                }
                MBTextManager.SetTextVariable("LEVEL", "(Current Level: " + toolMaker.UpgradeLevel.ToString() + ")", false);
            }), GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("town_tool_maker_menu", "tool_maker_upgrade", "Upgrade Buildings {LEVEL}", (GameMenuOption.OnConditionDelegate)(args =>
            {
                ToolMaker toolMaker;
                if(!ToolMakers.TryGetValue(Settlement.CurrentSettlement.Town, out toolMaker))
                {
                    return false;
                }
                int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"));
                string tooltipText = "";
                if (itemNumber < 100 * (toolMaker.UpgradeLevel + 2))
                {
                    args.IsEnabled = false;
                    tooltipText = tooltipText + "you lack the " + (100 * (toolMaker.UpgradeLevel + 2)).ToString() + " hardwood needed to upgrade to next level.  ";
                }
                else
                {
                    tooltipText = tooltipText + (100 * (toolMaker.UpgradeLevel + 2)).ToString() + " hardwood needed to upgrade to next level.  ";
                }
                if(toolMaker.UpgradeLevel == 0)
                {
                    tooltipText = tooltipText + "This currently has no bonuses";
                }
                else
                {
                    tooltipText = tooltipText + "This building has a " + (50 * toolMaker.UpgradeLevel).ToString() + "% bounus to base daily work rate";
                }
                args.Tooltip = new TextObject(tooltipText);
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                ToolMaker toolMaker;
                ToolMakers.TryGetValue(Settlement.CurrentSettlement.Town, out toolMaker);
                _startTime = CampaignTime.Now;
                int partySize = MobileParty.MainParty.Army == null ? MobileParty.MainParty.Party.NumberOfAllMembers : (int)MobileParty.MainParty.Army.TotalManCount;
                int engineeringSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Engineering);
                int partyEngineeringSum = MoreSettlementActionHelper.GetSkillTotalForParty(DefaultSkills.Engineering);
                _Duration = 1000 * (toolMaker.UpgradeLevel + 2);
                _upgrade_cost = 100 * (toolMaker.UpgradeLevel + 2);
                _Duration = _Duration / ((100 + engineeringSkill) / 100 * (partySize + (partyEngineeringSum / 100)));
                GameMenu.SwitchToMenu("tool_maker_upgrade_wait");
            }));

            campaignGameStarter.AddWaitGameMenu("tool_maker_upgrade_wait", "Your party is upgrading your toolmaker workshop", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => {
                ToolMaker toolMaker;
                ToolMakers.TryGetValue(Settlement.CurrentSettlement.Town, out toolMaker);
                PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), (int)(-1 * _upgrade_cost));
                toolMaker.UpgradeLevel++;
                InformationManager.DisplayMessage(new InformationMessage("Toolmaker workshop in the slave estate at " + Settlement.CurrentSettlement.Town.Name.ToString() + " has been upgraded to level " + toolMaker.UpgradeLevel.ToString()));

                GameMenu.SwitchToMenu("town_tool_maker_menu");
            }), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("tool_maker_upgrade_wait", "tool_maker_upgrade_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                InformationManager.DisplayMessage(new InformationMessage("You stopped builing before the job was done."));
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("town_tool_maker_menu", "town_tool_maker_menu_leave_companion", "Leave companion to assist tool maker", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                LeaveCompanion();
            }));

            campaignGameStarter.AddGameMenuOption("town_tool_maker_menu", "town_tool_maker_menu_take_companion", "Take companion to party", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                ToolMaker toolMaker;
                if (ToolMakers.TryGetValue(Settlement.CurrentSettlement.Town, out toolMaker))
                {
                    return toolMaker.Assistants.Count > 0;
                }
                else
                {
                    return false;
                }
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                TakeCompanion();
            }));

            campaignGameStarter.AddGameMenuOption("town_tool_maker_menu", "town_tool_maker_menu_manage_stockpile", "Manage Inventory", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                ToolMaker toolMaker;
                if (!ToolMakers.TryGetValue(Settlement.CurrentSettlement.Town, out toolMaker))
                {
                    toolMaker = new ToolMaker();
                    ToolMakers.Add(Settlement.CurrentSettlement.Town, toolMaker);
                }
                if (ToolMakers.TryGetValue(Settlement.CurrentSettlement.Town, out toolMaker))
                {
                    InventoryManager.OpenScreenAsStash(toolMaker.StockPile);
                }
            }));

            campaignGameStarter.AddGameMenuOption("town_tool_maker_menu", "town_tool_maker_menu_import_materials_on", "Import Materials : off", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (ImportMaterials)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                args.Tooltip = new TextObject("click to turn on");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                ImportMaterials = true;
                GameMenu.SwitchToMenu("town");
            }));

            campaignGameStarter.AddGameMenuOption("town_tool_maker_menu", "town_tool_maker_menu_import_materials_off", "Import Materials : on", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (!ImportMaterials)
                {
                    return false;
                }
                args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                args.Tooltip = new TextObject("click to turn off");
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                ImportMaterials = false;
                GameMenu.SwitchToMenu("town");
            }));

            campaignGameStarter.AddGameMenuOption("town_tool_maker_menu", "town_tool_maker_menu_leave", "Back", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("town");
            }));
        }

        private void LeaveCompanion()
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>();
            ToolMaker toolMaker;
            if (!ToolMakers.TryGetValue(Settlement.CurrentSettlement.Town, out toolMaker))
            {
                GameMenu.SwitchToMenu("town");
                return;
            }
            foreach (TroopRosterElement troop in Hero.MainHero.PartyBelongedTo.MemberRoster)
            {
                if (troop.Character.IsHero && troop.Character.HeroObject != Hero.MainHero)
                {
                    inquiryElements.Add(new InquiryElement((object)troop.Character.HeroObject, troop.Character.HeroObject.Name.ToString(), new ImageIdentifier(CharacterCode.CreateFrom((BasicCharacterObject)troop.Character.HeroObject.CharacterObject))));
                }
            }
            if (inquiryElements.Count < 1)
            {
                GameMenu.SwitchToMenu("town");
                return;
            }
            else
            {
                InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData("", "Select companions to leave at tool maker workshop", inquiryElements, true, 1000, "Continue", (string)null, (Action<List<InquiryElement>>)(args =>
                {
                    List<InquiryElement> source = args;
                    if (source != null && !source.Any<InquiryElement>())
                        return;
                    InformationManager.HideInquiry();

                    IEnumerable<Hero> selected = args.Select<InquiryElement, Hero>((Func<InquiryElement, Hero>)(element => element.Identifier as Hero));
                    foreach (Hero hero in selected)
                    {
                        hero.AddEventForOccupiedHero("tool_making");
                        toolMaker.Assistants.AddToCounts(hero.CharacterObject, 1);
                        Hero.MainHero.PartyBelongedTo.MemberRoster.AddToCounts(hero.CharacterObject, -1);
                    }
                    GameMenu.SwitchToMenu("town");
                }), (Action<List<InquiryElement>>)null));
            }
        }

        private void TakeCompanion()
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>();
            ToolMaker toolMaker;
            if (!ToolMakers.TryGetValue(Settlement.CurrentSettlement.Town, out toolMaker))
            {
                GameMenu.SwitchToMenu("town");
                return;
            }
            foreach (TroopRosterElement troop in toolMaker.Assistants)
            {
                if (troop.Character.IsHero)
                {
                    inquiryElements.Add(new InquiryElement((object)troop.Character.HeroObject, troop.Character.HeroObject.Name.ToString(), new ImageIdentifier(CharacterCode.CreateFrom((BasicCharacterObject)troop.Character.HeroObject.CharacterObject))));
                }
            }
            if (inquiryElements.Count < 1)
            {
                GameMenu.SwitchToMenu("town");
                return;
            }
            else
            {
                InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData("", "Select companions to take to party", inquiryElements, true, 1000, "Continue", (string)null, (Action<List<InquiryElement>>)(args =>
                {
                    List<InquiryElement> source = args;
                    if (source != null && !source.Any<InquiryElement>())
                        return;
                    InformationManager.HideInquiry();

                    IEnumerable<Hero> selected = args.Select<InquiryElement, Hero>((Func<InquiryElement, Hero>)(element => element.Identifier as Hero));
                    foreach (Hero hero in selected)
                    {
                        hero.RemoveEventFromOccupiedHero("tool_making");
                        toolMaker.Assistants.AddToCounts(hero.CharacterObject, -1);
                        Hero.MainHero.PartyBelongedTo.MemberRoster.AddToCounts(hero.CharacterObject, 1);
                    }
                    GameMenu.SwitchToMenu("town");
                }), (Action<List<InquiryElement>>)null));
            }
        }

        private void DailyTick()
        {
            foreach(KeyValuePair<Town, ToolMaker> pair in ToolMakers)
            {
                ToolMaker toolMaker = pair.Value;
                int total_xp = 0;
                removeDeadHeroes(toolMaker);
                Produce(toolMaker, (20 * (1 + toolMaker.Assistants.Count) + (getSmithingSum(toolMaker) / 5) + (10 * toolMaker.UpgradeLevel)), ref total_xp);
            }
        }

        private int getSmithingSum(ToolMaker toolMaker)
        {
            int sum = 0;
            foreach(TroopRosterElement troop in toolMaker.Assistants)
            {
                if (troop.Character.IsHero)
                {
                    sum += troop.Character.HeroObject.GetSkillValue(DefaultSkills.Crafting);
                }
            }
            return sum;
        }

        private void Produce(ToolMaker toolMaker, int WorkUnits, ref int total_xp)
        {
            if(WorkUnits > 1000)
            {
                WorkUnits = 1000;
            }

            if (WorkUnits == 0)
            {
                if(toolMaker.Assistants.Count > 0)
                {
                    int xp_per_character = total_xp / toolMaker.Assistants.Count;
                    foreach (TroopRosterElement troop in toolMaker.Assistants)
                    {
                        troop.Character.HeroObject.AddSkillXp(DefaultSkills.Crafting, xp_per_character);
                    }
                }
                if(woodImport != 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage(woodImport + " units of hardwood were delievered to the tool maker at " + toolMaker.Town.Name.ToString() + " at a cost of " + (woodImport * 5) + " <img src=\"Icons\\Coin@2x\">gold"));
                    woodImport = 0;
                }
                if(ironImport != 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage(ironImport + " units of iron ore were delievered to the tool maker at " + toolMaker.Town.Name.ToString() + " at a cost of " + (ironImport * 5) + " <img src=\"Icons\\Coin@2x\">gold"));
                    ironImport = 0;
                }
                return;
            }
            if (toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot3")) > 0 )
            {
                toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("tools"), 1);
                toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot3"), -1);
                total_xp += 30;
                Produce(toolMaker, WorkUnits - 1, ref total_xp );
                return;
            }
            else if(toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot2")) > 1 && toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("charcoal")) > 0)
            {
                toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot3"), 1);
                toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot1"), 1);
                toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot2"), -2);
                toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("charcoal"), -1);
                total_xp += 18;
                Produce(toolMaker, WorkUnits - 1, ref total_xp);
                return;
            }
            else if (toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot1")) > 0 && toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("charcoal")) > 0)
            {
                toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot2"), 1);
                toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot1"), -1);
                toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("charcoal"), -1);
                total_xp += 9;
                Produce(toolMaker, WorkUnits - 1, ref total_xp);
                return;
            }
            else if (toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("iron")) > 0 && toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("charcoal")) > 0)
            {
                bool efficentIronMakingPerk = false;
                foreach(TroopRosterElement troop in toolMaker.Assistants)
                {
                    if (troop.Character.HeroObject.GetPerkValue(DefaultPerks.Crafting.IronMaker))
                    {
                        efficentIronMakingPerk = true;
                        break;
                    }
                }
                if (efficentIronMakingPerk)
                {
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot1"), 3);
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("iron"), -1);
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("charcoal"), -1);
                    total_xp += 18;
                }
                else
                {
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot1"), 2);
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("iron"), -1);
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("charcoal"), -1);
                    total_xp += 12;
                }
                Produce(toolMaker, WorkUnits - 1, ref total_xp);
                return;
            }
            else if (HasSmeltableItem(toolMaker) && toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("charcoal")) > 0)
            {
                SmeltRandomItem(toolMaker, ref total_xp);
                Produce(toolMaker, WorkUnits - 1, ref total_xp);
                return;
            }
            else if (toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood")) > 1 && toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("charcoal")) == 0)
            {
                bool efficentCharcoalMakingPerk = false;
                foreach (TroopRosterElement troop in toolMaker.Assistants)
                {
                    if (troop.Character.HeroObject.GetPerkValue(DefaultPerks.Crafting.CharcoalMaker))
                    {
                        efficentCharcoalMakingPerk = true;
                        break;
                    }
                }
                if (efficentCharcoalMakingPerk)
                {
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("charcoal"), 3);
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), -2);
                    total_xp += 45;
                }
                else
                {
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("charcoal"), 1);
                    toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), -2);
                    total_xp += 15;
                }
                Produce(toolMaker, WorkUnits - 1, ref total_xp);
                return;
            }
            else if (ImportMaterials && canImportIron() && toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("iron")) == 0)
            {
                foreach (var pair in SlaveEstateBehavior.SlaveEstates)
                {
                    if (pair.Value.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("iron") && pair.Value.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("iron")) > 0)
                    {
                        Hero.MainHero.Gold -= 5;
                        ironImport++;
                        pair.Value.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("iron"), -1);
                        toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("iron"), 1);
                        break;
                    }
                }
                Produce(toolMaker, WorkUnits, ref total_xp);
                return;
            }
            else if (ImportMaterials && canImportWood() && toolMaker.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood")) <= 1)
            {
                foreach (var pair in SlaveEstateBehavior.SlaveEstates)
                {
                    if (pair.Value.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("hardwood") && pair.Value.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood")) > 1)
                    {
                        Hero.MainHero.Gold -= 10;
                        woodImport += 2;
                        pair.Value.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), -2);
                        toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), 2);
                        break;
                    }
                }
                Produce(toolMaker, WorkUnits, ref total_xp);
                return;
            }
            Produce(toolMaker, 0, ref total_xp);
        }

        private bool canImportWood()
        {
            foreach(var pair in SlaveEstateBehavior.SlaveEstates)
            {
                if(pair.Value.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("hardwood") && pair.Value.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("hardwood")) > 1 && Hero.MainHero.Gold >= 10)
                {
                    return true;
                }
            }
            return false;
        }

        private bool canImportIron()
        {
            foreach (var pair in SlaveEstateBehavior.SlaveEstates)
            {
                if (pair.Value.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("iron") && pair.Value.StockPile.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("iron")) > 0 && Hero.MainHero.Gold >= 5)
                {
                    return true;
                }
            }
            return false;
        }

        private void removeDeadHeroes(ToolMaker toolMaker)
        {
            foreach(TroopRosterElement troop in toolMaker.Assistants)
            {
                if (troop.Character.IsHero && troop.Character.HeroObject.HeroState == Hero.CharacterStates.Dead)
                {
                    toolMaker.Assistants.AddToCounts(troop.Character, -1);
                }
            }
        }

        private void SmeltRandomItem(ToolMaker toolMaker, ref int total_xp)
        {
            EquipmentElement element;
            foreach (ItemRosterElement item in toolMaker.StockPile)
            {
                if (IsSmeltable(item.EquipmentElement.Item))
                {
                    element = item.EquipmentElement;
                    break;
                }
            }
            DefaultSmithingModel SmithingModel = new DefaultSmithingModel();
            int[] MaterialAmounts = SmithingModel.GetSmeltingOutputForItem(element.Item);
            total_xp += SmithingModel.GetSkillXpForSmelting(element.Item);

            toolMaker.StockPile.AddToCounts(element, -1);
            toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot1"), MaterialAmounts[1]);
            toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot2"), MaterialAmounts[2]);
            toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot3"), MaterialAmounts[3]);
            toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot4"), MaterialAmounts[4]);
            toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot5"), MaterialAmounts[5]);
            toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("ironIngot6"), MaterialAmounts[6]);
            toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("hardwood"), MaterialAmounts[7]);
            toolMaker.StockPile.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("charcoal"), -1);
        }

        private bool HasSmeltableItem(ToolMaker toolMaker)
        {
            foreach(ItemRosterElement item in toolMaker.StockPile)
            {
                if (IsSmeltable(item.EquipmentElement.Item))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsSmeltable(ItemObject itemObject)
        {
            return (
                itemObject.Type == ItemObject.ItemTypeEnum.OneHandedWeapon ||
                itemObject.Type == ItemObject.ItemTypeEnum.Polearm ||
                itemObject.Type == ItemObject.ItemTypeEnum.Thrown ||
                itemObject.Type == ItemObject.ItemTypeEnum.TwoHandedWeapon);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<Dictionary<Town, ToolMaker>>("_tool_makers", ref ToolMakers);
            dataStore.SyncData<bool>("_tool_maker_import_materials", ref ImportMaterials);
        }
    }
}