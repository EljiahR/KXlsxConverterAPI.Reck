# Use .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY KXlsxConverterAPI/KXlsxConverterAPI.csproj ./KXlsxConverterAPI/
RUN dotnet restore "KXlsxConverterAPI/KXlsxConverterAPI.csproj"

# Copy all files and publish the app
COPY . . 
RUN dotnet publish "KXlsxConverterAPI/KXlsxConverterAPI.csproj" -c Release -o /app/publish

# Use .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy the published app to the runtime container
COPY --from=build /app/publish .

# Install dotnet-ef tool (EF Core CLI tools) in the build stage (not in the runtime stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS ef-tools

# Install dotnet-ef globally
RUN dotnet tool install --global dotnet-ef

# Ensure the .NET tools are in the PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Set the working directory to where your project .csproj is located
WORKDIR /app/KXlsxConverterAPI

# The ENTRYPOINT is set to run migrations first and then the app
ENTRYPOINT ["sh", "-c", "dotnet ef database update --context EmployeeContext --project KXlsxConverterAPI.csproj && dotnet KXlsxConverterAPI.dll"]
