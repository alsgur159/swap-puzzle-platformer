
import math
import os
import random
import struct
import wave
import zlib

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ART = os.path.join(ROOT, "Assets", "Art")
UI = os.path.join(ROOT, "Assets", "Resources", "UI")
AUDIO = os.path.join(ROOT, "Assets", "Resources", "Audio")


def hexrgba(h, a=255):
    h = h.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


def write_png(path, rows, palette):
    h, w = len(rows), len(rows[0])
    raw = bytearray()
    for r in rows:
        assert len(r) == w, (path, r)
        raw.append(0)
        for ch in r:
            raw.extend(palette.get(ch, (0, 0, 0, 0)))

    def chunk(tag, data):
        c = tag + data
        return struct.pack(">I", len(data)) + c + struct.pack(">I", zlib.crc32(c) & 0xFFFFFFFF)

    png = b"\x89PNG\r\n\x1a\n"
    png += chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(bytes(raw), 9))
    png += chunk(b"IEND", b"")
    with open(path, "wb") as f:
        f.write(png)


PLAYER = [
    "................",
    "....oooooooo....",
    "...obbbbbbbbo...",
    "..obllbbbbbbbo..",
    "..oblbbbbbbbbo..",
    "..obbbbwwbbwwo..",
    "..obbbbwkbbwko..",
    "..obbbbbbbbbbo..",
    "..obbbbbbbbbbo..",
    "..occcccccccco..",
    "..obbbbbbbbbbo..",
    "..obbbbbbbbbbo..",
    "...obbbbbbbbo...",
    "...oddo..oddo...",
    "...oddo..oddo...",
    "...oooo..oooo...",
]
PLAYER_PAL = {
    "o": hexrgba("1b2440"), "b": hexrgba("3d7bff"), "l": hexrgba("8fb8ff"),
    "w": hexrgba("ffffff"), "k": hexrgba("101018"), "c": hexrgba("5ef2ff"),
    "d": hexrgba("26324f"),
}

ENEMY = [
    "................",
    "................",
    "................",
    "......oooo......",
    "....oopppppo....",
    "...oppppppppo...",
    "..opplpppppppo..",
    "..oplppppppppo..",
    ".oppkkppppkkppo.",
    ".oppwkkppkkwppo.",
    ".opppppppppppppo",
    ".oppppmmmmppppo.",
    "oppppmwmwmmppppo",
    "oppppppppppppppo",
    "oddddddddddddddo",
    ".oooooooooooooo.",
]
ENEMY_PAL = {
    "o": hexrgba("2a1033"), "p": hexrgba("a542d6"), "l": hexrgba("dd9cf5"),
    "k": hexrgba("150818"), "w": hexrgba("ffffff"), "m": hexrgba("5a1a6e"),
    "d": hexrgba("7a2aa0"),
}

CRATE = [
    "oooooooooooooooo",
    "oyyyyyyyyyyyyyyo",
    "oyhhhhhhhhhhhhyo",
    "oyhwwhhhhhhwwhyo",
    "oyhwwwhhhhwwwhyo",
    "oyhhwwwhhwwwhhyo",
    "oyhhhwwwwwwhhhyo",
    "oyhhhhwwwwhhhhyo",
    "oyhhhhwwwwhhhhyo",
    "oyhhhwwwwwwhhhyo",
    "oyhhwwwhhwwwhhyo",
    "oyhwwwhhhhwwwhyo",
    "oyhwwhhhhhhwwhyo",
    "oyhhhhhhhhhhhhyo",
    "oyyyyyyyyyyyyyyo",
    "oooooooooooooooo",
]
CRATE_PAL = {
    "o": hexrgba("4a2511"), "y": hexrgba("e0913a"), "h": hexrgba("b8692a"),
    "w": hexrgba("f2b25c"),
}

