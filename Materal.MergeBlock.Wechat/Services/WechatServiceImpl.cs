using Materal.Extensions.DependencyInjection;
using Materal.MergeBlock.Wechat.DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace Materal.MergeBlock.Wechat.Services;

/// <summary>
/// 微信服务
/// </summary>
public class WechatServiceImpl(IOptionsMonitor<ApplicationConfig> config, IMemoryCache cache) : IWechatService, ITransientDependency<IWechatService>
{
    /// <inheritdoc/>
    public async Task<UserAccessTokenDTO> GetUserAccessTokenAsync(string code, string configKey = "Default")
    {
        WechatApiClient client = GetWechatApiClient(configKey);
        SnsOAuth2AccessTokenResponse response = await client.ExecuteSnsOAuth2AccessTokenAsync(new SnsOAuth2AccessTokenRequest()
        {
            Code = code
        });
        if (!response.IsSuccessful()) throw new WechatModuleException(response);
        UserAccessTokenDTO result = new()
        {
            AccessToken = response.AccessToken,
            ExpiresIn = response.ExpiresIn,
            OpenID = response.OpenId,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            UnionID = response.UnionId
        };
        return result;
    }

    private WechatApiClient GetWechatApiClient(string configKey = "Default")
    {
        string cacheKey = $"WechatApiClient_{configKey}";
        WechatApiClient result = cache.GetOrCreate(cacheKey, entry =>
        {
            WechatApiClientOptions options = GetWechatApiClientOptions(configKey);
            return new WechatApiClient(options);
        }) ?? throw new WechatModuleException($"创建微信客户端缓存失败");
        return result;
    }

    private WechatApiClientOptions GetWechatApiClientOptions(string configKey = "Default")
    {
        WechatOptions? wechatOptions = config.CurrentValue.WechatOptions.FirstOrDefault(m => m.Key == configKey);
        if (wechatOptions is null && configKey == "Default")
        {
            wechatOptions = config.CurrentValue.WechatOptions.FirstOrDefault();
        }
        if (wechatOptions is null) throw new WechatModuleException($"获取配置项{configKey}失败");
        WechatApiClientOptions result = new()
        {
            AppId = wechatOptions.AppID,
            AppSecret = wechatOptions.AppSecret,
        };
        return result;
    }
}