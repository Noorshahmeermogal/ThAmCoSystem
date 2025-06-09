Write-Host "=== ThAmCo Monitoring Setup ===" -ForegroundColor Green
Write-Host "This script sets up comprehensive monitoring for ThAmCo System" -ForegroundColor Yellow

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Configuration
$MonitoringPath = "C:\Monitoring\ThAmCo"
$LogsPath = "C:\Logs\ThAmCo"
$BackupPath = "C:\Backups\ThAmCo"

Write-Host "Setting up monitoring infrastructure..." -ForegroundColor Cyan

# Step 1: Create monitoring directories
Write-Host "Step 1: Creating monitoring directories..." -ForegroundColor Yellow
$directories = @($MonitoringPath, $LogsPath, "$BackupPath\Database", "$BackupPath\Application", "$BackupPath\Logs")
foreach ($dir in $directories) {
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "Created directory: $dir" -ForegroundColor Green
    }
}

# Step 2: Install required PowerShell modules
Write-Host "Step 2: Installing PowerShell modules..." -ForegroundColor Yellow
$modules = @("SqlServer", "ImportExcel", "PSScheduledJob")
foreach ($module in $modules) {
    if (!(Get-Module -ListAvailable -Name $module)) {
        Write-Host "Installing module: $module" -ForegroundColor Cyan
        Install-Module -Name $module -Force -Scope AllUsers
    } else {
        Write-Host "Module already installed: $module" -ForegroundColor Green
    }
}

# Step 3: Create monitoring scripts
Write-Host "Step 3: Creating monitoring scripts..." -ForegroundColor Yellow

# Health Check Script
$healthCheckScript = @'
param(
    [string]$LogPath = "C:\Logs\ThAmCo\health-check.log"
)

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$results = @()

# Check API Health
try {
    $apiResponse = Invoke-RestMethod -Uri "https://localhost:5001/health" -TimeoutSec 30
    $results += "$timestamp - API Health: $apiResponse"
    $apiHealthy = $true
} catch {
    $results += "$timestamp - API Health: FAILED - $($_.Exception.Message)"
    $apiHealthy = $false
}

# Check Database Connection
try {
    $connectionString = "Server=.\SQLEXPRESS;Database=ThAmCoSystem;Trusted_Connection=true;Connection Timeout=30"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT COUNT(*) FROM Products"
    $productCount = $command.ExecuteScalar()
    $connection.Close()
    $results += "$timestamp - Database: HEALTHY - $productCount products"
    $dbHealthy = $true
} catch {
    $results += "$timestamp - Database: FAILED - $($_.Exception.Message)"
    $dbHealthy = $false
}

# Check Windows Services
$services = Get-Service -Name "ThAmCo*" -ErrorAction SilentlyContinue
foreach ($service in $services) {
    if ($service.Status -eq "Running") {
        $results += "$timestamp - Service $($service.Name): RUNNING"
    } else {
        $results += "$timestamp - Service $($service.Name): $($service.Status)"
    }
}

# Check Performance Counters
try {
    $cpuUsage = Get-Counter "\Processor(_Total)\% Processor Time" | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
    $memoryUsage = Get-Counter "\Memory\Available MBytes" | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
    $results += "$timestamp - CPU Usage: $([math]::Round($cpuUsage, 2))%"
    $results += "$timestamp - Available Memory: $([math]::Round($memoryUsage, 2)) MB"
} catch {
    $results += "$timestamp - Performance Counters: FAILED - $($_.Exception.Message)"
}

# Write results to log
$results | Out-File -FilePath $LogPath -Append -Encoding UTF8

# Send alert if critical issues
if (!$apiHealthy -or !$dbHealthy) {
    $alertMessage = "CRITICAL: ThAmCo System Health Check Failed`n" + ($results -join "`n")
    Write-EventLog -LogName Application -Source "ThAmCo.Monitoring" -EventId 1001 -EntryType Error -Message $alertMessage
}

return $results
'@

$healthCheckScript | Out-File -FilePath "$MonitoringPath\health-check.ps1" -Encoding UTF8

# Performance Monitoring Script
$performanceScript = @'
param(
    [string]$LogPath = "C:\Logs\ThAmCo\performance.log",
    [string]$ReportPath = "C:\Logs\ThAmCo\performance-report.xlsx"
)

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$data = @()

