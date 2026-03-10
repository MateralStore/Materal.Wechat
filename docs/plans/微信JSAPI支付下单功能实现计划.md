# 微信 JSAPI 支付下单功能实现计划

## Context

在现有 Materal.Wechat 项目基础上，添加微信 JSAPI 支付下单功能。后端使用 SKIT.FlurlHttpClient.Wechat.TenpayV3 SDK，前端使用 React + TypeScript。

**核心流程**：
1. 前端提交订单信息（金额、描述、OpenID）→ 后端
2. 后端调用微信支付 JSAPI 下单 API → 获取 prepay_id
3. 后端生成前端支付参数（含签名）→ 返回前端
4. 前端调用 wx.chooseWXPay → 调起微信支付
5. 微信服务器回调 → 后端处理支付结果通知

---

## 一、后端实现

### 1.1 添加 NuGet 包

**文件**: `Directory.Packages.props`
```xml
<PackageVersion Include="SKIT.FlurlHttpClient.Wechat.TenpayV3" Version="3.x.x" />
```

**文件**: `Materal.MergeBlock.Wechat/Materal.MergeBlock.Wechat.csproj`
```xml
<PackageReference Include="SKIT.FlurlHttpClient.Wechat.TenpayV3" />
```

### 1.2 新建文件列表

| 文件路径 | 说明 |
|---------|------|
| `Services/IWechatPayService.cs` | 支付服务接口 |
| `Services/WechatPayServiceImpl.cs` | 支付服务实现 |
| `DTO/CreateOrderRequestDTO.cs` | 创建订单请求 |
| `DTO/CreateOrderResponseDTO.cs` | 创建订单响应 |
| `DTO/OrderQueryResponseDTO.cs` | 订单查询响应 |
| `DTO/JsapiPayParametersDTO.cs` | JSAPI支付参数 |

### 1.3 修改现有文件

**文件**: `WechatOptions.cs` - 添加支付相关配置属性
```csharp
/// <summary>
/// 微信商户号
/// </summary>
public string? MchId { get; set; }

/// <summary>
/// 微信商户V3 API密钥
/// </summary>
public string? ApiV3Key { get; set; }

/// <summary>
/// 商户证书序列号
/// </summary>
public string? CertificateSerialNumber { get; set; }

/// <summary>
/// 商户私钥文件路径（PEM格式）
/// </summary>
public string? PrivateKeyPath { get; set; }

/// <summary>
/// 支付回调通知地址
/// </summary>
public string? NotifyUrl { get; set; }
```

### 1.4 核心服务实现

**IWechatPayService 接口方法**:
- `CreateJsapiOrderAsync` - 创建JSAPI订单，返回prepay_id和支付参数
- `QueryOrderByOutTradeNoAsync` - 根据商户订单号查询
- `QueryOrderByTransactionIdAsync` - 根据微信订单号查询
- `HandlePayNotifyAsync` - 处理支付回调通知

**WechatPayServiceImpl 关键逻辑**:
- 从 `WechatOptions` 获取支付配置（MchId、ApiV3Key、PrivateKeyPath 等）
- 从 `PrivateKeyPath` 指定的文件路径读取私钥内容
- 使用 `IMemoryCache` 缓存 `WechatTenpayClient`（参考现有 `WechatServiceImpl` 模式）
- 调用 SDK 的 `ExecuteCreatePayTransactionJsapiAsync` 下单
- 使用 SDK 的 `GenerateParameterSignatureForJsapiSdk` 生成前端支付签名
- 回调验签使用 `VerifyEventSignature` 和 `DecryptEventResource`

### 1.5 控制器

**文件**: `Materal.Wechat.Application/Controllers/WechatPayController.cs`

| API | 方法 | 路径 | 说明 |
|-----|------|------|------|
| CreateJsapiOrder | POST | `/WechatAPI/WechatPay/CreateJsapiOrder` | 创建订单 |
| QueryOrderByOutTradeNo | GET | `/WechatAPI/WechatPay/QueryOrderByOutTradeNo` | 查询订单 |
| QueryOrderByTransactionId | GET | `/WechatAPI/WechatPay/QueryOrderByTransactionId` | 查询订单 |
| PayNotify | POST | `/WechatAPI/WechatPay/PayNotify` | 支付回调 |

