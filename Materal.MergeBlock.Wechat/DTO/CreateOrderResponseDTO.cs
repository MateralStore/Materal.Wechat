namespace Materal.MergeBlock.Wechat.DTO;

/// <summary>
/// 创建订单响应DTO
/// </summary>
public class CreateOrderResponseDTO
{
    /// <summary>
    /// 预支付交易会话标识
    /// </summary>
    public string PrepayID { get; set; } = string.Empty;

    /// <summary>
    /// 应用ID
    /// </summary>
    public string AppID { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    public string TimeStamp { get; set; } = string.Empty;

    /// <summary>
    /// 随机字符串
    /// </summary>
    public string NonceStr { get; set; } = string.Empty;

    /// <summary>
    /// 订单详情扩展字符串
    /// </summary>
    public string Package { get; set; } = string.Empty;

    /// <summary>
    /// 签名方式
    /// </summary>
    public string SignType { get; set; } = string.Empty;

    /// <summary>
    /// 签名
    /// </summary>
    public string PaySign { get; set; } = string.Empty;
}
