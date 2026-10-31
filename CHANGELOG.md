# Changelog

All notable changes to this package will be documented in this file.

## [1.1.0] - 2026-10-31

### Changed
- Removed the nested `Unity/TruchetTiles/` development-sandbox project, including the
  vendored `gamelib` git submodule and the vendored DOTween library it carried. The
  package's `GameLib.Random`/`GameLib.Alg` usage is unchanged — `com.qcat.gamelib`
  remains a manual UPM dependency (see README).
- Removed all git submodules (`ScriptUtils`, `CodingConvention`, and the vendored
  `gamelib` submodule) and the now-empty `.gitmodules`.
- Removed obsolete tooling folders `UserScripts/`, `UserScriptsCoding/`, and
  `CodingConvention/`.
- Renamed `UserScriptsTileGenerator/` → `TileGeneratorScripts~/` (kept in the repo,
  hidden from Unity's importer).
- Unhid package documentation: merged `Documentation~/*.md` into `Documentation/` and
  removed `Documentation~/`. Updated README links accordingly.

No runtime behavior, public APIs, namespaces or assembly names were changed.

## [1.0.0] - 2026-10-30

### Changed
- Converted to a Unity Package Manager (UPM) package installable via Git URL.
- Moved `Unity/TruchetTiles/Assets/Core/Runtime` → `Runtime/`, `.../Core/Authoring` → `Editor/`,
  `.../Core/Docs` → `Documentation~/`, demo scenes under `.../Core/Scenes/*` → `Samples~/*`,
  and `.../Core/{Materials,Shaders,Textures,DataAndSettings}` → repo-root folders of the same name.
- Added `package.json` (`com.qcat.truchet`), this changelog, and a license file.

No runtime behavior, public APIs, namespaces or assembly names were changed.
