$nupkgFiles = Get-ChildItem -Path .\bin\Debug -Filter *.nupkg
foreach ($file in $nupkgFiles) {
   $output = dotnet nuget push $file.FullName --source nuget.org --skip-duplicate 
   Write-Output $output
}

$nupkgFiles = Get-ChildItem -Path .\bin\Debug -Filter *.snupkg
foreach ($file in $nupkgFiles) {
   $output = dotnet nuget push $file.FullName --source nuget.org --skip-duplicate 
   Write-Output $output
}