using System.Diagnostics;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.Core.Services.SendCore;

namespace UZonMail.Pro.Controllers.SystemInfo.Model
{
    public class SystemUsageInfo
    {
        public double CpuUsage { get; private set; }

        public double MemoryUsage { get; private set; }

        public int RunningTasksCount { get; private set; }

        public List<OutboxPoolInfo> OutboxPoolInfos { get; set; }
        public List<SendingGroupInfo> SendingGroupsPoolInfos { get; set; }

        public async Task GatherInfomations(GroupTasksList groupTasksList
        , OutboxesPoolList outboxesPools
        , SendingThreadsManager sendingThreadsManager)
        {
            CpuUsage = await GetCpuUsageForProcess();
            MemoryUsage = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

            OutboxPoolInfos = outboxesPools.Values
                .Select(x => new OutboxPoolInfo(x))
                .ToList();

            RunningTasksCount = sendingThreadsManager.RunningTasksCount;
            SendingGroupsPoolInfos = groupTasksList.Values
                .Select(x => new SendingGroupInfo(x))
                .ToList();
        }

        private async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }
    }
}
