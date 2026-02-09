# Docker Compose Setup for PostgreSQL

This directory contains a Docker Compose configuration for running a PostgreSQL database for the Facilitation Assistant application.

## Quick Start

1. **Start PostgreSQL**:
   ```bash
   docker-compose up -d
   ```

2. **Apply Database Migrations**:
   ```bash
   cd src/FacilitationAssistant.Web
   dotnet ef database update --project ../FacilitationAssistant.Infrastructure/FacilitationAssistant.Infrastructure.csproj
   ```

3. **Run the Application with PostgreSQL**:
   Update `appsettings.json` to use PostgreSQL:
   ```json
   {
     "Database": {
       "Provider": "PostgreSQL"
     }
   }
   ```
   Then run:
   ```bash
   dotnet run --project src/FacilitationAssistant.Web/FacilitationAssistant.Web.csproj
   ```

## Configuration

The default database configuration is:
- **Host**: localhost
- **Port**: 5432
- **Database**: facilitation_assistant
- **Username**: facilitation_user
- **Password**: facilitation_pass

You can modify these values in `docker-compose.yml` and the corresponding connection string in `appsettings.json`.

## Commands

- **Start database**: `docker-compose up -d`
- **Stop database**: `docker-compose down`
- **Stop and remove data**: `docker-compose down -v`
- **View logs**: `docker-compose logs -f postgres`
- **Access PostgreSQL CLI**:
  ```bash
  docker exec -it facilitation_assistant_db psql -U facilitation_user -d facilitation_assistant
  ```

## Development vs Production

For **development**, you can use the InMemory database by setting:
```json
{
  "Database": {
    "Provider": "InMemory"
  }
}
```

For **production**, use PostgreSQL with environment-specific connection strings.
