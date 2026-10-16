# API Dockerfile - multi-stage build for .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ["ITAssetManagement.API/ITAssetManagement.API.csproj", "ITAssetManagement.API/"]
COPY ["ITAssetManagement.Data/ITAssetManagement.Data.csproj", "ITAssetManagement.Data/"]
COPY ["ITAssetManagement.Core/ITAssetManagement.Core.csproj", "ITAssetManagement.Core/"]
RUN dotnet restore "ITAssetManagement.API/ITAssetManagement.API.csproj"

# copy everything and build
COPY . .
WORKDIR /src/ITAssetManagement.API
RUN dotnet publish "ITAssetManagement.API.csproj" -c Release -o /app/publish --no-restore

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80
ENTRYPOINT ["dotnet", "ITAssetManagement.API.dll"]
