using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using System.Collections.Generic;
using System;
using HarmonyLib;
using MCM.Abstractions.Settings.Base.Global;

namespace MoreSettlementAction
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly List<Action> ActionsToExecuteNextTick = new List<Action>();
        private MoreSettlementActionSettings instance = GlobalSettings<MoreSettlementActionSettings>.Instance;
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign))
                return;
            ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new VillageOptionsBehavior());
            ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new TownOptionsBehavior());
            ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new CastleOptionsBehavior());
            ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new SackSettlementBehavior());
            ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new NotableBehavior());

            if (instance.SlaveEstates)
            {
                ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new SlaveEstateBehavior());
            }

            if (instance.DyanmicBanditDensity)
            {
                ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new DyanmicLootersBehavior());
            }

            ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new ToolMakerBehavior());
        }

        public static void ExecuteActionOnNextTick(Action action)
        {
            if (action == null)
            {
                return;
            }
            SubModule.ActionsToExecuteNextTick.Add(action);
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            foreach (Action action in SubModule.ActionsToExecuteNextTick)
                action();
            SubModule.ActionsToExecuteNextTick.Clear();
        }
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("MoreSettlementAction").PatchAll();
        }
    }
}
