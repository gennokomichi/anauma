import pandas as pd
from lightgbm import LGBMClassifier

from features import FEATURES


def split_train_test(
    df: pd.DataFrame,
    test_start_date: str,
) -> tuple[pd.DataFrame, pd.DataFrame]:
    """指定日を境に、過去データを学習用とテスト用へ時系列で分割する。"""
    test_start = pd.Timestamp(test_start_date)

    train = df[df["RaceDate"] < test_start].copy()
    test = df[df["RaceDate"] >= test_start].copy()

    return train, test


def create_model() -> LGBMClassifier:
    """穴馬予測で使用するLightGBM分類モデルを生成する。"""
    return LGBMClassifier(
        n_estimators=200,
        random_state=42,
        verbosity=-1,
    )


def train_and_predict(
    train: pd.DataFrame,
    test: pd.DataFrame,
) -> tuple[LGBMClassifier, pd.DataFrame]:
    """学習データでLightGBMを学習し、テストデータへ3着以内確率を付与する。"""
    model = create_model()

    X_train = train[FEATURES]
    y_train = train["Target"]
    X_test = test[FEATURES]

    model.fit(X_train, y_train)

    predicted = test.copy()
    predicted["Prediction"] = model.predict_proba(X_test)[:, 1]

    return model, predicted
