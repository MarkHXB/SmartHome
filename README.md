# SmartHome

[![.NET](https://github.com/MarkHXB/SmartHome/actions/workflows/dotnet.yml/badge.svg)](https://github.com/MarkHXB/SmartHome/actions/workflows/dotnet.yml)

# Add feature
0. Create a console application or webapi
1. Copy past this propertygroup to csproj
`<PropertyGroup>
<PublishSingleFile>true</PublishSingleFile>
		<SelfContained>true</SelfContained>
		<RuntimeIdentifier>win-x64</RuntimeIdentifier>
	</PropertyGroup>`
2. Change the build definition debug to release
3. Open up saturn and add sln file or the builded exe file to saturn
