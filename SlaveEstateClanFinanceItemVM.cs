using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;


namespace MoreSettlementAction
{
    internal class SlaveEstateClanFinanceItemVM : ClanFinanceIncomeItemBaseVM
    {
        public SlaveEstateClanFinanceItemVM(Action<ClanFinanceIncomeItemBaseVM> onSelection, Action onRefresh) : base(onSelection, onRefresh);
        {ddfsd
            dsfsdf
            dsfsdfsd
            sfdfsd
            this.set_IncomeTypeAsEnum((IncomeTypes)2);
            SettlementComponent component = this._brothel.Settlement.GetComponent<SettlementComponent>();
            this.set_ImageName(component != null ? component.WaitMeshName : "");
            ((ViewModel)this).RefreshValues();
        }

        protected override void PopulateActionList()
        {
            throw new NotImplementedException();
        }

        protected override void PopulateStatsList()
        {
            throw new NotImplementedException();
        }
    }
}
