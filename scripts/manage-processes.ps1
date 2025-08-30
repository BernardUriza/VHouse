# Script to manage VHouse application processes safely
# This script addresses the issue of multiple processes running simultaneously
# causing file locking errors during development

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("stop", "start", "restart", "status")]
    [string]$Action
)

function Get-VHouseProcesses {
    return Get-Process | Where-Object { 
        $_.ProcessName -eq "VHouse.Web" -or 
        ($_.ProcessName -eq "dotnet" -and $_.MainWindowTitle -like "*VHouse*")
    }
}

function Stop-VHouseProcesses {
    Write-Host "Stopping VHouse processes..." -ForegroundColor Yellow
    
    $processes = Get-VHouseProcesses
    
    if ($processes.Count -eq 0) {
        Write-Host "No VHouse processes found." -ForegroundColor Green
        return $true
    }
    
    foreach ($process in $processes) {
        try {
            Write-Host "Stopping process: $($process.ProcessName) (ID: $($process.Id))" -ForegroundColor Yellow
            $process.Kill()
            $process.WaitForExit(5000) # Wait up to 5 seconds
            Write-Host "Process stopped successfully." -ForegroundColor Green
        }
        catch {
            Write-Host "Failed to stop process $($process.Id): $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
    
    # Wait a moment for file handles to be released
    Start-Sleep -Seconds 2
    return $true
}

function Start-VHouseApplication {
    Write-Host "Starting VHouse application..." -ForegroundColor Yellow
    
    # Ensure we're in the correct directory
    $projectRoot = Split-Path $PSScriptRoot -Parent
    $webProjectPath = Join-Path $projectRoot "VHouse.Web"
    
    if (!(Test-Path $webProjectPath)) {
        Write-Host "VHouse.Web project not found at: $webProjectPath" -ForegroundColor Red
        return $false
    }
    
    try {
        Push-Location $webProjectPath
        
        # Clean build first to avoid file locking issues
        Write-Host "Cleaning solution..." -ForegroundColor Yellow
        dotnet clean --verbosity quiet
        
        # Build the solution
        Write-Host "Building solution..." -ForegroundColor Yellow
        $buildResult = dotnet build --verbosity quiet
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Build failed!" -ForegroundColor Red
            return $false
        }
        
        # Start the application
        Write-Host "Starting application..." -ForegroundColor Yellow
        Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory $webProjectPath
        
        Write-Host "Application started successfully!" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Failed to start application: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    finally {
        Pop-Location
    }
}

function Show-ProcessStatus {
    $processes = Get-VHouseProcesses
    
    if ($processes.Count -eq 0) {
        Write-Host "No VHouse processes are currently running." -ForegroundColor Green
    }
    else {
        Write-Host "Found $($processes.Count) VHouse process(es):" -ForegroundColor Yellow
        foreach ($process in $processes) {
            Write-Host "  - $($process.ProcessName) (ID: $($process.Id))" -ForegroundColor Cyan
        }
    }
}

# Main script logic
switch ($Action) {
    "stop" {
        $success = Stop-VHouseProcesses
        exit $(if ($success) { 0 } else { 1 })
    }
    
    "start" {
        # Stop any existing processes first
        Stop-VHouseProcesses | Out-Null
        $success = Start-VHouseApplication
        exit $(if ($success) { 0 } else { 1 })
    }
    
    "restart" {
        Stop-VHouseProcesses | Out-Null
        $success = Start-VHouseApplication
        exit $(if ($success) { 0 } else { 1 })
    }
    
    "status" {
        Show-ProcessStatus
        exit 0
    }
}