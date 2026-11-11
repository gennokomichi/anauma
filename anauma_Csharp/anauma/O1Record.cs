using System;
using System.Collections.Generic;

namespace anauma;

internal sealed class O1Record
{
    public string RaceId { get; set; } = "";

    // O1データ区分
    public int DataKubun { get; set; }

    // JV-Data上の発表月日時分
    // MMDDHHMM
    public string PublishedRaw { get; set; } = "";

    // 年を補完した発表日時
    public DateTime? PublishedAt { get; set; }

    public List<O1WinOdds> WinOdds { get; set; }
        = new List<O1WinOdds>();
}


internal sealed class O1WinOdds
{
    public int HorseNumber { get; set; }

    public double Odds { get; set; }

    public int Popularity { get; set; }
}