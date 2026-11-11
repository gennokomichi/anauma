import numpy as np
import pandas as pd

from .constants import (
    TRACK_NAMES,
    convert_surface,
)


def add_target_features(
    history_df: pd.DataFrame,
    target_df: pd.DataFrame
) -> pd.DataFrame:
    """
    過去の確定レースだけを使って、
    当日出走馬用の特徴量を作る。

    history_df:
        過去の確定済みレース

    target_df:
        当日出走馬
    """

    target = target_df.copy()

    # =====================================
    # 当日データ基本整形
    # =====================================

    target["RaceDate"] = pd.to_datetime(
        target["RaceDate"]
    )

    target["TrackName"] = (
        target["TrackCode"]
        .map(TRACK_NAMES)
    )

    target["Surface"] = (
        target["TrackType"]
        .apply(convert_surface)
    )

    # =====================================
    # 当日朝は馬体重未発表なら
    # 0の可能性があるためNaN扱い
    # =====================================

    target.loc[
        target["HorseWeight"] <= 0,
        "HorseWeight"
    ] = np.nan

    rows = []

    # =====================================
    # 当日各馬ごとに過去データを検索
    # =====================================

    for _, row in target.iterrows():

        horse_id = row["HorseID"]

        history = history_df[
            (
                history_df["HorseID"]
                == horse_id
            )
            &
            (
                history_df["RaceDate"]
                < row["RaceDate"]
            )
        ].sort_values(
            [
                "RaceDate",
                "RaceID",
            ]
        )

        # =================================
        # 前走
        # =================================

        if len(history) >= 1:

            last1 = history.iloc[-1]

            row["Last1Finish"] = (
                last1["ResultOrder"]
            )

            row["Last1Popularity"] = (
                last1["Popularity"]
            )

            # =============================
            # 前走からの日数
            # =============================

            last_race_date = pd.to_datetime(
                last1["RaceDate"]
            )

            row["DaysSinceLastRace"] = float(
                (
                    row["RaceDate"]
                    - last_race_date
                ).days
            )

            # =============================
            # 距離変化
            # =============================

            row["DistanceChange"] = float(
                row["Distance"]
                - last1["Distance"]
            )

            # =============================
            # 斤量変化
            # =============================

            row["WeightCarriedChange"] = float(
                row["WeightCarried"]
                - last1["WeightCarried"]
            )

        else:

            row["Last1Finish"] = np.nan

            row["Last1Popularity"] = np.nan

            row["DaysSinceLastRace"] = np.nan

            row["DistanceChange"] = np.nan

            row["WeightCarriedChange"] = np.nan

        # =================================
        # 過去3走
        # =================================

        last3 = history.tail(3)

        if len(last3) > 0:

            row["Last3AvgFinish"] = (
                last3[
                    "ResultOrder"
                ].mean()
            )

            row["Last3AvgPopularity"] = (
                last3[
                    "Popularity"
                ].mean()
            )

            row["Last3Top3Rate"] = (
                (
                    last3[
                        "ResultOrder"
                    ] <= 3
                ).mean()
            )

        else:

            row["Last3AvgFinish"] = np.nan

            row["Last3AvgPopularity"] = np.nan

            row["Last3Top3Rate"] = np.nan

        # =================================
        # 過去5走
        # =================================

        last5 = history.tail(5)

        if len(last5) > 0:

            row["Last5AvgFinish"] = (
                last5[
                    "ResultOrder"
                ].mean()
            )

            row["Last5AvgPopularity"] = (
                last5[
                    "Popularity"
                ].mean()
            )

            row["Last5Top3Rate"] = (
                (
                    last5[
                        "ResultOrder"
                    ] <= 3
                ).mean()
            )

        else:

            row["Last5AvgFinish"] = np.nan

            row["Last5AvgPopularity"] = np.nan

            row["Last5Top3Rate"] = np.nan

        # =================================
        # 同距離
        # =================================

        same_distance = history[
            history["Distance"]
            == row["Distance"]
        ]

        if len(same_distance) > 0:

            row["SameDistanceTop3Rate"] = (
                (
                    same_distance[
                        "ResultOrder"
                    ] <= 3
                ).mean()
            )

        else:

            row["SameDistanceTop3Rate"] = np.nan

        # =================================
        # 馬 × 競馬場
        # =================================

        same_track = history[
            history["TrackCode"]
            == row["TrackCode"]
        ]

        row["HorseTrackStarts"] = float(
            len(same_track)
        )

        if len(same_track) > 0:

            row["HorseTrackTop3Rate"] = (
                (
                    same_track[
                        "ResultOrder"
                    ] <= 3
                ).mean()
            )

        else:

            row["HorseTrackTop3Rate"] = np.nan

        # =================================
        # 馬 × Surface
        # =================================

        same_surface = history[
            history["Surface"]
            == row["Surface"]
        ]

        row["HorseSurfaceStarts"] = float(
            len(same_surface)
        )

        if len(same_surface) > 0:

            row["HorseSurfaceTop3Rate"] = (
                (
                    same_surface[
                        "ResultOrder"
                    ] <= 3
                ).mean()
            )

        else:

            row["HorseSurfaceTop3Rate"] = np.nan

        rows.append(
            row
        )

    result = pd.DataFrame(
        rows
    )

    return result