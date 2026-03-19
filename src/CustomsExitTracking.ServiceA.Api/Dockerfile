FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CustomsExitTrackingSystem.slnx", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/CustomsExitTracking.ServiceA.Api/CustomsExitTracking.ServiceA.Api.csproj", "src/CustomsExitTracking.ServiceA.Api/"]
RUN dotnet restore "src/CustomsExitTracking.ServiceA.Api/CustomsExitTracking.ServiceA.Api.csproj"

COPY . .
RUN dotnet publish "src/CustomsExitTracking.ServiceA.Api/CustomsExitTracking.ServiceA.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CustomsExitTracking.ServiceA.Api.dll"]
