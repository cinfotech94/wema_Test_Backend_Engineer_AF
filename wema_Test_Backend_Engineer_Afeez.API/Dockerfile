#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["wema_Test_Backend_Engineer_Afeez.API/wema_Test_Backend_Engineer_Afeez.API.csproj", "wema_Test_Backend_Engineer_Afeez.API/"]
COPY ["wema_Test_Backend_Engineer_Afeez.Data/wema_Test_Backend_Engineer_Afeez.Data.csproj", "wema_Test_Backend_Engineer_Afeez.Data/"]
COPY ["wema_Test_Backend_Engineer_Afeez.Domain/wema_Test_Backend_Engineer_Afeez.Domain.csproj", "wema_Test_Backend_Engineer_Afeez.Domain/"]
COPY ["wema_Test_Backend_Engineer_Afeez.Services/wema_Test_Backend_Engineer_Afeez.Services.csproj", "wema_Test_Backend_Engineer_Afeez.Services/"]
RUN dotnet restore "./wema_Test_Backend_Engineer_Afeez.API/wema_Test_Backend_Engineer_Afeez.API.csproj"
COPY . .
WORKDIR "/src/wema_Test_Backend_Engineer_Afeez.API"
RUN dotnet build "./wema_Test_Backend_Engineer_Afeez.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./wema_Test_Backend_Engineer_Afeez.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "wema_Test_Backend_Engineer_Afeez.API.dll"]