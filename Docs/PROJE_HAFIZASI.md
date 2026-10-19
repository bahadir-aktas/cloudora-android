# Cloudora — Proje Hafızası (Claude için oturum başlangıcı)

> Her oturumun başında **ilk iş bu dosyayı oku.** Projeyi anlamak için nelere bakılacağını, hangi kuralların geçerli olduğunu ve işin nerede kaldığını özetler. Ayrıntı her zaman kaynak dosyadadır; burada yazanla kaynak çelişirse **kaynak doğrudur**, bu dosya düzeltilir.
> Son güncelleme: 2026-10-19 13:09 (v4 commit öncesi).

---

## 0. Nerede kaldık (2026-10-19) — yeni oturumda önce bunu oku
- **v4 = iOS sürümünün Android'e birebir uyarlaması.** Kaynak: kullanıcının verdiği `https://github.com/yigityvz/cloudora-ios` (`main` @ `1ee4334`); salt okunur kopya `C:\Dev\ClaudeProjects\Android-Cloudora\iOS-inceleme\cloudora-ios\` (+ ölçüm `iOS-inceleme\olcum\`). Kullanıcı: "ios'da neyse aynısı olsun", farklar belgede işaretlenir. **Farklar ve çelişkiler: `Docs/IOS_FARKLARI.md` (+ `.html`)** — önce bunu oku; v3 tasarım belgeleri artık tarihçe + not.
- **Durum:** çalışma ağacında v4 hazır, testler geçti, telefon testi bitti. Commit/etiket `v4` kullanıcı onayıyla (Faz 7); push ayrı onayla. `v3.2` (`73611d2`) ve uzak repolar (`d7c9416`) değişmedi; v3.2 2026-10-15 21:05'te iki repoya push edildi.
- **Testler:** EditMode 242 → 236 geçti, 0 başarısız, 6 atlandı (2 grafik gerektiren + 4 eski).
- **APK:** `Builds/Android/Cloudora-v4-qa.apk` ("Cloudora v4", `com.yigityvz.cloudora.v4`, QA modu, 70 MB); ekip için kopya `C:\Dev\ClaudeProjects\Android-Cloudora\APK-ekip\Cloudora-v4.apk` + `KURULUM.txt`. v3.2 APK yedeği `Builds/Android/Cloudora-v3.2-dev.apk`.
- **Planlanan (kullanıcı, 2026-10-19 13:11: "temaların vs güncelleneceğini not düş"):** temalar (dünya görselleri, arka planlar, tema adları/tasarımı vb.) ileride güncellenecek; yüksek çözünürlüklü dünya resimleri bu işin parçası.
- **Açık işler (kullanıcı kararı bekleyen):** (1) başarısızlık kuralı (Ç1: hamle varken ekran, Undo/Extra yok — 60'ta sahada görüldü); (2) yüksek çözünürlüklü dünya resimleri (kullanıcı: "daha sonra"; yigityvz'den Figma 2×/3× dışa aktarım); (3) küçükler: kurtuluş ekranında can sayacı donuk, üst/alt gri şerit, "Ödülle devam et" metni. v3 faz planındaki 2.4, 3, 5.1 artık v4 yapısına göre yeniden değerlendirilmeli (dünya listesi iOS'la geldi).
- **Cihaz/kayıt durumu:**
  - Telefonda "Cloudora v4" kurulu, kayıt 100 bitmiş (coin 2300); v1, v2, v3 ve Efe'nin `claodeera` paketi yan yana. v4 kaydı `/sdcard/Android/data/com.yigityvz.cloudora.v4/files/` (adb ile düzenlenebilir; Git Bash'te `MSYS_NO_PATHCONV=1` + yerel yol `cygpath -w`). İlk v4 kaydının yedeği `yedek-telefon-v4-20261019-1217\`.
  - Editor kaydı L81 (coin 150); iOS uyarlaması öncesi yedek `yedek-editor-ios-uyarlama-oncesi-20261019-1135\`. Editor'da v4 henüz oynanmadı (kullanıcı: Android'den sonra).
  - Unity Editor kapalı.

## 1. Oturum başında okuma sırası
1. **Bu dosya** (önce §0).
2. `ClaudoraRelease.md` (repo kökü) — yapılan her işlemin kaydı, en yeni en üstte.
3. **`Docs/IOS_FARKLARI.md`** — v4'ün (iOS'tan birebir) v3.2'den farkları, çelişkiler Ç1–Ç10, ölçüm, telefon testi.
4. `Docs/V3_BOLUM_VE_ZORLUK_TASARIMI.md` — v3'ün tasarım dokümanı (v4'te bazı kararlar değişti; başındaki not): parçalar/temalar, zorluk ilkeleri, kararlar (§11b–11l), faz planı (§11l), açık konular (§15).
5. `crVersion.md` (repo kökü) — onaylanmış versiyonlar (v1 … v4), **baştan sona** oku.
6. `Docs/V3_SABLON.md` ("Uygulanan plan": 1–100 bölüm bölüm tablo, v3.2 hali; 60–80 v4'te farklı) ve `Docs/V3_BOLUM_HARITASI.html` (v3.2 görsel özeti).
7. İşe göre: ilgili kod dosyaları (bkz. §6 kod haritası) ve iOS'tan gelen `Docs/IOS_*.md` belgeleri (yigityvz'in kendi kayıtları; aralarında çelişkiler var, bkz. `IOS_FARKLARI.md` §3.1).
8. **Çalıştırarak doğrula (kullanıcı kuralı, her oturum):** Unity Editor kapalıyken (kilit: `Temp\UnityLockfile`; açıksa `CloseMainWindow` ile kapat) sırayla:
   - EditMode testleri: `Unity.exe -batchmode -nographics -projectPath <proje> -runTests -testPlatform EditMode -testResults Logs\<ad>.xml -logFile Logs\<ad>.log` → XML'deki `total/passed/failed/skipped`. Beklenen (v4): 242 / 236 / 0 / 6.
   - Bölümlerin ölçümü: `-executeMethod Cloudora.Editor.V3LevelTool.RunCalibrationReportBatch` (`CLOUDORA_V3_CAL` satırları). Önizlemeler: `RunTransitionPreviewBatch`, `RunTheme2PreviewBatch`, `RunTheme3PreviewBatch` → `Logs/v3-preview-<Parça>.csv`. Veri üretimi: `GenerateLevelDataBatch` (veri değişmemeli; `GeneratedDataMatchesBuilder` testi korur).
   - Unity yolu: `C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Unity.exe`.
9. **Editor oyun testi (kullanıcı kuralı, her oturum):** Claude oynayamaz; kullanıcı oynar, Claude hazırlar ve log'dan doğrular.
   - Batch işleri bitince Unity Editor'ı projeyle aç (`Start-Process Unity.exe -ArgumentList '-projectPath "<proje>"'`); kaydı yedekleyip (SHA kontrolü) gerekirse `currentLevel` / `highestCompletedLevel`'i ayarla (`%USERPROFILE%\AppData\LocalLow\YigitYvz\Cloudora\`).
   - Kısa rutin (5–10 dk): 2–3 bölüm; Undo, Extra, Shuffle birer kez; 1 boş bulutlu bir bölümde başarısızlık + devam. Parça değiştiyse parçanın tamamı.
   - Sonra `Logs/Editor.log`'dan doğrula: `[Analytics]` ve `[Input]` satırları (frame ve t ile), exception; bölüm verisiyle (en kısa çözüm) karşılaştır, tabloyu `ClaudoraRelease.md`'ye yaz.
   - **`Logs/Editor.log` Editor her açılışta üzerine yazılır** → önemli test loglarını `Logs/oyun-testleri/` altına tarihli adla kopyala (§8).
   - Editor açıkken build alma ve `Assets/` altına yazma (oyun sırasında yeniden derleme tetiklenir).

## 2. Çalışma kuralları (kullanıcı: Bahadır; Türkçe konuşur)
- Promptları uygula; **gerçek karar noktalarında onay iste** (kullanıcı fazlarda "her adımda sor" dedi). Oyuncuya görünen metinler için 3–4 somut seçenek sun.
- **Her işlem** (küçük düzeltme ve önemli kontroller dahil) tarih-saatle `ClaudoraRelease.md`'ye, en yeni en üstte, Türkçe yazılır. **Saati her zaman `Get-Date` / `date` ile al, tahmin etme** (2026-10-15'te tahmini saat yazılıp düzeltildi). Kişisel yol yazma (`%USERPROFILE%` kullan).
- Uzun işlerde ara ara yüzde ver.
- **Versiyon:** yalnızca kullanıcı onayıyla; `bahadir-claude-versions` dalında commit + etiket, `crVersion.md`'ye giriş. Eklemeler v4.1…; bir sonraki major değişiklik v5. `ClaudoraRelease.md` her versiyonla birlikte commit'lenir.
- **Push: her seferinde ayrı, açık onay.** Önceki onay sonrakine geçmez. Hedefler: `upstream` (yigityvz/cloudora-android) `cloudora-bahadir` + `origin` (bahadir-aktas/cloudora-android) `main`, etiketlerle. Force yok; takım reposunun `main`'ine dokunulmaz.
- **Her push'tan önce proje hafızasını güncelle (kullanıcı, 2026-10-15):** `Docs/PROJE_HAFIZASI.md` (özellikle §0 "Nerede kaldık": versiyon, push durumu, sıradaki iş) ve gerekiyorsa `ClaudoraRelease.md` güncellenir, commit'lenir; push bu commit'le birlikte yapılır.
- Commit'ler değiştirilmez (amend/rebase yok). Çok satırlı mesaj için `git commit -F` (dosya ya da stdin).
- Gördüğün her tutarsızlığı/çakışmayı (küçük de olsa) kullanıcıya söyle. İstisna: `paylasim/DEVRALMA.md` eski ve aktif kullanılmıyor — kaynak sayma, eskiliğini raporlama, dokunma.
- Kullanıcı testlerinde art arda hızlı basmalar kasıtlı buton testidir (genelde 1–15'te). Bilerek "No Moves" denenen bölümler ve özelliklerle hızlı geçilen bölümler zorluk analizine katılmaz. **Daha önce çözülmüş bölümler tekrar oynanınca** kolay gelir (telefon testi 75/78) — zorluk yorumunda bunu belirt.
- Geçmişte olanı anlatırken ne zaman yapıldığını açık yaz.
- Oyunda görünen metinde ASCII dışı sembol kullanma; Android'de görünmez → sprite ikon.
- Yeni C# dosyalarını CRLF'ye çevir. Python'da Türkçe çıktı için `PYTHONIOENCODING=utf-8`.

## 3. Kaynaklar — ne nerede yazıyor
| Kaynak | İçerik | Durum |
|---|---|---|
| `Docs/IOS_FARKLARI.md` + `.html` | v4: iOS sürümünün v3.2'den farkları (kural, bölüm, ekonomi, arayüz, platform), çelişkiler Ç1–Ç10, iki kuralla ölçüm, Android uyarlaması, telefon testi, dosya listesi | **Güncel, v4 referansı** |
| `Docs/IOS_*.md` (iOS'tan) | yigityvz'in iOS kayıtları (tasarım, UX, hareket, bitiriş, Ay 60, performans) | Kaynak belge; aralarında eskimiş/çelişen ifadeler var |
| `Docs/V3_BOLUM_VE_ZORLUK_TASARIMI.md` | v3 tasarımı: desen, parçalar/temalar, yöntem, Shuffle/başarısızlık/coin kararları, modifier arşivi, kararlar §11b–11l, faz planı §11l, açık konular §15 | v3 referansı; v4 farkları başındaki notta |
| `Docs/V3_SABLON.md` | 1–100 şablonu: ilkeler, yenilik envanteri, hedef eğrisi, **"Uygulanan plan"** (100 satır: hedef/puan/renk/kapasite/boş/dizilim/en kısa/tuzak/No Moves/yenilik) + taslaktan farklar; Faz 1.4 taslağı tarihçe | Güncel (20:20) |
| `Docs/V3_BOLUM_HARITASI.html` | Tek sayfa: gelen özellikler sırası, kurallar, tuzak ve No Moves listeleri, 100 bölümün kartları | Güncel (20:10); bölümler değişirse `Logs/araclar/make_map.py` + `make_html.py` ile yeniden üret |
| `Docs/V3_CERCEVE_ENVANTERI.md` | Faz 1.1 ölçümleri: 60 çerçevenin dağılımları + bulgular | Güncel; puan ağırlıkları değişirse yeniden ölçülmeli |
| `ClaudoraRelease.md` | Her işlemin kaydı; test tabloları (insan hamle/süre), kararların gerekçeleri | Güncel |
| `crVersion.md` | v1, v2, v3, v3.1, v3.2, **v4** ayrıntıları | Güncel |
| `README.md` | Proje tanıtımı | v3.2'ye göre güncellendi |
| `Docs/Cloudora 1–100 Şablonu.html` | Faz 1.4 inceleme sayfasının yerel kopyası (eski) | **Commit'e alınmadı** (kullanıcı kararı); yerini `V3_BOLUM_HARITASI.html` aldı |
| claude.ai "1–100 şablonu" (`https://claude.ai/artifact/PDN14bYos5oRJkJLCjHeMW`) | Faz 1.4 inceleme sayfası (grafik + tablo) | Taslak plana göre; uygulanan plan `V3_SABLON.md`'de |
| `Docs/GELISTIRME_ONERILERI.md` | v1 sonrası öneri listesi | Versiyon planı artık geçerli değil |
| `Docs/PROJECT_PROGRESS.md`, `ANDROID_BUILD.md`, `GOOGLE_PLAY_SUBMISSION.md`, `RELEASE_CHECKLIST.md`, `PRIVACY_RELEASE_REVIEW.md` | Ana yazarın geçmişi ve yayın süreçleri | Tarihsel; yayın onayı yok |
| claude.ai "Cloudora — Temalar, Bölümler ve Zorluk" (`https://claude.ai/artifact/2mwyMmgphECxmDmkSojPWH`) | **Eski** 13 dünyalı yapı (1–400) | v3 öncesi; tarihsel |
| `paylasim/DEVRALMA.md` | Eski devralma notu | **Kullanılmıyor, kaynak değil** |
| `Logs/oyun-testleri/` (git dışı) | Tarihli test logları: Editor 61–100, telefon logcat'leri, Shuffle yük ölçümü, önizleme CSV'leri | Bkz. §8 |
| `Logs/araclar/` (git dışı) | Bölüm haritası ve şablon tablosunu üreten Python betikleri | Bkz. §8 |
| `..\iOS-inceleme\` (repo dışı) | iOS reposunun salt okunur kopyası, fark dokümanı taslakları, ölçüm programı ve `olcum.csv` | v4 kaynağı |
| `..\APK-ekip\` (repo dışı) | Ekip arkadaşları için `Cloudora-v4.apk` + `KURULUM.txt` | Paylaşım kullanıcıda |
| Hafıza dizini (`%USERPROFILE%\.claude\projects\…\memory\`) | Kullanıcı tercihleri ve kurallar | Her oturumda otomatik yüklenir |

## 4. Proje özeti
- Unity **6000.5.9f1**, Android, dikey, tek parmakla hava sıralama bulmacası. Ana yazar **yigityvz**; Bahadır katkıda bulunan. Tek sahne `Assets/_Cloudora/Scenes/02_Gameplay.unity`; arayüz kodla oluşturulur.
- **Dal:** `bahadir-claude-versions`. Etiketler: v1 (`adaa0b8`), v2 (`61088e4`), v3 (`b0b2f13`), v3.1 (`ae54acc`), v3.2 (`73611d2`), **v4 (onayla, commit sonrası)**. v3/v3.1 2026-10-15 00:32'de, v3.2 21:05'te iki repoya push edildi.
- **iOS sürümü:** yigityvz'in ayrı reposu `yigityvz/cloudora-ios` (`main`); v4 bunun `1ee4334`'ünün Android karşılığı.
- **Telefon (Samsung Galaxy S24 FE, adb `R5CXA3032LP`):** "Cloudora" (v1), "Cloudora v2", "Cloudora v3" (`com.yigityvz.cloudora.v3`, v3.2 derlemesi), **"Cloudora v4"** (`com.yigityvz.cloudora.v4`, QA derlemesi) ve Efe'nin `com.yigityvz.cloudora.claodeera` yan yana.

## 5. Oyun kuralları (kod: `Scripts/Core/PuzzleRules.cs`, `CloudState.cs`)
> v4 (iOS'tan) kuralları; v3.2'den farklar ✱ ile işaretli. Ayrıntı `Docs/IOS_FARKLARI.md` §2–4.
- Hazneler (eski adıyla bulutlar; v4'te cam hazne görünümü) alttan üste dizili hava parçası yığınları; kapasite 4–5. Türler: Güneş, Yağmur, Kar (6+), Rüzgar (31+), **Ay (60+ ✱)**, Şimşek (yok) + Gökkuşağı joker (1–100'de yok).
- Hamle: kaynak → hedef. En üstteki aynı renkli grup **sığdığı kadar otomatik** taşınır. Hedef boş ya da üstü aynı renk olmalı. **✱ Tamamlanmış (dolu, tek renk) hazne kilitli:** seçilemez, boşaltılamaz, karıştırılamaz.
- Kazanma: boş olmayan her hazne dolu ve tek renk. Son hazneden sonra 1,78 sn bitiriş efekti, sonra sonuç paneli.
- **✱ Tahta çözülemez olunca = bölüm başarısız** (arka planda `ReachabilitySearch`, hamle kalmış olsa bile; v3.2'de "hiç legal hamle yok"du): Retry (−1 can; 1–5 ücretsiz) · reklam ("Ödülle devam et") veya **500 coin** ile **çözülebilen son duruma** dönüş (`FailRecovery`). Ekranda Undo/Extra yok. Kapanırsa `FailRecord`.
- **Booster'lar:** Undo (başlangıç 3, reklam +3), Extra (başlangıç 1, reklam +1, bölüm başına en fazla 2), Shuffle (16'dan; başlangıç 1, reklam +1). Shuffle/devam kontrolü `PuzzleSolver.CheckSolvable`.
- **Can:** en fazla 5, 30 dakikada 1. **✱ Bitmemiş bölümden Ana Sayfa −1 can (1–5 dahil).** **✱ Coin:** Orta +100, Zor +200, Çok zor +300.
- **✱ Oyun 100'de biter** (101 yüklenmez). **✱ Dünyalar:** tanıtım 1–14, Yeşil Vadi 15–30, Rainy Coast 31–60, Frost Peaks 61–100; 14'ten sonra "tanıtım tamamlandı", 30/60/100'de kurtuluş ekranı.
- **Restart yok.** ✱ "Reklamla +1 can" duraklatma menüsünde (can 0 iken ana sayfada "Can Kazan · +1"). ✱ Bilgi penceresi girdiyi yakalar, dokunuş yalnızca kapatır. ✱ Arayüz Türkçe. ✱ Ayarlarda "Azaltılmış hareket" (açıksa efektler/kurtuluş animasyonu çok kısa).
- **Modifier'lar** kodda var, 1–100'de kullanılmıyor.

## 6. Kod haritası (`Assets/_Cloudora/`)
| Yer | Ne |
|---|---|
| `Scripts/Core/` | Saf kurallar |
| `Scripts/Puzzle/PuzzleController.cs` | Ana akış; `LoadLevel`: 1–15 `AuthoredLevelCatalog.Get`, 16–100 `TryGetBuilt` (veri), 101+ `LevelGenerator`; `SolverStateLimit = 200000` |
| `Scripts/Puzzle/CloudContainerView.cs` | Bulut görünümü, renkler (`GetColor`, `GetPatternColor`) |
| `Scripts/Level/V3LevelPlan.cs` | Parçalar, bant deseni, `ShuffleUnlockLevel = 16`; 16+ için `FrameFor` yok (çerçeveyi kurucu seçer) |
| `Scripts/Level/PuzzleSolver.cs` | Ölçüm çözücüsü `Solve` (bütün durum uzayı + zorluk ölçüleri + sanal oyuncu; araç kullanır) ve oyun içi **`CheckSolvable`** (hızlı) |
| `Scripts/Level/BoardFrame.cs` | Çerçeve: kapasite, renk, boş, `OpenClouds` (serpiştirilmiş dizilim) |
| `Scripts/Level/V3LevelData*.cs` | Seçilmiş 1–100 bölüm (`.Generated.cs` araçla üretilir, elle değiştirilmez) |
| `Scripts/Level/AuthoredLevelCatalog.cs` | 1–15 (öğretici yazılar) + `TryGetBuilt` (16–100) |
| `Scripts/Level/LevelGenerator.cs`, `BalanceProfile.cs` | **Eski** üretici: yalnızca 101+ |
| `Scripts/Level/CloudShuffler.cs` | Bulut içi Shuffle + `FailRecovery` |
| `Scripts/Progression/` | Can, coin (`CoinEconomy` 100/200/300, devam 500), booster, `WorldCatalog` (3 dünya, `MaxLevel = 100`, `IntroLastLevel = 14`) |
| `Scripts/Services/` | Kayıt, `FailRecord`, reklam (`CLOUDORA_DEVICE_QA` → sahte reklam), analytics (`DebugAnalyticsService`: frame ve t) |
| `Scripts/UI/GameplayOverlay.cs` | HUD (Figma), Undo · Extra · Shuffle, duraklatma, sonuç/başarısızlık panelleri, `[Input]` debug satırları |
| `Scripts/UI/` (v4) | `DesignUI`, `VectorShapes`, `WeatherGlyph`, `LiquidFillGraphic`, `VesselCompletionFlourish` (bitiriş), `OpeningSplash`, `MilestoneScreen` (tanıtım bitti / kurtuluş), `WorldRescueReplay`, `WorldAtmosphere` (+ `GameplayBackdrop`), `MainMenuController`, `WorldScreenController` |
| `Scripts/Puzzle/MoveAnimator.cs`, `WeatherMotionLibrary.cs` | Elemente özel aktarım animasyonu |
| `Editor/CampaignMoonRollout.cs` | Ay 60 yayımlama katmanı: kurucuların sonucu üstünde 60–80'i yeniden seçer (`V3LevelTool` veri/önizleme bunu kullanır) |
| `Editor/CloudoraBuildTools.cs` | Android build: dev APK, **Device QA APK (v4)**, Release/Play AAB; `CloudoraIosBuildTools.cs` iOS (Android'de kullanılmıyor) |
| `Editor/DesignQuality.cs` | Tasarım görsellerinin içe aktarma ayarı (iOS + Android: 4096 / RGBA32 / sıkıştırmasız) |
| `Editor/V3LevelBuilder.cs` | Parça dağıtımı (Ladder → Theme → sabit 1–15); 1–15 kurulumu (400 aday, 300.000 durum) |
| `Editor/V3LadderBuilder.cs` | 16–30 kurucusu; ortak havuzlar (`Pool`), `LongMovesThreshold`, `PreviousBoards`, `NoMovesBonus`, `V3Novelty` |
| `Editor/V3ThemeBuilder.cs` | 31–100 kurucusu: segment yapılandırması (renk, dizilim adımları, eğriler, dönüş penceresi, `TrapsByCycle`, `HardTrapLevels`), final kuralları, `EarlyTrapPreference`, `TrapPoolCandidates`, Tema 2 için yalnızca 59'u etkileyen `FallbackTrapPoolCandidates` |
| `Editor/V3LevelTool.cs` | Menüler/batch: veri üretimi (`PartsToBuild` = 5 parça), önizlemeler, envanter, dizilim, kalibrasyon raporları |
| `Editor/LevelCandidateFactory.cs` | Aday tahtalar (`DealScattered` dahil) |
| `Tests/Editor/` | `V3LevelBuildTests`, `V32LadderTests`, `V32ThemeTests`, `V32Theme3Tests`, `V32SolverLoadTests`, `V32VisualTests`, `V32InputTests`, `V32NoMovesTests`, `V32LayoutTests`, `V31GameplayTests`, `LevelToolTests`, `PuzzleFeelTests`; `V32ShuffleLoadProbe` (`[Explicit]`, elle) |

## 7. Bölüm tasarımı (v3) — özet
**Desen:** her 5 bölümde **Orta, Orta, Zor, Orta, Çok zor**; etiketler parçadan parçaya farklı ölçekte, zorluk birikimli, parça geçişlerinde sıçrama yok (geçiş testinde Orta'lar için 0,25 pay: eğriler sınırda birleşir).

| Parça | Bölümler | Ad | Ne geliyor | Durum |
|---|---|---|---|---|
| 1 | 1–5 | Tanıtım | 2 renk, kap. 4, 2 boş, öğretici yazılar | Bitti (v3) |
| 2 | 6–15 | Alışma | Kar (6), "Undo is always safe" (10) | Bitti (v3) |
| 3 | 16–30 | Green Valley (görsel) | Shuffle 16, serpiştirilmiş dizilim 17, kap. 5 (19), tek boş bulut 21; final 30 | Bitti (v3.2) |
| 4 | 31–60 | Tema 2 | Rüzgar 31 (2 boşla nefes), tek boş 36'da döner, 1 açık 39, klasik 49; döngüde 1 tuzak; final 60 | Bitti (v3.2) |
| 5 | 61–100 | Tema 3 | 61–80: yeni görsel tema, klasik 74, tuzaklar 1 → 2, Zor/Çok zor tuzak 75, 78. 81–100: Ay 81, tek boş 87, 1 açık 89, klasik 94, Zor/Çok zor tuzak 95, 98; final 100 | Bitti (v3.2) |

- **Kurallar:** bir bölümde tek yenilik, Orta'da gelir; kapasite/renk geri düşmez; yeni renkle boş bulut ve dizilim "nefes alır"; No Moves zaman zaman ve bantlara dağılmış; kilitleyen tahta (takılma ≥ 0,5) yalnızca finalde; final en az ortanca uzunlukta ve parçanın en tuzaklısı; hamle sayısı absürt olmamalı (çerçevenin %90'lık değeri).
- **Puan:** en kısa hamle ×1 + tuzak ×15 + yanıltıcılık ×10 + çıkmaz ×**20** + sanal oyuncu fazla hamlesi ×1,5 + takılma ×10.
- **Hedef eğrisi (Orta / Zor / Çok zor):** 31–60: 20→24 / 22,5→26,5 / 25,5→28,5 · 61–80: 24→26 / 26,5→28,5 / 28,5→30,5 · 81–100: 26,5→29,5 / 29→32 / 31→34.
- **Yöntem (değişiklik gerekirse):** önce test (`V32*Tests`), sonra kurucu; önizleme CSV'si ile kontrol; `GenerateLevelDataBatch`; tam test; veri dosyasını öncekiyle satır satır karşılaştır (yalnızca hedeflenen bölümler değişmeli — 59 düzeltmesinde ilk deneme 49/50'yi de değiştirmişti); Editor testi; telefon testi.
- **İnsan verisi (2026-10-15):** tuzak bölümleri (1 boş) düşünme süresi uzun ama fazla hamle az; planlama bölümleri (2 boş) fazla hamle 2–6. 81+ (5 renk) belirgin daha uzun sürüyor; kullanıcı "81 ve sonrası öncekilere göre epey zorlayıcı". Finaller (60, 100) insan için kolay kaldı.

## 8. Build, test ve log notları
- Build öncesi Unity Editor kapalı. **v4 (QA, iOS cihaz derlemesinin karşılığı):** `GRADLE_USER_HOME=C:\Dev\gradle-cloudora`, `CLOUDORA_DEV_APP_ID=com.yigityvz.cloudora.v4`, `CLOUDORA_DEV_PRODUCT_NAME="Cloudora v4"`, `CLOUDORA_BUILD_OUTPUT=Builds/Android/Cloudora-v4-qa.apk` → `./Build-Android.ps1 -Mode qa -ProjectPath <proje>` (arka planda, 3–8 dk). Eski dev APK: `-Mode dev` (v3 için `...cloudora.v3`). Editor menüsünden derlenirse paket v1'in üzerine gider. Ekip APK'sını güncellerken `..\APK-ekip\` kopyasını ve `KURULUM.txt` SHA-256'sını da güncelle (aynı makinede derlenmeli: debug imza).
- adb: `...\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe`. Kurulum `adb install -r`. Git Bash'te telefon yolları için `MSYS_NO_PATHCONV=1`, yerel dosya için `cygpath -w`. Telefon kaydı (v4): `/sdcard/Android/data/com.yigityvz.cloudora.v4/files/cloudora-save.json` (+`.bak`): uygulamayı `am force-stop` ile durdur, `pull` → düzenle (`currentLevel`, `highestCompletedLevel`, `failRecord` temizle) → ikisine de `push` → `monkey -p … 1` ile başlat.
- **logcat:** `adb logcat -v time -s Unity:V` arka planda dosyaya. **Unity batch çalışması adb sunucusunu yeniden başlatır ve logcat yakalamayı keser** → batch'ten sonra yeniden başlat.
- Telefon PC'den yaklaşık 10 kat yavaş (Shuffle ölçümü): PC'de 100 ms ≈ telefonda 1 sn.
- Editor kaydı: `%USERPROFILE%\AppData\LocalLow\YigitYvz\Cloudora\cloudora-save.json` (+ `.bak`); değiştirmeden önce `yedek-*` klasörüne yedekle. Telefon kaydı yedekleri de burada (`yedek-telefon-*`). **Scratchpad geçicidir**, kalıcı yedekleri oraya koyma.
- **Loglar (git dışı, `Logs/`):** `Logs/oyun-testleri/`: `2026-10-15-1911-editor-61-100.log`, `2026-10-15-1206-telefon-16-31-eski-plan.txt` (yeniden tasarım öncesi plan), `2026-10-15-2017-telefon-16-83-donma.txt`, `2026-10-15-2046-telefon-83-87-duzeltme.txt`, `2026-10-15-2038-shuffle-yuk-olcumu-eski-kod.csv`, `v32-v3-preview-*.csv`, `v32-v3-map-data.json`. 16–30 ve 31–60 Editor testlerinin ham logları üzerine yazıldı (analizleri `ClaudoraRelease.md` 17:56 ve 18:42'de). `Logs/araclar/`: `make_map.py` (veri → `Logs/v3-map-data.json`), `make_html.py` (→ bölüm haritası), `update_sablon.py` (→ şablon tablosu), `fix59.py` (uygulanmış yama, tarihçe).
- Play Mode'dayken kod değiştirme.
- Commit dışı bırakılanlar: `ProjectSettings.asset` (build'in yazdığı keystore alanı + boşluk farkları), `.vscode/`, `Docs/Cloudora 1–100 Şablonu.html`.

## 9. Açık konular ve bilinen tutarsızlıklar
**v4 (2026-10-19):**
- **Başarısızlık kuralı (Ç1):** ekran hamle varken gelir, Undo/Extra sunmaz; 60'ta 4 başarısızlık. Seçenekler: (a) kalsın, (b) başarısızlık ekranına Undo/Extra, (c) eski kural. Kullanıcı kararı bekliyor.
- **Dünya resimleri düşük çözünürlüklü** (aşama başına ~772×1300; S24 FE'de ~1,8× büyüyor): yigityvz'den Figma 2×/3× dışa aktarım (kullanıcı: "daha sonra").
- Kurtuluş ekranında can sayacı donuk (`MilestoneScreen.SetResources` güncellenmiyor; iOS'tan); kurtuluş ekranında üst/alt gri şerit (Android); "Ödülle devam et" reklam olduğunu söylemiyor; 15'in öğretici yazısı tanıtım 14'te bittiği için yersiz kalabilir.
- v3 tasarım kurallarıyla çelişkiler Ç3–Ç10 (`IOS_FARKLARI.md` §10): Ay 60 Çok zor finalde ve nefessiz, 1–5 can, ekonomi, Türkçe metinler seçeneksiz vb.
- Analytics başarısızlık nedeni `unsolvable` (eski log analizleri `no_moves` arıyordu).

**v3.2'den kalanlar** (v4'te bir kısmı değişti: dünya listesi ve 101+ iOS'la çözüldü):
- **Final ve ölçü (tasarım §15, kullanıcı "ileride yaparız"):** takılma ölçüsü dikkatli insanı tahmin etmiyor; 60/100 kolay, 59/78/83/90 zorladı. Faz 3'te oyun kayıtları ölçülerle karşılaştırılacak; alternatif: final adaylarını kullanıcı seçer.
- **Faz 2.6 kalanı (tasarım §15):** büyük tahtada gerçekten çözülemeyen bir Shuffle dizilişi hâlâ 200.000 duruma kadar arar (ölçümde görülmedi); gerekirse toplam bütçe ya da aramayı karelere bölme.
- **Tahta ölçeği (tasarım §15):** 7 bulutta (81+) ve Extra alınınca tahta küçülüyor; kullanıcı "uzun vadede değiştirelim".
- **Dünya listesi (Faz 5.1):** ekranda eski adlar (31–40 Rainy Coast, 41–60 Frost Peaks "Frozen", 61–80 sun-desert, 81–100 storm-islands). Dünya kimliği iki yerde: `WorldCatalog` ve `LevelGenerator.WorldId`. Katalog değişince kırılabilecek testler: `WorldProgressionCrossesBoundaryAndRestoresInStages`, `DifficultyWaveHasFinaleAndEasyWorldReset`, `CatalogUsesV3BoardsAndKeepsTutorialFrame`.
- **101+ sıçraması:** 101 eski sistemde, Rainbow modifier'lı, zorluk puanı 68,5 (100: 34).
- **Monotonluk:** 5 renkte en kısa çözüm 14–20 hamle; izlenecek.
- **Kullanıcının tartışmak istediği:** World ekranının kapsamı ve ortaya dokununca kapanması; Shuffle seçim modu daha belirgin olmalı.
- **Diğer açıklar (tasarım §14–15):** öğretici yazıların görünürlüğü, coin sayıları/başlangıç coini, reklamla can almanın yeri ("AD +1"), menüden "bölümü baştan başlat", zorluk sembolü (sprite), tema adları/özellikleri, Şimşek (6. renk) 1–100'de yok.
- **Zararsız kalıntılar:** `LevelCandidateSelector`'daki "Shuffle yedeği" (`backupCount`); `LevelDefinition.difficultyBand` (okunmuyor); `BalanceProfile.DurationTarget` (yalnızca testte).
- **Test notu:** buton denemeleri 1–15'te; o bölümlerin verisi zorluk analizine katılmaz.

## 10. Bu dosyanın bakımı
Önemli bir karar, yeni bir kaynak doküman, parça tamamlanması ya da oturum kapanışında bu dosya (özellikle §0) güncellenir ve değişiklik `ClaudoraRelease.md`'ye yazılır.
