-- Views and match helpers for DMB-LMS.

create or replace view lms_v_tutor_search as
select
  tp.id as tutor_profile_id,
  tp.location_id,
  tp.user_id,
  u.first_name,
  u.last_name,
  tp.headline,
  tp.bio,
  tp.hourly_rate,
  tp.currency,
  tp.experience_years,
  tp.online_only,
  tp.rating,
  tp.review_count,
  tp.avatar_path,
  coalesce((
    select string_agg(s.name, ', ' order by s.name)
    from lms_tutor_subjects ts
    join lms_subjects s on s.id = ts.subject_id
    where ts.tutor_profile_id = tp.id
  ), '') as subjects
from lms_tutor_profiles tp
join lms_users u on u.id = tp.user_id;

create or replace view lms_v_parent_dashboard as
select
  st.parent_user_id,
  st.location_id,
  count(distinct st.id) as student_count,
  count(distinct e.course_id) as course_count,
  count(*) filter (where b.status = 'accepted' and b.starts_at > now()) as upcoming_lessons,
  count(*) filter (where b.status = 'completed') as completed_lessons
from lms_students st
left join lms_enrollments e on e.student_id = st.id
left join lms_bookings b on b.student_id = st.id
group by st.parent_user_id, st.location_id;

create or replace view lms_v_tutor_dashboard as
select
  tp.id as tutor_profile_id,
  tp.location_id,
  count(*) filter (where b.status = 'requested') as pending_requests,
  count(*) filter (where b.status = 'accepted' and b.starts_at > now()) as upcoming_lessons,
  count(*) filter (where b.status = 'completed') as completed_lessons,
  count(distinct c.id) as course_count
from lms_tutor_profiles tp
left join lms_bookings b on b.tutor_profile_id = tp.id
left join lms_courses c on c.tutor_profile_id = tp.id
group by tp.id, tp.location_id;

create or replace function lms_fn_location_stats(p_location_id uuid)
returns table (
  parent_count int,
  tutor_count int,
  student_count int,
  upcoming_lessons int,
  open_requests int
)
language sql
stable
as $$
  select
    (select count(*)::int from lms_user_locations ul where ul.location_id = p_location_id and ul.role = 'parent'),
    (select count(*)::int from lms_tutor_profiles tp where tp.location_id = p_location_id),
    (select count(*)::int from lms_students s where s.location_id = p_location_id),
    (select count(*)::int from lms_bookings b where b.location_id = p_location_id and b.status = 'accepted' and b.starts_at > now()),
    (select count(*)::int from lms_bookings b where b.location_id = p_location_id and b.status = 'requested');
$$;

create or replace function lms_fn_match_tutors(
  p_location_id uuid,
  p_subject text default null,
  p_grade text default null,
  p_max_rate numeric default null,
  p_online_only boolean default null
)
returns table (
  tutor_profile_id uuid,
  first_name text,
  last_name text,
  headline text,
  bio text,
  hourly_rate numeric,
  currency text,
  experience_years int,
  online_only boolean,
  rating numeric,
  review_count int,
  avatar_path text,
  subjects text,
  score numeric
)
language sql
stable
as $$
  select
    v.tutor_profile_id,
    v.first_name,
    v.last_name,
    v.headline,
    v.bio,
    v.hourly_rate,
    v.currency,
    v.experience_years,
    v.online_only,
    v.rating,
    v.review_count,
    v.avatar_path,
    v.subjects,
    (coalesce(v.rating, 0) * 10 + v.experience_years +
      case when p_subject is null or v.subjects ilike '%' || p_subject || '%' then 20 else 0 end
    ) as score
  from lms_v_tutor_search v
  where v.location_id = p_location_id
    and (p_max_rate is null or v.hourly_rate <= p_max_rate)
    and (p_online_only is null or v.online_only = p_online_only)
    and (p_subject is null or v.subjects ilike '%' || p_subject || '%')
    and (
      p_grade is null
      or exists (
        select 1
        from lms_tutor_subjects ts
        where ts.tutor_profile_id = v.tutor_profile_id
          and (ts.level_min = '' or ts.level_min <= p_grade)
          and (ts.level_max = '' or ts.level_max >= p_grade)
      )
    )
  order by score desc, v.hourly_rate asc;
$$;
