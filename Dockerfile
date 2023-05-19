#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["UserRegistration.API/UserRegistration.API.csproj", "UserRegistration.API/"]
COPY ["UserRegistration.API.Common/UserRegistration.API.Common.csproj", "UserRegistration.API.Common/"]
COPY ["UserRegistration.IoC.Configuration/UserRegistration.IoC.Configuration.csproj", "UserRegistration.IoC.Configuration/"]
COPY ["UserRegistration.Services/UserRegistration.Services.csproj", "UserRegistration.Services/"]
COPY ["UserRegistration.Services.Model/UserRegistration.Services.Model.csproj", "UserRegistration.Services.Model/"]
COPY ["UserRegistration.DAL/UserRegistration.DAL.csproj", "UserRegistration.DAL/"]
COPY ["UserRegistration.API.DataContracts/UserRegistrationAPI.API.DataContracts.csproj", "UserRegistration.API.DataContracts/"]

RUN dotnet new nugetconfig  
RUN dotnet nuget add source "https://gitlab.com/api/v4/projects/32362468/packages/nuget/index.json" -n CommonTools -u developer -p xBXjt88qjjDozm_GzwS1 --store-password-in-clear-text --configfile nuget.config  

RUN dotnet restore "UserRegistration.API/UserRegistration.API.csproj"
COPY . .
WORKDIR "/src/UserRegistration.API"
RUN dotnet build "UserRegistration.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserRegistration.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserRegistration.API.dll"]
