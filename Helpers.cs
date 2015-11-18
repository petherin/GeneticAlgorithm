using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace GeneticAlgorithm
{
    static public class Helpers
    {
        public static string ConvertIntToBinaryString(int x, int geneLength)
        {
            char[] bits = new char[geneLength];
            int i = 0;

            while (x != 0)
            {
                bits[i++] = (x & 1) == 1 ? '1' : '0';
                x >>= 1;
            }

            Array.Reverse(bits, 0, i);
            string retVal = new string(bits);
            int slashPos = retVal.IndexOf('\0');
            if (slashPos > -1)
            {
                retVal = retVal.Substring(0, slashPos);
            }
            return retVal.PadLeft(geneLength, '0');
        }

        public static string ConvertIntToBinaryString(int x)
        {
            char[] bits = new char[1000];
            int i = 0;

            while (x != 0)
            {
                bits[i++] = (x & 1) == 1 ? '1' : '0';
                x >>= 1;
            }

            Array.Reverse(bits, 0, i);
            string retVal = new string(bits);
            int slashPos = retVal.IndexOf('\0');
            retVal = retVal.Substring(0, slashPos);
            return retVal;
        }

        public static IEnumerable<string> SplitIntoChunks(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static DataTable GetDataTableFromCSVFile(string csv_file_path)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    //read column names
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return csvData;
        }

        public static GeneticAlgorithm.Scheduler.EnumVenues ParseLocation(string location)
        {
            if (location == "Leeds")
            {
                return Scheduler.EnumVenues.Leeds; 
            }

            if (location.Contains("Victoria"))
            {
                return Scheduler.EnumVenues.LeedsTownHall_VictoriaHall;
            }

            if (location.Contains("Albert"))
            {
                return Scheduler.EnumVenues.LeedsTownHall_AlbertRoom;
            }

            if (location.Contains("Vue"))
            {
                return Scheduler.EnumVenues.Vue;
            }

            if (location.Contains("Hyde"))
            {
                return Scheduler.EnumVenues.HydePark;
            }

            if (location.Contains("Everyman"))
            {
                return Scheduler.EnumVenues.Everyman;
            }

            if (location.Contains("Left Bank"))
            {
                return Scheduler.EnumVenues.LeftBank;
            }

            if (location.Contains("Carriage"))
            {
                return Scheduler.EnumVenues.Carriageworks;
            }

            if (location.Contains("Varieties"))
            {
                return Scheduler.EnumVenues.CityVarieties;
            }

            if (location.Contains("Chapel"))
            {
                return Scheduler.EnumVenues.ChapelFMSeacroft;
            }

            if (location.Contains("Armouries"))
            {
                return Scheduler.EnumVenues.RoyalArmouries;
            }

            if (location.Contains("Belgrave"))
            {
                return Scheduler.EnumVenues.BelgraveMusicHall;
            }

            if (location.Contains("Tetley"))
            {
                return Scheduler.EnumVenues.TheTetley;
            }

            if (location.Contains("Cottage"))
            {
                return Scheduler.EnumVenues.CottageRoadCinema;
            }

            if (location.Contains("Reliance"))
            {
                return Scheduler.EnumVenues.TheReliance;
            }

            if (location.Contains("University"))
            {
                return Scheduler.EnumVenues.LeedsUniversity;
            }

            return Scheduler.EnumVenues.Leeds; 
        }

        static public List<TimeSlot> GetSlots(List<string> genes, List<TimeSlot> allSlots)
        {
            List<TimeSlot> slots = new List<TimeSlot>();
            foreach (string gene in genes)
            {
                int slotIndex = Convert.ToInt32(gene, 2);
                if (slotIndex > allSlots.Count - 1)
                {
                    continue;
                }
                slots.Add(allSlots[slotIndex]);
            }
            return slots;
        }
    }

}
