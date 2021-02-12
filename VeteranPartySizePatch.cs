using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;

namespace MoreSettlementAction
{
    [HarmonyPatch(typeof(DefaultPartySizeLimitModel), "GetPartyMemberSizeLimit")]
    class VeteranPartySizePatch
    {
        private static bool Prefix(PartyBase party, ref ExplainedNumber __result)
        {
            foreach(var pair in VillageOptionsBehavior.VeteranParties)
            {
                if(pair.Value == party.MobileParty)
                {
                    __result = new ExplainedNumber(0.0f);
                    __result.Add(10000f, new TaleWorlds.Localization.TextObject("Settled Veterans"));
                    return false;
                }
            }
            return true;
        }
    }
}