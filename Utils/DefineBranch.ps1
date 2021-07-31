[string] $BranchHead = ${env:$GITHUB_REF}

function Get-Git-BranchName {
    param ([string] $GitBranchHead)
    
    [string] $GitBranchFullName = $GitBranchHead.Replace("refs/heads/", "").Trim()
    [string[]] $GitBranchDirs = $GitBranchFullName.Split("/")
    [string] $GitBranchName = $GitBranchDirs[0]

    Write-Output $GitBranchName
}

[string] $BranchName = Get-Git-BranchName -GitBranchHead $BranchHead
Write-Host "Branch name: $BranchName"
${env:$BRANCH_NAME} = $BranchName