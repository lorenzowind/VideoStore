import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Typography, Layout, Menu } from 'antd';
import { 
  ShopOutlined, 
  UserOutlined,
  VideoCameraOutlined 
} from '@ant-design/icons';

const { Sider } = Layout;
const { Title } = Typography;

import { LogoContainer } from './styles';

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  
  const [selectedKey] = useState(() => {
    switch (location.pathname) {
      case '/rentals': return '1';
      case '/customers': return '2';
      case '/movies': return '3';
      default: return '1';
    }
  });
  const [collapsed, setCollapsed] = useState(false);

  return (
    <Sider collapsible collapsed={collapsed} onCollapse={setCollapsed}>
      <LogoContainer>
        <Title level={3}>{!collapsed ? "VideoStore" : "VS"}</Title>
      </LogoContainer>
      <Menu theme="dark" defaultSelectedKeys={[selectedKey]} mode="inline">
        <Menu.Item 
          key="1" 
          icon={<ShopOutlined />} 
          onClick={() => navigate("/rentals")}
        >
          Locações
        </Menu.Item>
        
        <Menu.Item 
          key="2" 
          icon={<UserOutlined />}
          onClick={() => navigate("/customers")}
        >
          Clientes
        </Menu.Item>

        <Menu.Item 
          key="3" 
          icon={<VideoCameraOutlined />}
          onClick={() => navigate("/movies")}
        >
          Filmes
        </Menu.Item>
      </Menu>
    </Sider>
  );
}

export default Dashboard;
