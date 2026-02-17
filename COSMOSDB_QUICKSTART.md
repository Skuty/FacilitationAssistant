# CosmosDB Setup - Quick Start

## 🎯 How It Works

**Local Development** → InMemory database (default, no setup needed)  
**Azure Production** → CosmosDB (auto-detected when credentials exist)

The app automatically detects which database to use:
- If CosmosDB credentials are configured → Uses CosmosDB
- Otherwise → Uses InMemory database

**No PostgreSQL, no migrations, no complexity!**

## 📝 Quick Setup for Azure Production

### 1. Get CosmosDB Credentials
```
Azure Portal → Your CosmosDB → Keys
- Copy: URI (endpoint) 
- Copy: Primary Key
```

### 2. Configure Azure Web App
```
Azure Portal → FacilitationAssistant Web App → Configuration → Application settings

Add these settings:
Name: Database__ConnectionStrings__CosmosDB__AccountEndpoint
Value: https://your-cosmos-account.documents.azure.com:443/

Name: Database__ConnectionStrings__CosmosDB__AccountKey  
Value: your-primary-key-here

Name: Database__ConnectionStrings__CosmosDB__DatabaseName
Value: FacilitationAssistant
```

Click **Save** and restart the app.

### 3. Deploy
Push to `net-poc` branch → GitHub Actions deploys automatically

---

## 💡 Testing Locally with CosmosDB (Optional)

If you want to test CosmosDB locally:

```powershell
cd src/FacilitationAssistant.Web
dotnet user-secrets set "Database:ConnectionStrings:CosmosDB:AccountEndpoint" "YOUR_ENDPOINT"
dotnet user-secrets set "Database:ConnectionStrings:CosmosDB:AccountKey" "YOUR_KEY"
dotnet user-secrets set "Database:ConnectionStrings:CosmosDB:DatabaseName" "FacilitationAssistant"
dotnet run
```

## ✅ Checking Which Database Is Used

When the app starts, check the console output:
- `"Using InMemory Database: FacilitationDb"` → Local development
- `"Using CosmosDB: FacilitationAssistant"` → Production mode

---

## 📋 What Changed

### Files Modified:
- ✅ `FacilitationAssistant.Infrastructure.csproj` - Added CosmosDB NuGet package
- ✅ `appsettings.json` - Defaults to InMemory for local dev
- ✅ `Program.cs` - Auto-detects CosmosDB when credentials present
- ✅ `FacilitationDbContext.cs` - Simplified multi-provider support (no partition keys)
- ✅ `.github/workflows/*.yml` - Kept simple (no secrets needed)

---

## 🔐 Security Best Practices

**Local Development:**
- ✅ Use InMemory database (no credentials needed)
- ✅ If testing CosmosDB locally, use User Secrets (never commit credentials)

**Production:**
- ✅ Configure credentials in Azure Portal → Web App → Configuration
- ✅ Credentials stored securely in Azure (not in code or GitHub)
- ✅ No GitHub Secrets needed

---

## ❓ Do I Need Partition Keys?

**No!** The simplified configuration doesn't use partition keys:
- ✅ Easier to set up and use
- ✅ Works for small to medium applications
- ⚠️ Less optimal for very large datasets (100k+ documents per container)

For most facilitation meetings, this is perfectly fine!

---

## 🛠️ Troubleshooting

**Using InMemory when I want CosmosDB?**
→ Check Azure App Configuration has the three settings
→ Verify the AccountEndpoint and AccountKey are not empty
→ Check console output: should say "Using CosmosDB"

**Database not persisting?**
→ InMemory database is temporary (resets on restart)
→ CosmosDB persists data permanently

**"Unauthorized" error?**
→ Verify AccountKey in Azure Portal → CosmosDB → Keys
→ Make sure you copied the full key (it's very long)

---

## 📖 Full Documentation

See [AZURE_COSMOSDB_SETUP.md](AZURE_COSMOSDB_SETUP.md) if you need:
- Advanced configuration options
- GitHub Secrets setup alternative
- Detailed security guidance

