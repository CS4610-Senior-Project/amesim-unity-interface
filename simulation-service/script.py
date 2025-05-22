#!/usr/bin/env python
from __future__ import annotations
r"""
Simcenter → normalise → PID builder
• Double‑click  → runs once with example\plane_config.json
• -c <cfg.json> → runs once with that config
• --watch DIR   → polls DIR for new *.json configs

Dependencies: pandas (auto‑installed if missing)
"""

# ── auto‑install pandas ──────────────────────────────────────────────────────
import os, sys, subprocess, importlib, time
def _ensure_pandas():
    try:
        import pandas as pd; return pd
    except ModuleNotFoundError:
        print("[BOOT] pandas not found—installing …")
        subprocess.check_call([sys.executable, "-m", "pip",
                               "install", "--user", "pandas"])
        print("[BOOT] installed, restarting script …\n")
        os.execv(sys.executable, [sys.executable] + sys.argv)

pd = _ensure_pandas()

# ── stdlib imports (after future import) ─────────────────────────────────────
import argparse
from pathlib import Path
from typing import Iterable, Tuple, Set

# ── paths & constants ────────────────────────────────────────────────────────
SCRIPT_DIR   = Path(__file__).resolve().parent
AME_DIR      = r"C:\Program Files\Simcenter\2310\Amesim"
SIM_PY       = Path(AME_DIR) / "python.bat"
SIM_SCRIPT   = str(SCRIPT_DIR / "src" / "__main__.py")
DEFAULT_CFG  = str(SCRIPT_DIR / "example" / "plane_config.json")

# input.csv lives in example\data
INPUT_PATH   = SCRIPT_DIR / "example" / "data" / "input.csv"

# roll / pitch CSVs come from AMESIM\Outputs
CSV_DIR      = Path(r"C:\Users\PhotonUser\My Files\OneDrive\Files\AMESIM\Outputs") # Make sure this is correct
OUT_DIR      = SCRIPT_DIR

EXCLUDE_COLS: Iterable[str] = ("Time - s", "Time", "time")
TIME_ALIAS, PITCH_ALIAS, ROLL_ALIAS = "Time", "Target Pitch", "Target Roll"
FLOAT_FMT    = "%.6f"
POLL_SECS    = 5

# ── simulation launcher ──────────────────────────────────────────────────────
def run_sim(cfg_json: str) -> None:
    env = {**os.environ, "AME": AME_DIR}
    cmd = [str(SIM_PY), SIM_SCRIPT, "-c", cfg_json]
    print("[SIM] →", " ".join(cmd))
    proc = subprocess.run(cmd, env=env, capture_output=True, text=True)
    if proc.stdout: print(proc.stdout)
    if proc.stderr: print(proc.stderr, file=sys.stderr)
    if proc.returncode: raise RuntimeError("Simulation failed")

# ── scaling helpers ──────────────────────────────────────────────────────────
def minmax(col: pd.Series) -> pd.Series:
    lo, hi = col.min(), col.max()
    if lo == hi: return pd.Series(0.0, index=col.index)
    return 2 * (col - lo) / (hi - lo) - 1

def symmetric(col: pd.Series) -> pd.Series:
    lo, hi = col.min(), col.max()
    if lo == 0 and hi == 0: return pd.Series(0.0, index=col.index)
    def f(x_val):
        if x_val == 0:
            return 0.0
        if x_val > 0:
            return x_val / hi if hi != 0 else 0.0
        return x_val / abs(lo) if lo != 0 else 0.0
    return col.apply(f)

# ── CSV normaliser ───────────────────────────────────────────────────────────
def normalise(path: Path, symmetric_mode: bool, label: str) -> Tuple[pd.DataFrame, str]:
    try:
        df = pd.read_csv(path)
    except FileNotFoundError:
        print(f"[ERR-NORM] File not found during normalise: {path}", file=sys.stderr)
        raise
    except Exception as e:
        print(f"[ERR-NORM] Could not read CSV {path}: {e}", file=sys.stderr)
        raise

    angle_col_candidates = [
        c for c in df.columns
        if c not in EXCLUDE_COLS and pd.api.types.is_numeric_dtype(df[c])
    ]
    if not angle_col_candidates:
        raise ValueError(
            f"No suitable numeric data column found in '{path.name}' for '{label}'. "
            f"Excluded: {EXCLUDE_COLS}. Columns found: {list(df.columns)}"
        )
    angle_col_name = angle_col_candidates[0]

    is_roll_processing = "roll" in label.lower()
    original_data_is_all_zeros = (df[angle_col_name].fillna(0) == 0).all()

    if is_roll_processing and original_data_is_all_zeros:
        df[angle_col_name] = 0.0
        print(f"[NORM] {label} ({path.name}): Original data is all zeros. Normalized to all zeros.")
    else:
        if symmetric_mode:
            df[angle_col_name] = symmetric(df[angle_col_name])
        else:
            df[angle_col_name] = minmax(df[angle_col_name])

    out_file = OUT_DIR / f"{label.replace(' ', '_').lower()}_norm.csv"
    df.to_csv(out_file, index=False, float_format=FLOAT_FMT)
    print(f"[NORM] {label} → {out_file.name}")
    return df, angle_col_name

# ── choose roll CSV (based on input.csv content) ─────────────────────────────
def roll_csv() -> Tuple[Path, str]:
    # (Keeping your existing roll_csv logic as it was in the last version you confirmed)
    try:
        raw = pd.read_csv(INPUT_PATH)
    except FileNotFoundError:
        print(f"[WARN] {INPUT_PATH} not found. Defaulting to 'roll angle.csv'.", file=sys.stderr)
        return CSV_DIR / "roll angle.csv", "Roll angle CSV" # Fallback
            
    angle_col_name_found = None
    angle_col_series_processed = None

    for col_header in raw.columns:
        if col_header.lower() == "angle":
            potential_angle_series = pd.to_numeric(raw[col_header], errors='coerce')
            if not potential_angle_series.isnull().all():
                angle_col_name_found = col_header
                angle_col_series_processed = potential_angle_series.fillna(0)
                break 
            
    if angle_col_series_processed is not None:
        if (angle_col_series_processed == 0).all():
            fname = "no roll.csv"
            print(f"[ROLL_CHOICE] Selected 'no roll.csv' based on 'Angle' column in {INPUT_PATH.name}")
            return CSV_DIR / fname, "Roll angle CSV"

    numeric_df = raw.select_dtypes(include='number')

    if numeric_df.empty or (numeric_df.fillna(0) == 0).all().all():
        fname = "no roll.csv"
        print(f"[ROLL_CHOICE] Selected 'no roll.csv' based on all numeric data in {INPUT_PATH.name}")
        return CSV_DIR / fname, "Roll angle CSV"

    has_positive = (numeric_df > 0).any().any()
    has_negative = (numeric_df < 0).any().any()

    if has_positive and has_negative:
        fname = "mixed.csv"
        print(f"[ROLL_CHOICE] Selected 'mixed.csv' from {INPUT_PATH.name}")
    elif has_negative: 
        fname = "negative roll angle.csv"
        print(f"[ROLL_CHOICE] Selected 'negative roll angle.csv' from {INPUT_PATH.name}")
    else: 
        fname = "roll angle.csv"
        print(f"[ROLL_CHOICE] Selected 'roll angle.csv' (default/positive) from {INPUT_PATH.name}")
        
    return CSV_DIR / fname, "Roll angle CSV"

# ── build pid_targets.csv ────────────────────────────────────────────────────
def build_pid():
    roll_path, roll_label = roll_csv()

    # Explicitly check if the determined roll CSV file exists
    if not roll_path.exists():
        # This error means roll_csv() gave a path, but the file isn't there.
        raise FileNotFoundError(
            f"The chosen roll CSV file ('{roll_path.name}') was not found in '{CSV_DIR}'. "
            f"Please ensure it exists. (Determined based on {INPUT_PATH.name})"
        )
    
    pitch_csv_path = CSV_DIR / "pitch angle.csv"
    if not pitch_csv_path.exists():
        raise FileNotFoundError(f"Required pitch data file 'pitch angle.csv' not found in '{CSV_DIR}'.")

    roll_df, roll_col   = normalise(roll_path, symmetric_mode=True,  label=roll_label)
    pitch_df, pitch_col = normalise(pitch_csv_path, symmetric_mode=False, label="Pitch angle CSV")

    # Robustly find time column in roll_df
    tcol_roll_candidates = [c for c in roll_df.columns if c.lower().startswith("time")]
    if not tcol_roll_candidates:
        raise ValueError(
            f"No time column (e.g., 'Time', 'Time - s') found in normalized roll data (from {roll_path.name}). "
            f"Columns present: {list(roll_df.columns)}"
        )
    tcol = tcol_roll_candidates[0] # Use the first found time column

    # Ensure the same time column name exists in pitch_df or can be found and renamed
    if tcol not in pitch_df.columns:
        tcol_pitch_candidates = [c for c in pitch_df.columns if c.lower().startswith("time")]
        if not tcol_pitch_candidates:
            raise ValueError(
                f"No time column found in normalized pitch data (from {pitch_csv_path.name}). "
                f"Columns present: {list(pitch_df.columns)}"
            )
        
        original_pitch_tcol = tcol_pitch_candidates[0]
        if original_pitch_tcol != tcol: # Avoid renaming if names already match
            print(f"[BUILD] Aligning time columns: Renaming '{original_pitch_tcol}' to '{tcol}' in pitch data.")
            pitch_df.rename(columns={original_pitch_tcol: tcol}, inplace=True)
        
        if tcol not in pitch_df.columns: # Double check rename or if it was supposed to be there
             raise ValueError(
                 f"Failed to align time column '{tcol}'. Pitch data (from {pitch_csv_path.name}) "
                 f"does not have a time column named '{tcol}' after attempting alignment. "
                 f"Original pitch time candidate: '{original_pitch_tcol}'. Roll time column: '{tcol}'."
            )

    merged = pd.merge(
        pitch_df[[tcol, pitch_col]],
        roll_df[[tcol, roll_col]],
        on=tcol, how="inner")
    
    if merged.empty:
        print(f"[WARN] The merge of pitch and roll data on time column '{tcol}' resulted in an empty dataset. "
              f"pid_targets.csv will be empty or not updated with new data. "
              f"Check time values in '{roll_path.name}' and '{pitch_csv_path.name}'.")
        # Decide if you want to write an empty file or skip writing
        # For now, it will write an empty file if merged is empty.

    merged.rename(columns={
        tcol: TIME_ALIAS, pitch_col: PITCH_ALIAS, roll_col: ROLL_ALIAS},
        inplace=True)
    merged.to_csv(OUT_DIR / "pid_targets.csv",
                  index=False, float_format=FLOAT_FMT)
    print("[BUILD] pid_targets.csv written")

