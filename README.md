# The Grand Chess Tree

[grandchesstree.com](https://grandchesstree.com/) · [Discord](https://discord.gg/cTu3aeCZVe) · [Docs](https://timmoth.github.io/grandchesstree/)

A community-driven project to traverse the depths of chess — counting **perft**
nodes at extreme depths from notable positions, alongside the writing, tooling,
and reference code needed to do it well. Started as an offshoot of building the
[Sapling](https://github.com/Timmoth/Sapling) move generator and grew into a
six-piece bundle: distributed compute, a reference move generator, an article
series teaching how to build one, a static site with interactive engine-dev
tools, a perft compliance CLI, and the raw result data.

## What's in this repo

| Where                 | What                                                                                                    |
|-----------------------|---------------------------------------------------------------------------------------------------------|
| `Distributed-Perft/`  | The distributed-perft system — REST API + worker client + worker engine + shared types + admin toolkit + docker stack + mkdocs docs. `GrandChessTree.sln` lives here. |
| `MoveGen/`            | The reference C# move generator the article series builds toward. Used by PerftSuite, the in-browser perft tester (via AOT WASM), and the published articles. Passes perft on the six standard CPW positions, 119/119 xUnit tests green. |
| `site/`               | The static site at [grandchesstree.com](https://grandchesstree.com/). `site/build.js` turns `site/src/` into a deployable `site/dist/` (templated chrome, asset copy, article publishing). |
| `PerftSuite/`         | `perftcheck` — a cross-platform CLI that validates any UCI engine's `go perft N` against ~28k known-correct positions. Bundled corpora baked into the binary; pre-built releases on GitHub. |
| `results/`            | Raw distributed-perft result data — every depth from every studied position, machine-readable. |

## Live site

[grandchesstree.com](https://grandchesstree.com/) is the public face of the
project. Five sections in the nav:

- **Distributed Perft** — project background + the three studied positions
  (Startpos, Kiwipete, SJE) with their depth-by-depth result tables.
- **Tools** — interactive single-page tools (see below).
- **Move generator** — the published article series.
- **Discord** / **GitHub** — external.

### Interactive tools

Every tool runs entirely in the browser and supports deep-linking via URL:

- **FEN visualizer** — paste a FEN, render a board, copy a shareable link or
  save the position as a sharp pixel-art PNG.
- **Bitboard inspector** — paste any 64-bit value to see which squares light
  up. Or derive the 15 standard bitboards (per-piece + occupancy) from a FEN.
  Click squares to toggle bits live.
- **Perft tester** — run perft from any FEN with optional per-root divide.
  Compares against reference values for the standard CPW positions. Runs by
  default on a [.NET-compiled WASM AOT build](MoveGen/MoveGen.Wasm/) of the
  reference engine (~80 M nps in-browser); the pure-JS port is the fallback.
- **PGN viewer** — paste a PGN, step through ply-by-ply with the position
  rendered at each move, click any move to jump.
- **`perftcheck`** documentation — landing page for the CLI under
  [`PerftSuite/`](PerftSuite/).

### The move-generation series

A seven-part walkthrough of building a chess move generator from scratch:
bitboard representation, 16-bit move encoding, pseudo-legal generation,
make/unmake, magic bitboards, and proper legal-move generation via the
checkers/pins/king-danger trinity. Each article ends with runnable code that
plugs into the next. Markdown source in
[`site/src/articles/`](site/src/articles/); published at
[grandchesstree.com/move-generator/](https://grandchesstree.com/move-generator/);
canonical reference implementation in [`MoveGen/`](MoveGen/).

## Distributed perft — the headline numbers

Leaf counts from the standard starting position, contributed by volunteered
compute coordinated through the project Discord:

```
| depth | nodes            | captures        | enpassants   | castles       | promotions  | direct_checks  | single_discovered_checks | direct_discovered_checks | double_discovered_check | total_checks   | direct_mates | single_discovered_mates | direct_discoverd_mates | double_discoverd_mates | total_mates  |
|-------|------------------|-----------------|--------------|---------------|-------------|----------------|--------------------------|--------------------------|-------------------------|----------------|--------------|-------------------------|------------------------|------------------------|--------------|
| 0     | 1                | 0               | 0            | 0             | 0           | 0              | 0                        | 0                        | 0                       | 0              | 0            | 0                       | 0                      | 0                      | 0            |
| 1     | 20               | 0               | 0            | 0             | 0           | 0              | 0                        | 0                        | 0                       | 0              | 0            | 0                       | 0                      | 0                      | 0            |
| 2     | 400              | 0               | 0            | 0             | 0           | 0              | 0                        | 0                        | 0                       | 0              | 0            | 0                       | 0                      | 0                      | 0            |
| 3     | 8902             | 34              | 0            | 0             | 0           | 12             | 0                        | 0                        | 0                       | 12             | 0            | 0                       | 0                      | 0                      | 0            |
| 4     | 197281           | 1576            | 0            | 0             | 0           | 461            | 0                        | 0                        | 0                       | 461            | 8            | 0                       | 0                      | 0                      | 8            |
| 5     | 4865609          | 82719           | 258          | 0             | 0           | 26998          | 6                        | 0                        | 0                       | 27004          | 347          | 0                       | 0                      | 0                      | 347          |
| 6     | 119060324        | 2812008         | 5248         | 0             | 0           | 797896         | 329                      | 46                       | 0                       | 798271         | 10828        | 0                       | 0                      | 0                      | 10828        |
| 7     | 3195901860       | 108329926       | 319617       | 883453        | 0           | 32648427       | 18026                    | 1628                     | 0                       | 32668081       | 435767       | 0                       | 0                      | 0                      | 435767       |
| 8     | 84998978956      | 3523740106      | 7187977      | 23605205      | 0           | 958135303      | 847039                   | 147215                   | 0                       | 959129557      | 9852032      | 4                       | 0                      | 0                      | 9852036      |
| 9     | 2439530234167    | 125208536153    | 319496827    | 1784356000    | 17334376    | 35653060996    | 37101713                 | 5547221                  | 10                      | 35695709940    | 399421379    | 1869                    | 768715                 | 0                      | 400191963    |
| 10    | 69352859712417   | 4092784875884   | 7824835694   | 50908510199   | 511374376   | 1077020493859  | 1531274015               | 302900733                | 879                     | 1078854669486  | 8771693969   | 598058                  | 18327128               | 0                      | 8790619155   |
| 11    | 2097651003696806 | 142537161824567 | 313603617408 | 2641343463566 | 49560932860 | 39068470901662 | 67494850305              | 11721852393              | 57443                   | 39147687661803 | 360675926605 | 60344676                | 1553739626             | 0                      | 362290010907 |
```

Full per-position depth data lives in [`results/`](results/) and is browsable
on the studied-position pages
([startpos](https://grandchesstree.com/startpos.html) ·
[kiwipete](https://grandchesstree.com/kiwipete.html) ·
[sje](https://grandchesstree.com/sje.html)).

## Repository map

```
.
├── Distributed-Perft/          the distributed-perft system
│   ├── GrandChessTree.sln
│   ├── GrandChessTree.Api/         REST API (ASP.NET Core)
│   ├── GrandChessTree.Client/      worker client
│   ├── GrandChessTree.Client.Tests/
│   ├── GrandChessTree.Engine/      engine used by the worker
│   ├── GrandChessTree.Shared/      shared types
│   ├── GrandChessTree.Toolkit/     CLI utilities
│   ├── Dockerfile · docker-compose.yml · .dockerignore
│   ├── mkdocs.yml + docs/          documentation → timmoth.github.io/grandchesstree/
│   └── global.json
├── MoveGen/                    the reference C# move generator
│   ├── MoveGen.sln
│   ├── MoveGen.App/                the engine — one .cs file per article concept
│   ├── MoveGen.Tests/              xUnit, one test file per article + Validate
│   ├── MoveGen.Wasm/               browser-WASM build (output → site/dist/wasm/movegen/)
│   └── movegen-engine.sh           PerftSuite-friendly UCI wrapper
├── site/                       the static site (build.js → dist/)
│   ├── README.md
│   ├── build.js                orchestrator: src/ → dist/, inlines header/footer, runs articles publisher
│   ├── src/
│   │   ├── *.html                  page sources (header/footer get inlined at build)
│   │   ├── templates/              shared chrome (header.html, footer.html)
│   │   ├── articles/               move-generation series + publisher
│   │   │   ├── 01-introduction.md … 07-legal-move-generation.md
│   │   │   ├── img/                figure PNGs (rendered by render-board.js)
│   │   │   ├── render-board.js     FEN/bitboard → PNG (vanilla Node + rsvg-convert)
│   │   │   └── build-articles.js   markdown → HTML; writes to site/dist/move-generator/
│   │   └── assets/                 everything else
│   │       ├── js/                     movegen.js, movegen-wasm.js, board-export.js
│   │       ├── img/                    og-card.png, github-mark.svg, pieces/ (12 piece PNGs)
│   │       └── data/                   perft result JSON
│   └── dist/                   built output — what gets deployed
│       ├── *.html                  pages with chrome inlined
│       ├── assets/                 copied verbatim from src/
│       ├── move-generator/         GENERATED by build-articles.js
│       └── wasm/movegen/           GENERATED by MoveGen.Wasm publish (managed independently)
├── PerftSuite/                 CLI: external engines vs the published perft data
├── results/                    raw distributed-perft data
├── LICENSE
└── README.md                   ← this file
```

Each major subdirectory ships its own `README.md` with deeper detail.

## Working on the project

Quickstart per subdirectory:

### `MoveGen/` — the reference engine

```sh
cd MoveGen
dotnet test -c Release                   # 119/119 xUnit tests
dotnet run --project MoveGen.App         # UCI mode (default)
dotnet run --project MoveGen.App -- --demo   # perft demo
```

Targets `net10.0`. Three projects: `MoveGen.App` (the engine), `MoveGen.Tests`
(per-article xUnit suites), and `MoveGen.Wasm` (browser AOT build for the
in-browser perft tester).

### `site/` — the static site

```sh
cd site
node build.js                            # full rebuild (pages + assets + articles)
node build.js --skip-articles            # skip article regeneration (faster iteration)
python3 -m http.server -d dist 8765      # serve locally
```

No npm dependencies; the build is one Node script (`site/build.js`). It
inlines the shared header/footer template, copies assets, and re-runs the
articles publisher. Output is `site/dist/`; deploy it as-is.

### `MoveGen/MoveGen.Wasm/` — the AOT WASM engine

```sh
cd MoveGen/MoveGen.Wasm
./publish-to-webapp.sh                   # AOT publish → site/dist/wasm/movegen/
```

Requires the `wasm-tools` workload (`sudo dotnet workload install wasm-tools`,
~300 MB). First publish is 2–3 min; incremental is ~30 s. The site build
doesn't touch this — rebuild only when you change the C# move generator.

### `Distributed-Perft/` — the perft network

```sh
cd Distributed-Perft
dotnet build GrandChessTree.sln
docker compose up                        # API + Postgres for local dev
mkdocs serve                             # documentation
```

See [`Distributed-Perft/docs/`](Distributed-Perft/docs/) and
[timmoth.github.io/grandchesstree/](https://timmoth.github.io/grandchesstree/).

### `PerftSuite/` — the compliance CLI

```sh
cd PerftSuite
dotnet build                             # debug
./build/publish-all.sh                   # self-contained binaries for all platforms
```

Pre-built downloads on
[GitHub Releases](https://github.com/Timmoth/grandchesstree/releases/latest).
Usage:

```sh
perftcheck --engine /path/to/your-engine     # validates against ~28k positions, writes perft-report.json
```

## Get involved

Two contribution paths:

- **Collaborate** — work on the reference move generator, the article series,
  the webapp tools, the distributed-perft network, or the compliance CLI.
- **Compete** — write your own move generator that outpaces the reference
  one. `perftcheck` is set up to validate any engine that speaks the standard
  UCI `go perft N` protocol against the bundled corpus of known-correct
  positions.

[Join the Discord](https://discord.gg/cTu3aeCZVe) for coordination, results
review, and engine-dev chat.

## Find out more

- **[grandchesstree.com](https://grandchesstree.com/)** — the live site
  (results, articles, tools)
- **[timmoth.github.io/grandchesstree/](https://timmoth.github.io/grandchesstree/)**
  — mkdocs project docs
- **[Discord](https://discord.gg/cTu3aeCZVe)** — coordination, results review,
  engine-dev chat
- **[GitHub Releases](https://github.com/Timmoth/grandchesstree/releases/latest)**
  — pre-built `perftcheck` binaries per platform

## License

[MIT](LICENSE).
