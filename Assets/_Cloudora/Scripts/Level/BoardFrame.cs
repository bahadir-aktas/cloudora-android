using System;

namespace Cloudora.Level
{
    // Bir bölümün çerçevesi: bulut kapasitesi, hava türü (renk) sayısı ve boş bulut sayısı.
    // Oyun tarafında V3LevelPlan kullanır; aday üretimi Editor'daki bölüm aracındadır.
    public readonly struct BoardFrame
    {
        public const int MaxColors = 6; // Sun, Rain, Snow, Wind, Moon, Lightning

        public readonly int Capacity;
        public readonly int ColorCount;
        public readonly int EmptyCount;

        public BoardFrame(int capacity, int colorCount, int emptyCount)
        {
            if (capacity < 2 || capacity > 7) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (colorCount < 2 || colorCount > MaxColors) throw new ArgumentOutOfRangeException(nameof(colorCount));
            if (emptyCount < 0 || emptyCount > 6) throw new ArgumentOutOfRangeException(nameof(emptyCount));
            Capacity = capacity;
            ColorCount = colorCount;
            EmptyCount = emptyCount;
        }

        public int CloudCount => ColorCount + EmptyCount;
        public override string ToString() => $"cap{Capacity}/c{ColorCount}/e{EmptyCount}";
    }
}
