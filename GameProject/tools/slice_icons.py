# 1-05アイコンシート（2列×4行）を個別スプライトに切り出す
# 使い方: python tools/slice_icons.py
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "01_UI/Icons/1-05_icons_base.png"
OUT = ROOT / "01_UI/Icons"
NAMES = [
    ["icon_settings", "icon_sortie"],
    ["icon_formation", "icon_items"],
    ["icon_news", "icon_home"],
    ["icon_upgrade", "icon_currency"],
]
THRESH = 48


def bands(mask, axis, min_gap=20):
    """axis=0で縦帯(行)、axis=1で横帯(列)を検出"""
    w, h = mask.size
    if axis == 0:
        on = [any(mask.getpixel((x, y)) for x in range(0, w, 3)) for y in range(h)]
    else:
        on = [any(mask.getpixel((x, y)) for y in range(0, h, 3)) for x in range(w)]
    out, start = [], None
    for i, v in enumerate(on):
        if v and start is None:
            start = i
        elif not v and start is not None:
            if i - start > min_gap:
                out.append((start, i))
            start = None
    if start is not None:
        out.append((start, len(on)))
    return out


def main():
    sheet = Image.open(SRC).convert("RGBA")
    solid = sheet.getchannel("A").point(lambda a: 255 if a > THRESH else 0)
    rows = bands(solid, axis=0)
    assert len(rows) == 4, f"行検出 {len(rows)} != 4"
    tiles = []
    for (y0, y1), row_names in zip(rows, NAMES):
        strip = sheet.crop((0, y0, sheet.width, y1))
        strip_solid = solid.crop((0, y0, sheet.width, y1))
        cols = bands(strip_solid, axis=1)
        assert len(cols) == 2, f"列検出 {len(cols)} != 2 (row {row_names})"
        for (x0, x1), name in zip(cols, row_names):
            cell = strip.crop((x0, 0, x1, strip.height))
            sprite = cell.crop(cell.getchannel("A").point(lambda a: 255 if a > THRESH else 0).getbbox())
            sprite.save(OUT / f"1-05_{name}.png")
            tiles.append((name, sprite))
            print(f"OK: {name} ({sprite.width}x{sprite.height})")

    cw = max(t.width for _, t in tiles) + 60
    ch = max(t.height for _, t in tiles) + 60
    sheet_img = Image.new("RGBA", (cw * 4, ch * 2), (16, 26, 48, 255))
    for i, (_, t) in enumerate(tiles):
        x = (i % 4) * cw + (cw - t.width) // 2
        y = (i // 4) * ch + (ch - t.height) // 2
        sheet_img.alpha_composite(t, (x, y))
    sheet_img.convert("RGB").save(OUT / "1-05_contact_sheet.png")
    print("OK: contact sheet")


if __name__ == "__main__":
    main()
