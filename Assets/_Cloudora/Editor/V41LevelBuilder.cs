using System;
using System.Collections.Generic;
using System.Linq;
using Cloudora.Core;

namespace Cloudora.Level
{
    // Bir bölümün getirdiği yenilik (Docs/V41_TASARIM.md §3).
    public enum V3Novelty { None, NewColor, Shuffle, LargerCapacity, FewerEmpty }

    // Bölümün takılma sınıfı (Docs/V41_TASARIM.md §5): 1 boş açılmadan önce yok; sonra her döngüde 2 yüksek, 2 orta, 1 sıfır.
    public enum TrapClass { None, Zero, Medium, High, Finale }

    public sealed class V3LevelResult
    {
        public int Level { get; }
        public DifficultyBand Band { get; }
        public BoardFrame Frame { get; }
        public LevelCandidate Primary { get; }
        public float Target { get; }
        public V3Novelty Novelty { get; }
        public TrapClass Trap { get; }
        public (float Low, float High) TrapRange { get; }
        // 0: hedefe yakın ve bütün kurallara uyan; 1: kurallara uyan ama hedeften uzak; 2: takılma aralığı gevşetildi.
        public int Tier { get; }

        public V3LevelResult(int level, DifficultyBand band, BoardFrame frame, LevelCandidate primary, float target,
            V3Novelty novelty, TrapClass trap, (float Low, float High) trapRange, int tier)
        {
            Level = level;
            Band = band;
            Frame = frame;
            Primary = primary;
            Target = target;
            Novelty = novelty;
            Trap = trap;
            TrapRange = trapRange;
            Tier = tier;
        }
    }

    // v4.1 bölüm kurucusu (yalnızca Editor): 1–100'ü tek geçişte, bölüm bölüm kurar. Her bölüm için çerçeveyi (renk,
    // kapasite, boş hazne, dizilim) plandan alır, aday havuzlarından kurallara uyan ve hedef puana en yakın tahtayı seçer.
    // Kurallar: Docs/V41_TASARIM.md. Aynı girdi her zaman aynı sonucu verir.
    public static class V41LevelBuilder
    {
        public const int FirstSeed = 1;
        public const int CandidatesPerFrame = 400;
        public const int TrapPoolCandidates = 20000;      // 1 boş, 3–4 renk: kilitli kuralla çözülebilir ve No Moves ≥ 5 olan tahta az
        public const int RareTrapPoolCandidates = 100000; // 1 boş, 5 renk: 400 adayda 3–20 uygun (envanter 18:39); ölçümü hızlı
        public const int MaxStates = 300000;
        public const int LastFinalePoolCandidates = 300000; // 100: kap. 6 + 5 renk + 1 boş, final aralığı + düşünen %10–25 çok nadir

        public const int CapacityFiveLevel = 16;   // kullanıcı Claude'a bıraktı: kap. 4 + 3 renk ~19,5 puanda tıkanır
        public const int SingleEmptyLevel = 20;    // kullanıcı 18:43
        public const int CapacitySixLevel = 81;    // kullanıcı: "yaklaşık 80'de"; 81 Frost Peaks'in ikinci yarısının ilk Orta'sı

        public const float TargetTolerance = 1.0f;
        public const float MaxCycleDeviation = 2.0f; // döngü dağılımı ancak her bölüm hedefe bu kadar yakınsa kabul edilir
        public const float SameBandSlack = 1.0f;     // takılma sınıfları karıştığı için aynı bant en çok bu kadar geri düşebilir
        public const float ScatteredPenalty = 0.75f; // dağınık dizilim mümkün olduğunca az (kullanıcı)
        public const float BreathClassicPenalty = 0.5f;
        public const float ZeroTrapMax = 0.03f;
        public const float MaxTrap = 0.80f;          // sanal oyuncuların %80'inden fazlası takılıyorsa aşırı cezalandırıcı
        public const float ThinkingLimit = 0.10f;
        public const float FinaleThinkingLimit = 0.20f;
        // 100 oyunun sınavı: dikkatli oyuncuyu da düşündürsün (Editor testi: 90 düşünen %10 ile 100'den zor geldi).
        public const float LastFinaleThinkingMin = 0.10f;
        public const float LastFinaleThinkingMax = 0.25f;
        public const int MinNoMovesDepth = 4;       // ilk hamlelerde No Moves yok (kullanıcı: 3 → 5 → 4, 2026-10-19)
        public const float RelaxedTrapSlack = 0.08f;
        public const float RelaxedThinkingSlack = 0.05f;
        private const int PatternSeed = 4100;

