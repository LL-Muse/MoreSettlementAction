using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
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
    [SaveableClass(1)]
    internal class SlaveEstate
    {
        internal Village _village;
        private ItemObject _product;
        private TroopRoster _prisoners;
        private TroopRoster _guards;
        private ItemRoster _stockpile;
        private List<DailyLog> _logs;
        private int _surgeon_house_level;
        private int _overseer_house_level;
        private int _tool_repair_workshop_level;
        public SlaveEstate() {

            _village = Settlement.CurrentSettlement.Village;
            _prisoners = TroopRoster.CreateDummyTroopRoster();
            _stockpile = new ItemRoster();
            _product = FindProduct(_village);
            _logs = new List<DailyLog>();
            _surgeon_house_level = 0;
            _overseer_house_level = 0;
            _tool_repair_workshop_level = 0;
        }


        [SaveableProperty(1)]
        public ItemRoster StockPile
        {
            get { return _stockpile; }
            set { _stockpile = value; }
        }

        [SaveableProperty(2)]
        public TroopRoster Prisoners
        {
            get { return _prisoners; }
            set { _prisoners = value; }
        }

        [SaveableProperty(3)]
        public ItemObject PrimaryProduction
        {
            get { return _product; }
            set { _product = value; }
        }

        [SaveableProperty(4)]
        public Village Village
        {
            get { return _village; }
            set { _village = value; }
        }

        [SaveableProperty(5)]
        private List<DailyLog> Logs
        {
            get { return _logs; }
            set { _logs = value; }
        }

        [SaveableProperty(6)]
        public int SurgeonLevel
        {
            get { return _surgeon_house_level; }
            set { _surgeon_house_level = value; }
        }

        [SaveableProperty(7)]
        public int OverseerLevel
        {
            get { return _overseer_house_level; }
            set { _overseer_house_level = value; }
        }

        [SaveableProperty(8)]
        public int ToolRepairLevel
        {
            get { return _tool_repair_workshop_level; }
            set { _tool_repair_workshop_level = value; }
        }

        [SaveableProperty(9)]
        public TroopRoster Guards
        {
            get { return _guards; }
            set { _guards = value; }
        }

        public void Display()
        {
            if(_logs == null)
            {
                _logs = new List<DailyLog>();
            }

            if (_logs.Count < 1)
            {
                InformationManager.DisplayMessage(new InformationMessage("No entries in log"));
                return;
            }
            _logs.Last<DailyLog>().Display();
        }

        public void AddToLogs(int slavesDied, int goodsProduced, int toolsUsed)
        {
            DailyLog newEntry = new DailyLog(slavesDied, goodsProduced, toolsUsed);
            if (_logs == null)
            {
                _logs = new List<DailyLog>();
            }

            if (_logs.Count > 0)
            {
                _logs.Last<DailyLog>().NextDay = newEntry;
                newEntry.PreviousDay = _logs.Last<DailyLog>();
            }
            _logs.Add(newEntry);
        }

        public class DailyLog{
            private CampaignTime day;
            private int _slavesDied;
            private int _goodsProduced;
            private int _toolsUsed;
            private DailyLog _nextDay;
            private DailyLog _previousDay;

            public DailyLog(int slavesDied, int goodsProduced, int toolsUsed)
            {
                day = CampaignTime.Now;
                _slavesDied = slavesDied;
                _goodsProduced = goodsProduced;
                _toolsUsed = toolsUsed;
                _nextDay = null;
                _previousDay = null;
            }

            public void Display()
            {
                InformationManager.ShowInquiry(new InquiryData(day.ToString(), "Goods Produced: " + _goodsProduced + "\nSlaves Died: " + _slavesDied + "\nTools Used: " + _toolsUsed, true, true, (_nextDay == null) ? "Close" : "Next", (_previousDay == null) ? "Close" : "Previous", (Action)(()=> {
                    InformationManager.HideInquiry();
                    if(_nextDay != null)
                    {
                        _nextDay.Display();
                    }
                }),(Action)(()=> {
                    InformationManager.HideInquiry();
                    if(_previousDay != null)
                    {
                        _previousDay.Display();
                    }
                }) ));
            }

            [SaveableProperty(1)]
            public int SlavesDied
            {
                get { return _slavesDied; }
                set { _slavesDied = value; }
            }

            [SaveableProperty(2)]
            public int GoodsProduced
            {
                get { return _goodsProduced; }
                set { _goodsProduced = value; }
            }

            [SaveableProperty(3)]
            public int ToolsUsed
            {
                get { return _toolsUsed; }
                set { _toolsUsed = value; }
            }

            [SaveableProperty(4)]
            public DailyLog NextDay
            {
                get { return _nextDay; }
                set { _nextDay = value; }
            }

            [SaveableProperty(5)]
            public DailyLog PreviousDay
            {
                get { return _previousDay; }
                set { _previousDay = value; }
            }
        }

        private ItemObject FindProduct(Village village)
        {
            if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("flax")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("flax");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("fish")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("fish");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("fur")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("fur");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("hardwood")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("hardwood");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("clay")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("clay");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("salt")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("salt");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("iron")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("iron");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("grape")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("grape");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("date_fruit")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("date_fruit");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("cotton")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("cotton");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("olives")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("olives");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("silver")))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("silver");
            }
            else if (village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("hog"))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("hog");
            }
            else if (village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("sheep"))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("sheep");
            }
            else if (village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("cow"))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("cow");
            }
            else if (village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse")))
            {
                if(village.Settlement.Culture == MBObjectManager.Instance.GetObject<ItemObject>("aserai_horse").Culture)
                {
                    return MBObjectManager.Instance.GetObject<ItemObject>("aserai_horse");
                }
                else if(village.Settlement.Culture == MBObjectManager.Instance.GetObject<ItemObject>("battania_horse").Culture)
                {
                    return MBObjectManager.Instance.GetObject<ItemObject>("battania_horse");
                }
                else if (village.Settlement.Culture == MBObjectManager.Instance.GetObject<ItemObject>("empire_horse").Culture)
                {
                    return MBObjectManager.Instance.GetObject<ItemObject>("empire_horse");
                }
                else if (village.Settlement.Culture == MBObjectManager.Instance.GetObject<ItemObject>("khuzait_horse").Culture)
                {
                    return MBObjectManager.Instance.GetObject<ItemObject>("khuzait_horse");
                }
                else if (village.Settlement.Culture == MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse").Culture)
                {
                    return MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse");
                }
                else if (village.Settlement.Culture == MBObjectManager.Instance.GetObject<ItemObject>("sturgia_horse").Culture)
                {
                    return MBObjectManager.Instance.GetObject<ItemObject>("sturgia_horse");
                }
                else
                {
                    return MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                }
            }
            else
            {
                return MBObjectManager.Instance.GetObject<ItemObject>("grain");
            }
        }
    }
}
