using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;


namespace MoreSettlementAction
{
    [SaveableClass(1)]
    internal class SackSettlement
    {
        private Town _town;
        private CampaignTime _sackTime;

        public SackSettlement()
        {
            _town = Settlement.CurrentSettlement.Town;
            _sackTime = CampaignTime.Now;
        }

        [SaveableProperty(1)]
        public Town Town
        {
            get { return _town; }
            set { _town = value; }
        }

        [SaveableProperty(2)]
        public CampaignTime SackTime
        {
            get { return _sackTime; }
            set { _sackTime = value; }
        }
    }
}
