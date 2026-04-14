TileGenerator.py         engine + CLI (central)
Tileset_n6.py            data only
render_tileset_n6.py     user script (default params)
render_tileset_n6_rot_all.py  user script (different params)



Engine owns:
- ArgumentParser
- rendering
- variants

User scripts:
- just call engine with a tileset + config