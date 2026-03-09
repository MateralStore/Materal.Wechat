using Materal.MergeBlock.Wechat.DTO;

namespace Materal.MergeBlock.Wechat.Services;

/// <summary>
/// 微信服务
/// </summary>
public interface IWechatService
{
    /// <summary>
    /// 获取用户AccessToken(OpenID也在这里获取)
    /// </summary>
    /// <param name="code"></param>
    /// <param name="configKey"></param>
    /// <returns></returns>
    Task<UserAccessTokenDTO> GetUserAccessTokenAsync(string code, string configKey = "Default");
}
