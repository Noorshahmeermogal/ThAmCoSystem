using namespace System.ServiceProcess

Write-Host "=== ThAmCo Windows Service Deployment ===" -ForegroundColor Green
Write-Host "This script will deploy ThAmCo as a Windows Service" -ForegroundColor Yellow

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Configuration
$ServiceName = "ThAmCoWebApi"
$ServiceDisplayName = "ThAmCo Web API Service"
$ServiceDescription = "ThAmCo E-commerce Web API Service"
$InstallPath = "C:\Services\ThAmCo"

# Detect project structure
$CurrentDir = Get-Location
Write-Host "Current directory: $CurrentDir" -ForegroundColor Cyan

# Look for the project file in common locations
$ProjectPaths = @(
    "src\ThAmCo.WebApi",
    ".\src\ThAmCo.WebApi",
    "..\src\ThAmCo.WebApi",
    "ThAmCo.WebApi",
    ".\ThAmCo.WebApi",
    ".\src\ThAmCo.WebApi\ThAmCo.WebApi.csproj"
)

$ProjectPath = $null
$ProjectFile = $null

foreach ($path in $ProjectPaths) {
    $fullPath = Join-Path $CurrentDir $path
    $csprojFile = Join-Path $fullPath "ThAmCo.WebApi.csproj"
    
    Write-Host "Checking: $fullPath" -ForegroundColor Yellow
    
    if (Test-Path $csprojFile) {
        $ProjectPath = $fullPath
        $ProjectFile = $csprojFile
        Write-Host "✅ Found project at: $ProjectPath" -ForegroundColor Green
        break
    }
}

if (-not $ProjectPath) {
    Write-Host "❌ ERROR: Could not find ThAmCo.WebApi.csproj file!" -ForegroundColor Red
    Write-Host "Please ensure you're running this script from the project root directory." -ForegroundColor Yellow
    Write-Host "Expected structure:" -ForegroundColor Yellow
    Write-Host "  📁 thamco-system/" -ForegroundColor White
    Write-Host "    📁 src/" -ForegroundColor White
    Write-Host "      📁 ThAmCo.WebApi/" -ForegroundColor White
    Write-Host "        📄 ThAmCo.WebApi.csproj" -ForegroundColor White
    Write-Host "    📄 deploy-windows-service.ps1" -ForegroundColor White
    
    Write-Host "`nCurrent directory contents:" -ForegroundColor Yellow
    Get-ChildItem -Name | ForEach-Object { Write-Host "  📄 $_" -ForegroundColor White }
    
    Read-Host "Press Enter to exit"
    exit 1
}

# Look for SQL scripts
$SqlScriptPaths = @(
    "scripts",
    ".\scripts",
    "..\scripts",
    "sql",
    ".\sql",
    "database",
    ".\database"
)

$SqlScriptsPath = $null
$CreateDbScript = $null
$SeedDataScript = $null

foreach ($path in $SqlScriptPaths) {
    $fullPath = Join-Path $CurrentDir $path
    $createScript = Join-Path $fullPath "01-create-database.sql"
    $seedScript = Join-Path $fullPath "02-seed-data.sql"
    
    Write-Host "Checking SQL scripts in: $fullPath" -ForegroundColor Yellow
    
    if ((Test-Path $createScript) -and (Test-Path $seedScript)) {
        $SqlScriptsPath = $fullPath
        $CreateDbScript = $createScript
        $SeedDataScript = $seedScript
        Write-Host "✅ Found SQL scripts at: $SqlScriptsPath" -ForegroundColor Green
        break
    }
}

if (-not $SqlScriptsPath) {
    Write-Host "⚠️  Warning: Could not find SQL scripts!" -ForegroundColor Yellow
    Write-Host "Looking for:" -ForegroundColor Yellow
    Write-Host "  📄 01-create-database.sql" -ForegroundColor White
    Write-Host "  📄 02-seed-data.sql" -ForegroundColor White
    Write-Host "Database setup will be skipped." -ForegroundColor Yellow
}

