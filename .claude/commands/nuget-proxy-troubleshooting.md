# .NET NuGet Proxy Solution for Claude Code Environment

## Problem Overview

In the Claude Code environment, NuGet package restoration fails with authentication errors because:
- Claude Code uses a JWT-authenticated HTTP proxy at `21.0.0.179:15004`
- NuGet doesn't support the authentication method required by this proxy
- This results in 401 errors when trying to restore packages from nuget.org

## Solution: Custom Proxy Bridge

This project includes a custom proxy solution that bridges NuGet to the authenticated Claude Code proxy.

### Components

1. **nuget-proxy.py** - Python HTTP/HTTPS proxy that:
   - Listens on localhost:8888 (unauthenticated)
   - Forwards requests to Claude Code proxy with JWT authentication
   - Handles HTTPS CONNECT tunneling for secure connections

2. **dotnet-with-proxy.sh** - Wrapper script that:
   - Automatically detects/starts the proxy if needed
   - Sets HTTP_PROXY/HTTPS_PROXY environment variables
   - Runs dotnet commands seamlessly

3. **NuGet.config** - Configured to use the local proxy on port 8888

4. **NUGET-PROXY-README.md** - Detailed documentation

## Quick Start

### For New Sessions

When starting a new Claude Code session:

1. **Check if proxy files exist:**
   ```bash
   ls -la nuget-proxy.py dotnet-with-proxy.sh NuGet.config
   ```

2. **If files exist, use the wrapper script:**
   ```bash
   ./dotnet-with-proxy.sh restore
   ./dotnet-with-proxy.sh build
   ./dotnet-with-proxy.sh run
   ```

3. **The wrapper automatically:**
   - Detects if proxy is running
   - Starts proxy if needed
   - Keeps proxy running between commands
   - Sets proper environment variables

### Manual Proxy Control

**Start proxy manually:**
```bash
python3 nuget-proxy.py
```

**Check if running:**
```bash
ps aux | grep nuget-proxy
cat /tmp/nuget-proxy.pid && ps -p $(cat /tmp/nuget-proxy.pid)
```

**Stop proxy:**
```bash
kill $(cat /tmp/nuget-proxy.pid)
```

## How It Works

### Authentication Flow

```
NuGet -> localhost:8888 (nuget-proxy.py) -> 21.0.0.179:15004 (Claude Code proxy) -> nuget.org
         [no auth required]                 [JWT auth added]                       [internet]
```

### Environment Variables

The proxy solution relies on these environment variables (automatically set by Claude Code):
- `HTTP_PROXY` - Used by the Python proxy to connect upstream
- `HTTPS_PROXY` - Same as HTTP_PROXY
- `PROXY_AUTHORIZATION` - JWT token for authentication

## Troubleshooting

### Issue: "Connection refused" on port 8888
**Cause:** Proxy is not running
**Solution:**
```bash
python3 nuget-proxy.py &
./dotnet-with-proxy.sh restore
```

### Issue: "Address already in use"
**Cause:** Proxy is already running or port is taken
**Solution:**
```bash
lsof -i :8888
ps aux | grep nuget-proxy
kill $(lsof -t -i:8888)
```

### Issue: 401 Unauthorized from upstream proxy
**Cause:** Claude Code proxy environment variables missing or expired
**Solution:**
```bash
echo $PROXY_AUTHORIZATION
curl -I https://api.nuget.org/v3/index.json
```

### Issue: Proxy starts but restore still fails
**Cause:** NuGet not using proxy configuration
**Solution:**
```bash
cat NuGet.config
dotnet restore --configfile NuGet.config
./dotnet-with-proxy.sh restore
```

### Issue: SSL/TLS certificate errors
**Cause:** Python proxy not properly handling HTTPS CONNECT
**Solution:**
```bash
kill $(cat /tmp/nuget-proxy.pid)
python3 nuget-proxy.py &
./dotnet-with-proxy.sh restore
```
