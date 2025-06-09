import { useState, useEffect } from 'react';
import { TextField, Button, Typography, Container, Box, Alert, CircularProgress, Paper, Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions } from '@mui/material';
import { useAuth } from '../../AuthContext';
import { useNavigate } from 'react-router-dom';
import { API_BASE_URL } from '../../config';

function Profile() {
  const { user, token, logout, updateUser } = useAuth();
  const navigate = useNavigate();
  const [profile, setProfile] = useState(null);
  const [funds, setFunds] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    phoneNumber: '',
    deliveryAddress: '',
  });
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);

  useEffect(() => {
    const fetchProfileAndFunds = async () => {
      if (!token) {
        setLoading(false);
        setError("Authentication token not found.");
        return;
      }

      try {
        // Fetch profile
        const profileResponse = await fetch(`${API_BASE_URL}/api/customers/profile`, {
          headers: {
            'Authorization': `Bearer ${token}`,
          },
        });
        if (!profileResponse.ok) {
          if (profileResponse.status === 404) {
            throw new Error("Profile not found. It might have been deleted.");
          }
          throw new Error(`Failed to fetch profile: ${profileResponse.statusText}`);
        }
        const profileData = await profileResponse.json();
        setProfile(profileData);
        setFormData({
          name: profileData.name || '',
          phoneNumber: profileData.phoneNumber || '',
          deliveryAddress: profileData.deliveryAddress || '',
        });

        // Fetch funds
        const fundsResponse = await fetch(`${API_BASE_URL}/api/customers/funds`, {
          headers: {
            'Authorization': `Bearer ${token}`,
          },
        });
        if (!fundsResponse.ok) {
          throw new Error(`Failed to fetch funds: ${fundsResponse.statusText}`);
        }
        const fundsData = await fundsResponse.json();
        setFunds(fundsData);

      } catch (e) {
        console.error("Error fetching data:", e);
        setError(e.message);
        setProfile(null); // Clear profile if fetching fails
        setFunds(null); // Clear funds if fetching fails
      } finally {
        setLoading(false);
      }
    };

    fetchProfileAndFunds();
  }, [token, logout]); // Added logout to dependency array for clarity

  const handleFormChange = (event) => {
    const { name, value } = event.target;
    setFormData(prevData => ({ ...prevData, [name]: value }));
  };

  const handleUpdate = async (event) => {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    setLoading(true);

    try {
      const response = await fetch(`${API_BASE_URL}/api/customers/profile`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(formData),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || 'Failed to update profile');
      }

      setProfile(data);
      updateUser(data); // Update user in AuthContext
      setSuccess('Profile updated successfully!');
      setEditMode(false);
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteAccount = async () => {
    setOpenDeleteDialog(false);
    setLoading(true);
    setError(null);
    setSuccess(null);

    try {
      const response = await fetch(`${API_BASE_URL}/api/customers/account`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.message || 'Failed to request account deletion');
      }

      setSuccess('Account deletion requested successfully!');
      logout();
      navigate('/login');

    } catch (e) {
      console.error("Error requesting account deletion:", e);
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  const handleClickOpenDeleteDialog = () => {
    setOpenDeleteDialog(true);
  };

  const handleCloseDeleteDialog = () => {
    setOpenDeleteDialog(false);
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error && !profile) {
    return (
      <Container component={Paper} elevation={3} sx={{ p: 4, mt: 8, textAlign: 'center' }}>
        <Typography variant="h6" color="error">Error: {error}</Typography>
        <Button variant="contained" sx={{ mt: 2 }} onClick={() => navigate('/login')}>Login</Button>
      </Container>
    );
  }

  if (!profile) {
    return (
      <Container component={Paper} elevation={3} sx={{ p: 4, mt: 8, textAlign: 'center' }}>
        <Typography variant="h6">No profile data available. Please login.</Typography>
        <Button variant="contained" sx={{ mt: 2 }} onClick={() => navigate('/login')}>Login</Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Customer Profile
      </Typography>
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}
      
      <Box component={Paper} elevation={3} sx={{ p: 4 }}>
        {!editMode ? (
          <>
            <Typography variant="h6" gutterBottom>Name: {profile.name}</Typography>
            <Typography variant="body1" gutterBottom>Email: {profile.email}</Typography>
            <Typography variant="body1" gutterBottom>Phone Number: {profile.phoneNumber || 'N/A'}</Typography>
            <Typography variant="body1" gutterBottom>Delivery Address: {profile.deliveryAddress || 'N/A'}</Typography>
            <Typography variant="body1" gutterBottom>Payment Address: {profile.paymentAddress || 'N/A'}</Typography>
            <Typography variant="h6" sx={{ mt: 2 }}>Account Funds: £{funds?.accountFunds?.toFixed(2) || '0.00'}</Typography>
            <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
              <Button variant="contained" onClick={() => setEditMode(true)}>
                Edit Profile
              </Button>
              <Button variant="outlined" color="error" onClick={handleClickOpenDeleteDialog}>
                Request Account Deletion
              </Button>
            </Box>
          </>
        ) : (
          <Box component="form" onSubmit={handleUpdate} noValidate>
            <TextField
              margin="normal"
              required
              fullWidth
              id="name"
              label="Name"
              name="name"
              value={formData.name}
              onChange={handleFormChange}
              sx={{ mb: 2 }}
            />
            <TextField
              margin="normal"
              fullWidth
              id="phoneNumber"
              label="Phone Number"
              name="phoneNumber"
              value={formData.phoneNumber}
              onChange={handleFormChange}
              sx={{ mb: 2 }}
            />
            <TextField
              margin="normal"
              fullWidth
              id="deliveryAddress"
              label="Delivery Address"
              name="deliveryAddress"
              value={formData.deliveryAddress}
              onChange={handleFormChange}
              sx={{ mb: 2 }}
            />
            <Box sx={{ display: 'flex', gap: 2, mt: 3 }}>
              <Button type="submit" variant="contained" disabled={loading}>
                {loading ? 'Updating...' : 'Save Changes'}
              </Button>
              <Button variant="outlined" onClick={() => setEditMode(false)}>
                Cancel
              </Button>
            </Box>
          </Box>
        )}
      </Box>

      {/* Delete Account Confirmation Dialog */}
      <Dialog
        open={openDeleteDialog}
        onClose={handleCloseDeleteDialog}
        aria-labelledby="delete-dialog-title"
        aria-describedby="delete-dialog-description"
      >
        <DialogTitle id="delete-dialog-title">{"Request Account Deletion"}</DialogTitle>
        <DialogContent>
          <DialogContentText id="delete-dialog-description">
            Are you sure you want to request your account deletion? This action cannot be undone.
            All your personal data will be erased/anonymised, but other related data will be retained.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDeleteDialog}>Cancel</Button>
          <Button onClick={handleDeleteAccount} color="error" autoFocus>
            Confirm Deletion
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

export default Profile; 