import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../AuthContext';

const StaffPrivateRoute = ({ children }) => {
  const { isAuthenticated, isStaff } = useAuth();
  const location = useLocation();

  if (!isAuthenticated()) {
    // Redirect to login page if not authenticated
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (!isStaff()) {
    // Redirect to home page if authenticated but not staff
    return <Navigate to="/" replace />;
  }

  return children;
};

export default StaffPrivateRoute; 