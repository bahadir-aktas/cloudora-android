using System;
using System.Collections.Generic;
using System.Linq;
using Cloudora.Core;
using Cloudora.Level;
using Cloudora.Puzzle;
using NUnit.Framework;

namespace Cloudora.Tests.Editor
{
    // v4.1 bölüm kurulumu (V41LevelBuilder, Docs/V41_TASARIM.md): 1–100'ün planı, seçilen tahtalar ve oyuna bağlanması.
    // İlk test 100 bölümü baştan kurar (aday havuzları ~3–4 dk); varsayılan 3 dk sınırı yetmez.
    [Timeout(900000)]
    public sealed class V3LevelBuildTests
    {
        // 1–15'in öğretici yazıları ve çerçevesi (kap. 4, 2 boş).
        private static readonly string[] AuthoredCues =
        {
            "Bir hazneye dokun.", "Aynı hava türünün olduğu hazneyi seç.", "Aynı türden parçalar birlikte akar.", "Boş hazneler sana yer açar.", "Gökyüzünü dengele.",
            null, null, null, null, "Geri Al ile son hamleni geri çevirebilirsin.", null, null, null, null, "Yeni: Karıştır bir hazneyi karıştırır."
        };

        private static readonly V3Part[] AllParts = { V3Part.Intro, V3Part.Adaptation, V3Part.GreenValley, V3Part.RainyCoast, V3Part.FrostPeaks };
        private static IReadOnlyList<V3LevelResult> _built;
        private static IReadOnlyList<V3LevelResult> Built => _built ??= V41LevelBuilder.Build();
        private static V3LevelResult At(int level) => Built[level - 1];

        // ---------- Plan ----------

        [Test]
        public void BuildCoversOneToHundredWithTheFivePattern()
        {
            Assert.That(Built.Select(r => r.Level), Is.EqualTo(Enumerable.Range(1, 100)));
            foreach (V3LevelResult r in Built)
            {
                Assert.That(r.Band, Is.EqualTo(V3LevelPlan.BandFor(r.Level)), $"level {r.Level}");
                Assert.That(r.Primary.Result.Status, Is.EqualTo(SolveStatus.Solved), $"level {r.Level}");
            }
        }

        [Test]
        public void FramesFollowThePlan()
        {
            // 1–5 iki renk, 6–30 üç, 31–60 dört (Rüzgâr 31), 61–100 beş (Ay temayla 61'de); kap. 5 16'da, kap. 6 81'de;
            // 1 boş 20'den önce yok, 20'de gelir; 1–15 klasik dizilim.
            foreach (V3LevelResult r in Built)
            {
                int level = r.Level;
                Assert.That(r.Frame.ColorCount, Is.EqualTo(level <= 5 ? 2 : level <= 30 ? 3 : level <= 60 ? 4 : 5), $"level {level}");
                Assert.That(r.Frame.Capacity, Is.EqualTo(level < 16 ? 4 : level < 81 ? 5 : 6), $"level {level}");
                if (level < 20) Assert.That(r.Frame.EmptyCount, Is.EqualTo(2), $"level {level}");
                if (level <= 15) Assert.That(r.Frame.IsPacked, Is.True, $"level {level}");
            }
            Assert.That(At(20).Frame.EmptyCount, Is.EqualTo(1), "single empty arrives at 20 (user 18:43)");
            Assert.That(At(20).Novelty, Is.EqualTo(V3Novelty.FewerEmpty));
            Assert.That(At(15).Novelty, Is.EqualTo(V3Novelty.Shuffle));
            Assert.That(At(61).Novelty, Is.EqualTo(V3Novelty.NewColor), "Moon arrives with the Frost Peaks theme");
            Assert.That(At(81).Novelty, Is.EqualTo(V3Novelty.LargerCapacity));
        }

        [Test]
        public void ScatteredLayoutsOnlyWhereTheyLetThePlayerBreathe()
        {
            // Dağınık dizilim mümkün olduğunca az (kullanıcı 18:16): yalnızca nefes bölümlerinde ve orta takılmada.
            foreach (V3LevelResult r in Built.Where(r => !r.Frame.IsPacked))
                Assert.That(V41LevelBuilder.IsBreath(r.Level) || r.Trap == TrapClass.Medium, Is.True, $"level {r.Level} {r.Frame} {r.Trap}");
        }

