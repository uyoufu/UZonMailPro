﻿using DeviceId;
using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Permission;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Settings;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Service;

namespace UZonMailProPlugin.Services.License
{
    /// <summary>
    /// 授权管理器
    /// </summary>
    public class LicenseManagerService(SqlContext sqlContext, HttpClient httpClient, PermissionService permissionService) : IScopedService
    {
#if DEBUG
        private const string _licenseAPI = "https://app.223434.xyz:2234/api/v1/license-machine";
#else
        private const string _licenseAPI = "https://app.223434.xyz:2234/api/v1/license-machine";
#endif

        private static LicenseInfo? _licenseInfo;
        private static DateTime _lastUpdateDate = DateTime.MinValue;
        private static readonly string _licenseKey = "license";

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
            var license = await DownloadLicense(false);
            if (license == null) return ResponseResult<LicenseInfo>.Fail("授权码无效");
            _licenseInfo = license;
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
        /// 若没有授权信息，会返回一个默认的授权
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
            if (updateThrottle && _licenseInfo != null && _licenseInfo.ExpireDate > DateTime.Now && _lastUpdateDate.AddDays(1) > DateTime.Now)
            {
                return _licenseInfo;
            }

            var defaultLicenseInfo = LicenseInfo.CreateDefaultLicense();
            _licenseInfo = defaultLicenseInfo;
            _lastUpdateDate = DateTime.Now;

            // 如果更新的日期大于当前日期，说明系统时间被修改了
            if (_lastUpdateDate > DateTime.Now)
            {
                _logger.Warn("请勿修改系统日期");
                return _licenseInfo;
            }

            var license = await DownloadLicense();
            if (license == null)
            {
                _licenseInfo = defaultLicenseInfo;
                return _licenseInfo;
            }

            // 更新授权信息
            _licenseInfo = license;
            _lastUpdateDate = DateTime.Now;

            return _licenseInfo;
        }

        /// <summary>
        /// 下载授权
        /// </summary>
        /// <returns></returns>
        private async Task<LicenseInfo?> DownloadLicense(bool checkLicenseExist = true)
        {
            // 判断数据库中是否有 license
            if (checkLicenseExist)
            {
                var systemSettings = await sqlContext.SystemSettings.FirstOrDefaultAsync(x => x.Key == _licenseKey);
                if (systemSettings == null) return null;
            }

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
            var fileTokenPath = "data/device/device-token.txt";
            if (!File.Exists(fileTokenPath))
            {
                // 创建目录
                Directory.CreateDirectory(Path.GetDirectoryName(fileTokenPath));
                // 保存内容
                File.WriteAllText(fileTokenPath, Guid.NewGuid().ToString());
            }

            // 获取机器识别码
            string deviceId = new DeviceIdBuilder()
                                .AddOsVersion()
                                .AddMachineName()
                                .AddUserName()
                                .AddMacAddress()
                                .AddFileToken(fileTokenPath)
                                .ToString();

            return deviceId;
        }
    }
}
