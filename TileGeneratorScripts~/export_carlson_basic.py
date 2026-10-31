from truchet_viewer import show_tiles
from truchet_viewer.carlson import carlson_basic

show_tiles(
    carlson_basic,
    output="carlson_basic.png",
    with_value=False,
    with_name=True
)