# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY WebQueryTool.API/WebQueryTool.API.csproj WebQueryTool.API/
RUN dotnet restore "WebQueryTool.API/WebQueryTool.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/WebQueryTool.API"
RUN dotnet build "WebQueryTool.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WebQueryTool.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebQueryTool.API.dll"]
