# render_tileset_n6_rot_all.py

from TileGenerator import run
from Tileset_n6 import get_tiles

if __name__ == "__main__":
    run(
        get_tiles(),
        args=[
            "--atlas",
            "--tile_size", "256",
            "--out", "n6_full",
            "--generate-rotations",
            "--generate-mirror-x",
            "--generate-mirror-y",
        ],
    )
