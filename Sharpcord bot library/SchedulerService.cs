using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sharpcord_bot_library
{
    public class SchedulerService
    {
        private static SchedulerService _instance;
        private List<Timer> timers = new List<Timer>();

        private SchedulerService() { }

        public static SchedulerService Instance => _instance ?? (_instance = new SchedulerService());

        public void ScheduleTask(DateTime firstRun, TimeSpan interval, Action task)
        {
            DateTime now = DateTime.Now;
            if (now > firstRun)
            {
                firstRun = firstRun.AddDays(1);
            }

            TimeSpan timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero;
            }

            var timer = new Timer(x =>
            {
                task.Invoke();
            }, null, timeToGo, interval);

            timers.Add(timer);
        }
    }
}
