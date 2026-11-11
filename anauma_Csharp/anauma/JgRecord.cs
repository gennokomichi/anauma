using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace anauma
{
    internal class JgRecord
    {
        // レコード種別ID
        public string RecordType { get; set; } = "";

        // データ区分
        public string DataKubun { get; set; } = "";

        // データ作成年月日
        public string DataCreateDate { get; set; } = "";

        // 開催年
        public string Year { get; set; } = "";

        // 開催月日
        public string MonthDay { get; set; } = "";

        // 競馬場コード
        public string RacecourseCode { get; set; } = "";

        // 開催回
        public string Kai { get; set; } = "";

        // 開催日
        public string Day { get; set; } = "";

        // レース番号
        public string RaceNumber { get; set; } = "";

        // 血統登録番号
        public string BloodlineNumber { get; set; } = "";

        // 馬名
        public string HorseName { get; set; } = "";

        // 出馬投票受付順番
        public string EntryOrder { get; set; } = "";

        // 出走区分
        public string RunningStatus { get; set; } = "";

        // 除外状態区分
        public string ExclusionStatus { get; set; } = "";
    }
}