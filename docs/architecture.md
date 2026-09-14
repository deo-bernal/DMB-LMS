# Architecture

DMB LMS copies **CRM folder layers**, not the CRM domain.

```
DMB.LMS.WEB (CRA static)  →  DMB.LMS.API (.NET)  →  Supabase dmbProject (lms_* tables)
```

## Layers

| Project | Role |
| --- | --- |
| `DMB.LMS.API` | Thin controllers. `[Authorize]`, inject `I*Service` only (workspace + auth). |
| `DMB.LMS.Service` | Auth, LMS facade, email, Supabase Storage uploads. |
| `DMB.LMS.DATA` | EF Core `LmsContext` (Npgsql), entities, repositories, AutoMapper. |
| `DMB.LMS.MODEL` | DTOs, `Roles.cs`. |
| `DMB.LMS.WEB` | `src/lms/` pages, JWT context, navy/gold chrome. |
| `DMB.LMS.Test` | Unit tests. |

## Auth

- Login identity lives in `lms_users` (HMAC-SHA512 hash + salt).
- JWT issuer/audience `dmblms`.
- Location header `X-Location-Id`. Roles: `owner`, `admin`, `tutor`, `parent`.
- Students are child profiles, not logins.

Do not reuse dmbportfolio `User` / `leads` or `crm_*` tables.
