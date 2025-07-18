# Railway Deployment Guide for AutoFiCore

This guide provides step-by-step instructions for deploying the AutoFiCore .NET 9.0 Web API to Railway.

## Prerequisites

- Railway account ([Sign up here](https://railway.app))
- Railway CLI installed (optional but recommended)
- Git repository with your code

## 🚀 Quick Deployment Steps

### 1. Connect Your Repository

1. Go to [Railway Dashboard](https://railway.app/dashboard)
2. Click "New Project"
3. Select "Deploy from GitHub repo"
4. Choose your AutoFiCore repository
5. Railway will automatically detect the Dockerfile and start building

### 2. Set Environment Variables

Railway will automatically create a PostgreSQL database. You need to set the following environment variables:

#### Required Environment Variables:

```bash
# Database Connection (Railway will provide DATABASE_URL automatically)
DATABASE_URL=your_postgresql_connection_string

# JWT Configuration
JWT_SECRET=your_super_secure_jwt_secret_key_here

# ASP.NET Core Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:$PORT
```

#### Setting Environment Variables in Railway Dashboard:

1. Go to your project dashboard
2. Click on your service
3. Navigate to "Variables" tab
4. Add the following variables:
   - `JWT_SECRET`: A secure random string (at least 32 characters)
   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `ASPNETCORE_URLS`: `http://+:$PORT`

### 3. Database Setup

Railway automatically provides a PostgreSQL database. The `DATABASE_URL` environment variable is automatically set.

The application will run Entity Framework migrations automatically on startup via the `DbInitializer.InitializeAsync()` method.

## 🛠️ Configuration Details

### Dockerfile Configuration

The project uses a multi-stage Dockerfile with:
- **Build stage**: Uses `mcr.microsoft.com/dotnet/sdk:9.0`
- **Runtime stage**: Uses `mcr.microsoft.com/dotnet/aspnet:9.0`
- **Security**: Runs as non-root user (`dotnetuser`)
- **Health checks**: Configured on `/health` endpoint
- **Port**: Exposes port 8080 (Railway will map this to the public port)

### Railway Configuration

The `railway.toml` file contains:
- Docker build configuration
- Health check settings (`/health` endpoint)
- Environment-specific variables
- Build watch patterns for efficient rebuilds

### Environment Variable Overrides

The application is configured to use environment variables when available:
- `DATABASE_URL` overrides the connection string in `appsettings.json`
- `JWT_SECRET` overrides the JWT secret in `appsettings.json`

## 🔧 Railway CLI Deployment (Alternative)

If you prefer using the CLI:

```bash
# Install Railway CLI
npm install -g @railway/cli

# Login to Railway
railway login

# Initialize project
railway init

# Link to existing project (optional)
railway link [project-id]

# Deploy
railway up
```

## 🏥 Health Checks

The application includes health checks:
- **Endpoint**: `/health`
- **Database check**: Verifies PostgreSQL connection
- **Vehicle service check**: Custom health check for vehicle service

Railway will automatically use these health checks to ensure your application is running properly.

## 📊 Monitoring and Logging

Railway provides built-in monitoring:
- **Logs**: Available in the Railway dashboard
- **Metrics**: CPU, memory, and network usage
- **Deployments**: Track deployment history and rollbacks

## 🚨 Troubleshooting

### Common Issues and Solutions:

#### 1. Database Connection Issues
```bash
# Check if DATABASE_URL is set correctly
railway variables

# Verify database is running
railway status
```

#### 2. JWT Secret Missing
```bash
# Set JWT_SECRET environment variable
railway variables set JWT_SECRET=your_secure_secret_key_here
```

#### 3. Application Won't Start
```bash
# Check application logs
railway logs

# Verify all required environment variables are set
railway variables
```

#### 4. Build Failures
- Ensure all NuGet packages are compatible with .NET 9.0
- Check that the Dockerfile is in the project root
- Verify `.dockerignore` is properly configured

#### 5. Database Migrations
If migrations fail:
```bash
# Connect to your database directly
railway connect postgres

# Or run migrations manually if needed
railway run dotnet ef database update
```

## 🔄 Continuous Deployment

Railway automatically deploys when you push to your main branch. To customize this:

1. Go to your service settings
2. Navigate to "Build & Deploy"
3. Configure branch and build settings

## 📈 Performance Optimization

### Database Performance:
- The application includes database indexes for common queries
- Connection pooling is configured with retry policies
- Health checks monitor database connectivity

### Application Performance:
- Multi-stage Docker build reduces image size
- Non-root user for security
- Proper logging configuration for production
- Memory caching enabled for frequently accessed data

## 🔐 Security Best Practices

1. **Environment Variables**: All secrets are stored as environment variables
2. **Non-root User**: Docker container runs as non-root user
3. **HTTPS**: Configure your custom domain with HTTPS in Railway
4. **JWT**: Use strong, randomly generated JWT secrets
5. **CORS**: Configure appropriate CORS origins for your frontend

## 📝 Environment Variable Examples

```bash
# Production Environment Variables
JWT_SECRET=ThisIsAVerySecureJwtSecretKeyThatIsAtLeast32CharactersLong!
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:$PORT

# Optional: Custom CORS origins
CORS_ORIGINS=https://yourdomain.com,https://www.yourdomain.com
```

## 🌐 Custom Domain Setup

1. Go to your service settings in Railway
2. Navigate to "Domains"
3. Add your custom domain
4. Configure DNS records as instructed
5. Railway will automatically provide SSL certificates

## 📞 Support

- **Railway Documentation**: [docs.railway.app](https://docs.railway.app)
- **Railway Discord**: [discord.gg/railway](https://discord.gg/railway)
- **Railway Status**: [status.railway.app](https://status.railway.app)

## 🎯 Next Steps

After successful deployment:
1. Test all API endpoints
2. Verify database connectivity
3. Check health endpoints
4. Monitor application logs
5. Set up custom domain (if needed)
6. Configure CI/CD workflows (if needed)

---

**Happy Deploying! 🚀** 