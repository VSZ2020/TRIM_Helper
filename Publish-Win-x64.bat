dotnet publish -c Release -r win-x64 -p:PublishTrimmed=True -p:TrimMode=CopyUsed -p:PublishSingleFile=true --self-contained true
pause