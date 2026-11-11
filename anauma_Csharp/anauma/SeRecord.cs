using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace anauma
{
    internal class SeRecord
    {
        public string RecordType { get; set; } = "";
        public string DataKubun { get; set; } = "";
        public string DataCreateDate { get; set; } = "";

        public string Year { get; set; } = "";
        public string MonthDay { get; set; } = "";
        public string RacecourseCode { get; set; } = "";
        public string Kai { get; set; } = "";
        public string Day { get; set; } = "";
        public string RaceNumber { get; set; } = "";

        public string FrameNumber { get; set; } = "";
        public string HorseNumber { get; set; } = "";
        public string BloodlineNumber { get; set; } = "";
        public string HorseName { get; set; } = "";

        public string WeightCarried { get; set; } = "";

        public string JockeyCode { get; set; } = "";
        public string JockeyName { get; set; } = "";

        public string HorseWeight { get; set; } = "";
        public string HorseWeightSign { get; set; } = "";
        public string HorseWeightDiff { get; set; } = "";

        public string FinishOrder { get; set; } = "";

        public string WinOdds { get; set; } = "";
        public string WinPopularity { get; set; } = "";
    }
}
