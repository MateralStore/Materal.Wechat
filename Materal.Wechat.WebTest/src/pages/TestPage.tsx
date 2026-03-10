import { useParams } from 'react-router-dom'
import './TestPage.css'

const TestPage: React.FC = () => {
  const { moduleId } = useParams<{ moduleId: string }>()

  return (
    <div className="test-page">
      <div className="test-placeholder">
        <div className="placeholder-icon">🧪</div>
        <p>测试模块: {moduleId}</p>
        <p className="placeholder-hint">在此处添加具体的测试功能</p>
      </div>
    </div>
  )
}

export default TestPage
