"""
Generates the Bodymovin/Lottie JSON assets embedded under
SophiaWin11.UI/Assets/Animations/.

Standalone, offline, deterministic: no GUI export tool (After Effects) is
available in this environment, so every asset is emitted directly as valid
Bodymovin JSON (shape layers, keyframes, cubic-bezier easing) built from the
same Art Deco palette defined in SophiaWin11.UI/Theme/DesignTokens.xaml.
Re-run this script any time an asset needs to change; it is not invoked at
application runtime (offline-first, zero generation on load).

Usage: python tools/animations/generate_lottie_assets.py
"""

import json
import math
import os

OUTPUT_DIR = os.path.join(
    os.path.dirname(__file__), "..", "..",
    "src", "SophiaWin11.UI", "Assets", "Animations",
)

# Palette copied 1:1 from SophiaWin11.UI/Theme/DesignTokens.xaml -- token
# names are kept alongside every hex value below so the mapping back to the
# theme's source of truth stays traceable from this file alone.
COLOR_BACKGROUND_DEEP = "#0B0B10"
COLOR_BACKGROUND_CHARCOAL = "#151521"
COLOR_ACCENT_GOLD = "#D4AF37"
COLOR_ACCENT_GOLD_DARK = "#B8860B"
COLOR_ACCENT_GOLD_LIGHT = "#F4E5A1"
COLOR_ACCENT_EMERALD = "#0F5C4C"
COLOR_ACCENT_BORDEAUX = "#6E1423"
COLOR_ACCENT_PEACOCK = "#0F3D5C"
COLOR_TEXT_PRIMARY = "#F5F1E6"


def hex_rgb01(hex_color):
    hex_color = hex_color.lstrip("#")
    r = int(hex_color[0:2], 16) / 255.0
    g = int(hex_color[2:4], 16) / 255.0
    b = int(hex_color[4:6], 16) / 255.0
    return [round(r, 4), round(g, 4), round(b, 4)]


def static_prop(value):
    return {"a": 0, "k": value}


def animated_prop(keyframes, dims=1):
    frames = []
    count = len(keyframes)
    for index, (t, value) in enumerate(keyframes):
        v = value if isinstance(value, list) else [value]
        entry = {"t": t, "s": v}
        if index < count - 1:
            if dims == 1:
                entry["i"] = {"x": [0.4], "y": [1]}
                entry["o"] = {"x": [0.6], "y": [0]}
            else:
                entry["i"] = {"x": [0.4] * dims, "y": [1] * dims}
                entry["o"] = {"x": [0.6] * dims, "y": [0] * dims}
        frames.append(entry)
    return {"a": 1, "k": frames}


def transform(position=(0, 0), anchor=(0, 0), scale=(100, 100, 100), rotation=0, opacity=100):
    return {
        "ty": "tr",
        "p": static_prop([position[0], position[1], 0]),
        "a": static_prop([anchor[0], anchor[1], 0]),
        "s": static_prop(list(scale)),
        "r": static_prop(rotation),
        "o": static_prop(opacity),
    }


def fill(color_hex, opacity=100):
    r, g, b = hex_rgb01(color_hex)
    return {"ty": "fl", "c": static_prop([r, g, b, 1]), "o": static_prop(opacity)}


def stroke(color_hex, width, opacity=100, cap=2, join=2):
    r, g, b = hex_rgb01(color_hex)
    return {
        "ty": "st",
        "c": static_prop([r, g, b, 1]),
        "o": static_prop(opacity),
        "w": static_prop(width),
        "lc": cap,
        "lj": join,
    }


def trim(start_prop, end_prop, offset=0):
    return {"ty": "tm", "s": start_prop, "e": end_prop, "o": static_prop(offset)}


def path_item(points, closed=False):
    n = len(points)
    zero = [0, 0]
    return {"ty": "sh", "ks": static_prop({"i": [zero] * n, "o": [zero] * n, "v": points, "c": closed})}


def ellipse_item(size, position=(0, 0)):
    return {"ty": "el", "p": static_prop(list(position)), "s": static_prop(list(size))}


def group(items, name=None):
    g = {"ty": "gr", "it": list(items) + []}
    if name:
        g["nm"] = name
    return g


