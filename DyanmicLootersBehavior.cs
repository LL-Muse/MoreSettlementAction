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
    internal class DyanmicLootersBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener((object)this, new Action(DailyTick));
        }

        private void DailyTick()
        {
            float extraParties = 0;
            foreach(Settlement settlement in Campaign.Current.Settlements)
            {
                if (settlement.IsTown && settlement.Town.Security < 75)
                {
                    extraParties += (settlement.Town.Prosperity / 100) * ((100 - settlement.Town.Security) / 100);
                }
            }
            FieldInfo field = Campaign.Current.Models.BanditDensityModel.GetType().GetField("NumberOfMaximumLooterParties", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if ((FieldInfo)null != field)
            {
                field.SetValue((object)Campaign.Current.Models.BanditDensityModel, 300 + (int) extraParties);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}