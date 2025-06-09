import React, { useState, useEffect } from 'react';
import { TextField, Button, Typography, Container, Box, Grid, Card, CardContent, CardActions, MenuItem, Select, FormControl, InputLabel, Snackbar, Alert } from '@mui/material';
import { useAuth } from '../../AuthContext';
import { API_BASE_URL } from '../../config';
import { Link } from 'react-router-dom';

const CreateOrder = () => {
  const { user, token } = useAuth();
  const [products, setProducts] = useState([]);
  const [selectedProducts, setSelectedProducts] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState("");
  const [snackbarSeverity, setSnackbarSeverity] = useState("success");

  const hasRequiredProfileInfo = user && user.deliveryAddress && user.phoneNumber;

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/api/products`);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        setProducts(data);
      } catch (error) {
        setError(error);
        setSnackbarMessage("Failed to fetch products.");
        setSnackbarSeverity("error");
        setSnackbarOpen(true);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);

  const handleQuantityChange = (productId, quantity) => {
    setSelectedProducts(prev => ({
      ...prev,
      [productId]: quantity,
    }));
  };

  const calculateTotal = () => {
    return Object.entries(selectedProducts).reduce((total, [productId, quantity]) => {
      const product = products.find(p => p.id === parseInt(productId));
      return total + (product ? product.price * quantity : 0);
    }, 0);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);

    if (!user || !token) {
      setSnackbarMessage("You must be logged in to create an order.");
      setSnackbarSeverity("error");
      setSnackbarOpen(true);
      return;
    }

    if (!hasRequiredProfileInfo) {
        setSnackbarMessage("Please update your profile with a delivery address and phone number before placing an order.");
        setSnackbarSeverity("error");
        setSnackbarOpen(true);
        return;
    }

    const orderItems = Object.entries(selectedProducts)
      .filter(([, quantity]) => quantity > 0)
      .map(([productId, quantity]) => ({
        productId: parseInt(productId),
        quantity: parseInt(quantity),
      }));

    if (orderItems.length === 0) {
      setSnackbarMessage("Please select at least one product.");
      setSnackbarSeverity("warning");
      setSnackbarOpen(true);
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/api/orders`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({ items: orderItems }),
      });

      const responseData = await response.json();

      if (!response.ok) {
        throw new Error(responseData.message || 'Failed to create order');
      }

      setSnackbarMessage("Order created successfully!");
      setSnackbarSeverity("success");
      setSnackbarOpen(true);
      setSelectedProducts({}); // Clear selected products
    } catch (error) {
      setError(error);
      setSnackbarMessage(error.message || "An error occurred during order creation.");
      setSnackbarSeverity("error");
      setSnackbarOpen(true);
    }
  };

  const handleCloseSnackbar = () => {
    setSnackbarOpen(false);
  };

  if (loading) return <Typography>Loading products...</Typography>;
  if (error) return <Typography color="error">Error: {error.message}</Typography>;

  if (!user) {
    return (
      <Container maxWidth="md" sx={{ my: 4, textAlign: 'center' }}>
        <Typography variant="h6" color="error">You must be logged in to create an order.</Typography>
        <Button component={Link} to="/login" variant="contained" sx={{ mt: 2 }}>Login</Button>
      </Container>
    );
  }

  if (!hasRequiredProfileInfo) {
    return (
      <Container maxWidth="md" sx={{ my: 4, textAlign: 'center' }}>
        <Typography variant="h6" color="error">
          Please update your profile with a delivery address and phone number to place an order.
        </Typography>
        <Button component={Link} to="/profile" variant="contained" sx={{ mt: 2 }}>Update Profile</Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="md">
      <Box sx={{ my: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Create New Order
        </Typography>
        <form onSubmit={handleSubmit}>
          <Grid container spacing={3}>
            {products.map(product => (
              <Grid item xs={12} sm={6} md={4} key={product.id}>
                <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                  <CardContent sx={{ flexGrow: 1 }}>
                    <Typography variant="h6">{product.name}</Typography>
                    <Typography color="text.secondary">£{product.price?.toFixed(2) || 'N/A'}</Typography>
                    <Typography variant="body2" color="text.secondary">Stock: {product.stockQuantity}</Typography>
                    <Typography variant="body2" color="text.secondary">{product.description}</Typography>
                  </CardContent>
                  <CardActions>
                    <FormControl fullWidth size="small">
                      <InputLabel id={`quantity-label-${product.id}`}>Quantity</InputLabel>
                      <Select
                        labelId={`quantity-label-${product.id}`}
                        value={selectedProducts[product.id] || 0}
                        label="Quantity"
                        onChange={(e) => handleQuantityChange(product.id, e.target.value)}
                      >
                        {[...Array(product.stockQuantity + 1).keys()].map(qty => (
                          <MenuItem key={qty} value={qty}>{qty}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </CardActions>
                </Card>
              </Grid>
            ))}
          </Grid>
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h5">Total: £{calculateTotal().toFixed(2)}</Typography>
            <Button
              type="submit"
              variant="contained"
              color="primary"
              disabled={calculateTotal() === 0}
            >
              Place Order
            </Button>
          </Box>
        </form>
      </Box>
      <Snackbar open={snackbarOpen} autoHideDuration={6000} onClose={handleCloseSnackbar}>
        <Alert onClose={handleCloseSnackbar} severity={snackbarSeverity} sx={{ width: '100%' }}>
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </Container>
  );
};

export default CreateOrder; 