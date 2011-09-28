properties
{
	$nuget_pub_dir = "$base_dir\releases\"
	$nuget_dir = Get-ChildItem "$base_dir\packages\NuGet.CommandLine.*" | Sort-Object Name | Select-Object -First 1
	$nuget = "$nuget_dir\NuGet.exe"
}

task package -depends test
{
	Push-Location "$source_dir\PostSharp.NotifyPropertyChanged"
	& $nuget pack .\PostSharp.NotifyPropertyChanged.csproj -Sym -Properties Configuration=Release -Version $version -OutputDirectory $nuget_pub_dir
	Pop-Location
}

task publish
{
	Push-Location "$nuget_pub_dir"
	ls *$version.nupkg | ForEach-Object {& $nuget push $_ }
	Pop-Location
}