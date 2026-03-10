import { Link } from 'react-router-dom'
import './Home.css'

interface TestModule {
  id: string
  title: string
  path: string
}

const testModules: TestModule[] = [
  { id: 'wechat-auth', title: '微信授权', path: '/test/wechat-auth' },
  { id: 'jsapi-signature', title: 'JSAPI签名', path: '/test/jsapi-signature' },
  { id: 'user-info', title: '用户信息', path: '/test/user-info' },
  { id: 'template-message', title: '模板消息', path: '/test/template-message' }
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
