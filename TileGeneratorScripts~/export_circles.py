from truchet_viewer import show_tiles
from truchet_viewer.n6 import n6_circles

show_tiles(
    n6_circles,
    output="circles.png",
    with_value=False,
    with_name=True
)