# ── pipeline ────────────────────────────────────────────────────────────────
def pipeline(cfg: str):
    try:
        run_sim(cfg)
        build_pid()
        print("[DONE]", Path(cfg).name)
    except FileNotFoundError as e:
        print(f"[ERR-PIPELINE] File not found: {e}", file=sys.stderr)
    except ValueError as e:
        print(f"[ERR-PIPELINE] Value error (likely bad CSV structure or missing columns): {e}", file=sys.stderr)
    except RuntimeError as e: # from run_sim
        print(f"[ERR-PIPELINE] Runtime error (simulation failed?): {e}", file=sys.stderr)
    except Exception as e: # Catch-all for other unexpected errors in pipeline
        print(f"[ERR-PIPELINE] An unexpected error occurred processing {Path(cfg).name}: {e}", file=sys.stderr)


# ── watch mode ──────────────────────────────────────────────────────────────
def watch(folder: Path):
    seen: Set[Path] = set()
    print("[WATCH] scanning", folder)
    try:
        while True:
            for fp in folder.glob("*.json"):
                if fp not in seen:
                    # Run pipeline and catch exceptions here so watch mode continues
                    try:
                        print(f"[WATCH] Processing new config: {fp.name}")
                        pipeline(str(fp))
                        seen.add(fp)
                    except Exception as e: # This catches errors from pipeline not already handled inside it
                        print(f"[ERR-WATCH] Failed to process {fp.name}: {e}", file=sys.stderr)
            time.sleep(POLL_SECS)
    except KeyboardInterrupt:
        print("\n[WATCH] stopped by user.")

# ── CLI entry ───────────────────────────────────────────────────────────────
def main():
    ap = argparse.ArgumentParser(add_help=False) # Basic parser
    # For a better CLI experience, consider adding descriptions and help messages
    # ap = argparse.ArgumentParser(description="Simcenter → normalise → PID builder")
    grp = ap.add_mutually_exclusive_group()
    grp.add_argument("-c", "--config", help=f"Path to config JSON (default: {DEFAULT_CFG})")
    grp.add_argument("--watch", metavar="DIR", help="Directory to watch for new *.json configs")
    # Add a proper help argument if you expand the ArgumentParser
    # ap.add_argument("-h", "--help", action="help", help="Show this help message and exit.")
    args, _ = ap.parse_known_args() # Use parse_args() if you define all args

    OUT_DIR.mkdir(exist_ok=True)
    if args.watch:
        watch_path = Path(args.watch)
        if not watch_path.is_dir():
            print(f"[ERR] Watch directory '{watch_path}' not found or not a directory.", file=sys.stderr)
            sys.exit(1)
        watch(watch_path)
    else:
        config_to_run = args.config or DEFAULT_CFG
        if not Path(config_to_run).exists():
            print(f"[ERR] Config file '{config_to_run}' not found.", file=sys.stderr)
            sys.exit(1)
        pipeline(config_to_run) # Errors from pipeline are handled inside it or by main's try-finally

    # pause if launched by double‑click (no tty)
    # Only pause if no specific config was given (implying default run) and not in watch mode
    if not sys.stdin.isatty() and not args.watch and not args.config:
        input("Press Enter to close…")

if __name__ == "__main__":
    main()