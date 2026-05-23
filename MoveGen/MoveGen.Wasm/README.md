# MoveGen.Wasm

A browser-WASM build of the reference move generator in `../MoveGen.App/`.
Pulls the App's source files in directly (no IL-level reference) and exposes
three functions to JavaScript via `[JSExport]`:

| Export                              | Purpose                                                |
|-------------------------------------|--------------------------------------------------------|
| `Perft(fen, depth) → double`        | Whole-tree perft. Result fits in 2⁵³ for any practical depth. |
| `PerftRootMove(fen, uci, depth)`    | Per-root perft. Lets JS iterate roots and surface progress while keeping the depth-(N−1) subtree at native speed. |
| `LegalMovesUci(fen) → string`       | Space-separated UCI list of legal moves from the FEN. |
| `EngineInfo() → string`             | Identifying string for the "engine in use" indicator.  |

Returns are deliberately `double` / `string` to avoid `[JSMarshalAs]`
annotations — perft node counts always fit in 2⁵³, and UCI lists are short.

## Output

The published bundle is ~11 MB on disk and ~3-4 MB over the wire with the
Brotli artefacts (`.br`) the publish step emits alongside each file:

| File                              | Approx size |
|-----------------------------------|-------------|
| `_framework/dotnet.native.*.wasm` | 5.3 MB      |
| `_framework/System.Private.CoreLib.*.wasm` | 988 KB |
| `_framework/MoveGen.Wasm.*.wasm`  | 24 KB       |

The runtime is the dominant cost; our actual code is tiny. AOT, full trimming,
invariant globalization, no debugger support — see the `.csproj` for the full
set of size/perf flags.

## Prerequisites

- **.NET 10 SDK** (targets `net10.0`).
- **`wasm-tools` workload:** `sudo dotnet workload install wasm-tools` (~300 MB,
  needs sudo on macOS since the SDK lives under `/usr/local/`).
- **`emscripten`** is pulled in by the workload — no separate install.

A plain `dotnet build` works without the workload but skips AOT. For the
optimised bundle the workload is required.

## Build & deploy

The convenience script handles publish + copy in one shot:

```sh
./publish-to-webapp.sh
```

Equivalent to:

```sh
rm -rf bin/Release/publish
dotnet publish -c Release -o bin/Release/publish
rm -rf ../../site/dist/wasm/movegen
mkdir -p ../../site/dist/wasm/movegen
cp -R bin/Release/publish/wwwroot/. ../../site/dist/wasm/movegen/
```

The `rm -rf` of the publish output before each run matters — `dotnet publish`
hashes assemblies into their filenames (`MoveGen.Wasm.8nywo5h9cs.wasm` etc.)
and stale outputs accumulate without it, ballooning the deployed bundle.

The first AOT build takes a couple of minutes (emcc + wasm-opt). Incremental
builds afterwards are ~15-30 s.

## How the webapp consumes it

[`site/src/assets/movegen-wasm.js`](../../site/src/assets/movegen-wasm.js)
is a non-module script that dynamic-imports
`wasm/movegen/_framework/dotnet.js` on demand and exposes the four functions
above via `window.GCT.MoveGenWasm`.

[`site/src/perft-test.html`](../../site/src/perft-test.html) kicks off the
WASM load in the background on page open and shows a status line:

```
Engine: pure-JS (loading .NET WASM in background…)
Engine: MoveGen.Wasm (.NET 10.0.x, AOT)
```

A checkbox lets users flip between WASM and the pure-JS engine
(`assets/movegen.js`). The JS engine is the always-available fallback if the
WASM load fails.

## Performance

Measured locally on the same machine running `MoveGen.App` natively. WASM AOT
clocks in at ~50–60% of native:

| Position / depth | Native .NET | WASM (AOT) | Pure JS (BigInt) |
|------------------|------------:|-----------:|-----------------:|
| Startpos d5      | 0.16 s      | ~0.3 s     | 1.14 s           |
| Startpos d6      | 0.71 s      | ~1.3 s     | ~30 s            |
| Kiwipete d5      | 0.61 s      | ~1.2 s     | impractical      |

In nodes/second that's roughly 167 M nps native vs 80 M nps WASM vs 4 M nps JS
— so the WASM is ~20× faster than the JS port and the runtime payload pays for
itself the first time someone runs d5+ on a complex position.

## Smoke test

`wwwroot/index.html` + `wwwroot/main.js` ship a tiny standalone tester. After
publishing, serve `bin/Release/publish/wwwroot/` directly:

```sh
cd bin/Release/publish/wwwroot
python3 -m http.server 8765
# open http://localhost:8765 and check the console
```

Useful for verifying a build without touching the WebApp.

## Source layout

```
MoveGen.Wasm/
├── MoveGen.Wasm.csproj    SDK="Microsoft.NET.Sdk.WebAssembly", AOT-tuned
├── Program.cs             Browser-WASM entry — calls Magic.Init() to warm tables
├── Exports.cs             The four [JSExport] methods
├── publish-to-webapp.sh   AOT publish + copy to site/dist/wasm/movegen/
└── wwwroot/               Smoke-test page (not shipped)
    ├── index.html
    └── main.js
```

The `.csproj` `<Compile Include="..\MoveGen.App\*.cs">` references the App's
source files directly. We don't `<ProjectReference>` MoveGen.App because it's
an `Exe`, which would conflict with our own `Program.cs`.
