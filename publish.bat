dotnet build -c release
dotnet publish /p:PublishProfile="%CD%\Properties\PublishProfiles\PublishNewVersion.pubxml"