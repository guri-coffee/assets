# -*- coding: utf-8 -*-
"""研磨加工指示図 (A4横・ベクターPDF) 生成スクリプト"""
from reportlab.lib.pagesizes import A4, landscape
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
import math

pdfmetrics.registerFont(TTFont('JP', '/usr/share/fonts/opentype/ipafont-gothic/ipag.ttf'))

PAGE_W, PAGE_H = landscape(A4)  # 841.89 x 595.28
OUT = 'kenma_shiji_zumen.pdf'

c = canvas.Canvas(OUT, pagesize=landscape(A4))
c.setTitle('研磨加工指示図')

THIN, MED, THICK = 0.4, 0.7, 1.2

def text(x, y, s, size=9, anchor='l', angle=0):
    c.saveState()
    c.setFont('JP', size)
    c.translate(x, y)
    if angle:
        c.rotate(angle)
    if anchor == 'c':
        c.drawCentredString(0, 0, s)
    elif anchor == 'r':
        c.drawRightString(0, 0, s)
    else:
        c.drawString(0, 0, s)
    c.restoreState()

def arrowhead(x, y, ang_deg, size=7):
    """filled arrowhead pointing in direction ang_deg (deg, CCW from +x)"""
    a = math.radians(ang_deg)
    w = math.radians(11)
    p = c.beginPath()
    p.moveTo(x, y)
    p.lineTo(x - size*math.cos(a - w), y - size*math.sin(a - w))
    p.lineTo(x - size*math.cos(a + w), y - size*math.sin(a + w))
    p.close()
    c.drawPath(p, fill=1, stroke=0)

def dim_h(x1, x2, y, label, caption=None, ext_from=None, txt_dy=3):
    """horizontal dimension line"""
    c.setLineWidth(THIN)
    if ext_from is not None:
        gap, over = 2, 4
        for xx in (x1, x2):
            if ext_from > y:
                c.line(xx, ext_from - gap, xx, y - over)
            else:
                c.line(xx, ext_from + gap, xx, y + over)
    c.line(x1, y, x2, y)
    arrowhead(x1, y, 180); arrowhead(x2, y, 0)
    text((x1+x2)/2, y + txt_dy, label, 10, 'c')
    if caption:
        text((x1+x2)/2, y + txt_dy + 12, caption, 7, 'c')

def dim_v(y1, y2, x, label, caption=None, ext_from=None, side='r'):
    """vertical dimension line, text to the right"""
    c.setLineWidth(THIN)
    if ext_from is not None:
        gap, over = 2, 4
        for yy in (y1, y2):
            if ext_from < x:
                c.line(ext_from + gap, yy, x + over, yy)
            else:
                c.line(ext_from - gap, yy, x - over, yy)
    c.line(x, y1, x, y2)
    arrowhead(x, y1, 270); arrowhead(x, y2, 90)
    ym = (y1+y2)/2
    if side == 'r':
        text(x + 5, ym + 4, label, 10, 'l')
        if caption:
            text(x + 5, ym - 8, caption, 7, 'l')
    else:
        text(x - 5, ym, label, 10, 'r', angle=90)

# ---------------- outer frame ----------------
c.setLineWidth(THICK)
c.rect(15, 15, PAGE_W-30, PAGE_H-30)

# ---------------- geometry ----------------
s = 0.94                    # draw scale pt/mm (representation only; NTS)
L, D, H = 402.3, 302.6, 63.3
ox = 70
# front view (正面図)
fx0, fx1 = ox, ox + L*s
fy0, fy1 = 80.0, 80.0 + H*s
# plan view (平面図, 第三角法: 正面図の上)
px0, px1 = fx0, fx1
py0, py1 = 225.0, 225.0 + D*s

c.setLineWidth(THICK)
c.rect(fx0, fy0, fx1-fx0, fy1-fy0)
c.rect(px0, py0, px1-px0, py1-py0)

# projection alignment lines (light, optional) -- omitted for clarity

# ---------------- dimensions ----------------
c.setLineWidth(THIN)
# (402.3) above plan view
dim_h(px0, px1, py1 + 24, '(402.3)', '参考：現在の長さ', ext_from=py1)
# (302.6) left of plan view
dim_v(py0, py1, px0 - 25, '(302.6)', None, ext_from=px0, side='l')
text(px0 - 42, (py0+py1)/2, '参考：現在の長さ', 7, 'c', angle=90)
# (63.3) right of front view
dim_v(fy0, fy1, fx1 + 26, '(63.3)', None, ext_from=fx1)
text(fx1 + 31, (fy0+fy1)/2 - 9, '参考：', 7, 'l')
text(fx1 + 31, (fy0+fy1)/2 - 19, '現在の高さ', 7, 'l')

