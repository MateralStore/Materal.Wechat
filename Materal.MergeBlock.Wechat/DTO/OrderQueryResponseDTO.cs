namespace Materal.MergeBlock.Wechat.DTO;

/// <summary>
/// 订单查询响应DTO
/// </summary>
public class OrderQueryResponseDTO
{
    /// <summary>
    /// 微信支付订单号
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// 商户订单号
    /// </summary>
    public string OutTradeNo { get; set; } = string.Empty;

    /// <summary>
    /// 交易状态
    /// </summary>
    public string TradeState { get; set; } = string.Empty;

    /// <summary>
    /// 交易状态描述
    /// </summary>
    public string? TradeStateDesc { get; set; }

    /// <summary>
    /// 付款银行
    /// </summary>
    public string? BankType { get; set; }

    /// <summary>
    /// 附加数据
    /// </summary>
    public string? Attach { get; set; }

    /// <summary>
    /// 支付完成时间
    /// </summary>
    public string? SuccessTime { get; set; }

    /// <summary>
    /// 用户标识
    /// </summary>
    public string? OpenID { get; set; }

    /// <summary>
    /// 订单金额信息
    /// </summary>
    public OrderAmountDTO? Amount { get; set; }
}