        private static readonly int[] Finales = { 30, 60, 100 };
        private static readonly int[] BreathLevels = { CapacityFiveLevel, 31, 61, CapacitySixLevel };

        // ---------- Plan ----------

        public static int ColorsFor(int level) => level <= 5 ? 2 : level <= 30 ? 3 : level <= 60 ? 4 : 5;
        public static int CapacityFor(int level) => level < CapacityFiveLevel ? 4 : level < CapacitySixLevel ? 5 : 6;
        public static bool IsFinale(int level) => Array.IndexOf(Finales, level) >= 0;
        public static bool IsBreath(int level) => Array.IndexOf(BreathLevels, level) >= 0;

        public static V3Novelty NoveltyFor(int level) => level switch
        {
            6 or 31 or 61 => V3Novelty.NewColor,
            V3LevelPlan.ShuffleUnlockLevel => V3Novelty.Shuffle,
            CapacityFiveLevel or CapacitySixLevel => V3Novelty.LargerCapacity,
            SingleEmptyLevel => V3Novelty.FewerEmpty,
            _ => V3Novelty.None
        };

        private sealed class Curve
        {
            public int First, Last;
            public (float From, float To) Medium, Hard, VeryHard;
            public float Finale = float.NaN;
        }

        // Puan eğrisi (kullanıcı onayı 2026-10-19, Docs/V41_TASARIM.md §7): her bantta parçanın başından sonuna doğrusal.
        private static readonly Curve[] Curves =
        {
            new Curve { First = 1, Last = 5, Medium = (11.5f, 12.5f), Hard = (13.5f, 13.5f), VeryHard = (14.5f, 14.5f) },
            new Curve { First = 6, Last = 14, Medium = (15.5f, 17f), Hard = (17.5f, 18.5f), VeryHard = (19f, 19f) },
            // 15 (Çok zor) kap. 4 + 3 renkle ~20'de kalır; 1 boş 20'de gelince Çok zor yükselir.
            new Curve { First = 15, Last = 30, Medium = (18f, 22.5f), Hard = (20.5f, 24.5f), VeryHard = (20f, 26f), Finale = 29f },
            new Curve { First = 31, Last = 60, Medium = (23f, 28f), Hard = (26f, 31f), VeryHard = (28f, 33f), Finale = 35f },
            new Curve { First = 61, Last = 100, Medium = (28f, 36f), Hard = (31f, 39f), VeryHard = (33f, 41f), Finale = 44f }
        };

        public static float TargetFor(int level)
        {
            Curve curve = Curves.First(c => c.First <= level && level <= c.Last);
            if (IsFinale(level)) return curve.Finale;
            DifficultyBand band = V3LevelPlan.BandFor(level);
            int[] same = Enumerable.Range(curve.First, curve.Last - curve.First + 1)
                .Where(l => V3LevelPlan.BandFor(l) == band && !IsFinale(l)).ToArray();
            (float from, float to) = band == DifficultyBand.Medium ? curve.Medium : band == DifficultyBand.Hard ? curve.Hard : curve.VeryHard;
            float t = same.Length == 1 ? 0f : Array.IndexOf(same, level) / (float)(same.Length - 1);
            return from + (to - from) * t;
        }

