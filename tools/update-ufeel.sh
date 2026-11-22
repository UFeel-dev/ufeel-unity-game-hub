#!/usr/bin/env bash

# * syncs a built ufeel package into Packages/com.ufcorp.ufeel

set -euo pipefail

error() { printf 'error: %s\n' "$*" >&2; exit 1; }

(($# == 1)) || error "usage: $0 <ufeel package directory>"

source_dir="$(realpath "$1")"
package_dir="$(realpath "$(dirname "$0")/../Packages/com.ufcorp.ufeel")"
plugins_dir="$package_dir/Runtime/Plugins/Linux-x86_64"
core_dir="$package_dir/Runtime/Core"
adapter_dir="$source_dir/adapters/csharp/UFeel.Core"

[[ -f "$source_dir/libufeel_wrapper.so" ]] || error "$source_dir/libufeel_wrapper.so not found"

sync() { rsync -a --delete --exclude='*.meta' "$@"; }

mkdir -p "$plugins_dir"
sync "$source_dir/libufeel_wrapper.so" "$plugins_dir/"
sync "$source_dir/dependencies/" "$plugins_dir/dependencies/"
sync "$source_dir/models/" "$plugins_dir/models/"

if [[ -d "$adapter_dir" ]]; then
    sync "$adapter_dir/" "$core_dir/"
else
    printf 'warning: no C# adapter in the package (build it with --adapter csharp), Runtime/Core left unchanged\n' >&2
fi

find "$plugins_dir" "$core_dir" -name '*.meta' -print0 2>/dev/null |
    while IFS= read -r -d '' meta; do
        asset="${meta%.meta}"
        [[ -e "$asset" || -L "$asset" ]] || rm -- "$meta"
    done

printf 'Updated %s from %s\n' "$package_dir" "$source_dir"
