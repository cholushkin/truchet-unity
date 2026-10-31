from truchet_viewer.n6 import n6_tiles
from truchet_viewer.drawing import cairo_context, CE, CN, CS, CW

tile = n6_tiles[0]

tile_name = tile.__class__.__name__
filename = f"{tile_name}.png"

tile_size = 256
inner = tile_size * 0.6

with cairo_context(tile_size, tile_size, output=filename) as ctx:
    # --- 1. gray background ---    
    ctx.set_source_rgba(0, 0, 0, 0)          
    ctx.paint()

    ctx.save()
    ctx.translate((tile_size - inner) / 2, (tile_size - inner) / 2)

    wh = inner
    wh1 = wh / 3

    # --- 2. tile ---
    tile.draw_tile(ctx, inner)

    ctx.restore()