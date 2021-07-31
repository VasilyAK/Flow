param ([string] $BranchHead)

function Get-Git-BranchName {
    param ([string] $GitBranchHead)
    
    [string] $GitBranchFullName = $GitBranchHead.Replace("refs/heads/", "").Trim()
    [string[]] $GitBranchDirs = $GitBranchFullName.Split("/")
    [string] $GitBranchName = $GitBranchDirs[0]

    Write-Output $GitBranchName
}

[string] $BranchName = Get-Git-BranchName -GitBranchHead $BranchHead
Write-Host "Branch name: $BranchName"
BRANCH_NAME=$BranchName >> $BRANCH_NAME