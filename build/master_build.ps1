properties {
	$base_dir = Resolve-Path ..\
	$source_dir = "$base_dir\source"
	$tool_dir = "$base_dir\packages"
	$sln_file = "$base_dir\PostSharp.NotifyPropertyChanged.sln"
	$sharedAssemblyInfo = "$source_dir\SolutionAssemblyInfo.cs"
	$msbuild = "$env:windir\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
}

task version {
	$version_pattern = "\d*\.\d*\.\d*\.\d*"
	$content =
		Get-Content $sharedAssemblyInfo |
			ForEach { [regex]::replace($_, $version_pattern, $version) } 

	Set-Content -Value $content -Path $sharedAssemblyInfo
}

task compile -depends version {
	& $msbuild $sln_file /p:Configuration=Release
  
	if($lastExitCode -ne 0) {
		throw "Compile Failed."
	}
}