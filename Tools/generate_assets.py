"""Generates the game's pixel-art sprites and chiptune sounds (no external deps).

Run from the project root:  python Tools/generate_assets.py
Outputs: Assets/Art/*.png, Assets/Resources/Audio/*.wav
"""
import math
import os
import random
import struct
import wave
import zlib

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ART = os.path.join(ROOT, "Assets", "Art")
AUDIO = os.path.join(ROOT, "Assets", "Resources", "Audio")


# ---------------------------------------------------------------- sprites

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
    # 16x24 exit portal: green frame with glowing interior.
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


# ------------------------------------------------------------------ audio

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

    # swap: shimmering down-up warp with a little noise
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


def bgm():
    # 8 bars, 4/4, 104 BPM: Am - F - C - G (x2), bass + arpeggio + soft hats.
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
    sfx()
    bgm()
    print("assets generated")
