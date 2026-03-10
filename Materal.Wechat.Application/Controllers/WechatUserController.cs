using Materal.MergeBlock.Wechat.DTO;
using Materal.MergeBlock.Wechat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Materal.Wechat.Application.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
/// <param name="wechatService"></param>
[AllowAnonymous]
public class WechatUserController(IWechatService wechatService) : WechatBaseController
{
    /// <summary>
    /// 获取用户AccessToken(OpenID也在这里获取)
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ResultModel<UserAccessTokenDTO>> GetUserAccessTokenAsync([FromQuery] string code)
    {
        UserAccessTokenDTO result = await wechatService.GetUserAccessTokenAsync(code);
        return ResultModel<UserAccessTokenDTO>.Success(result, "获取成功");
    }
}