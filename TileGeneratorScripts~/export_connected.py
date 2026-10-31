from truchet_viewer import show_tiles
from truchet_viewer.n6 import n6_connected

show_tiles(
    n6_connected,
    output="connected.png",
    with_value=False,
    with_name=True
)