# Railway Deployment - Adım Adım Rehber

## Adım 1: GitHub'a Push Et

```bash
cd AwqatSalah
git add .
git commit -m "Railway deployment için hazırlandı"
git push origin main
```

## Adım 2: Railway'e Kayıt Ol

1. https://railway.app → **Sign up with GitHub**
2. GitHub hesabını bağla

## Adım 3: Yeni Proje Oluştur

1. Railway Dashboard → **New Project**
2. **Deploy from GitHub repo** seç
3. Repository'yi seç: `cuma-mesajlari-namaz-api` (veya hangi repo ise)
4. **Deploy Now**

## Adım 4: Environment Variables Ekle

1. Railway Dashboard → Projen → **Variables** tab
2. **New Variable** ile şunları ekle:

```
AwqatSalahSettings__ApiUri = https://awqatsalah.diyanet.gov.tr/
AwqatSalahSettings__TokenLifetimeMinutes = 45
AwqatSalahSettings__RefreshTokenLifetimeMinutes = 15
AwqatSalahSettings__UserName = cumamesaj@gmail.com
AwqatSalahSettings__Password = eW7%!wH0
```

**Önemli:** 
- `__` (double underscore) kullan (nested config için)
- Değerlerde özel karakterler varsa tırnak içine alma, direkt yaz

3. **Add** → Deploy otomatik yeniden başlar

## Adım 5: URL'i Al

1. Railway Dashboard → Projen → **Settings**
2. **Generate Domain** → Domain oluştur
3. URL: `https://your-app-name.up.railway.app`

## Adım 6: Test Et

Tarayıcıda aç:
```
https://your-app-name.up.railway.app/api/AwqatSalah/Daily/9541
```

Başarılı ise JSON response görmelisin.

## Adım 7: Firebase'e Environment Variable Ekle

1. Firebase Console → **App Hosting** → **Backend settings**
2. **Environment variables** → **Add variable**
3. **Name**: `DIYANET_PROXY_API_URL`
4. **Value**: Railway URL'in (örn: `https://your-app-name.up.railway.app`)
5. **Save**

## Adım 8: Firebase'i Yeniden Deploy Et

```bash
cd project
firebase deploy --only apphosting
```

## Troubleshooting

### Build Hatası
- Railway Dashboard → **Deployments** → **View Logs**
- Hata mesajını kontrol et

### 500 Error
- Railway Dashboard → **Deployments** → **View Logs**
- Runtime hatalarını kontrol et

### CORS Hatası
- `Program.cs`'de CORS ayarları zaten var, sorun olmamalı

### Port Hatası
- Railway otomatik PORT environment variable'ını set eder
- Kod zaten PORT'u kullanıyor

