using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace MoreSettlementAction
{
    public class TownPrisonerUse
    {

        private Town _town;
        private TownPrisonerUse.PrisonerUse _prisoner_use;
        public TownPrisonerUse()
        {
            _town = Settlement.CurrentSettlement.Town;
            _prisoner_use = TownPrisonerUse.PrisonerUse.None;
        }

        [SaveableProperty(1)]
        public Town Town
        {
            get { return _town; }
            set { _town = value; }
        }

        [SaveableProperty(2)]
        public TownPrisonerUse.PrisonerUse Prisoner_Use
        {
            get { return _prisoner_use; }
            set { _prisoner_use = value; }
        }

        public enum PrisonerUse
        {
            None,
            Construction,
            Profit
        }
    }
}
