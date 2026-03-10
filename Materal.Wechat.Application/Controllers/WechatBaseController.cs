using Materal.MergeBlock.Web.Abstractions.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Materal.Wechat.Application.Controllers;

/// <summary>
/// 微信基础控制器
/// </summary>
[Route("WechatAPI/[controller]/[action]")]
public abstract class WechatBaseController : MergeBlockController
{
}
