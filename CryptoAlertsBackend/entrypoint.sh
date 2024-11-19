#!/bin/bash
set -e

# Start the application
echo "Starting the application..."
exec dotnet CryptoAlertsBackend.dll
