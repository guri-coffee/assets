# 1-04ボタンシートを個別スプライトに切り出し、押下状態＋シアン発光を機械生成する
# 使い方: python tools/slice_buttons.py
from pathlib import Path

from PIL import Image, ImageEnhance, ImageFilter

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "01_UI/Buttons/1-04_buttons_base.png"
OUT = ROOT / "01_UI/Buttons"
NAMES = ["btn_large", "btn_small", "btn_back", "btn_close"]
GLOW = (80, 220, 255)  # シアン
PAD = 24  # 発光がはみ出す余白


def find_bands(alpha, thresh=8):
    """アルファの横投影から縦方向のスプライト帯を検出"""
    w, h = alpha.size
    rows = [any(alpha.getpixel((x, y)) > thresh for x in range(0, w, 4)) for y in range(h)]
    bands, start = [], None
    for y, on in enumerate(rows):
        if on and start is None:
            start = y
        elif not on and start is not None:
            if y - start > 20:
                bands.append((start, y))
            start = None
    if start is not None:
        bands.append((start, h))
    return bands


def add_glow(sprite):
    """スプライトの形に沿ったシアン発光を背面に敷く"""
    canvas = Image.new("RGBA", (sprite.width + PAD * 2, sprite.height + PAD * 2), (0, 0, 0, 0))
    mask = sprite.getchannel("A").point(lambda a: min(a, 160))
    glow_layer = Image.new("RGBA", sprite.size, GLOW + (0,))
    glow_layer.putalpha(mask)
    glow_canvas = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    glow_canvas.alpha_composite(glow_layer, (PAD, PAD))
    glow_canvas = glow_canvas.filter(ImageFilter.GaussianBlur(10))
    canvas.alpha_composite(glow_canvas)
    canvas.alpha_composite(sprite, (PAD, PAD))
    return canvas


def make_pressed(sprite):
    """暗く＋わずかに縮めて押し込み感を出す"""
    dark = ImageEnhance.Brightness(sprite).enhance(0.72)
    shrunk = dark.resize((int(sprite.width * 0.96), int(sprite.height * 0.96)), Image.LANCZOS)
    canvas = Image.new("RGBA", sprite.size, (0, 0, 0, 0))
    canvas.alpha_composite(shrunk, ((sprite.width - shrunk.width) // 2,
                                    (sprite.height - shrunk.height) // 2 + 3))
    return canvas


def main():
    sheet = Image.open(SRC).convert("RGBA")
    alpha = sheet.getchannel("A")
    bands = find_bands(alpha)
    assert len(bands) == len(NAMES), f"検出帯数 {len(bands)} != {len(NAMES)}"
    for (y0, y1), name in zip(bands, NAMES):
        strip = sheet.crop((0, y0, sheet.width, y1))
        # 低アルファのゴミを無視してトリム（本体の輪郭だけ拾う）
        solid = strip.getchannel("A").point(lambda a: 255 if a > 48 else 0)
        sprite = strip.crop(solid.getbbox())
        normal = add_glow(sprite)
        pressed = add_glow(make_pressed(sprite))
        normal.save(OUT / f"1-04_{name}_normal.png")
        pressed.save(OUT / f"1-04_{name}_pressed.png")
        print(f"OK: {name} ({sprite.width}x{sprite.height})")

    # 確認用コンタクトシート（藍背景）
    tiles = []
    for name in NAMES:
        for state in ("normal", "pressed"):
            tiles.append(Image.open(OUT / f"1-04_{name}_{state}.png"))
    cw = max(t.width for t in tiles) + 40
    ch = max(t.height for t in tiles) + 40
    sheet_img = Image.new("RGBA", (cw * 2, ch * 4), (16, 26, 48, 255))
    for i, t in enumerate(tiles):
        x = (i % 2) * cw + (cw - t.width) // 2
        y = (i // 2) * ch + (ch - t.height) // 2
        sheet_img.alpha_composite(t, (x, y))
    sheet_img.convert("RGB").save(OUT / "1-04_contact_sheet.png")
    print("OK: contact sheet")


if __name__ == "__main__":
    main()