# Collect performance data
try {
    # CPU Usage
    $cpuCounter = Get-Counter "\Processor(_Total)\% Processor Time"
    $cpuUsage = $cpuCounter.CounterSamples[0].CookedValue
    
    # Memory Usage
    $memoryCounter = Get-Counter "\Memory\Available MBytes"
    $availableMemory = $memoryCounter.CounterSamples[0].CookedValue
    $totalMemory = (Get-CimInstance Win32_PhysicalMemory | Measure-Object -Property capacity -Sum).sum /1mb
    $memoryUsage = (($totalMemory - $availableMemory) / $totalMemory) * 100
    
    # Disk Usage
    $diskUsage = Get-CimInstance -ClassName Win32_LogicalDisk | Where-Object {$_.DriveType -eq 3} | ForEach-Object {
        [PSCustomObject]@{
            Drive = $_.DeviceID
            Size = [math]::Round($_.Size / 1GB, 2)
            FreeSpace = [math]::Round($_.FreeSpace / 1GB, 2)
            UsedPercent = [math]::Round((($_.Size - $_.FreeSpace) / $_.Size) * 100, 2)
        }
    }
    
    # Network Usage
    $networkCounters = Get-Counter "\Network Interface(*)\Bytes Total/sec" | Where-Object {$_.CounterSamples.InstanceName -notlike "*Loopback*" -and $_.CounterSamples.InstanceName -notlike "*isatap*"}
    $networkUsage = ($networkCounters.CounterSamples | Measure-Object -Property CookedValue -Sum).Sum
    
    # Process Information
    $processes = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | ForEach-Object {
        [PSCustomObject]@{
            ProcessName = $_.ProcessName
            PID = $_.Id
            CPU = $_.CPU
            WorkingSet = [math]::Round($_.WorkingSet / 1MB, 2)
            VirtualMemory = [math]::Round($_.VirtualMemorySize / 1MB, 2)
        }
    }
    
    # Create performance record
    $performanceData = [PSCustomObject]@{
        Timestamp = $timestamp
        CPUUsage = [math]::Round($cpuUsage, 2)
        MemoryUsage = [math]::Round($memoryUsage, 2)
        AvailableMemory = [math]::Round($availableMemory, 2)
        NetworkBytesPerSec = [math]::Round($networkUsage, 2)
        DiskUsage = ($diskUsage | ConvertTo-Json -Compress)
        ProcessInfo = ($processes | ConvertTo-Json -Compress)
    }
    
    # Log to file
    $logEntry = "$timestamp,CPU:$($performanceData.CPUUsage)%,Memory:$($performanceData.MemoryUsage)%,Network:$($performanceData.NetworkBytesPerSec)B/s"
    $logEntry | Out-File -FilePath $LogPath -Append -Encoding UTF8
    
    # Export to Excel if module available
    if (Get-Module -ListAvailable -Name ImportExcel) {
        $performanceData | Export-Excel -Path $ReportPath -WorksheetName "Performance" -Append -AutoSize
    }
    
    # Check thresholds and alert
    if ($performanceData.CPUUsage -gt 80) {
        Write-EventLog -LogName Application -Source "ThAmCo.Monitoring" -EventId 2001 -EntryType Warning -Message "High CPU Usage: $($performanceData.CPUUsage)%"
    }
    
    if ($performanceData.MemoryUsage -gt 85) {
        Write-EventLog -LogName Application -Source "ThAmCo.Monitoring" -EventId 2002 -EntryType Warning -Message "High Memory Usage: $($performanceData.MemoryUsage)%"
    }
    
    return $performanceData
    
} catch {
    $errorMessage = "Performance monitoring failed: $($_.Exception.Message)"
    Write-EventLog -LogName Application -Source "ThAmCo.Monitoring" -EventId 2999 -EntryType Error -Message $errorMessage
    $errorMessage | Out-File -FilePath $LogPath -Append -Encoding UTF8
}
'@

$performanceScript | Out-File -FilePath "$MonitoringPath\performance-monitor.ps1" -Encoding UTF8

# Backup Script
$backupScript = @'
param(
    [string]$BackupPath = "C:\Backups\ThAmCo",
    [int]$RetentionDays = 30
)

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$logFile = "$BackupPath\backup-$timestamp.log"

function Write-Log {
    param([string]$Message)
    $logEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $Message"
    Write-Host $logEntry
    $logEntry | Out-File -FilePath $logFile -Append -Encoding UTF8
}

Write-Log "Starting backup process..."

