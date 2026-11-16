# EduGame

EduGame is a .NET 9 web application with a PostgreSQL database and a static frontend.

## Run with Docker Compose

Requirements: Docker Desktop with Docker Compose.

The Compose setup starts both the application and its own PostgreSQL 18 database. PostgreSQL data is persisted in the `postgres_data` volume. The database is exposed to the Mac on port `5433` (the existing PostgreSQL container on port `5432` is left untouched); the application connects to it internally.

Set these environment variables before starting the stack:

- `POSTGRES_PASSWORD`: password for the PostgreSQL service.
- `EDUGAME_JWT_PRIVATE_KEY_BASE64`: base64-encoded ECDSA private key matching `Backend/EduGame.API/public.key`.

Copy `.env.example` to `.env` and set both values. Keep the private key and passwords outside Git; the root `.env` file is ignored.

If rotating JWT keys, generate a new P-256 ECDSA pair, replace `Backend/EduGame.API/public.key` with the public key, and set the base64-encoded private PEM as `EDUGAME_JWT_PRIVATE_KEY_BASE64`. Never commit the private PEM or its base64 value.

Start the application:

```sh
docker compose up --build
```

Open `http://localhost:8080`. On startup, the application applies EF Core migrations and seeds the learning paths required by course creation. A new Compose database starts empty; data from an unrelated PostgreSQL container is not imported automatically. `docker compose down` keeps the database volume, while `docker compose down -v` deletes it.

## Run the API locally

Requirements: .NET 9 SDK and PostgreSQL.

Configure `ConnectionStrings__DefaultConnection` and `EDUGAME_JWT_PRIVATE_KEY_BASE64`, then run:

```sh
dotnet run --project Backend/EduGame.API/EduGameProject.csproj
```

Swagger is available in the Development environment at `/swagger`.