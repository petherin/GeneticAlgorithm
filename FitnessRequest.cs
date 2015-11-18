using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticAlgorithm
{
    public class FitnessRequest
    {
        //Data to work on
        public Dictionary<GeneticAlgorithm.Scheduler.EnumVenues, string> Venues { get; set; }
        public List<Schedule> Population { get; set; }
        public List<TimeSlot> TimeSlots { get; set; }
        public Dictionary<string, Film> Films { get; set; }
        public int GeneLength { get; set; }

        //User options
        public List<DateRange> SelectedDateRanges { get; set; }
        public Dictionary<string, Film> SelectedFilms { get; set; }
        public Dictionary<GeneticAlgorithm.Scheduler.EnumVenues, Dictionary<GeneticAlgorithm.Scheduler.EnumVenues, TravelTime>> TravelTimes { get; set; }
        public GeneticAlgorithm.Scheduler.EnumTravelOptions PreferredTravelOption { get; set; }
        public int ContingencyMins { get; set; }
        public int GapBetweenTimeSlotsMins { get; set; }
        public bool AllowDuplicateEvents { get; set; }
        public bool SelectedFilmsOnly { get; set; }
        public List<int> TimeSlotsWithSelectedFilms { get; set; }
        public string ReplacementGenes { get; set; }
    } 

}
