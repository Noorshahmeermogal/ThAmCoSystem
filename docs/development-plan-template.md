# ThAmCo System - Development Plan

**Student Name**: [Your Name]  
**Student ID**: [Your Student ID]  
**Module**: CIS3032-N Cloud Computing DevOps  
**Tutor Approval Date**: [To be filled by tutor]  

## 1. Project Overview

### 1.1 System Context
The ThAmCo System is an enterprise e-commerce platform for Three Amigos Corp, enabling product resale from approved suppliers to registered customers through a web application.

### 1.2 Development Scope
This development plan covers approximately 20% of the total system effort, focusing on two primary containers that demonstrate all assessment criteria including distributed architecture, security, resilience, and DevOps practices.

## 2. Container Selection and Justification

### 2.1 Primary Container: Web API Service
**Technology Stack**: ASP.NET Core 8.0, Entity Framework Core, SQL Server  
**Justification**: Core business logic container handling all customer-facing functionality  

**Responsibilities**:
- Product catalog management and search
- Customer registration and authentication (OAuth2)
- Order processing and management
- Staff dispatch operations
- RESTful API endpoints for all operations

**Key Features Implemented**:
- Product browsing with filtering and search
- Secure customer authentication using JWT tokens
- Order placement with business rule validation
- Staff order dispatch management
- Customer profile management
- Account deletion with data anonymization

**Interfaces to Other Containers**:
- Database: Entity Framework Core with connection pooling
- Email Service: SMTP integration for order notifications
- Background Services: Shared database for coordination
- External Suppliers: HTTP clients with resilience patterns

### 2.2 Secondary Container: Background Services
**Technology Stack**: .NET 8.0 Hosted Services, Entity Framework Core  
**Justification**: Demonstrates distributed processing and scheduled operations  

**Responsibilities**:
- Stock level updates every 5 minutes
- Daily product catalog and price updates
- Supplier API integration with resilience
- Automated email notifications

**Key Features Implemented**:
- StockUpdateService: Scheduled every 5 minutes
- ProductCatalogUpdateService: Daily execution at 2 AM
- Circuit breaker patterns for supplier API calls
- Comprehensive error handling and logging

**Interfaces to Other Containers**:
- Database: Direct Entity Framework access
- Supplier APIs: HTTP clients with retry policies
- Email Service: Notification triggers

## 3. System Requirements Addressed

### 3.1 Functional Requirements Coverage
✅ **Public Users**:
- Browse and filter products (ProductsController.GetProducts)
- Search products by name/description (ProductsController.SearchProducts)
- User registration (AuthController.Register)

✅ **Registered Customers**:
- Secure sign-in with OAuth2 (AuthController.Login)
- Profile updates (CustomersController.UpdateProfile)
- View stock status with 5-minute updates (Background service)
- View account funds (CustomersController.GetFunds)
- Place orders with validation (OrdersController.CreateOrder)
- Email notifications (EmailService integration)
- Order history viewing (OrdersController.GetOrderHistory)
- Account deletion requests (CustomersController.DeleteAccount)

✅ **Staff Operations**:
- View pending dispatch orders (OrdersController.GetPendingDispatchOrders)
- Mark orders as dispatched (OrdersController.DispatchOrder)
- View customer details (CustomersController.GetCustomer)
- Delete customer accounts with anonymization (CustomersController.DeleteCustomer)

✅ **Product Management**:
- Multi-supplier product sourcing (ProductSupplier relationships)
- Unique product listings (Product entity design)
- Cheapest price + 10% calculation (ProductCatalogUpdateService)
- Daily catalog updates (Background service scheduling)

### 3.2 Non-Functional Requirements Coverage
✅ **Security**: OAuth2 JWT authentication, role-based authorization  
✅ **Resilience**: Circuit breaker patterns, retry policies, error handling  
✅ **Scalability**: Stateless design, connection pooling, async operations  
✅ **Maintainability**: Clean architecture, dependency injection, logging  

## 4. Architecture and Design Decisions

### 4.1 Authentication Strategy
**Decision**: OAuth2 with JWT tokens (explicitly NOT Microsoft Identity)  
**Rationale**: Industry standard, stateless, supports multiple client types  
**Implementation**: Custom JWT service with role-based claims  

### 4.2 Data Architecture
**Decision**: SQL Server with Entity Framework Core  
**Rationale**: ACID compliance for financial transactions, mature tooling  
**Implementation**: Code-first approach with migrations  

### 4.3 Resilience Patterns
**Decision**: Circuit breaker and retry patterns for external dependencies  
**Rationale**: Ensures system stability when suppliers are unavailable  
**Implementation**: Polly library integration with exponential backoff  

### 4.4 Background Processing
**Decision**: .NET Hosted Services for scheduled tasks  
**Rationale**: Built-in framework support, easy testing and deployment  
**Implementation**: Separate services for stock and catalog updates  

## 5. Development Methodology

### 5.1 Agile Approach
- **Sprint Duration**: 1 week iterations
- **Product Owner**: Module tutor
- **Backlog Management**: GitHub Issues
- **Definition of Done**: Code complete, tested, documented, deployed

