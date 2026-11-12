import pandas as pd

from features import FEATURES
from modeling import create_model


# =====================================
# 人気帯
# =====================================

POPULARITY_GROUPS = [
    ("5番人気", 5, 5),
    ("6～9番人気", 6, 9),
    ("10番人気以下", 10, 99),
]


def print_historical_feature_check(
    df: pd.DataFrame
) -> None:
    """過去成績系の特徴量が期待どおり計算されているか確認表示する。"""

    print()
    print("===== 過去成績特徴量 確認 =====")

    columns = [
        "RaceDate",
        "HorseName",
        "ResultOrder",
        "Popularity",

        "Last1Finish",
        "Last1Popularity",

        "Last3AvgFinish",
        "Last3AvgPopularity",
        "Last3Top3Rate",

        "SameDistanceTop3Rate",

        "HorseTrackTop3Rate",
        "HorseTrackStarts",

        "HorseSurfaceTop3Rate",
        "HorseSurfaceStarts",
    ]

    print(
        df[columns].tail(30)
    )


def print_track_type_check(
    df: pd.DataFrame
) -> None:
    """TrackTypeとSurfaceの変換結果を確認表示する。"""

    print()
    print("===== TrackType確認 =====")

    print(
        df["TrackType"]
        .value_counts(
            dropna=False
        )
    )


def print_data_split(
    train: pd.DataFrame,
    test: pd.DataFrame,
    total: pd.DataFrame
) -> None:
    """学習・テストデータの件数と対象期間を確認表示する。"""

    print()
    print("===== データ分割 =====")

    print(
        "全データ   :",
        total.shape
    )

    print(
        "学習データ :",
        train.shape
    )

    print(
        "テストデータ:",
        test.shape
    )

    print()

    print(
        "学習期間:",
        train["RaceDate"].min(),
        "～",
        train["RaceDate"].max(),
    )

    print(
        "テスト期間:",
        test["RaceDate"].min(),
        "～",
        test["RaceDate"].max(),
    )


def print_feature_dtypes(
    df: pd.DataFrame
) -> None:
    """モデルへ渡す特徴量のデータ型を一覧表示する。"""

    print()
    print(
        "===== 特徴量の型確認 ====="
    )

    print(
        df[FEATURES].dtypes
    )


def print_top_predictions(
    test: pd.DataFrame,
    top_n: int = 30
) -> None:
    """予測値が高い馬を上位から表示し、モデル出力を確認する。"""

    result = test[
        [
            "RaceID",
            "HorseID",
            "Odds",
            "Popularity",
            "ResultOrder",
            "Target",
            "Prediction",
        ]
    ].sort_values(
        "Prediction",
        ascending=False
    )

    print()
    print(
        "===== LightGBM予測結果 ====="
    )

    print(
        result.head(top_n)
    )


# =====================================
# 従来方式確認
# 5番人気以下
# Prediction >= 0.50
# =====================================

def evaluate_legacy_anauma(
    test: pd.DataFrame
) -> None:
    """従来の穴馬抽出条件でテストデータを評価し、3着内率などを集計する。"""

    anauma = test[
        (test["Popularity"] >= 5)
        & (test["Prediction"] >= 0.50)
    ].copy()

    anauma = anauma.sort_values(
        "Prediction",
        ascending=False
    )

    print()
    print(
        "===== 穴馬候補 詳細 ====="
    )

    for _, row in anauma.iterrows():

        print(
            f"{row['RaceDate'].strftime('%Y/%m/%d')} "
            f"{row['TrackName']}"
            f"{int(row['RaceNumber'])}R "
            f"{row['RaceName']}"
        )

        print(
            f"  {row['HorseName']} "
            f"{int(row['Popularity'])}番人気 "
            f"単勝{row['Odds']:.1f}倍"
        )

        print(
            f"  AI予測: "
            f"{row['Prediction'] * 100:.1f}% "
            f"実際: "
            f"{int(row['ResultOrder'])}着"
        )

        print()

    candidate_count = len(anauma)

    hit_count = (
        anauma["ResultOrder"] <= 3
    ).sum()

    hit_rate = (
        hit_count
        / candidate_count
        * 100
        if candidate_count > 0
        else 0
    )

    print(
        "===== 穴馬候補の成績 ====="
    )

    print(
        f"候補数       : "
        f"{candidate_count}頭"
    )

    print(
        f"3着以内      : "
        f"{hit_count}頭"
    )

    print(
        f"3着内率      : "
        f"{hit_rate:.1f}%"
    )


# =====================================
# 特徴量重要度
# =====================================

