([string] $NuGetPackageId)

[string] $BranchHead = $Env:GITHUB_REF
[string] $ReleaseBranch = "release"
[string] $NewReleasePostfix = "-new"

function Get-Git-BranchName
{
    param ([string] $GitBranchHead)
    
    [string] $GitBranchFullName = $GitBranchHead.Replace("refs/heads/", "").Trim()
    [string[]] $GitBranchDirs = $GitBranchFullName.Split("/")
    [string] $GitBranchName = $GitBranchDirs[0]

    Return $GitBranchName
}

function Get-ReleaseVersion
{
    param ([string] $GitBranchName)

    [string] $ReleaseVersion = "0.0"

    if ($GitBranchName.StartsWith("$ReleaseBranch")) {
        $ReleaseVersion = $GitBranchName.Replace("$ReleaseBranch-", "")
    }

    Return $ReleaseVersion
}

function Get-LatestNuGetPackageVersion
{
    param ([string] $PackageId, [string] $ReleaseVersion)

    [string] $LatestNuGetReleaseVersion = "0.0.0"
 
    try
    {
        [string] $URL = "https://api.nuget.org/v3-flatcontainer/$PackageId/index.json"
        [Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls, Ssl3"
        $Response = Invoke-WebRequest -Uri $URL -TimeoutSec 15
    
        do{sleep 0.1} until($Response.StatusCode -ne "")

        if ($Response.StatusCode -eq 200)
        {
            [PSCustomObject] $PesponseContent = $Response.Content | ConvertFrom-Json
            [string[]]$NuGetReleaseVersions = $PesponseContent.PSObject.Properties["versions"].Value
            $NuGetReleaseVersions = $NuGetReleaseVersions.Where({$_.StartsWith($ReleaseVersion)}) | Sort-Object -Descending
            if ($NuGetReleaseVersions.Length -gt 0) {
               $LatestNuGetReleaseVersion = $NuGetReleaseVersions[0]
            }
        } else {
            Write-Host "No response received from the $URL."
            Write-Host "Status code: ${Response.StatusCode}"
        }
    }
    catch [System.Net.WebException]
    {
        [string] $WebErrorMessage = $_.ToString()
        if ($WebErrorMessage.Contains("BlobNotFound"))
        {
            $LatestNuGetReleaseVersion = "$ReleaseVersion.0$NewReleasePostfix"
        }
        else
        {
            Write-Warning "Error while response receiving from the $URL."
            Write-Warning $WebErrorMessage
        }
    }

    Return $LatestNuGetReleaseVersion
}

function Get-NewFixVersionWithError
{
    Write-Warning "Can not parse latest NuGet package version $LatestNuGetPackageVersion"
    Write-Warning "Latest NuGet package version must match the mask 'x.x.x'"
    Return -1
}

function Get-NewFixVersion
{
    param([string] $LatestNuGetPackageVersion)
    
    [int32] $FixVersionInt = $Null
    [string] $FixVersion = $LatestNuGetPackageVersion.Split(".")[2]
    
    if ($FixVersion -eq $Null)
    {        
        Return Get-NewFixVersionWithError
    }

    if ($FixVersion.EndsWith($NewReleasePostfix))
    {
        if ([int32]::TryParse($FixVersion.Replace($NewReleasePostfix, ""), [ref]$FixVersionInt))
        {
            Return $FixVersionInt
        }
        Return Get-NewFixVersionWithError
    }

    if ([int32]::TryParse($FixVersion, [ref]$FixVersionInt))
    {
        Return $FixVersionInt + 1
    }
    Return Get-NewFixVersionWithError
}

Write-Host "Start parse branch head: $BranchHead"
[string] $BranchName = Get-Git-BranchName -GitBranchHead $BranchHead

if ($BranchName.StartsWith("$ReleaseBranch")) {
    [string] $ReleaseVersion = Get-ReleaseVersion -GitBranchName $BranchName
    [string] $LatestNuGetPackageVersion = Get-LatestNuGetPackageVersion -PackageId $NuGetPackageId -ReleaseVersion $ReleaseVersion
    [int] $NewFixVersion = Get-NewFixVersion -LatestNuGetPackageVersion $LatestNuGetPackageVersion
    [string] $NewReleaseVersion = "$ReleaseVersion.$NewFixVersion"

    Write-Host "Branch name: $ReleaseBranch"
    echo "BRANCH_NAME=$ReleaseBranch" >> $Env:GITHUB_ENV
    Write-Host "Update release version: $NewReleaseVersion"
    echo "RELEASE_VERSION=$NewReleaseVersion" >> $Env:GITHUB_ENV
} else {
    Write-Host "Branch name: $BranchName"
    echo "BRANCH_NAME=$BranchName" >> $Env:GITHUB_ENV
}
