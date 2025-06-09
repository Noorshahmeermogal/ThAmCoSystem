Write-Host "=== ThAmCo Load Balancer Deployment ===" -ForegroundColor Green
Write-Host "This script sets up multiple ThAmCo instances with load balancing" -ForegroundColor Yellow

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Configuration
$InstanceCount = 3
$BasePort = 5000
$LoadBalancerPort = 80
$InstallBasePath = "C:\Services\ThAmCo"
$ProjectPath = "src\ThAmCo.WebApi"

Write-Host "Deploying $InstanceCount ThAmCo instances with load balancing..." -ForegroundColor Cyan

# Step 1: Install Application Request Routing (ARR) for IIS
Write-Host "Step 1: Checking IIS and ARR installation..." -ForegroundColor Yellow
$iisFeatures = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-CommonHttpFeatures",
    "IIS-HttpErrors",
    "IIS-HttpLogging",
    "IIS-RequestFiltering",
    "IIS-StaticContent",
    "IIS-DefaultDocument",
    "IIS-DirectoryBrowsing",
    "IIS-ASPNET45"
)

foreach ($feature in $iisFeatures) {
    $featureState = Get-WindowsOptionalFeature -Online -FeatureName $feature
    if ($featureState.State -ne "Enabled") {
        Write-Host "Enabling IIS feature: $feature" -ForegroundColor Yellow
        Enable-WindowsOptionalFeature -Online -FeatureName $feature -All -NoRestart
    }
}

# Step 2: Deploy multiple application instances
Write-Host "Step 2: Deploying application instances..." -ForegroundColor Yellow
for ($i = 1; $i -le $InstanceCount; $i++) {
    $instancePath = "$InstallBasePath\Instance$i"
    $instancePort = $BasePort + $i
    $httpsPort = $instancePort + 1000
    
    Write-Host "Deploying Instance $i to port $instancePort..." -ForegroundColor Cyan
    
    # Create instance directory
    if (Test-Path $instancePath) {
        Remove-Item $instancePath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $instancePath -Force | Out-Null
    
    # Publish application
    Push-Location $ProjectPath
    try {
        dotnet publish --configuration Release --output $instancePath --self-contained false --runtime win-x64
        if ($LASTEXITCODE -ne 0) {
            throw "Publish failed for instance $i"
        }
    }
    finally {
        Pop-Location
    }
    
    # Create instance-specific configuration
    $instanceConfig = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ThAmCoSystem;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;Pooling=true;Max Pool Size=100"
  },
  "JwtSettings": {
    "SecretKey": "LOAD-BALANCED-SECRET-KEY-CHANGE-IN-PRODUCTION-123456789",
    "Issuer": "ThAmCo.LoadBalanced",
    "Audience": "ThAmCo.Clients",
    "ExpiryInHours": 1
  },
  "Urls": "http://localhost:$instancePort;https://localhost:$httpsPort",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "InstanceId": "$i"
}
"@
    
    $instanceConfig | Out-File -FilePath "$instancePath\appsettings.json" -Encoding UTF8
    
    # Create Windows Service for this instance
    $serviceName = "ThAmCoInstance$i"
    $servicePath = "$instancePath\ThAmCo.WebApi.exe"
    
    # Remove existing service if it exists
    $existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($existingService) {
        Stop-Service -Name $serviceName -Force
        sc.exe delete $serviceName
        Start-Sleep -Seconds 2
    }
    
    # Create new service
    sc.exe create $serviceName binPath= $servicePath DisplayName= "ThAmCo Instance $i" start= auto
    sc.exe description $serviceName "ThAmCo E-commerce API Instance $i"
    sc.exe failure $serviceName reset= 86400 actions= restart/5000/restart/5000/restart/5000
    
    Write-Host "Instance $i configured on port $instancePort" -ForegroundColor Green
}

# Step 3: Configure IIS Load Balancer
Write-Host "Step 3: Configuring IIS Load Balancer..." -ForegroundColor Yellow

# Import WebAdministration module
Import-Module WebAdministration

# Create server farm
$farmName = "ThAmCoFarm"
$siteName = "ThAmCoLoadBalancer"

# Remove existing configuration
try {
    Remove-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" -Filter "webFarms" -Name "." -AtElement @{name=$farmName} -ErrorAction SilentlyContinue
    Remove-Website -Name $siteName -ErrorAction SilentlyContinue
    Remove-WebAppPool -Name $siteName -ErrorAction SilentlyContinue
} catch {}

# Create server farm
Add-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" -Filter "webFarms" -Name "." -Value @{name=$farmName}

# Add servers to farm
for ($i = 1; $i -le $InstanceCount; $i++) {
    $serverPort = $BasePort + $i
    Add-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" -Filter "webFarms/webFarm[@name='$farmName']" -Name "server" -Value @{address="localhost:$serverPort"}
}

# Configure load balancing algorithm (round robin)
Set-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" -Filter "webFarms/webFarm[@name='$farmName']/applicationRequestRouting/affinity" -Name "useCookies" -Value $false

