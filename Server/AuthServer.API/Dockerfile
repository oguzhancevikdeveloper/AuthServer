#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Server/AuthServer.API/AuthServer.API.csproj", "Server/AuthServer.API/"]
COPY ["Shared/AuthServer.Shared/AuthServer.Shared.csproj", "Shared/AuthServer.Shared/"]
COPY ["Server/AuthServer.Service/AuthServer.Service.csproj", "Server/AuthServer.Service/"]
COPY ["Server/AuthServer.Data/AuthServer.Data.csproj", "Server/AuthServer.Data/"]
COPY ["Server/AuthServer.Core/AuthServer.Core.csproj", "Server/AuthServer.Core/"]
RUN dotnet restore "./Server/AuthServer.API/AuthServer.API.csproj"
COPY . .
WORKDIR "/src/Server/AuthServer.API"
RUN dotnet build "./AuthServer.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AuthServer.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthServer.API.dll"]