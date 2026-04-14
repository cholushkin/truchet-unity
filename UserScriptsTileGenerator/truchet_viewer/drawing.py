import contextlib
import math
import cairo

# Compass points for arcs
DEG90 = math.pi / 2
DEG180 = math.pi
CE, CS, CW, CN = [i * DEG90 for i in range(4)]


class CairoContext:
    def __init__(self, width: int, height: int, fmt="png", output=None):
        self.width = width
        self.height = height
        self.output = output
        self.fmt = fmt

        if fmt == "png":
            self.surface = cairo.ImageSurface(cairo.FORMAT_ARGB32, width, height)
        elif fmt == "svg":
            if output is None:
                raise ValueError("SVG requires an output file")
            self.surface = cairo.SVGSurface(output, width, height)
        else:
            raise ValueError(f"Unknown format: {fmt}")

        self.ctx = cairo.Context(self.surface)

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc, tb):
        if self.fmt == "png" and self.output:
            self.surface.write_to_png(self.output)
        self.surface.finish()

    def __getattr__(self, name):
        return getattr(self.ctx, name)

    # --- helpers (used by tiles) ---

    def circle(self, x, y, r):
        self.ctx.arc(x, y, r, 0, 2 * math.pi)

    @contextlib.contextmanager
    def save_restore(self):
        self.ctx.save()
        try:
            yield
        finally:
            self.ctx.restore()

    @contextlib.contextmanager
    def flip_lr(self, wh):
        with self.save_restore():
            self.ctx.translate(wh, 0)
            self.ctx.scale(-1, 1)
            yield

    @contextlib.contextmanager
    def flip_tb(self, wh):
        with self.save_restore():
            self.ctx.translate(0, wh)
            self.ctx.scale(1, -1)
            yield

    @contextlib.contextmanager
    def rotated(self, wh, nturns):
        with self.save_restore():
            self.ctx.translate(wh / 2, wh / 2)
            self.ctx.rotate(math.pi * nturns / 2)
            self.ctx.translate(-wh / 2, -wh / 2)
            yield


def cairo_context(width, height, format="png", output=None):
    return CairoContext(width, height, format, output)
    
    
    
    
_CairoContext = CairoContext