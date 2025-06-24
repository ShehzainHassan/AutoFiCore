# AutoFiCore

A modern .NET Core API for vehicle management and auto loan calculations.

## Overview

AutoFiCore is a robust, scalable backend solution designed to manage vehicle inventory and perform loan calculations. Built with .NET 9.0, it provides a RESTful API that can be integrated with any frontend application.

## Features

- **Vehicle Management**: Complete CRUD operations for vehicle inventory
- **Loan Calculations**: Calculate monthly payments, interest, and total cost
- **Flexible Data Source**: Works with both mock data and PostgreSQL database
- **RESTful API**: Clean, well-documented endpoints following REST best practices

## Technology Stack

- **.NET 9.0**: Latest .NET framework for optimal performance
- **Entity Framework Core 9.0**: ORM for database operations
- **PostgreSQL**: Database option for production environments
- **Swagger/OpenAPI**: API documentation and testing

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) (optional, only if not using mock data)

## Installation

1. Clone the repository

   ```bash
   git clone https://github.com/yourusername/AutoFiCore.git
   cd AutoFiCore
   ```

2. Restore dependencies

   ```bash
   dotnet restore
   ```

3. Build the project
   ```bash
   dotnet build
   ```

## Configuration

The application can be configured via the `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=autofi;Username=postgres;Password=postgres"
  },
  "UseMockApi": true
}
```

- Set `UseMockApi` to `true` to use mock data (no database required)
- Set `UseMockApi` to `false` to use PostgreSQL database (requires connection string)

## Running the Application

### Development Environment

```bash
dotnet run --launch-profile http
```

This will start the application at http://localhost:5011

For HTTPS:

```bash
dotnet run --launch-profile https
```

This will start the application at https://localhost:7050 and http://localhost:5011

### Production Environment

```bash
dotnet run --configuration Release
```

## API Endpoints

| Method | Endpoint      | Description                 |
| ------ | ------------- | --------------------------- |
| GET    | /Vehicle      | Retrieve all vehicles       |
| GET    | /Vehicle/{id} | Retrieve a specific vehicle |
| POST   | /Vehicle      | Create a new vehicle        |
| PUT    | /Vehicle/{id} | Update an existing vehicle  |
| DELETE | /Vehicle/{id} | Delete a vehicle            |

### Example Request (Create Vehicle)

```json
POST /Vehicle
{
  "make": "Toyota",
  "model": "Camry",
  "year": 2023,
  "price": 25999.00,
  "vin": "1HGCM82633A123456",
  "mileage": 0,
  "color": "Silver",
  "fuelType": "Gasoline",
  "transmission": "Automatic"
}
```

## Development

### Project Structure

- `Controllers/`: API endpoints
- `Models/`: Data models
- `Data/`: Repository pattern implementation and database context
- `Properties/`: Launch settings

### Adding Migrations

If using the database mode:

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Testing

Execute tests using:

```bash
dotnet test
```

## License

[MIT](LICENSE)

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request
