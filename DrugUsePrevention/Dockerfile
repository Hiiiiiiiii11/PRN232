# Use the official ASP.NET Core runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution file
COPY ["DrugUsePrevention.sln", "./"]

# Copy project files
COPY ["DrugUsePrevention/DrugUsePrevention.csproj", "DrugUsePrevention/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Repositories/Repositories.csproj", "Repositories/"]
COPY ["BussinessObject/BussinessObjects.csproj", "BussinessObject/"]

# Restore dependencies
RUN dotnet restore "DrugUsePrevention/DrugUsePrevention.csproj"

# Copy all source code
COPY . .

# Build the API project
WORKDIR "/src/DrugUsePrevention"
RUN dotnet build "DrugUsePrevention.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DrugUsePrevention.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads/avatars

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "DrugUsePrevention.dll"]