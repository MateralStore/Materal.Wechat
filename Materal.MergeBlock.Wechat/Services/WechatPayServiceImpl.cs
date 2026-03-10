using Materal.Extensions.DependencyInjection;
using Materal.MergeBlock.Wechat.DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Events;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings;

namespace Materal.MergeBlock.Wechat.Services;

/// <summary>
/// 微信支付服务实现
/// </summary>
public class WechatPayServiceImpl(IOptionsMonitor<ApplicationConfig> config, IMemoryCache cache) : IWechatPayService, ITransientDependency<IWechatPayService>
{
    /// <inheritdoc/>
    public async Task<CreateOrderResponseDTO> CreateJsapiOrderAsync(CreateOrderRequestDTO request, string configKey = "Default")
    {
        WechatTenpayClient client = GetWechatTenpayClient(configKey);
        WechatOptions wechatOptions = GetWechatOptions(configKey);
        // 构建下单请求
        CreatePayTransactionJsapiRequest payRequest = new()
        {
            OutTradeNumber = request.OutTradeNo,
            AppId = wechatOptions.AppID,
            Description = request.Description,
            NotifyUrl = wechatOptions.NotifyUrl ?? string.Empty,
            Amount = new CreatePayTransactionJsapiRequest.Types.Amount
            {
                Total = request.TotalAmount
            },
            Payer = new CreatePayTransactionJsapiRequest.Types.Payer
            {
                OpenId = request.OpenID
            }
        };
        // 调用下单接口
        CreatePayTransactionJsapiResponse response = await client.ExecuteCreatePayTransactionJsapiAsync(payRequest);
        if (!response.IsSuccessful()) throw new WechatModuleException($"下单失败: {response.ErrorCode} - {response.ErrorMessage}");
        // 生成前端调起支付所需的参数
        IDictionary<string, string> jsapiParams = client.GenerateParametersForJsapiPayRequest(wechatOptions.AppID, response.PrepayId);
        CreateOrderResponseDTO result = new()
        {
            PrepayID = response.PrepayId,
            AppID = jsapiParams["appId"],
            TimeStamp = jsapiParams["timeStamp"],
            NonceStr = jsapiParams["nonceStr"],
            Package = jsapiParams["package"],
            SignType = jsapiParams["signType"],
            PaySign = jsapiParams["paySign"]
        };
        return result;
    }

    /// <inheritdoc/>
    public async Task<OrderQueryResponseDTO> QueryOrderByOutTradeNoAsync(string outTradeNo, string configKey = "Default")
    {
        WechatTenpayClient client = GetWechatTenpayClient(configKey);
        WechatOptions wechatOptions = GetWechatOptions(configKey);

        GetPayTransactionByOutTradeNumberRequest request = new()
        {
            OutTradeNumber = outTradeNo,
            MerchantId = wechatOptions.MchId ?? string.Empty
        };

        GetPayTransactionByOutTradeNumberResponse response = await client.ExecuteGetPayTransactionByOutTradeNumberAsync(request);
        if (!response.IsSuccessful()) throw new WechatModuleException($"查询订单失败: {response.ErrorCode} - {response.ErrorMessage}");

        return MapToOrderQueryResponse(response);
    }

    /// <inheritdoc/>
    public async Task<OrderQueryResponseDTO> QueryOrderByTransactionIdAsync(string transactionId, string configKey = "Default")
    {
        WechatTenpayClient client = GetWechatTenpayClient(configKey);

        GetPayTransactionByIdRequest request = new()
        {
            TransactionId = transactionId
        };

        GetPayTransactionByIdResponse response = await client.ExecuteGetPayTransactionByIdAsync(request);
        if (!response.IsSuccessful())
        {
            throw new WechatModuleException($"查询订单失败: {response.ErrorCode} - {response.ErrorMessage}");
        }

        return MapToOrderQueryResponse(response);
    }

    /// <inheritdoc/>
    public Task<OrderQueryResponseDTO> HandlePayNotifyAsync(string body, string signature, string timestamp, string nonce, string serial, string configKey = "Default")
    {
        WechatTenpayClient client = GetWechatTenpayClient(configKey);

        // 验证签名
        bool valid = client.VerifyEventSignature(timestamp, nonce, body, signature, serial);
        if (!valid) throw new WechatModuleException("支付回调签名验证失败");

        // 解析回调数据
        WechatTenpayEvent callbackEvent = client.DeserializeEvent(body);
        if ("TRANSACTION.SUCCESS" != callbackEvent.EventType) throw new WechatModuleException("支付回调数据解析失败");
        // 解密资源
        TransactionResource resource = client.DecryptEventResource<TransactionResource>(callbackEvent);
        OrderQueryResponseDTO result = new()
        {
            TransactionId = resource.TransactionId,
            OutTradeNo = resource.OutTradeNumber,
            TradeState = resource.TradeState,
            TradeStateDesc = resource.TradeStateDescription ?? string.Empty,
            BankType = resource.BankType,
            Attach = resource.Attachment,
            SuccessTime = FormatSuccessTime(resource.SuccessTime),
            OpenID = resource.Payer?.OpenId,
            Amount = resource.Amount != null ? new OrderAmountDTO
            {
                Total = resource.Amount.Total,
                PayerTotal = resource.Amount.PayerTotal ?? 0,
                Currency = resource.Amount.Currency,
                PayerCurrency = resource.Amount.PayerCurrency
            } : null
        };

        return Task.FromResult(result);
    }

