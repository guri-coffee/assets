"""
TSUKIKAGE×Shinobi ゲームアセット用画像生成ツール（GameProject公式）

CryptoNinjaアートディレクションガイドv0.1準拠のSTYLE_BASEを焼き込み済み
（project-shinobi-unity/marketing/image-gen/generate.py と同一の世界観定義）。

使い方:
  python tools/gen_image.py "シーン説明" --out 00_Direction/worldboard_A.png
  python tools/gen_image.py "朱色の鳥居" --out 02_HomeTown/Buildings/2-05_torii_front.png --ref3d
  --size landscape(既定)/square/portrait  --ref3d: 3D化用（白背景・セル塗り・均一光）

APIキー: 環境変数 OPENAI_API_KEY（setx済みのUser環境変数もレジストリから拾う）
要件: openai, Pillow, truststore
"""

import argparse
import base64
import os
import sys
from io import BytesIO
from pathlib import Path

import truststore

truststore.inject_into_ssl()  # AvastのSSL検査対策

from openai import OpenAI
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent  # GameProject/
MODEL = "gpt-image-1"
QUALITY = "high"

SIZES = {
    "landscape": "1536x1024",
    "square": "1024x1024",
    "portrait": "1024x1536",
}

# CryptoNinjaアートディレクションガイドv0.1（2026-07-23）
STYLE_BASE = (
    "Japanese fantasy ninja world concept art, anime toon-shaded illustration "
    "(not photorealistic, not overly deformed), cinematic key visual quality, "
    "Edo-period castle town where near-future technology quietly blends in: "
    "70% traditional Japanese elements (wooden architecture, roof tiles, stone "
    "walls, lattice windows, paper lanterns, castles, bamboo fences) and 30% "
    "near-future elements (glowing clan crests, hologram bulletin boards, "
    "floating talismans, energy-powered karakuri, faintly glowing blue torii "
    "gates), fixed night-time setting with a large full moon, "
    "color palette: indigo blue, ink black, moonlight white and navy as main "
    "colors with gold, deep crimson, emerald green and purple accents, "
    "effect colors of cyan and pale blue-white glow, "
    "warm orange lantern light contrasted against cool cyan hologram glow, "
    "serene and slightly mystical atmosphere, quiet beauty, "
    "characters stand out as protagonists while the detailed background stays "
    "subdued, fantastical brightness that is dark but easy to see"
)

# 3D化リファレンス用（shinobi-rotation/prompts/uka_character_sheet.md 準拠）
REF3D_BASE = (
    "Flat colors, clean thick lineart, cel shading, anime style, "
    "plain solid white background, even lighting, no shadows on the background, "
    "the entire object fully visible in frame with margin, "
    "reference sheet style for 3D modeling"
)


def get_api_key() -> str | None:
    key = os.environ.get("OPENAI_API_KEY")
    if key:
        return key
    if sys.platform == "win32":
        import winreg
        try:
            with winreg.OpenKey(winreg.HKEY_CURRENT_USER, "Environment") as k:
                return winreg.QueryValueEx(k, "OPENAI_API_KEY")[0]
        except OSError:
            return None
    return None


def main() -> int:
    parser = argparse.ArgumentParser(description="ゲームアセット画像生成")
    parser.add_argument("scene", help="シーン説明（日本語OK）")
    parser.add_argument("--out", required=True, help="出力パス（GameProject相対 or 絶対）")
    parser.add_argument("--size", choices=SIZES, default="landscape")
    parser.add_argument("--ref3d", action="store_true", help="3D化用リファレンス（白背景・セル塗り）")
    parser.add_argument("--transparent", action="store_true", help="透過背景（ロゴ・UIパーツ用）")
    parser.add_argument("--raw", action="store_true", help="STYLE_BASEを使わずシーン説明をそのまま使う（ロゴ等）")
    args = parser.parse_args()

    api_key = get_api_key()
    if not api_key:
        print("ERROR: OPENAI_API_KEY が見つかりません。")
        return 1

    if args.raw:
        prompt = args.scene
    elif args.ref3d:
        # 3D化用でも世界観の色・意匠は引き継ぐ（形状把握を邪魔せん範囲で）
        prompt = (f"{REF3D_BASE}. Subject: {args.scene}. Design language: Edo-period Japanese "
                  f"with subtle near-future accents (faint cyan glow allowed). "
                  f"No text, no logos, no watermarks.")
    else:
        prompt = f"{STYLE_BASE}. Scene: {args.scene}. No text, no logos, no watermarks."

    out_path = Path(args.out)
    if not out_path.is_absolute():
        out_path = ROOT / out_path
    out_path.parent.mkdir(parents=True, exist_ok=True)

    client = OpenAI(api_key=api_key)
    kwargs = {"model": MODEL, "prompt": prompt, "size": SIZES[args.size],
              "quality": QUALITY, "n": 1}
    if args.transparent:
        kwargs["background"] = "transparent"
    result = client.images.generate(**kwargs)
    img = Image.open(BytesIO(base64.b64decode(result.data[0].b64_json)))
    img.save(out_path, "PNG")
    print(f"OK: {out_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
