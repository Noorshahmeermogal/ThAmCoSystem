import { useState, useEffect } from 'react';
import { Typography, Container, Box, CircularProgress, Alert, Paper, List, ListItem, ListItemText, Divider, Button, Select, MenuItem, FormControl, InputLabel } from '@mui/material';
import { useAuth } from '../../AuthContext';
import { API_BASE_URL } from '../../config';

function StaffOrders() {
  const { token } = useAuth();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [statusFilter, setStatusFilter] = useState('');

  const fetchOrders = async () => {
    if (!token) {
      setLoading(false);
      setError("Authentication token not found. Please log in as staff.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const queryParams = new URLSearchParams();
      if (statusFilter) {
        queryParams.append('status', statusFilter);
      }
      const response = await fetch(`${API_BASE_URL}/api/orders?${queryParams.toString()}`, {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      if (!response.ok) {
        throw new Error(`Failed to fetch orders: ${response.statusText}`);
      }
      const data = await response.json();
      setOrders(data);
    } catch (e) {
      console.error("Error fetching orders:", e);
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
  }, [token, statusFilter]);

  const handleDispatchOrder = async (orderId) => {
    if (!token) {
      setError("Authentication token not found.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/orders/${orderId}/dispatch`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || 'Failed to dispatch order');
      }

      // Refresh orders after successful dispatch
      fetchOrders();

    } catch (e) {
      console.error("Error dispatching order:", e);
      setError(e.message);
    } finally {
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

  return (
    <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        All Orders (Staff View)
      </Typography>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 3 }}>
        <FormControl sx={{ minWidth: 150 }}>
          <InputLabel>Status Filter</InputLabel>
          <Select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            label="Status Filter"
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Pending">Pending</MenuItem>
            <MenuItem value="Dispatched">Dispatched</MenuItem>
          </Select>
        </FormControl>
      </Box>
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
                        {order.dispatchedDate && (
                          <Typography component="span" variant="body2" color="text.secondary">
                            <br />
                            Dispatched Date: {new Date(order.dispatchedDate).toLocaleDateString()}
                          </Typography>
                        )}
                        <br />
                        <Typography component="span" variant="body2" color="text.secondary">
                          Customer ID: {order.customerId}
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
                {order.status === 'Pending' && (
                  <Button
                    variant="contained"
                    color="primary"
                    size="small"
                    sx={{ ml: 2, mb: 1 }}
                    onClick={() => handleDispatchOrder(order.id)}
                    disabled={loading}
                  >
                    Mark as Dispatched
                  </Button>
                )}
                <Divider component="li" />
              </Box>
            ))}
          </List>
        )}
      </Paper>
    </Container>
  );
}

export default StaffOrders; 