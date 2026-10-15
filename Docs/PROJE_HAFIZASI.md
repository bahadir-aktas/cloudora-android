# Cloudora — Proje Hafızası (Claude için oturum başlangıcı)

> Her oturumun başında **ilk iş bu dosyayı oku.** Projeyi anlamak için nelere bakılacağını, hangi kuralların geçerli olduğunu ve işin nerede kaldığını özetler. Ayrıntı her zaman kaynak dosyadadır; burada yazanla kaynak çelişirse **kaynak doğrudur**, bu dosya düzeltilir.
> Son güncelleme: 2026-10-15 (v3.1 push edildikten sonra, v3.2 öncesi).

---

## 1. Oturum başında okuma sırası
1. **Bu dosya.**
2. `ClaudoraRelease.md` (repo kökü) — yapılan her işlemin kaydı, en yeni en üstte. En üstteki birkaç kayıt "nerede kaldık"ı söyler (özellikle "Oturum kapanışı" girdileri).
3. `Docs/V3_BOLUM_VE_ZORLUK_TASARIMI.md` — v3'ün **asıl tasarım dokümanı**: parçalar/temalar, zorluk ilkeleri, yöntem, kararlar, parça durumu (§13), açık konular (§15).
4. `crVersion.md` (repo kökü) — onaylanmış versiyonlar (v1, v2, v3, v3.1…), en yeni en üstte.
5. İşe göre: ilgili kod dosyaları (bkz. §6 kod haritası).
6. **Çalıştırarak doğrula (kullanıcı kuralı, her oturum):** Unity Editor kapalıyken (kilit: `Temp\UnityLockfile`) sırayla:
   - EditMode testleri: `Unity.exe -batchmode -nographics -projectPath <proje> -runTests -testPlatform EditMode -testResults Logs\<ad>.xml -logFile Logs\<ad>.log` → sonuç XML'deki `total/passed/failed`.
   - Bölümlerin kendi ölçümü: `-executeMethod Cloudora.Editor.V3LevelTool.RunCalibrationReportBatch` (oyundaki 1–15'i yeniden ölçer; `CLOUDORA_V3_CAL` satırları) ve gerekirse `RunCandidateReportBatch` (çerçeve dağılımları → `Logs/v3-candidate-summary.txt`).
   - Unity yolu: `C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Unity.exe`.
7. **Editor oyun testi (kullanıcı kuralı, her oturum, 2026-10-15):** Claude oynayamaz; kullanıcı oynar, Claude hazırlar ve log'dan doğrular.
   - Batch işleri bitince Unity Editor'ı projeyle aç (`Start-Process Unity.exe -ArgumentList '-projectPath', <proje>`); kaydı gerekirse yedekleyip ayarla (`%USERPROFILE%\AppData\LocalLow\YigitYvz\Cloudora\`).
   - Kısa rutin (5–10 dk): 2–3 bölüm; Undo, Extra, Shuffle birer kez; 1 boş bulutlu bölümde (19, 29…) başarısızlık + devam. Parça değiştiyse parçanın tamamı.
   - Sonra `Logs/Editor.log`'dan doğrula: `[Analytics]` satırları (level_started/completed, move_count, duration, undo/extra/shuffle, level_failed/continued, coins), exception var mı; bölüm verisiyle (en kısa çözüm) karşılaştır, tabloyu `ClaudoraRelease.md`'ye yaz.
   - Editor'ı kapatmadan build alma (proje kilidi); kod değiştirmeden önce Play'i durdur.

## 2. Çalışma kuralları (kullanıcı: Bahadır; Türkçe konuşur)
- Promptları uygula; **gerçek karar noktalarında onay iste.** Oyuncuya görünen metinler için 3–4 somut seçenek sun.
- **Her işlem** (küçük düzeltme ve önemli kontroller dahil) tarih-saatle (`Get-Date`) `ClaudoraRelease.md`'ye, en yeni en üstte, Türkçe yazılır. Kişisel yol yazma (`%USERPROFILE%` kullan).
- **Versiyon:** yalnızca kullanıcı onayıyla; `bahadir-claude-versions` dalında commit + etiket, `crVersion.md`'ye giriş. Eklemeler v3.2, v3.3…; bir sonraki major değişiklik v4. `ClaudoraRelease.md` her versiyonla birlikte commit'lenir.
- **Push: her seferinde ayrı, açık onay.** Önceki onay sonrakine geçmez. Hedefler: `upstream` (yigityvz/cloudora-android) `cloudora-bahadir` + `origin` (bahadir-aktas/cloudora-android) `main`. Force yok; takım reposunun `main`'ine dokunulmaz.
- Commit'ler değiştirilmez (amend/rebase yok). PowerShell 5.1'de çok satırlı mesaj için `git commit -F dosya`.
- Gördüğün her tutarsızlığı/çakışmayı (küçük de olsa) kullanıcıya söyle. İstisna: `paylasim/DEVRALMA.md` eski ve aktif kullanılmıyor — kaynak sayma, eskiliğini raporlama, dokunma.
- Kullanıcı testlerinde art arda hızlı basmalar kasıtlı buton testidir, zorluk göstergesi değildir.
- Geçmişte olanı anlatırken ne zaman yapıldığını açık yaz (bu oturumda yapılan işle karışmasın).
- Oyunda görünen metinde ASCII dışı sembol (`♥`, emoji…) kullanma; Android'de görünmez → sprite ikon (`CloudoraVisuals.Heart`).

## 3. Kaynaklar — ne nerede yazıyor
| Kaynak | İçerik | Durum |
|---|---|---|
| `Docs/V3_BOLUM_VE_ZORLUK_TASARIMI.md` | v3 tasarımı: desen, parçalar/temalar, yöntem, Shuffle/Restart/başarısızlık/coin kararları, modifier arşivi, test planı, parça durumu, açık konular | **Güncel, asıl referans** |
| `Docs/V3_SABLON.md` | 1–100 arka plan şablonu (Faz 1.2–1.4): ilkeler, yenilik envanteri, yerleşim, hedef eğrisi, bölüm bölüm şablon | Taslak, Faz 1 sürüyor |
| `Docs/V3_CERCEVE_ENVANTERI.md` | Faz 1.1 ölçümleri: 60 çerçevenin (kap. 4–6, 3–6 renk, 1–2 boş, klasik/serpiştirilmiş) puan, hamle, çıkmaz, takılma, No Moves tablosu + bulgular | Güncel (2026-10-15); puan ağırlıkları değişirse yeniden ölçülmeli |
| `ClaudoraRelease.md` | Her işlemin kaydı; test tabloları (insan hamle/süre verileri), kararların gerekçeleri | Güncel |
| `crVersion.md` | v1, v2, v3, v3.1 ayrıntıları (değişiklikler, doğrulama, bilinen sınırlar) | Güncel |
| `README.md` | Proje tanıtımı, katkıda bulunanlar, repo bağlantıları | 2026-10-15'te v3.1'e göre güncellendi |
| `Docs/GELISTIRME_ONERILERI.md` | v1 sonrası öneri listesi (D/G/F/O/Q/B maddeleri) ve eski versiyon planı | Öneri listesi; versiyon planı artık geçerli değil (v3 = zorluk çalışması oldu) |
| `Docs/PROJECT_PROGRESS.md` | Ana yazarın faz 0–15 geçmişi (İngilizce) | Tarihsel; Restart/Shuffle anlatımı v3.1 öncesine ait |
| `Docs/ANDROID_BUILD.md`, `GOOGLE_PLAY_SUBMISSION.md`, `RELEASE_CHECKLIST.md`, `PRIVACY_RELEASE_REVIEW.md` | Build, yayın, gizlilik süreçleri (ana yazar) | Tarihsel/yayın öncesi; yayın onayı yok |
| `StoreAssets/README.md`, `STORE_COPY.md` | Mağaza görselleri ve metin taslağı (EN/TR) | Taslak |
| claude.ai "Cloudora — Temalar, Bölümler ve Zorluk" (`https://claude.ai/artifact/2mwyMmgphECxmDmkSojPWH`, 14.10) | **Eski** 13 dünyalı yapı (1–400), eski zorluk dalgası, 5 tutarsızlık | v3 öncesi; tarihsel. PDF'i repo dışında: `..\Cloudora-Temalar-Bolumler-Zorluk.pdf` |
| claude.ai "Cloudora Buton Hakları" (`https://claude.ai/artifact/4wb1GpLBHCTZaGaYvV6rHj`, 13.10) | v2 rozet kararları (köşe rozeti, AD ipucu, Restart can rozeti) | Restart rozeti v3.1'de kalktı |
| `paylasim/DEVRALMA.md` | Eski devralma notu | **Kullanılmıyor, kaynak değil** |
| Hafıza dizini (`%USERPROFILE%\.claude\projects\…\memory\`) | Kullanıcı tercihleri ve kurallar | Her oturumda otomatik yüklenir |

## 4. Proje özeti
- Unity **6000.5.9f1**, Android, dikey, tek parmakla hava sıralama bulmacası. Ana yazar **yigityvz**; Bahadır katkıda bulunan (Unity geliştirme, Android build). Tek sahne `Assets/_Cloudora/Scenes/02_Gameplay.unity`; arayüzün tamamı kodla oluşturulur.
- **Dal:** `bahadir-claude-versions`. Etiketler: v1 (`adaa0b8`), v2 (`61088e4`), v3 (`b0b2f13`), v3.1 (`ae54acc`). v3/v3.1 2026-10-15 00:32'de iki repoya push edildi.
- **Telefon (Samsung Galaxy S24 FE):** "Cloudora" (v1), "Cloudora v2", "Cloudora v3" (`com.yigityvz.cloudora.v3`, v3.1 derlemesi) yan yana.

## 5. Oyun kuralları (kod: `Scripts/Core/PuzzleRules.cs`, `CloudState.cs`)
- Bulutlar alttan üste dizili hava parçası yığınları; kapasite 2–7 (1–15'te 4). Türler: Güneş, Yağmur, Kar, Rüzgar, Ay, Şimşek (+ Gökkuşağı joker).
- Hamle: kaynak → hedef. En üstteki aynı renkli grup, **sığdığı kadar otomatik** taşınır (miktar seçilmez). Hedef boş ya da üstü aynı renk olmalı (Gökkuşağı her şeyle eşleşir).
- Kazanma: boş olmayan her bulut dolu ve (Gökkuşağı hariç) tek renk.
- **Hiç legal hamle kalmazsa = bölüm başarısız** (v3.1). Seçenekler: Retry (−1 can; 1–5 ücretsiz, bölüm sıfırdan, ekstra bulutlar silinir) · reklam veya 40 coin ile **çözülebilen son duruma** dönüş. Hamle var ama çözüm yoksa başarısızlık tetiklenmez → Undo/Shuffle/Extra.
- Başarısızlık ekranındayken uygulama kapanırsa durum kayda yazılır (`FailRecord`), açılışta aynı ekran gelir.
- **Booster'lar:** Undo (başlangıç 3, reklamla +3), Extra Cloud (başlangıç 1; boş bulut ekler, bölüm başına en fazla 2; Undo geçmişini siler), Shuffle (başlangıç 1; **16'dan itibaren**; seçilen tek bulutu kendi içinde karıştırır, sonuç her zaman çözülebilir ve farklı; Undo geçmişini siler; tek renkli/tek elemanlı/boş/modifier'lı bulut seçilemez). Hak bitince reklam (dev build'de sahte reklam anında verir; release'te reklam yok).
- **Can:** en fazla 5, 30 dakikada 1 dolar. **Coin:** Orta +10, Zor +15, Çok zor +20 (101+ için +10), başlangıç 0.
- **Restart yok** (v3.1'de kaldırıldı). Geçici geliştirme butonu: sağ üstte "AD +1" (reklam → +1 can).
- **Modifier'lar** (kodda var, 1–100'de **kullanılmıyor**): Frozen, Locked, Fog, Night, Wind, Rainbow — bkz. tasarım §7.
- Ara reklam (interstitial): 5'ten sonra, her 4 tamamlamada bir, en az 5 dakika arayla.

## 6. Kod haritası (`Assets/_Cloudora/`)
| Yer | Ne |
|---|---|
| `Scripts/Core/` | Saf kurallar: `PuzzleRules`, `CloudState`, `PuzzleState`, `Move` |
| `Scripts/Puzzle/PuzzleController.cs` | Ana akış: bölüm yükleme, hamle, Undo, Extra, Shuffle, başarısızlık/devam, coin, kayıt |
| `Scripts/Puzzle/CloudContainerView.cs` | Bulut görünümü (kuralların görünüm tarafı kopyası burada da var) |
| `Scripts/Level/V3LevelPlan.cs` | Parçalar, desen, çerçeveler (`FrameFor`), bant yüzdelikleri, Shuffle açılış bölümü |
| `Scripts/Level/PuzzleSolver.cs` | Çözücü + zorluk ölçütleri + sanal oyuncu + puan ağırlıkları |
| `Scripts/Level/V3LevelData*.cs` | Seçilmiş bölümler (`.Generated.cs` araçla üretilir, elle değiştirilmez) |
| `Scripts/Level/AuthoredLevelCatalog.cs` | 1–15'i `V3LevelData`'dan kurar (öğretici yazılar burada) |
| `Scripts/Level/LevelGenerator.cs`, `BalanceProfile.cs`, `DifficultyCalculator.cs` | **Eski** üretici: henüz kurulmamış 16+ bölümler buradan gelir |
| `Scripts/Level/CloudShuffler.cs` | Bulut içi Shuffle + `FailRecovery` (çözülebilen son durum) |
| `Scripts/Progression/` | Can, coin, booster, ilerleme, `WorldCatalog` (dünya listesi) |
| `Scripts/Services/` | Kayıt (JSON + yedek), `FailRecord`, reklam (sahte/AdMob kapalı), analytics |
| `Scripts/Modifiers/` | Modifier kuralları ve çalışma zamanı |
| `Scripts/UI/` | `GameplayOverlay` (HUD, butonlar, başarısızlık ekranı), menü, dünya ekranı, yerleşim |
| `Editor/V3LevelTool.cs`, `V3LevelBuilder.cs`, `LevelCandidate*.cs` | **Bölüm aracı** (yalnızca Editor, APK'ya girmez) |
| `Editor/CloudoraBuildTools.cs`, `ContentValidationTool.cs` | Build menüleri, eski üretici için 10.000 bölüm kontrolü |
| `Tests/Editor/` | `LevelToolTests`, `V3LevelBuildTests`, `V31GameplayTests`, `PuzzleFeelTests` (son durum 96/96) |

## 7. Bölüm tasarımı (v3) — nasıl yapılır
**Plan (tasarım §2, §4):** Her 5 bölümde **Orta, Orta, Zor, Orta, Çok zor**; "Kolay" yok. Yani 3, 8, 13… Zor; 5, 10, 15… Çok zor (her parçanın sonu Çok zor). Etiketler parçadan parçaya aynı şeyi ifade etmez: her parçanın kendi ölçeği var, genel zorluk birikimli artar (ileride bir Orta baştaki bir Zor'a ulaşabilir), parça geçişlerinde sıçrama/düşüş yok, parça içinde aynı bant hafifçe zorlaşır, oyunun başında artış yavaş, oyuncunun alışması hesaba katılır.

**Şu anki puanlar (kalibrasyon raporuyla doğrulanır):**
| Bant | 1–5 | 6–15 |
|---|---|---|
| Orta | 10,38 · 11,55 · 11,91 | 15,13 → 16,80 (6 bölüm, artan) |
| Zor | 13,69 | 18,12 · 18,79 |
| Çok zor | 15,44 | 20,24 · 21,35 |
6–15'in en kolay Orta'sı (15,13) 1–5'in Zor'undan zor, 5. bölümden (15,44) zor değil → birikimli ama sıçramasız. 6–15'in 2. döngüsünde Zor/Çok zor'un Orta'lardan zayıf ayrışması kullanıcı kararıyla kabul edildi (tasarım §13).

| Parça | Bölümler | Geçici ad | Çerçeve / not | Durum |
|---|---|---|---|---|
| 1 | 1–5 | Tanıtım | kap. 4, 2 renk, 2 boş | Tamamlandı |
| 2 | 6–15 | Alışma | kap. 4, 3 renk, 2 boş | Tamamlandı |
| 3 | 16–30 | **Green Valley** (kesin, tasarımı sonra) | Kapasite 4 → 5, geçiş noktasını araç seçer; boş bulut kademeli 2 → 1; Shuffle 16'da; dünya listesi bu parçayla yeni aralıklara geçer (tasarım §11b–11d) | **Sıradaki (v3.2)**, onay bekliyor |
| 4 | 31–60 | Tema 2 | Belirlenmedi | — |
| 5 | 61–100 | Tema 3 | Belirlenmedi | — |
**1–100 faz planı (tasarım §11k–11l, 2026-10-15 16:04) — güncel yol haritası:** 1–100'de temalar yalnızca görsel; Faz 1 şablon → Faz 2 araç düzenlemesi → Faz 3 simülasyon → Faz 4 parça parça oyun testi → Faz 5 kapanış. Her adımdan önce/sonra kullanıcıya sor. Hangi adımda kalındığı işlem kaydının en üstünde. Durum (2026-10-15 20:50): Faz 1 bitti; Faz 2.1–2.3 bitti; 2.5 atlandı (kullanıcı); 2.6'nın acil kısmı yapıldı (oyun içi `PuzzleSolver.CheckSolvable`: Shuffle/No Moves dönüşü; telefonda 83 + 2 Extra donması düzeldi). 16–100 deneme verisi Editor'da ve telefonda test edildi, kullanıcı memnun; commit yok. Bölüm haritası `Docs/V3_BOLUM_HARITASI.html`, uygulanan plan `Docs/V3_SABLON.md`. Sıradaki: commit (onayla), sonra Faz 2.4 / Faz 3; açık: tahta ölçeği, final zorluğu (tasarım §15), dünya adları (Faz 5.1).
**v3.2 planı (tasarım §11e, ilk hali):** 1–15 sabit; hedef eğrisi dengeli, tabloyla onaylatılır, sonra üretilir; sıra renkler → araç → eğri → 16–30 üretimi + dünya listesi → baştan test. **Her adımdan önce kullanıcıya sor.**
**Çerçeve merdiveni (karar 15.10, tasarım §11c):** çerçeveyi araç puana göre seçecek; kapasite ve hava türü sayısı geri düşmez; yeni tür geldiğinde boş bulut geçici 2'ye dönüp kademeli 1'e iner. Türler 1–100'e kademeli yayılır (31–60: +Rüzgar, 61–100: +Ay, belki +Şimşek). Dağılımda değişiklik gerekirse kullanıcıya sorulur. Bir bölümde tek yenilik, yenilik ilk kez Orta'da. Renkler (v3.2, seçildi): Kar #7FE3F5, Rüzgar #AEB7C2, Şimşek #25358A + beyaz desen. **v3.2 testi kayıt sıfırlanıp 1. bölümden yapılacak** (renkler 1–15'i de etkiliyor).
Tema adları, tasarımları ve temaya özel özellikleri **henüz belirlenmedi.** 1–100'de şimdilik modifier yok. 101+ eski sistemle kalır.

**Yöntem (bölüm aracı, test önce):**
1. Karar tasarım dokümanına yazılır → çerçeve `V3LevelPlan.FrameFor`'a eklenir.
2. `V3LevelBuildTests`'e parça testi eklenir, parça `DesignedParts`'a girer; veri yokken testler başarısız olmalı.
3. Parça `V3LevelTool.PartsToBuild`'e eklenir → **Cloudora/V3/Bölüm Verisini Üret** (veya batch `GenerateLevelDataBatch`).
   - Her çerçeve için seed 1'den 400 aday, çözücü (en fazla 300.000 durum), çözümsüzler elenir.
   - Puan = en kısa hamle ×1 + tuzak oranı ×15 + yanıltıcılık ×10 + çıkmaz oranı ×5 + sanal oyuncu fazla hamlesi ×1,5 + sanal oyuncu takılma oranı ×10.
   - Bant aralıkları çerçevenin aday dağılımından: Orta %30–60, Zor %70–85, Çok zor %90–100 (çakışmasız).
   - Her bölüme aralıkta hedefe en yakın, parçada tekrar etmeyen, öğretici yazıyı gösteren tahta; aynı banttaki bölümler parça içinde hafifçe zorlaşır.
4. Testler: döngüde Orta < Zor < Çok zor; aynı bant kolaylaşmaz; aralıklar çakışmaz; yeni parçanın Orta'ları öncekinin Orta'larından zor ve ilk bölüm ≤ önceki parçanın sonu ×1,10; tahtalar tekrarsız; deterministik; veri dosyası araçla uyumlu (`GeneratedDataMatchesBuilder`). Mutasyonla (kodu bilerek bozma) testlerin yakaladığı doğrulanır.
5. Editor testi (kayıt yedeklenip sıfırlanır) → telefon testi ("Cloudora v3" ayrı paket olarak derlenip kurulur, logcat). Her bölüm için tablo: bant, en kısa çözüm, oyuncu hamlesi, fazla hamle, süre.
6. İnsan verisi ölçümle uyuşmazsa **Cloudora/V3/Kalibrasyon Raporu**, ağırlık/ölçüt ayarı, parça yeniden üretilir; cihaz verisi teste yazılır (`NaivePlayerAgreesWithDeviceTestData`). Kabul kararı kullanıcınındır. Sonra tasarım §13 ve işlem kaydı güncellenir.

**16+ için çerçeve merdiveni (v3.2, tasarım §11f):** `Editor/V3LadderBuilder.cs` — mutlak hedef puan eğrisi (her 5 bölümde +1,0), çerçeveyi ve yeniliklerin yerini araç seçer, testler `Tests/Editor/V32LadderTests.cs`. Önizleme menüsü/batch: `V3LevelTool.RunTransitionPreviewBatch` → `Logs/v3-preview-TransitionTheme.csv`. 1–15 eski yoldan (`V3LevelBuilder`), değişmez.
**Bilinen ölçüm gerçekleri:** 2 boş bulutta çıkmaz/tuzak oluşmaz (zorluk = uzunluk + yanıltıcılık); 1 boş bulutta tuzak ve çözümsüz tahtalar başlar. 1–5 çerçevesinde yalnızca 37 farklı tahta var. Çözücü modifier'ları bilmez (1–100 için sorun değil).

## 8. Build ve test notları
- Build öncesi Unity Editor kapalı olmalı (proje kilidi). Komut satırından:
  `GRADLE_USER_HOME=C:\Dev\gradle-cloudora`, `CLOUDORA_DEV_APP_ID=com.yigityvz.cloudora.v3`, `CLOUDORA_DEV_PRODUCT_NAME="Cloudora v3"`, `CLOUDORA_BUILD_OUTPUT=Builds/Android/Cloudora-v3-dev.apk` → `./Build-Android.ps1 -Mode dev`. Editor menüsünden derlenirse paket v1'in üzerine gider.
- Kurulum: `adb install -r` (yalnızca v3 paketi); kayıt sıfırlamak gerekirse `pm clear com.yigityvz.cloudora.v3`.
- Editor kaydı: `%USERPROFILE%\AppData\LocalLow\YigitYvz\Cloudora\cloudora-save.json` (+ `.bak`); sıfırlamadan önce `yedek-*` klasörüne yedekle.
- Play Mode'dayken kod değiştirme (sahte `NullReferenceException`'lar).
- Commit dışı bırakılanlar: `ProjectSettings.asset` (Unity format değişikliği), `.vscode/`.

## 9. Açık konular ve bilinen tutarsızlıklar
- **Açık (tasarım §14–15):** öğretici yazıların görünürlüğü (tutorial ileride), coin sayıları ve başlangıç coini, reklamla can almanın asıl yeri (şimdilik "AD +1"), menüden "bölümü baştan başlat", zorluk sembolü (sprite, parçalar bittikten sonra), tema adları/özellikleri, 101+.
- **16–30 kurulurken dikkat:**
  - Dünya kimliği iki yerde: `WorldCatalog` ve `LevelGenerator.WorldId` — birlikte güncellenmeli.
  - `BalanceProfile.BandFor` dünya kataloğuna bağlı → katalog değişince henüz kurulmamış 31–100'ün üretilen tahtaları da değişir.
  - Katalog değişince kırılacak testler: `WorldProgressionCrossesBoundaryAndRestoresInStages`, `DifficultyWaveHasFinaleAndEasyWorldReset`, `CatalogUsesV3BoardsAndKeepsTutorialFrame` ("green-valley"); modifier'lar 1–100'den kalkınca `GeneratorIntroducesExpectedModifier` (45/65/85).
  - 1 boş bulutla "hamle var ama çözüm yok" durumları çoğalır (başarısızlık tetiklenmez).
  - **Parça içinde çerçeve değişimi:** bant aralıkları her çerçeve için ayrı hesaplanır, ama "aynı bant parça içinde kolaylaşmaz" testi puanları çerçeveden bağımsız karşılaştırır. 1 boş bulutta tuzak/takılma puanı devreye girdiği için 2 → 1 geçişinde puan sıçrayabilir. 1 boş bulutun hangi bölümlerde ve ne sıklıkla geleceği tasarımda kullanıcıyla belirlenmeli.
- **İleride (kullanıcı):** araç "No Moves"a ulaşılabilirliği de tasarlasın (eski bölümlerde neredeyse imkânsızdı). Çözücüye "hiç hamle kalmayan durum" ölçütü eklenebilir. Tasarım §15.
- **Test notu:** kullanıcı buton denemelerini (hızlı art arda basmalar dahil) 1–15'te yapar; o bölümlerin hamle/süre verisi zorluk analizine katılmaz.
- **Zararsız kalıntılar:** `LevelCandidateSelector`'daki "Shuffle yedeği" (`backupCount`) ve ilgili test yorumu; `LevelDefinition.difficultyBand` (eski sistemle doluyor, okunmuyor); `BalanceProfile.DurationTarget` (yalnızca testte).

## 10. Bu dosyanın bakımı
Önemli bir karar, yeni bir kaynak doküman ya da parça tamamlanması olduğunda bu dosya da güncellenir ve değişiklik `ClaudoraRelease.md`'ye yazılır.
