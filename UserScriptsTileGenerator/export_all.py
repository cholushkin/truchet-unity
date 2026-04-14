from truchet_viewer import show_tiles
from truchet_viewer.n6 import *
from truchet_viewer.carlson import *
from truchet_viewer.truchet import truchet_tiles

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

for name, tiles in TILE_SETS.items():
    show_tiles(
        tiles,
        output=f"{name}.png",
        with_name=True,
        only_one=True
    )