TILE = [
    "tttttttttttttttt",
    "tsssssssmsssssss",
    "sgggggggmgggggga",
    "sgggggggmgggggga",
    "sgggggggmgggggga",
    "aaaaaaaamaaaaaaa",
    "mmmmmmmmmmmmmmmm",
    "sssmssssssssmsss",
    "gggmgggggggamggg",
    "gggmgggggggamggg",
    "gggmgggggggamggg",
    "aaamaaaaaaaamaaa",
    "mmmmmmmmmmmmmmmm",
    "sssssssmssssssss",
    "gggggggmgggggggg",
    "aaaaaaamaaaaaaaa",
]
TILE_PAL = {
    "t": hexrgba("9aa3b8"), "s": hexrgba("7e879c"), "g": hexrgba("6b7489"),
    "a": hexrgba("596176"), "m": hexrgba("3a4052"),
}

SPIKE = [
    "...w.......w....",
    "...wr......wr...",
    "..wwr.....wwr...",
    "..wsrr....wsrr..",
    ".wssrr...wssrr..",
    ".wsssrr..wsssrr.",
    "wssssrr.wssssrr.",
    "wsssssrrwsssssrr",
    "dddddddddddddddd",
    "kkkkkkkkkkkkkkkk",
]
SPIKE_PAL = {
    "w": hexrgba("ffe2e2"), "s": hexrgba("e8505b"), "r": hexrgba("a3242f"),
    "d": hexrgba("5c1a20"), "k": hexrgba("2b0d10"),
}

PAD = [
    "................",
    ".....cccccc.....",
    "...cc......cc...",
    "..c..llllll..c..",
    "..c.l......l.c..",
    ".c.l..cccc..l.c.",
    ".c.l.c....c.l.c.",
    ".c.l.c.ww.c.l.c.",
    ".c.l.c.ww.c.l.c.",
    ".c.l.c....c.l.c.",
    ".c.l..cccc..l.c.",
    "..c.l......l.c..",
    "..c..llllll..c..",
    "...cc......cc...",
    ".....cccccc.....",
    "................",
]
PAD_PAL = {
    "c": hexrgba("3ff5e3", 230), "l": hexrgba("3ff5e3", 120), "w": hexrgba("e8fffc", 255),
}

def make_door():
    rows = []
    for y in range(24):
        r = []
        for x in range(16):
            frame = x in (1, 2, 13, 14) or y in (1, 2)
            edge = x in (0, 15) or y == 0
            if y < 3 and (x < 2 + (2 - y) or x > 13 - (2 - y)):
                r.append(".")
            elif edge:
                r.append("o")
            elif frame:
                r.append("g")
            elif y == 23:
                r.append("o")
            else:
                glow = (y + x) % 7 == 0 and 4 < y < 21
                r.append("w" if glow else ("l" if x in (3, 12) or y == 3 else "i"))
        rows.append("".join(r))
    return rows


DOOR_PAL = {
    "o": hexrgba("123d1c"), "g": hexrgba("3fcf5e"), "l": hexrgba("a8ffb8"),
    "i": hexrgba("6ef08a", 200), "w": hexrgba("ffffff", 230),
}


def sprites():
    os.makedirs(ART, exist_ok=True)
    write_png(os.path.join(ART, "player.png"), PLAYER, PLAYER_PAL)
    write_png(os.path.join(ART, "enemy.png"), ENEMY, ENEMY_PAL)
    write_png(os.path.join(ART, "crate.png"), CRATE, CRATE_PAL)
    write_png(os.path.join(ART, "tile.png"), TILE, TILE_PAL)
    write_png(os.path.join(ART, "spike.png"), SPIKE, SPIKE_PAL)
    write_png(os.path.join(ART, "pad.png"), PAD, PAD_PAL)
    write_png(os.path.join(ART, "goal.png"), make_door(), DOOR_PAL)


def write_rgba(path, w, h, px):
    write_png(path, ["".join(chr(256 + x + y * w) for x in range(w)) for y in range(h)],
              {chr(256 + i): c for i, c in enumerate(px)})


