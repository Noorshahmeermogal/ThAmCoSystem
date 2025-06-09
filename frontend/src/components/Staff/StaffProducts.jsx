import { useState, useEffect } from 'react';
import { Typography, Container, Box, CircularProgress, Alert, Paper, List, ListItem, ListItemText, Divider, Button, TextField, Dialog, DialogTitle, DialogContent, DialogActions } from '@mui/material';
import { useAuth } from '../../AuthContext';
import { API_BASE_URL } from '../../config';

function StaffProducts() {
  const { token } = useAuth();
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [restockDialogOpen, setRestockDialogOpen] = useState(false);
  const [restockQuantity, setRestockQuantity] = useState(1);
  const [restockError, setRestockError] = useState(null);

  const fetchProducts = async () => {
    if (!token) {
      setLoading(false);
      setError("Authentication token not found. Please log in as staff.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/products`, {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      if (!response.ok) {
        throw new Error(`Failed to fetch products: ${response.statusText}`);
      }
      const data = await response.json();
      setProducts(data);
    } catch (e) {
      console.error("Error fetching products:", e);
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, [token]);

  const handleRestockClick = (product) => {
    setSelectedProduct(product);
    setRestockQuantity(1);
    setRestockError(null);
    setRestockDialogOpen(true);
  };

  const handleRestockSubmit = async () => {
    if (!selectedProduct || !token) return;

    try {
      const response = await fetch(`${API_BASE_URL}/api/products/${selectedProduct.id}/restock`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({ quantity: restockQuantity }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to restock product');
      }

      const updatedProduct = await response.json();
      setProducts(products.map(p => p.id === updatedProduct.id ? updatedProduct : p));
      setRestockDialogOpen(false);
    } catch (e) {
      console.error("Error restocking product:", e);
      setRestockError(e.message);
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
        Manage Products
      </Typography>
      <Paper elevation={3} sx={{ p: 4 }}>
        {products.length === 0 ? (
          <Typography variant="h6" sx={{ mt: 2 }}>No products found.</Typography>
        ) : (
          <List>
            {products.map((product) => (
              <Box key={product.id}>
                <ListItem alignItems="flex-start">
                  <ListItemText
                    primary={product.name}
                    secondary={
                      <>
                        <Typography component="span" variant="body2" color="text.primary">
                          Price: £{product.price?.toFixed(2) || '0.00'}
                        </Typography>
                        <br />
                        <Typography component="span" variant="body2" color="text.secondary">
                          Category: {product.category}
                        </Typography>
                        <br />
                        <Typography component="span" variant="body2" color="text.secondary">
                          Current Stock: {product.stockQuantity}
                        </Typography>
                        <br />
                        <Typography component="span" variant="body2" color="text.secondary">
                          Last Updated: {new Date(product.lastStockUpdate).toLocaleString()}
                        </Typography>
                      </>
                    }
                  />
                  <Button
                    variant="contained"
                    color="primary"
                    size="small"
                    onClick={() => handleRestockClick(product)}
                  >
                    Restock
                  </Button>
                </ListItem>
                <Divider component="li" />
              </Box>
            ))}
          </List>
        )}
      </Paper>

      {/* Restock Dialog */}
      <Dialog open={restockDialogOpen} onClose={() => setRestockDialogOpen(false)}>
        <DialogTitle>Restock Product</DialogTitle>
        <DialogContent>
          {selectedProduct && (
            <Box sx={{ pt: 2 }}>
              <Typography variant="subtitle1" gutterBottom>
                {selectedProduct.name}
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Current Stock: {selectedProduct.stockQuantity}
              </Typography>
              <TextField
                autoFocus
                margin="dense"
                label="Quantity to Add"
                type="number"
                fullWidth
                value={restockQuantity}
                onChange={(e) => setRestockQuantity(Math.max(1, parseInt(e.target.value) || 1))}
                inputProps={{ min: 1 }}
              />
              {restockError && (
                <Alert severity="error" sx={{ mt: 2 }}>
                  {restockError}
                </Alert>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRestockDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleRestockSubmit} variant="contained" color="primary">
            Restock
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default StaffProducts; 