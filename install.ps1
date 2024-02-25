# 安装命令： iex "& { $(irm https://raw.githubusercontent.com/winter2048/PalServerTools/master/install.ps1) }"
# 更新命令： iex "& { $(irm https://raw.githubusercontent.com/winter2048/PalServerTools/master/install.ps1) } -Update"
# 安装指定版本命令： iex "& { $(irm https://raw.githubusercontent.com/winter2048/PalServerTools/master/install.ps1) } -Version v2.0.0"

param(
    [string]$Version = "latest",
    [switch]$Update
)

$script:ErrorActionPreference = 'Stop'

function Install-DotNetRuntime {
    param (
        [string]$Name,
        [string]$SdkVersion
    )
    
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue) -or -not (dotnet --list-runtimes | Where-Object { $_ -match "$Name $SdkVersion" })) {
        Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile "$env:TEMP\dotnet-install.ps1"
        $runtimeName = "dotnet"
        if ($Name -like "*Microsoft.AspNetCore.App*") {
            $runtimeName = "aspnetcore"
        }
        & "$env:TEMP\dotnet-install.ps1" -Channel $SdkVersion -Runtime $runtimeName
        Write-Host "$Name $SdkVersion Runtime 安装成功" -ForegroundColor Green
    }
    else {
        Write-Host "$Name $SdkVersion Runtime 已安装"
    }
}

function Install-PalServerTools {
    param (
        [string]$Owner = "winter2048",
        [string]$RepoName = "PalServerTools",
        [string]$Version = "latest"
    )
    $currentDir = Get-Location
    $folderPath = Join-Path -Path $currentDir -ChildPath "PalServerTools"
    if (Test-Path -Path (Join-Path -Path $folderPath -ChildPath "PalServerTools.exe")) {
        Write-Host "PalServerTools 已安装"
        return $null
    }

    $apiUrl = "https://api.github.com/repos/$Owner/$RepoName/releases/tags/$Version"
    if ($Version -eq "latest") {
        $apiUrl = "https://api.github.com/repos/$Owner/$RepoName/releases/latest"
    }
    $response = Invoke-WebRequest -Uri $apiUrl -UseBasicParsing | ConvertFrom-Json
    $downloadUrl = $response.assets[0].browser_download_url
    $fileName = [System.IO.Path]::GetFileName($downloadUrl)
    Invoke-WebRequest -Uri $downloadUrl -OutFile $fileName
    Expand-Archive -LiteralPath $fileName -DestinationPath "PalServerTools"
    Remove-Item -Path $fileName
    Write-Host "PalServerTools 安装成功" -ForegroundColor Green
}

function Update-PalServerTools {
    param (
        [string]$Owner = "winter2048",
        [string]$RepoName = "PalServerTools"
    )
    $currentDir = Get-Location
    $folderPath = Join-Path -Path $currentDir -ChildPath "PalServerTools"
    if (-not (Test-Path -Path (Join-Path -Path $folderPath -ChildPath "PalServerTools.exe"))) {
        Write-Host "未安装PalServerTools，请在安装目录下执行安装命令。"
        return $null
    }

    $versionInfo = Get-ItemProperty -Path "$folderPath/PalServerTools.exe" | Select-Object -ExpandProperty VersionInfo
    $fileVersion = "v" + $versionInfo.FileVersion.Substring(0, $versionInfo.FileVersion.LastIndexOf('.'))
    Write-Host "当前版本号: $fileVersion"
    $apiUrl = "https://api.github.com/repos/$Owner/$RepoName/releases/latest"
    $response = Invoke-WebRequest -Uri $apiUrl -UseBasicParsing | ConvertFrom-Json
    $latestFileVersion = $response.tag_name
    Write-Output "最新版本号: $latestFileVersion"
    if ($fileVersion -ge $latestFileVersion) {
        Write-Host "PalServerTools 已是最新版本"
        return $null
    }

    if (Get-Process PalServerTools -ErrorAction SilentlyContinue) {
        Write-Error "PalServerTools 正在运行，请关闭后重试。"
    }

    $downloadUrl = $response.assets[0].browser_download_url
    $fileName = [System.IO.Path]::GetFileName($downloadUrl)
    Invoke-WebRequest -Uri $downloadUrl -OutFile $fileName
    Rename-Item -Path "$folderPath/appsettings.json" -NewName "appsettings.json.bak"
    Remove-Item $folderPath -Recurse -Force -Exclude "appsettings.json.bak"
    Expand-Archive -LiteralPath $fileName -DestinationPath "PalServerTools"
    Remove-Item "$folderPath/appsettings.json" -Force
    Rename-Item -Path "$folderPath/appsettings.json.bak" -NewName "appsettings.json"
    Remove-Item -Path $fileName
    Write-Host "PalServerTools 更新成功" -ForegroundColor Green
}

# .NET 6.0 Runtime
Install-DotNetRuntime -Name "Microsoft.NETCore.App" -SdkVersion "6.0"
# ASP.NET Core Runtime
Install-DotNetRuntime -Name "Microsoft.AspNetCore.App" -SdkVersion "6.0"
# PalServerTools
if ($Update -eq $True) {
    Update-PalServerTools
}
else {
    Install-PalServerTools -Version $Version
}