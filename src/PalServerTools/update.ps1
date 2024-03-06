$Owner = "winter2048"
$RepoName = "PalServerTools"

$folderPath = Get-Location
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
    if (!(Get-Process PalServerTools -ErrorAction SilentlyContinue)) {
        Start-Sleep -Seconds 2 
        Start-Process -FilePath  ".\PalServerTools.exe"
    }
    return $null
}

while ($true) {
    if (Get-Process PalServerTools -ErrorAction SilentlyContinue) {
        Write-Warning "PalServerTools 正在运行，等待关闭后继续。"
        Start-Sleep -Seconds 2 
    }
    else {
        break
    }
}

$downloadUrl = $response.assets[0].browser_download_url
$fileName = [System.IO.Path]::GetFileName($downloadUrl)
Invoke-WebRequest -Uri $downloadUrl -OutFile $fileName
Rename-Item -Path ".\appsettings.json" -NewName "appsettings.json.bak"
Remove-Item ".\*" -Recurse -Force -Exclude @("appsettings.json.bak", "$fileName")
Expand-Archive -LiteralPath $fileName -DestinationPath "PalServerTools"
Move-Item -Path ".\PalServerTools\*" -Destination ".\"
Remove-Item ".\appsettings.json" -Force
Rename-Item -Path ".\appsettings.json.bak" -NewName "appsettings.json"
Remove-Item -Path $fileName
Remove-Item -Path "PalServerTools"
Write-Host "PalServerTools 更新成功" -ForegroundColor Green
Start-Sleep -Seconds 2 
Start-Process -FilePath  ".\PalServerTools.exe"