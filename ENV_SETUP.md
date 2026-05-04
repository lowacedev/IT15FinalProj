# Environment Variables Setup

This project uses a `.env` file to manage sensitive configuration and environment-specific settings.

## Setup Instructions

### 1. Copy the Example File
```bash
cp .env.example .env
```

### 2. Configure Your Environment
Edit the `.env` file and update the values according to your environment:

```env
# Database Configuration
DB_SERVER=127.0.0.1      # Your MySQL server host
DB_PORT=3306             # MySQL port (default: 3306)
DB_NAME=itsms            # Database name
DB_USER=root             # MySQL username
DB_PASSWORD=             # MySQL password

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7072;http://localhost:5072

# Logging
LOG_LEVEL=Information
```

### 3. Important Security Notes

⚠️ **NEVER commit `.env` to version control!**

- The `.env` file is added to `.gitignore` and will NOT be committed
- The `.env.example` file serves as a template for your team
- Keep all passwords and secrets ONLY in `.env`
- Each developer should have their own `.env` file

### 4. Running the Application

The application will automatically load environment variables from `.env` when it starts:

```bash
dotnet run
```

## Environment Variables Reference

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| DB_SERVER | No | 127.0.0.1 | MySQL server hostname or IP |
| DB_PORT | No | 3306 | MySQL server port |
| DB_NAME | No | itsms | Database name |
| DB_USER | No | root | Database username |
| DB_PASSWORD | No | (empty) | Database password |
| ASPNETCORE_ENVIRONMENT | No | Development | Environment (Development/Production) |
| ASPNETCORE_URLS | No | - | HTTPS and HTTP URLs |
| LOG_LEVEL | No | Information | Logging level |

## For Production Deployment

When deploying to production:

1. Set environment variables directly on your hosting platform (e.g., Azure App Service, AWS, Heroku)
2. Do NOT use `.env` files in production
3. Use your hosting provider's secrets management system
4. The application will fall back to default values if environment variables are not set

## Troubleshooting

**"Table 'itsms.roles' doesn't exist"**
- Make sure your `.env` has the correct database credentials
- Ensure MySQL server is running and accessible
- Verify the database exists or let the application create it

**Connection string not working**
- Double-check spacing in your `.env` file (no spaces around `=`)
- Verify MySQL user has appropriate permissions
- Check that the database port is accessible
