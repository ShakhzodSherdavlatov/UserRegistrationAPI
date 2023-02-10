#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.API/UserRegistrationAPI.API.csproj", "UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.API/"]
COPY ["UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.API.Common/UserRegistrationAPI.API.Common.csproj", "UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.API.Common/"]
COPY ["UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.Services/UserRegistrationAPI.Services.csproj", "UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.Services/"]
COPY ["UserRegistrationAPI.DAL/UserRegistrationAPI.DAL.csproj", "UserRegistrationAPI.DAL/"]
COPY ["UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.Services.Model/UserRegistrationAPI.Services.Model.csproj", "UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.Services.Model/"]
COPY ["UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.IoC.Configuration/UserRegistrationAPI.IoC.Configuration.csproj", "UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.IoC.Configuration/"]
RUN dotnet restore "UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.API/UserRegistrationAPI.API.csproj"
COPY . .
WORKDIR "/src/UserRegistrationAPI/UserRegistrationAPI/UserRegistrationAPI.API"
RUN dotnet build "UserRegistrationAPI.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserRegistrationAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserRegistrationAPI.API.dll"]