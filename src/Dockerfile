# Multi-platform targeting based on
# https://devblogs.microsoft.com/dotnet/improving-multiplatform-container-support/
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS base
WORKDIR /app
EXPOSE 80

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build

ARG TARGETARCH
ARG BUILDPLATFORM

RUN echo "Building on ${BUILDPLATFORM} and targeting ${TARGETARCH} "

WORKDIR /src

# Copy all project files to allow multi-project restore. See https://andrewlock.net/optimising-asp-net-core-apps-in-docker-avoiding-manually-copying-csproj-files-part-2/
COPY ./*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ./${file%.*}/ && mv $file ./${file%.*}/; done
RUN dotnet restore PhotoBoothService/PhotoBoothService.csproj -a $TARGETARCH

COPY . .
WORKDIR /src/PhotoBoothService
RUN dotnet build PhotoBoothService.csproj -c HeadlessRelease -o /app/build -a $TARGETARCH

FROM build AS publish
RUN dotnet publish PhotoBoothService.csproj -c HeadlessRelease -o /app/publish -a $TARGETARCH

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoBoothService.dll"]
