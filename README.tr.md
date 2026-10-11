# ScreenQuarters

[English](README.md) | [Türkçe](README.tr.md)

Pencereleri her monitör için seçilen bölgelere yerleştiren ve desteklenen tarayıcılardaki tam ekran içeriği seçilen bölgede tutmayı amaçlayan hafif bir Windows sistem tepsisi aracı.

**GallabiGaminG** tarafından, yapay zekâ desteğiyle geliştirildi. Amaç günlük masaüstü kullanımını rahatlatmak: bir bölgede kod yazarken diğerinde video izlemek, gerektiğinde yeniden ekranın tamamını kullanmak.

## Özellikler

- Her bağlı monitöre ayrı düzen seçimi.
- Beş preset: 2×2, 3×3, iki sütun, üç sütun ve sol geniş + sağ üst/alt.
- Settings içinde numaralı düzen önizlemesi.
- Ctrl+Alt+1–9 ile aktif pencereyi bölgeye yerleştirme.
- İsteğe bağlı sürükleyip bırakınca otomatik yerleştirme.
- Deneysel tarayıcı F11 / video tam ekranını bölgede tutma.
- Tek pencereyi bırakma veya film ve oyun için bölge yönetimini kapatma.
- Yerel ayarlar, işlem bazlı hariç tutma ve Settings / About / Exit tepsi menüsü.

Uygulama arayüzü şu anda Türkçe. İngilizce arayüz henüz eklenmedi.

## Hemen çalıştır

Windows 10/11, **.NET masaüstü geliştirme** iş yüküyle Visual Studio 2022 17.8 veya sonrası ve .NET 8 SDK gerekir.

1. Repoyu klonla veya indir; `ScreenQuarters.sln` dosyasını aç.
2. **F5** bas. Sistem tepsisinde ScreenQuarters ikonunu bul; gizli simgelerin altında olabilir.
3. İkona sağ tık → **Settings**. Monitörü seç, düzeni seç ve **Kaydet** bas.
4. O monitördeki uygulama penceresine tıkla; **Ctrl+Alt+1–9** ile hücreye yerleştir.
5. Tarayıcı için önce bölgeyi ata; sonra F11 veya videonun tam ekran düğmesini dene.

Düzen seçmek masaüstünde sürekli çizgiler oluşturmaz veya boş hücreleri doldurmaz. Kullanılmayan alanlarda wallpaper görünmeye devam eder.

## Kısayollar

| Kısayol | İşlev |
|---|---|
| Ctrl+Alt+1–9 | Aktif pencereyi bulunduğu monitörün ilgili bölgesine yerleştir |
| Ctrl+Alt+0 | Aktif pencereyi bölge yönetiminden çıkar |
| Ctrl+Alt+F12 | Çalışma / tek ekran modu arasında geçiş yap |

Üst sıradaki rakam tuşlarını kullan; numpad değil. Yalnızca seçili düzende bulunan numaralar geçerlidir; olmayan bölge için bildirim gösterilir. Kısayol çakışmaları açılışta bildirilir. Kısayol düzenleme henüz yok.

Grid numaraları soldan sağa, yukarıdan aşağıya ilerler. Sol geniş düzende **1 = sol**, **2 = sağ üst**, **3 = sağ alt**. Settings önizlemesi numaraları gösterir.

Tek ekran modu yönetilen pencereleri bırakır. Uygulamanın kendi kontrolüyle tam ekranı aç. Çalışma moduna dönünce pencereleri yeniden bölgelere ata.

## Ayarlar ve hariç tutma

Otomatik yerleştirme için **Sürükleyip bırakınca otomatik bölgeye yerleştir** seçeneğini aç. Atanmış pencereler de başka bir bölgeye sürüklenebilir.

Hariç tutma alanına virgülle ayrılmış, **`.exe` içermeyen işlem adları** yazılır. Örneğin `explorer`, Dosya Gezgini'ni hariç tutar; Explorer pencerelerini yerleştirmek istiyorsan listeden çıkar. Kural pencerenin sahibi olan işleme uygulanır; bir uygulamanın tüm bileşenlerine uygulanmaz. Yardımcı işlemler ayrıca listelenebilir.

Ayarlar UTF-8 JSON olarak burada saklanır:

```text
%LOCALAPPDATA%\ScreenQuarters\settings.json
```

Kaynak koddaki varsayılanları değiştirmek mevcut kaydedilmiş ayarları değiştirmez.

## Tam ekran desteği ve sınırlar

Bu, .NET 8, Windows Forms ve Win32 pencere konumlandırma kullanan erken bir prototiptir.

- Tarayıcı tam ekran desteği deneyseldir. Chrome, Edge, Firefox, Brave, Opera ve Vivaldi işlem adları tanınır. Davranış tarayıcıya ve oynatıcıya bağlıdır; ayrı tam ekran penceresi açılırsa otomatik yönetilmez.
- Tam ekrana geçişte kısa bir boyut değişimi görülebilir. Pencere bölge boyutunu sürekli reddederse araç yönetimi bırakır ve bildirim gösterir.
- Sanal monitör oluşturmaz, ekran sürücüsünü değiştirmez, DirectX/Vulkan tespiti yapmaz ve exclusive fullscreen oyunları bölgede tutmaz. Oyun öncesi bölge yönetimini kapat.
- NVIDIA Surround, Windows'un sunduğu tek ekran yüzeyi olarak ele alınır. Fiziksel panel sınırları kullanılmaz. Surround davranışı ayrıca doğrulanmadı.
- Bölgeler görev çubuğu hariç monitör çalışma alanını kullanır. Uygulamanın minimum pencere boyutu tam yerleşmeyi engelleyebilir.
- Yönetici yetkisiyle çalışan uygulamalar normal yetkili araçtan gelen konumlandırmayı reddedebilir. Araç yönetici yetkisi istemez.
- Windows Snap, sürükleme davranışıyla etkileşebilir. Araç Windows Snap ayarlarını değiştirmez.
- Ekran yapılandırması değişince ekran kimlikleri değişebilir; yeniden bağlantı sonrasında kayıtlı düzenleri kontrol et.
- Arka plan servisi, telemetri ve otomatik başlangıç kaydı yok. Windows ile başlatmak için istersen yayımlanmış EXE'nin kısayolunu `shell:startup` içine ekle.

## Bağımsız EXE üretme

.NET 8 SDK kurulu bilgisayarda, repo kökünden:

```powershell
.\Publish.ps1
```

Çıktı: `publish\ScreenQuarters.exe` — .NET runtime kurulumu gerektirmeyen Windows x64 sürümü. Yayınlama gerekli SDK paketlerini indirebilir.

## Doğrulama

Proje sahibi tarafından Windows'ta derlenip elle denendi: bölge kısayolları, çeyrek ekranda tarayıcı/video tam ekranı, monitör başına ayar kaydı ve iki monitörde 3×3 yerleşim. Numaralı düzen önizlemesi de gösterildi.

Tüm tarayıcılar, uygulamalar, DPI kombinasyonları, monitör yeniden bağlama senaryoları ve exclusive fullscreen oyunlar için uyumluluk doğrulanmış değildir.

## Planlanan geliştirmeler

- Bölge sınırlarını fareyle değiştirme ve özel düzen kaydetme.
- Bilgi alınabildiğinde anlaşılır monitör model adları.
- Pencere seçerek hariç tutma kuralı oluşturma.
- Hariç tutulan uygulamalar için açık geri bildirim ve güncel About/yardım metinleri.

Bunlar planlanan özelliklerdir; henüz mevcut değildir.

## Lisans

MIT. Bkz. [LICENSE](LICENSE).
