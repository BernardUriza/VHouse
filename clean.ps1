# VHouse Project Cleanup Script
Write-Host "🧹 Cleaning VHouse project..." -ForegroundColor Green

# Remove build artifacts
Write-Host "Removing build artifacts (bin/obj)..."
Get-ChildItem -Path . -Recurse -Directory -Name "bin" | Remove-Item -Recurse -Force
Get-ChildItem -Path . -Recurse -Directory -Name "obj" | Remove-Item -Recurse -Force

# Remove temporary files
Write-Host "Removing temporary files..."
Get-ChildItem -Path . -Recurse -Include "*.tmp", "*.temp", "*.log", "*.cache" | Remove-Item -Force

# Remove IDE files
Write-Host "Removing IDE temporary files..."
Remove-Item -Path ".vs" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "*.user" -Force -ErrorAction SilentlyContinue

# Remove node_modules if exists (shouldn't in a .NET project)
if (Test-Path "node_modules") {
    Write-Host "Removing node_modules..."
    Remove-Item -Path "node_modules" -Recurse -Force
}

Write-Host "✅ Project cleaned successfully!" -ForegroundColor Green
Write-Host "Use 'dotnet clean' for additional .NET-specific cleanup." -ForegroundColor Yellow