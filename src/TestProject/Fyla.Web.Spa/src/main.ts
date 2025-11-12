import { initApi, list, search, download, upload } from "./api.js";
import type { DiskItem } from "./types.js";
import { getCurrentPath, setCurrentPath, onRouteChange } from "./router.js";

const els = {
    list: document.getElementById("list") as HTMLDivElement,
    upBtn: document.getElementById("upBtn") as HTMLButtonElement,
    uploadBtn: document.getElementById("uploadBtn") as HTMLButtonElement,
    refreshBtn: document.getElementById("refreshBtn") as HTMLButtonElement,
    pathInput: document.getElementById("pathInput") as HTMLInputElement,
    searchInput: document.getElementById("searchInput") as HTMLInputElement,
    searchBtn: document.getElementById("searchBtn") as HTMLButtonElement
};

type ClientConfig = { apiBaseUrl: string; defaultPath: string; };

function joinPath(parent: string, name: string): string {
    const sep = parent.includes("\\") ? "\\" : "/";
    return parent.endsWith(sep) ? parent + name : parent + sep + name;
}

function parentPath(p: string): string {
    const isWin = p.includes("\\");
    const sep = isWin ? "\\" : "/";
    const trimmed = p.endsWith(sep) && p.length > 1 ? p.slice(0, -1) : p;
    const idx = trimmed.lastIndexOf(sep);
    if (idx <= 0) return isWin ? "C:\\" : "/";
    return trimmed.slice(0, idx + 1);
}

function fmtBytes(n: number): string {
    if (n < 1024) return `${n} B`;
    const units = ["KB", "MB", "GB", "TB"];
    let i = -1;
    do { n = n / 1024; i++; } while (n >= 1024 && i < units.length - 1);
    return `${n.toFixed(1)} ${units[i]}`;
}

function counts(items: DiskItem[]): { files: number; dirs: number; size: number } {
    let files = 0, dirs = 0, size = 0;
    for (const it of items) {
        if (it.type === "Directory") dirs++; else { files++; size += (it.size || 0); }
    }
    return { files, dirs, size };
}

function getDirectoryPart(p: string): string {
    if (!p) return "";
    const sep = p.includes("\\") ? "\\" : "/";
    const idx = p.lastIndexOf(sep);
    if (idx <= 0) return p;
    return p.slice(0, idx + 1);
}

function showSpinner(show: boolean): void {
    const el = document.getElementById("spinner");
    if (!el) return;
    el.classList.toggle("hidden", !show);
}

function render(path: string, items: DiskItem[], mode: "list" | "search"): void {
    els.pathInput.value = path;

    const { files, dirs, size } = counts(items);

    const rows = items.map(i => 
    {
        const isDir = i.type === "Directory";
        const full = (i as any).path || joinPath(path, i.name);

        const nameCell = isDir
            ? `<span class="click" data-nav="${full}">${i.name}</span>`
            : `<span class="click" data-dl="${full}">${i.name}</span>`;

        const subPath = mode === "search"
            ? `<div class="subpath">${getDirectoryPart(full)}</div>`
            : "";

        return `<tr>
          <td>${nameCell}${subPath}</td>
          <td>${i.type}</td>
          <td>${isDir ? "" : fmtBytes(i.size)}</td>
          <td>${i.lastModified ?? ""}</td>
        </tr>`;
    }).join("");

    els.list.innerHTML = `
      <h3>${mode === "list" ? "Browsing" : "Search"}: ${path}</h3>
      <table>
        <thead><tr><th>Name</th><th>Type</th><th>Size</th><th>Modified</th></tr></thead>
        <tbody>${rows}</tbody>
        <tfoot><tr><td colspan="4">${dirs} folders • ${files} files • ${fmtBytes(size)} total</td></tr></tfoot>
      </table>
    `;

    els.list.querySelectorAll<HTMLElement>("[data-nav]").forEach(el => {
        el.onclick = () => setCurrentPath(el.dataset.nav!);
    });
    els.list.querySelectorAll<HTMLElement>("[data-dl]").forEach(el => {
        el.onclick = () => download(el.dataset.dl!);
    });
}

async function refresh(): Promise<void> {
    const path = getCurrentPath();
    try {
        const items = await list(path);
        render(path, items, "list");
    } catch (e: any) {
        els.list.innerHTML = `<pre>${e?.message ?? e}</pre>`;
    }
}

async function doSearch(): Promise<void> {
    const path = getCurrentPath();
    const pattern = els.searchInput.value.trim();
    const deep = (document.getElementById("deepCheck") as HTMLInputElement)?.checked ?? false;

    try {
        if (!pattern) {
            await refresh();
        } else {
            const items = await search(path, pattern, deep);
            render(path, items, "search");
        }
    } catch (e: any) {
        els.list.innerHTML = `<pre>${e?.message ?? e}</pre>`;
    }
}

function attachEvents(): void {
    els.upBtn.onclick = () => setCurrentPath(parentPath(getCurrentPath()));

    els.pathInput.onkeydown = (ev) => {
        if (ev.key === "Enter") {
            setCurrentPath(els.pathInput.value.trim());
        }
    };

    //unified search / refresh button
    els.searchBtn.onclick = async () => {
        const path = getCurrentPath();
        const pattern = els.searchInput.value.trim();
        const deep = (document.getElementById("deepCheck") as HTMLInputElement)?.checked ?? false;

        showSpinner(true);
        try {
            if (!pattern) {
                await refresh();
            } else {
                const items = await search(path, pattern, deep);
                render(path, items, "search");
            }
        } catch (e: any) {
            els.list.innerHTML = `<pre>${e?.message ?? e}</pre>`;
        } finally {
            showSpinner(false);
        }
    };


    els.uploadBtn.onclick = async () => {
        const input = document.createElement("input");
        input.type = "file";
        input.onchange = async () => {
            const f = input.files?.[0];
            if (!f) return;
            const dest = joinPath(getCurrentPath(), f.name);
            await upload(dest, f);
            await refresh();
        };
        input.click();
    };

    onRouteChange(() => {
        refresh();
    });
}


async function loadConfig(): Promise<ClientConfig> {
  const r = await fetch("/fyla.config.json", { cache: "no-store" });
  if (!r.ok) { throw new Error("Failed to load fyla.config.json"); }
  return r.json();
}

(async function boot() {
    const cfg = await loadConfig();
    initApi(cfg.apiBaseUrl);

    if (!location.hash || getCurrentPath() === "C:\\") {
        setCurrentPath(cfg.defaultPath);
    }

    attachEvents();
    refresh();
    onRouteChange(() => { refresh(); });
})();

