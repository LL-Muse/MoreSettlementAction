using System;
using TaleWorlds.CampaignSystem.GameMenus;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace MoreSettlementAction
{
    [HarmonyPatch(typeof(KingdomManager), "SiegeCompleted")]
    class OnSettlementCapturePatch
    {
        private static void Postfix(Settlement settlement, MobileParty capturerParty, bool isWin, bool isSiege)
        {
  
            if (capturerParty == null || capturerParty.Leader == null || capturerParty.Leader.HeroObject == null ||capturerParty.Leader.HeroObject != Hero.MainHero)
            {
                return;
            }
            InformationManager.ShowInquiry(new InquiryData("Sack Settlement", "Do you want to sack the settlement?", true, true, "Sack it", "Leave it be", (Action)(() => {
                InformationManager.HideInquiry();
                GameMenu.SwitchToMenu("sack_settlement");
            }),(Action)null));
        }
    }
}