-- Seed initial data for ThAmCo System
-- Insert sample suppliers
-- Insert sample suppliers
IF NOT EXISTS (SELECT * FROM Suppliers WHERE Name = 'TechSupplier Ltd')
BEGIN
    INSERT INTO Suppliers (Name, ApiEndpoint, ApiKey, IsActive) VALUES
    ('TechSupplier Ltd', 'https://api.techsupplier.com/v1', 'tech_api_key_123', 1),
    ('ElectroWorld', 'https://electroworld.api.com/products', 'electro_key_456', 1),
    ('GadgetSource', 'https://gadgetsource.com/api/v2', 'gadget_api_789', 1);
END

-- Insert sample customers
IF NOT EXISTS (SELECT * FROM Customers WHERE Email = 'john.smith@email.com')
BEGIN
    INSERT INTO Customers (Name, Email, PhoneNumber, DeliveryAddress, PaymentAddress, AccountFunds, IsActive) VALUES
    ('John Smith', 'john.smith@email.com', '+44 7700 900123', '123 Main Street, London, SW1A 1AA', '123 Main Street, London, SW1A 1AA', 500.00, 1),
    ('Sarah Johnson', 'sarah.johnson@email.com', '+44 7700 900456', '456 Oak Avenue, Manchester, M1 1AA', '456 Oak Avenue, Manchester, M1 1AA', 750.00, 1),
    ('Mike Wilson', 'mike.wilson@email.com', '+44 7700 900789', '789 Pine Road, Birmingham, B1 1AA', '789 Pine Road, Birmingham, B1 1AA', 300.00, 1),
    ('Emma Davis', 'emma.davis@email.com', '+44 7700 900321', '321 Elm Street, Leeds, LS1 1AA', '321 Elm Street, Leeds, LS1 1AA', 1000.00, 1);
END

-- Insert sample staff
IF NOT EXISTS (SELECT * FROM Staff WHERE Email = 'admin@thamco.com')
BEGIN
    INSERT INTO Staff (Name, Email, Role, IsActive) VALUES
    ('Admin User', 'admin@thamco.com', 'Administrator', 1),
    ('Staff Member 1', 'staff1@thamco.com', 'Staff', 1),
    ('Staff Member 2', 'staff2@thamco.com', 'Staff', 1);
END

-- Insert sample orders
IF NOT EXISTS (SELECT * FROM Orders WHERE OrderNumber = 'ORD-2024-001')
BEGIN
    INSERT INTO Orders (CustomerId, OrderNumber, TotalAmount, Status, DeliveryAddress, PhoneNumber, OrderDate) VALUES
    (1, 'ORD-2024-001', 120.98, 'Pending', '123 Main Street, London, SW1A 1AA', '+44 7700 900123', GETUTCDATE()),
    (2, 'ORD-2024-002', 65.99, 'Dispatched', '456 Oak Avenue, Manchester, M1 1AA', '+44 7700 900456', DATEADD(day, -2, GETUTCDATE())),
    (3, 'ORD-2024-003', 87.48, 'Pending', '789 Pine Road, Birmingham, B1 1AA', '+44 7700 900789', DATEADD(day, -1, GETUTCDATE()));
END
