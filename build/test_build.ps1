properties {
	$base_dir = Resolve-Path ..\
	$source_dir = "$base_dir\source"
	$test_dir = "$source_dir\PostSharp.NotifyPropertyChanged.Tests\bin\Release"
	$tests = @('PostSharp.NotifyPropertyChanged.Tests.dll') 
	$nunit_dir = (Get-ChildItem "$base_dir\packages\NUnit.Runners.*" | Sort-Object Name | Select-Object -First 1).FullName
	$nunit = "$nunit_dir\tools\nunit-console-x86.exe"
}

task test -depends compile {
	if ($tests.Length -le 0) { 
		Write-Host -ForegroundColor Red 'No tests defined'
		return 
	}

	$test_assemblies = $tests | ForEach-Object { "$test_dir\$_" }

	& $nunit $test_assemblies /noshadow

	if($lastExitCode -ne 0) {
		throw "Tests Failed."
	}

	Write-Host "Finished Runnign Tests"
}