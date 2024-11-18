# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the CSPROJ file into the container and restore dependencies
COPY KXlsxConverterAPI/KXlsxConverterAPI.csproj ./KXlsxConverterAPI/
WORKDIR /app/KXlsxConverterAPI

RUN dotnet restore

# Copy the entire project into the container and build it
COPY . /app
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET runtime image to host the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KXlsxConverterAPI.dll"]
