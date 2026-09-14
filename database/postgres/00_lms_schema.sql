-- DMB-LMS schema for existing Supabase project dmbProject.
-- Prefix lms_ so this never collides with dmbportfolio or crm_* tables.
-- Run as postgres/service_role: 00 -> 01 -> 02.

create extension if not exists pgcrypto;

-- ---------- Agency / access ----------
create table if not exists lms_agencies (
  id uuid primary key default gen_random_uuid(),
  name text not null,
  slug text not null unique,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table if not exists lms_locations (
  id uuid primary key default gen_random_uuid(),
  agency_id uuid not null references lms_agencies(id) on delete cascade,
  name text not null,
  timezone text not null default 'Asia/Manila',
  is_active boolean not null default true,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);
create index if not exists lms_locations_agency_idx on lms_locations(agency_id);

create table if not exists lms_users (
  id uuid primary key default gen_random_uuid(),
  agency_id uuid not null references lms_agencies(id) on delete cascade,
  username text not null,
  email text not null,
  first_name text not null default '',
  last_name text not null default '',
  password_hash text not null,
  password_salt text not null,
  contact_no text,
  activated boolean not null default false,
  is_super_admin boolean not null default false,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  unique (agency_id, username),
  unique (agency_id, email)
);

create table if not exists lms_user_locations (
  user_id uuid not null references lms_users(id) on delete cascade,
  location_id uuid not null references lms_locations(id) on delete cascade,
  role text not null check (role in ('owner', 'admin', 'tutor', 'parent')),
  created_at timestamptz not null default now(),
  primary key (user_id, location_id)
);

create table if not exists lms_roles (
  id text primary key,
  name text not null
);

create table if not exists lms_permissions (
  id text primary key,
  name text not null
);

create table if not exists lms_role_permissions (
  role_id text not null references lms_roles(id) on delete cascade,
  permission_id text not null references lms_permissions(id) on delete cascade,
  primary key (role_id, permission_id)
);

create table if not exists lms_account_activation_tokens (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references lms_users(id) on delete cascade,
  token_hash text not null unique,
  expires_at timestamptz not null,
  used_at timestamptz,
  created_at timestamptz not null default now()
);

create table if not exists lms_password_reset_tokens (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references lms_users(id) on delete cascade,
  token_hash text not null unique,
  expires_at timestamptz not null,
  used_at timestamptz,
  created_at timestamptz not null default now()
);

create table if not exists lms_revoked_tokens (
  id uuid primary key default gen_random_uuid(),
  jti text not null unique,
  user_id uuid,
  expires_at timestamptz not null,
  created_at timestamptz not null default now()
);

create table if not exists lms_external_logins (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references lms_users(id) on delete cascade,
  provider text not null,
  provider_user_id text not null,
  created_at timestamptz not null default now(),
  unique (provider, provider_user_id),
  unique (user_id, provider)
);

create table if not exists lms_pending_external_logins (
  id uuid primary key default gen_random_uuid(),
  ticket text not null unique,
  provider text not null,
  provider_user_id text not null,
  first_name text not null default '',
  last_name text not null default '',
  email text,
  phone text,
  client text not null default 'web',
  return_path text,
  code_hash text,
  code_expires_at timestamptz,
  expires_at timestamptz not null,
  created_at timestamptz not null default now()
);

-- ---------- Marketplace ----------
create table if not exists lms_students (
  id uuid primary key default gen_random_uuid(),
  location_id uuid not null references lms_locations(id) on delete cascade,
  parent_user_id uuid not null references lms_users(id) on delete cascade,
  first_name text not null,
  last_name text not null default '',
  grade_level text not null default '',
  notes text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);
create index if not exists lms_students_location_idx on lms_students(location_id, updated_at desc);
create index if not exists lms_students_parent_idx on lms_students(parent_user_id);

create table if not exists lms_subjects (
  id uuid primary key default gen_random_uuid(),
  location_id uuid not null references lms_locations(id) on delete cascade,
  name text not null,
  unique (location_id, name)
);

create table if not exists lms_tutor_profiles (
  id uuid primary key default gen_random_uuid(),
  location_id uuid not null references lms_locations(id) on delete cascade,
  user_id uuid not null references lms_users(id) on delete cascade,
  headline text not null default '',
  bio text not null default '',
  hourly_rate numeric(10,2) not null default 0,
  currency text not null default 'PHP',
  experience_years int not null default 0,
  online_only boolean not null default true,
  rating numeric(3,2) not null default 0,
  review_count int not null default 0,
  avatar_path text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  unique (location_id, user_id)
);

create table if not exists lms_tutor_subjects (
  tutor_profile_id uuid not null references lms_tutor_profiles(id) on delete cascade,
  subject_id uuid not null references lms_subjects(id) on delete cascade,
  level_min text not null default '',
  level_max text not null default '',
  primary key (tutor_profile_id, subject_id)
);

create table if not exists lms_tutor_availability (
  id uuid primary key default gen_random_uuid(),
  tutor_profile_id uuid not null references lms_tutor_profiles(id) on delete cascade,
  weekday int not null check (weekday between 0 and 6),
  start_time time not null,
  end_time time not null
);
create index if not exists lms_tutor_availability_tutor_idx on lms_tutor_availability(tutor_profile_id, weekday);

create table if not exists lms_bookings (
  id uuid primary key default gen_random_uuid(),
  location_id uuid not null references lms_locations(id) on delete cascade,
  parent_user_id uuid not null references lms_users(id) on delete cascade,
  student_id uuid not null references lms_students(id) on delete cascade,
  tutor_profile_id uuid not null references lms_tutor_profiles(id) on delete cascade,
  subject_id uuid references lms_subjects(id) on delete set null,
  starts_at timestamptz not null,
  ends_at timestamptz not null,
  status text not null default 'requested' check (status in ('requested', 'accepted', 'rejected', 'cancelled', 'completed')),
  price numeric(10,2) not null default 0,
  meeting_url text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  unique (tutor_profile_id, starts_at)
);
create index if not exists lms_bookings_location_idx on lms_bookings(location_id, starts_at desc);

create table if not exists lms_lesson_notes (
  id uuid primary key default gen_random_uuid(),
  booking_id uuid not null references lms_bookings(id) on delete cascade,
  author_user_id uuid not null references lms_users(id) on delete cascade,
  body text not null,
  created_at timestamptz not null default now()
);

create table if not exists lms_attendance (
  id uuid primary key default gen_random_uuid(),
  booking_id uuid not null unique references lms_bookings(id) on delete cascade,
  student_id uuid not null references lms_students(id) on delete cascade,
  present boolean not null default true,
  marked_at timestamptz not null default now()
);

-- ---------- LMS core ----------
create table if not exists lms_courses (
  id uuid primary key default gen_random_uuid(),
  location_id uuid not null references lms_locations(id) on delete cascade,
  tutor_profile_id uuid not null references lms_tutor_profiles(id) on delete cascade,
  subject_id uuid references lms_subjects(id) on delete set null,
  title text not null,
  description text not null default '',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);
create index if not exists lms_courses_location_idx on lms_courses(location_id, updated_at desc);

create table if not exists lms_enrollments (
  course_id uuid not null references lms_courses(id) on delete cascade,
  student_id uuid not null references lms_students(id) on delete cascade,
  created_at timestamptz not null default now(),
  primary key (course_id, student_id)
);

create table if not exists lms_materials (
  id uuid primary key default gen_random_uuid(),
  course_id uuid not null references lms_courses(id) on delete cascade,
  title text not null,
  external_url text,
  storage_path text,
  mime text,
  size_bytes bigint,
  created_at timestamptz not null default now()
);

create table if not exists lms_assignments (
  id uuid primary key default gen_random_uuid(),
  course_id uuid not null references lms_courses(id) on delete cascade,
  title text not null,
  instructions text not null default '',
  due_at timestamptz,
  max_score numeric(6,2) not null default 20,
  created_at timestamptz not null default now()
);

create table if not exists lms_submissions (
  id uuid primary key default gen_random_uuid(),
  assignment_id uuid not null references lms_assignments(id) on delete cascade,
  student_id uuid not null references lms_students(id) on delete cascade,
  body_text text,
  storage_path text,
  submitted_at timestamptz not null default now(),
  unique (assignment_id, student_id)
);

create table if not exists lms_grades (
  id uuid primary key default gen_random_uuid(),
  submission_id uuid not null unique references lms_submissions(id) on delete cascade,
  score numeric(6,2) not null,
  feedback text,
  graded_at timestamptz not null default now()
);

create table if not exists lms_progress (
  id uuid primary key default gen_random_uuid(),
  student_id uuid not null references lms_students(id) on delete cascade,
  course_id uuid not null references lms_courses(id) on delete cascade,
  materials_done int not null default 0,
  assignments_graded int not null default 0,
  lessons_attended int not null default 0,
  updated_at timestamptz not null default now(),
  unique (student_id, course_id)
);

-- ---------- Later stubs ----------
create table if not exists lms_payments (
  id uuid primary key default gen_random_uuid(),
  location_id uuid not null references lms_locations(id) on delete cascade,
  booking_id uuid references lms_bookings(id) on delete set null,
  amount numeric(10,2) not null default 0,
  status text not null default 'pending',
  provider text,
  created_at timestamptz not null default now()
);

create table if not exists lms_payouts (
  id uuid primary key default gen_random_uuid(),
  location_id uuid not null references lms_locations(id) on delete cascade,
  tutor_profile_id uuid references lms_tutor_profiles(id) on delete set null,
  amount numeric(10,2) not null default 0,
  status text not null default 'pending',
  created_at timestamptz not null default now()
);

create table if not exists lms_video_sessions (
  id uuid primary key default gen_random_uuid(),
  booking_id uuid references lms_bookings(id) on delete cascade,
  agora_channel text,
  recording_url text,
  created_at timestamptz not null default now()
);

do $$
declare
  t text;
begin
  foreach t in array array[
    'lms_agencies','lms_locations','lms_users','lms_user_locations','lms_roles','lms_permissions','lms_role_permissions',
    'lms_account_activation_tokens','lms_password_reset_tokens','lms_revoked_tokens',
    'lms_external_logins','lms_pending_external_logins',
    'lms_students','lms_subjects','lms_tutor_profiles','lms_tutor_subjects','lms_tutor_availability',
    'lms_bookings','lms_lesson_notes','lms_attendance',
    'lms_courses','lms_enrollments','lms_materials','lms_assignments','lms_submissions','lms_grades','lms_progress',
    'lms_payments','lms_payouts','lms_video_sessions'
  ]
  loop
    execute format('alter table %I enable row level security', t);
    execute format('revoke all on table %I from anon, authenticated', t);
  end loop;
end $$;
