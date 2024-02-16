using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightCalendar
{
    public class MidnightDay
    {
        public static MidnightDay Today
        {
            get => new(SDate.Now());
        }

        private int daysSinceStart;
        public int DaysSinceStart 
        {
            get => daysSinceStart;
            set
            {
                Setup(SDate.FromDaysSinceStart(value));
                daysSinceStart = value;
            }
        }

        private int day;
        /// <summary>
        /// The day of the month.
        /// </summary>
        public int Day 
        {
            get => day;
            set
            {
                DaysSinceStart = daysSinceStart + (value - day);
            }
        }
        /// <summary>
        /// The season of the year.
        /// </summary>
        public MidnightSeason Season { get; set; }
        /// <summary>
        /// The current game year.
        /// </summary>
        public int Year { get; set; }

        public MidnightDay(SDate date)
        {
            daysSinceStart = date.DaysSinceStart;

            day = date.Day;
            Season = date.Season;
            Year = date.Year;
        }

        /// <summary>
        /// Checks if the given day has a festival.
        /// </summary>
        /// <returns>True if there is a festival on this day.</returns>
        public bool IsFestivalDay()
        {
            return Utility.isFestivalDay(Day, (Season)Season);
        }

        public void NextDay()
        {
            DaysSinceStart++;
        }

        public void PreviousDay()
        {
            DaysSinceStart--;
        }

        private void Setup(SDate date)
        {
            day = date.Day;
            Season = date.Season;
            Year = date.Year;
        }

        public static implicit operator SDate(MidnightDay midnightDay) => new SDate(midnightDay.Day, midnightDay.Season, midnightDay.Year);
    }
}
