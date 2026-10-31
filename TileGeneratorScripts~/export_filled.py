from truchet_viewer import show_tiles
from truchet_viewer.n6 import n6_filled

show_tiles(
    n6_filled,
    output="filled.png",
    with_value=False,
    with_name=True
)