using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UZonMailCrawlerPlugin.Controllers.Base
{
    /// <summary>
    /// pro 版本控制器基类
    /// </summary>
    [Authorize]
    [Route("api/pro/[controller]")]
    [ApiController]
    public class ControllerBasePro : ControllerBase
    {
    }
}
