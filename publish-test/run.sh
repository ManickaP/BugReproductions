rm -rf bin obj && dotnet build && date +"%M:%S.%N" && dotnet run --no-build
rm -rf bin obj && dotnet publish -r linux-x64 --sc -p:PublishSingleFile=true && date +"%M:%S.%N" && ./bin/Debug/net6.0/linux-x64/publish/publish-test