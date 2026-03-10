using Materal.MergeBlock.Wechat.DTO;

namespace Materal.MergeBlock.Wechat.Services;

/// <summary>
/// 微信支付服务接口
/// </summary>
public interface IWechatPayService
{
    /// <summary>
    /// 创建JSAPI订单
    /// </summary>
    /// <param name="request">创建订单请求</param>
    /// <param name="configKey">配置Key</param>
    /// <returns>创建订单响应</returns>
    Task<CreateOrderResponseDTO> CreateJsapiOrderAsync(CreateOrderRequestDTO request, string configKey = "Default");

    /// <summary>
    /// 根据商户订单号查询订单
    /// </summary>
    /// <param name="outTradeNo">商户订单号</param>
    /// <param name="configKey">配置Key</param>
    /// <returns>订单查询响应</returns>
    Task<OrderQueryResponseDTO> QueryOrderByOutTradeNoAsync(string outTradeNo, string configKey = "Default");

    /// <summary>
    /// 根据微信订单号查询订单
    /// </summary>
    /// <param name="transactionId">微信订单号</param>
    /// <param name="configKey">配置Key</param>
    /// <returns>订单查询响应</returns>
    Task<OrderQueryResponseDTO> QueryOrderByTransactionIdAsync(string transactionId, string configKey = "Default");

    /// <summary>
    /// 处理支付回调通知
    /// </summary>
    /// <param name="body">回调请求体</param>
    /// <param name="signature">签名</param>
    /// <param name="timestamp">时间戳</param>
    /// <param name="nonce">随机串</param>
    /// <param name="serial">证书序列号</param>
    /// <param name="configKey">配置Key</param>
    /// <returns>解密后的订单数据</returns>
    Task<OrderQueryResponseDTO> HandlePayNotifyAsync(string body, string signature, string timestamp, string nonce, string serial, string configKey = "Default");
}
