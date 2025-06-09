import React, { useState } from 'react';
import { TextField, Button, Box, Typography, Alert } from '@mui/material';
import { useAuth } from '../../AuthContext';

const AddFunds = () => {
    const [amount, setAmount] = useState('');
    const [message, setMessage] = useState('');
    const [severity, setSeverity] = useState('info');
    const { user, apiBaseUrl, refreshUserProfile } = useAuth();

    const handleAddFunds = async (event) => {
        event.preventDefault();
        setMessage('');

        if (!user) {
            setMessage('You must be logged in to add funds.');
            setSeverity('error');
            return;
        }

        if (parseFloat(amount) <= 0 || isNaN(parseFloat(amount))) {
            setMessage('Please enter a valid amount greater than 0.');
            setSeverity('error');
            return;
        }

        try {
            const response = await fetch(`${apiBaseUrl}/api/customers/self-add-funds`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${user.token}`,
                },
                body: JSON.stringify({ amount: parseFloat(amount) }),
            });

            const data = await response.json();

            if (response.ok) {
                setMessage('Funds added successfully!');
                setSeverity('success');
                await refreshUserProfile();
                setAmount('');
            } else {
                setMessage(data.message || 'Failed to add funds.');
                setSeverity('error');
            }
        } catch (error) {
            console.error('Error adding funds:', error);
            setMessage('An unexpected error occurred.');
            setSeverity('error');
        }
    };

    return (
        <Box 
            sx={{
                mt: 8,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
            }}
        >
            <Typography component="h1" variant="h5">
                Add Funds to Your Account
            </Typography>
            <Box component="form" onSubmit={handleAddFunds} noValidate sx={{ mt: 1 }}>
                <TextField
                    margin="normal"
                    required
                    fullWidth
                    id="amount"
                    label="Amount"
                    name="amount"
                    autoComplete="amount"
                    autoFocus
                    value={amount}
                    onChange={(e) => setAmount(e.target.value)}
                    type="number"
                    inputProps={{ step: "0.01" }}
                />
                <Button
                    type="submit"
                    fullWidth
                    variant="contained"
                    sx={{ mt: 3, mb: 2 }}
                >
                    Add Funds
                </Button>
                {message && <Alert severity={severity} sx={{ mt: 2 }}>{message}</Alert>}
            </Box>
        </Box>
    );
};

export default AddFunds; 