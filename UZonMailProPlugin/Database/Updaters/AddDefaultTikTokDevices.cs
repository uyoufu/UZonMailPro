using UZonMail.CorePlugin.Database.Updater;
using UZonMail.DB.SQL;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.EmailCrawler;

namespace UZonMail.ProPlugin.Database.Updaters
{
    public class AddDefaultTikTokDevices(SqlContextPro db) : IDatabaseUpdater
    {
        public Version Version => new(0, 11, 1, 0);

        public async Task Update()
        {
            var adminUserId = 2l;

            var deviceInfos = new[]
            {
                new TikTokDevice()
                {
                    Name = "UzonMail-Edge",
                    DeviceId = "7453962167239935504",
                    OdinId = "7453963230416684052",
                    UserId = adminUserId
                },
                new TikTokDevice()
                {
                    Name = "UzonMail-Chrome",
                    DeviceId = "7460744261232903698",
                    OdinId = "7460744912754803720",
                    UserId = adminUserId
                },
            };

            foreach (var device in deviceInfos)
            {
                var existOne = db
                    .TikTokDevices.Where(x =>
                        x.UserId == adminUserId && x.DeviceId == device.DeviceId
                    )
                    .FirstOrDefault();
                if (existOne == null)
                {
                    await db.TikTokDevices.AddRangeAsync(device);
                }
            }
            await db.SaveChangesAsync();
        }
    }
}