def print_feature_importance(
    model
) -> None:
    """LightGBMの特徴量重要度を高い順に表示する。"""

    importance = pd.DataFrame(
        {
            "Feature": FEATURES,
            "Importance":
                model.feature_importances_,
        }
    )

    importance = importance.sort_values(
        "Importance",
        ascending=False
    )

    print()
    print(
        "===== 特徴量重要度 ====="
    )

    print(
        importance
    )


# =====================================
# AIあり / AIなし
# 5番人気以下
# =====================================

def print_baseline_comparison(
    test: pd.DataFrame
) -> None:
    """人気薄全体とAI抽出馬を比較し、3着内率の改善度を確認する。"""

    # -----------------------------
    # AIなし
    # -----------------------------

    all_longshots = test[
        test["Popularity"] >= 5
    ].copy()

    baseline_count = len(
        all_longshots
    )

    baseline_hit_count = (
        all_longshots[
            "ResultOrder"
        ] <= 3
    ).sum()

    baseline_hit_rate = (
        baseline_hit_count
        / baseline_count
        * 100
        if baseline_count > 0
        else 0
    )

    # -----------------------------
    # AIあり
    # -----------------------------

    ai = test[
        (test["Popularity"] >= 5)
        & (test["Prediction"] >= 0.50)
    ]

    ai_count = len(ai)

    ai_hits = (
        ai["ResultOrder"] <= 3
    ).sum()

    ai_rate = (
        ai_hits
        / ai_count
        * 100
        if ai_count > 0
        else 0
    )

    print()
    print(
        "===== AIあり / AIなし 比較 ====="
    )

    print(
        f"5番人気以下 全頭      : "
        f"{baseline_count}頭"
    )

    print(
        f"そのうち3着以内       : "
        f"{baseline_hit_count}頭"
    )

    print(
        f"AIなし3着内率         : "
        f"{baseline_hit_rate:.1f}%"
    )

    print()

    print(
        f"AIが選んだ穴馬        : "
        f"{ai_count}頭"
    )

    print(
        f"そのうち3着以内       : "
        f"{ai_hits}頭"
    )

    print(
        f"AIあり3着内率         : "
        f"{ai_rate:.1f}%"
    )


# =====================================
# Prediction閾値比較
# 5番人気以下
# =====================================

def print_threshold_comparison(
    test: pd.DataFrame
) -> None:
    """Predictionの閾値を変えたときの候補数と3着内率を比較する。"""

    print()
    print(
        "===== Prediction閾値 比較 ====="
    )

    thresholds = [
        0.30,
        0.35,
        0.40,
        0.45,
        0.50,
        0.55,
        0.60,
        0.65,
        0.70,
    ]

    for threshold in thresholds:

        candidates = test[
            (test["Popularity"] >= 5)
            & (
                test["Prediction"]
                >= threshold
            )
        ]

        count = len(
            candidates
        )

        hit_count = (
            candidates[
                "ResultOrder"
            ] <= 3
        ).sum()

        hit_rate = (
            hit_count
            / count
            * 100
            if count > 0
            else 0
        )

        print(
            f"Prediction >= "
            f"{threshold:.2f} "
            f"| 候補 {count:4d}頭 "
            f"| 3着以内 "
            f"{hit_count:3d}頭 "
            f"| 3着内率 "
            f"{hit_rate:5.1f}%"
        )


# =====================================
# 人気帯別
# =====================================

def print_popularity_group_comparison(
    test: pd.DataFrame
) -> None:
    """人気帯ごとにベースラインとAI抽出結果を比較する。"""

    print()
    print(
        "===== 人気帯別 "
        "AIあり / AIなし比較 ====="
    )

    for (
        label,
        min_pop,
        max_pop
    ) in POPULARITY_GROUPS:

        # -----------------------------
        # AIなし
        # -----------------------------

        baseline = test[
            (
                test["Popularity"]
                >= min_pop
            )
            & (
                test["Popularity"]
                <= max_pop
            )
        ]

        baseline_count = len(
            baseline
        )

        baseline_hits = (
            baseline[
                "ResultOrder"
            ] <= 3
        ).sum()

        baseline_rate = (
            baseline_hits
            / baseline_count
            * 100
            if baseline_count > 0
            else 0
        )

        # -----------------------------
        # AIあり
        # -----------------------------

        ai = test[
            (
                test["Popularity"]
                >= min_pop
            )
            & (
                test["Popularity"]
                <= max_pop
            )
            & (
                test["Prediction"]
                >= 0.30
            )
        ]

        ai_count = len(
            ai
        )

        ai_hits = (
            ai["ResultOrder"] <= 3
        ).sum()

        ai_rate = (
            ai_hits
            / ai_count
            * 100
            if ai_count > 0
            else 0
        )

        lift = (
            ai_rate
            / baseline_rate
            if baseline_rate > 0
            else 0
        )

        print()
        print(
            label
        )

        print(
            f"  AIなし : "
            f"{baseline_count}頭 / "
            f"3着以内 "
            f"{baseline_hits}頭 / "
            f"{baseline_rate:.1f}%"
        )

        print(
            f"  AIあり : "
            f"{ai_count}頭 / "
            f"3着以内 "
            f"{ai_hits}頭 / "
            f"{ai_rate:.1f}%"
        )

        print(
            f"  改善倍率 : "
            f"{lift:.2f}倍"
        )


