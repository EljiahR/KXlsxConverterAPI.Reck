# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the CSPROJ file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the project files and build the app
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image for the application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose the port the app runs on
EXPOSE 80

# Define the entry point for the container
ENTRYPOINT ["dotnet", "KXlsxConverterAPI.Reck.dll"]
