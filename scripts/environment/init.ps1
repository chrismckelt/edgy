<# 

initialize an Azure Edge environment
create a linux VM with IOT edge on it
create a virtual device on the IOT edge device
deploy modules to the devices


 #>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)][AllowNull()]$environment,
    [string] $subscriptionid,
    [string] $tenantid
)

#& "$debugfile"
Write-Host "************************************************"
Write-Host "environment init"
Write-Host "************************************************"

# https://docs.microsoft.com/en-us/cli/azure/ext/azure-cli-iot-ext/iot/hub?view=azure-cli-latest
# create resource group
az group create -l $location1 -n $workload.ToUpper()
# setup monitoring
# az monitor log-analytics workspace list --resource-group $workload --subscription $subscriptionid
# az monitor log-analytics workspace show --resource-group $workload --workspace-name $workload --subscription $subscriptionid
# az monitor diagnostic-settings create --name "diags" --resource-group $workload --logs '[{"category": "AuditEvent","enabled": true}]'  --metrics '[{"category": "AllMetrics","enabled": true}]' --workspace /subscriptions/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/resourcegroups/oi-default-east-us/providers/microsoft.operationalinsights/workspaces/myworkspace \
# --event-hub-rule /subscriptions/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/resourceGroups/myresourcegroup/providers/Microsoft.EventHub/namespaces/myeventhub/authorizationrules/RootManageSharedAccessKey
#create app insights
# https://github.com/Azure/azure-cli/issues/5543#issuecomment-365001620
az resource create --resource-group $workload --resource-type "Microsoft.Insights/components" --name $workload --location $location1 --properties '{\"Application_Type\":\"web\"}'

#create container registry
az acr create --name $workload_abbr --resource-group $workload --sku Standard;
# blob
az storage account create --name $workload_abbr --resource-group $workload --location $location1 --sku Standard_LRS --encryption blob;
$storage_connection=az storage account show-connection-string -n  $workload_abbr -g $workload --query connectionString -o tsv;
#$env:AZURE_STORAGE_CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=auazexedgexxdev;AccountKey=OPPv3Jxo5VMxSWgdHep9JB/M20HUpyf5ejEUW3nfwdguVWwwU0l8g1ybRO24i2D0vH8DSeZ6phgCi2VLteN3qw==;EndpointSuffix=core.windows.net"
$env:AZURE_STORAGE_CONNECTION_STRING = $storage_connection
# container
az storage container create --name 'certs' 
# az keyvault create --name $keyvaultname --resource-group $workload --location $location1 --subscription $subscriptionid --enabled-for-deployment true
# create iot hub
az iot hub create --name $workload --resource-group $workload  --sku F1 --partition-count 2

#device
# az iot hub device-identity delete --hub-name $workload --device-id $d1 
# az iot hub device-identity delete --hub-name $workload --device-id $d2 

az iot hub device-identity create --hub-name $workload --device-id $d1 --edge-enabled
az iot hub device-identity create --hub-name $workload --device-id $d2 --edge-enabled


# https://seeedjp.github.io/ReButton/IoT_Hub.html
# az iot hub list
# az iot hub show --name $workload
# az iot hub show-stats --name $workload

# https://docs.microsoft.com/en-us/azure/iot-edge/quickstart-linux
# create linux vm with IOT edge runtime installed
az vm image terms accept --urn microsoft_iot_edge:iot_edge_vm_ubuntu:ubuntu_1604_edgeruntimeonly:latest;

if ([string]::IsNullOrEmpty($adminpassword)){
    Write-Host "Must set admin password variable $adminpassword"
    Exit-PSHostProcess
    throw
}

az vm create --resource-group $workload --name $workload_abbr --image microsoft_iot_edge:iot_edge_vm_ubuntu:ubuntu_1604_edgeruntimeonly:latest --admin-username azureuser --admin-password "$adminpassword" --generate-ssh-keys;

# create a device
# get connection string for device
$connection_string = az iot hub device-identity show-connection-string --device-id $d2 --hub-name $workload --query connectionString ;

# set edge vm connection string
az vm run-command invoke -g $workload -n $workload_abbr --command-id RunShellScript --script "/etc/iotedge/configedge.sh '$connection_string'"
# get vm public ip for ssh
$ip = az vm show -d -g $workload -n $workload_abbr --query publicIps -o tsv 

# az vm restart --resource-group $workload --name $workload 

$sas = az iot hub generate-sas-token --device-id $d1 --hub-name $workload --query sas

# simultate device message to hub
#  https://docs.microsoft.com/en-us/cli/azure/ext/azure-cli-iot-ext/iot/device?view=azure-cli-latest
az iot device send-d2c-message -n $workload -d $d1 --data "device test " --login $connection_string
#az iot device simulate -n $workload -d $d1  --protocol http
az iot device c2d-message send  -n $workload -d $d1 --data 'cloud to device test' --props 'key0=value0;key1=value1'
#az iot device simulate -n $workload -d $workload --msg-count 1000 --msg-interval 5 --login $connection_string

ssh azureuser@$ip

# setup a dns name so you can do this
# ssh azureuser@auazexedgexxdev.australiaeast.cloudapp.azure.com