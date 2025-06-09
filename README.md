# ThAmCo System

A comprehensive e-commerce system built with ASP.NET Core 8, implementing a distributed microservices architecture with OAuth2 authentication, automated testing, and CI/CD deployment.

## 🏗️ Architecture Overview

The ThAmCo System is designed as a distributed application with the following key components:

### Core Services
- **Web API**: Main application service handling products, orders, and customer management
- **Authentication Service**: OAuth2-based authentication and authorization
- **Email Service**: Automated email notifications for orders
- **Background Services**: Automated stock updates and catalog management

### Infrastructure
- **Database**: SQL Server with Entity Framework Core
- **Containerization**: Docker and Docker Compose
- **Cloud Deployment**: Azure App Service with Azure SQL Database
- **CI/CD**: GitHub Actions pipeline

## 🚀 Features

### Public Features
- ✅ Browse and filter products
- ✅ Search products by name and description
- ✅ User registration
- ✅ View product stock status (updated every 5 minutes)

### Customer Features
- ✅ Secure OAuth2 authentication
- ✅ Profile management (name, delivery address, phone number)
- ✅ View account funds
- ✅ Place orders with validation
- ✅ Email notifications for orders
- ✅ Order history and status tracking
- ✅ Account deletion requests

### Staff Features
- ✅ View pending dispatch orders
- ✅ Mark orders as dispatched
- ✅ Customer management and profile viewing
- ✅ Customer account deletion with data anonymization

### System Features
- ✅ Automated stock updates every 5 minutes
- ✅ Daily product catalog and price updates
- ✅ Supplier integration with resilience
- ✅ Comprehensive audit logging
- ✅ Error handling and logging middleware

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT with OAuth2
- **Containerization**:
