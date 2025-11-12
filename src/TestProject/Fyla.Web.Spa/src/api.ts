import { DiskItem } from "./types.js";

const base = "/Disk";
let apiBaseUrl = "";

export function getApiBaseUrl() { return apiBaseUrl; }

export function initApi(baseUrl: string): void {
    apiBaseUrl = baseUrl.replace(/\/+$/, "");
}

export async function list(path: string): Promise<DiskItem[]> {
    const res = await fetch(`${apiBaseUrl}${base}/List?path=${encodeURIComponent(path)}`);
    if (!res.ok) {
        throw new Error(`List failed: ${res.status}`);
    }
    return res.json();
}

export async function search(path: string, pattern: string, deep: boolean): Promise<DiskItem[]> {
    const res = await fetch(
        `${apiBaseUrl}${base}/Search?path=${encodeURIComponent(path)}&pattern=${encodeURIComponent(pattern)}&deep=${deep}`
    );
    if (!res.ok) {
        throw new Error(`Search failed: ${res.status}`);
    }
    return res.json();
}

export function download(fullPath: string): void {
    const url = `${apiBaseUrl}${base}/Download?path=${encodeURIComponent(fullPath)}`;
    window.open(url, "_blank");
}

export async function upload(destPath: string, file: File): Promise<void> {
    const form = new FormData();
    form.append("file", file);
    form.append("path", destPath);

    const res = await fetch(`${apiBaseUrl}${base}/Upload`, {
        method: "POST",
        body: form
    });

    if (!res.ok) {
        throw new Error(await res.text());
    }
}

export async function del(path: string): Promise<void> {
    const res = await fetch(`${apiBaseUrl}/Disk/Delete?path=${encodeURIComponent(path)}`, {
        method: "DELETE"
    });
    if (!res.ok) {
        throw new Error(`Delete failed: ${res.status}`);
    }
}

export async function copy(src: string, dest: string): Promise<void> {
    const res = await fetch(`${apiBaseUrl}/Disk/Copy?src=${encodeURIComponent(src)}&dest=${encodeURIComponent(dest)}`, {
        method: "POST"
    });
    if (!res.ok) {
        throw new Error(`Copy failed: ${res.status}`);
    }
}

export async function move(src: string, dest: string): Promise<void> {
    const res = await fetch(`${apiBaseUrl}/Disk/Move?src=${encodeURIComponent(src)}&dest=${encodeURIComponent(dest)}`, {
        method: "POST"
    });
    if (!res.ok) {
        throw new Error(`Move failed: ${res.status}`);
    }
}

