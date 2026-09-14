type Listener = (pending: number) => void;
let pending = 0;
const listeners = new Set<Listener>();
function emit() { listeners.forEach((l) => l(pending)); }
export function beginLoading() { pending += 1; emit(); }
export function endLoading() { pending = Math.max(0, pending - 1); emit(); }
export function subscribeLoading(listener: Listener) {
  listeners.add(listener);
  listener(pending);
  return () => { listeners.delete(listener); };
}
export function tracksPageLoading(url?: string) {
  return !/\/auth\/(login|logout|external|me)\b/i.test(String(url ?? ""));
}
