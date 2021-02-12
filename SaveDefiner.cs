using System.Collections.Generic;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem;

namespace MoreSettlementAction
{
    public class SaveDefiner : SaveableTypeDefiner
    {
        public SaveDefiner() : base(1436514365)
        {
        }

        protected override void DefineClassTypes() {
            this.AddClassDefinition(typeof(SlaveEstate), 1);
            this.AddClassDefinition(typeof(SlaveEstate.DailyLog), 2);
            this.AddClassDefinition(typeof(ToolMaker), 3);
            this.AddClassDefinition(typeof(SackSettlement), 4);
        }

        protected override void DefineContainerDefinitions() {
            this.ConstructContainerDefinition(typeof(Dictionary<Hero, IFaction>));
            this.ConstructContainerDefinition(typeof(Dictionary<Village, MobileParty>));
            this.ConstructContainerDefinition(typeof(Dictionary<Village, SlaveEstate>));
            this.ConstructContainerDefinition(typeof(Dictionary<Town, ToolMaker>));
            this.ConstructContainerDefinition(typeof(List<SlaveEstate.DailyLog>));
            this.ConstructContainerDefinition(typeof(Dictionary<Town, CampaignTime>));
            this.ConstructContainerDefinition(typeof(List<SackSettlement>));
        }
    }
}