        // Takılma sınıfları: 1–19 yok (2 boşlu 2–3 renkte takılma olmaz); 20 yüksek (1 boşun gelişi); finaller; 21+ her döngüde
        // 2 yüksek, 2 orta, 1 sıfır — yerleri tohumla rastgele, "sıfır" Orta'da daha olası, Çok zor'da olamaz (kullanıcı 18:16).
        // Yeni renk / kapasite bölümleri (nefes) sıfırdır. Bu, her döngünün ilk tercihidir; tutmazsa kurucu döngünün diğer
        // geçerli dağılımlarını dener (CycleAssignments).
        public static IReadOnlyDictionary<int, TrapClass> TrapPlan()
        {
            var plan = new Dictionary<int, TrapClass>();
            for (int level = 1; level < 21; level++) plan[level] = FixedTrap(level);
            for (int first = 21; first <= 96; first += 5)
                foreach (KeyValuePair<int, TrapClass> slot in CycleAssignments(first).First()) plan[slot.Key] = slot.Value;
            return plan;
        }

        private static TrapClass FixedTrap(int level) => level == SingleEmptyLevel ? TrapClass.High : TrapClass.None;

        // Döngünün geçerli takılma dağılımları: önce rastgele ilk tercih, sonra diğerleri (tohumla karıştırılmış sırada).
        public static IEnumerable<Dictionary<int, TrapClass>> CycleAssignments(int first)
        {
            var random = new DeterministicRandom(PatternSeed + first);
            var fixedSlots = new Dictionary<int, TrapClass>();
            var remaining = new List<TrapClass> { TrapClass.High, TrapClass.High, TrapClass.Medium, TrapClass.Medium, TrapClass.Zero };
            var open = new List<int>();
            for (int level = first; level < first + 5; level++)
            {
                if (IsFinale(level)) { fixedSlots[level] = TrapClass.Finale; remaining.Remove(TrapClass.High); }
                else if (IsBreath(level)) { fixedSlots[level] = TrapClass.Zero; remaining.Remove(TrapClass.Zero); }
                else open.Add(level);
            }

            // İlk tercih: "sıfır" ağırlıkla (Orta 2, Zor 1) seçilir, kalanlar karıştırılır.
            var preferred = new Dictionary<int, TrapClass>(fixedSlots);
            var rest = new List<TrapClass>(remaining);
            var openLeft = new List<int>(open);
            if (rest.Remove(TrapClass.Zero))
            {
                int[] candidates = openLeft.Where(l => V3LevelPlan.BandFor(l) != DifficultyBand.VeryHard).ToArray();
                int[] weights = candidates.Select(l => V3LevelPlan.BandFor(l) == DifficultyBand.Medium ? 2 : 1).ToArray();
                int pick = random.Next(weights.Sum()), chosen = candidates[0];
                for (int i = 0; i < candidates.Length; i++) { if (pick < weights[i]) { chosen = candidates[i]; break; } pick -= weights[i]; }
                preferred[chosen] = TrapClass.Zero;
                openLeft.Remove(chosen);
            }
            Shuffle(rest, random);
            for (int i = 0; i < openLeft.Count; i++) preferred[openLeft[i]] = rest[i];
            yield return preferred;

            // Diğer geçerli dağılımlar (sıfır Çok zor'da olamaz).
            var all = new List<Dictionary<int, TrapClass>>();
            foreach (TrapClass[] order in Permutations(remaining).Distinct(new SequenceComparer()))
            {
                var assignment = new Dictionary<int, TrapClass>(fixedSlots);
                for (int i = 0; i < open.Count; i++) assignment[open[i]] = order[i];
                if (assignment.Any(a => a.Value == TrapClass.Zero && V3LevelPlan.BandFor(a.Key) == DifficultyBand.VeryHard)) continue;
                if (open.All(l => assignment[l] == preferred[l])) continue;
                all.Add(assignment);
            }
            Shuffle(all, random);
            foreach (Dictionary<int, TrapClass> assignment in all) yield return assignment;
        }

