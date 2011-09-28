if(-not(Get-Module -name psake))
{	
	$psake_path = Get-ChildItem .\packages\psake.* | Sort-Object Name | Select-Object -First 1
	Import-Module $psake_path\psake.psm1
}