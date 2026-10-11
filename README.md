# ScreenQuarters 0.1.0

4K ekranı veya Windows'a tek ekran olarak görünen NVIDIA Surround yüzeyini dört pencere bölgesine ayıran, sistem tepsisinde çalışan deneysel Windows aracı. .NET 8 / Windows Forms / Win32. Ana pencere yok; Settings, About ve Exit tepsi menüsündedir.

## Hemen çalıştır

1. ZIP'i bir klasöre çıkar.
2. Visual Studio 2022'de `ScreenQuarters.sln` aç. “.NET masaüstü geliştirme” iş yükü ve .NET 8 SDK gerekli; .NET 8 için VS 2022 17.8 veya sonrası.
3. F5'e bas. Görev çubuğunun sağındaki sistem tepsisinde ikonu bul (gizli ikonlar okunun altında olabilir).
4. Tarayıcıya geç, Ctrl+Alt+2 ile sağ üste yerleştir. Ardından F11 veya videonun tam ekran düğmesini dene.
5. Soldaki Visual Studio / Adobe penceresini Ctrl+Alt+1 veya 3 ile yerleştir.
6. Film / oyun öncesi Ctrl+Alt+F12: bölge yönetimi durur ve pencereler bırakılır. Artık uygulamanın kendi tam ekranını kullan. Tekrar F12 ile çalışma moduna geç; bölge atamalarını yeniden yap.

## Kısayollar

| Kısayol | İşlev |
|---|---|
| Ctrl+Alt+1 | Sol üst |
| Ctrl+Alt+2 | Sağ üst |
| Ctrl+Alt+3 | Sol alt |
| Ctrl+Alt+4 | Sağ alt |
| Ctrl+Alt+0 | Aktif pencereyi bölge yönetiminden çıkar |
| Ctrl+Alt+F12 | Çalışma / tek ekran modu |

Kısayollar üst sıradaki rakam tuşlarını kullanır; numpad değil. Çakışan kısayollar açılışta bildirilir. v0.1'de kısayol düzenleme yok.

## Davranış ve sınırlar

- Normal pencereler bölgeye boyutlandırılır; atanmış pencereyi büyütmek yeniden bölgesine yerleştirir.
- Tarayıcı tam ekran desteği deneyseldir: atanan Chrome, Edge, Firefox, Brave, Opera ve Vivaldi pencerelerindeki başlıksız tam ekran boyut değişimini yaklaşık 200 ms aralıkla düzeltir. Tarayıcı sürümüne ve video oynatıcıya bağlıdır. Ayrı bir tam ekran HWND oluşturan oynatıcılar desteklenmez. İlk geçişte kısa bir ekran büyümesi görülebilir.
- Pencere 2,5 saniyede dört defadan fazla yeniden düzeltme gerektirirse yönetim bırakılır ve bildirim gösterilir; tarayıcı ile sürekli mücadele etmez.
- Bu bir gerçek monitör / sanal ekran sürücüsü değildir. Uygulamalar ve Windows hâlâ fiziksel / Surround ekranı görür. Exclusive fullscreen oyunları bölgeye kilitlemez. DirectX / Vulkan tespiti yapmaz, süreç içine kod enjekte etmez.
- Varsayılan otomatik sürükleme kapalıdır. Settings'ten açarsan uygun boyutlandırılabilir pencereler bırakıldıkları noktadaki çeyreğe yerleşir. Oyunlarının işlem adlarını hariç tutma listesine ekle veya oyun öncesi tek ekran moduna geç.
- Atanmış bir pencereyi başka bölgeye sürüklemek atamayı günceller. Pencereyi serbest kullanmak için Ctrl+Alt+0.
- Windows'un yerleşik Snap özelliği aynı anda etkin olabilir; sonuç çakışırsa Windows > Sistem > Çoklu görev > Pencereleri tuttur ayarını incele. Araç bu ayarı değiştirmez.
- Bölgeler ekranın görev çubuğu hariç çalışma alanına göre hesaplanır. 3840×2160 ekran, görev çubuğu gizliyse 1920×1080 dört bölge verir; görev çubuğu görünürse yükseklik biraz azalır. Surround tek yüzey olarak dörde bölünür; fiziksel panel sınırları esas alınmaz.
- Karma DPI, negatif ekran koordinatları ve tek sayılı çözünürlükler dikkate alınır. Bazı uygulamaların minimum pencere boyutu bölgeden büyük olabilir.
- Çıkış / bırakma işleminde normal pencerenin ilk konumu geri yüklenir; tarayıcı hâlâ tam ekrandaysa ekranın tümüne genişletilir. F11'in uygulama içi durumunu değiştirmez. Fullscreen kapalı ayarı yalnızca izlemeyi kapatır.
- Yönetici olarak çalışan uygulamalar standart yetkili bu araçtan gelen boyutlandırmayı reddedebilir. Araç admin talep etmez.
- Ayarlar `%LOCALAPPDATA%\ScreenQuarters\settings.json` içinde UTF-8 JSON olarak saklanır. Başlangıç kaydı, servis, sürücü, telemetri veya ağ erişimi yok. Windows başlangıcında çalıştırmak istersen yayımlanmış EXE'nin kısayolunu `shell:startup` klasörüne kendin ekleyebilirsin.

## EXE üretme

Visual Studio'da Release derle veya .NET 8 SDK kurulu PowerShell'de:

```powershell
.\Publish.ps1
```

`publish\ScreenQuarters.exe` .NET runtime gerektirmeyen win-x64 tek dosya çıktısı olur. İlk yayınlama SDK'nın gerekli paketleri indirmesini gerektirebilir. Proje kaynak olarak teslim edilmiştir; önceden derlenmiş EXE içermez.

## İlk Windows denemesi

1. Notepad ve Visual Studio'yu farklı bölgelere yerleştir; büyüt düğmesini dene.
2. Edge / Chrome: sağ üst bölge, F11 aç-kapat, video tam ekran aç-kapat. Diğer pencereleri kullanırken videonun bölgede kalmasını kontrol et.
3. Ctrl+Alt+0 ile tarayıcıyı serbest bırak; F11 ve normal pencere geri dönüşünü dene.
4. Ctrl+Alt+F12 ile tek ekran; ardından oyunu kendi tam ekran modunda aç.
5. Sürükleme seçeneğini aç; farklı bölgeler ve fiziksel monitörler arasında dene.
6. %100 / %150 ölçeklendirme, Surround, uyku dönüşü ve monitör çıkarma senaryolarını kontrol et.
7. Tepsi > Exit sonrası pencerelerin serbest kaldığını ve ikinci açılışın tek örnek olduğunu doğrula.

## Doğrulama durumu

Kaynak / proje dosyaları hazırlanmıştır. Üretim ortamında Windows ve .NET SDK bulunmadığından build ve gerçek HWND / F11 / Surround davranışları çalıştırılarak doğrulanmamıştır. Yukarıdaki deneme özellikle tarayıcı uyumluluğu için gereklidir.

## Teknik başvuru

- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-monitorinfo
- https://learn.microsoft.com/en-us/windows/win32/gdi/positioning-objects-on-multiple-display-monitors
- https://learn.microsoft.com/en-us/windows/win32/hidpi/dpi-awareness-context