Write-Host "Starting Windows Service deployment..." -ForegroundColor Cyan

# Step 1: Setup Database
if ($SqlScriptsPath) {
    Write-Host "Step 1: Setting up database..." -ForegroundColor Yellow
    
    # Start LocalDB
    Write-Host "Starting LocalDB..." -ForegroundColor Cyan
    try {
        sqllocaldb start MSSQLLocalDB 2>$null
        Write-Host "✅ LocalDB started successfully." -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️  Warning: Could not start LocalDB: $_" -ForegroundColor Yellow
    }
    
    # Wait for LocalDB to be ready
    Start-Sleep -Seconds 3
    
    # Test LocalDB connection
    Write-Host "Testing LocalDB connection..." -ForegroundColor Cyan
    try {
        $connectionTest = sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT 1 AS Test" -h -1 2>$null
        if ($LASTEXITCODE -eq 0 -and $connectionTest -match "1") {
            Write-Host "✅ LocalDB connection successful." -ForegroundColor Green
        } else {
            throw "Connection test failed"
        }
    }
    catch {
        Write-Host "❌ ERROR: Cannot connect to LocalDB!" -ForegroundColor Red
        Write-Host "Please ensure SQL Server LocalDB is installed." -ForegroundColor Yellow
        Write-Host "Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads" -ForegroundColor Yellow
        Read-Host "Press Enter to continue without database setup"
        $SqlScriptsPath = $null
    }
    
    if ($SqlScriptsPath) {
        # Check if database already exists
        Write-Host "Checking if database exists..." -ForegroundColor Cyan
        try {
            $dbExists = sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases WHERE name = 'ThAmCoSystem'" -h -1 2>$null
            if ($dbExists -match "ThAmCoSystem") {
                Write-Host "Database 'ThAmCoSystem' already exists." -ForegroundColor Yellow
                $response = Read-Host "Do you want to recreate the database? (y/N)"
                if ($response -eq "y" -or $response -eq "Y") {
                    Write-Host "Dropping existing database..." -ForegroundColor Yellow
                    sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "DROP DATABASE IF EXISTS ThAmCoSystem" 2>$null
                    Start-Sleep -Seconds 2
                } else {
                    Write-Host "Keeping existing database." -ForegroundColor Green
                    $CreateDbScript = $null  # Skip database creation
                }
            }
        }
        catch {
            Write-Host "Could not check database existence. Proceeding with creation..." -ForegroundColor Yellow
        }
        
        # Run database creation script
        if ($CreateDbScript) {
            Write-Host "Creating database schema..." -ForegroundColor Cyan
            Write-Host "Running: $CreateDbScript" -ForegroundColor Gray
            try {
                sqlcmd -S "(localdb)\MSSQLLocalDB" -i "$CreateDbScript" -o "database-creation.log"
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ Database schema created successfully." -ForegroundColor Green
                } else {
                    throw "Database creation failed. Check database-creation.log for details."
                }
            }
            catch {
                Write-Host "❌ ERROR: Database creation failed: $_" -ForegroundColor Red
                Write-Host "Check database-creation.log for details." -ForegroundColor Yellow
                Read-Host "Press Enter to continue"
            }
        }
        
        # Run data seeding script
        Write-Host "Seeding database with sample data..." -ForegroundColor Cyan
        Write-Host "Running: $SeedDataScript" -ForegroundColor Gray
        try {
            sqlcmd -S "(localdb)\MSSQLLocalDB" -i "$SeedDataScript" -o "database-seeding.log"
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Database seeded successfully." -ForegroundColor Green
            } else {
                throw "Database seeding failed. Check database-seeding.log for details."
            }
        }
        catch {
            Write-Host "❌ ERROR: Database seeding failed: $_" -ForegroundColor Red
            Write-Host "Check database-seeding.log for details." -ForegroundColor Yellow
            Read-Host "Press Enter to continue"
        }
        
        # Verify database setup
        Write-Host "Verifying database setup..." -ForegroundColor Cyan
        try {
            $tableCount = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "ThAmCoSystem" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" -h -1 2>$null
            $productCount = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "ThAmCoSystem" -Q "SELECT COUNT(*) FROM Products" -h -1 2>$null
            $customerCount = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "ThAmCoSystem" -Q "SELECT COUNT(*) FROM Customers" -h -1 2>$null
            
            Write-Host "Database verification:" -ForegroundColor Green
            Write-Host "  Tables created: $($tableCount.Trim())" -ForegroundColor White
            Write-Host "  Sample products: $($productCount.Trim())" -ForegroundColor White
            Write-Host "  Sample customers: $($customerCount.Trim())" -ForegroundColor White
        }
        catch {
            Write-Host "⚠️  Could not verify database setup." -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "Step 1: Skipping database setup (SQL scripts not found)..." -ForegroundColor Yellow
}

