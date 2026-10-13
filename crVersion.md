# crVersion — Cloudora Versiyon Kaydı

`bahadir-claude-versions` dalındaki major versiyonların kaydı. Her versiyon kullanıcı onayıyla oluşturulur, git etiketiyle (`v1`, `v2`, …) işaretlenir ve **yalnızca yerelde** tutulur (GitHub'a push edilmez).

---

## v2 — Temel tasarım, booster/can göstergeleri ve oynanış düzeltmeleri

- **Tarih:** 2026-10-13
- **Dal:** `bahadir-claude-versions` (v1 `adaa0b8` → docs `755cbbc` → docs `d21a94c` → v2)
- **Uygulama:** `com.yigityvz.cloudora` 0.1.0 (version code 1) — kaynak değişmedi. Cihaz testi için ayrı paket adıyla (`com.yigityvz.cloudora.v2`, "Cloudora v2") derlendi; aşağıya bkz.
- **Kapsam kararı (kullanıcı):** "Temel düzeyde tasarım — göze güzel görünsün, her şey yerinde olsun, renkler hafif değişsin." Öneri dokümanından D1, D2, D3 ve D6'nın bir kısmı + test sırasında bulunan hatalar. D4 (World ekranı), D5 (titreşim), ses, zorluk dengesi bu sürümde **yok**.

### 1. Görsel tasarım
1. **Bulut / gökyüzü kontrastı (D1)** — `CloudoraVisuals.cs`, `CloudContainerView.cs`
   - Gökyüzü degradesi: üst `(0.92,0.97,1)` → `(0.56,0.76,0.96)`, alt `(0.96,0.94,1)` → `(0.80,0.85,0.98)`.
   - Bulut gövdesi `(0.93,0.97,1,0.96)` → `(0.99,1,1,1)`; gölge alpha 0.16 → 0.30; yeni ince mavi `Outline` kenarı.
   - Boş slot `(1,1,1,0.08)` → `(0.82,0.88,0.95,0.55)`; Snow rengi `(0.82,0.94,1)` → `(0.72,0.87,1)` (beyaz bulutta kaybolmasın).
   - Seçili bulut beyaza değil sıcak sarıya döner, ölçek 1.04 → 1.06 (bulut beyazlaşınca eski vurgu görünmez olurdu).
2. **Alt butonlar 3+3 eşit ızgara (D2)** — `GameplayOverlay.cs`
   - 300×84 butonlar, x = −336 / 0 / +336; üst sıra Restart / Undo / World (indigo), alt sıra Extra / Shuffle / Menu (camgöbeği); gölge.
   - Üst yazılar yeniden dizildi: seviye −70, ipucu satırı −125, öğretici −180.
3. **Menü ekranları tam opak (D3)** — `MainMenuController.cs`, `WorldScreenController.cs`, `CloudoraVisuals.CreateBackdrop`
   - Main Menu, Settings ve World tam ekran opak gökyüzü arka planı üzerinde; arkadaki oyun görünmez ve dokunulamaz.
   - **Hata düzeltmesi:** World ekranı oyun butonlarının *altında* kalıyordu; açılınca artık en üste gelir.
4. **Pencereler** — "Sky Balanced!" ve "No Moves / Out of Lives" kartları yarı saydam karartma üzerinde. "No Moves" karartması alt butonları örtmez (mesaj o butonları kullanmayı söylüyor).
5. **Yuvarlak köşeler geri alındı** — İlk denemede butonlara/kartlara eklenen yuvarlak köşeler kenar yumuşatma olmadığı için kötü göründü (kullanıcı geri bildirimi) → dikdörtgen. Ana yazarın orijinal yuvarlak öğelerine (Main Menu kart/butonları, bulut slotları) dokunulmadı.

### 2. Booster ve can göstergeleri
1. **Köşe rozetleri** (kullanıcı seçimi; maketle karşılaştırıldı) — Restart / Undo / Extra / Shuffle butonlarının sağ üst köşesinde hap rozet (`CloudoraVisuals.Pill`, kenarı yumuşatılmış sprite).
   - Rozet = eldeki ücretsiz hak (kayıtta tutulur, seviyeler arası taşınır). Hak 0 ve reklam varsa turuncu **AD**; reklam yoksa **0**.
2. **Üstteki "Undo 3 • Extra 1 • Shuffle 1" yazısı kaldırıldı** (rozetlerle çift bilgi olurdu). Yerine yalnızca bir rozet AD durumundayken görünen satır: "**AD** Watch an ad to refill".
3. **Restart can rozeti** — `RestartCost` (Free / Life / Ad / NoLives): bedavaysa rozet yok; can gidecekse kalp + "-1"; can yoksa AD (reklam varsa) veya kalp + "0". Kurallar `RestartLevel` ile birebir aynı (`GetRestartCost`).
4. **Kalp ikonu** — `♥` karakteri Unity'nin yerleşik fontunda yok; Editor'da sistem fontundan çiziliyor ama **Android'de görünmüyordu** (sağ üst can göstergesi v1'den beri kalpsizdi). Kalp artık prosedürel, kenarı yumuşatılmış sprite (`CloudoraVisuals.Heart`): sağ üst can göstergesi ve Restart rozeti. Öğretici ipucundaki aynı sorunlu `☝` kaldırıldı.

### 3. Oynanış kuralları ve hata düzeltmeleri — `PuzzleController.cs`, `LevelGenerator.cs`
1. **Restart/Shuffle sonrası ızgara boyutu (hata, kullanıcı buldu)** — Extra bulutlarla küçülen ızgara, Restart'ta ekstralar silinince küçük kalıyordu. `_boardLayout.Configure` artık `LoadState` sonunda.
2. **Restart ekstra bulutları korur** (kullanıcı kararı) — boş olarak geri eklenir.
3. **Shuffle ekstra bulutları korur ve onlara da dağıtır** (kullanıcı kararı) — `LevelGenerator.Generate(level, seed, extraEmptyClouds)`; ekstralar yeni tahtanın parçası olur. Sayı seviyenin orijinal bulut sayısına (`_baseCloudCount`) göre hesaplanır (ilk denemede art arda iki Shuffle ekstraları sıfırlıyordu — düzeltildi). Restart, Shuffle'ın dağıtımını korur (Shuffle ücretli olacağı için normal kabul edildi).
4. **Extra Cloud seviye başına en fazla 2** (kullanıcı kararı; analiz: boş bulut sayısı renk sayısına ulaşınca bulmaca önemsizleşir). Limit dolunca buton soluklaşır ama basılabilir; mesaj **"Sky Is Full"** (kullanıcı seçeneklerden seçti): üretilmiş seviyede "…Try Undo or Shuffle.", Shuffle kilitli 1–15'te "…Try Undo.". Hak/reklam harcanmaz.
5. **Akıllı "No Moves" mesajı** — yalnızca o an kullanılabilen seçenekler: Undo (geçmiş varsa), Extra Cloud (limit dolmadıysa), Shuffle (üretilmiş seviyede), Restart (can/reklam durumuna göre); hakkı 0 olup reklamla dolanlar "(ad)" + alt satırda "(ad) = watch an ad first"; hiçbiri yoksa "Wait for the next life to regenerate."
6. **Seviye bittikten sonra Restart engellendi** — v1'de "Sky Balanced!" ekrandayken Restart'a basılabiliyor, can harcanıp seviye ikinci kez tamamlanabiliyordu (çift analytics).

### 4. Build aracı — `Assets/_Cloudora/Editor/CloudoraBuildTools.cs`
- `CLOUDORA_DEV_APP_ID` / `CLOUDORA_DEV_PRODUCT_NAME`: **yalnızca development APK**'da paket adı ve görünen ad geçersiz kılınabilir (aynı telefonda sürümleri yan yana kurmak için). Release/Play build'de verilirse build durur. Build sonrası değerler geri yüklenir; `ProjectSettings.asset`'e kalıcı yazılmaz. Değişken verilmezse davranış v1 ile aynı.

### Doğrulama
- **Unity Editor (Play Mode):** Kullanıcı tüm akışları defalarca denedi (renkler, butonlar, menüler, Restart/Shuffle/Extra, limit, rozetler). Editor.log'da exception yok. Ara derlemeler ayrıca Unity'nin Roslyn derleyicisiyle bağımsız kontrol edildi (0 hata).
- **Android cihaz (Galaxy S24 FE):** "Cloudora v2" (`com.yigityvz.cloudora.v2`) v1'in yanına **sıfır kurulum** olarak yüklendi; v1 telefonda kendi kaydıyla duruyor.
  - İlk açılış: varsayılan kayıtla seviye 1; L1 → L21 oynandı, Green Valley tamamlandı.
  - Extra limiti her seviyede tam 2; Shuffle 1–15'te kilitli, 16+'da açık; can bitince ödüllü can teklifi; ara reklam fırsatı.
  - **No Moves:** L19'da (1 boş bulutlu seviye) hesaplanan 3 hamlelik diziyle tetiklendi; reklamlı/reklamsız tüm seçenek kombinasyonları kullanıcı tarafından doğrulandı (`level_failed reason=no_moves`).
  - Kalp ikonları telefonda görünüyor (kullanıcı teyidi).
  - Logcat: çökme, ANR, C# exception yok; yalnızca dev build'e özgü Profiler soket uyarıları ve `AssetPackManager` kontrolü.

### Build ortamı notları (v2'de değişenler)
- **Windows 260 karakter yol sınırı:** `GRADLE_USER_HOME` proje içinde (`Library\GradleHome`) olunca native derleme (`buildCMakeDebug`) `Filename longer than 260 characters` ile düştü. Kısa yol kullanılmalı:
  ```powershell
  $env:GRADLE_USER_HOME = "C:\Dev\gradle-cloudora"
  ```
  Yol değiştirildiyse eski CMake önbelleği proje kökündeki `.utmp\` ve `Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\build\` klasörlerinde kalır; ikisi de silinmeli (git dışı, yeniden üretilir).
- **Yan yana v2 kurulumu:**
  ```powershell
  $env:CLOUDORA_DEV_APP_ID = "com.yigityvz.cloudora.v2"
  $env:CLOUDORA_DEV_PRODUCT_NAME = "Cloudora v2"
  $env:CLOUDORA_BUILD_OUTPUT = "Builds/Android/Cloudora-v2-dev.apk"
  ./Build-Android.ps1 -Mode dev -ProjectPath "C:\Dev\ClaudeProjects\Android-Cloudora\Cloudora"
  adb install -r Builds\Android\Cloudora-v2-dev.apk
  ```
- APK üzerine yazılarak üretilirse dosya boyutu büyüyebilir (Gradle artımlı paketleme boşluğu; içerik aynı). Temiz boyut için build öncesi çıktı dosyası silinmeli.

### Bilinen açık sorunlar / notlar (v2'de dokunulmadı)
1. v1 listesindeki 1, 2, 4, 6, 7, 8 hâlâ açık (Night/Fog desenleri, doğrulayıcı Wind'i bilmiyor, Rainy Coast "Next rule" metni, `Update` GC, ayarların iki yerde tutulması, yerelleştirme yok). #3 (bitmiş seviyede Undo) hâlâ açık; #5 (Restart süre sayacını sıfırlamıyor) hâlâ açık — cihazda L1 368 sn, L16 1101 sn görüldü.
2. "No Moves" yalnızca "hiç legal hamle yok" durumunu yakalar; anlamsız ileri-geri hamleler legal sayıldığı için fiilen çıkmaza giren oyuncu mesajı görmeyebilir. 3 renk + 2 boş bulutlu seviyelerde No Moves matematiksel olarak imkânsız; yalnızca 1 boş bulutlu seviyelerde (19, 29, 39…) mümkün.
3. Reklamla alınan can, "Tap again" ile basılan Restart'ta hemen harcanır (kullanıcı: gerçek reklam gelene kadar kalsın).
4. Hamlesiz Restart'lar (Undo ile her şey geri alınmış / Shuffle sonrası) tahtayı değiştirmez ama `level_restarted` olarak kaydedilir (analytics gürültüsü).
5. Shuffle seed dizisi uygulama yeniden açıldığında aynı seviyede aynı sırayla tekrar eder.
6. 41–80'de Frozen/Locked son buluta konur; Shuffle sonrası son bulut bir ekstra bulut olabilir.
7. Release build hiç cihazda denenmedi (reklam kapalı → rozetler "0", "Reward Unavailable"); build aracı release'i yalnızca AAB üretiyor.
8. Telefondaki v1 "Cloudora" adıyla duruyor ("Cloudora v1" olarak yeniden adlandırılmadı).

---

## v1 — Çalışır durumdaki temel sürüm

- **Tarih:** 2026-10-12
- **Dal:** `bahadir-claude-versions` (`main` @ `0c967b1` üzerinden açıldı)
- **Temel:** Ana yazarın (yigityvz) son kod commit'i `a18a3e4` — *fix: restore gameplay UI on Android startup* (upstream `codex/phase2-15`) + `main`'deki README katkı bölümü (`0c967b1`).
- **Uygulama:** `com.yigityvz.cloudora` 0.1.0 (version code 1), Unity 6000.5.9f1, URP 2D, Android min API 26 / target API 36, IL2CPP, ARM64.

### Amaç
Orijinal projenin bu makinede Editor'da ve gerçek Android cihazda çalıştığı doğrulanmış, sonraki geliştirmeler için güvenli başlangıç noktası.

### Değişiklikler
1. **SkyGradient için eksik `CanvasRenderer` eklendi** — `Assets/_Cloudora/Scripts/UI/CloudoraVisuals.cs`
   - Sorun: Play Mode'da her karede `MissingComponentException: There is no 'CanvasRenderer' attached to the "SkyGradient" game object`. Hata EventSystem'in UI raycast'ini kesiyordu; Editor'da oyun fiilen oynanamıyordu.
   - Önce: `new GameObject("SkyGradient", typeof(RectTransform), typeof(SkyGradientGraphic));`
   - Sonra: `new GameObject("SkyGradient", typeof(RectTransform), typeof(CanvasRenderer), typeof(SkyGradientGraphic));`
2. **`crVersion.md` eklendi** — bu dosya.

### Doğrulama
- **Unity Editor (Play Mode):** Seviye 1–17 oynandı; hata yok.
- **Android cihaz:** Samsung Galaxy S24 FE (SM-S721B), development APK (45.278.886 bayt, 0 build hatası).
  - ~13 dk oyun: seviye 1 → 42, 41 seviye tamamlandı; Green Valley ve Rainy Coast dünyaları bitirildi, Frost Peaks'e (Frozen) geçildi.
  - Undo, Extra Cloud, Restart, sahte ödüllü reklam ve interstitial akışları tetiklendi.
  - Uygulama 6 kez kapatılıp açıldı; her seferinde doğru seviyeden devam etti (kayıt sistemi çalışıyor).
  - Logcat: çökme, ANR veya C# exception yok. Yalnızca zararsız Unity mesajları (`AssetPackManager` kontrolü, dev build'e özgü Profiler multicast socket uyarıları).
- Not: Test edilen APK `add-contributors-file` dalından derlendi; bu dalla tek fark doküman dosyalarıdır (CONTRIBUTORS.md / README), oyun kodu birebir aynıdır.

### Build ortamı notları (bu makine)
- Unity Hub → 6000.5.9f1 → **Android SDK & NDK Tools** ve **OpenJDK** modülleri gerekli.
- Kullanıcı adında ASCII olmayan karakter ("ı") olduğu için Gradle'ın `prefab_command.bat` adımı bozuluyor (`ClassNotFoundException: com.google.prefab.cli.AppKt`). Çözüm, build'i başlatırken süreç bazında:
  ```powershell
  $env:GRADLE_USER_HOME = "C:\Dev\ClaudeProjects\Android-Cloudora\Cloudora\Library\GradleHome"
  ./Build-Android.ps1 -Mode dev -ProjectPath "C:\Dev\ClaudeProjects\Android-Cloudora\Cloudora"
  ```
- Build öncesi Unity Editor kapalı olmalı (proje kilidi).
- Kurulum: `adb install -r Builds/Android/Cloudora-dev.apk` (adb: `...\AndroidPlayer\SDK\platform-tools\adb.exe`).

### Bilinen açık sorunlar (v1'de dokunulmadı)
1. Night/Fog gizlemesi desen yazılarını (`*`, `///`, `+`) gizlemiyor — `CloudContainerView.cs:203-205`.
2. `PuzzleValidator` modifier'ları (özellikle Wind) hesaba katmıyor; 10.000 seviye doğrulaması Wind seviyelerinde oynanabilirliği kanıtlamıyor.
3. Seviye bittikten sonra Undo yapılabiliyor; yeniden çözülünce `CheckWin` ikinci kez çalışıyor (çift analytics / reklam sayacı).
4. Rainy Coast (21–40) "Next rule: Wind" gösteriyor ama üretici bu aralıkta modifier eklemiyor.
5. `RestartLevel` `_levelStartedAt`'ı sıfırlamıyor; süre analytics'i yanlış.
6. `PuzzleController.Update` her karede string üretiyor (GC).
7. Ses/titreşim ayarı hem PlayerPrefs'te hem JSON kayıtta.
8. Arayüz metinleri sabit İngilizce; yerelleştirme yok.
- Gözlem: Cihaz testinde seviye 9'da 5, seviye 31'de 3 yeniden başlatma — zorluk sıçraması olabilir.
  - **Düzeltme:** Zorluk değil. Kullanıcı Restart (ve diğer butonları) **kasıtlı olarak** art arda basarak test etti (L9: 3 sn'de 5, L31: 0,7 sn'de 3 Restart). Hızlı art arda basmaya karşı koruma (debounce) ileride eklenebilecek bir iyileştirme olarak not edildi; mevcut davranış bir kullanıcı hatası değildir.
- Yayın için açık kalanlar: imzalama, gizlilik politikası/destek e-postası, gerçek ekran görüntüleri, Play Console beyanları.

### Cihaz testi logcat gözlemleri (ikinci inceleme)
- Seviye süreleri L9–L41 arası 6–25 sn, 6–13 hamle; tasarım hedefi (≤20: 30–60 sn, 21–100: 60–180 sn) karşılanmıyor.
- Extra Cloud dev build'de fiilen sınırsız (sahte reklam anında dolduruyor; L18'de 2 sn içinde 3 kez — kullanıcının kasıtlı buton testi).
- Her geçersiz hamlede `Handheld.Vibrate()` 500 ms titreşim — uzun/sert.

### Kullanıcı yorumu (Galaxy S24 FE testi sonrası)
0. **Test notu:** Restart, Extra Cloud, Shuffle vb. butonlara hızlı art arda basmalar kullanıcının **kasıtlı buton testi**dir; yanlışlıkla basma veya zorluk kaynaklı değildir. Hızlı basmaya karşı koruma eklenmesi güzel bir özellik olabilir.
1. **Zorluk / tekrar:** Mantık anlaşıldıktan sonra zorluk hissedilmiyor, bölümler hızlıca geçiliyor. 15. seviyeden sonraki üretilen bölümler kendini tekrarlıyor gibi; basit bir matematikle aynı veya çok benzer hamlelerle geçilebiliyor.
2. **World butonu:** Basınca ne olduğu anlaşılmıyor.
3. **Buton hizalaması:** Alt butonlar 3+3 ortalı olabilecekken 4 üst / 4 alt yuvalı ızgarada üst sıranın 1.'si, alt sıranın 3.'sü boş bırakılmış gibi kaymış.
   - Koddan teyit (`GameplayOverlay.cs`): üst sıra -130 / +130 / +390, alt sıra -390 / -155 / +350.
4. **Renk (en dikkat çeken):** Bulut arka planı ekran arka planıyla çakışıyor, göz ayırt edemiyor; birkaç ton daha belirgin olmalı.
   - Koddan teyit: bulut `(0.93, 0.97, 1.0)` ≈ gökyüzü üstü `(0.92, 0.97, 1.0)`.
5. **Menü ekranları:** Continue / World / Settings açıkken arkadaki oyun görünmemeli.
   - Koddan teyit: paneller ekranın ~%84'ünü kaplıyor (anchor 0.08–0.92).
