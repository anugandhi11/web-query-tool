# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/WebQueryTool.Api/WebQueryTool.Api.csproj", "src/WebQueryTool.Api/"]
RUN dotnet restore "src/WebQueryTool.Api/WebQueryTool.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/WebQueryTool.Api"
RUN dotnet build "WebQueryTool.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WebQueryTool.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebQueryTool.Api.dll"]
