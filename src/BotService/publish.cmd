nuget restore
msbuild BotService.sln -p:DeployOnBuild=true -p:PublishProfile=storytimewebappbot-Web-Deploy.pubxml -p:Password=

