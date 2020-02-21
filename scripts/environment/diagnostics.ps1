



function setDiagnosticsSettings() {

	$workspaces = Get-AzOperationalInsightsWorkspace -ResourceGroupName $global:workload
	if (@($workspaces).Length -eq 0)
	{
		throw "No log analytics workspace in resource group: $global:workload"
	}
	else
	{
		$workspacesResourceId = $workspaces[0].ResourceId
	}

	$resourceTypes = @(
		#"Microsoft.Logic/workflows",
		#"Microsoft.ApiManagement/service",
	#	"Microsoft.KeyVault/vaults",
#		"Microsoft.ServiceBus/namespaces",
#		"Microsoft.Web/sites",
   #     "Microsoft.Web/serverfarms",
		#"Microsoft.Sql/servers/databases",
        #"Microsoft.Network/applicationGateways",
        "Microsoft.Devices/IotHubs"
	)

	$rg = Get-AzResourceGroup -Name $global:_workload

	if($null -ne $rg){

		foreach ($resourceType in $resourceTypes)
		{
			# $resourceType =  'Microsoft.Logic/workflows';
			# https://www.codeisahighway.com/how-to-safely-replace-find-Azresource-resourcetype-calls-in-azure-powershell-6-x/

			Write-Host "Lookup for $($resourceType) in $($rg.ResourceGroupName) ..."
			$rgname = $rg.ResourceGroupName
			$resources = Get-AzResource -ODataQuery "`$filter=resourcetype eq '$resourceType' and resourcegroup eq '$rgname'" -WarningAction Ignore
			Write-Host $resources
			foreach ($resource in $resources)
			{
				# $resource = $resources[0]
				Write-Host "Check Diagnostic Settings for $($resource.Name)..." -NoNewline
				$setting = Get-AzDiagnosticSetting -ResourceId $resource.ResourceId -WarningAction Ignore -ErrorAction Ignore
				if(($null -eq $setting) -or $setting.WorkspaceId -eq '' -or $setting.WorkspaceId -ne $workspacesResourceId -or @($setting.Metrics).Length -eq 0 -or (-not $setting.Metrics[0].Enabled))
				{
					Write-Host
					Write-Host "Set Diagnostic Settings for $($resource.Name)..." -NoNewline
					try {
						$setting = Set-AzDiagnosticSetting -ResourceId $resource.ResourceId -Enabled $True  -WorkspaceId $workspacesResourceId  -WarningAction Continue -ErrorAction Continue
						Write-Host "done!" -ForegroundColor DarkGreen
					}
					catch {
						Write-Host
						Write-Warning "FAILED to add diagnostic for $($resource.Name) of type $($resourceType) in $($rg.ResourceGroupName)"
					}
				}
				else
				{
					Write-Host "enabled!" -ForegroundColor DarkGreen
				}
			}
		}

		Write-Host "Finished Lookup Resource Group $($rg.ResourceGroupName)"

	}
}