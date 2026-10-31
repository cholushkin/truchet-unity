from truchet_viewer import show_tiles
from truchet_viewer.n6 import n6_lattice

show_tiles(
    n6_lattice,
    output="lattice.png",
    with_value=False,
    with_name=True
)