        [Test]
        public void BoardsAreDistinct()
        {
            List<string> keys = Built.Select(r => r.Primary.CanonicalKey).ToList();
            Assert.That(keys.Distinct().Count(), Is.EqualTo(keys.Count));
        }

        [Test]
        public void HarderBandsScoreHigherWithinEachCycle()
        {
            foreach (IGrouping<int, V3LevelResult> cycle in Built.GroupBy(r => (r.Level - 1) / 5).Where(g => g.Any(r => r.Band == DifficultyBand.Hard)))
            {
                float medium = cycle.Where(r => r.Band == DifficultyBand.Medium).Max(r => r.Primary.Score);
                float hard = cycle.Single(r => r.Band == DifficultyBand.Hard).Primary.Score;
                Assert.That(hard, Is.GreaterThan(medium), $"cycle {cycle.Key}");
                V3LevelResult veryHard = cycle.SingleOrDefault(r => r.Band == DifficultyBand.VeryHard);
                if (veryHard != null) Assert.That(veryHard.Primary.Score, Is.GreaterThan(hard), $"cycle {cycle.Key}");
            }
        }

        [Test]
        public void SameBandBarelyGetsEasierWithinAPart()
        {
            // Takılma sınıfları karıştığı için aynı bant parça içinde en çok SameBandSlack kadar geri düşebilir.
            foreach (IGrouping<(V3Part, DifficultyBand), V3LevelResult> band in Built.GroupBy(r => (V3LevelPlan.PartFor(r.Level), r.Band)))
            {
                float best = float.MinValue;
                foreach (V3LevelResult r in band.OrderBy(r => r.Level))
                {
                    Assert.That(r.Primary.Score, Is.GreaterThanOrEqualTo(best - V41LevelBuilder.SameBandSlack - 1e-4f), $"level {r.Level}");
                    best = Math.Max(best, r.Primary.Score);
                }
            }
        }

        [Test]
        public void MostLevelsHitTheirTargetScore()
        {
            int onTarget = Built.Count(r => Math.Abs(r.Primary.Score - r.Target) <= V41LevelBuilder.TargetTolerance);
            Assert.That(onTarget, Is.GreaterThanOrEqualTo(80), string.Join(", ", Built.Where(r => Math.Abs(r.Primary.Score - r.Target) > V41LevelBuilder.TargetTolerance)
                .Select(r => $"{r.Level}:{r.Primary.Score:F1}/{r.Target:F1}")));
        }

        // ---------- Takılma deseni ----------

        [Test]
        public void EveryCycleFromTwentyOneHasTwoHighTwoMediumAndOneZero()
        {
            // Kurulan bölümlerin sınıfları (ilk tercih tutmazsa kurucu döngünün başka geçerli dağılımını seçer).
            Dictionary<int, TrapClass> plan = Built.ToDictionary(r => r.Level, r => r.Trap);
            for (int first = 21; first <= 96; first += 5)
            {
                TrapClass[] cycle = Enumerable.Range(first, 5).Select(l => plan[l]).ToArray();
                Assert.That(cycle.Count(c => c == TrapClass.High || c == TrapClass.Finale), Is.EqualTo(2), $"cycle {first}");
                Assert.That(cycle.Count(c => c == TrapClass.Medium), Is.EqualTo(2), $"cycle {first}");
                Assert.That(cycle.Count(c => c == TrapClass.Zero), Is.EqualTo(1), $"cycle {first}");
                Assert.That(plan[first + 4], Is.Not.EqualTo(TrapClass.Zero), $"cycle {first}: no zero on Very Hard");
            }
            Assert.That(plan[20], Is.EqualTo(TrapClass.High));
            foreach (int finale in new[] { 30, 60, 100 }) Assert.That(plan[finale], Is.EqualTo(TrapClass.Finale));
            foreach (int breath in new[] { 31, 61, 81 }) Assert.That(plan[breath], Is.EqualTo(TrapClass.Zero), $"breath {breath}");
            // "Sıfır" Orta'da daha olası (kullanıcı 18:16).
            int[] zeros = plan.Where(p => p.Value == TrapClass.Zero && p.Key >= 21 && !V41LevelBuilder.IsBreath(p.Key)).Select(p => p.Key).ToArray();
            Assert.That(zeros.Count(l => V3LevelPlan.BandFor(l) == DifficultyBand.Medium), Is.GreaterThan(zeros.Count(l => V3LevelPlan.BandFor(l) == DifficultyBand.Hard)));
        }

        [Test]
        public void TrapRatesFollowTheirClassAndThinkingPlayersPass()
        {
            foreach (V3LevelResult r in Built)
            {
                DifficultyMetrics m = r.Primary.Result.Metrics;
                float slack = r.Tier == 2 ? V41LevelBuilder.RelaxedTrapSlack : 0f;
                float thinkingSlack = r.Tier == 2 ? V41LevelBuilder.RelaxedThinkingSlack : 0f;
                Assert.That(m.NaiveFailureRate, Is.InRange(r.TrapRange.Low - slack - 1e-4f, r.TrapRange.High + slack + 1e-4f), $"level {r.Level} {r.Trap}");
                Assert.That(m.ThinkingFailureRate, Is.LessThanOrEqualTo(V41LevelBuilder.ThinkingLimitFor(r.Trap, r.Level) + thinkingSlack + 1e-4f), $"level {r.Level}");
            }
            // Derin tuzak şartları (31+) eklenince 5 renkte gevşetme 5 → 7 oldu (önizleme 2026-10-19 21:40); sınır 8.
            Assert.That(Built.Count(r => r.Tier == 2), Is.LessThanOrEqualTo(8), "relaxed trap ranges must stay rare");
        }

        [Test]
        public void NoMovesNeverInTheFirstMovesAndEverySingleEmptyLevelCanReachIt()
        {
            foreach (V3LevelResult r in Built)
            {
                int depth = r.Primary.Result.Metrics.NoMovesDepth;
                if (depth >= 0) Assert.That(depth, Is.GreaterThanOrEqualTo(V41LevelBuilder.MinNoMovesDepth), $"level {r.Level}");
                if (r.Frame.EmptyCount == 1) Assert.That(depth, Is.GreaterThanOrEqualTo(V41LevelBuilder.MinNoMovesDepth), $"level {r.Level}");
            }
        }

        [Test]
        public void NoMovesComesNoEarlierThanTheFourthMove()
        {
            // Kullanıcı (2026-10-19): 3. hamle "spamlayan için ani"; 5'te ikinci test fazla rahat geldi → 4 ("4 hamle erken değil").
            Assert.That(V41LevelBuilder.MinNoMovesDepth, Is.EqualTo(4));
        }

        [Test]
        public void TrapsFromThirtyOneNeedRealThought()
        {
            // Öneri 3 (kullanıcı 2026-10-19 "deneyelim"): 31–60 yüksek takılmada 2 hamle ileri bakan da düşer (tuzak 3 hamle düşünmeyi
            // gerektirir); 61–100'de 3 hamle ileri bakan da arada düşer; final 60 derin. 15–30 öğrenme bölgesi: sınır yok.
            foreach (V3LevelResult r in Built.Where(r => r.Level >= 31 && (r.Trap == TrapClass.High || (r.Trap == TrapClass.Finale && r.Level < 100))))
            {
                DifficultyMetrics m = r.Primary.Result.Metrics;
                float slack = r.Tier == 2 ? V41LevelBuilder.RelaxedThinkingSlack : 0f;
                if (r.Frame.ColorCount <= 4)
                    Assert.That(m.ShallowThinkingFailureRate, Is.GreaterThanOrEqualTo(V41LevelBuilder.ShallowHighMin - slack - 1e-4f), $"level {r.Level}");
                else
                    Assert.That(m.ThinkingFailureRate, Is.GreaterThanOrEqualTo(V41LevelBuilder.DeepHighThinkingMin - slack - 1e-4f), $"level {r.Level}");
            }
        }

        [Test]
        public void LastLevelIsTheHardestExamAndMakesCarefulPlayersThink()
        {
            // Kullanıcı (Editor testi 2026-10-19): 100 90'dan kolay geldi; "100 özelinde daha zor bir final olabilir".
            V3LevelResult last = At(100);
            Assert.That(last.Primary.Score, Is.GreaterThan(Built.Take(99).Max(r => r.Primary.Score)));
            Assert.That(last.Primary.Result.Metrics.ThinkingFailureRate,
                Is.InRange(V41LevelBuilder.LastFinaleThinkingMin - 1e-4f, V41LevelBuilder.LastFinaleThinkingMax + 1e-4f));
            // Editor testi 3 (2026-10-19): gevşetilmiş 2 boşlu 100 "100 için kolay" geldi → 1 boşlu, final aralığı tam, No Moves mümkün.
            Assert.That(last.Frame.EmptyCount, Is.EqualTo(1));
            Assert.That(last.Tier, Is.Not.EqualTo(2), "the last exam must not relax its trap range");
            Assert.That(last.Primary.Result.Metrics.NaiveFailureRate, Is.InRange(last.TrapRange.Low, last.TrapRange.High));
            Assert.That(last.Primary.Result.Metrics.NoMovesDepth, Is.GreaterThanOrEqualTo(V41LevelBuilder.MinNoMovesDepth));
        }

        [Test]
        public void FinalesAreExamsAndNotShort()
        {
            foreach (int finale in new[] { 30, 60, 100 })
            {
                V3LevelResult r = At(finale);
                Assert.That(r.Trap, Is.EqualTo(TrapClass.Finale));
                Assert.That(r.Primary.Result.Metrics.OptimalMoves, Is.GreaterThanOrEqualTo(V41LevelBuilder.MedianMoves(r.Frame)), $"level {finale}");
            }
        }

        [Test]
        public void TutorialCuesAreDemonstratedByTheBoard()
        {
            // L3 "Aynı türden parçalar birlikte akar.": en kısa çözümde en az bir grup (2+) hamlesi.
            Assert.That(At(3).Primary.Result.Solution.Any(m => m.Count >= 2), Is.True);
            // L4 "Boş hazneler sana yer açar.": en kısa çözüm boş bir buluta en az bir hamle yapar.
            Assert.That(UsesEmptyCloud(At(4).Primary), Is.True);
        }

        [Test]
        public void BuildIsDeterministic()
        {
            IReadOnlyList<V3LevelResult> again = V41LevelBuilder.Build();
            Assert.That(again.Select(r => r.Primary.Seed), Is.EqualTo(Built.Select(r => r.Primary.Seed)));
            Assert.That(again.Select(r => r.Frame.ToString()), Is.EqualTo(Built.Select(r => r.Frame.ToString())));
        }

        [Test]
        public void BuiltLevelsAreValidLevelDefinitions()
        {
            foreach (V3LevelResult result in Built)
            {
                LevelDefinition definition = result.Primary.ToLevelDefinition(result.Level, "intro", null);
                Assert.That(PuzzleValidator.Validate(definition, out string error), Is.True, $"level {result.Level}: {error}");
            }
        }

        // ---------- Üretilen veri dosyası ve oyuna bağlanma ----------

        [Test]
        public void GeneratedDataMatchesBuilder()
        {
            // Veri dosyası elle değiştirilmiş ya da araç değişip yeniden üretilmemişse burada yakalanır.
            Assert.That(V3LevelData.BuiltParts, Is.EqualTo(AllParts), "regenerate V3LevelData");
            foreach (V3LevelResult result in Built)
            {
                V3LevelEntry entry = V3LevelData.Find(result.Level);
                Assert.That(entry, Is.Not.Null, $"level {result.Level} missing — regenerate V3LevelData");
                Assert.That(entry.Seed, Is.EqualTo(result.Primary.Seed), $"level {result.Level}");
                Assert.That(entry.Band, Is.EqualTo(result.Band));
                Assert.That(entry.Capacity, Is.EqualTo(result.Frame.Capacity));
                Assert.That(entry.Board, Is.EqualTo(V3BoardText.Format(result.Primary.Board)));
                Assert.That(entry.Score, Is.EqualTo(result.Primary.Score).Within(1e-4f));
                Assert.That(entry.OptimalMoves, Is.EqualTo(result.Primary.Result.Metrics.OptimalMoves));
            }
        }

        [Test]
        public void CatalogUsesV3BoardsAndKeepsTutorialFrame()
        {
            for (int level = 1; level <= 15; level++)
            {
                LevelDefinition definition = AuthoredLevelCatalog.Get(level);
                V3LevelEntry entry = V3LevelData.Find(level);
                Assert.That(entry, Is.Not.Null, $"level {level}");
                Assert.That(V3BoardText.Format(definition.CreateBoard()), Is.EqualTo(entry.Board), $"level {level}");
                Assert.That(definition.tutorialCue, Is.EqualTo(AuthoredCues[level - 1]), $"level {level}");
                Assert.That(definition.capacity, Is.EqualTo(4));
                Assert.That(definition.worldId, Is.EqualTo("green-valley"));
                Assert.That(definition.generated, Is.False);
                Assert.That(definition.seed, Is.EqualTo(entry.Seed));
                Assert.That(definition.difficultyScore, Is.EqualTo(entry.Score).Within(1e-4f));
                Assert.That(PuzzleValidator.Validate(definition, out string error), Is.True, error);
            }
            Assert.That(AuthoredLevelCatalog.Count, Is.EqualTo(15));
        }

        [Test]
        public void LevelsAfterTheCampaignStayOnOldSystem()
        {
            // 101+ eski üreticide kalır (oyun 100'de biter; v4).
            Assert.That(V3LevelData.Find(101), Is.Null);
            Assert.That(AuthoredLevelCatalog.TryGetBuilt(101, out _), Is.False);
            Assert.That(LevelGenerator.Generate(101).generated, Is.True);
            foreach (int level in new[] { 45, 75, 100 })
            {
                Assert.That(AuthoredLevelCatalog.TryGetBuilt(level, out LevelDefinition built), Is.True, $"level {level}");
                Assert.That(built.modifiers, Is.Empty, $"level {level}: 1–100 themes are visual only: no modifiers (§11k)");
            }
        }

        [Test]
        public void BuiltLevelsLoadFromV3Data()
        {
            for (int level = 16; level <= 100; level++)
            {
                V3LevelEntry entry = V3LevelData.Find(level);
                Assert.That(entry, Is.Not.Null, $"level {level} missing — regenerate V3LevelData");
                Assert.That(AuthoredLevelCatalog.TryGetBuilt(level, out LevelDefinition definition), Is.True, $"level {level}");
                Assert.That(V3BoardText.Format(definition.CreateBoard()), Is.EqualTo(entry.Board), $"level {level}");
                Assert.That(definition.capacity, Is.EqualTo(entry.Capacity));
                Assert.That(definition.seed, Is.EqualTo(entry.Seed));
                Assert.That(definition.generated, Is.False);
                Assert.That(definition.tutorialCue, Is.Null);
                Assert.That(PuzzleValidator.Validate(definition, out string error), Is.True, error);
            }
            Assert.That(AuthoredLevelCatalog.TryGetBuilt(15, out _), Is.False, "1–15 stay in the authored catalog");
        }

        // ---------- Tahta metni ----------

        [Test]
        public void BoardTextRoundTrips()
        {
            WeatherType[][] board =
            {
                new[] { WeatherType.Sun, WeatherType.Rain, WeatherType.Snow, WeatherType.Wind },
                new[] { WeatherType.Moon, WeatherType.Lightning, WeatherType.Rainbow },
                Array.Empty<WeatherType>()
            };
            string text = V3BoardText.Format(board);
            Assert.That(text, Is.EqualTo("SRNW/MLB/"));
            Assert.That(V3BoardText.Format(V3BoardText.Parse(text)), Is.EqualTo(text));
            Assert.Throws<FormatException>(() => V3BoardText.Parse("SX/"));
        }

        private static bool UsesEmptyCloud(LevelCandidate candidate)
        {
            var state = new PuzzleState(candidate.Board.Select(c => new CloudState(candidate.Frame.Capacity, c)));
            foreach (Move move in candidate.Result.Solution)
            {
                if (state.Clouds[move.TargetIndex].IsEmpty) return true;
                PuzzleRules.TryApply(state, move);
            }
            return false;
        }
    }
}
