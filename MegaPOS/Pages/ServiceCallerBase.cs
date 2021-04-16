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
        [Inject] PosState PosState { get; set; }
        public async Task<TResult> ExecureAsync<TResult>(
            Expression<Func<PosState, Task<TResult>>> serviceMethod,
            Func<TResult, Task> efterbehandling = null
            )
        {
            try
            {
                var serviceMethodCall = serviceMethod.Compile();
                using var serviceScope = ScopeFactory.CreateScope();
                //var service = posState; 
                var service = PosState; // serviceScope.ServiceProvider.GetService<PosState>();
                if (!service.IsInitilized)
                    service.Init(Id);

                var result = await serviceMethodCall(service);
                if (efterbehandling != null)
                    await efterbehandling(result);
                return result;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task ExecuteAsync(Func<PosState, Task> serviceMethod)
        {
            try
            {
                //using var serviceScope = ScopeFactory.CreateScope();
                var service = PosState;  //serviceScope.ServiceProvider.GetService<PosState>();
                if (!service.IsInitilized)
                    service.Init(Id);
                await serviceMethod(service);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public TResult ExecuteSync<TResult>(
            Func<PosState, TResult> serviceMethod
            )
        {
            try
            {
                
                //using var serviceScope = ScopeFactory.CreateScope();
                var service = PosState; // serviceScope.ServiceProvider.GetService<PosState>();
                var result = serviceMethod(service);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public void ExekuteSync(
            Action<PosState> serviceMethod
            )
        {
            try
            {
                //using var serviceScope = ScopeFactory.CreateScope();
                var service = PosState; // serviceScope.ServiceProvider.GetService<PosState>();
                serviceMethod(service);
            }
            catch
            {
                throw;
            }
        }
    }
}
