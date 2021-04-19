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
        protected StoreSetupVm Model { get; set; }
        protected Modal modalRef;

        protected override void OnInitialized()
        {
            Model = new StoreSetupVm { StoreId = StoreId };
        }

        public void OpenModal()
        {

            //Get stats
            Model = ExecuteSync(_ => _.GetStoreSetup(StoreId));

            modalRef.Show();
        }

        protected void EraseUnusedCustomers()
        {
            ExecuteSync(_ => _.ClenupCustomers(Model.StoreId));
        }
   
        public void Save()
        {
            ExecuteSync(_ => _.UpdateStoreInfo(Model));
            modalRef.Hide();
        }
        public void Close()
        {
            modalRef.Hide();
        }
    }
}
