#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["UserRegistration.API/UserRegistration.API.csproj", "UserRegistration.API/"]
COPY ["UserRegistration.API.Common/UserRegistration.API.Common.csproj", "UserRegistration.API.Common/"]
COPY ["UserRegistration.IoC.Configuration/UserRegistration.IoC.Configuration.csproj", "UserRegistration.IoC.Configuration/"]
COPY ["UserRegistration.Services/UserRegistration.Services.csproj", "UserRegistration.Services/"]
COPY ["UserRegistration.DAL/UserRegistration.DAL.csproj", "UserRegistration.DAL/"]
COPY ["UserRegistration.Services.Model/UserRegistration.Services.Model.csproj", "UserRegistration.Services.Model/"]
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