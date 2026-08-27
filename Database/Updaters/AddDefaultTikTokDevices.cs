using UzonMail.CorePlugin.Database.Initializers;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.EmailCrawler;

namespace UzonMail.ProPlugin.Database.Updaters
{
    public class AddDefaultTikTokDevices(SqlContextPro db) : IDbInitializer
    {
        public string Name => nameof(AddDefaultTikTokDevices);

        public async Task ExecuteAsync()
        {
            var adminUserId = 2L;

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
                    await db.TikTokDevices.AddAsync(device);
                }
            }
            await db.SaveChangesAsync();
        }
    }
}
