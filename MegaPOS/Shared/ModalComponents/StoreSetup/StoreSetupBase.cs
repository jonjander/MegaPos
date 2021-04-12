using Blazorise;
using MegaPOS.Model;
using MegaPOS.Pages;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Shared.ModalComponents.StoreSetup
{
    public class StoreSetupBase : ServiceCallerBase
    {
        [Parameter] public string StoreId { get; set; }
        protected StoreSetupVm model { get; set; }
        protected Modal modalRef;

        protected override async Task OnInitializedAsync()
        {
            model = new StoreSetupVm { StoreId = StoreId };
        }

        public void OpenModal()
        {

            //Get stats
            model = ExekveraSync(_ => _.GetStoreSetup(StoreId));

            modalRef.Show();
        }

        protected async Task EraseUnusedCustomers()
        {
            ExekveraSync(_ => _.ClenupCustomers(model.StoreId));
        }

   
        public async Task Save()
        {
            ExekveraSync(_ => _.UpdateStoreInfo(model));
            modalRef.Hide();
        }
        public void Close()
        {
            modalRef.Hide();
        }
    }
}
