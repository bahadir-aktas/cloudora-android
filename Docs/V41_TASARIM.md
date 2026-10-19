# v4.1 — Bölümlerin yeni zorlukla yeniden kurulması

> **Durum:** son hal, 2026-10-19 23:00 (v4.1 commit'i). Taban v4 (`0604618`). Kararlar kullanıcının (Bahadır) 2026-10-19 17:28–22:57 arası cevaplarından; ayrıntılı konuşma ve gerekçeler `ClaudoraRelease.md` (17:28 → 23:00). Görsel özet: `Docs/V41_BOLUM_HARITASI.html`.
> **Sürüm:** v4.1; v5 değil (kullanıcı).

## 1. Amaç
- Oyun genel olarak daha zor ("şu an fazla kolay, sonlardaki bölümler bile").
- Zorluk hamle sayısından çok **tuzaktan** gelsin: oyun bitmez ama tıkanır, oyuncu özellik (Geri Al / Karıştır / Ek Bulut) kullanmak zorunda kalır → düşünerek oynamaya sevk.
- **Düşünerek oynayan geçebilmeli, düşünmeden oynayanın takılma ihtimali yüksek olmalı.** Zor bölümlerde dikkatli oyuncu da arada zorlanmalı (test 2'den sonra).

## 2. Oyun kuralları (v4'e göre değişenler)
| Konu | Karar |
|---|---|
| Başarısızlık | Eski kural: yalnızca hiç legal hamle kalmayınca (tamamlanmış hazne hamle sayılmaz); analytics `no_moves` |
| Tamamlanmış hazne kilidi | Kalır; araç da kilitli kuralla ölçer |
| Başarısızlık ekranı | Aynı; açıklama "Yapılacak hamle kalmadı. Baştan dene veya son güvenli hamlene dön." (4 seçenekten) |
| Reklam düğmesi | "Reklam İzle ve Devam Et" (4 seçenekten) |
| Karıştır | 15'te açılır; 15'in yazısı "Yeni: Karıştır bir hazneyi karıştırır." (4 seçenekten; ilk metin iki satıra taşıyordu) |
| Öğretici yazı | 16 punto, beyaz, tek satır (sığmazsa 12'ye küçülür); yazılı bölümde tahta yazının altından başlar (iOS'tan gelen yerleşim hatası: 2 sıralı tahta yazının üstüne biniyordu) |
| Kurtuluş ekranı can sayacı | Güncelleniyor (iOS'tan gelen hata) |
| Değişmeyenler | "Can Kazan · +1" (ana sayfa can 0, duraklatma), 1–5 dahil Ana Sayfa 1 can, ekonomi 100/200/300 – 500, rüzgâr mint, Türkçe arayüz, dünyalar (tanıtım 1–14, Yeşil Vadi 15–30, Rainy Coast 31–60, Frost Peaks 61–100) |

## 3. Bölüm yapısı
- **Desen:** her 5 bölümde Orta, Orta, Zor, Orta, Çok zor. Her döngüde Orta < Zor < Çok zor; aynı bant parça içinde en çok 1,0 geri düşebilir (takılma sınıfları karıştığı için).
- **Parçalar dünyalara göre:** 1–5 (2 renk), 6–14 (3 renk; kap. 4, 2 boş, daha zor tahtalar), 15–30 Yeşil Vadi, 31–60 Rainy Coast, 61–100 Frost Peaks.
- **Yenilikler (bir bölümde tek):** Kar 6, Karıştır 15, kapasite 5 **16** (Claude, kullanıcının bıraktığı karar: kap. 4 + 3 renk ~19,5 puanda tıkanır), 1 boş **20** (kullanıcı), Rüzgâr 31 (dünya başı), **Ay 61** (Frost Peaks ile; temayla yeni renk istisna kabul), kapasite 6 **81**. Final 60 kalır.
- **Nefes bölümleri** (16, 31, 61, 81): sıfır takılma, dağınık dizilim olabilir.
- **Dağınık dizilim:** mümkün olduğunca az (nefes bölümleri; orta takılmada cezalı).
- **Finaller (30, 60, 100):** sınav; takılma %60–80. 100 özel: 1 boşlu, gevşetilmez, düşünen oyuncu %10–25, hedef 44.

## 4. Ölçü
- Kilitli kural (oyunla aynı). Puan = en kısa ×1 + tuzak hamle ×15 + yanıltıcılık ×10 + çıkmaz ×20 + fazla hamle ×1,5 + **takılma ×15** (×10'du; kullanıcı: "tuzağa çok abanmasın, hamle de kıymetli olsun").
- **Takılma** = düşünmeden oynayan sanal oyuncunun ("mantıklı görünen hamle") 200 oyunda çıkmaza düşme oranı. Çıkmaza düşmeden çok uzun oynamak ("dolaşma") ayrı ölçülür.
- **Düşünen oyuncular:** aynı tercihlerle oynar, ama bir hamleden sonra N hamle ileriye bakar; o kadar içinde bütün devamları görülüp çözüme gitmeyen ("kapanan") çıkmaza giden hamleyi seçmez. **3 hamle** (`ThinkingFailureRate`, kullanıcı onayı) ve **2 hamle** (`ShallowThinkingFailureRate`; tuzağın derinliğini ölçer).
- Not (hata, 2026-10-19 21:35): onaylanan 3 hamle derinliği koda geç girdi; 1. ve 2. Editor testlerindeki veri 2 hamleyle seçilmişti.

## 5. Takılma deseni ve aralıklar
- 1 boş açıldıktan sonra (21+) her 5'li döngüde **2 yüksek, 2 orta, 1 sıfır**; yerler tohumla rastgele, sıfır Orta'da daha olası, Çok zor'da olamaz. İlk tercih tutmazsa (ör. kap. 6'da yüksek takılmalı Orta, Zor'dan zor çıkıyor) kurucu döngünün başka geçerli dağılımını seçer; dağılım ancak her bölüm hedefe ≤ 2,0 yakınsa kabul.
- Aralıklar (düşünmeden oynayan): yüksek %35→40 … %68→78, orta %12→20 … %35→45, sıfır ≤ %3, final %60–80.
- 3 hamle ileri bakan ≤ %10 (finaller ≤ %20, 100: %10–25).
- **Derin tuzak (öneri 3, kullanıcı "deneyelim"):** 31–60 yüksek takılma ve final 60: 2 hamle ileri bakan ≥ %10 (tuzak 3 hamle düşünmeyi gerektirir). 61–100 yüksek takılma: 3 hamle ileri bakan %5–15 (dikkatli oyuncu da arada düşer). 15–30: şart yok — 3 renkli küçük tahtada derin tuzak = erken No Moves (simülasyon: uygun final adayı 0).

## 6. No Moves
- 1 boşlu her bölüm No Moves'a çıkabilmeli; 2 boşlu (4+ renk) bölümlerde de olabilir.
- **En erken 4. hamle** (kullanıcı: 3 → 5 → 4; "3 spamlayan için ani olabilir, ama 4 okey").

## 7. Puan eğrisi (uygulanan; ölçülen çerçeve tavanlarına göre)
| Bölümler | Orta | Zor | Çok zor |
|---|---|---|---|
| 1–5 | 11,5 → 12,5 | 13,5 | 14,5 |
| 6–14 | 15,5 → 17 | 17,5 → 18,5 | 19 |
| 15–30 | 18 → 22,5 | 20,5 → 24,5 | 20 → 26, final 30: 29 |
| 31–60 | 23 → 28 | 26 → 31 | 28 → 33, final 60: 35 |
| 61–100 | 28 → 36 | 31 → 39 | 33 → 41, final 100: 44 |

## 8. Sonuç (veri = `Logs/v41-preview-4.csv`)
- 85 bölüm hedefe ±1 (en büyük sapma 1,91). Gevşetilen 6 (64, 74, 84, 89, 94, 99; 5 renk). No Moves'lu 47 (en erken 4. hamle). 1 boşlu 40. Dağınık 6 (16, 31, 61, 81, 82, 86).
- Yüksek/final ortalaması (düşünmeyen / 2 hamle / 3 hamle): 15–30 %49 / %0 / %0 · 31–60 %51 / %33 / %0 · 61–100 %52 / %20 / %6.
- 100: kap. 6, 5 renk, 1 boş; %78,5 / %23,5 / %15; 23 hamle; puan 42,5 (en zor).
- Havuzlar: 2 boş 400; 1 boş 3–4 renk 20.000; 5 renk 1 boş 100.000; 100 için ayrı 300.000. Kurulum ~6 dk.

## 9. Oyun testleri (2026-10-19)
| Tur | Veri | Bölümler | Özet |
|---|---|---|---|
| Editor 1 | derinlik 2, No Moves 3 | 15–100'den 13 | Çoğu en kısa/+1; erken No Moves'ta takılma (25: 3×, 50, 60); 100, 90'dan kolay → No Moves 5, 100 güçlendir |
| Editor 2 | derinlik 2, No Moves 5 | 20, 25, 50, 60, 79, 95, 100 | Hepsi yardımsız, "kolay" → oyun dikkatli oyuncuya fazla rahat |
| Editor 3 | **öneri 3** | 30, 50, 60, 79, 95, 100 | Süreler uzadı; 79'da No Moves + Yeniden Başlat; "düşünmeye itti, beğendim"; 100 hâlâ "kolay" (gevşetilmiş 2 boşlu) |
| Editor 4 | 100 güçlendirildi | 100 | 23/23, 57 sn; "tek yanlış hamlede fail olacakmış gibi" |
| Telefon | son veri (QA APK) | 1, 15, 20, 30, 61, 81, 100 | Çökme yok; 81'de 2 Ek Bulut + 3 Karıştır sorunsuz; 30 "zorlandım, güzel"; 100 "zorlandım, doğru tercihleri yapmazsan tuzakta kalıyorsun". Başarısızlık ekranı görülmedi |

## 10. Ölçüm notları (2026-10-19)
- Envanterler: `Logs/v41-envanter-derinlik2.txt` / `-derinlik3.txt`; simülasyonlar `Logs/v41-preview-nm4*.csv`, `-b2*.csv`; teşhis `Logs/b2-diag*.log`.
- Çerçeve tavanları (p90, eski ağırlıkla): kap.4 2 renk 15,1 · kap.4 3 renk 19,5 · kap.5 3 renk 2 boş 22,6 / 1 boş 31,5 · 4 renk 2 boş 28,2 / 1 boş 35,8 · 5 renk 2 boş 35,3 / 1 boş 41 · kap.6 5 renk 2 boş 43,9.
- No Moves ≥ 4 ve derin tuzak şartı küçük tahtada çelişiyor: 3 renk 1 boşta No Moves ≥ 4 olan final adayı 45 → 2 hamle ileri bakanın düştüğü 0.

## 11. İleride (kullanıcı fikirleri, şimdilik yapılmıyor)
- **Çıkmazı fark ettirme:** oyuncu çözemediğini anlamakta zorlanabilir ("şimdilik kalsın, sevdim"); ileride birkaç hamle ilerleme yoksa o an mantıklı olan çözüm (Geri Al / Ek Bulut / Karıştır) parlasın.
- **Hamle kaydı:** oyun hamleleri tek tek loga yazmıyor; yazarsa insan oyunu sanal oyuncularla hamle hamle karşılaştırılabilir.
- Gevşetilen 6 bölüm için daha büyük havuzlar; ekip APK'sının v4.1'e güncellenmesi; temalar ve dünya resimleri.
