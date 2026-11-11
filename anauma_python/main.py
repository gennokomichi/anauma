from config import (
    BACKTEST_MONTHS,
    DATA_START_DATE,
    PUBLIC_MIN_POPULARITY,
    PUBLIC_MIN_PREDICTION,
    STRUCTURED_JSON_PATH,
    TARGET_RACE_DATE,
    TEST_START_DATE,
    WORDPRESS_JSON_PATH,
)

from database import (
    create_db_engine,
    load_race_data,
    load_target_race_data,
)

from evaluation import (
    evaluate_legacy_anauma,
    print_baseline_comparison,
    print_data_split,
    print_feature_dtypes,
    print_feature_importance,
    print_popularity_group_comparison,
    print_threshold_comparison,
    print_top_predictions,
    run_monthly_backtest,
)

from exporter import (
    export_structured_json,
    export_wordpress_json,
    select_public_candidates,
)

from features import (
    FEATURES,
    add_historical_features,
    add_target_features,
    prepare_base_data,
    print_feature_check,
)

from modeling import (
    create_model,
    split_train_test,
    train_and_predict,
)


def main() -> None:

    # =====================================
    # 1. DB接続
    # =====================================

    engine = create_db_engine()

    # =====================================
    # 2. 当日の出走馬確認
    # =====================================

    target_df = load_target_race_data(
        engine,
        TARGET_RACE_DATE,
    )

    print()
    print("===== 当日出走データ確認 =====")
    print("対象日 :", TARGET_RACE_DATE)
    print("件数   :", len(target_df))

    if len(target_df) > 0:

        print(
            target_df[
                [
                    "RaceID",
                    "RaceNumber",
                    "HorseName",
                    "Popularity",
                    "Odds",
                ]
            ].head(100)
        )

    else:

        print(
            "対象日の出走データがDBにありません。"
        )

        print()
        print("予測処理を終了します。")

        return

    # =====================================
    # 3. 過去データ取得
    # =====================================

    df = load_race_data(
        engine
    )

    # =====================================
    # 4. 基本整形
    # =====================================

    df = prepare_base_data(
        df
    )

    # =====================================
    # 5. 過去特徴量
    # =====================================

    df = add_historical_features(
        df
    )

    # =====================================
    # 6. 当日の出走馬に過去特徴量を付与
    # =====================================

    target_features = add_target_features(
        df,
        target_df,
    )

    print()
    print("===== 当日予測用 特徴量確認 =====")
    print("件数 :", len(target_features))

    if len(target_features) > 0:

        print(
            target_features[
                [
                    "RaceID",
                    "HorseName",
                    "TrackName",
                    "RaceNumber",
                    "Distance",
                    "Surface",
                    "Last1Finish",
                    "Last3AvgFinish",
                    "SameDistanceTop3Rate",
                    "HorseTrackTop3Rate",
                    "HorseSurfaceTop3Rate",
                ]
            ].head(50)
        )

    else:

        print(
            "当日予測用の特徴量データが作成できませんでした。"
        )

        print()
        print("予測処理を終了します。")

        return

    # =====================================
    # 7. 特徴量確認
    # =====================================

    print_feature_check(
        df
    )

    # =====================================
    # 8. 学習対象期間だけに絞る
    # =====================================

    df = df[
        df["RaceDate"] >= DATA_START_DATE
    ].copy()

    # =====================================
    # 9. 学習 / テスト
    # =====================================

    train, test = split_train_test(
        df,
        TEST_START_DATE
    )

    print_data_split(
        train,
        test,
        df
    )

    print_feature_dtypes(
        df
    )

    # =====================================
    # 10. LightGBM 学習・予測
    # =====================================

    model, predicted_test = (
        train_and_predict(
            train,
            test
        )
    )

    # =====================================
    # 11. 評価
    # =====================================

    print_top_predictions(
        predicted_test
    )

    evaluate_legacy_anauma(
        predicted_test
    )

    print_feature_importance(
        model
    )

    print_baseline_comparison(
        predicted_test
    )

    print_threshold_comparison(
        predicted_test
    )

    print_popularity_group_comparison(
        predicted_test
    )

    # =====================================
    # 12. 当日の出走馬を本番予測
    # =====================================

    production_model = create_model()

    production_model.fit(
        df[FEATURES],
        df["Target"]
    )

    target_features["Prediction"] = (
        production_model.predict_proba(
            target_features[FEATURES]
        )[:, 1]
    )

    print()
    print("===== 当日のAI予測 =====")

    print(
        target_features[
            [
                "RaceID",
                "TrackName",
                "RaceNumber",
                "HorseName",
                "Popularity",
                "Odds",
                "Prediction",
            ]
        ]
        .sort_values(
            "Prediction",
            ascending=False
        )
        .head(50)
    )

    # =====================================
    # 13. 月別バックテスト
    # =====================================

    run_monthly_backtest(
        df,
        BACKTEST_MONTHS,
        min_popularity=PUBLIC_MIN_POPULARITY,
        min_prediction=PUBLIC_MIN_PREDICTION,
    )

    # =====================================
    # 14. WordPress公開用
    # =====================================

    public_candidates = (
        select_public_candidates(
            target_features,
            PUBLIC_MIN_POPULARITY,
            PUBLIC_MIN_PREDICTION,
        )
    )

    print()
    print("===== 当日の公開候補 =====")
    print("公開候補数 :", len(public_candidates))

    if len(public_candidates) > 0:

        print(
            public_candidates[
                [
                    "RaceID",
                    "TrackName",
                    "RaceNumber",
                    "HorseName",
                    "Popularity",
                    "Odds",
                    "Prediction",
                ]
            ].to_string(
                index=False
            )
        )

    else:

        print(
            "現在は人気・オッズ未取得、"
            "または公開条件を満たす馬がありません。"
        )

    # =====================================
    # 15. JSON出力
    # =====================================

    export_wordpress_json(
        public_candidates,
        WORDPRESS_JSON_PATH,
    )

    export_structured_json(
        public_candidates,
        STRUCTURED_JSON_PATH,
        PUBLIC_MIN_POPULARITY,
        PUBLIC_MIN_PREDICTION,
    )

    print()
    print("===== Python処理完了 =====")


if __name__ == "__main__":
    main()