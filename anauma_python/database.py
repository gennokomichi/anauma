import pandas as pd
from sqlalchemy import create_engine
from sqlalchemy.engine import Engine

from config import DATABASE_URL


SQL = """
SELECT
    re.RaceID,
    re.HorseID,
    h.HorseName,
    h.SireID,
    h.SireName,
    re.GateNumber,
    re.Odds,
    re.Popularity,
    re.FrameNumber,
    re.WeightCarried,
    re.JockeyCode,
    re.JockeyName,
    re.HorseWeight,
    re.HorseWeightDiff,
    r.RaceDate,
    r.TrackCode,
    r.RaceName,
    r.Distance,
    r.TrackType,
    r.Weather,
    r.TrackCondition,
    r.RaceNumber,
    rr.ResultOrder
FROM raceentries re
JOIN races r
    ON re.RaceID = r.RaceID
JOIN horses h
    ON re.HorseID = h.HorseID
JOIN raceresults rr
    ON re.RaceID = rr.RaceID
    AND re.HorseID = rr.HorseID;
"""


def create_db_engine() -> Engine:
    return create_engine(DATABASE_URL)


def load_race_data(engine: Engine) -> pd.DataFrame:
    """MySQLからレース・馬・結果データを取得する。"""
    return pd.read_sql(SQL, engine)

import pandas as pd


def load_target_race_data(
    engine,
    target_date: str
) -> pd.DataFrame:

    sql = """
    SELECT
        re.RaceID,
        re.HorseID,

        h.HorseName,

        re.GateNumber,
        re.Odds,
        re.Popularity,
        re.FrameNumber,
        re.WeightCarried,
        re.JockeyCode,
        re.JockeyName,
        re.HorseWeight,
        re.HorseWeightDiff,

        r.RaceDate,
        r.TrackCode,
        r.RaceName,
        r.Distance,
        r.TrackType,
        r.Weather,
        r.TrackCondition,
        r.RaceNumber,
        r.StartTime

    FROM raceentries re

    JOIN races r
        ON re.RaceID = r.RaceID

    JOIN horses h
        ON re.HorseID = h.HorseID

    WHERE r.RaceDate = %(target_date)s

    ORDER BY
        r.TrackCode,
        r.RaceNumber,
        re.GateNumber;
    """

    df = pd.read_sql(
        sql,
        engine,
        params={
            "target_date": target_date
        }
    )

    return df
