# v3 — Bölüm ve Zorluk Tasarımı (1–100)

Kullanıcının (Bahadır) 2026-10-14'te verdiği tasarım tanımı ve soru-cevaplar. v3 çalışmasının referansıdır; kararlar netleştikçe güncellenir.

---

## 1. Amaç
- İlk 100 bölüm, zorlukları yeniden düzenlenerek **yeniden oluşturulacak**.
- **Oyunun yapısı ve mantığı değişmeyecek.** Değişen şey zorluğun bölümlere dağıtılması: eskiden fiilen rastgeleydi (bant sabit sırayla verilse de rastgele karıştırma yüzünden gerçek zorluk tutmuyordu), artık her bölümün zorluğu belirli olacak.
- v3 yalnızca bu zorluk çalışmasıdır; başka major geliştirme eklenmeyecek.

## 2. Zorluk deseni
Zorluk **her 5 bölümde bir** aynı sırayla tekrar eder. **"Kolay" yoktur; en düşük seviye "Orta"dır.**

| 5'li gruptaki sıra | 1. | 2. | 3. | 4. | 5. |
|---|---|---|---|---|---|
| Zorluk | Orta | Orta | Zor | Orta | Çok zor |

Döngüler: 1–5, 6–10, 11–15, 16–20, … (her döngünün son bölümü "Çok zor").

**Zorluk göreli ve birikimli:** Oyun ilerledikçe ve temalar değiştikçe her bant önceki bölümlerdekinden daha zor olur. Baştaki "Zor", ileride bir bölümde "Orta" seviyesine denk gelebilir. Baştaki "Orta" aslında o kadar zor değildir; kullanıcı sorunsuz geçebilir. (İleride ayrıca tutorial eklenecek; bu yüzden "Kolay" olmaması sorun değil.)

> **Her zaman hatırlanacak ilke (kullanıcı):** Oyun ilerledikçe zorluk etiketleri temalar arasında **aynı şeyi ifade etmez.** Her parçanın kendi Orta/Zor/Çok zor ölçeği vardır ve bu ölçekler **dengeli** kurgulanmalıdır:
> - Genel zorluk parçadan parçaya kademeli artar; parça geçişlerinde ani sıçrama veya düşüş olmaz (ör. yeni parçanın ilk "Orta"sı, öncekinin "Çok zor"undan belirgin şekilde zor olmaz).
> - Bir parçanın "Orta"sı, önceki parçaların "Orta"sından zordur; ileride bir "Orta", baştaki bir "Zor" seviyesine ulaşabilir.
> - Parça içinde de hafif bir artış olabilir; desen (Orta, Orta, Zor, Orta, Çok zor) her yerde korunur.
> - **Alışma etkisi (kullanıcı notu, 2026-10-14):** Oyuncular oynadıkça alışır; aynı ölçülen zorluk ilerleyen bölümlerde daha kolay hissettirir. Parçalar arası artış bu alışmayı da hesaba katmalı.
> - **Artış hızı (kullanıcı, 2026-10-14):** Oyunun başında radikal zorluk artışı olmasın; ileriki parçalarda yavaş yavaş artışlar konur, parçaların kendi içinde de zorlaşır.

## 3. Yöntem (kullanıcı seçimi)
**Tablo + ölçülmüş, seçilmiş seed:**
- Her bölüm için parametreler (kapasite, hava türü sayısı, boş bulut, hedef zorluk) bir tabloda tanımlanır.
- Editor aracı her bölüm için çok sayıda aday tahta üretir; bir çözücü her adayın gerçek zorluğunu ölçer (en kısa çözüm uzunluğu, çıkmaza götüren hamleler vb.).
- Hedef zorluğa en uygun adayın seed'i o bölüme sabitlenir.
- **Zorluk ölçütlerini Claude belirler**, kullanıcı test eder; geri bildirime göre revize edilir.

### Araç (2026-10-14, testli)
- Kod (oyunla birlikte derlenen): `Scripts/Level/PuzzleSolver.cs`, `CloudShuffler.cs`, `DeterministicRandom.cs`, `BoardFrame.cs`, `V3LevelPlan.cs`, `V3BoardText.cs`, `V3LevelData*.cs`. Yalnızca Editor (APK'ya girmez, 2026-10-15'te taşındı): `Editor/LevelCandidateFactory.cs`, `Editor/LevelCandidateSelector.cs`, `Editor/V3LevelBuilder.cs`, `Editor/V3LevelTool.cs`; testler `Tests/Editor/LevelToolTests.cs`.
- Ölçütler: en kısa çözüm (hamle), ulaşılabilir durum sayısı, çıkmaz durum oranı, en kısa yollar üzerinde ilerleten hamle oranı ve tuzak (çıkmaza götüren) hamle oranı, ortalama seçenek; bunlardan birleşik **puan** (ağırlıklar ilk ayar, testlerle kalibre edilecek).
- Sınırlar: modifier'sız tahtalar (1–100 kararıyla uyumlu), en fazla 12 bulut; çok büyük tahtalar durum sınırını aşarsa "ölçülemedi" sayılır ve seçilmez.
- İlk rapor bulguları: 2 boş bulutlu çerçevelerde çıkmaz/tuzak hamle oluşmuyor (zorluk yalnızca çözüm uzunluğu ve yanıltıcılıktan); 1 boş bulutta tuzaklar ve çözümsüz tahtalar ortaya çıkıyor (çözümsüzler araçta elenir). 1–5 çerçevesinde (kapasite 4, 2 renk, 2 boş) yalnızca 37 farklı tahta var; çözüm uzunluğu 3–7 hamle.

## 4. Bölüm aralıkları ve parçalar
İlk 100 bölüm **parça parça** oluşturulacak; parçalar aralıklara göre ayrılır. Hangi bölümün hangi aralıkta olduğu şimdiden sabit; tema adları, tasarımları ve temaya özel özellikler sonradan eklenecek (adlandırmalar geçici).

| Parça | Bölümler | Geçici ad | Amaç |
|---|---|---|---|
| 1 | 1–5 | Tanıtım | Oyunun tanıtımı; zorlukların gösterilmesi. Tema sayılmaz. |
| 2 | 6–15 | Alışma | Kullanıcının iyice alışması; tutorial'ın tamamlanması. Tema sayılmaz. |
| 3 | 16–30 | Geçiş teması | İlk temanın gelişi; temaları tanıtır ve öğretir. **Shuffle 16'da açılır.** |
| 4 | 31–60 | Tema 2 | İlk gerçek tema; ona göre özellikler (belirlenmedi). |
| 5 | 61–100 | Tema 3 | İkinci tema; ona göre özellikler (belirlenmedi). |

- Dünya kataloğu teknik olarak her bölümün bir dünyaya ait olmasını ister: 1–15 için "Tanıtım" adlı geçici bir kayıt kullanılır (oyuncuya tema olarak sunulmaz), diğerleri yukarıdaki geçici adlarla.
- Her aralık 5'in katında bittiği için her parçanın son bölümü "Çok zor"dur.

## 5. Korunacak mevcut davranışlar
- **L1–5'te Restart can götürmez** (şu anki kural, kalır).
- **Shuffle 16. bölümde açılır** (şu anki gibi).
- Extra Cloud seviye başına en fazla 2, No Moves, can sistemi vb. mevcut kurallar değişmez.
- Çakışma çıkarsa kullanıcıya sorulur.

## 6. Shuffle — yeni tasarım: bulut içi karıştırma (karar, 2026-10-14)
- **Neden:** Kullanıcıya göre ana yazarın asıl amacı bir bulutu kendi içinde karıştırmaktı. Kodda (orijinal `a18a3e4` dahil) Shuffle bütün tahtayı yeni seed'le yeniden üretiyordu; bu karışıklık v2 ve v3 kararlarına taşındı. Yeni tasarımla değiştirilecek.
- **Davranış:**
  1. Shuffle'a basılınca oyun bir bulut seçilmesini ister.
  2. Shuffle'a basıldığında zaten seçili bir bulut varsa o seçim kaldırılır.
  3. Seçilen bulut kendi içinde karıştırılır; oyun aynı tahtayla devam eder (bulut sayısı ve diğer bulutlar değişmez).
- **Shuffle 16. bölümde açılır** (§5, değişmedi).
- **Geçersiz kalan eski kararlar:** her bölüm için yedek seed (bölüm tahtası artık değişmiyor), "Shuffle'da ekstra bulutlar boş kalsın" ve "Shuffle ekstra bulutlara da dağıtır" (v2), Shuffle sayısını yedek seed sayısıyla sınırlama notu. İlgili kodlar kaldırılacak/geri alınacak.
- **Kurallar (kullanıcı yanıtları, 2026-10-14):**
  1. **Güvenli:** Karıştırma sonucu tahta mutlaka çözülebilir kalır (çıkmaza sokan sonuç kabul edilmez) ve bulutun sırası mutlaka değişir (oyuncu hakkının karşılığını almalı).
  2. **Seçilemeyen bulutlar:** boş, tek renkli, tek elemanlı (karıştırılınca değişmeyenler) seçilemez; hak düşmez.
  3. **Hak ve iptal:** Hak, bulut seçilip karıştırma yapılınca düşer; seçim modundayken Shuffle'a tekrar basmak veya boş yere dokunmak iptal eder; ekranda "bir bulut seç" yazısı çıkar.
  4. **Undo:** Shuffle Undo ile geri alınamaz. Shuffle'dan sonra Undo geçmişi temizlenir ve Undo butonu söner (eski Shuffle'daki gibi); sonraki hamleler yine geri alınabilir.
  5. **Açılma koşulu:** bölüm ≥ 16 (önceki "üretilmiş bölüm" koşulu yerine; Shuffle olmayan bölümler karışmasın, kod daha temiz).
  6. **Özel bulutlar:** Modifier'lı bulutlar (Frozen, Locked vb.; 101+) Shuffle'dan etkilenmez — dokunulmaz.
  - Restart'la ilgili sorular (Restart'ın döneceği tahta, karıştırmanın hamle sayılması) kullanıcı Restart'ı kaldırmayı düşündüğü için beklemede.

