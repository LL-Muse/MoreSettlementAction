using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace MoreSettlementAction
{   
    internal class ToolMaker
    {  
        private TroopRoster _assistants;
        private ItemRoster _stockpile;
        private Town _town;
        private int _upgrade_level;
        public ToolMaker()
        {
            _town = Settlement.CurrentSettlement.Town;
            _assistants = TroopRoster.CreateDummyTroopRoster();
            _stockpile = new ItemRoster();
        }

        [SaveableProperty(1)]
        public ItemRoster StockPile
        {
            get { return _stockpile; }
            set { _stockpile = value; }
        }

        [SaveableProperty(2)]
        public TroopRoster Assistants
        {
            get { return _assistants; }
            set { _assistants = value; }
        }

        [SaveableProperty(3)]
        public Town Town
        {
            get { return _town; }
            set { _town = value; }
        }

        [SaveableProperty(4)]
        public int UpgradeLevel
        {
            get { return _upgrade_level; }
            set { _upgrade_level = value; }
        }
    }
}
