import pandas as pd

from .constants import (
    TRACK_NAMES,
    convert_surface,
)


def prepare_base_data(
    df: pd.DataFrame
) -> pd.DataFrame:
    """
    過去レース用の基本整形。

    ・競馬場名
    ・Surface
    ・学習不能データ除外
    ・Target
    ・日付型
    ・時系列ソート
    """

    df = df.copy()

    # =====================================
    # 競馬場名
    # =====================================

    df["TrackName"] = (
        df["TrackCode"]
        .map(TRACK_NAMES)
    )

    # =====================================
    # 芝 / ダート / 障害
    # =====================================

    df["Surface"] = (
        df["TrackType"]
        .apply(convert_surface)
    )

    # =====================================
    # 学習不能データを除外
    # =====================================

    df = df[
        (df["ResultOrder"] > 0)
        & (df["Odds"] > 0)
        & (df["Popularity"] > 0)
    ].copy()

    # =====================================
    # 正解ラベル
    # 1～3着 = 1
    # 4着以下 = 0
    # =====================================

    df["Target"] = (
        (df["ResultOrder"] >= 1)
        & (df["ResultOrder"] <= 3)
    ).astype(int)

    # =====================================
    # 日付型
    # =====================================

    df["RaceDate"] = pd.to_datetime(
        df["RaceDate"]
    )

    # =====================================
    # 馬ごとの時系列順
    # =====================================

    df = df.sort_values(
        [
            "HorseID",
            "RaceDate",
            "RaceID",
        ]
    ).copy()

    return df