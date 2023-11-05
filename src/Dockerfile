FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/PhotoBoothService
RUN dotnet build PhotoBoothService.csproj -c HeadlessRelease -o /app/build

FROM build AS publish
RUN dotnet publish PhotoBoothService.csproj -c HeadlessRelease -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoBoothService.dll"]
