FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["KubernetesSample.Api/KubernetesSample.Api.csproj", "KubernetesSample.Api/"]
RUN dotnet restore "./KubernetesSample.Api/./KubernetesSample.Api.csproj"
COPY . .
WORKDIR "/src/KubernetesSample.Api"
RUN dotnet build "./KubernetesSample.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KubernetesSample.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KubernetesSample.Api.dll"]