# Step 2: Stop existing service if running
Write-Host "Step 2: Checking for existing service..." -ForegroundColor Yellow
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Found existing service. Stopping..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force
    Write-Host "Service stopped." -ForegroundColor Green
}

# Step 3: Create installation directory
Write-Host "Step 3: Creating installation directory..." -ForegroundColor Yellow
if (Test-Path $InstallPath) {
    Remove-Item $InstallPath -Recurse -Force
}
New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
Write-Host "Directory created: $InstallPath" -ForegroundColor Green

# Step 4: Publish application
Write-Host "Step 4: Publishing application..." -ForegroundColor Yellow
Write-Host "Project path: $ProjectPath" -ForegroundColor Cyan
Write-Host "Project file: $ProjectFile" -ForegroundColor Cyan

try {
    # Change to project directory
    Push-Location $ProjectPath
    
    # Restore packages first
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "Package restore failed"
    }
    
    # Build the project
    Write-Host "Building project..." -ForegroundColor Yellow
    dotnet build --configuration Release
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    
    # Publish the application
    Write-Host "Publishing application..." -ForegroundColor Yellow
    dotnet publish --configuration Release --output $InstallPath --self-contained true --runtime win-x64
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed"
    }
    
    Write-Host "✅ Application published successfully." -ForegroundColor Green
}
catch {
    Write-Host "❌ ERROR: Failed to publish application: $_" -ForegroundColor Red
    Pop-Location
    Read-Host "Press Enter to exit"
    exit 1
}
finally {
    Pop-Location
}

# Step 5: Create service configuration
Write-Host "Step 5: Creating service configuration..." -ForegroundColor Yellow

# Create the JSON configuration as a here-string
$serviceConfig = @'
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ThAmCoSystem;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "WINDOWS-SERVICE-SECRET-KEY-CHANGE-IN-PRODUCTION-123456789",
    "Issuer": "ThAmCo.WindowsService",
    "Audience": "ThAmCo.Clients",
    "ExpiryInHours": 1
  },
  "Urls": "http://localhost:5000;https://localhost:5001",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "EventLog": {
      "LogLevel": {
        "Default": "Information"
      },
      "SourceName": "ThAmCo.WebApi"
    }
  },
  "AllowedHosts": "*",
  "SupplierApiSettings": {
    "BaseUrl": "https://api.supplier.com",
    "ApiKey": "demo-key",
    "TimeoutSeconds": 30
  },
  "EmailSettings": {
    "SmtpServer": "localhost",
    "SmtpPort": 587,
    "FromEmail": "noreply@thamco.com",
    "FromName": "ThAmCo System"
  }
}
'@

$serviceConfig | Out-File -FilePath "$InstallPath\appsettings.json" -Encoding UTF8
Write-Host "✅ Service configuration created." -ForegroundColor Green

# Step 6: Install/Update Windows Service
Write-Host "Step 6: Installing Windows Service..." -ForegroundColor Yellow
$servicePath = "$InstallPath\ThAmCo.WebApi.exe"

if (-not (Test-Path $servicePath)) {
    Write-Host "❌ ERROR: Service executable not found at: $servicePath" -ForegroundColor Red
    Write-Host "Published files:" -ForegroundColor Yellow
    Get-ChildItem $InstallPath -Name | ForEach-Object { Write-Host "  📄 $_" -ForegroundColor White }
    Read-Host "Press Enter to exit"
    exit 1
}

