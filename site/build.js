#!/usr/bin/env node
/*
 * Build the static site at site/dist/ from site/src/.
 *
 * What it does
 *   1. Cleans dist/ — but preserves dist/wasm/movegen/ (managed by
 *      ../MoveGen/MoveGen.Wasm/publish-to-webapp.sh, which is too slow to
 *      run on every site rebuild).
 *   2. For each top-level src/*.html: replaces the existing <header>...
 *      </header> and <footer>...</footer> blocks with the shared templates
 *      in src/templates/, with the active nav link picked up from the
 *      current page's `class="text-slate-900"` marker.
 *   3. Copies assets/, robots.txt, sitemap.xml.
 *   4. Runs articles/build-articles.js so the published article HTML lands
 *      under dist/move-generator/.
 *
 * Usage
 *   node site/build.js
 *   node site/build.js --skip-articles    # faster iteration when working on shell pages
 *
 * Idempotent. Safe to re-run.
 */
"use strict";

const fs = require("fs");
const path = require("path");
const { spawnSync } = require("child_process");

const HERE = __dirname;
const SRC = path.join(HERE, "src");
const DIST = path.join(HERE, "dist");
const TPL_DIR = path.join(SRC, "templates");

const args = process.argv.slice(2);
const SKIP_ARTICLES = args.includes("--skip-articles");

// ---------- I/O helpers ----------
const read = (p) => fs.readFileSync(p, "utf8");
const write = (p, c) => { fs.mkdirSync(path.dirname(p), { recursive: true }); fs.writeFileSync(p, c); };
const exists = (p) => fs.existsSync(p);

function copyDir(srcDir, destDir) {
  fs.mkdirSync(destDir, { recursive: true });
  for (const entry of fs.readdirSync(srcDir, { withFileTypes: true })) {
    const s = path.join(srcDir, entry.name);
    const d = path.join(destDir, entry.name);
    if (entry.isDirectory()) copyDir(s, d);
    else fs.copyFileSync(s, d);
  }
}
function removeDir(p) { if (exists(p)) fs.rmSync(p, { recursive: true, force: true }); }
function removeIfFile(p) { if (exists(p) && fs.statSync(p).isFile()) fs.unlinkSync(p); }

// ---------- Templates ----------
const headerTpl = read(path.join(TPL_DIR, "header.html"));
const footerTpl = read(path.join(TPL_DIR, "footer.html"));

// Sections the nav highlights. Add here when a new top-level section is added.
const NAV_SECTIONS = ["distributed-perft", "tools", "move-generator", "leaderboard"];

function navClass(active, section) {
  return active === section ? "text-slate-900" : "hover:text-slate-900";
}

function tplVars({ active = null, base = "" }) {
  const vars = { BASE: base };
  for (const s of NAV_SECTIONS) {
    vars[`NAV_${s.toUpperCase().replace(/-/g, "_")}`] = navClass(active, s);
  }
  return vars;
}

function interpolate(tpl, vars) {
  return tpl.replace(/\{\{(\w+)\}\}/g, (m, key) => (key in vars ? vars[key] : m));
}

function renderHeader(opts) { return interpolate(headerTpl, tplVars(opts)); }
function renderFooter(opts) { return interpolate(footerTpl, tplVars(opts)); }

// ---------- Page transform ----------
// Detect which nav link is currently active in a page by looking for
// `class="text-slate-900">` on an <a> inside the <header>.
function detectActiveSection(html) {
  const headerMatch = html.match(/<header [\s\S]*?<\/header>/);
  if (!headerMatch) return null;
  const header = headerMatch[0];
  // Look for hrefs of known sections with the active class.
  if (/href="(?:\.\.\/)?distributed-perft\.html"[^>]*class="text-slate-900"/.test(header)) return "distributed-perft";
  if (/href="(?:\.\.\/)?tools\.html"[^>]*class="text-slate-900"/.test(header)) return "tools";
  if (/href="(?:\.\.\/)?move-generator\/?"[^>]*class="text-slate-900"/.test(header)) return "move-generator";
  if (/href="(?:\.\.\/)?leaderboard\.html"[^>]*class="text-slate-900"/.test(header)) return "leaderboard";
  return null;
}

function transformPage(html, { base = "" } = {}) {
  const active = detectActiveSection(html);
  const newHeader = renderHeader({ active, base });
  const newFooter = renderFooter({ base });
  return html
    .replace(/    <header [\s\S]*?<\/header>\n/, newHeader)
    .replace(/    <footer [\s\S]*?<\/footer>\n/, newFooter);
}

function buildPages() {
  for (const entry of fs.readdirSync(SRC)) {
    if (!entry.endsWith(".html")) continue;
    const srcPath = path.join(SRC, entry);
    const html = transformPage(read(srcPath), { base: "" });
    const destPath = path.join(DIST, entry);
    write(destPath, html);
    console.log(`  page    ${entry}`);
  }
}

// ---------- Static asset copy ----------
const ASSET_DIRS = ["assets"];
const ASSET_FILES = ["robots.txt", "sitemap.xml"];

function copyAssets() {
  for (const d of ASSET_DIRS) {
    const s = path.join(SRC, d);
    if (!exists(s)) continue;
    const dst = path.join(DIST, d);
    removeDir(dst);
    copyDir(s, dst);
    console.log(`  copy    ${d}/`);
  }
  for (const f of ASSET_FILES) {
    const s = path.join(SRC, f);
    if (exists(s)) {
      fs.mkdirSync(DIST, { recursive: true });
      fs.copyFileSync(s, path.join(DIST, f));
      console.log(`  copy    ${f}`);
    }
  }
}

// ---------- Articles ----------
function buildArticles() {
  const script = path.join(SRC, "articles", "build-articles.js");
  if (!exists(script)) {
    console.warn("  [skip] src/articles/build-articles.js not found");
    return;
  }
  const r = spawnSync("node", [script], {
    stdio: "inherit",
    env: {
      ...process.env,
      // The articles publisher picks up these paths and writes accordingly.
      MG_OUT_DIR: path.join(DIST, "move-generator"),
      MG_TEMPLATES_DIR: TPL_DIR,
    },
  });
  if (r.status !== 0) throw new Error(`build-articles.js exited ${r.status}`);
}

// ---------- Clean (preserves dist/wasm/movegen/) ----------
function cleanDist() {
  if (!exists(DIST)) return;
  for (const entry of fs.readdirSync(DIST)) {
    if (entry === "wasm") continue; // managed by MoveGen.Wasm/publish-to-webapp.sh
    const p = path.join(DIST, entry);
    if (fs.statSync(p).isDirectory()) removeDir(p);
    else removeIfFile(p);
  }
}

// ---------- Main ----------
function main() {
  console.log(`[site/build] ${SRC} → ${DIST}`);
  fs.mkdirSync(DIST, { recursive: true });
  cleanDist();
  buildPages();
  copyAssets();
  if (SKIP_ARTICLES) {
    console.log("  [skip] --skip-articles");
  } else {
    buildArticles();
  }
  console.log("\n[site/build] done. dist/:");
  for (const f of fs.readdirSync(DIST).sort()) {
    const p = path.join(DIST, f);
    console.log("  " + f + (fs.statSync(p).isDirectory() ? "/" : ""));
  }
}

main();
