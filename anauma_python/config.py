from pathlib import Path
from datetime import date
import os

# ==============================
# DB設定
# ==============================
DATABASE_URL = os.getenv(
    "ANAUMA_DATABASE_URL",
    "mysql+pymysql://USER:PASSWORD@127.0.0.1:3307/jra_keiba_db",
)

# ==============================
# 学習データ設定
# ==============================
DATA_START_DATE = "2025-09-01"
TEST_START_DATE = "2026-09-01"

# ==============================
# 予測対象日
# ==============================
TARGET_RACE_DATE = date.today().isoformat()

# ==============================
# 公開条件
# ==============================
PUBLIC_MIN_POPULARITY = 5
PUBLIC_MIN_PREDICTION = 0.30

# ==============================
# バックテスト対象月
# ==============================
BACKTEST_MONTHS = [
    "2026-05",
    "2026-06",
    "2026-07",
    "2026-08",
    "2026-09",
]

# ==============================
# 出力先
# ==============================
BASE_DIR = Path(__file__).resolve().parent

OUTPUT_DIR = BASE_DIR / "output"
OUTPUT_DIR.mkdir(exist_ok=True)

WORDPRESS_JSON_PATH = (
    OUTPUT_DIR
    / "anauma_predictions.json"
)

STRUCTURED_JSON_PATH = (
    OUTPUT_DIR
    / "anauma_predictions_v2.json"
)