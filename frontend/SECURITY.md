# Security Notes — CoparentHub Frontend

## Session storage

- The JWT is kept in `sessionStorage`, not `localStorage` or a cookie, so it is
  automatically cleared when the tab/browser closes and is never sent to any server other
  than the API this app is configured to call. It is still readable by any script running
  on the page, so the usual SPA/XSS caveat applies: keep third-party scripts to a minimum
  and review dependencies before adding new ones.
- On every request, `api/http.ts` clears the token and signs the user out automatically if
  the server returns `401` (expired/invalid/revoked token), and also proactively signs the
  user out client-side when the token's own `exp` claim elapses — so the app never sits in
  a "looks logged in but every call fails" state.
- A small, non-sensitive display profile (name/email) is cached alongside the token purely
  so the UI can show your name after a page refresh without waiting on a `/me` endpoint
  (the backend doesn't expose one). It is cleared on logout along with the token.

## Input hardening

- Client-side `maxLength`/`minLength` on form fields mirror the backend's FluentValidation
  rules (e.g. 200 chars for event titles, 1000 for notes, 8+ chars for passwords) as
  defense-in-depth — the backend remains the source of truth and re-validates everything.
- Registration requires explicit acknowledgement of a Privacy Policy / Terms checkbox
  before the form can submit, since this app stores information about the user's children.

## Transport

- All API calls go through a single `request()` helper (`api/http.ts`) that always sends
  `Content-Type: application/json`, attaches the bearer token when present, and enforces a
  15s timeout so a hung request can't leave the UI stuck indefinitely.
- `VITE_API_BASE_URL` controls which origin the app talks to. Set it explicitly for every
  deployment — do not rely on the `https://localhost:44327` development default.

## Production hardening checklist (hosting-level)

This is a static SPA; the following should be configured at your hosting/CDN/reverse-proxy
layer (e.g. Nginx, Cloudflare, Netlify, Azure Static Web Apps) since a `<meta>` CSP tag in
`index.html` cannot safely coexist with Vite's dev-mode HMR client:

- [ ] `Content-Security-Policy` restricting `connect-src` to your real API origin,
      `script-src 'self'`, `style-src 'self' 'unsafe-inline' https://fonts.googleapis.com`
      (inline styles are required by the current theming approach), `frame-ancestors 'none'`.
- [ ] `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`
      — mirroring the headers the backend already sends on `/api/*`.
- [ ] Serve over HTTPS only, with HSTS enabled.
- [ ] Keep dependencies patched (`npm audit`); this project intentionally has a very small
      dependency surface (React + Vite tooling only — no HTTP client library, no router)
      to minimize supply-chain exposure.
