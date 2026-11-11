TRACK_NAMES = {
    "01": "札幌",
    "02": "函館",
    "03": "福島",
    "04": "新潟",
    "05": "東京",
    "06": "中山",
    "07": "中京",
    "08": "京都",
    "09": "阪神",
    "10": "小倉",
}


FEATURES = [
    # =====================================
    # 今回のレース情報
    # =====================================
    "GateNumber",
    "FrameNumber",
    "WeightCarried",
    "HorseWeight",
    "HorseWeightDiff",
    "Distance",
    "RaceNumber",

    # =====================================
    # 前走
    # =====================================
    "Last1Finish",
    "Last1Popularity",

    # =====================================
    # 過去3走
    # =====================================
    "Last3AvgFinish",
    "Last3AvgPopularity",
    "Last3Top3Rate",

    # =====================================
    # 過去5走
    # =====================================
    "Last5AvgFinish",
    "Last5AvgPopularity",
    "Last5Top3Rate",

    # =====================================
    # 前走から今回への変化
    # =====================================
    "DaysSinceLastRace",
    "DistanceChange",
    "WeightCarriedChange",

    # =====================================
    # 馬自身 × 同距離
    # =====================================
    "SameDistanceTop3Rate",

    # =====================================
    # 馬自身 × 競馬場
    # =====================================
    "HorseTrackTop3Rate",
    "HorseTrackStarts",

    # =====================================
    # 馬自身 × 芝・ダート・障害
    # =====================================
    "HorseSurfaceTop3Rate",
    "HorseSurfaceStarts",
]


def convert_surface(track_type):
    """
    JRA-VANのTrackTypeコードを
    Turf / Dirt / Jump に変換する。
    """

    try:
        code = int(track_type)

    except (TypeError, ValueError):
        return "Unknown"

    # 平地 芝
    if 10 <= code <= 22:
        return "Turf"

    # 平地 ダート
    if 23 <= code <= 29:
        return "Dirt"

    # 障害
    if 51 <= code <= 59:
        return "Jump"

    return "Unknown"