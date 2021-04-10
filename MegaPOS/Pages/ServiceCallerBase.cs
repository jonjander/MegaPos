using MegaPOS.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MegaPOS.Pages
{
    public class ServiceCallerBase : ComponentBase
    {
        [Parameter] public string Id { get; set; }
        [Inject] protected IServiceScopeFactory ScopeFactory { get; set; }
        [Inject] PosState posState { get; set; }
        public async Task<TResult> Exekvera<TResult>(
            Expression<Func<PosState, Task<TResult>>> serviceMethod,
            Func<TResult, Task> efterbehandling = null
            )
        {
            try
            {
                var serviceMethodCall = serviceMethod.Compile();
                using var serviceScope = ScopeFactory.CreateScope();
                //var service = posState; 
                var service = posState; // serviceScope.ServiceProvider.GetService<PosState>();
                if (!service.IsInitilized)
                    await service.Init(Id);

                var result = await serviceMethodCall(service);
                if (efterbehandling != null)
                    await efterbehandling(result);
                return result;
            } catch (Exception ex)
            {
                throw;
            }
        }

        public async Task Exekvera(Func<PosState, Task> serviceMethod)
        {
            try
            {
                //using var serviceScope = ScopeFactory.CreateScope();
                var service = posState;  //serviceScope.ServiceProvider.GetService<PosState>();
                if (!service.IsInitilized)
                    await service.Init(Id);
                await serviceMethod(service);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public TResult ExekveraSync<TResult>(
            Func<PosState, TResult> serviceMethod
            )
        {
            try
            {
                
                //using var serviceScope = ScopeFactory.CreateScope();
                var service = posState; // serviceScope.ServiceProvider.GetService<PosState>();
                var result = serviceMethod(service);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public void ExekveraSync(
            Action<PosState> serviceMethod
            )
        {
            try
            {
                //using var serviceScope = ScopeFactory.CreateScope();
                var service = posState; // serviceScope.ServiceProvider.GetService<PosState>();
                serviceMethod(service);
            }
            catch
            {
                throw;
            }
        }
    }
}
