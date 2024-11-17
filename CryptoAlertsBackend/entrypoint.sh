#!/bin/bash
set -e

if [ -f /app/.env ]; then
    export $(cat /app/.env | xargs)
fi

# Wait for SQL Server to become available
echo "Waiting for SQL Server to be available..."
until /opt/mssql-tools/bin/sqlcmd -S $DB_IP,1433 -U sa -P $DB_SA_PASSWD -Q "SELECT 1" &>/dev/null; do
  echo "SQL Server is unavailable - waiting..."
  sleep 2
done

# Run database migrations
echo "Running database migrations..."
exec dotnet CryptoAlertsBackend.dll ef database update --no-build

# Start the application
echo "Starting the application..."
exec dotnet CryptoAlertsBackend.dll
