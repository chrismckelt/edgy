<# 

Obliterates all resources in an environment (configured for DEV) to blow it away & start again.

 #>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]$workload,
    [string] $subscriptionid,
    [string] $tenantid
)

Write-Host "************************************************"
#& "$debugfile"


Write-Host $(get-location).Path
Write-Host '$ENV:AGENT_WorkFolder'
$fileNames = Get-ChildItem -Path $ENV:AGENT_WorkFolder -Recurse
foreach ($f in $fileNames) {
    Write-Host $f.FullName 
}

#az login --service-principal --username $spn_clientid --password $spn_secret --tenant $tenantid

$armuri = 'https://gist.githubusercontent.com/chrismckelt/4dde0d8a13fa294dfc77d980e6b9beb3/raw/feb1e0ab4fc1f05a5a0cb7f648fca42e2a00b994/arm-resource-group-cleaner.json'

az group deployment create --mode complete --template-uri $armuri --resource-group $workload

 