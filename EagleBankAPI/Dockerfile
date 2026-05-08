# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["EagleBankAPI/EagleBankAPI.csproj", "EagleBankAPI/"]
COPY ["EagleBankAPI.Core/EagleBankAPI.Core.csproj", "EagleBankAPI.Core/"]
COPY ["EagleBankAPI.DAL/EagleBankAPI.DAL.csproj", "EagleBankAPI.DAL/"]
RUN dotnet restore "EagleBankAPI/EagleBankAPI.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/EagleBankAPI"
RUN dotnet build "EagleBankAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EagleBankAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Run the application
ENTRYPOINT ["dotnet", "EagleBankAPI.dll"]