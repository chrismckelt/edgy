<# 

Create the variables names according to the environment

 #>


if ([string]::IsNullOrEmpty($subscriptionid)){
    $subscriptionid = [Environment]::GetEnvironmentVariable('edgex.subscription', "User") 
    $tenantid = [Environment]::GetEnvironmentVariable('edgex.tenant', "User") 
}

$environment = "dev";
$location1 = "australiaeast"
$location2 = "australiasoutheast"
$workload = "auaze-edgex-$environment".ToLower()
$workload_abbr = $workload.Replace('-','x').ToLower()
$d1 = "LocalDevice"
$d2 = "CloudDevice"


 # copy these to each script 
Write-Host "************************************************"
Write-Host "variables"
Write-Host "************************************************"
Write-host "subscriptionid = $subscriptionid"
Write-host "tenantid = $tenantid"
Write-host "environment = $environment"


