import json
from datetime import datetime
from pathlib import Path
from zoneinfo import ZoneInfo

import pandas as pd

from features import FEATURES


def select_public_candidates(
    df: pd.DataFrame,
    min_popularity: int,
    min_prediction: float,
) -> pd.DataFrame:
    """
    WordPress公開対象の穴馬候補を抽出する。

    条件:
    - 指定人気以下ではなく「指定人気以上」
      例: min_popularity=5 → 5番人気以下の人気薄
    - Prediction が指定値以上
    - 1レース最大4頭
    """

    candidates = df[
        (df["Popularity"] >= min_popularity)
        & (df["Prediction"] >= min_prediction)
    ].copy()

    # 各レース内でAI評価が高い順
    candidates = candidates.sort_values(
        ["RaceID", "Prediction"],
        ascending=[True, False],
    )

    # 各レース最大4頭
    candidates = (
        candidates
        .groupby(
            "RaceID",
            group_keys=False,
        )
        .head(4)
        .copy()
    )

    # 最終表示順
    candidates = candidates.sort_values(
        [
            "RaceDate",
            "TrackCode",
            "RaceNumber",
            "Prediction",
        ],
        ascending=[
            True,
            True,
            True,
            False,
        ],
    )

    return candidates


def safe_int(value):
    """
    NaN / None の場合は None を返す。
    それ以外は int に変換する。
    """
    if pd.isna(value):
        return None

    return int(value)


def safe_float(value):
    """
    NaN / None の場合は None を返す。
    それ以外は float に変換する。
    """
    if pd.isna(value):
        return None

    return float(value)


def safe_string(value):
    """
    NaN / None の場合は None。
    それ以外は文字列化する。
    """
    if pd.isna(value):
        return None

    return str(value)


def safe_date_string(value):
    """
    pandas Timestamp / datetime を YYYY-MM-DD に変換する。
    NaN / None の場合は None。
    """
    if pd.isna(value):
        return None

    if hasattr(value, "strftime"):
        return value.strftime("%Y-%m-%d")

    return str(value)


def nullable_float(
    value,
    digits: int = 4,
):
    """
    JSONの特徴量出力用。

    NaN → None
    数値 → 指定桁数で丸める
    """
    if pd.isna(value):
        return None

    return round(
        float(value),
        digits,
    )


def export_wordpress_json(
    candidates: pd.DataFrame,
    output_path: Path,
) -> None:
    """
    現在のWordPressショートコードと互換の
    フラットJSONを出力する。

    WordPress側を変更しない限り、
    実際の公開表示はこちらを使用する。
    """

    output_data = []

    for _, row in candidates.iterrows():
        output_data.append(
            {
                "race_date": safe_date_string(
                    row["RaceDate"]
                ),
                "track": safe_string(
                    row["TrackName"]
                ),
                "race_number": safe_int(
                    row["RaceNumber"]
                ),
                "race_name": safe_string(
                    row["RaceName"]
                ),
                "horse_name": safe_string(
                    row["HorseName"]
                ),
                "popularity": safe_int(
                    row["Popularity"]
                ),
                "odds": safe_float(
                    row["Odds"]
                ),
                "prediction": (
                    None
                    if pd.isna(row["Prediction"])
                    else round(
                        float(
                            row["Prediction"]
                        ) * 100,
                        1,
                    )
                ),
            }
        )

    output_path.parent.mkdir(
        parents=True,
        exist_ok=True,
    )

    with output_path.open(
        "w",
        encoding="utf-8",
    ) as f:
        json.dump(
            output_data,
            f,
            ensure_ascii=False,
            indent=2,
        )

    print()
    print(
        f"{output_path.name} を出力しました"
    )

    print(
        f"公開候補数 : "
        f"{len(output_data)}頭"
    )


def export_structured_json(
    candidates: pd.DataFrame,
    output_path: Path,
    min_popularity: int,
    min_prediction: float,
) -> None:
    """
    今後の自動化・表示拡張用JSON。

    構造:
    - race
    - horse
    - prediction
    - features

    NaNはJSON上では null として出力する。
    """

    predictions = []

    for _, row in candidates.iterrows():

        feature_values = {}

        for feature in FEATURES:
            if feature in row.index:
                feature_values[feature] = (
                    nullable_float(
                        row[feature]
                    )
                )
            else:
                feature_values[feature] = None

        prediction_value = safe_float(
            row["Prediction"]
        )

        prediction_percent = (
            None
            if prediction_value is None
            else round(
                prediction_value * 100,
                1,
            )
        )

        prediction_probability = (
            None
            if prediction_value is None
            else round(
                prediction_value,
                6,
            )
        )

        race_data = {
            "race_id": safe_string(
                row["RaceID"]
            ),
            "race_date": safe_date_string(
                row["RaceDate"]
            ),
            "track_code": safe_string(
                row["TrackCode"]
            ),
            "track_name": safe_string(
                row["TrackName"]
            ),
            "race_number": safe_int(
                row["RaceNumber"]
            ),
            "race_name": safe_string(
                row["RaceName"]
            ),
            "distance": safe_int(
                row["Distance"]
            ),
            "track_type": safe_string(
                row["TrackType"]
            ),
        }

        horse_data = {
            "horse_id": safe_string(
                row["HorseID"]
            ),
            "horse_name": safe_string(
                row["HorseName"]
            ),
            "gate_number": safe_int(
                row["GateNumber"]
            ),
            "frame_number": safe_int(
                row["FrameNumber"]
            ),
            "popularity": safe_int(
                row["Popularity"]
            ),
            "odds": safe_float(
                row["Odds"]
            ),
            "weight_carried": safe_float(
                row["WeightCarried"]
            ),
            "horse_weight": safe_int(
                row["HorseWeight"]
            ),
            "horse_weight_diff": safe_int(
                row["HorseWeightDiff"]
            ),
            "sire_id": safe_string(
                row.get(
                    "SireID"
                )
            ),
            "sire_name": safe_string(
                row.get(
                    "SireName"
                )
            ),
        }

        prediction_data = {
            "top3_probability":
                prediction_probability,
            "percent":
                prediction_percent,
        }

        predictions.append(
            {
                "race": race_data,
                "horse": horse_data,
                "prediction":
                    prediction_data,
                "features":
                    feature_values,
            }
        )

    payload = {
        "generated_at": datetime.now(
            ZoneInfo(
                "Asia/Tokyo"
            )
        ).isoformat(
            timespec="seconds"
        ),

        "model_version": "0.2.0",

        "target": {
            "description":
                "人気薄の3着以内候補",

            "min_popularity":
                min_popularity,

            "min_prediction":
                min_prediction,
        },

        "prediction_count":
            len(predictions),

        "predictions":
            predictions,
    }

    output_path.parent.mkdir(
        parents=True,
        exist_ok=True,
    )

    with output_path.open(
        "w",
        encoding="utf-8",
    ) as f:
        json.dump(
            payload,
            f,
            ensure_ascii=False,
            indent=2,
        )

    print()

    print(
        f"{output_path.name} を出力しました"
    )

    print(
        f"構造化JSON候補数 : "
        f"{len(predictions)}頭"
    )