# Use .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the project file and restore dependencies
COPY KXlsxConverterAPI/KXlsxConverterAPI.csproj ./KXlsxConverterAPI/
RUN dotnet restore "KXlsxConverterAPI/KXlsxConverterAPI.csproj"

# Copy all files and publish the app
COPY . . 
RUN dotnet publish "KXlsxConverterAPI/KXlsxConverterAPI.csproj" -c Release -o /app/publish

# Use .NET SDK image for the runtime, so we can run migrations
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base  # Use runtime image for running the app

WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy the published app to the runtime container
COPY --from=build /app/publish .

# Install dotnet-ef tool (EF Core CLI tools)
RUN dotnet tool install --global dotnet-ef

# Ensure that the PATH includes the location of global tools
ENV PATH="$PATH:/root/.dotnet/tools"

# Set the working directory to where your project .csproj is located
WORKDIR /src/KXlsxConverterAPI  # This should match the folder where your .csproj file resides inside the container

# The ENTRYPOINT is set to run migrations first and then the app
ENTRYPOINT ["sh", "-c", "dotnet ef database update --context EmployeeContext --project KXlsxConverterAPI.csproj && dotnet KXlsxConverterAPI.dll"]