# Database Backup
try {
    Write-Log "Starting database backup..."
    $dbBackupPath = "$BackupPath\Database\ThAmCo-DB-$timestamp.bak"
    $sqlCommand = @"
BACKUP DATABASE [ThAmCoSystem] 
TO DISK = '$dbBackupPath'
WITH FORMAT, INIT, COMPRESSION,
NAME = 'ThAmCo Database Backup $timestamp'
"@
    
    Invoke-Sqlcmd -ServerInstance ".\SQLEXPRESS" -Query $sqlCommand -QueryTimeout 3600
    Write-Log "Database backup completed: $dbBackupPath"
    
    # Verify backup
    $backupInfo = Invoke-Sqlcmd -ServerInstance ".\SQLEXPRESS" -Query "RESTORE HEADERONLY FROM DISK = '$dbBackupPath'"
    Write-Log "Backup verification: Database Name = $($backupInfo.DatabaseName), Backup Size = $($backupInfo.BackupSize) bytes"
    
} catch {
    Write-Log "ERROR: Database backup failed - $($_.Exception.Message)"
}

# Application Backup
try {
    Write-Log "Starting application backup..."
    $appBackupPath = "$BackupPath\Application\ThAmCo-App-$timestamp.zip"
    $sourcePaths = @(
        "C:\Services\ThAmCo",
        "C:\inetpub\wwwroot\ThAmCo"
    )
    
    foreach ($sourcePath in $sourcePaths) {
        if (Test-Path $sourcePath) {
            Write-Log "Backing up: $sourcePath"
            Compress-Archive -Path "$sourcePath\*" -DestinationPath $appBackupPath -Update
        }
    }
    
    Write-Log "Application backup completed: $appBackupPath"
    
} catch {
    Write-Log "ERROR: Application backup failed - $($_.Exception.Message)"
}

# Logs Backup
try {
    Write-Log "Starting logs backup..."
    $logsBackupPath = "$BackupPath\Logs\ThAmCo-Logs-$timestamp.zip"
    $logsSourcePath = "C:\Logs\ThAmCo"
    
    if (Test-Path $logsSourcePath) {
        Compress-Archive -Path "$logsSourcePath\*" -DestinationPath $logsBackupPath
        Write-Log "Logs backup completed: $logsBackupPath"
    }
    
} catch {
    Write-Log "ERROR: Logs backup failed - $($_.Exception.Message)"
}

# Cleanup old backups
try {
    Write-Log "Cleaning up old backups (older than $RetentionDays days)..."
    $cutoffDate = (Get-Date).AddDays(-$RetentionDays)
    
    $oldBackups = Get-ChildItem -Path $BackupPath -Recurse -File | Where-Object {$_.CreationTime -lt $cutoffDate}
    foreach ($oldBackup in $oldBackups) {
        Remove-Item $oldBackup.FullName -Force
        Write-Log "Deleted old backup: $($oldBackup.Name)"
    }
    
    Write-Log "Cleanup completed. Removed $($oldBackups.Count) old backup files."
    
} catch {
    Write-Log "ERROR: Backup cleanup failed - $($_.Exception.Message)"
}

Write-Log "Backup process completed."
'@

$backupScript | Out-File -FilePath "$MonitoringPath\backup.ps1" -Encoding UTF8

# Step 4: Create Event Log Source
Write-Host "Step 4: Creating Event Log source..." -ForegroundColor Yellow
try {
    New-EventLog -LogName Application -Source "ThAmCo.Monitoring" -ErrorAction SilentlyContinue
    Write-Host "Event Log source created: ThAmCo.Monitoring" -ForegroundColor Green
} catch {
    Write-Host "Event Log source already exists or failed to create" -ForegroundColor Yellow
}

# Step 5: Create Scheduled Tasks
Write-Host "Step 5: Creating scheduled tasks..." -ForegroundColor Yellow

# Health Check Task (every 5 minutes)
$healthCheckAction = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-ExecutionPolicy Bypass -File `"$MonitoringPath\health-check.ps1`""
$healthCheckTrigger = New-ScheduledTaskTrigger -RepetitionInterval (New-TimeSpan -Minutes 5) -RepetitionDuration (New-TimeSpan -Days 365) -At (Get-Date)
$healthCheckSettings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
Register-ScheduledTask -TaskName "ThAmCo-HealthCheck" -Action $healthCheckAction -Trigger $healthCheckTrigger -Settings $healthCheckSettings -User "SYSTEM" -Force

