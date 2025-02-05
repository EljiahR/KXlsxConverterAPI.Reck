# KXlsxConverterAPI.Reck

A backend API designed to handle the conversion of `.xlsx` files, developed using C# and Docker.

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Running the Application](#running-the-application)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)

## Introduction

The `KXlsxConverterAPI.Reck` is a backend service that processes `.xlsx` file conversions. It is built with C# and is containerized using Docker for easy deployment and scalability.

## Features

- Accepts `.xlsx` files for conversion.
- Provides endpoints for uploading files and retrieving converted data.
- Scalable and containerized for efficient deployment.

## Getting Started

### Prerequisites

Ensure you have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/download) (version 5.0 or later)
- [Docker](https://www.docker.com/get-started)

### Installation

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/EljiahR/KXlsxConverterAPI.Reck.git
   cd KXlsxConverterAPI.Reck
   ```

2. **Build the Docker Image:**

   ```bash
   docker build -t kxlsxconverterapi .
   ```

### Running the Application

To run the application using Docker:

```bash
docker run -d -p 5000:80 kxlsxconverterapi
```

The API will be accessible at `http://localhost:5000`.

## Project Structure

The project's structure is as follows:

```
KXlsxConverterAPI.Reck/
├── .github/
│   └── workflows/
├── KXlsxConverterAPI/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── KXlsxConverterAPI.csproj
│   └── Program.cs
├── .gitattributes
├── .gitignore
├── KXlsxConverterAPI.Reck.sln
└── Dockerfile
```

- **.github/workflows/**: Contains GitHub Actions workflows for CI/CD.
- **KXlsxConverterAPI/**: Main project directory.
  - **Controllers/**: API controllers handling HTTP requests.
  - **Models/**: Data models used in the application.
  - **Services/**: Business logic and services.
  - `KXlsxConverterAPI.csproj`: Project file defining the C# project.
  - `Program.cs`: Entry point of the application.
- `.gitattributes`: Git attributes configuration.
- `.gitignore`: Specifies files to ignore in the repository.
- `KXlsxConverterAPI.Reck.sln`: Solution file for the project.
- `Dockerfile`: Instructions to build the Docker image.

## License

This project is licensed under the [MIT License](LICENSE).

