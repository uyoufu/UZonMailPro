using System.Diagnostics;
using UZonMail.CorePlugin.Services.SendCore;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.WaitList;

namespace UZonMail.Pro.Controllers.SystemInfo.Model
{
    public class SystemUsageInfo
    {
        public double CpuUsage { get; private set; }

        public double MemoryUsage { get; private set; }

        public int RunningTasksCount { get; private set; }

        public List<OutboxPoolInfo> OutboxPoolInfos { get; set; }
        public List<SendingGroupInfo> SendingGroupsPoolInfos { get; set; }

        public async Task GatherInfomations(
            UserGroupTasksPools groupTasksList,
            OutboxesManager outboxesManager,
            SendingTasksManager sendingTasksManager
        )
        {
            CpuUsage = await GetCpuUsageForProcess();
            MemoryUsage = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

            OutboxPoolInfos = outboxesManager
                .Values.GroupBy(x => x.UserId)
                .Select(x => new OutboxPoolInfo(x.Key, x.Count()))
                .ToList();

            RunningTasksCount = sendingTasksManager.RunningTasksCount;
            SendingGroupsPoolInfos = groupTasksList
                .Values.Select(x => new SendingGroupInfo(x))
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
