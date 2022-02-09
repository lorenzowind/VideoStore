import { BrowserRouter as Router } from 'react-router-dom';

import { ConfigProvider } from 'antd';
import ptBR from 'antd/lib/locale/pt_BR';
import 'antd/dist/antd.css';

import Routes from './routes';

const App: React.FC = () => (
  <ConfigProvider locale={ptBR}>
    <Router>
      <Routes />
    </Router>
  </ConfigProvider> 
);

export default App;
