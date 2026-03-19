FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CustomsExitTrackingSystem.slnx", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/CustomsExitTracking.ServiceB.Api/CustomsExitTracking.ServiceB.Api.csproj", "src/CustomsExitTracking.ServiceB.Api/"]
RUN dotnet restore "src/CustomsExitTracking.ServiceB.Api/CustomsExitTracking.ServiceB.Api.csproj"

COPY . .
RUN dotnet publish "src/CustomsExitTracking.ServiceB.Api/CustomsExitTracking.ServiceB.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CustomsExitTracking.ServiceB.Api.dll"]
