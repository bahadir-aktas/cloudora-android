using System;
using System.Collections.Generic;
using Cloudora.Core;
using Cloudora.Puzzle;

namespace Cloudora.Level
{
    // Bir bölümün çerçevesi: bulut kapasitesi, hava türü (renk) sayısı ve boş bulut sayısı.
    public readonly struct BoardFrame
    {
        public readonly int Capacity;
        public readonly int ColorCount;
        public readonly int EmptyCount;

        public BoardFrame(int capacity, int colorCount, int emptyCount)
        {
            if (capacity < 2 || capacity > 7) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (colorCount < 2 || colorCount > LevelCandidateFactory.Colors.Length) throw new ArgumentOutOfRangeException(nameof(colorCount));
            if (emptyCount < 0 || emptyCount > 6) throw new ArgumentOutOfRangeException(nameof(emptyCount));
            Capacity = capacity;
            ColorCount = colorCount;
            EmptyCount = emptyCount;
        }

        public int CloudCount => ColorCount + EmptyCount;
        public override string ToString() => $"cap{Capacity}/c{ColorCount}/e{EmptyCount}";
    }

    // Aday tahta: elle tasarlanmış bölümlerle aynı biçim — renkli bulutlar dolu ve karışık, boş bulutlar sonda.
    public static class LevelCandidateFactory
    {
        // LevelGenerator ve AuthoredLevelCatalog ile aynı renk sırası (1–5: Güneş/Yağmur, 6+: Kar…).
        internal static readonly WeatherType[] Colors =
        {
            WeatherType.Sun, WeatherType.Rain, WeatherType.Snow, WeatherType.Wind,
            WeatherType.Moon, WeatherType.Lightning
        };

        public static WeatherType[][] Deal(BoardFrame frame, int seed)
        {
            var random = new DeterministicRandom(seed);
            var pool = new WeatherType[frame.ColorCount * frame.Capacity];
            for (int attempt = 0; attempt < 64; attempt++)
            {
                for (int i = 0; i < pool.Length; i++) pool[i] = Colors[i / frame.Capacity];
                for (int i = pool.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (pool[i], pool[j]) = (pool[j], pool[i]);
                }

                var board = new WeatherType[frame.CloudCount][];
                for (int c = 0; c < frame.CloudCount; c++)
                {
                    board[c] = c < frame.ColorCount ? new WeatherType[frame.Capacity] : Array.Empty<WeatherType>();
                    if (c < frame.ColorCount) Array.Copy(pool, c * frame.Capacity, board[c], 0, frame.Capacity);
                }

                if (!StartsSolved(board, frame.Capacity)) return board;
            }

            throw new InvalidOperationException($"Could not deal an unsolved board for {frame} seed {seed}.");
        }

        private static bool StartsSolved(WeatherType[][] board, int capacity)
        {
            var clouds = new List<CloudState>(board.Length);
            foreach (WeatherType[] cloud in board) clouds.Add(new CloudState(capacity, cloud));
            return PuzzleRules.IsSolved(new PuzzleState(clouds));
        }
    }
}
