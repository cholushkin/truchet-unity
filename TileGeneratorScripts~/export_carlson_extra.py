from truchet_viewer import show_tiles
from truchet_viewer.carlson import carlson_extra

show_tiles(
    carlson_extra,
    output="carlson_extra.png",
    with_value=False,
    with_name=True
)