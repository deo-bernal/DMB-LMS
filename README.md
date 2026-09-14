# DMB LMS

Tutoring marketplace + learning management system. New standalone product — not part of dmbportfolio.

- **API:** `DMB.LMS.API` on Render (Docker). Cold start is expected on Hobby.
- **Web:** `DMB.LMS.WEB` static CRA on a **new** Vercel project. Talks to Render only.
- **Database:** existing Supabase project `dmbProject`, all tables prefixed `lms_`.

## Run locally

1. Apply SQL in the Supabase SQL editor, in order:
   - `database/postgres/00_lms_schema.sql`
   - `database/postgres/01_lms_functions.sql`
   - `database/postgres/02_lms_seed.sql`
2. Create a private Storage bucket named `lms` on the same project (avatars / materials / submissions).
3. Copy `DMB.LMS.API/appsettings.json` values into user secrets or env:
   - `ConnectionStrings__LmsDb` = Supabase pooler URI
   - `Jwt__Secret` = a long random string **different** from CRM and dmbportfolio
   - `App__FrontendUrl` = `http://localhost:3000`
   - `Supabase__Url` / `Supabase__ServiceRole` for uploads
4. Start API:

```bash
dotnet run --project DMB.LMS.API
```

5. Start web:

```bash
cd DMB.LMS.WEB
copy .env.example .env
npm install
npm start
```

6. Sign in as:
   - Admin: `deobernal@gmail.com` / `Test@123`
   - Parent: `parent.maria@example.com` / `Test@123`
   - Tutor: `tutor.naveed@example.com` / `Test@123`

## Hosting

See `docs/free-tier.md` and `docs/environment-variables.md`.
