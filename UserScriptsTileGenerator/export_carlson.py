from truchet_viewer import show_tiles
from truchet_viewer.carlson import carlson_tiles

show_tiles(
    carlson_tiles,
    output="carlson.png",
    with_value=False,
    with_name=True
)