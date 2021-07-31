[string] $BranchName = ${env:$BRANCH_NAME}
[string] $ReleaseBranch = "release"

function Get-ReleaseVersion {
    param ([string] $GitBranchName)

    [string] $ReleaseVersion = "no release"

    if ($GitBranchName.StartsWith("$ReleaseBranch")) {
        $ReleaseVersion = $GitBranchName.Replace("$ReleaseBranch-", "")
    }

    Write-Output $ReleaseVersion
}

[string] $ReleaseVersion = Get-ReleaseVersion -GitBranchName $BranchName
Write-Host "Release version: $ReleaseVersion"
