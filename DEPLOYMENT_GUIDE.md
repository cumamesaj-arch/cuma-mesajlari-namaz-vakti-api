# C# API Deployment Guide - Azure App Service

## Adım 1: Azure Portal'da App Service Oluştur

1. **Azure Portal'a git**: https://portal.azure.com
2. **"Create a resource"** → **"Web App"** seç
3. **Temel Bilgiler:**
   - **Subscription**: Seç
   - **Resource Group**: Yeni oluştur veya mevcut seç
   - **Name**: `cuma-mesajlari-namaz-api` (veya istediğin isim)
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (LTS)
   - **Operating System**: Linux (daha ucuz) veya Windows
   - **Region**: West Europe veya en yakın bölge

4. **Plan**: 
   - **App Service Plan**: Yeni oluştur
   - **Sku and size**: Basic B1 (en ucuz, ~$13/ay) veya Free F1 (sınırlı)

5. **Review + create** → **Create**

## Adım 2: Configuration (App Settings)

1. Azure Portal'da oluşturduğun App Service'e git
2. **Settings** → **Configuration** → **Application settings**
3. **New application setting** ile şunları ekle:

```
AwqatSalahSettings__ApiUri = https://awqatsalah.diyanet.gov.tr/
AwqatSalahSettings__TokenLifetimeMinutes = 45
AwqatSalahSettings__RefreshTokenLifetimeMinutes = 15
AwqatSalahSettings__UserName = cumamesaj@gmail.com
AwqatSalahSettings__Password = eW7%!wH0
```

**Not:** `__` (double underscore) kullan, çünkü Azure App Settings'te nested config için bu kullanılır.

4. **Save** → **Continue**

## Adım 3: Deployment

### Yöntem A: Visual Studio ile (En Kolay)

1. Visual Studio'da projeyi aç
2. **Solution Explorer** → Projeye sağ tık → **Publish**
3. **Azure** → **Azure App Service (Windows)** veya **Azure App Service (Linux)**
4. Oluşturduğun App Service'i seç
5. **Publish**

### Yöntem B: Azure CLI ile

```bash
# 1. Azure'a login ol
az login

# 2. Projeyi build et
cd AwqatSalah/DiyanetNamazVakti.Api
dotnet publish -c Release -o ./publish

# 3. Zip oluştur
cd publish
zip -r ../api.zip .

# 4. Azure'a deploy et
az webapp deployment source config-zip \
  --resource-group <RESOURCE_GROUP_NAME> \
  --name <APP_SERVICE_NAME> \
  --src ../api.zip
```

### Yöntem C: GitHub Actions (CI/CD)

1. GitHub'a projeyi push et
2. Azure Portal → App Service → **Deployment Center**
3. **GitHub** seç → Repository ve branch seç
4. **Save** → Otomatik deploy başlar

## Adım 4: URL'i Al ve Test Et

1. Azure Portal → App Service → **Overview**
2. **URL**'i kopyala: `https://cuma-mesajlari-namaz-api.azurewebsites.net`
3. Test et: `https://cuma-mesajlari-namaz-api.azurewebsites.net/api/AwqatSalah/Daily/9541`

## Adım 5: Firebase'e Environment Variable Ekle

1. Firebase Console → **App Hosting** → **Backend settings**
2. **Environment variables** → **Add variable**
3. **Name**: `DIYANET_PROXY_API_URL`
4. **Value**: `https://cuma-mesajlari-namaz-api.azurewebsites.net`
5. **Save**

6. Firebase App Hosting'i yeniden deploy et:
```bash
cd project
firebase deploy --only apphosting
```

## Alternatif: Railway (Daha Kolay, Ücretsiz Başlangıç)

1. https://railway.app → **Sign up**
2. **New Project** → **Deploy from GitHub repo**
3. Repository'yi seç
4. **Settings** → **Variables** ekle:
   - `AwqatSalahSettings__ApiUri = https://awqatsalah.diyanet.gov.tr/`
   - `AwqatSalahSettings__UserName = cumamesaj@gmail.com`
   - `AwqatSalahSettings__Password = eW7%!wH0`
5. Otomatik deploy başlar
6. URL: `https://your-app.railway.app`

## Alternatif: Heroku (Ücretsiz Plan Yok Artık)

1. https://heroku.com → **Sign up**
2. Heroku CLI yükle
3. `heroku create cuma-mesajlari-namaz-api`
4. `git push heroku main`
5. Config vars ekle:
   ```bash
   heroku config:set AwqatSalahSettings__ApiUri=https://awqatsalah.diyanet.gov.tr/
   heroku config:set AwqatSalahSettings__UserName=cumamesaj@gmail.com
   heroku config:set AwqatSalahSettings__Password=eW7%!wH0
   ```

## Troubleshooting

- **500 Error**: Logs kontrol et: Azure Portal → App Service → **Log stream**
- **CORS Error**: `Program.cs`'de CORS ayarları kontrol et
- **Connection Timeout**: App Service plan'ını yükselt (Basic → Standard)

