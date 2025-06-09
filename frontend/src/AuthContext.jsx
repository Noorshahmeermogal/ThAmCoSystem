import React, { createContext, useContext, useState, useEffect } from 'react';
import { API_BASE_URL } from '../src/config';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check for stored auth data on mount
    const storedUser = localStorage.getItem('user');
    const storedToken = localStorage.getItem('token');
    
    if (storedUser && storedToken) {
      setUser(JSON.parse(storedUser));
      setToken(storedToken);
    }
    
    setLoading(false);
  }, []);

  const login = async (email, password, isStaff = false) => {
    try {
      const endpoint = isStaff ? 'staff/login' : 'login';
      const response = await fetch(`${API_BASE_URL}/api/auth/${endpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || 'Login failed');
      }

      setUser(data.user);
      setToken(data.token);
      localStorage.setItem('user', JSON.stringify(data.user));
      localStorage.setItem('token', data.token);
      console.log('Login successful. Received token:', data.token);
      return true;
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  };

  const logout = () => {
    setUser(null);
    setToken(null);
    localStorage.removeItem('user');
    localStorage.removeItem('token');
  };

  const isAuthenticated = () => !!user && !!token;
  
  const isStaff = () => user?.role === 'Staff';
  
  const isAdmin = () => user?.role === 'Admin';

  const updateUser = (newUserData) => {
    setUser(prevUser => ({
      ...prevUser,
      ...newUserData
    }));
    localStorage.setItem('user', JSON.stringify({ ...user, ...newUserData }));
  };

  const refreshUserProfile = async () => {
    if (!user || !token) {
      console.log('refreshUserProfile: User or token not available.');
      return;
    }

    try {
      console.log('refreshUserProfile: Using token', token);
      const response = await fetch(`${API_BASE_URL}/api/customers/profile`, {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        console.error('Failed to refresh user profile:', response.statusText);
        // Optionally handle token expiration or other errors
        logout(); // Log out if profile fetch fails (e.g., token expired)
        return;
      }

      const updatedProfile = await response.json();
      setUser(updatedProfile); // Update user state with fresh data
      localStorage.setItem('user', JSON.stringify(updatedProfile)); // Update localStorage
    } catch (error) {
      console.error('Error refreshing user profile:', error);
      logout(); // Log out on network errors or other failures
    }
  };

  const value = {
    user,
    token,
    loading,
    login,
    logout,
    isAuthenticated,
    isStaff,
    isAdmin,
    updateUser,
    apiBaseUrl: API_BASE_URL,
    refreshUserProfile
  };

  return (
    <AuthContext.Provider value={value}>
      {!loading && children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}; 