def frame(w, h, rings, fill):
    rows = []
    for y in range(h):
        r = ""
        for x in range(w):
            d = min(x, y, w - 1 - x, h - 1 - y)
            r += rings[d] if d < len(rings) else fill(y)
        rows.append(r)
    return rows


PANEL = frame(16, 16, "kbd", lambda y: "f")
PANEL_PAL = {"k": hexrgba("080b16"), "b": hexrgba("5b6b9e"), "d": hexrgba("252d4a"), "f": hexrgba("161c30")}

BUTTON = frame(16, 16, "k", lambda y: "h" if y <= 2 else ("s" if y >= 12 else "w"))
BUTTON_PAL = {"k": hexrgba("080b16"), "h": hexrgba("ffffff"), "w": hexrgba("dde3f0"), "s": hexrgba("a3adc6")}

TRACK = frame(8, 8, "k", lambda y: "f")
TRACK_PAL = {"k": hexrgba("080b16"), "f": hexrgba("0f1424")}
FILL = frame(8, 8, "", lambda y: "h" if y <= 1 else "w")
FILL_PAL = {"h": hexrgba("ffffff"), "w": hexrgba("d0d8ea")}
KNOB = frame(10, 14, "k", lambda y: "h" if y <= 2 else ("s" if y >= 11 else "w"))
PILL = frame(8, 8, "", lambda y: "f")
PILL_PAL = {"f": hexrgba("0b0e1a", 190)}


def icon(fn):
    on = [[fn(x + 0.5 - 8, y + 0.5 - 8) for x in range(16)] for y in range(16)]
    rows = []
    for y in range(16):
        r = ""
        for x in range(16):
            if on[y][x]:
                r += "w"
            elif any(0 <= y + j < 16 and 0 <= x + i < 16 and on[y + j][x + i]
                     for j in (-1, 0, 1) for i in (-1, 0, 1)):
                r += "o"
            else:
                r += "."
        rows.append(r)
    return rows


ICON_PAL = {"w": hexrgba("ffffff"), "o": hexrgba("080b16", 200)}


def ang(x, y):
    return math.atan2(y, x)


def gear(x, y):
    r = math.hypot(x, y)
    teeth = math.cos(8 * ang(x, y)) > 0.2
    return 2.3 < r <= (6.6 if teeth else 4.9)


def seg(x, y, ax, ay, bx, by):
    dx, dy = bx - ax, by - ay
    t = max(0.0, min(1.0, ((x - ax) * dx + (y - ay) * dy) / (dx * dx + dy * dy)))
    return math.hypot(x - ax - t * dx, y - ay - t * dy)


def tri(x, y, a, b, c):
    def side(p, q):
        return (x - q[0]) * (p[1] - q[1]) - (p[0] - q[0]) * (y - q[1])
    s1, s2, s3 = side(a, b), side(b, c), side(c, a)
    return not ((s1 < 0 or s2 < 0 or s3 < 0) and (s1 > 0 or s2 > 0 or s3 > 0))


def restart(x, y):
    r = math.hypot(x, y)
    a = math.degrees(ang(x, y)) % 360
    ring = 3.3 <= r <= 5.5 and not (290 < a < 350)
    head = tri(x, y, (1.0, -7.5), (7.2, -4.4), (2.2, -1.2))
    return ring or head


def play(x, y):
    return -3.5 <= x <= 5 and abs(y) <= (5 - x) * 0.62


def door_exit(x, y):
    box = (-6 <= x <= 1 and -6.5 <= y <= 6.5) and not (-4.5 < x < -0.5 and -5 < y < 5)
    arrow = (-1.5 <= x <= 4.5 and abs(y) <= 1.0) or (3 <= x <= 6.8 and abs(y) <= (6.8 - x) * 0.9)
    return box or arrow


