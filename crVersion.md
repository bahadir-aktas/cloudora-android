# crVersion — Cloudora Versiyon Kaydı

`bahadir-claude-versions` dalındaki major versiyonların kaydı. Her versiyon kullanıcı onayıyla oluşturulur, git etiketiyle (`v1`, `v2`, …) işaretlenir ve **yalnızca yerelde** tutulur (GitHub'a push edilmez).

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
