import { useState, useEffect } from 'react';
import { Typography, Container, Box, CircularProgress, Alert, Paper, List, ListItem, ListItemText, Divider, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Snackbar, DialogContentText } from '@mui/material';
import { useAuth } from '../../AuthContext';
import { API_BASE_URL } from '../../config';

function StaffCustomers() {
  const { token } = useAuth();
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [openDetailDialog, setOpenDetailDialog] = useState(false);
  const [deleteConfirmationOpen, setDeleteConfirmationOpen] = useState(false);
  const [customerToDeleteId, setCustomerToDeleteId] = useState(null);
  const [openAddFundsDialog, setOpenAddFundsDialog] = useState(false);
  const [fundsAmount, setFundsAmount] = useState('');
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState("");
  const [snackbarSeverity, setSnackbarSeverity] = useState("success");

  const fetchCustomers = async () => {
    if (!token) {
      setLoading(false);
      setError("Authentication token not found. Please log in as staff.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/customers`, {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      if (!response.ok) {
        throw new Error(`Failed to fetch customers: ${response.statusText}`);
      }
      const data = await response.json();
      setCustomers(data);
    } catch (e) {
      console.error("Error fetching customers:", e);
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCustomers();
  }, [token]);

  const handleViewDetails = async (customerId) => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/customers/${customerId}`, {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      if (!response.ok) {
        throw new Error(`Failed to fetch customer details: ${response.statusText}`);
      }
      const data = await response.json();
      setSelectedCustomer(data);
      setOpenDetailDialog(true);
    } catch (e) {
      console.error("Error fetching customer details:", e);
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteClick = (customerId) => {
    setCustomerToDeleteId(customerId);
    setDeleteConfirmationOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!token || !customerToDeleteId) return;

    setLoading(true);
    setError(null);
    setDeleteConfirmationOpen(false);
    try {
      const response = await fetch(`${API_BASE_URL}/api/customers/${customerToDeleteId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        throw new Error(`Failed to delete customer: ${response.statusText}`);
      }

      // Refresh the customer list
      fetchCustomers();
      setSelectedCustomer(null);
      setSnackbarMessage("Customer account deleted successfully!");
      setSnackbarSeverity("success");
      setSnackbarOpen(true);

    } catch (e) {
      console.error("Error deleting customer:", e);
      setError(e.message);
      setSnackbarMessage(`Error: ${e.message}`);
      setSnackbarSeverity("error");
      setSnackbarOpen(true);
    } finally {
      setLoading(false);
      setCustomerToDeleteId(null);
    }
  };

  const handleAddFundsClick = (customer) => {
    setSelectedCustomer(customer); // Ensure selectedCustomer is set for the funds dialog
    setFundsAmount(''); // Clear previous amount
    setOpenAddFundsDialog(true);
  };

  const handleAddFundsConfirm = async () => {
    if (!token || !selectedCustomer || fundsAmount <= 0) return;

    setLoading(true);
    setError(null);
    setOpenAddFundsDialog(false);
    try {
      const response = await fetch(`${API_BASE_URL}/api/customers/add-funds`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({ customerId: selectedCustomer.id, amount: parseFloat(fundsAmount) }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || 'Failed to add funds');
      }

      // Refresh the customer list and selected customer details after adding funds
      fetchCustomers();
      // Optionally refresh selected customer if dialog is still open
      if (openDetailDialog) {
          handleViewDetails(selectedCustomer.id); // Re-fetch detailed view
      }
      setSnackbarMessage("Funds added successfully!");
      setSnackbarSeverity("success");
      setSnackbarOpen(true);

    } catch (e) {
      console.error("Error adding funds:", e);
      setError(e.message);
      setSnackbarMessage(`Error: ${e.message}`);
      setSnackbarSeverity("error");
      setSnackbarOpen(true);
    } finally {
      setLoading(false);
      setFundsAmount('');
    }
  };

  const handleCloseDetailDialog = () => {
    setOpenDetailDialog(false);
    setSelectedCustomer(null);
  };

  const handleCloseAddFundsDialog = () => {
    setOpenAddFundsDialog(false);
    setFundsAmount('');
  };

  const handleCloseSnackbar = () => {
    setSnackbarOpen(false);
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
        All Customers (Staff View)
      </Typography>
      <Paper elevation={3} sx={{ p: 4 }}>
        {customers.length === 0 ? (
          <Typography variant="h6" sx={{ mt: 2 }}>No customers found.</Typography>
        ) : (
          <List>
            {customers.map((customer) => (
              <Box key={customer.id}>
                <ListItem
                  alignItems="flex-start"
                  secondaryAction={
                    <>
                      <Button
                        variant="outlined"
                        size="small"
                        sx={{ mr: 1 }}
                        onClick={() => handleViewDetails(customer.id)}
                      >
                        Details
                      </Button>
                      <Button
                        variant="outlined"
                        color="error"
                        size="small"
                        onClick={() => handleDeleteClick(customer.id)}
                      >
                        Delete
                      </Button>
                    </>
                  }
                >
                  <ListItemText
                    primary={customer.name}
                    secondary={
                      <>
                        <Typography component="span" variant="body2" color="text.primary">
                          Email: {customer.email}
                        </Typography>
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

      {/* Customer Detail Dialog */}
      <Dialog open={openDetailDialog} onClose={handleCloseDetailDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Customer Details</DialogTitle>
        <DialogContent>
          {selectedCustomer && (
            <Box>
              <Typography variant="h6">Name: {selectedCustomer.name}</Typography>
              <Typography variant="body1">Email: {selectedCustomer.email}</Typography>
              <Typography variant="body1">Phone: {selectedCustomer.phoneNumber || 'N/A'}</Typography>
              <Typography variant="body1">Delivery Address: {selectedCustomer.deliveryAddress || 'N/A'}</Typography>
              <Typography variant="body1">Payment Address: {selectedCustomer.paymentAddress || 'N/A'}</Typography>
              <Typography variant="h6" sx={{ mt: 2 }}>Funds: £{selectedCustomer.accountFunds?.toFixed(2) || '0.00'}</Typography>
              
              <Typography variant="h6" sx={{ mt: 3 }}>Recent Orders:</Typography>
              {selectedCustomer.recentOrders && selectedCustomer.recentOrders.length > 0 ? (
                <List dense>
                  {selectedCustomer.recentOrders.map(order => (
                    <ListItem key={order.id}>
                      <ListItemText
                        primary={`Order #${order.orderNumber} - ${order.status} (£${order.totalAmount?.toFixed(2) || '0.00'})`}
                        secondary={`Ordered on: ${new Date(order.orderDate).toLocaleDateString()}`}
                      />
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Typography variant="body2">No recent orders.</Typography>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDetailDialog}>Close</Button>
          <Button onClick={() => handleAddFundsClick(selectedCustomer)} color="primary" variant="contained">
            Add Funds
          </Button>
        </DialogActions>
      </Dialog>

      {/* Add Funds Dialog */}
      <Dialog open={openAddFundsDialog} onClose={handleCloseAddFundsDialog} maxWidth="xs" fullWidth>
        <DialogTitle>Add Funds to Customer</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Enter the amount to add to {selectedCustomer?.name || 'this customer'}'s account.
          </DialogContentText>
          <TextField
            autoFocus
            margin="dense"
            label="Amount"
            type="number"
            fullWidth
            variant="outlined"
            value={fundsAmount}
            onChange={(e) => setFundsAmount(e.target.value)}
            inputProps={{ step: "0.01", min: "0.01" }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseAddFundsDialog}>Cancel</Button>
          <Button onClick={handleAddFundsConfirm} color="primary" variant="contained" disabled={!fundsAmount || parseFloat(fundsAmount) <= 0}>
            Add Funds
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteConfirmationOpen}
        onClose={() => setDeleteConfirmationOpen(false)}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
        <DialogTitle id="alert-dialog-title">
          {"Confirm Customer Deletion"}
        </DialogTitle>
        <DialogContent>
          <Typography id="alert-dialog-description">
            Are you sure you want to delete this customer account? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirmationOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error" autoFocus>
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={snackbarOpen} autoHideDuration={6000} onClose={handleCloseSnackbar}>
        <Alert onClose={handleCloseSnackbar} severity={snackbarSeverity} sx={{ width: '100%' }}>
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </Container>
  );
}

export default StaffCustomers; 