# ThAmCo System - Complete Implementation Guide

## Prerequisites

### Development Environment Setup
\`\`\`bash
# Required Software
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Docker Desktop
- SQL Server Management Studio (optional)
- Git
- Azure CLI (for deployment)
\`\`\`

### Azure Account Setup
\`\`\`bash
# Create Azure account (free tier available)
# Install Azure CLI
az login
az account set --subscription "your-subscription-id"
\`\`\`

## Step 1: Project Structure Setup

### 1.1 Create Directory Structure
\`\`\`bash
mkdir ThAmCoSystem
cd ThAmCoSystem

# Create main directories
mkdir src
mkdir src/ThAmCo.WebApi
mkdir src/ThAmCo.WebApi.Tests
mkdir scripts
mkdir docs
mkdir .github
mkdir .github/workflows

# Create deliverable directories
mkdir Media
mkdir Media/screenshots
mkdir Media/videos
mkdir Source
mkdir Documentation
\`\`\`

### 1.2 Initialize Git Repository
\`\`\`bash
git init
git remote add origin https://github.com/yourusername/thamco-system.git

# Create .gitignore
echo "bin/
obj/
packages/
.vs/
*.user
*.suo
appsettings.Development.json
.env" > .gitignore
\`\`\`

## Step 2: Database Implementation

### 2.1 Create Database Scripts
\`\`\`bash
# Copy the SQL scripts from the code project
# scripts/01-create-database.sql
# scripts/02-seed-data.sql
\`\`\`

### 2.2 Set Up Local Database
\`\`\`bash
# Option 1: Local SQL Server
sqlcmd -S (localdb)\mssqllocaldb -i scripts/01-create-database.sql
sqlcmd -S (localdb)\mssqllocaldb -i scripts/02-seed-data.sql

# Option 2: Docker SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
\`\`\`

## Step 3: Web API Implementation

### 3.1 Create ASP.NET Core Project
\`\`\`bash
cd src/ThAmCo.WebApi
dotnet new webapi -n ThAmCo.WebApi
cd ThAmCo.WebApi

# Add required packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
\`\`\`

### 3.2 Implement Core Components
Copy all the source code files from the code project:
- Models/ (Product, Customer, Order, etc.)
- Data/ThAmCoContext.cs
- Services/ (ProductService, OrderService, etc.)
- Controllers/ (ProductsController, OrdersController, etc.)
- DTOs/ (ProductDto, OrderDto, etc.)
- Middleware/ (ErrorHandling, RequestLogging)

### 3.3 Configure Application
\`\`\`bash
# Update appsettings.json with your connection strings
# Update Program.cs with service registrations
\`\`\`

## Step 4: Testing Implementation

### 4.1 Create Test Project
\`\`\`bash
cd ../..
mkdir src/ThAmCo.WebApi.Tests
cd src/ThAmCo.WebApi.Tests

dotnet new xunit -n ThAmCo.WebApi.Tests
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Moq

# Add project reference
dotnet add reference ../ThAmCo.WebApi/ThAmCo.WebApi.csproj
\`\`\`

### 4.2 Implement Tests
Copy test files from the code project:
- Controllers/ProductsControllerTests.cs
- Services/ProductServiceTests.cs

## Step 5: Containerization

### 5.1 Create Dockerfile
\`\`\`dockerfile
# Copy Dockerfile from code project to root directory
\`\`\`

### 5.2 Create Docker Compose
\`\`\`yaml
# Copy docker-compose.yml from code project
\`\`\`

### 5.3 Test Containerization
\`\`\`bash
# Build and run containers
docker-compose up --build

# Test API endpoints
curl http://localhost:8080/api/products
curl http://localhost:8080/health
\`\`\`

## Step 6: CI/CD Pipeline Setup

### 6.1 Create GitHub Actions Workflow
\`\`\`yaml
# Copy .github/workflows/ci-cd.yml from code project
\`\`\`

### 6.2 Configure GitHub Secrets
\`\`\`bash
# In GitHub repository settings, add secrets:
AZURE_WEBAPP_PUBLISH_PROFILE_STAGING
AZURE_WEBAPP_PUBLISH_PROFILE
\`\`\`

### 6.3 Test Pipeline
\`\`\`bash
git add .
git commit -m "Initial implementation"
git push origin main

# Check GitHub Actions tab for pipeline execution
\`\`\`

## Step 7: Azure Deployment

### 7.1 Create Azure Resources
\`\`\`bash
# Create resource group
az group create --name thamco-prod-rg --location "UK South"

# Create App Service Plan
az appservice plan create --name thamco-plan --resource-group thamco-prod-rg --sku S1

# Create Web App
az webapp create --name thamco-system --resource-group thamco-prod-rg --plan thamco-plan

# Create SQL Server and Database
az sql server create --name thamco-sql-server --resource-group thamco-prod-rg \
  --location "UK South" --admin-user thamco-admin --admin-password "YourStrong@Password123"

az sql db create --name ThAmCoSystem --server thamco-sql-server --resource-group thamco-prod-rg \
  --service-objective S2
\`\`\`

### 7.2 Configure Connection Strings
\`\`\`bash
# Set connection string in Azure Web App
az webapp config connection-string set --name thamco-system --resource-group thamco-prod-rg \
  --connection-string-type SQLServer \
  --settings DefaultConnection="Server=tcp:thamco-sql-server.database.windows.net,1433;Initial Catalog=ThAmCoSystem;Persist Security Info=False;User ID=thamco-admin;Password=YourStrong@Password123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
\`\`\`

### 7.3 Deploy Database Schema
\`\`\`bash
# Run database scripts against Azure SQL
sqlcmd -S thamco-sql-server.database.windows.net -U thamco-admin -P "YourStrong@Password123" \
  -d ThAmCoSystem -i scripts/01-create-database.sql
\`\`\`

## Step 8: Testing and Validation

### 8.1 Run Local Tests
\`\`\`bash
cd src/ThAmCo.WebApi.Tests
dotnet test --verbosity normal
\`\`\`

### 8.2 Integration Testing
\`\`\`bash
# Test deployed application
curl https://thamco-system.azurewebsites.net/api/products
curl https://thamco-system.azurewebsites.net/health
\`\`\`

### 8.3 Load Testing (Optional)
\`\`\`bash
# Use tools like Apache Bench or Azure Load Testing
ab -n 1000 -c 10 https://thamco-system.azurewebsites.net/api/products
\`\`\`

## Step 9: Documentation Creation

### 9.1 Architecture Diagrams
Create using draw.io or similar:
- C4 Context Diagram
- C4 Container Diagram
- Deployment Diagram
- Save as PNG/PDF in Documentation/

### 9.2 Development Plan
\`\`\`markdown
# Development Plan Template

## Project Overview
- System: ThAmCo E-commerce Platform
- Containers: Web API, Database, Background Services
- Effort: ~20% of total system (2 containers minimum)

## Container Selection
1. **Web API Container** (Primary)
   - Technology: ASP.NET Core 8
   - Responsibilities: Products, Orders, Customers, Authentication
   - Interfaces: REST API endpoints
   - Security: OAuth2 JWT authentication

2. **Background Services Container** (Secondary)
   - Technology: .NET Hosted Services
   - Responsibilities: Stock updates, Catalog updates
   - Interfaces: Database connections, Supplier APIs
   - Scheduling: 5-minute stock updates, daily catalog updates

## Key Features Implemented
- Product browsing and search
- Customer registration and authentication
- Order management with supplier integration
- Staff dispatch management
- Automated background processes

## Interfaces to Other Containers
- Database: Entity Framework Core with SQL Server
- Email Service: SMTP integration for notifications
- Supplier APIs: HTTP clients with resilience patterns

## Testing Strategy
- Unit tests with mocking (Moq framework)
- Integration tests with in-memory database
- API testing with WebApplicationFactory
- Weekly integration testing in deployed environment

## DevOps Practices
- Git workflow with feature branches
- GitHub Actions CI/CD pipeline
- Automated testing and deployment
- Multi-environment configuration
\`\`\`

### 9.3 Narrative Report
\`\`\`markdown
# Technology Decisions Narrative

## Architecture Decisions

### Microservices vs Monolithic
**Decision**: Started with modular monolith, designed for microservices evolution
**Rationale**: Easier initial development and deployment while maintaining separation of concerns
**Consequences**: Faster time to market, easier debugging, but potential scaling limitations

### Database Technology
**Decision**: SQL Server with Entity Framework Core
**Rationale**: ACID compliance for financial transactions, mature tooling, Azure integration
**Consequences**: Strong consistency but potential performance bottlenecks at scale

### Authentication Strategy
**Decision**: OAuth2 with JWT tokens (not Microsoft Identity)
**Rationale**: Industry standard, stateless, supports multiple clients
**Consequences**: Secure and scalable but requires careful token management

### Cloud Platform
**Decision**: Microsoft Azure
**Rationale**: Excellent .NET integration, comprehensive PaaS offerings
**Consequences**: Vendor lock-in but reduced operational overhead

## Implementation Challenges

### Supplier Integration Resilience
**Challenge**: External supplier APIs may be unreliable
**Solution**: Circuit breaker pattern with retry policies
**Outcome**: System remains functional even with supplier failures

### Data Consistency
**Challenge**: Order processing requires multiple database updates
**Solution**: Database transactions with proper error handling
**Outcome**: Ensures data integrity but may impact performance

### Background Service Reliability
**Challenge**: Stock updates must run reliably every 5 minutes
**Solution**: Hosted services with proper error handling and logging
**Outcome**: Reliable updates but requires monitoring

## Lessons Learned

### Testing Strategy
**Learning**: Integration tests caught issues unit tests missed
**Action**: Increased integration test coverage
**Result**: Higher confidence in deployments

### Configuration Management
**Learning**: Environment-specific configuration is crucial
**Action**: Implemented proper configuration patterns
**Result**: Smooth multi-environment deployments

### Monitoring and Observability
**Learning**: Production issues are hard to debug without proper logging
**Action**: Implemented comprehensive logging and health checks
**Result**: Faster issue resolution

## Future Improvements

1. **Microservices Migration**: Split into separate services for better scalability
2. **Event-Driven Architecture**: Implement message queues for better decoupling
3. **Caching Strategy**: Add Redis for improved performance
4. **Advanced Monitoring**: Implement distributed tracing
5. **Security Enhancements**: Add rate limiting and advanced threat protection

## Accountability Statement

I take full responsibility for all technology decisions made in this project. The choices were made based on the requirements, constraints, and my understanding of best practices. While some decisions may have trade-offs, they were made with careful consideration of the project goals and timeline.

Any issues or limitations in the implementation are my responsibility, and I am committed to learning from this experience to make better decisions in future projects.
\`\`\`

## Step 10: Media Creation

### 10.1 Screenshots Required
1. **Application Running**
   - Homepage with product listings
   - Product search functionality
   - Customer registration/login
   - Order placement process
   - Staff dispatch interface

2. **DevOps Tools**
   - GitHub Actions pipeline running
   - Azure portal showing deployed resources
   - Docker containers running
   - Test results in IDE
   - Database with sample data

3. **Architecture Diagrams**
   - C4 diagrams displayed
   - Deployment architecture
   - API documentation (Swagger)

### 10.2 Video Captures (20-30 seconds each)
1. **End-to-End User Journey**
   - Customer browsing and ordering products
   - Staff processing dispatch
   - Email notifications being sent

2. **DevOps Demonstration**
   - Git commit triggering CI/CD pipeline
   - Automated tests running
   - Deployment to Azure
   - Health checks passing

3. **System Resilience**
   - Handling supplier API failures
   - Database connection recovery
   - Error handling in action

## Step 11: Final Submission Preparation

### 11.1 Clean Source Code
\`\`\`bash
# Remove build artifacts
find . -name "bin" -type d -exec rm -rf {} +
find . -name "obj" -type d -exec rm -rf {} +
find . -name "packages" -type d -exec rm -rf {} +
find . -name ".vs" -type d -exec rm -rf {} +
\`\`\`

### 11.2 Create Submission ZIP
\`\`\`bash
# Create final directory structure
mkdir ThAmCoSystem_Submission
cp -r Media/ ThAmCoSystem_Submission/
cp -r Source/ ThAmCoSystem_Submission/
cp -r Documentation/ ThAmCoSystem_Submission/
echo "Student Name: [Your Name]
Student ID: [Your ID]
Module: CIS3032-N Cloud Computing DevOps
Assignment: ThAmCo System" > ThAmCoSystem_Submission/README.md

# Create ZIP file
zip -r ThAmCoSystem_Submission.zip ThAmCoSystem_Submission/
\`\`\`

### 11.3 Final Checklist
- [ ] All source code included and cleaned
- [ ] Screenshots in PNG/JPG format (full resolution)
- [ ] Videos in MP4 H.264 format (20-30 seconds)
- [ ] Architecture diagrams completed
- [ ] Development plan approved by tutor
- [ ] Narrative report completed
- [ ] Marking proforma filled out
- [ ] README with name and student ID
- [ ] ZIP file under size limit

## Troubleshooting Common Issues

### Database Connection Issues
\`\`\`bash
# Check connection string format
# Ensure SQL Server is running
# Verify firewall settings for Azure SQL
\`\`\`

### Docker Build Failures
\`\`\`bash
# Check Dockerfile syntax
# Ensure all files are copied correctly
# Verify base image availability
\`\`\`

### CI/CD Pipeline Failures
\`\`\`bash
# Check GitHub Actions logs
# Verify secrets are configured
# Ensure Azure credentials are valid
\`\`\`

### Azure Deployment Issues
\`\`\`bash
# Check App Service logs
# Verify connection strings
# Ensure database schema is deployed
\`\`\`

This implementation guide provides everything needed to successfully complete the ThAmCo System assessment with all requirements met.
\`\`\`

Now let me create the development plan template:
