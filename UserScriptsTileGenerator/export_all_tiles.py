from pathlib import Path

from truchet_viewer.n6 import (
    n6_tiles,
    n6_circles,
    n6_connected,
    n6_filled,
    n6_lattice,
    n6_strokes,
    n6_weird,
)
from truchet_viewer.carlson import (
    carlson_basic,
    carlson_tiles,
    carlson_extra,
)
from truchet_viewer.truchet import truchet_tiles
from truchet_viewer.drawing import cairo_context

# --- config ---
OUTPUT_DIR = Path("tiles")
OUTPUT_DIR.mkdir(exist_ok=True)

tile_size = 512
inner = tile_size * 0.6

# --- tile sets ---
TILE_SETS = {
    "n6": n6_tiles,
    "circles": n6_circles,
    "connected": n6_connected,
    "filled": n6_filled,
    "lattice": n6_lattice,
    "strokes": n6_strokes,
    "weird": n6_weird,
    "carlson_basic": carlson_basic,
    "carlson": carlson_tiles,
    "carlson_extra": carlson_extra,
    "truchet": truchet_tiles,
}

# --- export ---
for set_name, tiles in TILE_SETS.items():
    set_dir = OUTPUT_DIR / set_name
    set_dir.mkdir(exist_ok=True)

    for tile in tiles:
        filename = f"{tile.__class__.__name__}_r{tile.rot}_f{int(tile.flipped)}.png"
        output_path = set_dir / filename

        with cairo_context(tile_size, tile_size, output=str(output_path)) as ctx:
            # --- transparent background ---
            ctx.set_source_rgba(0, 0, 0, 0)
            ctx.paint()

            ctx.save()
            ctx.translate((tile_size - inner) / 2, (tile_size - inner) / 2)

            # --- draw tile (includes wings if defined) ---
            tile.draw_tile(ctx, inner)

            ctx.restore()