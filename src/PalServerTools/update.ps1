param(
    [switch]$Restart
)

$Owner = "winter2048"
$RepoName = "PalServerTools"
$folderPath = Get-Location

function GetToolsEnv {
    param (
        $Process
    )
    $commandLine = $Process.CommandLine
    $pattern = "-Env (\w+)"
    $match = [regex]::Match($commandLine, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    if ($match.Success) {
        $param = $match.Groups[1].Value
        return $param
    }
    else {
        return ""
    }
}

if ($Restart) {
    $palServerToolsPath = "$folderPath\PalServerTools.exe"
    $processList = Get-WmiObject -Query "select * from Win32_Process where ExecutablePath='$($palServerToolsPath.Replace("\","\\"))'"
    $runArgument = $processList | ForEach-Object { GetToolsEnv -Process $_ }

    Start-Sleep -Seconds 2
    $processList | ForEach-Object { Stop-Process -Id $_.ProcessId }
    Start-Sleep -Seconds 2

    $runArgument | ForEach-Object { Start-Process -FilePath  ".\PalServerTools.exe" -ArgumentList "-env $_" }
    return $null
}


if (-not (Test-Path -Path (Join-Path -Path $folderPath -ChildPath "PalServerTools.exe"))) {
    Write-Host "未安装PalServerTools，请在PalServerTools.exe根目录下执行。"
    return $null
}

$versionInfo = Get-ItemProperty -Path "$folderPath/PalServerTools.exe" | Select-Object -ExpandProperty VersionInfo
$fileVersion = "v" + $versionInfo.FileVersion.Substring(0, $versionInfo.FileVersion.LastIndexOf('.'))
Write-Host "当前版本号: $fileVersion"
$apiUrl = "https://api.github.com/repos/$Owner/$RepoName/releases/latest"
$response = Invoke-WebRequest -Uri $apiUrl -UseBasicParsing | ConvertFrom-Json
$latestFileVersion = $response.tag_name
Write-Output "最新版本号: $latestFileVersion"
if ($fileVersion -eq $latestFileVersion) {
    Write-Host "PalServerTools 已是最新版本"
    return $null
}

# 查询正在运行的PalServerTools.exe进程
$palServerToolsPath = "$folderPath\PalServerTools.exe"
$processList = Get-WmiObject -Query "select * from Win32_Process where ExecutablePath='$($palServerToolsPath.Replace("\","\\"))'"
$runArgument = $processList | ForEach-Object { GetToolsEnv -Process $_ }
# 结束PalServerTools.exe进程
if ($null -ne $runArgument) {
    Write-Host "关闭 PalServerTools" -ForegroundColor Yellow
    Start-Sleep -Seconds 2
    $processList | ForEach-Object { Stop-Process -Id $_.ProcessId }
}

Write-Host "PalServerTools 开始更新" -ForegroundColor Green
$downloadUrl = $response.assets[0].browser_download_url
$fileName = [System.IO.Path]::GetFileName($downloadUrl)
Invoke-WebRequest -Uri $downloadUrl -OutFile $fileName

Write-Host "备份配置 appsetting *"
$appsettings = Get-ChildItem -Path $folderPath -Filter 'appsetting*.json'
$appsettings | ForEach-Object { Rename-Item -Path $_.FullName -NewName "$($_.FullName).bak" }

$appsettingsBak = Get-ChildItem -Path $folderPath -Filter 'appsetting*.json.bak'
$excludeFile = @("$fileName")
$appsettingsBak | ForEach-Object { $excludeFile += $_.Name } 
Remove-Item ".\*" -Recurse -Force -Exclude $excludeFile
Expand-Archive -LiteralPath $fileName -DestinationPath "PalServerTools"
Move-Item -Path ".\PalServerTools\*" -Destination ".\"

Write-Host "还原配置 appsetting *"
Get-ChildItem -Path $folderPath -Filter 'appsetting*.json' | Remove-Item -Force
$appsettingsBak | ForEach-Object { Rename-Item -Path $_.FullName -NewName $_.FullName.TrimEnd(".bak") } 
Remove-Item -Path $fileName
Remove-Item -Path "PalServerTools"
Write-Host "PalServerTools 更新成功" -ForegroundColor Green

Write-Host "运行 PalServerTools"
Start-Sleep -Seconds 2
if ($null -eq $runArgument) {
    Start-Process -FilePath  ".\PalServerTools.exe"
}
else {
    $runArgument | ForEach-Object { Start-Process -FilePath  ".\PalServerTools.exe" -ArgumentList "-env $_" }
}