def shape_layer(ind, name, shapes, ip, op, position=(0, 0), anchor=(0, 0),
                 rotation=0, rotation_anim=None, scale=(100, 100, 100),
                 opacity=100, opacity_anim=None):
    return {
        "ddd": 0,
        "ind": ind,
        "ty": 4,
        "nm": name,
        "sr": 1,
        "ks": {
            "o": opacity_anim if opacity_anim else static_prop(opacity),
            "r": rotation_anim if rotation_anim else static_prop(rotation),
            "p": static_prop([position[0], position[1], 0]),
            "a": static_prop([anchor[0], anchor[1], 0]),
            "s": static_prop(list(scale)),
        },
        "ao": 0,
        "shapes": shapes,
        "ip": ip,
        "op": op,
        "st": 0,
    }


def document(name, w, h, fr, op, layers):
    return {"v": "5.7.4", "fr": fr, "ip": 0, "op": op, "w": w, "h": h, "nm": name, "ddd": 0, "assets": [], "layers": layers}


def radial_chevron_fan(cx, cy, count, r_inner, r_outer, half_angle_deg, colors, angle_offset_deg=0):
    groups = []
    for i in range(count):
        angle = 2 * math.pi * i / count + math.radians(angle_offset_deg)
        a1 = angle - math.radians(half_angle_deg)
        a2 = angle
        a3 = angle + math.radians(half_angle_deg)
        p1 = [round(cx + r_inner * math.cos(a1), 2), round(cy + r_inner * math.sin(a1), 2)]
        p2 = [round(cx + r_outer * math.cos(a2), 2), round(cy + r_outer * math.sin(a2), 2)]
        p3 = [round(cx + r_inner * math.cos(a3), 2), round(cy + r_inner * math.sin(a3), 2)]
        color = colors[i % len(colors)]
        groups.append(group([path_item([p1, p2, p3]), fill(color), transform()]))
    return groups


def ring_shape(cx, cy, radius, color_hex, width, opacity=100):
    return group([ellipse_item((radius * 2, radius * 2), (cx, cy)), stroke(color_hex, width, opacity), transform()])


def trimmed_shape(shape_item_value, color_hex, width, start_prop, end_prop):
    return group([shape_item_value, trim(start_prop, end_prop), stroke(color_hex, width), transform()])


def write(name, doc):
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    path = os.path.join(OUTPUT_DIR, name)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(doc, f, separators=(",", ":"))
    size = os.path.getsize(path)
    print(f"{name}: {size} bytes")


# ---------------------------------------------------------------------------
# 1. Splash screen loop -- ~4s perfect loop, gold sunburst fan + pulsing ring
#    + pulsing center diamond, deep-black-transparent background.
# ---------------------------------------------------------------------------
def build_splash_loop():
    w = h = 200
    cx = cy = 100
    fr = 30
    op = 120  # 4.0s @ 30fps

    fan_layer = shape_layer(
        1, "SunburstFan",
        radial_chevron_fan(cx, cy, 12, 55, 88, 9,
                            [COLOR_ACCENT_GOLD, COLOR_ACCENT_GOLD_DARK]),
        0, op,
        position=(cx, cy), anchor=(cx, cy),
        rotation_anim=animated_prop([(0, 0), (op, 360)], dims=1),
    )

    ring_layer = shape_layer(
        2, "OuterRing",
        [ring_shape(cx, cy, 92, COLOR_ACCENT_GOLD, 2, opacity=55)],
        0, op,
        opacity_anim=animated_prop([(0, 55), (60, 85), (op, 55)], dims=1),
    )

    diamond_points = [[cx, cy - 16], [cx + 16, cy], [cx, cy + 16], [cx - 16, cy]]
    diamond_layer = shape_layer(
        3, "CenterDiamond",
        [group([path_item(diamond_points, closed=True), fill(COLOR_ACCENT_GOLD_LIGHT), transform()])],
        0, op,
        position=(cx, cy), anchor=(cx, cy),
    )
    diamond_layer["ks"]["s"] = animated_prop(
        [(0, [82, 82, 100]), (60, [112, 112, 100]), (op, [82, 82, 100])], dims=3,
    )

    return document("SophiaSplashLoop", w, h, fr, op, [diamond_layer, fan_layer, ring_layer])


