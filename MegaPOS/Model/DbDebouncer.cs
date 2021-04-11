using MegaPOS.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
    public class DbDebouncer<T>
    {
        private bool PoolIsRunning { get; set; }
        private QueueWrapper<List<Func<Task<T>>>> TaskPool { get; set; } = new QueueWrapper<List<Func<Task<T>>>> { Object = new List<Func<Task<T>>>() };
        private T Result;

        public DbDebouncer()
        {

        }

        public async Task<T> PoolAndRun(Func<Task<T>> dbOperation)
        {
            TaskPool.Object.Add(dbOperation);

            if (PoolIsRunning)
                return Result ?? default;

            PoolIsRunning = true;
            while (TaskPool.Object.Count > 0)
            {
                if (TaskPool.Object.Count > 1)
                {
                    TaskPool.DropFist();
                    continue;
                }
                var currentTask = TaskPool.PopFist();
                Result = await currentTask.Invoke();
                TaskPool.DropFist();
            }
            PoolIsRunning = false;
            return Result;
        }
    }
}
