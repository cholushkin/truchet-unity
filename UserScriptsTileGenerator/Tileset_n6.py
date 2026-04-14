# Tileset_n6.py

import math
from TileGenerator import Tile, Draw


class Slash(Tile):
    def base_name(self):
        return "slash"

    def draw(self, ctx, s):
        r = s * 0.5
        Draw.arc(ctx, 0, 0, r, 0, math.pi / 2)
        Draw.arc(ctx, s, s, r, math.pi, 3 * math.pi / 2)
        ctx.fill()


class Dots(Tile):
    def base_name(self):
        return "dots"

    def draw(self, ctx, s):
        r = s * 0.08
        for (x, y) in [(0, 0), (s, 0), (0, s), (s, s)]:
            Draw.circle(ctx, x, y, r)
        ctx.fill()


class ArcBridge(Tile):
    def base_name(self):
        return "arcbridge"

    def draw(self, ctx, s):
        r = s * 0.5
        Draw.arc(ctx, 0, s, r, -math.pi / 2, 0)
        Draw.arc(ctx, s, 0, r, math.pi / 2, math.pi)
        ctx.fill()


def get_tiles():
    return [
        Slash(),
        Dots(),
        ArcBridge(),
    ]