# 世界観ボード生成（Gemini API直叩き / shinobi-rotation/tools/gen_uka_views.py と同方式）
# 使い方: python tools/gen_worldboard.py [A|B|all]
import base64
import json
import os
import ssl
import sys
import urllib.request
from pathlib import Path

# Avast等のHTTPS検査CAはPython3.13の厳格チェックに落ちるため緩める
SSL_CTX = ssl.create_default_context()
SSL_CTX.verify_flags &= ~ssl.VERIFY_X509_STRICT

ROOT = Path(__file__).resolve().parent.parent  # GameProject/
OUT = ROOT / "00_Direction"
PROJECTS = ROOT.parent.parent  # Projects/
MODEL = os.environ.get("IMG_MODEL", "nano-banana-pro-preview")

STYLE = (
    "Anime cel-shaded game concept art, clean toon shading with subtle outlines, "
    "Genshin Impact style environment art, rich color, high detail, no people, no text, no watermark. "
    "Warm orange paper-lantern light contrasting with cool indigo-blue moonlight."
)
BOARDS = {
    "A": (
        "worldboard_A_isometric.png",
        "Key visual of a stylized Japanese ninja village at night, Tsukikage village. "
        "High-angle three-quarter isometric view of a terraced hillside village with three stone-walled tiers: "
        "a small Japanese castle keep with curved dark roofs at the top tier, "
        "wooden machiya townhouses with glowing paper lanterns on the middle tier, "
        "a vermilion torii gate and a calm pond with a small waterfall at the bottom tier. "
        "Large crescent moon in an indigo night sky, drifting mist between the tiers, "
        "one blooming cherry blossom tree with falling pink petals, fireflies. " + STYLE,
    ),
    "B": (
        "worldboard_B_cinematic.png",
        "Cinematic wide establishing shot of a stylized Japanese ninja village at night, Tsukikage village. "
        "Low-angle view from beside a vermilion torii gate and a moonlit pond in the foreground, "
        "stone steps leading up through terraced stone walls lined with glowing orange paper lanterns, "
        "wooden machiya townhouses on the slope, a graceful Japanese castle keep silhouetted at the hilltop "
        "against a huge crescent moon. Indigo night sky full of stars, thin mist, "
        "a cherry blossom tree with petals drifting across the frame, fireflies over the water reflections. " + STYLE,
    ),
}


def load_key() -> str:
    if os.environ.get("GEMINI_API_KEY"):
        return os.environ["GEMINI_API_KEY"]
    for env in (PROJECTS / "weld-estimate" / ".env", PROJECTS / "weld-ai-test" / ".env"):
        if env.exists():
            for line in env.read_text(encoding="utf-8").splitlines():
                if line.startswith("GEMINI_API_KEY="):
                    return line.split("=", 1)[1].strip()
    sys.exit("GEMINI_API_KEY が見つからん")


def generate(key: str, prompt: str, aspect: bool = True) -> bytes:
    body = {
        "contents": [{"parts": [{"text": prompt}]}],
        "generationConfig": {"responseModalities": ["TEXT", "IMAGE"]},
    }
    if aspect:
        body["generationConfig"]["imageConfig"] = {"aspectRatio": "16:9"}
    url = f"https://generativelanguage.googleapis.com/v1beta/models/{MODEL}:generateContent?key={key}"
    req = urllib.request.Request(url, json.dumps(body).encode(), {"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=300, context=SSL_CTX) as r:
            res = json.load(r)
    except urllib.error.HTTPError as e:
        if aspect and e.code == 400:  # 古いAPIはimageConfig未対応
            return generate(key, prompt + " Wide 16:9 landscape composition.", aspect=False)
        sys.exit(f"HTTP {e.code}: {e.read().decode()[:500]}")
    for part in res["candidates"][0]["content"]["parts"]:
        if "inlineData" in part:
            return base64.b64decode(part["inlineData"]["data"])
    sys.exit(f"画像が返ってこんかった: {json.dumps(res)[:500]}")


def main() -> None:
    targets = sys.argv[1:] or ["all"]
    boards = list(BOARDS) if targets == ["all"] else targets
    key = load_key()
    OUT.mkdir(exist_ok=True)
    for b in boards:
        fname, prompt = BOARDS[b]
        path = OUT / fname
        path.write_bytes(generate(key, prompt))
        print(f"OK: {path}")


if __name__ == "__main__":
    main()