# ---------------- datum A (top face) ----------------
dx = fx0 + 55
# filled triangle sitting on top face
tri = c.beginPath()
tri.moveTo(dx, fy1)
tri.lineTo(dx - 5, fy1 + 9)
tri.lineTo(dx + 5, fy1 + 9)
tri.close()
c.setLineWidth(MED)
c.drawPath(tri, fill=1, stroke=1)
c.line(dx, fy1 + 9, dx, fy1 + 26)
c.rect(dx - 8, fy1 + 26, 16, 16)
text(dx, fy1 + 30.5, 'A', 10, 'c')

# ---------------- 研代 0.2 leaders ----------------
# top face
tx = fx0 + 250
c.setLineWidth(THIN)
c.line(tx, fy1 + 42, tx, fy1 + 3)
arrowhead(tx, fy1, 270)
text(tx, fy1 + 46, '研代 0.2', 9, 'c')
# bottom face
bx = fx0 + 250
c.line(bx, fy0 - 30, bx, fy0 - 3)
arrowhead(bx, fy0, 90)
text(bx, fy0 - 41, '研代 0.2', 9, 'c')

# ---------------- feature control frame 0.005 // A (bottom face) ----------------
fcf_w1, fcf_w2, fcf_w3, fcf_h = 20, 40, 18, 16
fcx, fcy = fx0 + 40, fy0 - 46      # box lower-left
c.setLineWidth(MED)
c.rect(fcx, fcy, fcf_w1, fcf_h)
c.rect(fcx + fcf_w1, fcy, fcf_w2, fcf_h)
c.rect(fcx + fcf_w1 + fcf_w2, fcy, fcf_w3, fcf_h)
# parallelism symbol: two slanted parallel strokes
c.setLineWidth(1.0)
sx = fcx + fcf_w1/2
c.line(sx - 5, fcy + 3, sx - 1, fcy + 13)
c.line(sx + 1, fcy + 3, sx + 5, fcy + 13)
text(fcx + fcf_w1 + fcf_w2/2, fcy + 4.5, '0.005', 9, 'c')
text(fcx + fcf_w1 + fcf_w2 + fcf_w3/2, fcy + 4.5, 'A', 9, 'c')
# leader from bottom face to FCF
lx = fcx + fcf_w1 + fcf_w2/2 - 25
c.setLineWidth(THIN)
c.line(lx, fcy + fcf_h + 2, lx, fy0 - 3)
arrowhead(lx, fy0, 90)

# view labels
text(fx0, fy0 - 62, '正面図', 8)
text(px0, py1 + 48, '平面図', 8)

# ---------------- notes ----------------
nx, ny = 505, PAGE_H - 60
text(nx, ny, '注 記', 11)
c.setLineWidth(MED)
c.line(nx, ny - 4, nx + 34, ny - 4)
notes = [
    '1. 本図は現状品の再研磨加工指示図である。',
    '2. （　）内寸法は現状品の実測値であり参考値とする。',
    '3. 上面・下面を研磨のこと。研代は各面 0.2 とする。',
    '　　（研磨後の高さ 約62.9 ： 参考）',
    '4. 上面をデータムAとし、研磨後の下面は',
    '　　データムAに対し 平行度 0.005 以内とする。',
    '5. 上面の凹み形状・内部形状は現状のまま',
    '　　（本図では省略して図示）。',
    '6. 指示なき事項は現状のままとする。',
]
yy = ny - 24
for n in notes:
    text(nx, yy, n, 9)
    yy -= 16

# ---------------- title block ----------------
tb_w, tb_h = 300, 110
tb_x, tb_y = PAGE_W - 15 - tb_w, 15
c.setLineWidth(THICK)
c.rect(tb_x, tb_y, tb_w, tb_h)
rows = 5
rh = tb_h / rows
c.setLineWidth(THIN)
for i in range(1, rows):
    c.line(tb_x, tb_y + rh*i, tb_x + tb_w, tb_y + rh*i)
lab_w = 60
mid = tb_x + tb_w/2
def tb_row(r, label1, val1, label2=None, val2=None):
    y = tb_y + rh*(rows-1-r) + rh/2 - 3.5
    text(tb_x + 5, y, label1, 8)
    text(tb_x + lab_w, y, val1, 9)
    if label2 is not None:
        text(mid + 5, y, label2, 8)
        text(mid + lab_w, y, val2, 9)
c.line(tb_x + lab_w - 5, tb_y, tb_x + lab_w - 5, tb_y + tb_h)
tb_row(0, '図　名', '研磨加工指示図')
tb_row(1, '品　名', 'プレート（現状品／材質指定なし）')
c.line(mid, tb_y, mid, tb_y + rh*3)
c.line(mid + lab_w - 5, tb_y, mid + lab_w - 5, tb_y + rh*3)
tb_row(2, '尺　度', '非比例（NTS）', '単　位', 'mm')
tb_row(3, '投影法', '第三角法', '数　量', '1')
tb_row(4, '作成日', '2026-08-03', '図　番', '—')

c.save()
print('written:', OUT)
