# Use the official ASP.NET Core runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8082
EXPOSE 8083

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution file
COPY ["DrugUsePrevention.sln", "./"]

# Copy project files
COPY ["DrugUserPreventionUI/DrugUserPreventionUI.csproj", "DrugUserPreventionUI/"]
COPY ["BussinessObject/BussinessObjects.csproj", "BussinessObject/"]

# Restore dependencies
RUN dotnet restore "DrugUserPreventionUI/DrugUserPreventionUI.csproj"

# Copy all source code
COPY . .

# Build the UI project
WORKDIR "/src/DrugUserPreventionUI"
RUN dotnet build "DrugUserPreventionUI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DrugUserPreventionUI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory for user avatars
RUN mkdir -p /app/wwwroot/uploads/avatars

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8082

ENTRYPOINT ["dotnet", "DrugUserPreventionUI.dll"]