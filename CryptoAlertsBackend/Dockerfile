# Use the official ASP.NET runtime for .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Use the .NET SDK to build the application for .NET 8.0
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["CryptoAlertsBackend.csproj", "./"]
RUN dotnet restore

# Copy the entire project and build it
COPY . .
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Use the runtime image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5235

ENV DB_IP=sqlserver
ENV DB_NAME=CryptoAlertsDb
ENV DB_SA_PASSWD=YourStrong!Passw0rd
ENV MINIMUM_PRICE_CHANGE_TO_ALERT_5M=3
ENV MINIMUM_PRICE_CHANGE_TO_ALERT_15M=5
ENV MINIMUM_PRICE_CHANGE_TO_ALERT_30M=6
ENV MINIMUM_PRICE_CHANGE_TO_ALERT_1H=7
ENV MINIMUM_PRICE_CHANGE_TO_ALERT_4H=10
ENV MINIMUM_PRICE_CHANGE_TO_ALERT_8H=15
ENV MINIMUM_PRICE_CHANGE_TO_ALERT_24H=20
ENV MINIMUM_PRICE_CHANGE_TO_ALERT_7D=30
ENV MINIMUM_PRICE_CHANGE_TO_ALERT_30D=40
ENV UPDATE_ENDPOINTS_INTERVAL_IN_SECONDS=300
ENV DISCORD_WEBHOOK_NORMAL_ALERT_URL=https://discord.com/api/webhooks/ID/TOKEN
ENV DISCORD_WEBHOOK_2X_RATIO_URL=https://discord.com/api/webhooks/ID/TOKEN
ENV DISCORD_WEBHOOK_3X_RATIO_URL=https://discord.com/api/webhooks/ID/TOKEN

# Install SQL Server tools (for sqlcmd)
RUN apt-get update \
    && apt-get install -y curl gnupg apt-transport-https \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/debian/10/prod.list > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y msodbcsql17 mssql-tools

# Copy the entrypoint script
COPY entrypoint.sh /app/entrypoint.sh

# Make the script executable
RUN chmod +x /app/entrypoint.sh

# Set the script as the entrypoint
ENTRYPOINT ["/app/entrypoint.sh"]