### 1.6 配置示例

```json
{
  "Wechat": {
    "WechatOptions": [{
      "Key": "Default",
      "AppID": "wx1234567890abcdef",
      "AppSecret": "your-app-secret",
      "MchId": "商户号",
      "ApiV3Key": "V3密钥(32位)",
      "CertificateSerialNumber": "证书序列号",
      "PrivateKeyPath": "/path/to/apiclient_key.pem",
      "NotifyUrl": "https://domain/WechatAPI/WechatPay/PayNotify"
    }]
  }
}
```

---

## 二、前端实现

### 2.1 新建文件列表

| 文件路径 | 说明 |
|---------|------|
| `src/utils/wechat-jsapi.ts` | 微信JS-SDK工具函数 |
| `src/pages/WechatPay.tsx` | 支付测试页面 |
| `src/pages/WechatPay.css` | 页面样式 |

### 2.2 修改现有文件

| 文件路径 | 修改内容 |
|---------|---------|
| `src/router/index.tsx` | 添加 `/test/wechat-pay` 路由 |
| `src/pages/Home.tsx` | 添加"微信支付"入口卡片 |

### 2.3 核心逻辑

**wechat-jsapi.ts**:
- `invokeWechatPay()` - 调起微信支付
- `isWechatEnv()` - 检测微信环境

**WechatPay.tsx**:
- 表单：订单号、金额（分）、描述、OpenID
- 调用 `/WechatAPI/WechatPay/CreateJsapiOrder` 创建订单
- 获取支付参数后调用 `wx.chooseWXPay`
- 支持订单查询功能

---

## 三、关键文件路径

### 后端（需创建/修改）
- `E:\Project\Materal\Materal.Wechat\Directory.Packages.props` - 添加SDK版本
- `E:\Project\Materal\Materal.Wechat\Materal.MergeBlock.Wechat\Materal.MergeBlock.Wechat.csproj` - 添加SDK引用
- `E:\Project\Materal\Materal.Wechat\Materal.MergeBlock.Wechat\WechatOptions.cs` - 添加支付配置属性
- `E:\Project\Materal\Materal.Wechat\Materal.MergeBlock.Wechat\Services\IWechatPayService.cs` - 新建
- `E:\Project\Materal\Materal.Wechat\Materal.MergeBlock.Wechat\Services\WechatPayServiceImpl.cs` - 新建
- `E:\Project\Materal\Materal.Wechat\Materal.MergeBlock.Wechat\DTO\*.cs` - 新建多个DTO
- `E:\Project\Materal\Materal.Wechat\Materal.Wechat.Application\Controllers\WechatPayController.cs` - 新建

### 前端（需创建/修改）
- `E:\Project\Materal\Materal.Wechat\Materal.Wechat.WebTest\src\utils\wechat-jsapi.ts` - 新建
- `E:\Project\Materal\Materal.Wechat\Materal.Wechat.WebTest\src\pages\WechatPay.tsx` - 新建
- `E:\Project\Materal\Materal.Wechat\Materal.Wechat.WebTest\src\pages\WechatPay.css` - 新建
- `E:\Project\Materal\Materal.Wechat\Materal.Wechat.WebTest\src\router\index.tsx` - 修改
- `E:\Project\Materal\Materal.Wechat\Materal.Wechat.WebTest\src\pages\Home.tsx` - 修改

---

## 四、验证方式

1. **后端验证**：
   - 构建项目：`dotnet build Materal.Wechat.slnx`
   - 检查 Swagger 文档确认 API 可访问

2. **前端验证**：
   - 启动后端服务
   - 在微信环境中访问测试页面
   - 完成授权获取 OpenID
   - 创建测试订单（金额1分）
   - 调起微信支付

3. **回调验证**：
   - 使用内网穿透工具暴露本地服务
   - 或部署到测试服务器验证回调

---

## 五、注意事项

1. **证书配置**：商户私钥通过 `PrivateKeyPath` 指定文件路径，运行时从文件读取 PEM 格式私钥
2. **回调地址**：必须是 HTTPS 且已备案，需在商户平台配置
3. **金额单位**：所有金额单位为分（整数）
4. **prepay_id 有效期**：2小时，超时需重新下单
5. **订单号唯一性**：`OutTradeNo` 在商户系统中必须唯一
