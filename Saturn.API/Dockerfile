FROM mcr.microsoft.com/dotnet/aspnet:6.0-buster-slim-arm32v7 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Saturn.API.csproj", "./"]
RUN dotnet restore "./Saturn.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Saturn.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Saturn.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Saturn.API.dll"]