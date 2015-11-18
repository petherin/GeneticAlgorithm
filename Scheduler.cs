using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace GeneticAlgorithm
{
    public class Scheduler
    {
        #region Public Enums
        public enum EnumVenues
        {
            LeedsTownHall_VictoriaHall,
            LeedsTownHall_AlbertRoom,
            Vue,
            Everyman,
            HydePark,
            LeftBank,
            Carriageworks, 
            CityVarieties,
            ChapelFMSeacroft,
            RoyalArmouries,
            BelgraveMusicHall,
            Leeds,
            TheTetley,
            CottageRoadCinema,
            TheReliance,
            LeedsUniversity
        }

        public enum EnumTravelOptions
        {
            OnFoot,
            ByCar
        }
        #endregion

        #region Constants
        const int numberOfSchedules = 100;
        const double mutationRate = 0.1;
        const int generations = 40;

        #region Default user option constants 
        const EnumTravelOptions _defaultPreferredTravelOption = EnumTravelOptions.OnFoot;
        const int _defaultContingencyMins = 10;
        const int _defaultGapBetweenSlotsMins = 10;
        const bool _defaultAllowDuplicateEvents = false;
        const bool _defaultSelectedEventsOnly = false;
        #endregion

        #endregion

        #region Private Variables
        private string _uniqueFilename;
        private List<TimeSlot> _bestSortedDoableSlots = null;
        private int _overallHighestFitness = 0;
        private int _overallBestGeneration = 0;
        #endregion

        #region Public Methods

        public void Run()
        {
            DataTable filmData = Helpers.GetDataTableFromCSVFile("C:\\Users\\Peter\\Documents\\Visual Studio 2013\\Projects\\GeneticAlgorithm\\GeneticAlgorithm\\liff28_films.csv");
            DataTable travelTimesData = Helpers.GetDataTableFromCSVFile("C:\\Users\\Peter\\Documents\\Visual Studio 2013\\Projects\\GeneticAlgorithm\\GeneticAlgorithm\\traveltimes.csv");

            FitnessRequest request = InitialiseData(filmData, travelTimesData);
            PreloadTimeSlotsWithSelectedFilms(request);

            // Loop through generations
            for (int generationX = 0; generationX < generations; generationX++)
            {
                CalculateSchedule calculator = new CalculateSchedule();
                calculator.Calculate(request);
                int highestFitness = 0;
                List<TimeSlot> sortedDoableSlots = GetBestSlotOrder(generationX, request.Population, request.TimeSlots, out highestFitness);
                LogGeneration(generationX, sortedDoableSlots, highestFitness, false);
                Console.Clear();
                Console.Write("Generation " + generationX);
                //Leave the last generation as is.
                if (generationX == generations - 1)
                {
                    break;
                }

                List<Schedule> newPopulation = CrossOver(request.Population);
                Mutation(newPopulation);
                request.Population = new List<Schedule>();
                request.Population = newPopulation;
                
                PreloadTimeSlotsWithSelectedFilms(request);
                DeselectTimeSlots(request.TimeSlots);
            }

            LogGeneration(_overallBestGeneration, _bestSortedDoableSlots, _overallHighestFitness, true);
            WriteBestResult();
        }

       
        #endregion

        #region Private Methods

        private void DeselectTimeSlots(List<TimeSlot> slots)
        {
            foreach (TimeSlot slot in slots)
            {
                slot.Selected = false;
            }
        }

        private FitnessRequest InitialiseData(DataTable eventData, DataTable travelTimeData)
        {
            //Get initial data
            FitnessRequest retVal = new FitnessRequest();

            // Films and TimeSlots never change once set.
            retVal.Films = PopulateFilms(eventData);
            retVal.TimeSlots = PopulateTimeSlots(eventData, retVal.Films);
            retVal.TimeSlots = retVal.TimeSlots.OrderBy(o => o.StartTime).ToList();
            retVal.GeneLength = Helpers.ConvertIntToBinaryString(retVal.TimeSlots.Count).Length;

            // A population is a list of alternative schedules. A schedule is a list of index numbers from the TimeSlots lost, expresesed in binary in a long string.
            retVal.Population = PopulateSchedules(retVal.TimeSlots.Count, retVal.GeneLength);

            //Get default user options
            retVal.SelectedFilms = PopulateDefaultSelectedFilms(retVal.Films);
            retVal.SelectedDateRanges = PopulateDefaultSelectedDateRanges(retVal.TimeSlots);
            retVal.TravelTimes = PopulateDefaultTravelTimes(travelTimeData);
            retVal.PreferredTravelOption = _defaultPreferredTravelOption;
            retVal.ContingencyMins = _defaultContingencyMins;
            retVal.GapBetweenTimeSlotsMins = _defaultGapBetweenSlotsMins;
            retVal.AllowDuplicateEvents = _defaultAllowDuplicateEvents;
            retVal.SelectedFilmsOnly = _defaultSelectedEventsOnly;
            retVal.TimeSlotsWithSelectedFilms = PopulateTimeSlotsWithSelectedFilms(retVal.SelectedFilms, retVal.TimeSlots);
            retVal.ReplacementGenes = CreateReplacementGenes(retVal);

            return retVal;
        }

        private List<int> PopulateTimeSlotsWithSelectedFilms(Dictionary<string, Film> selectedFilms, List<TimeSlot> slots)
        {
            List<int> retVal = new List<int>();
            foreach (KeyValuePair<string, Film> currentEvent in selectedFilms)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].Event.ID == currentEvent.Value.ID)
                    {
                        retVal.Add(i);
                    }
                }
            }
            return retVal;
        }

        private string CreateReplacementGenes(FitnessRequest request)
        {
            string replacementGenes = string.Empty;
            foreach (int slotIdx in request.TimeSlotsWithSelectedFilms)
            {
                replacementGenes += Helpers.ConvertIntToBinaryString(slotIdx, request.GeneLength);
            }
            return replacementGenes;
        }

        private List<TimeSlot> GetBestSlotOrder(int generation, List<Schedule> population, List<TimeSlot> slots, out int highestFitness)
        {
            highestFitness = GetHightestFitness(population);
            Schedule bestSchedule = GetBestSchedule(population, highestFitness);
            List<TimeSlot> doableSlots = new List<TimeSlot>();

            if (bestSchedule != null)
            {
                for (int i = 0; i < bestSchedule.UniqueSlots.Count; i++)
                {
                   // int nonConflictingSlot = bestChromosome.UniqueSlots[i];
                    doableSlots.Add(bestSchedule.UniqueSlots[i]);
                }
            }

            List<TimeSlot> sortedDoableSlots = doableSlots.OrderBy(o => o.StartTime).ToList();

            //Save the best overall order of slots.
            if (highestFitness > _overallHighestFitness)
            {
                _overallHighestFitness = highestFitness;
                _overallBestGeneration = generation;
                _bestSortedDoableSlots = sortedDoableSlots;
            }

            return sortedDoableSlots;
        }

        private void LogGeneration(int generation, List<TimeSlot> sortedDoableSlots, int highestFitness, bool logBest)
        {
            string logTemplate = "{0}) ID: {1} Title: {2} Start: {3} End: {4} Venue: {5}";
            string logFile = string.Empty;
            if (logBest)
            {
                logFile = "Best fit" + Environment.NewLine;
            }
            logFile += "Generation: " + generation + " Fitness: " + highestFitness + Environment.NewLine;
            int i = 0;

            foreach (TimeSlot slot in sortedDoableSlots)
            {
                i++;
                logFile += string.Format(logTemplate, i, slot.ID, slot.Event.Title,
                      slot.StartTime, slot.EndTime,
                      slot.Venue.ToString());
                logFile += Environment.NewLine;
            }

            System.IO.StreamWriter file = null;
            if (generation == 0)
            {
                // Create new log file with unique name
                _uniqueFilename = DateTime.Now.ToString("yyyyMMddHHmmss");
                file = new System.IO.StreamWriter("C:\\Users\\Peter\\Documents\\GALog" + _uniqueFilename + ".txt");
            }
            else
            {
                file = new System.IO.StreamWriter("C:\\Users\\Peter\\Documents\\GALog" + _uniqueFilename + ".txt", true);
            }
            
            file.WriteLine(logFile);
            file.Close();
        }

        private void WriteBestResult()
        {
            Console.Clear();
            Console.WriteLine("Highest fitness: {0} Generation: {1}", _overallHighestFitness, _overallBestGeneration);
            Console.WriteLine(Environment.NewLine);
            
            if (_bestSortedDoableSlots == null)
            {
                Console.WriteLine("No events selected.");
                Console.WriteLine("Press a key...");
                Console.ReadKey();
                return;
            }

            int i = 0;
            foreach (TimeSlot slot in _bestSortedDoableSlots)
            {
                i++;
                Console.WriteLine("{0})\nID: {1}\nTitle: {2}\nStart: {3}\nEnd:   {4}\nVenue: {5}",
                      i, slot.ID, slot.Event.Title,
                      slot.StartTime, slot.EndTime,
                      slot.Venue.ToString());
                Console.WriteLine(Environment.NewLine);
            }

            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }

        private Schedule GetBestSchedule(List<Schedule> population, int highestFitness)
        {
            for (int i = 0; i < population.Count; i++)
            {
                if (population[i].Fitness == highestFitness)
                {
                    return population[i];
                }
            }

            return null;
        }

        private void Mutation(List<Schedule> population)
        {
            Random r = new Random(DateTime.Now.Millisecond);
            Int32 mutationValue = (int)(1 / mutationRate);

            foreach (Schedule schedule in population)
            {
                StringBuilder sb = new StringBuilder(schedule.ScheduleString);

                for (int i = 0; i < sb.Length; i++)
                {
                    Int32 random = r.Next(0, mutationValue);
                    if (random == 1)
                    {
                        if (sb[i].Equals('0'))
                        {
                            sb[i] = '1';
                        }
                        else
                        {
                            sb[i] = '0';
                        }
                    }
                    schedule.ScheduleString = sb.ToString();
                }
            }
        }

        private int GetHightestFitness(List<Schedule> oldPopulation)
        {
            //Get highest fitness
            int highestFitness = 0;
            for (int i = 0; i < oldPopulation.Count; i++)
            {
                if (oldPopulation[i].Fitness > highestFitness)
                {
                    highestFitness = oldPopulation[i].Fitness;
                }
            }

            return highestFitness;
        }

        private List<Schedule> CrossOver(List<Schedule> oldPopulation)
        {
            List<Schedule> newPopulation = new List<Schedule>();
            int highestFitness = GetHightestFitness(oldPopulation);
            Random r = new Random(DateTime.Now.Millisecond);
            bool parentSelected = false;

            //Replace all the chromosomes with child chromosomes.
            for (int scheduleIdx = 0; scheduleIdx < oldPopulation.Count; scheduleIdx++)
            {
                Schedule parent1 = new Schedule();
                Schedule parent2 = new Schedule();

                //Loop through chromosomes twice to pick 2 parents.
                for (int parentIdx = 0; parentIdx < 2; parentIdx++)
                {
                    parentSelected = false;
                    int scheduleIdx2 = 0;

                    while (parentSelected == false && scheduleIdx2 < oldPopulation.Count)
                    {
                        scheduleIdx2++;
                        Schedule candidate = oldPopulation[r.Next(0, oldPopulation.Count - 1)];
                        //int fitnessToBeat = r.Next(0, highestFitness);

                        if (candidate.Fitness >= highestFitness)
                        {
                            if (parentIdx == 0)
                            {
                                parent1 = candidate;
                                parentSelected = true;
                            }
                            else
                            {
                                //Avoid picking same as parent 1.
                                if (candidate.ScheduleString != parent1.ScheduleString)
                                {
                                    parent2 = candidate;
                                    parentSelected = true;
                                }
                            }
                        }
                    } 
                }

                //If we've got to the end of the
                //chromosomes without picking parent 2, choose a random parent.
                if (!parentSelected)
                {
                    parent2 = oldPopulation[r.Next(0, oldPopulation.Count - 1)];
                }

                int flipPos = r.Next(0, parent1.ScheduleString.Length);
                string newSchedule = parent1.ScheduleString.Substring(0, flipPos) + 
                    parent2.ScheduleString.Substring(flipPos);
                Schedule child = new Schedule() { ScheduleString = newSchedule };
                newPopulation.Add(child);
            }

            return newPopulation;
        }

        private Dictionary<EnumVenues, Dictionary<EnumVenues, TravelTime>> PopulateDefaultTravelTimes(DataTable travelTimeData)
        {
 
            //foreach (DataRow item in travelTimeData.Rows)
            //{
               
            //    i++;
            //    string travelTime = item[0].ToString();
            //    //Event newEvent = new Event();
            //    //newEvent.ID = i;
            //    //newEvent.Title = item[0].ToString();
            //    //retVal.Add(item[0].ToString(), newEvent);
            //}



            Dictionary<EnumVenues, Dictionary<EnumVenues, TravelTime>> retVal = new Dictionary<EnumVenues, Dictionary<EnumVenues, TravelTime>>();
            var values = Helpers.GetEnumValues<EnumVenues>();
            int i = -1;

            foreach (var origin in values)
            {
                Dictionary<EnumVenues, TravelTime> travelTimes = new Dictionary<EnumVenues, TravelTime>();
                DataRow row = travelTimeData.Rows[(int)origin];
                foreach (var destination in values)
                {
                    i++;
                    TravelTime travelTime = new TravelTime();
                    travelTime.ID = i;
                    travelTime.Origin = origin;
                    travelTime.Destination = destination;
                    string travelTimeToParse = row.ItemArray[(int)destination].ToString();
                    string[] stringBits = travelTimeToParse.Split(new char[] {'-'});
                    int stringValue;
                    if (Int32.TryParse(stringBits[0], out stringValue))
                    {
                        travelTime.OnFoot = stringValue;
                    }
                    if (Int32.TryParse(stringBits[1], out stringValue))
                    {
                        travelTime.ByCar = stringValue;
                    }
                    travelTimes.Add(destination, travelTime);
                }

                retVal.Add(origin, travelTimes);
            }

            return retVal;
        }

        private Dictionary<string, Film> PopulateFilms(DataTable data)
        {
            Dictionary<string, Film> retVal = new Dictionary<string, Film>();
            int i = -1;

            foreach (DataRow item in data.Rows)
            {
                if (retVal.Any(e => e.Value.Title == item[0].ToString()))
                {
                    continue;
                }
                i++;
                Film newEvent = new Film();
                newEvent.ID = i;
                newEvent.Title = item[0].ToString();
                retVal.Add(item[0].ToString(), newEvent);
            }
           
            return retVal;
        }

        private Dictionary<string, Film> PopulateDefaultSelectedFilms(Dictionary<string, Film> events)
        {
            Dictionary<string, Film> retVal = new Dictionary<string, Film>();
         //   retVal.Add("The Taking", events["The Taking"]);
            //retVal.Add("Louis le Prince International Short Film Competition (Program 1)", events["Louis le Prince International Short Film Competition (Program 1)"]);
            //retVal.Add("Louis le Prince International Short Film Competition (Program 2)", events["Louis le Prince International Short Film Competition (Program 2)"]);
            //retVal.Add("Louis le Prince International Short Film Competition (Program 3)", events["Louis le Prince International Short Film Competition (Program 3)"]);
            //retVal.Add("Louis le Prince International Short Film Competition (Program 4)", events["Louis le Prince International Short Film Competition (Program 4)"]);
            //retVal.Add("Louis le Prince International Short Film Competition (Program 5)", events["Louis le Prince International Short Film Competition (Program 5)"]);
            //retVal.Add("Louis le Prince International Short Film Competition (Program 6)", events["Louis le Prince International Short Film Competition (Program 6)"]);
            //int i = 0;

            //foreach (KeyValuePair<string,Event> item in events)
            //{
            //    i++;
            //    if (i % 2 == 0)
            //    {
            //        retVal.Add(item.Key, item.Value);
            //    }
            //}

            //int i = 0;

            //foreach (KeyValuePair<string, Event> item in events)
            //{
            //    i++;
            //    if (i > 6)
            //    {
            //        break;
            //    }

            //    retVal.Add(item.Key, item.Value); 
            //}
            return retVal;
        }


        private List<DateRange> PopulateDefaultSelectedDateRanges(List<TimeSlot> slots)
        {
            List<DateRange> retVal = new List<DateRange>();
            //int i = -1;
            //
            //foreach (Slot item in slots)
            //{
            //    i++;
            //    if (i % 2 == 0)
            //    {
            //        retVal.Add(new DateRange(item.StartTime, item.EndTime));
            //    }
            //}

            // Add all day 1 events
          // retVal.Add(new DateRange(new DateTime(2014, 11, 10, 00, 00, 00), new DateTime(2014, 11, 10, 23, 59, 59)));

            return retVal;
        }

        private List<TimeSlot> PopulateTimeSlots(DataTable data, Dictionary<string, Film> films)
        {
            List<TimeSlot> slots = new List<TimeSlot>();

            int i = -1;

            foreach (DataRow item in data.Rows)
            {
                i++;
                TimeSlot slot = new TimeSlot();
                slot.ID = i;
                slot.Venue = Helpers.ParseLocation(item[5].ToString());
                slot.Event = films[item[0].ToString()];
                DateTime eventTime;
                if (DateTime.TryParse(item[1].ToString() + " " + item[2].ToString(), out eventTime))
                {
                    slot.StartTime = eventTime;
                }
                if (DateTime.TryParse(item[3].ToString() + " " + item[4].ToString(), out eventTime))
                {
                    slot.EndTime = eventTime;
                }
                slots.Add(slot);
            }
    
            return slots;
        }

        private void PreloadTimeSlotsWithSelectedFilms(FitnessRequest request)
        {
            if (request.ReplacementGenes.Length == 0) { return; }
            int replacementLength = request.ReplacementGenes.Length;
            foreach (Schedule schedule in request.Population)
            {
                //Starting at the start of each schedule, overwrite with slots containing selected events
                string rewrittenSchedule = request.ReplacementGenes + schedule.ScheduleString.Substring(replacementLength);
                schedule.ScheduleString = rewrittenSchedule;
            }
        }

        private List<Schedule> PopulateSchedules(int numberOfGenes, int geneLength)
        {
            List<Schedule> schedules = new List<Schedule>();
            Random r = new Random();

            for (int scheduleIdx = 0; scheduleIdx < numberOfSchedules; scheduleIdx++)
            {
                Schedule schedule = new Schedule();
                
                //If random number is same as number of slots, that's an empty slot (because range starts at 0
                //and ends at number of slots - 1 in the slot list).
                for (int geneIdx = 0; geneIdx < numberOfGenes; geneIdx++)
                {
                    int random = r.Next(0, numberOfGenes);
                    schedule.ScheduleString += Helpers.ConvertIntToBinaryString(random, geneLength);
                }
                schedules.Add(schedule);
            }

            return schedules;
        }
        #endregion
    }
}
