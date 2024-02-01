param(
    [string]$major,
    [string]$minor,
    [string]$patch,
    [string]$tag,
    [string]$refName
)
Write-Host "Env:" 
dir env:

$version = Get-Content "$PSScriptRoot\..\.version"
if (!$major) {
    $major = $version.Split(".")[0]
}
if (!$minor) {
    $minor = $version.Split(".")[1]
}
if (!$patch) {
    $patch = $version.Split(".")[2]
}

$remote_refs = git ls-remote --tags

if ($patch -eq "*") {
    $tags = $remote_refs | Where-Object { $_.ToLower().Contains("refs/tags/v$major.$minor.") -and !$_.ToLower().Contains('-')}
    $latest_tag = $tags | Sort-Object -Property { [int]$_.Split('.')[-1] } -Descending | Select-Object -First 1
    $patch = "0"
    if ($latest_tag) {
        $latest_patch = $latest_tag.Split(".")[-1]
        $patch = [int]$latest_patch + 1
    }
    
}

$CustomBuildVersion = "$major.$minor.$patch"

if ($refName) {
    $tag = $refName.Replace("_","-")
    if ($refName -eq "master") {
        $tag = $null
    }
}
if ($tag -eq "dev") {
    $tags = $remote_refs | Where-Object { $_.ToLower().Contains("refs/tags/v$CustomBuildVersion") -and $_.ToLower().Contains("-$tag")}
    $latest_tag = $tags | Sort-Object -Property { [int]$_.Split('.')[-1] } -Descending | Select-Object -First 1
    $devPatch = "1"
    if ($latest_tag) {
        $latest_patch = $latest_tag.Split(".")[-1]
        $devPatch = [int]$latest_patch + 1
    }
    $CustomBuildVersion = "$CustomBuildVersion-$tag.$devPatch"
}

Write-Host "Setting the value of current build version :  $CustomBuildVersion" 
echo "BUILD_BUILDNUMBER=$CustomBuildVersion" >> $env:GITHUB_OUTPUT
echo "BUILD_BUILDNUMBER=$CustomBuildVersion" >> $env:GITHUB_ENV
echo "BUILD_VERSION=v$CustomBuildVersion" >> $env:GITHUB_OUTPUT
echo "BUILD_VERSION=v$CustomBuildVersion" >> $env:GITHUB_ENV