using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UZonMailProPlugin.Controllers.Base
{
    /// <summary>
    /// biz 版本控制器基类
    /// biz = Business
    /// </summary>
    [Authorize]
    [Route("api/biz/[controller]")]
    [ApiController]
    public class ControllerBaseBusiness : ControllerBase
    {
    }
}
