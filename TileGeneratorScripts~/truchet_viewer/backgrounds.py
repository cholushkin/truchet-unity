"""Generate multiscale truchet images using n6_circles tiles."""

from pathlib import Path

from truchet_viewer.helpers import make_bgfg
from truchet_viewer.n6 import n6_circles
from truchet_viewer.tiler import multiscale_truchet

# --- base output dir (LOCAL, not user home) ---
BASE = Path.cwd() / "wallpapers"
BASE.mkdir(exist_ok=True)


def generate_set(name, width, height, tilew, nimg, seed_mul=1, use_palette=True):
    DIR = BASE / name
    DIR.mkdir(parents=True, exist_ok=True)

    for i in range(nimg):
        print(f"[{name}] Generating {i + 1}/{nimg}")
        out = DIR / f"bg_{i:03d}.png"

        kwargs = {}
        if use_palette:
            kwargs.update(make_bgfg(i / nimg, (0.55, 0.45), 0.45))
        else:
            kwargs.update({
                "bg": "#335495",
                "fg": "#243b6a",
            })

        multiscale_truchet(
            tiles=n6_circles,
            width=width,
            height=height,
            tilew=tilew,
            nlayers=3,
            chance=0.4,
            seed=i * seed_mul,
            format="png",
            output=str(out),
            tile_chooser=None,
            grid=False,
            should_split=None,
            **kwargs,
        )


# --- generate all sets ---

generate_set("1680", 1680, 1050, 200, nimg=10)
generate_set("1920", 1920, 1080, 200, nimg=5)
generate_set("2872", 2872, 5108, 300, nimg=5, use_palette=False)
generate_set("1536", 1536, 960, 200, nimg=5, seed_mul=3, use_palette=False)
generate_set("2560", 2560, 1440, 150, nimg=20, seed_mul=10)