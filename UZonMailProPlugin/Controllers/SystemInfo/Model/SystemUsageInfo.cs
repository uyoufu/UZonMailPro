using System.Diagnostics;
using UZonMail.CorePlugin.Services.SendCore;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.WaitList;

namespace UZonMail.Pro.Controllers.SystemInfo.Model
{
    public class SystemUsageInfo
    {
        public double CpuUsage { get; private set; }

        public string CpuUsageString => $"{CpuUsage:F2}%";

        public double MemoryUsage { get; private set; }

        public string MemoryUsageString => $"{MemoryUsage:F2} MB";

        public int RunningTasksCount { get; private set; }

        public List<OutboxPoolInfo> OutboxPools { get; set; }
        public List<SendingGroupInfo> UserSendingPools { get; set; }

        public async Task GatherInfomations(
            UserGroupTasksPools userGroupTaskPool,
            OutboxesManager outboxesManager,
            SendingTasksManager sendingTasksManager
        )
        {
            CpuUsage = await GetCpuUsageForProcess();
            MemoryUsage = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

            OutboxPools =
            [
                .. outboxesManager
                    .Values.GroupBy(x => x.UserId)
                    .Select(x => new OutboxPoolInfo(x.Key, x.Count()))
            ];
            RunningTasksCount = sendingTasksManager.RunningTasksCount;

            UserSendingPools = [.. userGroupTaskPool.Values.Select(x => new SendingGroupInfo(x))];
        }

        private static async Task<double> GetCpuUsageForProcess()
        {
            var process = Process.GetCurrentProcess();
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            await Task.Delay(500);

            process.Refresh();
            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            if (cpuUsedMs <= 0)
                return 0;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }
    }
}