if ($existingService) {
    # Update existing service
    Write-Host "Updating existing service..." -ForegroundColor Yellow
    sc.exe config $ServiceName binPath= "`"$servicePath`""
} else {
    # Create new service
    Write-Host "Creating new service..." -ForegroundColor Yellow
    sc.exe create $ServiceName binPath= "`"$servicePath`"" DisplayName= "`"$ServiceDisplayName`"" start= auto
    sc.exe description $ServiceName "`"$ServiceDescription`""
}

# Step 7: Configure service recovery
Write-Host "Step 7: Configuring service recovery..." -ForegroundColor Yellow
sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/5000/restart/5000

# Step 8: Set service permissions
Write-Host "Step 8: Setting service permissions..." -ForegroundColor Yellow
try {
    $acl = Get-Acl $InstallPath
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("NT AUTHORITY\LOCAL SERVICE", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule)
    Set-Acl $InstallPath $acl
    Write-Host "✅ Permissions set successfully." -ForegroundColor Green
}
catch {
    Write-Host "⚠️  Warning: Could not set service permissions: $_" -ForegroundColor Yellow
}

# Step 9: Start service
Write-Host "Step 9: Starting service..." -ForegroundColor Yellow
try {
    Start-Service -Name $ServiceName
    Start-Sleep -Seconds 10
    Write-Host "✅ Service start command executed." -ForegroundColor Green
}
catch {
    Write-Host "⚠️  Warning: Service start may have failed: $_" -ForegroundColor Yellow
}

# Step 10: Verify service status
Write-Host "Step 10: Verifying service status..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
$service = Get-Service -Name $ServiceName
Write-Host "Service Status: $($service.Status)" -ForegroundColor Cyan

if ($service.Status -eq "Running") {
    Write-Host "✅ Service is running successfully!" -ForegroundColor Green
    
    # Test API endpoint
    Write-Host "Testing API endpoint..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
    
    $testUrls = @("http://localhost:5000/health", "https://localhost:5001/health")
    $apiWorking = $false
    
    foreach ($url in $testUrls) {
        try {
            Write-Host "Testing: $url" -ForegroundColor Cyan
            $response = Invoke-RestMethod -Uri $url -TimeoutSec 30
            Write-Host "✅ API Health Check ($url): $response" -ForegroundColor Green
            $apiWorking = $true
            break
        }
        catch {
            Write-Host "❌ Failed to connect to $url" -ForegroundColor Red
        }
    }
    
    if ($apiWorking) {
        # Test database connectivity through API
        Write-Host "Testing database connectivity..." -ForegroundColor Yellow
        try {
            $productsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/products" -TimeoutSec 30
            $productCount = $productsResponse.Count
            Write-Host "✅ Database connectivity verified. Found $productCount products." -ForegroundColor Green
        }
        catch {
            Write-Host "⚠️  Could not verify database connectivity through API." -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️  API not responding. Check Windows Event Log for details." -ForegroundColor Yellow
        Write-Host "   This is normal for first startup. The service may need more time." -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ Service failed to start. Status: $($service.Status)" -ForegroundColor Red
    Write-Host "Check Windows Event Log for error details:" -ForegroundColor Yellow
    Write-Host "  Get-EventLog -LogName Application -Source 'ThAmCo.WebApi' -Newest 5" -ForegroundColor White
}

Write-Host "`n=== Deployment Summary ===" -ForegroundColor Green
Write-Host "Service Name: $ServiceName" -ForegroundColor White
Write-Host "Display Name: $ServiceDisplayName" -ForegroundColor White
Write-Host "Install Path: $InstallPath" -ForegroundColor White
Write-Host "Project Path: $ProjectPath" -ForegroundColor White
if ($SqlScriptsPath) {
    Write-Host "SQL Scripts: $SqlScriptsPath" -ForegroundColor White
}
Write-Host "Service Status: $($service.Status)" -ForegroundColor White
Write-Host "API URL: http://localhost:5000" -ForegroundColor White
Write-Host "Swagger UI: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "Health Check: http://localhost:5000/health" -ForegroundColor White

Write-Host "`n=== Test Data Available ===" -ForegroundColor Cyan
if ($SqlScriptsPath) {
    Write-Host "Customer Accounts (for testing):" -ForegroundColor Yellow
    Write-Host "  📧 john.smith@email.com (£250.00 funds)" -ForegroundColor White
    Write-Host "  📧 sarah.johnson@email.com (£180.50 funds)" -ForegroundColor White
    Write-Host "  📧 mike.wilson@email.com (£320.75 funds)" -ForegroundColor White
    Write-Host "Staff Accounts:" -ForegroundColor Yellow
    Write-Host "  👤 admin@thamco.com (Administrator)" -ForegroundColor White
    Write-Host "  👤 staff1@thamco.com (Staff)" -ForegroundColor White
    Write-Host "Sample Products: 10 products with stock available" -ForegroundColor Yellow
}

Write-Host "`n=== Service Management Commands ===" -ForegroundColor Cyan
Write-Host "Start Service:   Start-Service -Name $ServiceName" -ForegroundColor White
Write-Host "Stop Service:    Stop-Service -Name $ServiceName" -ForegroundColor White
Write-Host "Restart Service: Restart-Service -Name $ServiceName" -ForegroundColor White
Write-Host "Remove Service:  sc.exe delete $ServiceName" -ForegroundColor White
Write-Host "View Logs:       Get-EventLog -LogName Application -Source 'ThAmCo.WebApi' -Newest 10" -ForegroundColor White
Write-Host "Service Status:  Get-Service -Name $ServiceName" -ForegroundColor White

Write-Host "`n=== Database Management ===" -ForegroundColor Cyan
Write-Host "Connect to DB:   sqlcmd -S `"(localdb)\MSSQLLocalDB`" -d `"ThAmCoSystem`"" -ForegroundColor White
Write-Host "Check Tables:    sqlcmd -S `"(localdb)\MSSQLLocalDB`" -d `"ThAmCoSystem`" -Q `"SELECT name FROM sys.tables`"" -ForegroundColor White
Write-Host "View Products:   sqlcmd -S `"(localdb)\MSSQLLocalDB`" -d `"ThAmCoSystem`" -Q `"SELECT TOP 5 * FROM Products`"" -ForegroundColor White
Write-Host "View Customers:  sqlcmd -S `"(localdb)\MSSQLLocalDB`" -d `"ThAmCoSystem`" -Q `"SELECT TOP 5 * FROM Customers`"" -ForegroundColor White

Write-Host "`n=== Troubleshooting ===" -ForegroundColor Cyan
Write-Host "If the service fails to start:" -ForegroundColor Yellow
Write-Host "1. Check Event Log: Get-EventLog -LogName Application -Source 'ThAmCo.WebApi' -Newest 5" -ForegroundColor White
Write-Host "2. Verify LocalDB: sqllocaldb info" -ForegroundColor White
Write-Host "3. Test manually: cd '$InstallPath' && .\ThAmCo.WebApi.exe" -ForegroundColor White
Write-Host "4. Check ports: netstat -an | findstr ':5000'" -ForegroundColor White
Write-Host "5. Check database: sqlcmd -S `"(localdb)\MSSQLLocalDB`" -Q `"SELECT 1`"" -ForegroundColor White

if (Test-Path "database-creation.log") {
    Write-Host "`nDatabase creation log available: database-creation.log" -ForegroundColor Yellow
}
if (Test-Path "database-seeding.log") {
    Write-Host "Database seeding log available: database-seeding.log" -ForegroundColor Yellow
}

Write-Host "`nWindows Service deployment completed!" -ForegroundColor Green
Write-Host "Your ThAmCo System is now running as a Windows Service with a complete database!" -ForegroundColor Green
Read-Host "Press Enter to exit"
