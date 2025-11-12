export function getCurrentPath(): string {
  const h = location.hash || "";
  return h.startsWith("#") ? h.slice(1) : "C:\\";
}

export function setCurrentPath(p: string): void {
  location.hash = p;
}

export function onRouteChange(cb: (path: string) => void): void {
  window.addEventListener("hashchange", () => cb(getCurrentPath()));
}
