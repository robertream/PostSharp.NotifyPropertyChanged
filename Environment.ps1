if(-not(Get-Module -name psake))
{
	$psake_path = (Get-ChildItem .\packages\psake.* | Sort-Object Name | Select-Object -First 1).FullName
	Import-Module $psake_path\tools\psake.psm1
	Echo "Imported psake from $psake_path\tools\psake.psm1"
}