        private static void Shuffle<T>(List<T> items, DeterministicRandom random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }

        private static IEnumerable<TrapClass[]> Permutations(List<TrapClass> items)
        {
            if (items.Count <= 1) { yield return items.ToArray(); yield break; }
            for (int i = 0; i < items.Count; i++)
            {
                var rest = new List<TrapClass>(items);
                rest.RemoveAt(i);
                foreach (TrapClass[] tail in Permutations(rest)) yield return new[] { items[i] }.Concat(tail).ToArray();
            }
        }

        private sealed class SequenceComparer : IEqualityComparer<TrapClass[]>
        {
            public bool Equals(TrapClass[] a, TrapClass[] b) => a.SequenceEqual(b);
            public int GetHashCode(TrapClass[] a) => a.Aggregate(17, (h, c) => h * 31 + (int)c);
        }

        // Aralıklar oyun ilerledikçe dengeli yükselir (kullanıcı: 60'tan sonra Zor ~0,5, Çok zor ~0,7'ye çıkabilir).
        public static (float Low, float High) TrapRangeFor(TrapClass trap, int level)
        {
            float t = Math.Clamp((level - SingleEmptyLevel) / 80f, 0f, 1f);
            return trap switch
            {
                TrapClass.Zero => (0f, ZeroTrapMax),
                TrapClass.Medium => (0.12f + 0.08f * t, 0.35f + 0.10f * t),
                // Alt sınır 0,40–0,50 → 0,35–0,40: No Moves ≥ 5 kısa tuzakları ayıklayınca yüksek sınıf (özellikle 5 renkte)
                // 0,35–0,38'de kalıyordu (2026-10-19).
                TrapClass.High => (0.35f + 0.05f * t, Math.Min(MaxTrap, 0.68f + 0.10f * t)),
                TrapClass.Finale => (0.60f, MaxTrap),
                _ => (0f, 1f)
            };
        }

        public static float ThinkingLimitFor(TrapClass trap) => trap == TrapClass.Finale ? FinaleThinkingLimit : ThinkingLimit;
        public static float ThinkingLimitFor(TrapClass trap, int level) => level == V3LevelPlan.LastLevel ? LastFinaleThinkingMax :
            trap == TrapClass.High && ColorsFor(level) >= 5 ? DeepHighThinkingMax : ThinkingLimitFor(trap);

        // (b2) Tuzak derinliği alt sınırları: 3–4 renkte 2 hamle ileri bakan düşsün; 5 renkte 3 hamle ileri bakan da arada düşsün.
        public const float ShallowHighMin = 0.10f, ShallowFinaleMin = 0.10f, DeepHighThinkingMin = 0.05f, DeepHighThinkingMax = 0.15f;

        private static bool DeepEnough(TrapClass trap, int level, DifficultyMetrics m, float slack)
        {
            if (level < 31) return true;   // 15–30 (3 renk): derin tuzak = erken No Moves; öğrenme bölgesi sığ kalır (2026-10-19)
            bool fewColors = ColorsFor(level) <= 4;
            if (trap == TrapClass.High)
                return fewColors ? m.ShallowThinkingFailureRate >= ShallowHighMin - slack : m.ThinkingFailureRate >= DeepHighThinkingMin - slack;
            if (trap == TrapClass.Finale && level != V3LevelPlan.LastLevel)
                return m.ShallowThinkingFailureRate >= ShallowFinaleMin - slack;
            return true;
        }

        // Bölümde denenecek çerçeveler: 1 boş 20'den itibaren; dağınık dizilim yalnızca nefes bölümlerinde ve orta takılmada.
        public static IEnumerable<BoardFrame> FramesFor(int level, TrapClass trap)
        {
            int capacity = CapacityFor(level), colors = ColorsFor(level);
            bool scattered = level > V3LevelPlan.ShuffleUnlockLevel && (IsBreath(level) || trap == TrapClass.Medium);
            // 100 oyunun sınavı: 1 boşlu (No Moves mümkün), 30 ve 60 gibi (Editor testi 3, 2026-10-19).
            foreach (int empties in level < SingleEmptyLevel ? new[] { 2 } : level == V3LevelPlan.LastLevel ? new[] { 1 } : new[] { 2, 1 })
            for (int open = empties; open >= (scattered ? 0 : empties); open--)
                yield return new BoardFrame(capacity, colors, empties, open);
        }

        // ---------- Kurulum ----------

        private sealed class BuildState
        {
            public HashSet<string> Used = new();
            public List<V3LevelResult> Results = new();
            public Dictionary<(V3Part, DifficultyBand), float> LastByBand = new();

            public BuildState Clone() => new BuildState
            {
                Used = new HashSet<string>(Used),
                Results = new List<V3LevelResult>(Results),
                LastByBand = new Dictionary<(V3Part, DifficultyBand), float>(LastByBand)
            };
        }

        public static IReadOnlyList<V3LevelResult> Build()
        {
            var state = new BuildState();
            for (int level = 1; level < 21; level++)
                if (!TrySelect(state, level, FixedTrap(level), out string failure)) throw new InvalidOperationException(failure);
            // Döngü döngü: ilk tercih edilen dağılım her bölümü hedefe yakın (≤ MaxCycleDeviation) kurabiliyorsa o; değilse
            // sıradaki geçerli dağılımlardan ilk yakın olanı; hiçbiri yakın değilse en büyük sapması en küçük olanı.
            for (int first = 21; first <= 96; first += 5)
            {
                string firstFailure = null;
                BuildState accepted = null, best = null;
                float bestWorst = float.MaxValue;
                foreach (Dictionary<int, TrapClass> assignment in CycleAssignments(first))
                {
                    BuildState trial = state.Clone();
                    bool ok = true;
                    for (int level = first; level < first + 5 && ok; level++)
                    {
                        ok = TrySelect(trial, level, assignment[level], out string failure);
                        if (!ok) firstFailure ??= failure;
                    }
                    if (!ok) continue;
                    float worst = trial.Results.Where(r => r.Level >= first).Max(r => Math.Abs(r.Primary.Score - r.Target));
                    if (worst <= MaxCycleDeviation) { accepted = trial; break; }
                    if (worst < bestWorst) { best = trial; bestWorst = worst; }
                }
                state = accepted ?? best ?? throw new InvalidOperationException($"Cycle {first}–{first + 4}: no trap assignment works. First: {firstFailure}");
            }
            return state.Results;
        }

        private static bool TrySelect(BuildState state, int level, TrapClass trap, out string failure)
        {
            failure = null;
            DifficultyBand band = V3LevelPlan.BandFor(level);
            V3Part part = V3LevelPlan.PartFor(level);
            int cycleFirst = (level - 1) / 5 * 5 + 1;
            V3LevelResult[] cycle = state.Results.Where(r => r.Level >= cycleFirst).ToArray();
            float cycleMediumMax = cycle.Where(r => r.Band == DifficultyBand.Medium).Select(r => r.Primary.Score).DefaultIfEmpty(float.MinValue).Max();
            float cycleHard = cycle.Where(r => r.Band == DifficultyBand.Hard).Select(r => r.Primary.Score).DefaultIfEmpty(float.NaN).First();
            float floor = state.LastByBand.TryGetValue((part, band), out float last) ? last - SameBandSlack : float.MinValue;
            (float low, float high) = TrapRangeFor(trap, level);
            float target = TargetFor(level);
            HashSet<string> used = state.Used;

            bool Allowed(LevelCandidate c, float slack, float thinkingSlack)
            {
                DifficultyMetrics m = c.Result.Metrics;
                if (used.Contains(c.CanonicalKey) || c.Score < floor) return false;
                if (band == DifficultyBand.Hard && c.Score <= cycleMediumMax) return false;
                if (band == DifficultyBand.VeryHard && !(c.Score > cycleHard)) return false;
                if (band == DifficultyBand.Medium && !float.IsNaN(cycleHard) && c.Score >= cycleHard) return false;
                if (m.NaiveFailureRate < low - slack || m.NaiveFailureRate > high + slack) return false;
                if (m.ThinkingFailureRate > ThinkingLimitFor(trap, level) + thinkingSlack) return false;
                if (level == V3LevelPlan.LastLevel && m.ThinkingFailureRate < LastFinaleThinkingMin) return false;
                if (!DeepEnough(trap, level, m, thinkingSlack)) return false;
                if (m.NoMovesDepth >= 0 && m.NoMovesDepth < MinNoMovesDepth) return false;
                if (c.Frame.EmptyCount == 1 && m.NoMovesDepth < MinNoMovesDepth) return false; // her 1 boşlu bölüm No Moves'a çıkabilmeli
                if (m.OptimalMoves > LongMovesThreshold(c.Frame)) return false;
                if (IsFinale(level) && m.OptimalMoves < MedianMoves(c.Frame)) return false;
                return DemonstratesCue(level, c);
            }

            float Value(LevelCandidate c) => Math.Abs(c.Score - target) +
                (!c.Frame.IsPacked && !IsBreath(level) ? ScatteredPenalty : 0f) +
                (c.Frame.IsPacked && IsBreath(level) && level > V3LevelPlan.ShuffleUnlockLevel ? BreathClassicPenalty : 0f);

            BoardFrame[] frames = FramesFor(level, trap).ToArray();
            LevelCandidate pick = null;
            int tier = -1;
            for (int attempt = 0; attempt < 3 && pick == null; attempt++)
            {
                float slack = attempt == 2 ? RelaxedTrapSlack : 0f, thinkingSlack = attempt == 2 ? RelaxedThinkingSlack : 0f;
                // 100'ün ayrı, büyük havuzu: diğer bölümlerin (ortak havuzdan) seçimi değişmesin. 100 takılma aralığını gevşetmez.
                if (attempt == 2 && level == V3LevelPlan.LastLevel) break;
                IEnumerable<LevelCandidate> allowed = frames.SelectMany(f => level == V3LevelPlan.LastLevel ? Pool(f, LastFinalePoolCandidates) : Pool(f))
                    .Where(c => Allowed(c, slack, thinkingSlack));
                if (attempt == 0) allowed = allowed.Where(c => Math.Abs(c.Score - target) <= TargetTolerance);
                pick = allowed.OrderBy(Value).ThenBy(c => c.Frame.ToString(), StringComparer.Ordinal).ThenBy(c => c.Seed).FirstOrDefault();
                tier = attempt;
            }
            if (pick == null)
            {
                failure = $"Level {level} ({band}, {trap} {low:F2}–{high:F2}, target {target:F1}): no board. " + Diagnose(frames, low, high, trap, floor);
                return false;
            }

            used.Add(pick.CanonicalKey);
            state.LastByBand[(part, band)] = Math.Max(last, pick.Score);
            state.Results.Add(new V3LevelResult(level, band, pick.Frame, pick, target, NoveltyFor(level), trap, (low, high), tier));
            return true;
        }

        private static string Diagnose(BoardFrame[] frames, float low, float high, TrapClass trap, float floor)
        {
            var lines = new List<string>();
            foreach (BoardFrame frame in frames)
            {
                IReadOnlyList<LevelCandidate> pool = Pool(frame);
                int trapOk = pool.Count(c => c.Result.Metrics.NaiveFailureRate >= low && c.Result.Metrics.NaiveFailureRate <= high);
                int thinkOk = pool.Count(c => c.Result.Metrics.NaiveFailureRate >= low && c.Result.Metrics.NaiveFailureRate <= high &&
                                              c.Result.Metrics.ThinkingFailureRate <= ThinkingLimitFor(trap));
                lines.Add($"{frame}: pool {pool.Count}, trap ok {trapOk}, +thinking {thinkOk}, above floor {pool.Count(c => c.Score >= floor)}");
            }
            return string.Join("; ", lines);
        }

        // Öğretici yazının gösterdiği şey tahtada gerçekten olmalı.
        private static bool DemonstratesCue(int level, LevelCandidate candidate)
        {
            Move[] solution = candidate.Result.Solution;
            switch (level)
            {
                case 3: // "Aynı türden parçalar birlikte akar."
                    return solution.Any(m => m.Count >= 2);
                case 4: // "Boş hazneler sana yer açar."
                    var state = new PuzzleState(candidate.Board.Select(c => new CloudState(candidate.Frame.Capacity, c)));
                    foreach (Move move in solution)
                    {
                        if (state.Clouds[move.TargetIndex].IsEmpty) return true;
                        PuzzleRules.TryApply(state, move);
                    }
                    return false;
                default:
                    return true;
            }
        }

        // ---------- Aday havuzları ----------

        private static readonly Dictionary<string, List<LevelCandidate>> Pools = new();
        private static readonly Dictionary<string, int> LongMoves = new();
        private static readonly Dictionary<string, int> Medians = new();

        public static int PoolSizeFor(BoardFrame frame) =>
            frame.EmptyCount == 1 ? (frame.ColorCount >= 5 ? RareTrapPoolCandidates : TrapPoolCandidates) : CandidatesPerFrame;

        // Çerçevenin çözülebilir ve aşırı cezalandırıcı olmayan adayları (puana göre sıralı); oturum boyunca önbellekte.
        public static IReadOnlyList<LevelCandidate> Pool(BoardFrame frame) => Pool(frame, 0);

        // candidates > 0: o çerçeve için ayrı (daha büyük) havuz — ortak havuzu kullanan seçimler değişmez.
        public static IReadOnlyList<LevelCandidate> Pool(BoardFrame frame, int candidates)
        {
            string key = candidates > 0 ? frame + "#" + candidates : frame.ToString();
            if (Pools.TryGetValue(key, out List<LevelCandidate> pool)) return pool;
            CandidateSelection all = LevelCandidateSelector.Select(frame, new ScoreWindow(float.MinValue, float.MaxValue),
                FirstSeed, candidates > 0 ? candidates : PoolSizeFor(frame), 0, MaxStates);
            pool = all.Measured
                .Where(c => c.Result.Status == SolveStatus.Solved && c.Result.Metrics.NaiveFailureRate <= MaxTrap)
                .OrderBy(c => c.Score)
                .ThenBy(c => c.Seed)
                .ToList();
            Pools[key] = pool;
            return pool;
        }

        // "Absürt uzunluk" sınırı (v3 Faz 2.1): çerçevenin kullanılabilir tahtalarında en kısa çözümün %90'lık değeri.
        public static int LongMovesThreshold(BoardFrame frame)
        {
            string key = frame.ToString();
            if (LongMoves.TryGetValue(key, out int cached)) return cached;
            int[] moves = Pool(frame).Select(c => c.Result.Metrics.OptimalMoves).OrderBy(m => m).ToArray();
            return LongMoves[key] = moves.Length == 0 ? 0 : moves[(int)Math.Round(0.9 * (moves.Length - 1))];
        }

        // Final, çerçevesinin ortanca uzunluğundan kısa olamaz (v3, kullanıcı 2026-10-15 18:42).
        public static int MedianMoves(BoardFrame frame)
        {
            string key = frame.ToString();
            if (Medians.TryGetValue(key, out int cached)) return cached;
            int[] moves = Pool(frame).Select(c => c.Result.Metrics.OptimalMoves).OrderBy(m => m).ToArray();
            return Medians[key] = moves.Length == 0 ? 0 : moves[moves.Length / 2];
        }
    }
}
