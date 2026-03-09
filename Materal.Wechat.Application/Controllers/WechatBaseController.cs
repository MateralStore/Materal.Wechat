using Materal.MergeBlock.Web.Abstractions.Controllers;
using Materal.MergeBlock.Wechat.Services;
using Microsoft.AspNetCore.Mvc;

namespace Materal.Wechat.Application.Controllers;

[Route("WeChatAPI/[controller]/[action]")]
public abstract class WechatBaseController(IWechatService wechatService) : MergeBlockController
{
    /// <summary>
    /// 微信服务
    /// </summary>
    protected IWechatService WechatService { get; } = wechatService;
}
