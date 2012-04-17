properties {
	$nuget_pub_dir = "$base_dir\releases\"
	$nuget_dir = (Get-ChildItem "$base_dir\packages\NuGet.CommandLine.*" | Sort-Object Name | Select-Object -First 1).FullName
	$nuget = "$nuget_dir\tools\NuGet.exe"
}

task package -depends test {
	New-Item -Path $base_dir -Name releases -Type directory -Force
	Push-Location "$source_dir\PostSharp.NotifyPropertyChanged"
	& $nuget pack .\PostSharp.NotifyPropertyChanged.csproj -Sym -Properties 'Configuration=Release;ProjectUrl=https://github.com/robertream/PostSharp.NotifyPropertyChanged' -Version $version -OutputDirectory $nuget_pub_dir
	Pop-Location
}

task publish {
	Push-Location "$nuget_pub_dir"
	ls *$version.nupkg | ForEach-Object {& $nuget push $_ }
	Pop-Location
}