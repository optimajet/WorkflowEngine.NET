# set output encoding
$OutputEncoding = [Text.UTF8Encoding]::UTF8

# company name placeholder 
$oldCompanyName="AbpCompanyName"
# your company name
$newCompanyName="YourCompanyName"

# project name placeholder
$oldProjectName="AngularBPWorkflow"
# your project name
$newProjectName="YourProjectName"

# file type
$fileType="FileInfo"

# directory type
$dirType="DirectoryInfo"

# copy 
Write-Host 'Start copy folders...'
$newRoot=$newCompanyName+"."+$newProjectName
mkdir $newRoot
Copy-Item -Recurse .\aspnet-core\ .\$newRoot\
Copy-Item -Recurse .\vue\ .\$newRoot\
Copy-Item .gitignore .\$newRoot\
Copy-Item LICENSE .\$newRoot\
Copy-Item README.md .\$newRoot\

# folders to deal with
$slnFolder = (Get-Item -Path "./$newRoot/aspnet-core/" -Verbose).FullName
$vueFolder = (Get-Item -Path "./$newRoot/vue/" -Verbose).FullName

function Rename {
	param (
		$TargetFolder,
		$PlaceHolderCompanyName,
		$PlaceHolderProjectName,
		$NewCompanyName,
		$NewProjectName
	)
	# file extensions to deal with
	$include=@("*.cs","*.cshtml","*.asax","*.ps1","*.ts","*.csproj","*.sln","*.xaml","*.json","*.js","*.xml","*.config","Dockerfile")

	$elapsed = [System.Diagnostics.Stopwatch]::StartNew()

	Write-Host "[$TargetFolder]Start rename folder..."
	# rename folder
	Ls $TargetFolder -Recurse | Where { $_.GetType().Name -eq $dirType -and ($_.Name.Contains($PlaceHolderCompanyName) -or $_.Name.Contains($PlaceHolderProjectName)) } | ForEach-Object{
		Write-Host 'directory ' $_.FullName
		$newDirectoryName=$_.Name.Replace($PlaceHolderCompanyName,$NewCompanyName).Replace($PlaceHolderProjectName,$NewProjectName)
		Rename-Item $_.FullName $newDirectoryName
	}
	Write-Host "[$TargetFolder]End rename folder."
	Write-Host '-------------------------------------------------------------'


	# replace file content and rename file name
	Write-Host "[$TargetFolder]Start replace file content and rename file name..."
	Ls $TargetFolder -Include $include -Recurse | Where { $_.GetType().Name -eq $fileType} | ForEach-Object{
		$fileText = Get-Content $_ -Raw -Encoding UTF8
		if($fileText.Length -gt 0 -and ($fileText.contains($PlaceHolderCompanyName) -or $fileText.contains($PlaceHolderProjectName))){
			$fileText.Replace($PlaceHolderCompanyName,$NewCompanyName).Replace($PlaceHolderProjectName,$NewProjectName) | Set-Content $_ -Encoding UTF8
			Write-Host 'file(change text) ' $_.FullName
		}
		If($_.Name.contains($PlaceHolderCompanyName) -or $_.Name.contains($PlaceHolderProjectName)){
			$newFileName=$_.Name.Replace($PlaceHolderCompanyName,$NewCompanyName).Replace($PlaceHolderProjectName,$NewProjectName)
			Rename-Item $_.FullName $newFileName
			Write-Host 'file(change name) ' $_.FullName
		}
	}
	Write-Host "[$TargetFolder]End replace file content and rename file name."
	Write-Host '-------------------------------------------------------------'

	$elapsed.stop()
	write-host "[$TargetFolder]Total Time Cost: $($elapsed.Elapsed.ToString())"
}

Rename -TargetFolder $slnFolder -PlaceHolderCompanyName $oldCompanyName -PlaceHolderProjectName $oldProjectName -NewCompanyName $newCompanyName -NewProjectName $newProjectName
Rename -TargetFolder $vueFolder -PlaceHolderCompanyName $oldCompanyName -PlaceHolderProjectName $oldProjectName -NewCompanyName $newCompanyName -NewProjectName $newProjectName

