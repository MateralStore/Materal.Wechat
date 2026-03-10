import { Link } from 'react-router-dom'
import './Home.css'

interface TestModule {
  id: string
  title: string
  path: string
}

const testModules: TestModule[] = [
  { id: 'settings', title: '系统设置', path: '/settings' },
  { id: 'wechat-auth', title: '微信授权', path: '/test/wechat-auth' },
  { id: 'wechat-pay', title: '微信支付', path: '/test/wechat-pay' }
]

const Home: React.FC = () => {
  return (
    <div className="home">
      {testModules.map((module) => (
        <Link key={module.id} to={module.path} className="module-card">
          {module.title}
        </Link>
      ))}
    </div>
  )
}

export default Home
