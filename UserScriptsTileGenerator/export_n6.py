from truchet_viewer import show_tiles
from truchet_viewer.n6 import n6_tiles

show_tiles(
    n6_tiles,
    output="n6.png",
    with_value=False,
    with_name=True,
)