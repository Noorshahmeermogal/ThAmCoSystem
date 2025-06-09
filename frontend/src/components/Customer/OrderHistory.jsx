import { useState, useEffect } from 'react';
import { Typography, Container, Box, CircularProgress, Alert, Paper, List, ListItem, ListItemText, Divider } from '@mui/material';
import { useAuth } from '../../AuthContext';
import { API_BASE_URL } from '../../config';

function OrderHistory() {
  const { token } = useAuth();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchOrderHistory = async () => {
      if (!token) {
        setLoading(false);
        setError("Authentication token not found. Please log in.");
        return;
      }

      try {
        const response = await fetch(`${API_BASE_URL}/api/orders/history`, {
          headers: {
            'Authorization': `Bearer ${token}`,
          },
        });
        if (!response.ok) {
          throw new Error(`Failed to fetch order history: ${response.statusText}`);
        }
        const data = await response.json();
        setOrders(data);
      } catch (e) {
        console.error("Error fetching order history:", e);
        setError(e.message);
      } finally {
        setLoading(false);
      }
    };

    fetchOrderHistory();
  }, [token]);

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

  return (
    <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Your Order History
      </Typography>
      <Paper elevation={3} sx={{ p: 4 }}>
        {orders.length === 0 ? (
          <Typography variant="h6" sx={{ mt: 2 }}>No orders found.</Typography>
        ) : (
          <List>
            {orders.map((order) => (
              <Box key={order.id}>
                <ListItem alignItems="flex-start">
                  <ListItemText
                    primary={`Order #${order.orderNumber} - ${order.status}`}
                    secondary={
                      <>
                        <Typography component="span" variant="body2" color="text.primary">
                          Total: £{order.totalAmount?.toFixed(2) || '0.00'}
                        </Typography>
                        <br />
                        <Typography component="span" variant="body2" color="text.secondary">
                          Order Date: {new Date(order.orderDate).toLocaleDateString()}
                        </Typography>
                        <br />
                        <Typography component="span" variant="body2" color="text.secondary">
                          Delivery Address: {order.deliveryAddress}
                        </Typography>
                        <br />
                        <Typography component="span" variant="body2" color="text.secondary">
                          Phone Number: {order.phoneNumber}
                        </Typography>
                        {order.orderItems && order.orderItems.length > 0 && (
                            <Box component="div" sx={{ mt: 1 }}>
                                <Typography variant="body2" component="span" sx={{ fontWeight: 'bold' }}>Items:</Typography>
                                <List dense disablePadding>
                                    {order.orderItems.map(item => (
                                        <ListItem key={item.id} sx={{ pl: 2, pt: 0, pb: 0 }}>
                                            <ListItemText
                                                primary={`- ${item.productName} (x${item.quantity})`}
                                                secondary={`£${item.unitPrice?.toFixed(2) || '0.00'} each`}
                                            />
                                        </ListItem>
                                    ))}
                                </List>
                            </Box>
                        )}
                      </>
                    }
                  />
                </ListItem>
                <Divider component="li" />
              </Box>
            ))}
          </List>
        )}
      </Paper>
    </Container>
  );
}

export default OrderHistory; 