### 5.2 Git Workflow
- **Branching Strategy**: GitFlow with feature branches
- **Commit Standards**: Conventional commits with semantic versioning
- **Code Reviews**: Pull request reviews before merging
- **Release Management**: Tagged releases with automated deployment

### 5.3 Testing Strategy
- **Unit Testing**: xUnit with Moq for service layer testing
- **Integration Testing**: WebApplicationFactory for API testing
- **Test Coverage**: Minimum 80% coverage for business logic
- **Test Doubles**: Mocked external dependencies (suppliers, email)

## 6. DevOps Implementation

### 6.1 CI/CD Pipeline (GitHub Actions)
\`\`\`yaml
Trigger: Push to main/develop branches
Steps:
1. Checkout code
2. Setup .NET 8.0
3. Restore dependencies
4. Build solution
5. Run unit tests
6. Run integration tests
7. Security scanning
8. Build Docker images
9. Deploy to staging (develop branch)
10. Deploy to production (main branch)
11. Run smoke tests
\`\`\`

### 6.2 Environment Management
- **Development**: Local development with LocalDB
- **Staging**: Azure App Service with Azure SQL (Basic tier)
- **Production**: Azure App Service with Azure SQL (Standard tier)

### 6.3 Configuration Management
- **Local**: appsettings.Development.json
- **Staging**: Azure App Service configuration
- **Production**: Azure Key Vault for secrets

### 6.4 Monitoring and Logging
- **Application Insights**: Performance and error monitoring
- **Structured Logging**: Serilog with correlation IDs
- **Health Checks**: Built-in ASP.NET Core health checks
- **Alerting**: Azure Monitor alerts for critical issues

## 7. Risk Management

### 7.1 Technical Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| Supplier API failures | High | Circuit breaker patterns, fallback data |
| Database performance | Medium | Connection pooling, query optimization |
| Authentication issues | High | Comprehensive testing, fallback mechanisms |
| Deployment failures | Medium | Blue-green deployment, rollback procedures |

### 7.2 Project Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| Scope creep | Medium | Clear requirements documentation |
| Time constraints | High | Prioritized feature development |
| Technology learning curve | Medium | Proof of concepts, documentation |
| Integration complexity | High | Early integration testing |

## 8. Quality Assurance

### 8.1 Code Quality Standards
- **Coding Standards**: Microsoft C# conventions
- **Code Analysis**: SonarQube integration
- **Documentation**: XML comments for public APIs
- **Performance**: Response time < 200ms for 95% of requests

### 8.2 Testing Standards
- **Unit Test Coverage**: Minimum 80% for business logic
- **Integration Tests**: All API endpoints covered
- **Performance Tests**: Load testing with 100 concurrent users
- **Security Tests**: OWASP dependency scanning

### 8.3 Documentation Standards
- **API Documentation**: OpenAPI/Swagger specifications
- **Architecture Documentation**: C4 model diagrams
- **Deployment Documentation**: Step-by-step guides
- **User Documentation**: API usage examples

## 9. Timeline and Milestones

### Week 1-2: Foundation
- [ ] Project setup and repository creation
- [ ] Database schema design and implementation
- [ ] Basic Web API structure with authentication
- [ ] Initial CI/CD pipeline setup

### Week 3-4: Core Features
- [ ] Product management APIs
- [ ] Customer registration and authentication
- [ ] Order processing functionality
- [ ] Background services implementation

### Week 5-6: Advanced Features
- [ ] Staff management functionality
- [ ] Email notification system
- [ ] Resilience patterns implementation
- [ ] Comprehensive testing suite

### Week 7-8: Deployment and Documentation
- [ ] Azure deployment configuration
- [ ] Performance optimization
- [ ] Documentation completion
- [ ] Final testing and validation

## 10. Success Criteria

### 10.1 Functional Success
- [ ] All specified user stories implemented and tested
- [ ] System handles concurrent users without degradation
- [ ] Background services run reliably on schedule
- [ ] Email notifications sent successfully

### 10.2 Technical Success
- [ ] OAuth2 authentication working correctly
- [ ] Resilience patterns prevent system failures
- [ ] CI/CD pipeline deploys automatically
- [ ] Monitoring and alerting operational

### 10.3 Assessment Success
- [ ] Architecture clearly documented with diagrams
- [ ] Implementation demonstrates distributed design
- [ ] Security and resilience patterns evident
- [ ] DevOps practices fully demonstrated
- [ ] Narrative explains technology decisions

## 11. Tutor Approval

**Tutor Name**: _______________________  
**Approval Date**: ____________________  
**Signature**: ________________________  

**Comments/Modifications Required**:
_________________________________
_________________________________
_________________________________

---

**Student Acknowledgment**:
I understand this development plan and commit to delivering the specified containers with all required functionality, testing, and documentation.

**Student Signature**: ____________________  
**Date**: _______________________________
\`\`\`

Finally, let me create a quick setup script:
