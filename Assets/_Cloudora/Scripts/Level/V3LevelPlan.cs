using System;

namespace Cloudora.Level
{
    // v4.1: 1–100 parçaları dünyalarla hizalı — bkz. Docs/V41_TASARIM.md §3.
    public enum V3Part { Intro, Adaptation, GreenValley, RainyCoast, FrostPeaks }

    public static class V3LevelPlan
    {
        public const int LastLevel = 100;
        public const int ShuffleUnlockLevel = 15;

        public static V3Part PartFor(int level)
        {
            Check(level);
            return level <= 5 ? V3Part.Intro :
                level <= 14 ? V3Part.Adaptation :
                level <= 30 ? V3Part.GreenValley :
                level <= 60 ? V3Part.RainyCoast : V3Part.FrostPeaks;
        }

        public static (int First, int Last) RangeOf(V3Part part) => part switch
        {
            V3Part.Intro => (1, 5),
            V3Part.Adaptation => (6, 14),
            V3Part.GreenValley => (15, 30),
            V3Part.RainyCoast => (31, 60),
            _ => (61, 100)
        };

        // Her 5 bölümde: Orta, Orta, Zor, Orta, Çok zor. "Kolay" yok.
        public static DifficultyBand BandFor(int level)
        {
            Check(level);
            return ((level - 1) % 5) switch
            {
                2 => DifficultyBand.Hard,
                4 => DifficultyBand.VeryHard,
                _ => DifficultyBand.Medium
            };
        }

        private static void Check(int level)
        {
            if (level < 1 || level > LastLevel) throw new ArgumentOutOfRangeException(nameof(level));
        }
    }
}
