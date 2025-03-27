using DeviceId;
using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Permission;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Service;

namespace UZonMailProPlugin.Services.License
{
    /// <summary>
    /// 授权管理器
    /// </summary>
    public class LicenseManagerService(SqlContext sqlContext, HttpClient httpClient, PermissionService permissionService, LicenseAccessService licenseAccess) : IScopedService
    {
#if DEBUG
        private const string _licenseAPI = "https://app.223434.xyz:2234/api/v1/license-machine";
#else
        private const string _licenseAPI = "https://app.223434.xyz:2234/api/v1/license-machine";
#endif
        private static DateTime _lastUpdateDate = DateTime.MinValue;
        private static readonly string _licenseKey = "license";

        /// <summary>
        /// 授权信息
        /// </summary>
        private LicenseInfo LicenseInfo
        {
            get => licenseAccess.LicenseInfo;
            set => licenseAccess.LicenseInfo = value;
        }

        /// <summary>
        /// 授权文件路径
        /// </summary>
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LicenseManagerService));

        /// <summary>
        /// 更新 License
        /// </summary>
        /// <param name="licenseCode"></param>
        /// <returns></returns>
        public async Task<ResponseResult<LicenseInfo>> UpdateLicense(string licenseCode)
        {
            if (string.IsNullOrEmpty(licenseCode))
                return ResponseResult<LicenseInfo>.Fail("授权码不能为空");

            // 更新授权
            var deviceId = GetDeviceId();
            var addLicenseUrl = $"{_licenseAPI}?licenseCode={licenseCode}&machineId={deviceId}";
            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsync(addLicenseUrl, null);
            }
            catch (HttpRequestException e)
            {
                _logger.Warn(e.Message);
                return ResponseResult<LicenseInfo>.Fail("无法连接到授权服务器，请检查网络设置");
            }

            var responseResult = await response.ToResponseResult<bool>();
            if (!responseResult.Ok)
            {
                return ResponseResult<LicenseInfo>.Fail(responseResult.Message);
            }

            // 下载授权信息
            var license = await DownloadLicense();
            if (license == null) return ResponseResult<LicenseInfo>.Fail("授权码无效");
            LicenseInfo = license;
            _lastUpdateDate = DateTime.Now;

            // 将授权码更新到数据库中
            var systemSettings = await sqlContext.SystemSettings.FirstOrDefaultAsync(x => x.Key == _licenseKey);
            if (systemSettings == null)
            {
                systemSettings = new SystemSetting()
                {
                    Key = _licenseKey,
                    StringValue = licenseCode,
                    DateTime = license.ExpireDate
                };
                sqlContext.SystemSettings.Add(systemSettings);
            }
            else
            {
                systemSettings.StringValue = licenseCode;
                systemSettings.DateTime = license.ExpireDate;
            }
            await sqlContext.SaveChangesAsync();

            // 清除授权缓存
            await permissionService.ResetAllUserPermissionsCache();

            return license.ToSuccessResponse();
        }

        /// <summary>
        /// 重新更新已有的授权
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseResult<LicenseInfo>> UpdateExistingLicense()
        {
            // 下载授权信息
            var licenseInfo = await GetLicenseInfo(false);

            // 清除授权缓存
            await permissionService.ResetAllUserPermissionsCache();
            return licenseInfo.ToSuccessResponse();
        }


        /// <summary>
        /// 获取授权信息
        /// 若没有授权信息，会先从服务器验证
        /// 若验证失败，则会返回一个默认的授权
        /// </summary>
        /// <returns></returns>
        public async Task<LicenseInfo> GetLicenseInfo(bool updateThrottle = true)
        {
            //#if DEBUG
            //            _licenseInfo ??= LicenseInfo.CreateEnterpriseLicense();
            //            return _licenseInfo;
            //#endif

            // 如果过期了，则从数据库中获取
            // 只有更新日期超过一天才会去请求授权服务器
            if (updateThrottle && LicenseInfo != null && LicenseInfo.ExpireDate > DateTime.Now && _lastUpdateDate.AddDays(1) > DateTime.Now)
            {
                return LicenseInfo;
            }

            var defaultLicenseInfo = LicenseInfo.CreateDefaultLicense();
            LicenseInfo = defaultLicenseInfo;
            _lastUpdateDate = DateTime.Now;

            // 如果更新的日期大于当前日期，说明系统时间被修改了
            if (_lastUpdateDate > DateTime.Now)
            {
                _logger.Warn("请勿修改系统日期");
                return LicenseInfo;
            }

            var license = await DownloadLicense();
            if (license == null)
            {
                LicenseInfo = defaultLicenseInfo;
                return LicenseInfo;
            }

            // 更新授权信息
            LicenseInfo = license;
            _lastUpdateDate = DateTime.Now;

            return LicenseInfo;
        }

        /// <summary>
        /// 下载授权
        /// </summary>
        /// <returns></returns>
        private async Task<LicenseInfo?> DownloadLicense()
        {
            // 判断数据库中是否有 license
            //if (checkLicenseExist)
            //{
            //    var systemSettings = await sqlContext.SystemSettings.FirstOrDefaultAsync(x => x.Key == _licenseKey);
            //    if (systemSettings == null) return null;
            //}

            var deviceId = GetDeviceId();

            // 向授权服务器请求授权信息
            HttpResponseMessage response;
            try
            {
                response = await httpClient.GetAsync($"{_licenseAPI}?machineId={deviceId}");
            }
            catch (HttpRequestException e)
            {
                _logger.Warn(e.Message);
                _logger.Warn($"无法连接到授权服务器，无法提供授权功能");
                return null;
            }

            var responseResult = await response.ToResponseResult<string>();
            if (!responseResult.Ok)
            {
                _logger.Warn(responseResult.Message);
                return null;
            }
            var dataString = responseResult.Data;
            if (string.IsNullOrEmpty(dataString))
            {
                return null;
            }

            // 解密
            // 从嵌入的资源中获取密钥
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UZonMailProPlugin.Services.License.PrivateKey.pem");
            if (stream == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    Console.WriteLine(resourceName);
                }
                return null;
            }

            using var streamReader = new StreamReader(stream);
            var privateKey = streamReader.ReadToEnd();
            // base64 字符串转为 byte[]
            var privateKeyBytes = Convert.FromBase64String(responseResult.Data);
            var jsonString = privateKeyBytes.FromRSA(privateKey);
            var jsonData = JObject.Parse(jsonString);
            if (jsonData == null) return null;
            return jsonData.ToObject<LicenseInfo>();
        }

        /// <summary>
        /// 获取机器码
        /// </summary>
        /// <returns></returns>
        public string GetDeviceId()
        {
            // 判断是否有机器码文件，若没有，则新增
            var fileTokenPath = GetDeviceTokenPath();
            string deviceId = string.Empty;
            if (IsContainerEnv())
            {
                _logger.Info("检测到 Docker 环境, 使用自定义识别码");
                deviceId = new DeviceIdBuilder().AddFileToken(fileTokenPath).ToString();
            }
            else
            {
                // 获取机器识别码
                deviceId = new DeviceIdBuilder()
                          .AddOsVersion()
                          .AddMachineName()
                          .AddUserName()
                          .AddMacAddress()
                          .AddFileToken(fileTokenPath)
                          .ToString();
            }

            _logger.Info($"当前机器码: {deviceId}");
            return deviceId;
        }

        /// <summary>
        /// 判断是否是容器环境
        /// </summary>
        /// <returns></returns>
        private bool IsContainerEnv()
        {
            return IsVirtualEnvByCGroup() || IsVirtualEnvByEnvFile();
        }

        private bool IsVirtualEnvByCGroup()
        {
            if (!File.Exists("/proc/1/cgroup")) return false;
            string[] lines = File.ReadAllLines("/proc/1/cgroup");
            var isDocker = lines.Any(line => line.Contains("docker")
                || line.Contains("kubepods"))
                || lines.Any(line => line.Contains("lxc"))
                || lines.Any(line => line.Contains("libvirt"))
                || lines.Any(line => line.Contains("openvz"));
            return isDocker;
        }

        private bool IsVirtualEnvByEnvFile()
        {
            return File.Exists("/.dockerenv");
        }

        /// <summary>
        /// 获取设备 Token 路径
        /// 1. 若有 AppData 中包含，则优先使用 AppData 中的数据
        /// 2. 若没有，则从 data/device/device-token.txt 复制
        /// </summary>
        /// <returns></returns>
        private string GetDeviceTokenPath()
        {
            var fileTokenPath = "data/device/device-token.txt";
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var fullTokenPath = Path.Combine(localAppData, "UZonMail/device-token.txt");
            if (File.Exists(fullTokenPath)) return fullTokenPath;

            // 不存在时，判断用户目录下是否包含
            if (File.Exists(fileTokenPath))
            {
                // 复制到用户配置目录
                Directory.CreateDirectory(Path.GetDirectoryName(fullTokenPath));
                File.Copy(fileTokenPath, fullTokenPath);
                return fullTokenPath;
            }

            // 若都不存在，直接创建一个
            // 创建目录
            Directory.CreateDirectory(Path.GetDirectoryName(fileTokenPath));
            // 保存内容
            File.WriteAllText(fileTokenPath, Guid.NewGuid().ToString());

            // 重新获取
            return GetDeviceTokenPath();
        }
    }
}
