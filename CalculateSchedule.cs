using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticAlgorithm
{
    public class CalculateSchedule
    {
        public bool Calculate(FitnessRequest request)
        {
            //Todo
            //Change this so each chromosome is converted into a List<Slot> so LINQ can be used. Not getting correct shedule when choose selected events.
            for (int popIdx = 0; popIdx < request.Population.Count(); popIdx++)
            {
                //Process chromosome.
                List<string> genes = Helpers.SplitIntoChunks(request.Population[popIdx].ScheduleString, request.GeneLength).ToList<string>();
                List<TimeSlot> chromosomeSlots = Helpers.GetSlots(genes, request.TimeSlots);

                DateRange defaultDateRange = new DateRange(new DateTime(1000, 1, 1, 00, 00, 00), new DateTime(9999, 12, 31, 23, 59, 59));
                List<TimeSlot> filteredSlots = chromosomeSlots
                    .Where(slot =>
                    request.SelectedDateRanges.DefaultIfEmpty(defaultDateRange).
                    Any(dateRange => dateRange.Start < slot.EndTime && dateRange.End > slot.StartTime)).ToList<TimeSlot>();

                //Dedupe duplicate slots
                List<TimeSlot> dedupedSlots = new List<TimeSlot>();
                foreach (TimeSlot slot in filteredSlots)
                {
                    if (!dedupedSlots.Contains(slot))
                    {
                        dedupedSlots.Add(slot);
                    }
                }

                ////Selected events are messing this up. It works when there's selected dates
                //or no selected dates, but not with selected events. For some reason,
                //only a few events around the selected events are included, like 2 
                //either side. Something about selected events breaks the clash code.
                ////After the loop above, all slots not in selected date range have been filtered out.
                // //Now we can go through those and see which ones contain selected events.
                foreach (TimeSlot slot in dedupedSlots)
                {
                    if (request.SelectedFilms.Count == 0) { break; }
                    if (SlotHasSelectedEvent(request, slot, popIdx))
                    {
                        slot.Selected = true;
                        request.Population[popIdx].Fitness++;
                        request.Population[popIdx].UniqueSlots.Add(slot);
                        request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
                    }
                }

                //if (request.Population[popIdx].Fitness < request.SelectedEvents.Count)
                //{
                //    request.Population[popIdx].Fitness = 0;
                //    request.Population[popIdx].UniqueSlots.Clear();
                //    request.Population[popIdx].UniqueEvents.Clear();
                //    continue;
                //}

                List<TimeSlot> sortedSlots = dedupedSlots.OrderBy(o => o.StartTime).ToList();

                //Now we've got all the slots with selected events marked as Selected. They're fixed now.
                //If we want more than just the selected events, get the remaining unselected slots that don't clash
                //with any other currently unselected slot.
                if (request.SelectedFilmsOnly == false)
                {
                    // foreach (Slot slot in filteredSlots.Where(s => s.Selected == false))
                    for (int i = 0; i < sortedSlots.Count; i++)
                    {
                        TimeSlot slot = sortedSlots[i];
                        if (slot.Selected) { continue; }
                        bool fits = true;
                        // foreach (Slot sibling in filteredSlots.Where(sib => sib.ID != slot.ID))
                        for (int j = i + 1; j < sortedSlots.Count; j++)
                        {
                            //Don't compare same slot.
                           //  if (j == i) { continue; }
                            TimeSlot sibling = sortedSlots[j];
                            //if (sibling.Selected) 
                            //{
                            //    continue; 
                            //}

                            //if ((sibling.ID == 74 & slot.ID == 113) || (sibling.ID == 113 & slot.ID == 74))
                            //{
                            //    break;
                            //}
                            int addedTime = CalculateAddedTime(request, slot, sibling);
                            DateTime slotAmendedStart = slot.StartTime.AddMinutes(addedTime * -1);
                            DateTime slotAmendedEnd = slot.EndTime.AddMinutes(addedTime);

                            fits = !(slotAmendedStart < sibling.EndTime && slotAmendedEnd > sibling.StartTime);
                            if (!fits)
                            {
                                break;
                            }
                            //113 louis 74 other
                            //Do we allow duplicate event IDs?
                            fits = ((request.AllowDuplicateEvents) || (request.AllowDuplicateEvents == false && slot.Event.ID != sibling.Event.ID));

                            if (!fits)
                            {
                                break;
                            }
                        }
                        // }

                        if (fits)
                        {
                            slot.Selected = true;
                            request.Population[popIdx].Fitness++;
                            request.Population[popIdx].UniqueSlots.Add(slot);
                            request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
                        }
                    }
                }
            }

            return true;
        }
        //public bool Calculate(FitnessRequest request)
        //{
        //    //Todo
        //    //Change this so each chromosome is converted into a List<Slot> so LINQ can be used. Not getting correct shedule when choose selected events.
        //    for (int popIdx = 0; popIdx < request.Population.Count(); popIdx++)
        //    {
        //        //Process chromosome.
        //        List<string> genes = Helpers.SplitIntoChunks(request.Population[popIdx].ChromosomeString, request.GeneLength).ToList<string>();
        //        List<Slot> chromosomeSlots = Helpers.GetSlots(genes, request.Slots);
              
        //        DateRange defaultDateRange = new DateRange(new DateTime(1000, 1, 1, 00, 00, 00), new DateTime(9999, 12, 31, 23, 59, 59));
        //        List<Slot> filteredSlots = chromosomeSlots
        //            .Where(slot =>
        //            request.SelectedDateRanges.DefaultIfEmpty(defaultDateRange).
        //            Any(dateRange => dateRange.Start < slot.EndTime && dateRange.End > slot.StartTime)).ToList<Slot>();

        //        //Dedupe duplicate slots
        //        List<Slot> dedupedSlots = new List<Slot>();
        //        foreach (Slot slot in filteredSlots)
        //        {
        //            if (!dedupedSlots.Contains(slot))
        //            {
        //                dedupedSlots.Add(slot);
        //            }
        //        }
           
        //        ////Selected events are messing this up. It works when there's selected dates
        //        //or no selected dates, but not with selected events. For some reason,
        //        //only a few events around the selected events are included, like 2 
        //        //either side. Something about selected events breaks the clash code.
        //       ////After the loop above, all slots not in selected date range have been filtered out.
        //       // //Now we can go through those and see which ones contain selected events.
        //        foreach (Slot slot in dedupedSlots)
        //        {
        //            if (SlotHasSelectedEvent(request, slot, popIdx))
        //            {
        //                slot.Selected = true;
        //                request.Population[popIdx].Fitness++;
        //                request.Population[popIdx].UniqueSlots.Add(slot);
        //                request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
        //            }
        //        }

        //        if (request.Population[popIdx].Fitness < request.SelectedEvents.Count)
        //        {
        //            request.Population[popIdx].Fitness = 0;
        //            request.Population[popIdx].UniqueSlots.Clear();
        //            request.Population[popIdx].UniqueEvents.Clear();
        //            continue;
        //        }

        //        List<Slot> sortedSlots = dedupedSlots.OrderBy(o => o.StartTime).ToList();

        //        //Now we've got all the slots with selected events marked as Selected. They're fixed now.
        //        //If we want more than just the selected events, get the remaining unselected slots that don't clash
        //        //with any other currently unselected slot.
        //        if (request.SelectedEventsOnly == false)
        //        {
        //           // foreach (Slot slot in filteredSlots.Where(s => s.Selected == false))
        //            for (int i = 0; i < sortedSlots.Count; i++)
        //            {
        //                Slot slot = sortedSlots[i];
        //                if (slot.Selected) { continue; }
        //                bool fits = true;
        //               // foreach (Slot sibling in filteredSlots.Where(sib => sib.ID != slot.ID))
        //                for (int j = i + 1; j < sortedSlots.Count; j++)
        //                {
        //                    //Don't compare same slot.
        //                   // if (j == i) { continue; }
        //                    Slot sibling = sortedSlots[j];
        //                    //if (sibling.Selected) { continue; }
        //                    //if ((sibling.ID == 74 & slot.ID == 113) || (sibling.ID == 113 & slot.ID == 74))
        //                    //{
        //                    //    break;
        //                    //}
        //                    int addedTime = CalculateAddedTime(request, slot, sibling);
        //                    DateTime slotAmendedStart = slot.StartTime.AddMinutes(addedTime * -1);
        //                    DateTime slotAmendedEnd = slot.EndTime.AddMinutes(addedTime);
                            
        //                    fits = !(slotAmendedStart < sibling.EndTime && slotAmendedEnd > sibling.StartTime);
        //                    if (!fits)
        //                    {
        //                        break;
        //                    }
        //                    //113 louis 74 other
        //                    //Do we allow duplicate event IDs?
        //                    fits = ((request.AllowDuplicateEvents) || (request.AllowDuplicateEvents == false && slot.Event.ID != sibling.Event.ID));

        //                    if (!fits)
        //                    {
        //                        break;
        //                    }
        //                }
        //               // }

        //                if (fits)
        //                {
        //                    slot.Selected = true;
        //                    request.Population[popIdx].Fitness++;
        //                    request.Population[popIdx].UniqueSlots.Add(slot);
        //                    request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
        //                }
        //            }
        //        }
        //        //    // if (!SlotInSelectedEvents(request, slot)) { continue; }
        //        //    if (SlotHasSelectedEvent(request, slot))
        //        //    {
        //        //        if (request.Population[popIdx].UniqueEvents.Any(l => l == slot.Event.ID) == false)
        //        //        {
        //        //            request.Population[popIdx].Fitness++;
        //        //            request.Population[popIdx].UniqueSlots.Add(slot.ID);
        //        //            request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
        //        //            continue;
        //        //        }
        //        //    }
        //        //    else
        //        //    {
        //        //        if (request.SelectedEvents.Count > 0 && request.SelectedEventsOnly == false
        //        //            && SlotFitsInSchedule(request, genes, i, slot) && (request.Population[popIdx].UniqueEvents.Any(l => l == slot.Event.ID) == false))
        //        //        {
        //        //            request.Population[popIdx].Fitness++;
        //        //            request.Population[popIdx].UniqueSlots.Add(slot.ID);
        //        //            request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
        //        //            continue;
        //        //        }
        //        //    }

        //        //}

        //    //    DateRange defaultDateRange = new DateRange(new DateTime(9999, 12, 31, 23, 59, 59), new DateTime(1000, 1, 1, 00, 00, 00));
                


        //    //   // List<Slot> filteredSlots = chromosomeSlots.Where(f => 
        //    //    //   request.SelectedDateRanges.DefaultIfEmpty(defaultDateRange).
        //    //    //    Any(s =>s.Start < f.EndTime && s.End > f.StartTime) == false).ToList<Slot>();

        //    //        //  if ((request.SelectedDateRanges.Count > 0) &&
        //    //    //((request.SelectedDateRanges.Any(s => s.Start < slot.EndTime && s.End > slot.StartTime)) == false))
           
              
        //    //    //Process a gene within chromosome.
        //    //    for (int geneIdx = 0; geneIdx < genes.Count(); geneIdx++)
        //    //    {
        //    //        slotIdx = Convert.ToInt32(genes[geneIdx], 2);

        //    //        //If slot index is higher than number of slots, skip it and move on to next gene.
        //    //        if (slotIdx > request.Slots.Count - 1) { continue; }
        //    //        Slot slot = request.Slots[slotIdx];

        //    //        if (!SlotInSelectedDateRanges(request, slot))
        //    //        {
        //    //            continue;
        //    //        }

        //    //        // if (!SlotInSelectedEvents(request, slot)) { continue; }
        //    //        if (SlotHasSelectedEvent(request, slot))
        //    //        {
        //    //            if (request.Population[popIdx].UniqueEvents.Any(l => l == slot.Event.ID) == false)
        //    //            {
        //    //                request.Population[popIdx].Fitness++;
        //    //                request.Population[popIdx].UniqueSlots.Add(slotIdx);
        //    //                request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
        //    //                continue;
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            if (request.SelectedEvents.Count > 0 && request.SelectedEventsOnly == false
        //    //                && SlotFitsInSchedule(request, genes, geneIdx, slot) && (request.Population[popIdx].UniqueEvents.Any(l => l == slot.Event.ID) == false))
        //    //            {
        //    //                request.Population[popIdx].Fitness++;
        //    //                request.Population[popIdx].UniqueSlots.Add(slotIdx);
        //    //                request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
        //    //                continue;
        //    //            }
        //    //        }

        //    //    }

        //        //int slotIdx = 0;

        //        ////Process a gene within chromosome.
        //        //for (int geneIdx = 0; geneIdx < genes.Count(); geneIdx++)
        //        //{
        //        //    slotIdx = Convert.ToInt32(genes[geneIdx], 2);

        //        //    //If slot index is higher than number of slots, skip it and move on to next gene.
        //        //    if (slotIdx > request.Slots.Count - 1) { continue; }
        //        //    Slot slot = request.Slots[slotIdx];

        //        //    if (!SlotInSelectedDateRanges(request, slot))
        //        //    {
        //        //        continue;
        //        //    }
  
        //        //   // if (!SlotInSelectedEvents(request, slot)) { continue; }
        //        //    if (SlotHasSelectedEvent(request, slot))
        //        //    {
        //        //        if (request.Population[popIdx].UniqueEvents.Any(l => l == slot.Event.ID) == false)
        //        //        {
        //        //            request.Population[popIdx].Fitness++;
        //        //            request.Population[popIdx].UniqueSlots.Add(slotIdx);
        //        //            request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
        //        //            continue;
        //        //        }
        //        //    }
        //        //    else
        //        //    {
        //        //        if (request.SelectedEvents.Count > 0 && request.SelectedEventsOnly == false 
        //        //            && SlotFitsInSchedule(request, genes, geneIdx, slot) && (request.Population[popIdx].UniqueEvents.Any(l => l == slot.Event.ID) == false))
        //        //        {
        //        //            request.Population[popIdx].Fitness++;
        //        //            request.Population[popIdx].UniqueSlots.Add(slotIdx);
        //        //            request.Population[popIdx].UniqueEvents.Add(slot.Event.ID);
        //        //            continue;
        //        //        }
        //        //    }
                    
        //        //}
                
        //    }

        //    return true;   
        //}

        //private bool SlotFitsInSchedule(FitnessRequest request, List<string> genes, int geneIdx, Slot slot)
        //{
        //    bool fits = true;

        //    //Does this slot fit with all the other slots in the chromosome?
        //    for (int siblingGeneIdx = geneIdx + 1; siblingGeneIdx < genes.Count; siblingGeneIdx++)
        //    {
        //        int siblingSlotIdx = Convert.ToInt32(genes[siblingGeneIdx], 2);

        //        //If the slot index is higher than total number of slots, treat it as a 
        //        //blank slot and say it doesn't fit. It's only fits that increase
        //        //fitness, so if blanks are seen as fit, we get an empty schedule full of
        //        //blank slots.
        //        if (siblingSlotIdx > request.Slots.Count - 1)
        //        {
        //            continue;
        //        }

        //        Slot siblingSlot = request.Slots[siblingSlotIdx];
        //        if (SlotHasSelectedEvent(request, siblingSlot)) 
        //        {
        //            fits = false;
        //            break; 
        //        }
        //        //if (!SlotInSelectedDateRanges(request, siblingSlot)) { continue; }

        //        int addedTime = CalculateAddedTime(request, slot, siblingSlot);
        //        DateTime slotAmendedStart = slot.StartTime.AddMinutes(addedTime * -1);
        //        DateTime slotAmendedEnd = slot.EndTime.AddMinutes(addedTime);

        //        //Fits if slot doesn't clash and the slot events are different
        //        fits = !(slotAmendedStart < siblingSlot.EndTime && slotAmendedEnd > siblingSlot.StartTime);

        //        if (!fits) { break; }

        //        //Do we allow duplicate event IDs?
        //        fits = ((request.AllowDuplicateEvents) || (request.AllowDuplicateEvents == false && slot.Event.ID != siblingSlot.Event.ID));

        //        if (!fits) { break; }
        //    }

        //    return fits;
        //}

        //private bool SlotFitsInSchedule2(FitnessRequest request, List<string> genes, int geneIdx, Slot slot)
        //{
        //    bool fits = true;

        //    //Does this slot fit with all the other slots in the chromosome?
        //    for (int siblingGeneIdx = geneIdx + 1; siblingGeneIdx < genes.Count; siblingGeneIdx++)
        //    {
        //        int siblingSlotIdx = Convert.ToInt32(genes[siblingGeneIdx], 2);

        //        //If the slot index is higher than total number of slots, treat it as a 
        //        //blank slot and say it doesn't fit. It's only fits that increase
        //        //fitness, so if blanks are seen as fit, we get an empty schedule full of
        //        //blank slots.
        //        if (siblingSlotIdx > request.Slots.Count - 1)
        //        {
        //            continue;
        //        }

        //        Slot siblingSlot = request.Slots[siblingSlotIdx];
        //        if (SlotHasSelectedEvent(request, siblingSlot))
        //        {
        //            fits = false;
        //            break;
        //        }
        //        //if (!SlotInSelectedDateRanges(request, siblingSlot)) { continue; }

        //        int addedTime = CalculateAddedTime(request, slot, siblingSlot);
        //        DateTime slotAmendedStart = slot.StartTime.AddMinutes(addedTime * -1);
        //        DateTime slotAmendedEnd = slot.EndTime.AddMinutes(addedTime);

        //        //Fits if slot doesn't clash and the slot events are different
        //        fits = !(slotAmendedStart < siblingSlot.EndTime && slotAmendedEnd > siblingSlot.StartTime);

        //        if (!fits) { break; }

        //        //Do we allow duplicate event IDs?
        //        fits = ((request.AllowDuplicateEvents) || (request.AllowDuplicateEvents == false && slot.Event.ID != siblingSlot.Event.ID));

        //        if (!fits) { break; }
        //    }

        //    return fits;
        //}

        private static int CalculateAddedTime(FitnessRequest request, TimeSlot slot, TimeSlot siblingSlot)
        {
            int addedTime = 0;

            if (slot.Venue != siblingSlot.Venue)
            {
                Dictionary<GeneticAlgorithm.Scheduler.EnumVenues, TravelTime> travelTimesForVenue = request.TravelTimes[slot.Venue];
                TravelTime travelTimesToDestination = travelTimesForVenue[siblingSlot.Venue];

                //Factor in travel times.
                switch (request.PreferredTravelOption)
                {
                    case GeneticAlgorithm.Scheduler.EnumTravelOptions.OnFoot:
                        addedTime = travelTimesToDestination.OnFoot;
                        break;
                    case GeneticAlgorithm.Scheduler.EnumTravelOptions.ByCar:
                        addedTime = travelTimesToDestination.ByCar;
                        break;
                    default:
                        break;
                }
            }

            addedTime += request.ContingencyMins;
            addedTime += request.GapBetweenTimeSlotsMins;

            return addedTime;
        }

        private bool SlotHasSelectedEvent(FitnessRequest request, TimeSlot slot, int popIdx)
        {
            if (request.SelectedFilms.Count == 0) { return false; }
            //Is the event in this slot one of the selected events?
            if ((request.SelectedFilms.Count > 0) && ((request.SelectedFilms.Any(e => e.Value.ID == slot.Event.ID)) == false))
            {
                //Event isn't one of the selected events.
                return false;
            }
            else
            {
                //Has the event already been added?
                if (request.Population[popIdx].UniqueEvents.Any(e => e == slot.Event.ID))
                {
                    return false;
                }
                else 
                {
                    return true;
                }
                
            }
        }

        private bool SlotInSelectedDateRanges(FitnessRequest request, TimeSlot slot)
        {
            //Is this slot within any of the selected date ranges?
            if ((request.SelectedDateRanges.Count > 0) &&
                ((request.SelectedDateRanges.Any(s => s.Start < slot.EndTime && s.End > slot.StartTime)) == false))
            {
                //If there are selected date ranges and the current slot doesn't occur within it, move on to the next gene.
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
