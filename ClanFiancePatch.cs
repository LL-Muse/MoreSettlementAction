using TaleWorlds.Localization;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MoreSettlementAction
{
    [HarmonyPatch(typeof(DefaultClanFinanceModel), "CalculateClanIncomeInternal")]
    class ClanFiancePatch
    {
        private static void Postfix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
        {
            foreach(var pair in SlaveEstateBehavior.SlaveEstates)
            {
                int wage = 0;
                SlaveEstate slaveEstate = pair.Value;
                Village village = pair.Key;
                if(slaveEstate.Guards == null)
                {
                    slaveEstate.Guards = TroopRoster.CreateDummyTroopRoster();
                }
                else
                {
                    foreach(TroopRosterElement troop in slaveEstate.Guards)
                    {
                        wage += troop.Number * troop.Character.TroopWage;
                    }
                }

                if(wage > 0)
                {
                    goldChange.Add(-1 * wage, new TextObject("Wage of slave estate guards at " + village.Name.ToString()));
                }
            }
        }
    }
}