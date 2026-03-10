import { useState, useEffect } from 'react'
import { useSearchParams } from 'react-router-dom'
import axios from 'axios'
import { getApiBaseUrl } from '../utils/api-config'
import './WechatAuth.css'

interface UserAccessTokenDTO {
  AccessToken: string
  ExpiresIn: number
  RefreshToken: string
  OpenID: string
  Scope: string
  UnionID: string
}

interface ResultModel {
  ResultType: number
  Message: string
  Data: UserAccessTokenDTO | null
}

const STORAGE_KEY_APPID = 'wechat_appid'
const STORAGE_KEY_AUTH = 'wechat_auth'

const getStoredAuth = (): ResultModel | null => {
  const stored = localStorage.getItem(STORAGE_KEY_AUTH)
  return stored ? JSON.parse(stored) : null
}

const WechatAuth: React.FC = () => {
  const [searchParams] = useSearchParams()
  const codeFromUrl = searchParams.get('code')

  const defaultRedirectUri = `${window.location.origin}/WechatTest/test/wechat-auth`

  const [appid, setAppid] = useState(() => localStorage.getItem(STORAGE_KEY_APPID) || '')
  const [redirectUri, setRedirectUri] = useState(defaultRedirectUri)
  const [scope, setScope] = useState<'snsapi_base' | 'snsapi_userinfo'>('snsapi_base')
  const [code, setCode] = useState(() => codeFromUrl || '')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<ResultModel | null>(() => getStoredAuth())
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (codeFromUrl && !result && !loading) {
      handleGetToken(codeFromUrl)
    }
  }, [codeFromUrl])

  useEffect(() => {
    if (appid.trim()) {
      localStorage.setItem(STORAGE_KEY_APPID, appid)
    }
  }, [appid])

  useEffect(() => {
    if (result && result.ResultType === 0 && result.Data) {
      localStorage.setItem(STORAGE_KEY_AUTH, JSON.stringify(result))
    }
  }, [result])

  const handleAuth = () => {
    if (!appid.trim()) {
      setError('请输入 AppID')
      return
    }
    if (!redirectUri.trim()) {
      setError('请输入回调地址')
      return
    }

    const encodedRedirectUri = encodeURIComponent(redirectUri)
    const authUrl = `https://open.weixin.qq.com/connect/oauth2/authorize?appid=${appid}&redirect_uri=${encodedRedirectUri}&response_type=code&scope=${scope}#wechat_redirect`
    window.location.href = authUrl
  }

  const handleGetToken = async (codeValue: string) => {
    if (!codeValue.trim()) {
      setError('请输入 code')
      return
    }

    setLoading(true)
    setError(null)
    setResult(null)

    try {
      const response = await axios.get<ResultModel>(`${getApiBaseUrl()}/WechatAPI/WechatUser/GetUserAccessToken`, {
        params: { code: codeValue }
      })
      setResult(response.data)
    } catch (err) {
      setError(axios.isAxiosError(err) ? err.message : '请求失败')
    } finally {
      setLoading(false)
    }
  }

  const handleTest = () => {
    handleGetToken(code)
  }

  const handleReset = () => {
    setCode('')
    setResult(null)
    setError(null)
    window.history.replaceState({}, '', '/test/wechat-auth')
  }

  return (
    <div className="wechat-auth">
      {!codeFromUrl ? (
        <>
          <div className="test-section">
            <label className="input-label">AppID</label>
            <input
              type="text"
              className="input-field"
              value={appid}
              onChange={(e) => setAppid(e.target.value)}
              placeholder="请输入服务号AppID"
            />
          </div>

          <div className="test-section">
            <label className="input-label">回调地址</label>
            <input
              type="text"
              className="input-field"
              value={redirectUri}
              onChange={(e) => setRedirectUri(e.target.value)}
              placeholder="请输入授权回调地址"
            />
            <small className="input-hint">
              需在公众平台「网页授权域名」中配置该域名
            </small>
          </div>

          <div className="test-section">
            <label className="input-label">授权作用域</label>
            <div className="scope-options">
              <label className={`scope-option ${scope === 'snsapi_base' ? 'active' : ''}`}>
                <input
                  type="radio"
                  name="scope"
                  value="snsapi_base"
                  checked={scope === 'snsapi_base'}
                  onChange={() => setScope('snsapi_base')}
                />
                <span>snsapi_base</span>
                <small>静默授权，仅获取OpenID</small>
              </label>
              <label className={`scope-option ${scope === 'snsapi_userinfo' ? 'active' : ''}`}>
                <input
                  type="radio"
                  name="scope"
                  value="snsapi_userinfo"
                  checked={scope === 'snsapi_userinfo'}
                  onChange={() => setScope('snsapi_userinfo')}
                />
                <span>snsapi_userinfo</span>
                <small>弹出授权，可获取用户信息</small>
              </label>
            </div>
          </div>

          <button className="test-btn primary" onClick={handleAuth}>
            发起微信授权
          </button>
        </>
      ) : (
        <>
          <div className="test-section">
            <label className="input-label">授权码 (code)</label>
            <input
              type="text"
              className="input-field"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              placeholder="请输入微信授权code"
            />
          </div>

          <div className="btn-group">
            <button
              className="test-btn"
              onClick={handleTest}
              disabled={loading}
            >
              {loading ? '请求中...' : '重新测试'}
            </button>
            <button className="test-btn secondary" onClick={handleReset}>
              重新授权
            </button>
          </div>
        </>
      )}

      {error && (
        <div className="result-box error">
          <div className="result-title">错误</div>
          <div className="result-content">{error}</div>
        </div>
      )}

      {result && (
        <div className={`result-box ${result.ResultType === 0 ? 'success' : 'fail'}`}>
          <div className="result-title">
            {result.ResultType === 0 ? '成功' : '失败'}
            {result.Message && <span className="result-message"> - {result.Message}</span>}
          </div>
          {result.Data && (
            <div className="result-content">
              <div className="result-item">
                <span className="item-label">OpenID:</span>
                <span className="item-value">{result.Data.OpenID}</span>
              </div>
              <div className="result-item">
                <span className="item-label">UnionID:</span>
                <span className="item-value">{result.Data.UnionID || '-'}</span>
              </div>
              <div className="result-item">
                <span className="item-label">AccessToken:</span>
                <span className="item-value break-all">{result.Data.AccessToken}</span>
              </div>
              <div className="result-item">
                <span className="item-label">RefreshToken:</span>
                <span className="item-value break-all">{result.Data.RefreshToken}</span>
              </div>
              <div className="result-item">
                <span className="item-label">ExpiresIn:</span>
                <span className="item-value">{result.Data.ExpiresIn} 秒</span>
              </div>
              <div className="result-item">
                <span className="item-label">Scope:</span>
                <span className="item-value">{result.Data.Scope}</span>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

export default WechatAuth
