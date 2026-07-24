#!/usr/bin/env bash
# Gera o zip de release do JellyDirectGuard e atualiza manifest.json.
# Uso: ./scripts/package.sh 1.0.0.0 ["texto do changelog"] [URL_BASE_DOWNLOAD]
set -euo pipefail

VERSION="${1:?informe a versão, ex.: 1.0.0.0}"
CHANGELOG="${2:-Atualização de manutenção}"
BASE_URL="${3:-https://github.com/elvisfalmeida/JellyDirectGuard/releases/download/v${VERSION}}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJ="$ROOT/Jellyfin.Plugin.JellyDirectGuard"
OUT="$ROOT/dist"

dotnet build -c Release "$PROJ" -p:AssemblyVersion="$VERSION" -p:FileVersion="$VERSION"

mkdir -p "$OUT"
ZIP="$OUT/jellydirectguard_${VERSION}.zip"
rm -f "$ZIP"
(cd "$PROJ/bin/Release/net9.0" && zip -j "$ZIP" Jellyfin.Plugin.JellyDirectGuard.dll)

MD5=$(md5sum "$ZIP" | cut -d' ' -f1)
TS=$(date -u +%Y-%m-%dT%H:%M:%SZ)

python3 - "$ROOT/manifest.json" "$VERSION" "$MD5" "$TS" "$BASE_URL" "$CHANGELOG" <<'PY'
import json, sys
path, version, md5, ts, base_url, changelog = sys.argv[1:7]
tag_url = f"https://github.com/elvisfalmeida/JellyDirectGuard/releases/tag/v{version}"
with open(path) as f:
    manifest = json.load(f)
entry = {
    "version": version,
    "changelog": f"{changelog}\n\nChangelog completo: {tag_url}",
    "targetAbi": "10.11.0.0",
    "sourceUrl": f"{base_url}/jellydirectguard_{version}.zip",
    "checksum": md5,
    "timestamp": ts,
}
versions = [v for v in manifest[0]["versions"] if v["version"] != version]
manifest[0]["versions"] = [entry] + versions
with open(path, "w") as f:
    json.dump(manifest, f, indent=2)
print(f"manifest.json atualizado: {version} md5={md5}")
PY

echo "Release pronta: $ZIP"
