import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Typography, Container, Box, CircularProgress, Alert, Button, Paper, TextField } from '@mui/material';
import { useAuth } from '../../AuthContext';
import { API_BASE_URL } from '../../config';

function ProductDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user, token, refreshUserProfile } = useAuth();
  const [product, setProduct] = useState(null);
  const [customerFunds, setCustomerFunds] = useState(null);
  const [quantity, setQuantity] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [orderMessage, setOrderMessage] = useState(null);
  const [orderMessageType, setOrderMessageType] = useState(null); // 'success' or 'error'

  useEffect(() => {
    const fetchProductAndFunds = async () => {
      setLoading(true);
      setError(null);
      try {
        // Fetch product details
        const productResponse = await fetch(`${API_BASE_URL}/api/products/${id}`);
        if (!productResponse.ok) {
          throw new Error(`Failed to fetch product: ${productResponse.statusText}`);
        }
        const productData = await productResponse.json();
        setProduct(productData);

        // Fetch customer funds if logged in
        if (user && token) {
          const fundsResponse = await fetch(`${API_BASE_URL}/api/customers/funds`, {
            headers: {
              'Authorization': `Bearer ${token}`,
            },
          });
          if (!fundsResponse.ok) {
            throw new Error(`Failed to fetch funds: ${fundsResponse.statusText}`);
          }
          const fundsData = await fundsResponse.json();
          setCustomerFunds(fundsData.accountFunds);
        }

      } catch (e) {
        console.error("Error fetching product or funds:", e);
        setError(e.message);
      } finally {
        setLoading(false);
      }
    };

    fetchProductAndFunds();
  }, [id, user, token]);

  const handleQuantityChange = (event) => {
    const value = Math.max(1, Number(event.target.value));
    setQuantity(value);
  };

  const handlePlaceOrder = async () => {
    setOrderMessage(null);
    setOrderMessageType(null);
    if (!user || !token) {
      navigate('/login');
      return;
    }

    if (quantity <= 0) {
      setOrderMessage('Quantity must be at least 1.');
      setOrderMessageType('error');
      return;
    }

    if (product.stockQuantity < quantity) {
      setOrderMessage('Insufficient stock for this quantity.');
      setOrderMessageType('error');
      return;
    }

    if (customerFunds !== null && product.price * quantity > customerFunds) {
      setOrderMessage('Insufficient funds.');
      setOrderMessageType('error');
      return;
    }

    console.log('Placing order with user:', user);
    console.log('Sending token:', token);
    console.log('Order payload:', {
      deliveryAddress: user.deliveryAddress || "User's default address",
      phoneNumber: user.phoneNumber || "123-456-7890",
      items: [{
        productId: product.id,
        quantity: quantity,
      }],
    });

    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/orders`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({
          deliveryAddress: user.deliveryAddress || "User's default address",
          phoneNumber: user.phoneNumber || "123-456-7890",
          items: [{
            productId: product.id,
            quantity: quantity,
          }],
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || 'Failed to place order.');
      }

      setOrderMessage('Order placed successfully!');
      setOrderMessageType('success');
      await refreshUserProfile(); // Refresh user profile after successful order
      setLoading(false);
    } catch (e) {
      console.error("Error placing order:", e);
      setOrderMessage(`Error: ${e.message}`);
      setOrderMessageType('error');
      setLoading(false);
    }
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
      <Container component={Paper} elevation={3} sx={{ p: 4, mt: 8, textAlign: 'center' }}>
        <Typography variant="h6" color="error">Error: {error}</Typography>
      </Container>
    );
  }

  if (!product) {
    return (
      <Container component={Paper} elevation={3} sx={{ p: 4, mt: 8, textAlign: 'center' }}>
        <Typography variant="h6">Product not found.</Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
      <Paper elevation={3} sx={{ p: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          {product.name}
        </Typography>
        <Typography variant="body1" color="text.secondary" paragraph>
          {product.description}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Category: {product.category}
        </Typography>
        <Typography variant="h5" color="primary.main" sx={{ mt: 2 }}>
          Price: £{product.price?.toFixed(2) || 'N/A'}
        </Typography>
        <Typography variant="body1" color={product.stockQuantity > 0 ? "text.primary" : "error.main"}>
          Stock: {product.stockQuantity} {product.stockQuantity === 0 ? '(Out of Stock)' : ''}
        </Typography>

        {user && (
          <Box sx={{ mt: 3 }}>
            <Typography variant="body1">Your Available Funds: £{customerFunds?.toFixed(2) || '0.00'}</Typography>
            <TextField
              label="Quantity"
              type="number"
              value={quantity}
              onChange={handleQuantityChange}
              inputProps={{ min: 1, max: product.stockQuantity }}
              sx={{ mt: 2, mb: 2, width: 120 }}
              disabled={product.stockQuantity === 0}
            />
            <Button
              variant="contained"
              color="primary"
              onClick={handlePlaceOrder}
              disabled={product.stockQuantity === 0 || quantity > product.stockQuantity || (customerFunds !== null && product.price * quantity > customerFunds)}
              sx={{ ml: 2, mt: 2 }}
            >
              Place Order
            </Button>
            {orderMessage && (
              <Alert severity={orderMessageType} sx={{ mt: 2 }}>
                {orderMessage}
              </Alert>
            )}
          </Box>
        )}

        {!user && (
          <Alert severity="info" sx={{ mt: 3 }}>
            Please <Link to="/login">log in</Link> to order this product.
          </Alert>
        )}

      </Paper>
    </Container>
  );
}

export default ProductDetail; 