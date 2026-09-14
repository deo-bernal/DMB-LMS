# Environment variables

## API (Render / local)

| Name | Purpose |
| --- | --- |
| `ConnectionStrings__LmsDb` | Supabase **pooler** URI for `dmbProject` |
| `Jwt__Secret` | Long random secret. **Different** from CRM and dmbportfolio |
| `Jwt__Issuer` / `Jwt__Audience` | Default `dmblms` |
| `App__FrontendUrl` | LMS web origin (activation and reset links) |
| `Cors__Origins__0` | Allowed SPA origin |
| `Smtp__*` | Activation / reset email. First agency user can register without SMTP. |
| `Supabase__Url` | Project URL for Storage |
| `Supabase__ServiceRole` | Service role for private `lms` bucket. Never put this in the CRA bundle. |

## Web (Vercel / local)

| Name | Purpose |
| --- | --- |
| `REACT_APP_LMS_API_URL` | Public Render API base, including `/api` |
