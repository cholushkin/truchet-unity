from truchet_viewer import show_tiles
from truchet_viewer.n6 import n6_strokes

show_tiles(
    n6_strokes,
    output="strokes.png",
    with_value=False,
    with_name=True
)