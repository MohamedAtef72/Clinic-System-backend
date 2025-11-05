#!/bin/bash
set -e

SQL_HOST="${SQL_HOST:-sql}"
SQL_PORT="${SQL_PORT:-1433}"
MAX_ATTEMPTS=30
SLEEP_SECONDS=2

echo "Entry point started. Waiting for SQL Server at ${SQL_HOST}:${SQL_PORT} ..."

attempt=1
while ! (echo > /dev/tcp/${SQL_HOST}/${SQL_PORT}) >/dev/null 2>&1; do
  if [ $attempt -ge $MAX_ATTEMPTS ]; then
    echo "Timeout waiting for SQL Server at ${SQL_HOST}:${SQL_PORT} after ${attempt} attempts."
    exit 1
  fi
  echo "Waiting for SQL (${attempt}/${MAX_ATTEMPTS})..."
  attempt=$((attempt+1))
  sleep ${SLEEP_SECONDS}
done

echo "SQL Server port is open. Proceeding..."

export PATH="$PATH:/root/.dotnet/tools"

echo "Applying migrations to database..."
dotnet ef database update --project Clinic-System.Infrastructure --startup-project Clinic-System.API

echo "Starting the API..."
dotnet Clinic-System.API.dll