def lock(x, y):
    body = -5 <= x <= 5 and -0.5 <= y <= 6
    hole = abs(x) <= 0.9 and 1.5 <= y <= 4
    r = math.hypot(x, y + 0.5)
    shackle = y < -0.5 and 2.6 <= r <= 4.4
    return (body and not hole) or shackle


def check(x, y):
    return seg(x, y, -5.0, 0.0, -1.5, 3.8) <= 1.4 or seg(x, y, -1.5, 3.8, 5.5, -4.5) <= 1.4


def cross(x, y):
    return (abs(x - y) <= 1.6 or abs(x + y) <= 1.6) and abs(x) <= 5.2 and abs(y) <= 5.2


def swap_icon(x, y):
    top = (-5 <= x <= 3 and -4.5 <= y <= -2.5) or (3 <= x <= 6.5 and abs(y + 3.5) <= (6.5 - x) * 0.85)
    bot = (-3 <= x <= 5 and 2.5 <= y <= 4.5) or (-6.5 <= x <= -3 and abs(y - 3.5) <= (x + 6.5) * 0.85)
    return top or bot


def grid(x, y):
    return any(abs(x - cx) <= 2.2 and abs(y - cy) <= 2.2 for cx in (-3.2, 3.2) for cy in (-3.2, 3.2))


def back(x, y):
    return (-2 <= x <= 6 and abs(y) <= 1.0) or (-6.5 <= x <= -2 and abs(y) <= (x + 6.5) * 1.0)


def title_background():
    W, H = 480, 270
    rnd = random.Random(11)
    px = [(0, 0, 0, 255)] * (W * H)

    def put(x, y, c):
        if 0 <= x < W and 0 <= y < H:
            px[y * W + x] = c

    sky = [hexrgba(c) for c in ("0a0f24", "0e1430", "131a3b", "1a1f47", "221f4f", "2c2457", "3a2a5e")]
    bayer = [[0, 2], [3, 1]]
    for y in range(H):
        t = y / (H - 1) * (len(sky) - 1)
        i, f = int(t), t - int(t)
        for x in range(W):
            c = sky[min(i + (1 if f * 4 > bayer[y % 2][x % 2] + 0.5 else 0), len(sky) - 1)]
            put(x, y, c)

    for _ in range(150):
        x, y = rnd.randrange(W), rnd.randrange(170)
        c = rnd.choice([hexrgba("ffffff"), hexrgba("9fd8ff"), hexrgba("5ef2ff"), hexrgba("c9b8ff")])
        put(x, y, c)
        if rnd.random() < 0.12:
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                put(x + dx, y + dy, (c[0], c[1], c[2], 255) if rnd.random() < 0.5 else hexrgba("3a4a8a"))

    mx, my, mr = 412, 46, 20
    for y in range(my - mr, my + mr + 1):
        for x in range(mx - mr, mx + mr + 1):
            d = math.hypot(x - mx, y - my)
            if d <= mr:
                shade = hexrgba("f4f1dc") if (x - mx) + (y - my) < 8 else hexrgba("d6d2b8")
                if math.hypot(x - mx + 6, y - my - 4) < 4 or math.hypot(x - mx - 7, y - my + 6) < 3:
                    shade = hexrgba("bdb89c")
                put(x, y, shade)
            elif d <= mr + 2 and (x + y) % 2 == 0:
                put(x, y, hexrgba("3a3a66"))

    far, near = hexrgba("161a38"), hexrgba("10132a")
    for layer, col, base, hmin, hmax in ((0, far, 236, 40, 110), (1, near, 246, 20, 70)):
        x = -rnd.randrange(20)
        while x < W:
            w = rnd.randrange(18, 40)
            h = rnd.randrange(hmin, hmax)
            if 150 < x + w // 2 < 330:
                h = h // 3
            for yy in range(base - h, base):
                for xx in range(x, x + w):
                    put(xx, yy, col)
            for wy in range(base - h + 6, base - 4, 9):
                for wx in range(x + 4, x + w - 4, 7):
                    if rnd.random() < (0.25 if layer == 0 else 0.15):
                        put(wx, wy, hexrgba("3ff5e3" if rnd.random() < 0.6 else "ffd25e"))
                        put(wx + 1, wy, hexrgba("2aa89c"))
            x += w + rnd.randrange(2, 10)

    tile = [[TILE_PAL[c] for c in row] for row in TILE]

    def bricks(x0, y0, w, h):
        for y in range(h):
            for x in range(w):
                put(x0 + x, y0 + y, tile[y % 16][x % 16])

    bricks(0, 246, W, 24)
    bricks(40, 190, 80, 8)
    bricks(360, 120, 80, 8)
    bricks(440, 120, 8, 126)
    spike = [[SPIKE_PAL.get(c) for c in row] for row in SPIKE]
    for sx in range(176, 304, 16):
        for y in range(10):
            for x in range(16):
                if spike[y][x]:
                    put(sx + x, 236 + y, spike[y][x])
    pad = [[PAD_PAL.get(c) for c in row] for row in PAD]
    for y in range(16):
        for x in range(16):
            if pad[y][x]:
                put(380 + x, 230 + y, pad[y][x])

    write_rgba(os.path.join(UI, "title_bg.png"), W, H, px)


