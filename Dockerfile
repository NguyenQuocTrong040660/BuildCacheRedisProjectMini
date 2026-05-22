# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["BuildCacheRedisProjectMini.csproj", "./"]
RUN dotnet restore "BuildCacheRedisProjectMini.csproj"

# Copy the remaining files and build
COPY . .
RUN dotnet build "BuildCacheRedisProjectMini.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "BuildCacheRedisProjectMini.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BuildCacheRedisProjectMini.dll"]
