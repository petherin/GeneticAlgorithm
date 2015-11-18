using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticAlgorithm
{
    public class Schedule
    {
        public string ScheduleString { get; set; }
        public int Fitness { get; set; }
        public List<TimeSlot> UniqueSlots { get; set; }
        public List<int> UniqueEvents { get; set; }

        public Schedule()
        {
            ScheduleString = string.Empty;
            Fitness = 0;
            UniqueSlots = new List<TimeSlot>();
            UniqueEvents = new List<int>();
        }
    }
}
