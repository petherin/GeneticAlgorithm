using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticAlgorithm
{
    public class TimeSlot
    {
        public int ID { get; set; }
        public Film Event { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public GeneticAlgorithm.Scheduler.EnumVenues Venue { get; set; }
        public bool Selected { get; set; }
    }
}
