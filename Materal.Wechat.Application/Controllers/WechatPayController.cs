using Materal.MergeBlock.Wechat.DTO;
using Materal.MergeBlock.Wechat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Materal.Wechat.Application.Controllers;

/// <summary>
/// 微信支付控制器
/// </summary>
/// <param name="wechatPayService"></param>
[AllowAnonymous]
public class WechatPayController(IWechatPayService wechatPayService) : WechatBaseController
{
    /// <summary>
    /// 微信支付服务
    /// </summary>
    protected IWechatPayService WechatPayService { get; } = wechatPayService;

    /// <summary>
    /// 创建JSAPI订单
    /// </summary>
    /// <param name="request">创建订单请求</param>
    /// <returns>创建订单响应</returns>
    [HttpPost]
    public async Task<ResultModel<CreateOrderResponseDTO>> CreateJsapiOrderAsync([FromBody] CreateOrderRequestDTO request)
    {
        CreateOrderResponseDTO result = await WechatPayService.CreateJsapiOrderAsync(request);
        return ResultModel<CreateOrderResponseDTO>.Success(result, "下单成功");
    }

    /// <summary>
    /// 根据商户订单号查询订单
    /// </summary>
    /// <param name="outTradeNo">商户订单号</param>
    /// <returns>订单查询响应</returns>
    [HttpGet]
    public async Task<ResultModel<OrderQueryResponseDTO>> QueryOrderByOutTradeNoAsync([FromQuery] string outTradeNo)
    {
        OrderQueryResponseDTO result = await WechatPayService.QueryOrderByOutTradeNoAsync(outTradeNo);
        return ResultModel<OrderQueryResponseDTO>.Success(result, "查询成功");
    }

    /// <summary>
    /// 根据微信订单号查询订单
    /// </summary>
    /// <param name="transactionId">微信订单号</param>
    /// <returns>订单查询响应</returns>
    [HttpGet]
    public async Task<ResultModel<OrderQueryResponseDTO>> QueryOrderByTransactionIdAsync([FromQuery] string transactionId)
    {
        OrderQueryResponseDTO result = await WechatPayService.QueryOrderByTransactionIdAsync(transactionId);
        return ResultModel<OrderQueryResponseDTO>.Success(result, "查询成功");
    }

    /// <summary>
    /// 支付回调通知
    /// </summary>
    /// <returns>回调响应</returns>
    [HttpPost]
    public async Task<IActionResult> PayNotifyAsync()
    {
        string? body = null;
        string? signature = null;
        string? timestamp = null;
        string? nonce = null;
        string? serial = null;
        OrderQueryResponseDTO? result = null;
        string? errorMessage = null;

        try
        {
            // 读取请求体
            using var reader = new StreamReader(Request.Body);
            body = await reader.ReadToEndAsync();

            // 获取签名相关头部
            signature = Request.Headers["Wechatpay-Signature"].ToString();
            timestamp = Request.Headers["Wechatpay-Timestamp"].ToString();
            nonce = Request.Headers["Wechatpay-Nonce"].ToString();
            serial = Request.Headers["Wechatpay-Serial"].ToString();

            // 处理回调
            result = await WechatPayService.HandlePayNotifyAsync(body, signature, timestamp, nonce, serial);

            // 返回成功响应
            return Content("{\"code\":\"SUCCESS\",\"message\":\"成功\"}", "application/json");
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            // 返回失败响应
            return Content($"{{\"code\":\"FAIL\",\"message\":\"{ex.Message}\"}}", "application/json");
        }
        finally
        {
            // 记录回调日志到文件
            await SavePayNotifyLogAsync(body, signature, timestamp, nonce, serial, result, errorMessage);
        }
    }

    /// <summary>
    /// 保存支付回调日志到文件
    /// </summary>
    private static async Task SavePayNotifyLogAsync(string? body, string? signature, string? timestamp, string? nonce, string? serial, OrderQueryResponseDTO? result, string? errorMessage)
    {
        try
        {
            // 创建日志目录
            string logDirectory = Path.Combine(typeof(WechatApplicationModule).Assembly.GetDirectoryPath(), "PayNotifys");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 生成唯一文件名
            string fileName = $"PayNotify_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.json";
            string filePath = Path.Combine(logDirectory, fileName);

            // 构建日志内容
            var logContent = new
            {
                LogTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Request = new
                {
                    Body = body,
                    Headers = new
                    {
                        Wechatpay_Signature = signature,
                        Wechatpay_Timestamp = timestamp,
                        Wechatpay_Nonce = nonce,
                        Wechatpay_Serial = serial
                    }
                },
                Response = new
                {
                    Result = result,
                    Error = errorMessage
                },
                Status = string.IsNullOrEmpty(errorMessage) ? "Success" : "Failed"
            };

            // 写入文件
            string jsonContent = logContent.ToJson();
            await System.IO.File.WriteAllTextAsync(filePath, jsonContent);
        }
        catch
        {
            // 日志记录失败不影响主流程
        }
    }
}
