using SKIT.FlurlHttpClient.Wechat.Api;

namespace Materal.MergeBlock.Wechat.Services;

/// <summary>
/// 微信AccessToken服务
/// </summary>
public interface IWechatAccessTokenService
{
    /// <summary>
    /// 获得AccessToken
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    Task<string> GetAccessTokenAsync(WechatApiClient client);
}