# Performance Monitoring Task (every 15 minutes)
$perfAction = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-ExecutionPolicy Bypass -File `"$MonitoringPath\performance-monitor.ps1`""
$perfTrigger = New-ScheduledTaskTrigger -RepetitionInterval (New-TimeSpan -Minutes 15) -RepetitionDuration (New-TimeSpan -Days 365) -At (Get-Date)
$perfSettings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
Register-ScheduledTask -TaskName "ThAmCo-PerformanceMonitor" -Action $perfAction -Trigger $perfTrigger -Settings $perfSettings -User "SYSTEM" -Force

# Daily Backup Task (2 AM daily)
$backupAction = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-ExecutionPolicy Bypass -File `"$MonitoringPath\backup.ps1`""
$backupTrigger = New-ScheduledTaskTrigger -Daily -At "2:00AM"
$backupSettings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
Register-ScheduledTask -TaskName "ThAmCo-DailyBackup" -Action $backupAction -Trigger $backupTrigger -Settings $backupSettings -User "SYSTEM" -Force

Write-Host "Scheduled tasks created successfully." -ForegroundColor Green

# Step 6: Create monitoring dashboard script
Write-Host "Step 6: Creating monitoring dashboard..." -ForegroundColor Yellow

$dashboardScript = @'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Create main form
$form = New-Object System.Windows.Forms.Form
$form.Text = "ThAmCo System Monitor"
$form.Size = New-Object System.Drawing.Size(800, 600)
$form.StartPosition = "CenterScreen"

# Create status labels
$lblApiStatus = New-Object System.Windows.Forms.Label
$lblApiStatus.Location = New-Object System.Drawing.Point(20, 20)
$lblApiStatus.Size = New-Object System.Drawing.Size(200, 20)
$lblApiStatus.Text = "API Status: Checking..."

$lblDbStatus = New-Object System.Windows.Forms.Label
$lblDbStatus.Location = New-Object System.Drawing.Point(20, 50)
$lblDbStatus.Size = New-Object System.Drawing.Size(200, 20)
$lblDbStatus.Text = "Database Status: Checking..."

$lblCpuUsage = New-Object System.Windows.Forms.Label
$lblCpuUsage.Location = New-Object System.Drawing.Point(20, 80)
$lblCpuUsage.Size = New-Object System.Drawing.Size(200, 20)
$lblCpuUsage.Text = "CPU Usage: Checking..."

$lblMemoryUsage = New-Object System.Windows.Forms.Label
$lblMemoryUsage.Location = New-Object System.Drawing.Point(20, 110)
$lblMemoryUsage.Size = New-Object System.Drawing.Size(200, 20)
$lblMemoryUsage.Text = "Memory Usage: Checking..."

# Create log display
$txtLogs = New-Object System.Windows.Forms.TextBox
$txtLogs.Location = New-Object System.Drawing.Point(20, 150)
$txtLogs.Size = New-Object System.Drawing.Size(740, 300)
$txtLogs.Multiline = $true
$txtLogs.ScrollBars = "Vertical"
$txtLogs.ReadOnly = $true

# Create refresh button
$btnRefresh = New-Object System.Windows.Forms.Button
$btnRefresh.Location = New-Object System.Drawing.Point(20, 470)
$btnRefresh.Size = New-Object System.Drawing.Size(100, 30)
$btnRefresh.Text = "Refresh"

# Create timer for auto-refresh
$timer = New-Object System.Windows.Forms.Timer
$timer.Interval = 30000  # 30 seconds

