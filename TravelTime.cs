using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticAlgorithm
{
    public class TravelTime
    {
        public int ID { get; set; }
        public int OnFoot { get; set; }
        public int ByCar { get; set; }
        public GeneticAlgorithm.Scheduler.EnumVenues Destination { get; set; }
        public GeneticAlgorithm.Scheduler.EnumVenues Origin { get; set; }
    }
}
