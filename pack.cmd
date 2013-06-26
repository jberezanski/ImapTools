msbuild /nologo /v:m /p:Configuration=Release && nuget pack nuspec\ImapTools.nuspec -nopackageanalysis -version 1.0.0.0 -outputdirectory output
