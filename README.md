# Blazor 2FA with SMS using Vonage
## Requirements
- .NET 6 or later
- A Vonage account and API key
- SQL Express

## Update Configuration
To securely store your configuration details, it's recommended to use a `secrets.json` file.
```json
"Vonage": {
    "ApiKey": "your-vonage-api-key",
    "ApiSecret": "your-vonage-api-secret",
    "SenderName": "name-representing-your-organization"
  }
```
