# Web Dockerfile - multi-stage build for .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ITAssetManagement.Web/ITAssetManagement.Web.csproj", "ITAssetManagement.Web/"]
COPY ["ITAssetManagement.Core/ITAssetManagement.Core.csproj", "ITAssetManagement.Core/"]
COPY ["ITAssetManagement.Data/ITAssetManagement.Data.csproj", "ITAssetManagement.Data/"]
RUN dotnet restore "ITAssetManagement.Web/ITAssetManagement.Web.csproj"

COPY . .
WORKDIR /src/ITAssetManagement.Web
RUN dotnet publish "ITAssetManagement.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80
ENTRYPOINT ["dotnet", "ITAssetManagement.Web.dll"]