# ---------------------------------------------------------------------------
# 2. Status: success -- emerald ring draw-in + checkmark draw-in, then hold,
#    then a quick crossfade back to frame 0 so continuous looping reads as a
#    gentle confirm pulse rather than a jarring reset.
# ---------------------------------------------------------------------------
def build_status_success():
    w = h = 64
    cx = cy = 32
    fr = 30
    op = 45  # 1.5s

    ring = ellipse_item((52, 52), (cx, cy))
    ring_group = trimmed_shape(
        ring, COLOR_ACCENT_EMERALD, 4,
        animated_prop([(0, 0), (18, 0)], dims=1),
        animated_prop([(0, 0), (18, 100)], dims=1),
    )

    check_points = [[cx - 12, cy + 1], [cx - 3, cy + 10], [cx + 14, cy - 11]]
    check_group = trimmed_shape(
        path_item(check_points), COLOR_ACCENT_GOLD_LIGHT, 5,
        animated_prop([(10, 0), (26, 0)], dims=1),
        animated_prop([(10, 0), (26, 100)], dims=1),
    )

    layer = shape_layer(
        1, "SuccessCheck",
        [check_group, ring_group],
        0, op,
        opacity_anim=animated_prop([(0, 0), (4, 100), (34, 100), (op, 0)], dims=1),
    )

    return document("SophiaStatusSuccess", w, h, fr, op, [layer])


# ---------------------------------------------------------------------------
# 3. Status: failure -- bordeaux ring draw-in + X draw-in, same hold/crossfade
#    loop shape as success so the two read as a matched pair.
# ---------------------------------------------------------------------------
def build_status_failure():
    w = h = 64
    cx = cy = 32
    fr = 30
    op = 45

    ring = ellipse_item((52, 52), (cx, cy))
    ring_group = trimmed_shape(
        ring, COLOR_ACCENT_BORDEAUX, 4,
        animated_prop([(0, 0), (18, 0)], dims=1),
        animated_prop([(0, 0), (18, 100)], dims=1),
    )

    stroke1 = [[cx - 11, cy - 11], [cx + 11, cy + 11]]
    stroke2 = [[cx + 11, cy - 11], [cx - 11, cy + 11]]
    x1_group = trimmed_shape(
        path_item(stroke1), COLOR_ACCENT_GOLD_LIGHT, 5,
        animated_prop([(10, 0), (20, 0)], dims=1),
        animated_prop([(10, 0), (20, 100)], dims=1),
    )
    x2_group = trimmed_shape(
        path_item(stroke2), COLOR_ACCENT_GOLD_LIGHT, 5,
        animated_prop([(18, 0), (28, 0)], dims=1),
        animated_prop([(18, 0), (28, 100)], dims=1),
    )

    layer = shape_layer(
        1, "FailureCross",
        [x2_group, x1_group, ring_group],
        0, op,
        opacity_anim=animated_prop([(0, 0), (4, 100), (34, 100), (op, 0)], dims=1),
    )

    return document("SophiaStatusFailure", w, h, fr, op, [layer])


# ---------------------------------------------------------------------------
# 4. Status: in-progress -- small, fast, seamless spinning gold chevron ring.
#    Replaces the v0.8.0.0 placeholder loading-pulse.json.
# ---------------------------------------------------------------------------
def build_status_progress():
    w = h = 64
    cx = cy = 32
    fr = 30
    op = 30  # 1.0s, fast loop suited to a small inline spinner

    fan_layer = shape_layer(
        1, "ProgressChevrons",
        radial_chevron_fan(cx, cy, 8, 16, 27, 10,
                            [COLOR_ACCENT_GOLD, COLOR_ACCENT_GOLD_DARK]),
        0, op,
        position=(cx, cy), anchor=(cx, cy),
        rotation_anim=animated_prop([(0, 0), (op, 360)], dims=1),
    )

    return document("SophiaStatusProgress", w, h, fr, op, [fan_layer])


