# Schema (`lms_` on dmbProject)

Apply `00_lms_schema.sql`, then `01_lms_functions.sql`, then `02_lms_seed.sql`.

Access: `lms_agencies`, `lms_locations`, `lms_users`, `lms_user_locations`, roles/permissions, tokens.

Marketplace: `lms_students`, `lms_subjects`, `lms_tutor_profiles`, `lms_tutor_subjects`, `lms_tutor_availability`, `lms_bookings`, `lms_lesson_notes`, `lms_attendance`.

LMS: `lms_courses`, `lms_enrollments`, `lms_materials`, `lms_assignments`, `lms_submissions`, `lms_grades`, `lms_progress`.

Stubs: `lms_payments`, `lms_payouts`, `lms_video_sessions`.

RLS is enabled. `anon` / `authenticated` are revoked. The Render API uses the database password.
