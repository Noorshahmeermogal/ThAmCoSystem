-- Create ThAmCo Database and Tables
USE ThAmCoSystem;
GO

-- Suppliers table
CREATE TABLE Suppliers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    ApiEndpoint NVARCHAR(500) NOT NULL,
    ApiKey NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Products table
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Category NVARCHAR(100),
    BasePrice DECIMAL(10,2) NOT NULL,
    CurrentPrice DECIMAL(10,2) NOT NULL,
    StockQuantity INT DEFAULT 0,
    LastStockUpdate DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Product Suppliers mapping
CREATE TABLE ProductSuppliers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    SupplierId INT NOT NULL,
    SupplierProductId NVARCHAR(100),
    SupplierPrice DECIMAL(10,2) NOT NULL,
    StockQuantity INT DEFAULT 0,
    LastUpdated DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(Id),
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
);

-- Customers table
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20),
    DeliveryAddress NVARCHAR(MAX),
    PaymentAddress NVARCHAR(MAX),
    AccountFunds DECIMAL(10,2) DEFAULT 0.00,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Orders table
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Pending',
    DeliveryAddress NVARCHAR(MAX) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    OrderDate DATETIME2 DEFAULT GETUTCDATE(),
    DispatchedDate DATETIME2 NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

-- Order Items table
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Staff table
CREATE TABLE Staff (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Role NVARCHAR(100) DEFAULT 'Staff',
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Audit log for customer data changes
CREATE TABLE CustomerAuditLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT,
    Action NVARCHAR(100) NOT NULL,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    ChangedBy NVARCHAR(255),
    ChangedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Create indexes for performance
CREATE INDEX IX_Products_Name ON Products(Name);
CREATE INDEX IX_Products_Category ON Products(Category);
CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_ProductSuppliers_ProductId ON ProductSuppliers(ProductId);
