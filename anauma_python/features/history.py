import numpy as np
import pandas as pd


def add_historical_features(
    df: pd.DataFrame
) -> pd.DataFrame:
    """
    過去の確定レース用特徴量。

    今回レースより前の情報だけを使う。
    未来情報は使用しない。
    """

    df = df.copy()

    # =====================================
    # 馬単位
    # =====================================

    horse_group = df.groupby(
        "HorseID",
        sort=False
    )

    # =====================================
    # 前走
    # =====================================

    df["Last1Finish"] = (
        horse_group["ResultOrder"]
        .shift(1)
    )

    df["Last1Popularity"] = (
        horse_group["Popularity"]
        .shift(1)
    )

    # =====================================
    # 過去3走
    # =====================================

    df["Last3AvgFinish"] = (
        horse_group["ResultOrder"]
        .transform(
            lambda x:
            x.shift(1)
            .rolling(
                3,
                min_periods=1
            )
            .mean()
        )
    )

    df["Last3AvgPopularity"] = (
        horse_group["Popularity"]
        .transform(
            lambda x:
            x.shift(1)
            .rolling(
                3,
                min_periods=1
            )
            .mean()
        )
    )

    df["Last3Top3Rate"] = (
        horse_group["Target"]
        .transform(
            lambda x:
            x.shift(1)
            .rolling(
                3,
                min_periods=1
            )
            .mean()
        )
    )

    # =====================================
    # 過去5走
    # =====================================

    df["Last5AvgFinish"] = (
        horse_group["ResultOrder"]
        .transform(
            lambda x:
            x.shift(1)
            .rolling(
                5,
                min_periods=1
            )
            .mean()
        )
    )

    df["Last5AvgPopularity"] = (
        horse_group["Popularity"]
        .transform(
            lambda x:
            x.shift(1)
            .rolling(
                5,
                min_periods=1
            )
            .mean()
        )
    )

    df["Last5Top3Rate"] = (
        horse_group["Target"]
        .transform(
            lambda x:
            x.shift(1)
            .rolling(
                5,
                min_periods=1
            )
            .mean()
        )
    )

    # =====================================
    # 前走からの日数
    # =====================================

    previous_race_date = (
        horse_group["RaceDate"]
        .shift(1)
    )

    df["DaysSinceLastRace"] = (
        df["RaceDate"]
        - previous_race_date
    ).dt.days.astype(float)

    # =====================================
    # 前走からの距離変化
    #
    # 例:
    # 前走1800m → 今回1600m
    # -200
    #
    # 前走1200m → 今回1600m
    # +400
    # =====================================

    previous_distance = (
        horse_group["Distance"]
        .shift(1)
    )

    df["DistanceChange"] = (
        df["Distance"]
        - previous_distance
    ).astype(float)

    # =====================================
    # 前走からの斤量変化
    #
    # 例:
    # 前走57kg → 今回55kg
    # -2.0
    # =====================================

    previous_weight_carried = (
        horse_group["WeightCarried"]
        .shift(1)
    )

    df["WeightCarriedChange"] = (
        df["WeightCarried"]
        - previous_weight_carried
    ).astype(float)

    # =====================================
    # 馬自身 × 同距離
    # =====================================

    distance_group = df.groupby(
        [
            "HorseID",
            "Distance",
        ],
        sort=False
    )

    previous_distance_top3 = (
        distance_group["Target"].cumsum()
        - df["Target"]
    )

    previous_distance_count = (
        distance_group.cumcount()
    )

    df["SameDistanceTop3Rate"] = (
        previous_distance_top3
        /
        previous_distance_count.replace(
            0,
            np.nan
        )
    ).astype(float)

    # =====================================
    # 馬自身 × 競馬場
    # =====================================

    horse_track_group = df.groupby(
        [
            "HorseID",
            "TrackCode",
        ],
        sort=False
    )

    previous_horse_track_top3 = (
        horse_track_group["Target"].cumsum()
        - df["Target"]
    )

    previous_horse_track_count = (
        horse_track_group.cumcount()
    )

    df["HorseTrackTop3Rate"] = (
        previous_horse_track_top3
        /
        previous_horse_track_count.replace(
            0,
            np.nan
        )
    ).astype(float)

    df["HorseTrackStarts"] = (
        previous_horse_track_count
        .astype(float)
    )

    # =====================================
    # 馬自身 × 芝 / ダート / 障害
    # =====================================

    horse_surface_group = df.groupby(
        [
            "HorseID",
            "Surface",
        ],
        sort=False
    )

    previous_horse_surface_top3 = (
        horse_surface_group["Target"].cumsum()
        - df["Target"]
    )

    previous_horse_surface_count = (
        horse_surface_group.cumcount()
    )

    df["HorseSurfaceTop3Rate"] = (
        previous_horse_surface_top3
        /
        previous_horse_surface_count.replace(
            0,
            np.nan
        )
    ).astype(float)

    df["HorseSurfaceStarts"] = (
        previous_horse_surface_count
        .astype(float)
    )

    return df