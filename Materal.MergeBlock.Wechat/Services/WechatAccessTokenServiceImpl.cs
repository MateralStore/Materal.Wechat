using Materal.Extensions.DependencyInjection;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace Materal.MergeBlock.Wechat.Services;

/// <summary>
/// 微信AccessToken服务
/// </summary>
public class WechatAccessTokenServiceImpl : IWechatAccessTokenService, ISingletonDependency<IWechatAccessTokenService>
{
    /// <inheritdoc/>
    public async Task<string> GetAccessTokenAsync(WechatApiClient client)
    {
        CgibinTokenResponse response = await client.ExecuteCgibinTokenAsync(new());
        if (!response.IsSuccessful()) throw new WechatModuleException(response);
        return response.AccessToken;
    }
}