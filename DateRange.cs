﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticAlgorithm
{
    public class DateRange
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public bool Includes(DateRange range)
        {
            return (Start <= range.Start) && (range.End <= End);
        }

        public bool Includes(DateTime value)
        {
            return (Start <= value) && (value <= End);
        }
    }
}
