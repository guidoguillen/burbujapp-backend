# Use the official .NET SDK image to build and publish the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY *.sln .
COPY BurbujApp.API/*.csproj ./BurbujApp.API/
COPY BurbujApp.Domain/*.csproj ./BurbujApp.Domain/
COPY BurbujApp.Persistence/*.csproj ./BurbujApp.Persistence/
COPY BurbujApp.Shared/*.csproj ./BurbujApp.Shared/
COPY BurbujApp.Application/*.csproj ./BurbujApp.Application/
COPY BurbujApp.Infrastructure/*.csproj ./BurbujApp.Infrastructure/
COPY BurbujApp.Notification/*.csproj ./BurbujApp.Notification/
COPY BurbujApp.Billing/*.csproj ./BurbujApp.Billing/
COPY BurbujApp.Scheduling/*.csproj ./BurbujApp.Scheduling/
COPY BurbujApp.Identity/*.csproj ./BurbujApp.Identity/
COPY BurbujApp.Inventory/*.csproj ./BurbujApp.Inventory/

# Restore dependencies
RUN dotnet restore "BurbujApp.API/BurbujApp.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/BurbujApp.API"
RUN dotnet publish "BurbujApp.API.csproj" -c Release -o /app/publish

# Use the official ASP.NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app

COPY --from=build /app/publish .
USER appuser

EXPOSE 80
ENTRYPOINT ["dotnet", "BurbujApp.API.dll"]