def ui_sprites():
    os.makedirs(UI, exist_ok=True)
    write_png(os.path.join(UI, "panel.png"), PANEL, PANEL_PAL)
    write_png(os.path.join(UI, "button.png"), BUTTON, BUTTON_PAL)
    write_png(os.path.join(UI, "track.png"), TRACK, TRACK_PAL)
    write_png(os.path.join(UI, "fill.png"), FILL, FILL_PAL)
    write_png(os.path.join(UI, "knob.png"), KNOB, BUTTON_PAL)
    write_png(os.path.join(UI, "pill.png"), PILL, PILL_PAL)
    for name, fn in (("gear", gear), ("restart", restart), ("play", play), ("exit", door_exit),
                     ("lock", lock), ("check", check), ("close", cross), ("swap", swap_icon),
                     ("grid", grid), ("back", back)):
        write_png(os.path.join(UI, "icon_" + name + ".png"), icon(fn), ICON_PAL)
    title_background()


SR = 44100


def square(phase, duty=0.5):
    return 1.0 if (phase % 1.0) < duty else -1.0


def triangle(phase):
    p = phase % 1.0
    return 4 * p - 1 if p < 0.5 else 3 - 4 * p


def write_wav(path, samples, vol=0.8):
    peak = max(1e-6, max(abs(s) for s in samples))
    k = vol / peak
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s * k)) * 32767)) for s in samples))


def sweep(dur, f0, f1, wave_fn, env_pow=1.0, duty=0.5):
    n = int(dur * SR)
    out, ph = [], 0.0
    for i in range(n):
        t = i / n
        f = f0 * (f1 / f0) ** t
        ph += f / SR
        env = (1 - t) ** env_pow
        out.append((square(ph, duty) if wave_fn == "sq" else math.sin(2 * math.pi * ph)) * env)
    return out


