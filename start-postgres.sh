#!/bin/bash

echo "🔍 Checking PostgreSQL status..."

# Check if PostgreSQL is running
if pg_isready -h localhost -p 5432 >/dev/null 2>&1; then
    echo "✅ PostgreSQL is already running!"
    exit 0
fi

echo "⚠️ PostgreSQL is not running. Attempting to start..."

# Check if PostgreSQL is installed
if ! command -v psql &> /dev/null; then
    echo "📦 PostgreSQL not installed. Installing..."
    
    # Update package list
    sudo apt-get update
    
    # Install PostgreSQL
    sudo apt-get install -y postgresql postgresql-contrib
    
    echo "✅ PostgreSQL installed successfully!"
fi

# Start PostgreSQL service
echo "🚀 Starting PostgreSQL service..."
sudo service postgresql start

# Wait for PostgreSQL to start
sleep 2

# Check if service started successfully
if pg_isready -h localhost -p 5432 >/dev/null 2>&1; then
    echo "✅ PostgreSQL started successfully!"
    
    # Set up database and user if needed
    echo "🔧 Setting up database..."
    
    # Switch to postgres user and create database/user
    sudo -u postgres psql <<EOF
-- Create user if not exists
DO
\$\$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'postgres') THEN
      CREATE ROLE postgres WITH LOGIN PASSWORD 'mysecretpassword';
   END IF;
END
\$\$;

-- Grant permissions
ALTER USER postgres WITH SUPERUSER;
ALTER USER postgres PASSWORD 'mysecretpassword';

-- Create database if not exists
SELECT 'CREATE DATABASE vhouse_dev'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'vhouse_dev')\gexec

-- Grant permissions on database
GRANT ALL PRIVILEGES ON DATABASE vhouse_dev TO postgres;
EOF
    
    echo "✅ Database setup complete!"
else
    echo "❌ Failed to start PostgreSQL. Please check the logs:"
    echo "sudo journalctl -xe | grep postgresql"
    exit 1
fi

echo "🎉 PostgreSQL is ready! Connection details:"
echo "   Host: localhost"
echo "   Port: 5432"
echo "   Database: vhouse_dev"
echo "   User: postgres"
echo "   Password: mysecretpassword"