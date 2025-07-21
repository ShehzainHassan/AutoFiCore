#!/bin/bash

# Test script to run AutoFiCore locally with Railway-like environment
echo "🚀 Testing AutoFiCore locally with Railway configuration..."

# Set environment variables (using actual Railway database)
export DATABASE_URL="Host=mainline.proxy.rlwy.net;Port=55614;Database=railway;Username=postgres;Password=rkxMVIqKFnPiqOABfRecQVsvAVIxSLet"
export JWT_SECRET="AutoFiTestSecretKeyThatIsAtLeast32CharactersLong123!"
export ASPNETCORE_ENVIRONMENT="Production"
export PORT="8080"
export ASPNETCORE_URLS="http://+:8080"
export DOTNET_RUNNING_IN_CONTAINER="true"

echo "📋 Environment Variables Set:"
echo "DATABASE_URL: $DATABASE_URL"
echo "JWT_SECRET: [HIDDEN]"
echo "ASPNETCORE_ENVIRONMENT: $ASPNETCORE_ENVIRONMENT"
echo "PORT: $PORT"
echo "ASPNETCORE_URLS: $ASPNETCORE_URLS"
echo ""

# Build the Docker image
echo "🏗️  Building Docker image..."
docker build -t autoficore-test .

if [ $? -ne 0 ]; then
    echo "❌ Docker build failed!"
    exit 1
fi

echo "✅ Docker build successful!"
echo ""

# Run the container with environment variables
echo "🏃 Running container with Railway-like environment..."
docker run -d \
    --name autoficore-test \
    --rm \
    -p 8080:8080 \
    -e DATABASE_URL="$DATABASE_URL" \
    -e JWT_SECRET="$JWT_SECRET" \
    -e ASPNETCORE_ENVIRONMENT="$ASPNETCORE_ENVIRONMENT" \
    -e PORT="$PORT" \
    -e ASPNETCORE_URLS="$ASPNETCORE_URLS" \
    -e DOTNET_RUNNING_IN_CONTAINER="$DOTNET_RUNNING_IN_CONTAINER" \
    autoficore-test

if [ $? -ne 0 ]; then
    echo "❌ Container failed to start!"
    exit 1
fi

echo "✅ Container started successfully!"
echo ""

# Wait for application to start
echo "⏳ Waiting for application to start..."
sleep 10

# Test health endpoints
echo "🏥 Testing health endpoints..."
echo ""

echo "Testing /health endpoint:"
curl -s -o /dev/null -w "HTTP Status: %{http_code}\nResponse Time: %{time_total}s\n" http://localhost:8080/health
echo ""

echo "Testing /health/ready endpoint:"
curl -s -o /dev/null -w "HTTP Status: %{http_code}\nResponse Time: %{time_total}s\n" http://localhost:8080/health/ready
echo ""

# Get actual health response
echo "📊 Health endpoint response:"
curl -s http://localhost:8080/health | jq . 2>/dev/null || curl -s http://localhost:8080/health
echo ""

# Show container logs
echo "📋 Container logs (last 20 lines):"
docker logs --tail 20 autoficore-test
echo ""

# Cleanup
echo "🧹 Cleaning up..."
docker stop autoficore-test

echo "✅ Test completed!"
echo ""
echo "🎯 Next steps:"
echo "1. If health checks passed, the Railway deployment should work"
echo "2. Make sure to set JWT_SECRET and DATABASE_URL in Railway dashboard"
echo "3. Railway will automatically provide DATABASE_URL for PostgreSQL" 