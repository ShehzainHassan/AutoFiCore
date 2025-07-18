# CORS Configuration Guide

## Overview

This application supports flexible CORS (Cross-Origin Resource Sharing) configuration for both development and production environments. The configuration system allows you to specify allowed origins through multiple methods while maintaining security best practices.

## Configuration Methods

### 1. Development Configuration (`appsettings.json`)

The default development configuration includes common development server ports:

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:3001",
      "http://localhost:5173",
      "http://localhost:5174",
      "http://localhost:4200",
      "http://127.0.0.1:3000",
      "http://127.0.0.1:3001",
      "http://127.0.0.1:5173",
      "http://127.0.0.1:5174",
      "http://127.0.0.1:4200"
    ]
  }
}
```

**Supported Development Servers:**
- Port 3000: React development server
- Port 3001: Alternative React port
- Port 5173: Vite development server
- Port 5174: Alternative Vite port
- Port 4200: Angular development server

### 2. Production Configuration (`appsettings.Production.json`)

For production deployments, create an `appsettings.Production.json` file with your custom domains:

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "https://yourdomain.com",
      "https://www.yourdomain.com",
      "https://app.yourdomain.com",
      "https://admin.yourdomain.com",
      "https://your-frontend-domain.vercel.app",
      "https://your-frontend-domain.netlify.app",
      "https://your-custom-domain.com"
    ]
  }
}
```

### 3. Environment Variable Configuration

For dynamic configuration (especially useful for CI/CD and multiple environments), use the `CORS_ALLOWED_ORIGINS` environment variable:

```bash
# Single domain
export CORS_ALLOWED_ORIGINS="https://yourdomain.com"

# Multiple domains (comma-separated)
export CORS_ALLOWED_ORIGINS="https://yourdomain.com,https://app.example.com,https://admin.example.com"
```

## Deployment Examples

### Railway Deployment

```bash
# Set environment variable in Railway
CORS_ALLOWED_ORIGINS="https://yourdomain.com,https://app.yourdomain.com"
```

### Docker Deployment

```dockerfile
# In your Dockerfile or docker-compose.yml
ENV CORS_ALLOWED_ORIGINS="https://yourdomain.com,https://app.yourdomain.com"
```

### Azure App Service

```json
{
  "CORS_ALLOWED_ORIGINS": "https://yourdomain.com,https://app.yourdomain.com"
}
```

### AWS Lambda/ECS

```yaml
# In your serverless.yml or task definition
Environment:
  Variables:
    CORS_ALLOWED_ORIGINS: "https://yourdomain.com,https://app.yourdomain.com"
```

## Configuration Priority

The system combines origins from multiple sources in this order:

1. **Base Configuration**: `appsettings.json` or `appsettings.Production.json`
2. **Environment Override**: `CORS_ALLOWED_ORIGINS` environment variable
3. **Deduplication**: Duplicate origins are automatically removed

## Security Considerations

### ✅ Best Practices

- **Use HTTPS in Production**: Always use `https://` for production domains
- **Specific Domains**: List specific domains instead of wildcards
- **Environment Separation**: Use different configurations for dev/staging/prod
- **Regular Review**: Periodically review and update allowed origins

### ❌ Security Anti-patterns

```json
// DON'T DO THIS - Too permissive
{
  "AllowedOrigins": ["*"]  // Allows any origin
}

// DON'T DO THIS - Mixed protocols
{
  "AllowedOrigins": [
    "http://yourdomain.com",   // HTTP in production
    "https://yourdomain.com"   // HTTPS - only this should be used
  ]
}
```

## Testing CORS Configuration

### Development Testing

```bash
# Test development origin
curl -i -X OPTIONS http://localhost:5011/Vehicle \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: GET" \
  -H "Access-Control-Request-Headers: Content-Type"
```

### Production Testing

```bash
# Test production origin
curl -i -X OPTIONS https://your-api-domain.com/Vehicle \
  -H "Origin: https://yourdomain.com" \
  -H "Access-Control-Request-Method: GET" \
  -H "Access-Control-Request-Headers: Content-Type"
```

### Expected Response

```http
HTTP/1.1 204 No Content
Access-Control-Allow-Origin: https://yourdomain.com
Access-Control-Allow-Methods: GET
Access-Control-Allow-Headers: Content-Type
Access-Control-Allow-Credentials: true
Vary: Origin
```

## Common Hosting Platforms

### Vercel

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "https://your-app.vercel.app",
      "https://your-custom-domain.com"
    ]
  }
}
```

### Netlify

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "https://your-app.netlify.app",
      "https://your-custom-domain.com"
    ]
  }
}
```

### GitHub Pages

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "https://yourusername.github.io"
    ]
  }
}
```

## Troubleshooting

### Common Issues

1. **CORS Error in Browser**
   - Check that your frontend domain is in the `AllowedOrigins` list
   - Ensure the protocol (http/https) matches exactly
   - Verify the port number if using non-standard ports

2. **Environment Variable Not Working**
   - Confirm the environment variable is set correctly
   - Restart the application after setting the variable
   - Check for typos in the variable name

3. **Production Domains Not Working**
   - Ensure you're using `https://` not `http://`
   - Check that the domain is spelled correctly
   - Verify the `appsettings.Production.json` file is being loaded

### Debug Mode

To see which origins are loaded, check the console output when the application starts:

```
info: CORS configured with 12 allowed origins
```

## Implementation Details

The CORS configuration uses a custom `CorsSettings` class that:

- Loads base origins from configuration files
- Merges additional origins from environment variables
- Removes duplicates automatically
- Provides logging for debugging

### Code Structure

```csharp
// CorsSettings.cs
public class CorsSettings
{
    public List<string> AllowedOrigins { get; set; } = new();
    
    public List<string> GetAllowedOrigins()
    {
        // Combines config file origins with environment variable origins
        // Removes duplicates and returns final list
    }
}
```

## Migration Guide

### From Hard-coded Origins

If you previously had hard-coded origins in `Program.cs`:

```csharp
// Old approach
.WithOrigins("http://localhost:3000", "https://yourdomain.com")

// New approach - automatic from configuration
.WithOrigins(allowedOrigins.ToArray())
```

### From Environment-only Configuration

If you only used environment variables:

```bash
# Old - only environment
CORS_ALLOWED_ORIGINS="https://yourdomain.com"

# New - config file + environment (recommended)
# appsettings.json for development origins
# Environment variable for production overrides
```

## Support

For issues with CORS configuration:

1. Check the console output for loaded origins
2. Verify your frontend domain matches exactly
3. Test with the provided curl commands
4. Review the security considerations section 