def sfx():
    os.makedirs(AUDIO, exist_ok=True)
    rnd = random.Random(7)

    write_wav(os.path.join(AUDIO, "jump.wav"), sweep(0.13, 280, 640, "sq", 1.5, 0.25), 0.45)

    n = int(0.28 * SR)
    out, p1, p2 = [], 0.0, 0.0
    for i in range(n):
        t = i / n
        p1 += (1400 * (1 - t) + 220 * t) / SR
        p2 += (300 + 1500 * t) / SR
        env = math.sin(math.pi * min(1, t * 1.2)) ** 0.7
        vib = math.sin(2 * math.pi * 30 * i / SR)
        s = 0.55 * math.sin(2 * math.pi * p1 + vib) + 0.35 * square(p2, 0.3) * 0.6
        s += (rnd.random() * 2 - 1) * 0.12 * (1 - t)
        out.append(s * env)
    write_wav(os.path.join(AUDIO, "swap.wav"), out, 0.6)

    n = int(0.16 * SR)
    out, ph = [], 0.0
    for i in range(n):
        t = i / n
        ph += 110 / SR
        gate = 1.0 if (t < 0.4 or t > 0.55) else 0.0
        out.append(square(ph, 0.5) * gate * (1 - t))
    write_wav(os.path.join(AUDIO, "swap_fail.wav"), out, 0.35)

    n = int(0.55 * SR)
    out, ph = [], 0.0
    for i in range(n):
        t = i / n
        ph += (520 * (1 - t) ** 2 + 60) / SR
        s = square(ph, 0.5) * 0.7 + (rnd.random() * 2 - 1) * 0.3 * (1 - t)
        out.append(s * (1 - t) ** 1.2)
    write_wav(os.path.join(AUDIO, "death.wav"), out, 0.5)

    notes = [523.25, 659.25, 783.99, 1046.5]
    out = []
    for idx, f in enumerate(notes):
        dur = 0.09 if idx < 3 else 0.45
        n = int(dur * SR)
        ph = 0.0
        for i in range(n):
            t = i / n
            ph += f / SR
            env = (1 - t) ** (0.6 if idx == 3 else 0.2)
            out.append((square(ph, 0.25) * 0.6 + triangle(ph * 0.5) * 0.4) * env)
    write_wav(os.path.join(AUDIO, "clear.wav"), out, 0.55)

    write_wav(os.path.join(AUDIO, "ui_click.wav"), sweep(0.07, 660, 1320, "sq", 2.0, 0.25), 0.35)
    write_wav(os.path.join(AUDIO, "ui_hover.wav"), sweep(0.03, 1500, 1700, "sin", 1.5), 0.18)


def bgm():
    bpm = 104
    beat = 60 / bpm
    total = int(8 * 4 * beat * SR)
    buf = [0.0] * total
    rnd = random.Random(3)

    def note(freq, start, dur, fn, vol, duty=0.5):
        s0, n = int(start * SR), int(dur * SR)
        ph = 0.0
        for i in range(n):
            j = s0 + i
            if j >= total:
                break
            t = i / n
            ph += freq / SR
            env = min(1, i / 200) * (1 - t) ** 1.5
            v = triangle(ph) if fn == "tri" else square(ph, duty)
            buf[j] += v * vol * env

    def hz(m):
        return 440 * 2 ** ((m - 69) / 12)

    chords = [(57, [69, 72, 76]), (53, [69, 72, 77]), (48, [67, 72, 76]), (55, [67, 71, 74])] * 2
    for bar, (root, tones) in enumerate(chords):
        t0 = bar * 4 * beat
        for b in range(4):
            note(hz(root - 12), t0 + b * beat, beat * 0.9, "tri", 0.5)
        arp = tones + [tones[1]]
        for s in range(8):
            m = arp[s % 4] + (12 if (bar % 2 == 1 and s >= 6) else 0)
            note(hz(m), t0 + s * beat / 2, beat * 0.45, "sq", 0.10, 0.25)
        for s in range(8):
            s0 = int((t0 + s * beat / 2) * SR)
            for i in range(int(0.03 * SR)):
                if s0 + i < total:
                    buf[s0 + i] += (rnd.random() * 2 - 1) * 0.05 * (1 - i / (0.03 * SR))
    melody = [76, None, 74, 72, 74, None, 72, 69, 72, None, 71, 67, 69, None, None, None]
    for k, m in enumerate(melody * 2):
        if m is not None:
            note(hz(m), (k * 2) * beat, beat * 1.6, "sq", 0.09, 0.5)
    write_wav(os.path.join(AUDIO, "bgm.wav"), buf, 0.5)


if __name__ == "__main__":
    sprites()
    ui_sprites()
    sfx()
    bgm()
    print("assets generated")
