import { useState, useEffect } from 'react'
import { getApiBaseUrl, setApiBaseUrl, getDefaultApiUrl } from '../utils/api-config'
import './Settings.css'

const Settings: React.FC = () => {
  const [apiUrl, setApiUrlState] = useState('')
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    setApiUrlState(getApiBaseUrl())
  }, [])

  const handleSave = () => {
    setApiBaseUrl(apiUrl)
    setSaved(true)
    setTimeout(() => setSaved(false), 2000)
  }

  const handleReset = () => {
    setApiUrlState(getDefaultApiUrl())
    setApiBaseUrl('')
    setSaved(true)
    setTimeout(() => setSaved(false), 2000)
  }

  const handleClear = () => {
    setApiUrlState('')
    setApiBaseUrl('')
  }

  return (
    <div className="settings">
      <div className="section-title">API 配置</div>

      <div className="test-section">
        <label className="input-label">API 地址</label>
        <input
          type="text"
          className="input-field"
          value={apiUrl}
          onChange={(e) => setApiUrlState(e.target.value)}
          placeholder="请输入后端 API 地址"
        />
        <small className="input-hint">
          默认值: {getDefaultApiUrl() || '(未配置)'}
        </small>
      </div>

      <div className="btn-group">
        <button className="test-btn primary" onClick={handleSave}>
          保存
        </button>
        <button className="test-btn secondary" onClick={handleReset}>
          恢复默认
        </button>
        <button className="test-btn" onClick={handleClear}>
          清空
        </button>
      </div>

      {saved && (
        <div className="result-box success">
          <div className="result-content">已保存</div>
        </div>
      )}

      <div className="divider">
        <span>说明</span>
      </div>

      <div className="info-section">
        <p>此配置用于设置后端 API 的请求地址。</p>
        <p>调试时可根据需要切换不同的后端地址，无需修改 .env 文件。</p>
        <p>配置保存在浏览器 localStorage 中，清除浏览器数据后会重置。</p>
      </div>
    </div>
  )
}

export default Settings
