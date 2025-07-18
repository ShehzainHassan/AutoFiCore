# Use the official .NET SDK image for build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set working directory
WORKDIR /app

# Copy main project file and restore dependencies
COPY AutoFiCore/AutoFiCore.csproj ./AutoFiCore/
WORKDIR /app/AutoFiCore
RUN dotnet restore

# Copy source code
WORKDIR /app
COPY AutoFiCore/ ./AutoFiCore/

# Build and publish the application
WORKDIR /app/AutoFiCore
RUN dotnet publish -c Release -o /app/publish --no-restore

# Use the official ASP.NET runtime image for final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Create non-root user for security
RUN adduser --disabled-password --gecos '' --uid 1000 dotnetuser

# Set working directory
WORKDIR /app

# Copy published application from build stage
COPY --from=build /app/publish .

# Set ownership of the application directory to the non-root user
RUN chown -R dotnetuser:dotnetuser /app

# Switch to non-root user
USER dotnetuser

# Expose port (Railway automatically assigns PORT env var)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Start the application
ENTRYPOINT ["dotnet", "AutoFiCore.dll"] 