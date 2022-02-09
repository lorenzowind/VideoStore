import { Routes as Switch, Route } from 'react-router-dom';

import Rentals from '../pages/Rentals';
import Customers from '../pages/Customers';
import Movies from '../pages/Movies';

const Routes: React.FC = () => (
  <Switch>
    <Route path="/" element={<Rentals />} />
    <Route path="/rentals" element={<Rentals />} />
    <Route path="/customers" element={<Customers />} />
    <Route path="/movies" element={<Movies />} />
  </Switch>
);

export default Routes;
