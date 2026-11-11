using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace anauma
{
    internal class RaRecord
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

        public string RaceNameMain { get; set; } = "";

        public string GradeCode { get; set; } = "";
        public string RaceTypeCode { get; set; } = "";
        public string Distance { get; set; } = "";
        public string TrackCode { get; set; } = "";

        public string StartTime { get; set; } = "";

        public string StartersCount { get; set; } = "";
        public string FinishersCount { get; set; } = "";

        public string WeatherCode { get; set; } = "";
        public string TurfConditionCode { get; set; } = "";
        public string DirtConditionCode { get; set; } = "";
    }
}