# =====================================
# 月別バックテスト
# =====================================

def run_monthly_backtest(
    df: pd.DataFrame,
    months: list[str],
    min_popularity: int = 5,
    min_prediction: float = 0.30,
) -> pd.DataFrame:
    """月単位で同じ抽出条件を適用し、期間ごとの再現性をバックテストする。"""

    print()
    print(
        "===== 月別バックテスト ====="
    )

    results = []

    for month in months:

        test_start = pd.Timestamp(
            f"{month}-01"
        )

        test_end = (
            test_start
            + pd.offsets.MonthBegin(1)
        )

        # -----------------------------
        # その月より前で学習
        # -----------------------------

        train_bt = df[
            df["RaceDate"]
            < test_start
        ].copy()

        # -----------------------------
        # 対象月
        # -----------------------------

        test_bt = df[
            (
                df["RaceDate"]
                >= test_start
            )
            & (
                df["RaceDate"]
                < test_end
            )
        ].copy()

        if (
            len(train_bt) == 0
            or len(test_bt) == 0
        ):

            print()
            print(
                f"{month} : データ不足"
            )

            continue

        # -----------------------------
        # 学習
        # -----------------------------

        model = create_model()

        model.fit(
            train_bt[FEATURES],
            train_bt["Target"]
        )

        # -----------------------------
        # 予測
        # -----------------------------

        test_bt["Prediction"] = (
            model.predict_proba(
                test_bt[FEATURES]
            )[:, 1]
        )

        # -----------------------------
        # AIなし
        # -----------------------------

        baseline = test_bt[
            test_bt["Popularity"]
            >= min_popularity
        ]

        baseline_count = len(
            baseline
        )

        baseline_hits = (
            baseline[
                "ResultOrder"
            ] <= 3
        ).sum()

        baseline_rate = (
            baseline_hits
            / baseline_count
            * 100
            if baseline_count > 0
            else 0
        )

        # -----------------------------
        # AIあり
        # -----------------------------

        ai = test_bt[
            (
                test_bt["Popularity"]
                >= min_popularity
            )
            & (
                test_bt["Prediction"]
                >= min_prediction
            )
        ]

        ai_count = len(
            ai
        )

        ai_hits = (
            ai["ResultOrder"] <= 3
        ).sum()

        ai_rate = (
            ai_hits
            / ai_count
            * 100
            if ai_count > 0
            else 0
        )

        lift = (
            ai_rate
            / baseline_rate
            if baseline_rate > 0
            else 0
        )

        results.append(
            {
                "Month":
                    month,

                "BaselineCount":
                    baseline_count,

                "BaselineHits":
                    baseline_hits,

                "BaselineRate":
                    baseline_rate,

                "AICount":
                    ai_count,

                "AIHits":
                    ai_hits,

                "AIRate":
                    ai_rate,

                "Lift":
                    lift,
            }
        )

        print()
        print(
            f"【{month}】"
        )

        print(
            f"AIなし : "
            f"{baseline_count}頭 / "
            f"3着以内 "
            f"{baseline_hits}頭 / "
            f"{baseline_rate:.1f}%"
        )

        print(
            f"AIあり : "
            f"{ai_count}頭 / "
            f"3着以内 "
            f"{ai_hits}頭 / "
            f"{ai_rate:.1f}%"
        )

        print(
            f"改善倍率 : "
            f"{lift:.2f}倍"
        )

    result_df = pd.DataFrame(
        results
    )

    print()
    print(
        "===== 月別バックテスト一覧 ====="
    )

    if result_df.empty:

        print(
            "結果なし"
        )

    else:

        print(
            result_df[
                [
                    "Month",
                    "BaselineCount",
                    "BaselineRate",
                    "AICount",
                    "AIRate",
                    "Lift",
                ]
            ].to_string(
                index=False
            )
        )

    return result_df