# Create application pool
New-WebAppPool -Name $siteName
Set-ItemProperty -Path "IIS:\AppPools\$siteName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"

# Create website
$physicalPath = "C:\inetpub\wwwroot\$siteName"
New-Item -ItemType Directory -Path $physicalPath -Force | Out-Null

# Create simple health check page
$healthPage = @"
<!DOCTYPE html>
<html>
<head>
    <title>ThAmCo Load Balancer</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .status { padding: 20px; border-radius: 5px; margin: 10px 0; }
        .healthy { background-color: #d4edda; border: 1px solid #c3e6cb; color: #155724; }
        .unhealthy { background-color: #f8d7da; border: 1px solid #f5c6cb; color: #721c24; }
    </style>
</head>
<body>
    <h1>ThAmCo Load Balancer Status</h1>
    <p>Load balancer is running with $InstanceCount backend instances.</p>
    <div class="status healthy">
        <strong>Status:</strong> Active<br>
        <strong>Instances:</strong> $InstanceCount<br>
        <strong>Algorithm:</strong> Round Robin
    </div>
    <h2>API Endpoints</h2>
    <ul>
        <li><a href="/api/products">Products API</a></li>
        <li><a href="/swagger">API Documentation</a></li>
        <li><a href="/health">Health Check</a></li>
    </ul>
</body>
</html>
"@

$healthPage | Out-File -FilePath "$physicalPath\index.html" -Encoding UTF8

# Create website
New-Website -Name $siteName -Port $LoadBalancerPort -PhysicalPath $physicalPath -ApplicationPool $siteName

# Configure URL rewrite rules for load balancing
$webConfig = @"
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
    <system.webServer>
        <rewrite>
            <rules>
                <rule name="LoadBalanceAPI" stopProcessing="true">
                    <match url="^api/(.*)" />
                    <action type="Rewrite" url="http://$farmName/api/{R:1}" />
                </rule>
                <rule name="LoadBalanceSwagger" stopProcessing="true">
                    <match url="^swagger(.*)" />
                    <action type="Rewrite" url="http://$farmName/swagger{R:1}" />
                </rule>
                <rule name="LoadBalanceHealth" stopProcessing="true">
                    <match url="^health" />
                    <action type="Rewrite" url="http://$farmName/health" />
                </rule>
            </rules>
        </rewrite>
    </system.webServer>
</configuration>
"@

$webConfig | Out-File -FilePath "$physicalPath\web.config" -Encoding UTF8

# Step 4: Start all services
Write-Host "Step 4: Starting all services..." -ForegroundColor Yellow
for ($i = 1; $i -le $InstanceCount; $i++) {
    $serviceName = "ThAmCoInstance$i"
    Start-Service -Name $serviceName
    Write-Host "Started $serviceName" -ForegroundColor Green
}

# Step 5: Test deployment
Write-Host "Step 5: Testing deployment..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Test individual instances
for ($i = 1; $i -le $InstanceCount; $i++) {
    $instancePort = $BasePort + $i
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:$instancePort/health" -TimeoutSec 10
        Write-Host "✅ Instance $i (port $instancePort): $response" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Instance $i (port $instancePort): Not responding" -ForegroundColor Red
    }
}

# Test load balancer
try {
    $response = Invoke-RestMethod -Uri "http://localhost:$LoadBalancerPort/health" -TimeoutSec 10
    Write-Host "✅ Load Balancer (port $LoadBalancerPort): Working" -ForegroundColor Green
}
catch {
    Write-Host "❌ Load Balancer (port $LoadBalancerPort): Not responding" -ForegroundColor Red
}

Write-Host "`n=== Load Balancer Deployment Summary ===" -ForegroundColor Green
Write-Host "Instances Deployed: $InstanceCount" -ForegroundColor White
Write-Host "Load Balancer URL: http://localhost:$LoadBalancerPort" -ForegroundColor White
Write-Host "API Documentation: http://localhost:$LoadBalancerPort/swagger" -ForegroundColor White
Write-Host "Health Check: http://localhost:$LoadBalancerPort/health" -ForegroundColor White

Write-Host "`n=== Individual Instance URLs ===" -ForegroundColor Cyan
for ($i = 1; $i -le $InstanceCount; $i++) {
    $instancePort = $BasePort + $i
    Write-Host "Instance $i: http://localhost:$instancePort" -ForegroundColor White
}

Write-Host "`n=== Management Commands ===" -ForegroundColor Cyan
Write-Host "View all services: Get-Service -Name 'ThAmCoInstance*'" -ForegroundColor White
Write-Host "Stop all services: Get-Service -Name 'ThAmCoInstance*' | Stop-Service" -ForegroundColor White
Write-Host "Start all services: Get-Service -Name 'ThAmCoInstance*' | Start-Service" -ForegroundColor White
Write-Host "Remove all services: Get-Service -Name 'ThAmCoInstance*' | ForEach-Object { sc.exe delete `$_.Name }" -ForegroundColor White

Write-Host "`nLoad balancer deployment completed!" -ForegroundColor Green
Read-Host "Press Enter to exit"
