#!/usr/bin/bash

set -euo pipefail

ANIM="${SRC_DIR}/animated.gif"
LATEST="${SRC_DIR}/latest.gif"

[ -f "$ANIM" ] || { echo "Missing source: $ANIM" >&2; exit 1; }
[ -f "$LATEST" ] || { echo "Missing source: $LATEST" >&2; exit 1; }

mkdir -p "$DEST_DIR"

install -m0644 "$ANIM" "$DEST_DIR/animated.gif" 
install -m0644 "$LATEST" "$DEST_DIR/latest.gif"
