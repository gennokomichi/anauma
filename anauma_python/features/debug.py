import pandas as pd

from .constants import FEATURES


def print_feature_check(
    df: pd.DataFrame
) -> None:
    """
    過去特徴量の確認用。
    """

    print()
    print(
        "===== 過去成績特徴量 確認 ====="
    )

    check_columns = [
        "RaceDate",
        "HorseName",
        "TrackName",
        "Surface",
        "ResultOrder",
        "Popularity",

        # 前走
        "Last1Finish",
        "Last1Popularity",

        # 過去3走
        "Last3AvgFinish",
        "Last3AvgPopularity",
        "Last3Top3Rate",

        # 過去5走
        "Last5AvgFinish",
        "Last5AvgPopularity",
        "Last5Top3Rate",

        # 前走から今回への変化
        "DaysSinceLastRace",
        "DistanceChange",
        "WeightCarriedChange",

        # 適性
        "SameDistanceTop3Rate",

        "HorseTrackTop3Rate",
        "HorseTrackStarts",

        "HorseSurfaceTop3Rate",
        "HorseSurfaceStarts",
    ]

    print(
        df[
            check_columns
        ].tail(30)
    )

    print()
    print(
        "===== Surface確認 ====="
    )

    print(
        df["Surface"]
        .value_counts(
            dropna=False
        )
    )

    print()
    print(
        "===== TrackType → Surface確認 ====="
    )

    print(
        df[
            [
                "TrackType",
                "Surface",
            ]
        ]
        .drop_duplicates()
        .sort_values(
            "TrackType"
        )
        .to_string(
            index=False
        )
    )

    print()
    print(
        "===== 特徴量の型確認 ====="
    )

    print(
        df[
            FEATURES
        ].dtypes
    )