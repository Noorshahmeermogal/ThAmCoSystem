import { useState, useEffect } from 'react'
import { Routes, Route, Link, useNavigate } from 'react-router-dom'
import './App.css'
import { AppBar, Toolbar, Typography, Container, CircularProgress, Box, Card, CardContent, Grid, TextField, MenuItem, Select, FormControl, InputLabel, Button } from '@mui/material';
import { useAuth } from './AuthContext';
import Login from './components/Auth/Login';
import Register from './components/Auth/Register';
import PrivateRoute from './components/PrivateRoute';
import Profile from './components/Customer/Profile';
import ProductDetail from './components/Product/ProductDetail';
import OrderHistory from './components/Customer/OrderHistory';
import StaffPrivateRoute from './components/Staff/StaffPrivateRoute';
import StaffOrders from './components/Staff/StaffOrders';
import StaffCustomers from './components/Staff/StaffCustomers';
import StaffProducts from './components/Staff/StaffProducts';
import CreateOrder from './components/Customer/CreateOrder';
import AddFunds from './components/Customer/AddFunds';
import { API_BASE_URL } from './config';

function ProductListings() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/api/products/categories`);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        setCategories(data);
      } catch (e) {
        console.error("Error fetching categories:", e);
      }
    };

    fetchCategories();
  }, []);

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);
      setError(null);
      try {
        const queryParams = new URLSearchParams();
        if (selectedCategory) {
          queryParams.append('category', selectedCategory);
        }
        if (searchTerm) {
          queryParams.append('search', searchTerm);
        }

        const response = await fetch(`${API_BASE_URL}/api/products?${queryParams.toString()}`);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        setProducts(data);
      } catch (e) {
        setError(e.message);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [selectedCategory, searchTerm]);

  const handleCategoryChange = (event) => {
    setSelectedCategory(event.target.value);
  };

  const handleSearchChange = (event) => {
    setSearchTerm(event.target.value);
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', color: 'error.main' }}>
        <Typography variant="h6">Error: {error}</Typography>
      </Box>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Available Products
      </Typography>
      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <TextField
          label="Search Products"
          variant="outlined"
          value={searchTerm}
          onChange={handleSearchChange}
          sx={{ flexGrow: 1 }}
        />
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Category</InputLabel>
          <Select
            value={selectedCategory}
            onChange={handleCategoryChange}
            label="Category"
          >
            <MenuItem value="">All Categories</MenuItem>
            {categories.map((category) => (
              <MenuItem key={category} value={category}>{category}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>
      <Grid container spacing={4}>
        {products.map((product) => (
          <Grid item key={product.id} xs={12} sm={6} md={4}>
            <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column', cursor: 'pointer' }} onClick={() => window.location.href = `/products/${product.id}`}>
              <CardContent sx={{ flexGrow: 1 }}>
                <Typography gutterBottom variant="h5" component="h2">
                  {product.name}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {product.description}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Category: {product.category}
                </Typography>
                <Typography variant="h6" color="primary.main">
                  £{product.price?.toFixed(2) || 'N/A'}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Stock: {product.stockQuantity}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
      {products.length === 0 && (
        <Typography variant="h6" sx={{ mt: 4 }}>
          No products found.
        </Typography>
      )}
    </Container>
  );
}

function App() {
  const { user, isAuthenticated, isStaff, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <Box sx={{ flexGrow: 1 }}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            ThAmCo
          </Typography>
          <Box sx={{ display: 'flex', gap: 2 }}>
            {isAuthenticated() ? (
              <>
                <Button color="inherit" component={Link} to="/">
                  Products
                </Button>
                <Button color="inherit" component={Link} to="/profile">
                  Profile
                </Button>
                <Button color="inherit" component={Link} to="/orders">
                  Orders
                </Button>
                {isAuthenticated() && user && user.role === "Customer" && (
                  <>
                    <Button color="inherit" component={Link} to="/create-order">
                      Create Order
                    </Button>
                    <Button color="inherit" component={Link} to="/add-funds">
                      Add Funds
                    </Button>
                  </>
                )}
                {isStaff() && (
                  <>
                    <Button color="inherit" component={Link} to="/staff/orders">
                      Manage Orders
                    </Button>
                    <Button color="inherit" component={Link} to="/staff/customers">
                      Manage Customers
                    </Button>
                    <Button color="inherit" component={Link} to="/staff/products">
                      Manage Products
                    </Button>
                  </>
                )}
                <Button color="inherit" onClick={handleLogout}>
                  Logout
                </Button>
              </>
            ) : (
              <>
                <Button color="inherit" component={Link} to="/login">
                  Login
                </Button>
                <Button color="inherit" component={Link} to="/register">
                  Register
                </Button>
              </>
            )}
          </Box>
        </Toolbar>
      </AppBar>
      <Routes>
        <Route path="/" element={<ProductListings />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route
          path="/profile"
          element={
            <PrivateRoute>
              <Profile />
            </PrivateRoute>
          }
        />
        <Route
          path="/orders"
          element={
            <PrivateRoute>
              <OrderHistory />
            </PrivateRoute>
          }
        />
        <Route
          path="/staff/orders"
          element={
            <StaffPrivateRoute>
              <StaffOrders />
            </StaffPrivateRoute>
          }
        />
        <Route
          path="/staff/customers"
          element={
            <StaffPrivateRoute>
              <StaffCustomers />
            </StaffPrivateRoute>
          }
        />
        <Route
          path="/staff/products"
          element={
            <StaffPrivateRoute>
              <StaffProducts />
            </StaffPrivateRoute>
          }
        />
        <Route path="/products/:id" element={<ProductDetail />} />
        <Route path="/create-order" element={<PrivateRoute><CreateOrder /></PrivateRoute>} />
        <Route path="/add-funds" element={<PrivateRoute><AddFunds /></PrivateRoute>} />
      </Routes>
    </Box>
  );
}

export default App;
