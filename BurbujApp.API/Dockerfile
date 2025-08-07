# Use the official .NET SDK image to build and publish the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "BurbujApp.API.csproj"
RUN dotnet publish "BurbujApp.API.csproj" -c Release -o /app/publish

# Use the official ASP.NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "BurbujApp.API.dll"]