    /// <summary>
    /// 获取微信支付客户端
    /// </summary>
    private WechatTenpayClient GetWechatTenpayClient(string configKey = "Default")
    {
        string cacheKey = $"WechatTenpayClient_{configKey}";
        WechatTenpayClient result = cache.GetOrCreate(cacheKey, entry =>
        {
            WechatTenpayClientOptions options = GetWechatTenpayClientOptions(configKey);
            return new WechatTenpayClient(options);
        }) ?? throw new WechatModuleException("创建微信支付客户端缓存失败");
        return result;
    }

    /// <summary>
    /// 获取微信支付客户端配置
    /// </summary>
    private WechatTenpayClientOptions GetWechatTenpayClientOptions(string configKey = "Default")
    {
        WechatOptions wechatOptions = GetWechatOptions(configKey);

        // 读取私钥
        string privateKey = ReadKeyFile(wechatOptions.PrivateKeyPath);

        WechatTenpayClientOptions result = new()
        {
            MerchantId = wechatOptions.MchId ?? string.Empty,
            MerchantV3Secret = wechatOptions.ApiV3Key ?? string.Empty,
            MerchantCertificateSerialNumber = wechatOptions.CertificateSerialNumber ?? string.Empty,
            MerchantCertificatePrivateKey = privateKey
        };

        // 配置平台公钥认证（新商户，2024年10月后注册）
        if (!string.IsNullOrWhiteSpace(wechatOptions.PlatformPublicKeyId) && !string.IsNullOrWhiteSpace(wechatOptions.PlatformPublicKeyPath))
        {
            string platformPublicKey = ReadKeyFile(wechatOptions.PlatformPublicKeyPath);
            result.PlatformAuthScheme = PlatformAuthScheme.PublicKey;
            InMemoryPublicKeyManager manager = new();
            manager.AddEntry(new PublicKeyEntry("RSA", wechatOptions.PlatformPublicKeyId, platformPublicKey));
            result.PlatformPublicKeyManager = manager;
        }

        return result;
    }

    /// <summary>
    /// 读取密钥文件内容
    /// </summary>
    /// <param name="path">文件路径（支持绝对路径和相对路径）</param>
    /// <returns>密钥内容</returns>
    private static string ReadKeyFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;

        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        string absolutePath = Path.Combine(typeof(WechatPayServiceImpl).Assembly.GetDirectoryPath(), path);
        if (File.Exists(absolutePath)) return File.ReadAllText(absolutePath);

        return string.Empty;
    }

    /// <summary>
    /// 获取微信配置
    /// </summary>
    private WechatOptions GetWechatOptions(string configKey = "Default")
    {
        WechatOptions? wechatOptions = config.CurrentValue.WechatOptions.FirstOrDefault(m => m.Key == configKey);
        if (wechatOptions is null && configKey == "Default")
        {
            wechatOptions = config.CurrentValue.WechatOptions.FirstOrDefault();
        }
        if (wechatOptions is null)
        {
            throw new WechatModuleException($"获取配置项{configKey}失败");
        }
        return wechatOptions;
    }

    /// <summary>
    /// 格式化支付成功时间
    /// </summary>
    private static string FormatSuccessTime(DateTimeOffset successTime) => successTime.ToString("yyyy-MM-ddTHH:mm:sszzz");

    /// <summary>
    /// 映射订单查询响应
    /// </summary>
    private static OrderQueryResponseDTO MapToOrderQueryResponse(GetPayTransactionByOutTradeNumberResponse response) => new OrderQueryResponseDTO
    {
        TransactionId = response.TransactionId,
        OutTradeNo = response.OutTradeNumber,
        TradeState = response.TradeState,
        TradeStateDesc = response.TradeStateDescription ?? string.Empty,
        BankType = response.BankType,
        Attach = response.Attachment,
        SuccessTime = response.SuccessTime.HasValue ? FormatSuccessTime(response.SuccessTime.Value) : null,
        OpenID = response.Payer?.OpenId,
        Amount = response.Amount != null ? new OrderAmountDTO
        {
            Total = response.Amount.Total,
            PayerTotal = response.Amount.PayerTotal ?? 0,
            Currency = response.Amount.Currency,
            PayerCurrency = response.Amount.PayerCurrency
        } : null
    };

    /// <summary>
    /// 映射订单查询响应
    /// </summary>
    private static OrderQueryResponseDTO MapToOrderQueryResponse(GetPayTransactionByIdResponse response) => new OrderQueryResponseDTO
    {
        TransactionId = response.TransactionId,
        OutTradeNo = response.OutTradeNumber,
        TradeState = response.TradeState,
        TradeStateDesc = response.TradeStateDescription ?? string.Empty,
        BankType = response.BankType,
        Attach = response.Attachment,
        SuccessTime = response.SuccessTime.HasValue ? FormatSuccessTime(response.SuccessTime.Value) : null,
        OpenID = response.Payer?.OpenId,
        Amount = response.Amount != null ? new OrderAmountDTO
        {
            Total = response.Amount.Total,
            PayerTotal = response.Amount.PayerTotal ?? 0,
            Currency = response.Amount.Currency,
            PayerCurrency = response.Amount.PayerCurrency
        } : null
    };
}