# ---------------------------------------------------------------------------
# 5. Loading mascot -- distinct, more elaborate multi-ring motif for genuinely
#    long PowerShellNative operations (DISM servicing, UWP export), so it
#    reads as "this will take a while" rather than the quick inline spinner.
# ---------------------------------------------------------------------------
def build_loading_mascot():
    w = h = 120
    cx = cy = 60
    fr = 30
    op = 90  # 3.0s

    outer_fan = shape_layer(
        1, "OuterFan",
        radial_chevron_fan(cx, cy, 16, 40, 56, 6,
                            [COLOR_ACCENT_GOLD, COLOR_ACCENT_GOLD_DARK]),
        0, op,
        position=(cx, cy), anchor=(cx, cy),
        rotation_anim=animated_prop([(0, 360), (op, 0)], dims=1),
    )

    mid_ring = shape_layer(
        2, "MidRing",
        [ring_shape(cx, cy, 34, COLOR_ACCENT_PEACOCK, 3, opacity=80)],
        0, op,
        position=(cx, cy), anchor=(cx, cy),
        rotation_anim=animated_prop([(0, 0), (op, 360)], dims=1),
    )

    inner_fan = shape_layer(
        3, "InnerFan",
        radial_chevron_fan(cx, cy, 10, 12, 24, 8,
                            [COLOR_ACCENT_EMERALD, COLOR_ACCENT_GOLD]),
        0, op,
        position=(cx, cy), anchor=(cx, cy),
        rotation_anim=animated_prop([(0, 0), (op, -360)], dims=1),
    )

    diamond_points = [[cx, cy - 8], [cx + 8, cy], [cx, cy + 8], [cx - 8, cy]]
    center = shape_layer(
        4, "CenterPulse",
        [group([path_item(diamond_points, closed=True), fill(COLOR_ACCENT_GOLD_LIGHT), transform()])],
        0, op,
        position=(cx, cy), anchor=(cx, cy),
    )
    center["ks"]["s"] = animated_prop(
        [(0, [85, 85, 100]), (45, [120, 120, 100]), (op, [85, 85, 100])], dims=3,
    )

    return document("SophiaLoadingMascot", w, h, fr, op, [center, inner_fan, mid_ring, outer_fan])


# ---------------------------------------------------------------------------
# 6. About banner -- wide decorative loop, ready to be dropped into the
#    (not-yet-built) About page. Symmetric breathing fan bursts at both
#    ends and a slow shimmer highlight sweeping across a center gold line.
# ---------------------------------------------------------------------------
def build_about_banner():
    w = 480
    h = 140
    fr = 30
    op = 120  # 4.0s perfect loop

    def end_fan(cx, cy, mirror):
        offset = 180 if mirror else 0
        return shape_layer(
            1 if not mirror else 2, "EndFanMirror" if mirror else "EndFan",
            radial_chevron_fan(cx, cy, 9, 18, 40, 8,
                                [COLOR_ACCENT_GOLD, COLOR_ACCENT_GOLD_DARK],
                                angle_offset_deg=offset),
            0, op,
            position=(cx, cy), anchor=(cx, cy),
        )

    left = end_fan(60, h / 2, mirror=False)
    left["ks"]["s"] = animated_prop([(0, [90, 90, 100]), (60, [105, 105, 100]), (op, [90, 90, 100])], dims=3)

    right = end_fan(w - 60, h / 2, mirror=True)
    right["ks"]["s"] = animated_prop([(0, [105, 105, 100]), (60, [90, 90, 100]), (op, [105, 105, 100])], dims=3)

    center_line = shape_layer(
        3, "CenterLine",
        [group([path_item([[100, h / 2], [w - 100, h / 2]]), stroke(COLOR_ACCENT_GOLD, 1.5, opacity=70), transform()])],
        0, op,
    )

    shimmer_w = 70
    shimmer = shape_layer(
        4, "Shimmer",
        [group([
            path_item([[-shimmer_w / 2, -30], [shimmer_w / 2, -30], [shimmer_w / 2, 30], [-shimmer_w / 2, 30]], closed=True),
            fill(COLOR_ACCENT_GOLD_LIGHT, opacity=25),
            transform(),
        ])],
        0, op,
        position=(100, h / 2), anchor=(0, 0),
    )
    shimmer["ks"]["p"] = animated_prop([(0, [100, h / 2, 0]), (op, [w - 100, h / 2, 0])], dims=3)

    return document("SophiaAboutBanner", w, h, fr, op, [shimmer, center_line, left, right])


def main():
    write("splash-loop.json", build_splash_loop())
    write("status-success.json", build_status_success())
    write("status-failure.json", build_status_failure())
    write("status-progress.json", build_status_progress())
    write("loading-mascot.json", build_loading_mascot())
    write("about-banner.json", build_about_banner())


if __name__ == "__main__":
    main()