# Refresh function
$refreshData = {
    try {
        # Check API
        $apiResponse = Invoke-RestMethod -Uri "https://localhost:5001/health" -TimeoutSec 10
        $lblApiStatus.Text = "API Status: $apiResponse"
        $lblApiStatus.ForeColor = [System.Drawing.Color]::Green
    } catch {
        $lblApiStatus.Text = "API Status: OFFLINE"
        $lblApiStatus.ForeColor = [System.Drawing.Color]::Red
    }
    
    try {
        # Check Database
        $connectionString = "Server=.\SQLEXPRESS;Database=ThAmCoSystem;Trusted_Connection=true;Connection Timeout=10"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        $connection.Close()
        $lblDbStatus.Text = "Database Status: ONLINE"
        $lblDbStatus.ForeColor = [System.Drawing.Color]::Green
    } catch {
        $lblDbStatus.Text = "Database Status: OFFLINE"
        $lblDbStatus.ForeColor = [System.Drawing.Color]::Red
    }
    
    try {
        # Get CPU Usage
        $cpuUsage = Get-Counter "\Processor(_Total)\% Processor Time" | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
        $lblCpuUsage.Text = "CPU Usage: $([math]::Round($cpuUsage, 1))%"
        if ($cpuUsage -gt 80) {
            $lblCpuUsage.ForeColor = [System.Drawing.Color]::Red
        } elseif ($cpuUsage -gt 60) {
            $lblCpuUsage.ForeColor = [System.Drawing.Color]::Orange
        } else {
            $lblCpuUsage.ForeColor = [System.Drawing.Color]::Green
        }
        
        # Get Memory Usage
        $memoryCounter = Get-Counter "\Memory\Available MBytes" | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
        $totalMemory = (Get-CimInstance Win32_PhysicalMemory | Measure-Object -Property capacity -Sum).sum /1mb
        $memoryUsage = (($totalMemory - $memoryCounter) / $totalMemory) * 100
        $lblMemoryUsage.Text = "Memory Usage: $([math]::Round($memoryUsage, 1))%"
        if ($memoryUsage -gt 85) {
            $lblMemoryUsage.ForeColor = [System.Drawing.Color]::Red
        } elseif ($memoryUsage -gt 70) {
            $lblMemoryUsage.ForeColor = [System.Drawing.Color]::Orange
        } else {
            $lblMemoryUsage.ForeColor = [System.Drawing.Color]::Green
        }
    } catch {
        $lblCpuUsage.Text = "CPU Usage: ERROR"
        $lblMemoryUsage.Text = "Memory Usage: ERROR"
    }
    
    # Load recent logs
    $logFile = "C:\Logs\ThAmCo\health-check.log"
    if (Test-Path $logFile) {
        $recentLogs = Get-Content $logFile -Tail 20 | Out-String
        $txtLogs.Text = $recentLogs
        $txtLogs.SelectionStart = $txtLogs.Text.Length
        $txtLogs.ScrollToCaret()
    }
}

# Event handlers
$btnRefresh.Add_Click($refreshData)
$timer.Add_Tick($refreshData)

# Add controls to form
$form.Controls.Add($lblApiStatus)
$form.Controls.Add($lblDbStatus)
$form.Controls.Add($lblCpuUsage)
$form.Controls.Add($lblMemoryUsage)
$form.Controls.Add($txtLogs)
$form.Controls.Add($btnRefresh)

# Initial refresh
& $refreshData

# Start timer
$timer.Start()

# Show form
$form.Add_FormClosed({$timer.Stop()})
$form.ShowDialog()
'@

$dashboardScript | Out-File -FilePath "$MonitoringPath\dashboard.ps1" -Encoding UTF8

Write-Host "`n=== Monitoring Setup Complete ===" -ForegroundColor Green
Write-Host "Monitoring Path: $MonitoringPath" -ForegroundColor White
Write-Host "Logs Path: $LogsPath" -ForegroundColor White
Write-Host "Backup Path: $BackupPath" -ForegroundColor White

Write-Host "`n=== Scheduled Tasks Created ===" -ForegroundColor Cyan
Write-Host "ThAmCo-HealthCheck: Every 5 minutes" -ForegroundColor White
Write-Host "ThAmCo-PerformanceMonitor: Every 15 minutes" -ForegroundColor White
Write-Host "ThAmCo-DailyBackup: Daily at 2:00 AM" -ForegroundColor White

Write-Host "`n=== Monitoring Commands ===" -ForegroundColor Cyan
Write-Host "View Health Check: Get-Content C:\Logs\ThAmCo\health-check.log -Tail 10" -ForegroundColor White
Write-Host "View Performance: Get-Content C:\Logs\ThAmCo\performance.log -Tail 10" -ForegroundColor White
Write-Host "Run Dashboard: PowerShell -File $MonitoringPath\dashboard.ps1" -ForegroundColor White
Write-Host "Manual Backup: PowerShell -File $MonitoringPath\backup.ps1" -ForegroundColor White

Write-Host "`n=== Event Log Monitoring ===" -ForegroundColor Cyan
Write-Host "View Events: Get-EventLog -LogName Application -Source 'ThAmCo.Monitoring' -Newest 10" -ForegroundColor White

Write-Host "`nMonitoring setup completed successfully!" -ForegroundColor Green
Read-Host "Press Enter to exit"
