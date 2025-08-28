param(
  [string]$Solution = "VHouse.sln",
  [string]$Project = "VHouse/VHouse.csproj"
)

$root = (Get-Location)
$logs = Join-Path $root "_logs"
New-Item -ItemType Directory -Force -Path $logs | Out-Null
$stamp = (Get-Date -Format "yyyyMMdd-HHmmss")
$log = Join-Path $logs "diagnose-$stamp.log"

function Write-Log($msg) {
  $msg | Tee-Object -FilePath $log -Append
}

Write-Log "===== DIAG START $(Get-Date) ====="
Write-Log "dotnet --info"
dotnet --info | Tee-Object -FilePath $log -Append

Write-Log "`n== restore =="
dotnet restore $Solution 2>&1 | Tee-Object -FilePath $log -Append

Write-Log "`n== build (Release) =="
dotnet build $Solution -c Release -warnaserror 2>&1 | Tee-Object -FilePath $log -Append

Write-Log "`n== vulnerable packages (transitive) =="
dotnet list $Project package --vulnerable --include-transitive 2>&1 | Tee-Object -FilePath $log -Append

Write-Log "`n== tests =="
$tests = Get-ChildItem -Recurse -Filter "*Tests.csproj" | Select-Object -ExpandProperty FullName
if ($tests) {
  foreach ($t in $tests) {
    Write-Log "running tests: $t"
    dotnet test $t -c Release --collect:"XPlat Code Coverage" --no-build 2>&1 | Tee-Object -FilePath $log -Append
  }
} else {
  Write-Log "no test projects found."
}

Write-Log "===== DIAG END $(Get-Date) ====="
Write-Host "Log written to $log"