## 6b. v3.1: Restart kalkıyor, "No Moves = başarısızlık", coin (karar, 2026-10-14)
- **Sürüm:** Shuffle'ın yeniden tasarımı (§6), Restart'ın kaldırılması, başarısızlık ekranı ve coin **v3.1** olarak şimdi yapılır ve commit'lenir; push kullanıcı onayıyla, vakti gelince.
- **Restart:** Restart ve ona bağlı, başka şeyi etkilemeyen bütün özellikler kaldırılır (buton, can rozeti, ilgili mesajlar, Restart'ın ekstra bulutları koruması, `level_restarted`). **Can sistemi kalır.**
- **No Moves = bölüm başarısız** (tetikleyici değişmez: hiç legal hamle kalmaması). Bu ekranda Undo / Extra Cloud / Shuffle sunulmaz. Üç seçenek:
  1. **Can:** 1 can kaybederek bölüme sıfırdan başlamak (ekstra bulutlar silinir).
  2. **Reklam:** reklam izleyerek devam.
  3. **Coin:** coin vererek devam.
  - "Devam" = **çözüme ulaşılabilen son duruma** dönmek (gerekirse birkaç hamle geri); orada en az bir doğru yol mutlaka vardır.
- **Hamle yapılabiliyor ama kazanılamıyorsa** (çıkmaz, No Moves tetiklenmez): oyuncu Undo / Shuffle / Extra Cloud kullanabilir veya ileride eklenecek "menüden bölümü baştan başlat" seçeneğini kullanır. Not: Hakları ve reklamı olmayan oyuncu, menü seçeneği eklenene kadar bu durumda takılabilir.
- **1–5 (tutorial):** Bölüme başlamak ve "can ile baştan başlamak" can götürmez.
- **Reklamla can alma:** başka bir yere taşınacak (ileride). Restart üzerinden olan yol kalkar.
- **Coin (ilk ayar, sonra düzenlenecek):** bölüm sonunda kazanılır; başlangıç coini ileride tutorial görevinde verilecek (şimdilik 0).
- **Buton düzeni:** üst sıra Extra · Undo · Shuffle; alt sıra World (Extra ile Undo'nun tam ortası) · Menu (Undo ile Shuffle'ın tam ortası); satırlar hizalı.

## 7. Modifier'lar
- **İlk 100 bölümde şimdilik modifier olmayacak** (daha stabil çalışmak için). Temaya özel özellikler belirlenince eklenecek.
- Mevcut modifier kuralları kodda kalır (`Modifiers/`), yalnızca 1–100'e atanmaz. Eski atamaların arşivi:

| Eski aralık | Eski dünya | Modifier | Parametre (`LevelGenerator.ModifiersFor`) |
|---|---|---|---|
| 21–40 | Rainy Coast | (ekranda "Wind", ama atanmıyordu) | — |
| 41–60 | Frost Peaks | Frozen | son bulut, `counter = 2`; ayrıca bu aralıkta boş bulut +1 ve son bulut geri-karıştırmaya katılmaz |
| 61–80 | Sun Desert | Locked | son bulut, `parameter = 0`; boş bulut +1, son bulut karıştırmaya katılmaz |
| 81–100 | Storm Islands | Fog | ilk bulut, `counter = 1` |
| 101–150 | Blossom Land / Ember Valley | Rainbow | ilk bulut; ayrıca ilk bulutun en üst slotuna 1 Rainbow parçası |
| 151–175 | Moon Garden | Night | ilk bulut |
| 176–200 | Sky Kingdom | Wind | ilk bulut |
| 201+ | Ocean → Celestial | Karma | çift → Fog, tek → Night (`counter = 1`); 3'e bölünen → Rainbow, diğer → Wind (bulut 1) |

Eski kapasite/tür eşikleri (arşiv): kapasite ≤20: 4, ≤60: 5, ≤125: 6, sonrası 7; hava türü sayısı `3 + (bölüm−16)/35` (51'de 4, 86'da 5, 121'de 6); boş bulut 2, sonu 9 olanlarda 1.

## 8. 101 ve sonrası
Şimdilik odak ilk 100 bölüm; 101+ mevcut sistemle kalır, ileride ayrıca ele alınacak.

## 9. Zorluk sembolü (kullanıcı önerisi)
- Bölümün zorluğunu gösteren işaret/sembol **v3'e dahil**, ancak bölümlerin zorluk çalışması tamamlandıktan sonra yapılacak.
- Unicode sembol değil, sprite ikon kullanılmalı (Android'de yerleşik fontta görünmüyor — bkz. `CloudoraVisuals.Heart`).

## 10. Çalışma ve sürüm
- Ayrı dal yok: çalışma doğrudan `bahadir-claude-versions` üzerinde.
- Unity ve telefon testlerinin tamamı başarıyla geçerse **v3** olarak commit + etiket; push yine kullanıcıya sorularak (iki hedef: `upstream cloudora-bahadir` + `origin main`).

## 11. İlk 15 bölüm (karar: aynı çerçevede tahtaları yeniden seçmek)
- Kurallar ve çerçeve aynı kalır: elle tasarlanmış bölüm formatı, kapasite 4, renk sayısı (1–5: 2, 6–15: 3), öğretici yazılar ve bölüm numaraları, tutorial kayıtları, Restart/Shuffle kuralları.
- Değişen yalnızca tahtaların içeriği: kurallar aynı tutularak çok sayıda aday denenir, zorlukları ölçülür ve desene uyanlar seçilir.
- Seçilen tahtanın, öğretici yazının gösterdiği şeyi gerçekten içerdiği de kontrol edilir (ör. 3. bölümde grup hareketi).
- **Boş bulut (karar, 2026-10-14):** 1–15'te hep 2 boş bulut (çerçeveye dahil). 1 boş bulut kademeli olarak geçiş temasında (16+) gelir. Bu yüzden 1–15'te zorluk farkı çözüm uzunluğu ve yanıltıcı hamlelerden gelir (2 boş bulutta çıkmaz oluşmaz); kabul edildi.

## 12. Test planı
| Parça | Unity testi | Telefon testi |
|---|---|---|
| 1–5 (Tanıtım) | Parça bitince | Parça bitince |
| 6–15 (Alışma) | Parça bitince | Parça bitince |
| 16–30 (Geçiş teması) | Parça bitince | Parça bitince |
| 31–60 (Tema 2) | Parça bitince | Parça bitince, ama tüm bölümler bitirilmez |
| 61–100 (Tema 3) | Parça bitince | Önceki testte kalınan yerden devam, birkaç bölüm test edilir |

Test geri bildirimine göre zorluk ölçütleri revize edilir; ardından sonraki parçaya geçilir.

## 13. Parça durumu
| Parça | Durum | Not |
|---|---|---|
| 1–5 Tanıtım | **Tamamlandı** (2026-10-14): Unity + telefon testleri geçti | Bant aralıkları: Orta %30–60, Zor %70–85, Çok zor %90–100 (çerçevenin aday dağılımında). Sanal oyuncu ölçümüyle kalibre edilip yeniden seçildi (L1, L3, L4 değişti). İnsan verisi: Orta'larda 0–1, Zor/Çok zor'da 1–2 fazla hamle. |
| 6–15 Alışma | **Tamamlandı** (2026-10-14): Unity + telefon testleri geçti | Çerçeve: kapasite 4, 3 renk, 2 boş; varsayılan bant dilimleri. İnsan verisi (2 test ort.): Orta 0,9 < Zor 1,5 < Çok zor 1,75 fazla hamle; 2. döngüde Zor/Çok zor zayıf ayrışıyor — kullanıcı kararıyla kabul: oyunun başı, radikal artış istenmiyor, ikinci çözümün (ezber) etkisi büyük. |
| 16–30, 31–60, 61–100 | Tasarlanmadı | |

## 14. İleride eklenecek özellikler (v3 kapsamı dışında)
- **Coin sistemi (kullanıcı, 2026-10-14):** Oyuna coin sistemi eklenecek. Detaylar (nasıl kazanılacağı, nereye harcanacağı, booster/can/reklam ekonomisiyle ilişkisi) henüz belirlenmedi. İlgili: `GELISTIRME_ONERILERI.md` G3 (booster ekonomisi). Not: Coin ile alınan booster'lar (Extra Cloud, Shuffle, Undo) bölüm zorluğunu etkiler; tasarlanırken v3 zorluk dengesi göz önünde tutulmalı.

## 15. Açık konular
- **Kapat-aç açığı — karar (a), uygulandı (2026-10-15):** "No Moves" ekranındayken uygulama kapanırsa başarısızlık kayda yazılır (bölüm, tahta — ekstra bulutlar dahil, Undo geçmişi + modifier durumu, hamle sayısı; `FailRecord`). Açılışta aynı ekran aynı seçeneklerle geri gelir; "devam" çözülebilen son duruma yine döner. Kayıt başka bölüme aitse veya bozuksa yok sayılır. Retry/devam seçilince kayıt temizlenir. Not: Başarısız olmadan bölüm ortasında kapatılırsa bölüm hâlâ sıfırdan başlar (orijinal davranış).
- **Geçici geliştirme butonu:** sağ üstte "AD +1" (reklam izle → +1 can); ileride yeri/şekli değişecek, reklamla can alma asıl yerine taşınacak.
- **Öğretici yazılar fark edilmiyor (kullanıcı, 2026-10-14):** 1–5 testlerinde L1–L5'teki yazılar ("Tap a cloud", "Groups move together"…) hiç dikkat çekmedi, göze çarpmadı. Tutorial ileride ayrıca ele alınacak; o zaman yazıların görünürlüğü (konum, boyut, vurgu) düzeltilmeli. v3'te dokunulmadı.
- Shuffle sayısı sınırı (bkz. §6).
- Temaların adları, tasarımları ve özellikleri.
