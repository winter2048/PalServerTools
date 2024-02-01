param(
    [string]$version
)

$releaseNotes = Get-Content "$PSScriptRoot\..\..\..\ReleaseNotes.md" -Encoding utf8

$isAdd = $false
$node = ""
foreach ($row in $releaseNotes) {
    if ($row.StartsWith("###")) {
        $rowVersion = $row.split("v")[1]
        if ($rowVersion -eq $version) {
            $isAdd = $true
        }
    }

    if ($isAdd -and $row.StartsWith("---")) {
        $isAdd = $false
    }

    if ($isAdd) {
        $node += "$row`r`n"
    }
}
Write-Host $node
Set-Content "$PSScriptRoot\..\..\..\ReleaseNote.md" -Value $node -Encoding utf8