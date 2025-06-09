import { Navigate } from 'react-router-dom';
import { useAuth } from '../AuthContext';

const PrivateRoute = ({ children }) => {
  const { user } = useAuth();

  if (!user) {
    // Not logged in, redirect to login page
    return <Navigate to="/login" replace />;
  }

  return children;
};

export default PrivateRoute; 