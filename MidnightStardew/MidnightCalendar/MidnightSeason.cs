

namespace MidnightStardew.MidnightCalendar
{
    public class MidnightSeason
    {
        public static MidnightSeason Spring { get; } = new MidnightSeason("Spring", 0);
        public static MidnightSeason Summer { get; } = new MidnightSeason("Summer", 1);
        public static MidnightSeason Fall { get; } = new MidnightSeason("Fall", 2);
        public static MidnightSeason Winter { get; } = new MidnightSeason("Winter", 3);

        public static MidnightSeason[] Get { get; } = new[] { Spring, Summer, Fall, Winter };

        public string Name { get; }
        private readonly int arrayPosition;

        private MidnightSeason(string name, int position)
        {
            Name = name;
            arrayPosition = position;
        }

        /// <summary>
        /// Get the season that follows this season.
        /// </summary>
        /// <returns>The next season.</returns>
        public MidnightSeason GetNext()
        {
            var nextPos = arrayPosition + 1;
            if (nextPos > 3) nextPos = 0;
            return Get[nextPos];
        }

        /// <summary>
        /// Get the season that precedes this season.
        /// </summary>
        /// <returns>The preceding season.</returns>
        public MidnightSeason GetPrevious()
        {
            var nextPos = arrayPosition - 1;
            if (nextPos < 0) nextPos = 3;
            return Get[nextPos];
        }

        /// <summary>
        /// Name of the season in lower case. Game logic requires season names to be lower case.
        /// </summary>
        /// <returns>The season name in lower case.</returns>
        public string ToLower()
        {
            return Name.ToLower();
        }

        public static implicit operator string(MidnightSeason season) => season.Name;
        public static implicit operator MidnightSeason(string seasonName)
        {
            seasonName = seasonName.ToLower();
            foreach (var season in Get)
            {
                if (season.ToLower() == seasonName)
                {
                    return season;
                }
            }
            throw new ArgumentException($"Attempt to cast {seasonName} to a season failed.");
        }

        public static explicit operator StardewValley.Season(MidnightSeason season) => (StardewValley.Season) season.arrayPosition;
        public static implicit operator MidnightSeason(StardewValley.Season season) => Get[(int)season];
    }
}
