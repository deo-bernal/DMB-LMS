# Free-tier hosting

| Host | Limit that matters | LMS choice |
| --- | --- | --- |
| Supabase Free | 2 **active** projects | Reuse `dmbProject`. `lms_` tables + private Storage bucket `lms`. Do not create a third project. |
| Vercel Hobby | 12 serverless functions **per project** | **New Vercel project**, root `DMB.LMS.WEB`. Static CRA + SPA rewrite. API base = Render URL. |
| Render Hobby | 750 free instance-hours / month **per workspace** | New Docker web service from `DMB.LMS.API/Dockerfile`. Shared with portfolio, n8n, CRM. **No Render keep-alive cron.** |

## cron-job.org keep-alive

After the API is live, create a free job at [cron-job.org](https://cron-job.org):

- URL: `https://dmb-lms-api.onrender.com/health`
- Method: GET
- Interval: every 10–14 minutes

This reduces cold starts. It still counts against the shared 750 Hobby hours. Pause the job if hours run low.

## Vercel deploy

New project (not the portfolio or CRM). Root Directory: `DMB.LMS.WEB`. Build: `npm run build`. Output: `build`. Env: `REACT_APP_LMS_API_URL`.

## Render deploy

New Web Service, Docker, Dockerfile path `DMB.LMS.API/Dockerfile`, context the `DMB-LMS` repo root.
