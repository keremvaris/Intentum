#!/usr/bin/env bash
# Starts kev (port 8009) and laya (port 8000) in separate Terminal.app windows (macOS).
# First run prepares both engines:
#   - kev: cloned to ~/kev and synced with uv (the serve command only works from its repo)
#   - laya: installed into ~/venvs/laya (needs Python >= 3.10; system python3 may be older)
set -euo pipefail

KEV_DIR="${KEV_DIR:-$HOME/kev}"
LAYA_VENV="${LAYA_VENV:-$HOME/venvs/laya}"

open_terminal() {
  local cmd="$1" fallback="$2"
  osascript -e "tell app \"Terminal\" to do script \"$cmd\"" 2>/dev/null \
    || echo "Open a terminal and run: $fallback"
}

kev_cmd="if [ ! -d '$KEV_DIR' ]; then git clone https://github.com/jaredpalmer/kev.git '$KEV_DIR'; fi; cd '$KEV_DIR' && uv sync --extra serve && uv run --extra serve python -m kev.serve --run jaredpalmer/kev-4b --port 8009"
kev_fallback="git clone https://github.com/jaredpalmer/kev.git '$KEV_DIR' && cd '$KEV_DIR' && uv sync --extra serve && uv run --extra serve python -m kev.serve --run jaredpalmer/kev-4b --port 8009"

laya_cmd="if [ ! -x '$LAYA_VENV/bin/laya-serve' ]; then uv venv '$LAYA_VENV' --python 3.12 && uv pip install --python '$LAYA_VENV/bin/python' 'laya[serve]'; fi; '$LAYA_VENV/bin/laya-serve'"
laya_fallback="uv venv '$LAYA_VENV' --python 3.12 && uv pip install --python '$LAYA_VENV/bin/python' 'laya[serve]' && '$LAYA_VENV/bin/laya-serve'"

open_terminal "$kev_cmd" "$kev_fallback"
open_terminal "$laya_cmd" "$laya_fallback"
