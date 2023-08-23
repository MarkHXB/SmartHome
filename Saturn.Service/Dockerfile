#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Saturn.Service/Saturn.Service.csproj", "Saturn.Service/"]
RUN dotnet restore "Saturn.Service/Saturn.Service.csproj"
COPY . .
WORKDIR "/src/Saturn.Service"
RUN dotnet build "Saturn.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Saturn.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Saturn.Service.dll"]