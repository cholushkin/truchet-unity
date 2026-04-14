# TileGenerator.py

import math
import cairo
import os
import itertools
import argparse


# ----------------------------
# Drawing primitives
# ----------------------------

class Draw:
    @staticmethod
    def circle(ctx, x, y, r):
        ctx.arc(x, y, r, 0, 2 * math.pi)

    @staticmethod
    def arc(ctx, x, y, r, a0, a1):
        ctx.arc(x, y, r, a0, a1)


# ----------------------------
# Transform helpers
# ----------------------------

class Transform:
    @staticmethod
    def rotate(ctx, size, turns):
        ctx.translate(size / 2, size / 2)
        ctx.rotate(turns * math.pi / 2)
        ctx.translate(-size / 2, -size / 2)

    @staticmethod
    def flip_x(ctx, size):
        ctx.translate(size, 0)
        ctx.scale(-1, 1)

    @staticmethod
    def flip_y(ctx, size):
        ctx.translate(0, size)
        ctx.scale(1, -1)


# ----------------------------
# Base Tile
# ----------------------------

class Tile:
    def base_name(self):
        raise NotImplementedError

    def draw(self, ctx, size):
        raise NotImplementedError


# ----------------------------
# Tile Variant
# ----------------------------

class TileVariant:
    def __init__(self, tile, rot=0, flip_x=False, flip_y=False):
        self.tile = tile
        self.rot = rot
        self.flip_x = flip_x
        self.flip_y = flip_y

    def full_name(self):
        return f"{self.tile.base_name()}_r{self.rot}_fx{int(self.flip_x)}_fy{int(self.flip_y)}"

    def render(self, ctx, size, bg, fg):
        ctx.set_source_rgb(*bg)
        ctx.rectangle(0, 0, size, size)
        ctx.fill()

        ctx.save()

        if self.flip_x:
            Transform.flip_x(ctx, size)
        if self.flip_y:
            Transform.flip_y(ctx, size)
        if self.rot:
            Transform.rotate(ctx, size, self.rot)

        ctx.set_source_rgb(*fg)
        self.tile.draw(ctx, size)

        ctx.restore()


# ----------------------------
# Variant generator
# ----------------------------

def generate_variants(tiles, rotations=False, mirror_x=False, mirror_y=False):
    rot_values = [0, 1, 2, 3] if rotations else [0]
    flip_x_values = [False, True] if mirror_x else [False]
    flip_y_values = [False, True] if mirror_y else [False]

    variants = []

    for tile in tiles:
        for r, fx, fy in itertools.product(rot_values, flip_x_values, flip_y_values):
            variants.append(TileVariant(tile, r, fx, fy))

    return variants


# ----------------------------
# Rendering
# ----------------------------

def render_tile_to_file(variant, size, bg, fg, path):
    surface = cairo.ImageSurface(cairo.FORMAT_ARGB32, size, size)
    ctx = cairo.Context(surface)

    variant.render(ctx, size, bg, fg)
    surface.write_to_png(path)


def render_separate(variants, size, out_dir, bg=(1, 1, 1), fg=(0, 0, 0)):
    os.makedirs(out_dir, exist_ok=True)

    for v in variants:
        path = os.path.join(out_dir, v.full_name() + ".png")
        render_tile_to_file(v, size, bg, fg, path)


def render_atlas(variants, size, out_path, bg=(1, 1, 1), fg=(0, 0, 0)):
    n = len(variants)
    cols = math.ceil(math.sqrt(n))
    rows = math.ceil(n / cols)

    width = cols * size
    height = rows * size

    surface = cairo.ImageSurface(cairo.FORMAT_ARGB32, width, height)
    ctx = cairo.Context(surface)

    for i, v in enumerate(variants):
        x = (i % cols) * size
        y = (i // cols) * size

        ctx.save()
        ctx.translate(x, y)
        v.render(ctx, size, bg, fg)
        ctx.restore()

    surface.write_to_png(out_path)


# ----------------------------
# Engine entry
# ----------------------------

def run(tiles, args=None):
    parser = argparse.ArgumentParser()

    parser.add_argument("--atlas", action="store_true")
    parser.add_argument("--tile_size", type=int, default=128)
    parser.add_argument("--out", type=str, default="output")

    parser.add_argument("--generate-rotations", action="store_true")
    parser.add_argument("--generate-mirror-x", action="store_true")
    parser.add_argument("--generate-mirror-y", action="store_true")

    if args is None:
        args = parser.parse_args()
    else:
        args = parser.parse_args(args)

    variants = generate_variants(
        tiles,
        rotations=args.generate_rotations,
        mirror_x=args.generate_mirror_x,
        mirror_y=args.generate_mirror_y,
    )

    if args.atlas:
        out_path = args.out if args.out.endswith(".png") else args.out + ".png"
        render_atlas(variants, args.tile_size, out_path)
    else:
        render_separate(variants, args.tile_size, args.out)