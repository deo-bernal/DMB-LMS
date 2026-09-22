-- Demo agency, location, roles, admin Deo, parents, tutors, students, lessons, LMS rows.
-- Super admin password is HMAC-SHA512 hashed (same as AuthRepository). Plaintext: Test@123

insert into lms_agencies (id, name, slug)
values ('11111111-1111-1111-1111-111111111111', 'DMB Web Solutions', 'dmb')
on conflict (id) do nothing;

insert into lms_locations (id, agency_id, name, timezone)
values (
  '22222222-2222-2222-2222-222222222222',
  '11111111-1111-1111-1111-111111111111',
  'DMB Demo Location',
  'Asia/Manila'
)
on conflict (id) do nothing;

insert into lms_roles (id, name) values
  ('owner', 'Owner'),
  ('admin', 'Admin'),
  ('tutor', 'Tutor'),
  ('parent', 'Parent')
on conflict (id) do nothing;

insert into lms_permissions (id, name) values
  ('students.read', 'Read students'),
  ('students.write', 'Write students'),
  ('bookings.read', 'Read bookings'),
  ('bookings.write', 'Write bookings'),
  ('courses.read', 'Read courses'),
  ('courses.write', 'Write courses')
on conflict (id) do nothing;

insert into lms_role_permissions (role_id, permission_id)
select r.id, p.id
from lms_roles r
cross join lms_permissions p
where r.id in ('owner', 'admin')
on conflict do nothing;

insert into lms_role_permissions (role_id, permission_id)
select 'tutor', p.id
from lms_permissions p
where p.id in ('bookings.read', 'bookings.write', 'courses.read', 'courses.write', 'students.read')
on conflict do nothing;

insert into lms_role_permissions (role_id, permission_id)
select 'parent', p.id
from lms_permissions p
where p.id in ('students.read', 'students.write', 'bookings.read', 'bookings.write', 'courses.read')
on conflict do nothing;

-- Same hash/salt as DMB-CRM seed so Test@123 works for every demo user.
insert into lms_users (
  id, agency_id, username, email, first_name, last_name,
  password_hash, password_salt, contact_no, activated, is_super_admin
)
values
(
  '66666666-6666-6666-6666-666666666666',
  '11111111-1111-1111-1111-111111111111',
  'deobernal@gmail.com',
  'deobernal@gmail.com',
  'Deo',
  'Bernal',
  'ykwGEiGTSG2xYqbSsh89GupzhC6Mh2kjpIAykQboufcQeALD8n/Kg1enV2ioFHwrMX6WwHaoPP2rTQPgK1E06Q==',
  '2VokCELYP27oaSXSvmQ8lUBblRDoA+QoyhJHI58DJpb3UVDvqBbo9DnDO7d5BeqYBY7Z0cbXQymK3yDpKC32Kg1g0K2uzxrgeko6jBLXQ2JxPXqT1gwU4Et+6F8CcXgkr5bTjI+xNPlTskcHzuwVIS1J0Mh6unpVnvHZt5Z1T+s=',
  '+63 925 455 6063',
  true,
  true
),
(
  '77777777-7777-7777-7777-777777777771',
  '11111111-1111-1111-1111-111111111111',
  'parent.maria@example.com',
  'parent.maria@example.com',
  'Maria',
  'Santos',
  'ykwGEiGTSG2xYqbSsh89GupzhC6Mh2kjpIAykQboufcQeALD8n/Kg1enV2ioFHwrMX6WwHaoPP2rTQPgK1E06Q==',
  '2VokCELYP27oaSXSvmQ8lUBblRDoA+QoyhJHI58DJpb3UVDvqBbo9DnDO7d5BeqYBY7Z0cbXQymK3yDpKC32Kg1g0K2uzxrgeko6jBLXQ2JxPXqT1gwU4Et+6F8CcXgkr5bTjI+xNPlTskcHzuwVIS1J0Mh6unpVnvHZt5Z1T+s=',
  '+63 917 111 0001',
  true,
  false
),
(
  '77777777-7777-7777-7777-777777777772',
  '11111111-1111-1111-1111-111111111111',
  'parent.james@example.com',
  'parent.james@example.com',
  'James',
  'Rivera',
  'ykwGEiGTSG2xYqbSsh89GupzhC6Mh2kjpIAykQboufcQeALD8n/Kg1enV2ioFHwrMX6WwHaoPP2rTQPgK1E06Q==',
  '2VokCELYP27oaSXSvmQ8lUBblRDoA+QoyhJHI58DJpb3UVDvqBbo9DnDO7d5BeqYBY7Z0cbXQymK3yDpKC32Kg1g0K2uzxrgeko6jBLXQ2JxPXqT1gwU4Et+6F8CcXgkr5bTjI+xNPlTskcHzuwVIS1J0Mh6unpVnvHZt5Z1T+s=',
  '+63 917 111 0002',
  true,
  false
),
(
  '88888888-8888-8888-8888-888888888881',
  '11111111-1111-1111-1111-111111111111',
  'tutor.naveed@example.com',
  'tutor.naveed@example.com',
  'Naveed',
  'Khan',
  'ykwGEiGTSG2xYqbSsh89GupzhC6Mh2kjpIAykQboufcQeALD8n/Kg1enV2ioFHwrMX6WwHaoPP2rTQPgK1E06Q==',
  '2VokCELYP27oaSXSvmQ8lUBblRDoA+QoyhJHI58DJpb3UVDvqBbo9DnDO7d5BeqYBY7Z0cbXQymK3yDpKC32Kg1g0K2uzxrgeko6jBLXQ2JxPXqT1gwU4Et+6F8CcXgkr5bTjI+xNPlTskcHzuwVIS1J0Mh6unpVnvHZt5Z1T+s=',
  '+63 917 222 0001',
  true,
  false
),
(
  '88888888-8888-8888-8888-888888888882',
  '11111111-1111-1111-1111-111111111111',
  'tutor.priya@example.com',
  'tutor.priya@example.com',
  'Priya',
  'Sreekumar',
  'ykwGEiGTSG2xYqbSsh89GupzhC6Mh2kjpIAykQboufcQeALD8n/Kg1enV2ioFHwrMX6WwHaoPP2rTQPgK1E06Q==',
  '2VokCELYP27oaSXSvmQ8lUBblRDoA+QoyhJHI58DJpb3UVDvqBbo9DnDO7d5BeqYBY7Z0cbXQymK3yDpKC32Kg1g0K2uzxrgeko6jBLXQ2JxPXqT1gwU4Et+6F8CcXgkr5bTjI+xNPlTskcHzuwVIS1J0Mh6unpVnvHZt5Z1T+s=',
  '+63 917 222 0002',
  true,
  false
),
(
  '88888888-8888-8888-8888-888888888883',
  '11111111-1111-1111-1111-111111111111',
  'tutor.kathryn@example.com',
  'tutor.kathryn@example.com',
  'Kathryn',
  'Murphy',
  'ykwGEiGTSG2xYqbSsh89GupzhC6Mh2kjpIAykQboufcQeALD8n/Kg1enV2ioFHwrMX6WwHaoPP2rTQPgK1E06Q==',
  '2VokCELYP27oaSXSvmQ8lUBblRDoA+QoyhJHI58DJpb3UVDvqBbo9DnDO7d5BeqYBY7Z0cbXQymK3yDpKC32Kg1g0K2uzxrgeko6jBLXQ2JxPXqT1gwU4Et+6F8CcXgkr5bTjI+xNPlTskcHzuwVIS1J0Mh6unpVnvHZt5Z1T+s=',
  '+63 917 222 0003',
  true,
  false
)
on conflict (id) do update set
  username = excluded.username,
  email = excluded.email,
  first_name = excluded.first_name,
  last_name = excluded.last_name,
  password_hash = excluded.password_hash,
  password_salt = excluded.password_salt,
  activated = true,
  updated_at = now();

insert into lms_user_locations (user_id, location_id, role) values
  ('66666666-6666-6666-6666-666666666666', '22222222-2222-2222-2222-222222222222', 'owner'),
  ('77777777-7777-7777-7777-777777777771', '22222222-2222-2222-2222-222222222222', 'parent'),
  ('77777777-7777-7777-7777-777777777772', '22222222-2222-2222-2222-222222222222', 'parent'),
  ('88888888-8888-8888-8888-888888888881', '22222222-2222-2222-2222-222222222222', 'tutor'),
  ('88888888-8888-8888-8888-888888888882', '22222222-2222-2222-2222-222222222222', 'tutor'),
  ('88888888-8888-8888-8888-888888888883', '22222222-2222-2222-2222-222222222222', 'tutor')
on conflict (user_id, location_id) do update set role = excluded.role;

-- Google SSO may create deo_bernal@yahoo.com as a parent with no demo bookings.
-- Promote known Deo accounts to owner/super-admin so Lessons shows the full demo set.
update lms_users
set is_super_admin = true, updated_at = now()
where lower(email) in ('deobernal@gmail.com', 'deo_bernal@yahoo.com');

update lms_user_locations ul
set role = 'owner'
from lms_users u
where u.id = ul.user_id
  and ul.location_id = '22222222-2222-2222-2222-222222222222'
  and lower(u.email) in ('deobernal@gmail.com', 'deo_bernal@yahoo.com');

insert into lms_students (id, location_id, parent_user_id, first_name, last_name, grade_level, notes) values
  ('99999999-9999-9999-9999-999999999991', '22222222-2222-2222-2222-222222222222', '77777777-7777-7777-7777-777777777771', 'Sofia', 'Santos', 'Grade 5', 'Loves math puzzles'),
  ('99999999-9999-9999-9999-999999999992', '22222222-2222-2222-2222-222222222222', '77777777-7777-7777-7777-777777777771', 'Lucas', 'Santos', 'Grade 3', 'Building reading stamina'),
  ('99999999-9999-9999-9999-999999999993', '22222222-2222-2222-2222-222222222222', '77777777-7777-7777-7777-777777777772', 'Mia', 'Rivera', 'Grade 8', 'Geometry-focused')
on conflict (id) do update set
  first_name = excluded.first_name,
  last_name = excluded.last_name,
  grade_level = excluded.grade_level,
  notes = excluded.notes,
  updated_at = now();

insert into lms_subjects (id, location_id, name) values
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '22222222-2222-2222-2222-222222222222', 'Mathematics'),
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '22222222-2222-2222-2222-222222222222', 'English'),
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', '22222222-2222-2222-2222-222222222222', 'Geometry'),
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', '22222222-2222-2222-2222-222222222222', 'Computer Science')
on conflict (id) do nothing;

insert into lms_tutor_profiles (
  id, location_id, user_id, headline, bio, hourly_rate, currency, experience_years, online_only, rating, review_count
) values
(
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1',
  '22222222-2222-2222-2222-222222222222',
  '88888888-8888-8888-8888-888888888881',
  'Math and CS, online only',
  'I help Grade 4–10 students build confidence in math and intro programming.',
  25, 'USD', 6, true, 4.80, 18
),
(
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2',
  '22222222-2222-2222-2222-222222222222',
  '88888888-8888-8888-8888-888888888882',
  'Patient Grade 3–10 mathematics',
  'Six years teaching elementary and junior high math, including English support.',
  30, 'USD', 6, true, 4.70, 12
),
(
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3',
  '22222222-2222-2222-2222-222222222222',
  '88888888-8888-8888-8888-888888888883',
  'Geometry and middle-school math',
  'Clear explanations for proofs, triangles, and exam prep.',
  15, 'USD', 5, true, 4.90, 29
)
on conflict (id) do update set
  headline = excluded.headline,
  bio = excluded.bio,
  hourly_rate = excluded.hourly_rate,
  rating = excluded.rating,
  updated_at = now();

insert into lms_tutor_subjects (tutor_profile_id, subject_id, level_min, level_max) values
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Grade 4', 'Grade 10'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 'Grade 6', 'Grade 12'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Grade 5', 'Grade 10'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Grade 3', 'Grade 8'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Grade 6', 'Grade 10'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'Grade 7', 'Grade 10')
on conflict do nothing;

delete from lms_tutor_availability where tutor_profile_id in (
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3'
);
insert into lms_tutor_availability (tutor_profile_id, weekday, start_time, end_time) values
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', 1, '18:00', '20:00'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', 3, '18:00', '20:00'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', 6, '09:00', '12:00'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', 2, '17:00', '19:00'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', 6, '10:00', '12:00'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3', 4, '18:00', '20:00'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3', 6, '09:00', '11:00');

insert into lms_courses (id, location_id, tutor_profile_id, subject_id, title, description) values
(
  'cccccccc-cccc-cccc-cccc-ccccccccccc1',
  '22222222-2222-2222-2222-222222222222',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
  'Grade 5 Mathematics',
  'Fractions, decimals, and word problems.'
),
(
  'cccccccc-cccc-cccc-cccc-ccccccccccc2',
  '22222222-2222-2222-2222-222222222222',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
  'Grade 8 Geometry',
  'Triangles, proofs, and Pythagoras.'
),
(
  'cccccccc-cccc-cccc-cccc-ccccccccccc3',
  '22222222-2222-2222-2222-222222222222',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
  'Grade 3 English',
  'Reading fluency and short writing.'
)
on conflict (id) do update set title = excluded.title, description = excluded.description, updated_at = now();

insert into lms_enrollments (course_id, student_id) values
  ('cccccccc-cccc-cccc-cccc-ccccccccccc1', '99999999-9999-9999-9999-999999999991'),
  ('cccccccc-cccc-cccc-cccc-ccccccccccc2', '99999999-9999-9999-9999-999999999993'),
  ('cccccccc-cccc-cccc-cccc-ccccccccccc3', '99999999-9999-9999-9999-999999999992')
on conflict do nothing;

insert into lms_materials (id, course_id, title, external_url) values
  ('dddddddd-dddd-dddd-dddd-ddddddddddd1', 'cccccccc-cccc-cccc-cccc-ccccccccccc1', 'Equivalent fractions explainer', 'https://www.khanacademy.org/math/cc-fourth-grade-math/imp-fractions-2'),
  ('dddddddd-dddd-dddd-dddd-ddddddddddd2', 'cccccccc-cccc-cccc-cccc-ccccccccccc2', 'Pythagorean theorem intro', 'https://www.khanacademy.org/math/geometry/hs-geo-trig/hs-geo-pythagorean-theorem/a/pythagorean-theorem'),
  ('dddddddd-dddd-dddd-dddd-ddddddddddd3', 'cccccccc-cccc-cccc-cccc-ccccccccccc3', 'Grade 3 reading practice', 'https://www.khanacademy.org/ela/cc-3rd-reading-vocab')
on conflict (id) do update set title = excluded.title, external_url = excluded.external_url;

insert into lms_assignments (id, course_id, title, instructions, due_at, max_score) values
  ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1', 'cccccccc-cccc-cccc-cccc-ccccccccccc1', 'Fractions worksheet', 'Complete page 12. Show your work.', now() + interval '5 days', 20),
  ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2', 'cccccccc-cccc-cccc-cccc-ccccccccccc2', 'Triangle proofs', 'Prove two right-triangle identities from class.', now() + interval '6 days', 20),
  ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3', 'cccccccc-cccc-cccc-cccc-ccccccccccc3', 'Reading log', 'Read 15 minutes a day and write three sentences.', now() + interval '7 days', 20)
on conflict (id) do update set title = excluded.title, instructions = excluded.instructions;

insert into lms_submissions (id, assignment_id, student_id, body_text, submitted_at) values
  ('ffffffff-ffff-ffff-ffff-fffffffffff1', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1', '99999999-9999-9999-9999-999999999991', 'Worksheet photos uploaded in class. All 10 items done.', now() - interval '2 days'),
  ('ffffffff-ffff-ffff-ffff-fffffffffff2', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2', '99999999-9999-9999-9999-999999999993', 'Proofs 1–2 attached as text notes.', now() - interval '1 day')
on conflict (id) do update set body_text = excluded.body_text;

insert into lms_grades (id, submission_id, score, feedback, graded_at) values
  ('12121212-1212-1212-1212-121212121211', 'ffffffff-ffff-ffff-ffff-fffffffffff1', 18, 'Strong work on equivalent fractions. Watch item 8.', now() - interval '1 day'),
  ('12121212-1212-1212-1212-121212121212', 'ffffffff-ffff-ffff-ffff-fffffffffff2', 16, 'Solid start. Tighten the reason on step 3.', now() - interval '12 hours')
on conflict (id) do update set score = excluded.score, feedback = excluded.feedback;

insert into lms_bookings (
  id, location_id, parent_user_id, student_id, tutor_profile_id, subject_id,
  starts_at, ends_at, status, price, meeting_url
) values
(
  '13131313-1313-1313-1313-131313131311',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777771',
  '99999999-9999-9999-9999-999999999991',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
  now() - interval '7 days',
  now() - interval '7 days' + interval '60 minutes',
  'completed',
  25,
  'https://meet.google.com/dmb-sofia-math'
),
(
  '13131313-1313-1313-1313-131313131312',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777772',
  '99999999-9999-9999-9999-999999999993',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
  now() - interval '6 days',
  now() - interval '6 days' + interval '60 minutes',
  'completed',
  15,
  'https://meet.google.com/dmb-mia-geo'
),
(
  '13131313-1313-1313-1313-131313131313',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777771',
  '99999999-9999-9999-9999-999999999991',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
  now() + interval '1 day' + interval '10 hours',
  now() + interval '1 day' + interval '11 hours',
  'accepted',
  25,
  'https://meet.google.com/dmb-sofia-next'
),
(
  '13131313-1313-1313-1313-131313131314',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777771',
  '99999999-9999-9999-9999-999999999992',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
  now() + interval '3 days' + interval '10 hours',
  now() + interval '3 days' + interval '11 hours',
  'accepted',
  30,
  'https://meet.google.com/dmb-lucas-eng'
),
(
  '13131313-1313-1313-1313-131313131315',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777772',
  '99999999-9999-9999-9999-999999999993',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
  now() + interval '4 days' + interval '9 hours',
  now() + interval '4 days' + interval '10 hours',
  'requested',
  30,
  null
),
-- Extra demo lessons so Lessons stays populated after seed dates age.
(
  '13131313-1313-1313-1313-131313131316',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777771',
  '99999999-9999-9999-9999-999999999991',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
  now() + interval '2 days' + interval '16 hours',
  now() + interval '2 days' + interval '17 hours',
  'accepted',
  30,
  'https://meet.google.com/dmb-sofia-priya'
),
(
  '13131313-1313-1313-1313-131313131317',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777772',
  '99999999-9999-9999-9999-999999999993',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4',
  now() + interval '5 days' + interval '14 hours',
  now() + interval '5 days' + interval '15 hours',
  'accepted',
  25,
  'https://meet.google.com/dmb-mia-cs'
),
(
  '13131313-1313-1313-1313-131313131318',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777771',
  '99999999-9999-9999-9999-999999999992',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
  now() + interval '6 days' + interval '9 hours',
  now() + interval '6 days' + interval '10 hours',
  'accepted',
  15,
  'https://meet.google.com/dmb-lucas-geo'
),
(
  '13131313-1313-1313-1313-131313131319',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777772',
  '99999999-9999-9999-9999-999999999993',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
  now() + interval '8 days' + interval '11 hours',
  now() + interval '8 days' + interval '12 hours',
  'requested',
  15,
  null
),
(
  '13131313-1313-1313-1313-13131313131a',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777771',
  '99999999-9999-9999-9999-999999999991',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
  now() - interval '2 days',
  now() - interval '2 days' + interval '60 minutes',
  'completed',
  25,
  'https://meet.google.com/dmb-sofia-recent'
),
(
  '13131313-1313-1313-1313-13131313131b',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777771',
  '99999999-9999-9999-9999-999999999992',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
  now() - interval '3 days',
  now() - interval '3 days' + interval '60 minutes',
  'completed',
  30,
  'https://meet.google.com/dmb-lucas-recent'
),
(
  '13131313-1313-1313-1313-13131313131c',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777772',
  '99999999-9999-9999-9999-999999999993',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
  now() + interval '10 days' + interval '15 hours',
  now() + interval '10 days' + interval '16 hours',
  'accepted',
  25,
  'https://meet.google.com/dmb-mia-math-2'
),
(
  '13131313-1313-1313-1313-13131313131d',
  '22222222-2222-2222-2222-222222222222',
  '77777777-7777-7777-7777-777777777771',
  '99999999-9999-9999-9999-999999999991',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
  now() + interval '12 days' + interval '13 hours',
  now() + interval '12 days' + interval '14 hours',
  'accepted',
  15,
  'https://meet.google.com/dmb-sofia-geo'
)
on conflict (id) do update set
  status = excluded.status,
  starts_at = excluded.starts_at,
  ends_at = excluded.ends_at,
  meeting_url = excluded.meeting_url,
  parent_user_id = excluded.parent_user_id,
  student_id = excluded.student_id,
  tutor_profile_id = excluded.tutor_profile_id,
  subject_id = excluded.subject_id,
  price = excluded.price,
  updated_at = now();

insert into lms_lesson_notes (id, booking_id, author_user_id, body) values
  ('14141414-1414-1414-1414-141414141411', '13131313-1313-1313-1313-131313131311', '88888888-8888-8888-8888-888888888881', 'Worked through equivalent fractions; homework: p.12'),
  ('14141414-1414-1414-1414-141414141412', '13131313-1313-1313-1313-131313131312', '88888888-8888-8888-8888-888888888883', 'Introduced Pythagoras; review right triangles'),
  ('14141414-1414-1414-1414-141414141413', '13131313-1313-1313-1313-13131313131a', '88888888-8888-8888-8888-888888888881', 'Reviewed word problems; next session: mixed operations.'),
  ('14141414-1414-1414-1414-141414141414', '13131313-1313-1313-1313-13131313131b', '88888888-8888-8888-8888-888888888882', 'Reading fluency drill; assigned three-sentence journal.')
on conflict (id) do update set body = excluded.body;

insert into lms_attendance (id, booking_id, student_id, present, marked_at) values
  ('15151515-1515-1515-1515-151515151511', '13131313-1313-1313-1313-131313131311', '99999999-9999-9999-9999-999999999991', true, now() - interval '7 days'),
  ('15151515-1515-1515-1515-151515151512', '13131313-1313-1313-1313-131313131312', '99999999-9999-9999-9999-999999999993', true, now() - interval '6 days'),
  ('15151515-1515-1515-1515-151515151513', '13131313-1313-1313-1313-13131313131a', '99999999-9999-9999-9999-999999999991', true, now() - interval '2 days'),
  ('15151515-1515-1515-1515-151515151514', '13131313-1313-1313-1313-13131313131b', '99999999-9999-9999-9999-999999999992', true, now() - interval '3 days')
on conflict (id) do update set present = excluded.present;

-- Extra course + materials + assignment for a fuller demo catalog.
insert into lms_courses (id, location_id, tutor_profile_id, subject_id, title, description) values
(
  'cccccccc-cccc-cccc-cccc-ccccccccccc4',
  '22222222-2222-2222-2222-222222222222',
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1',
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4',
  'Intro to Coding (Scratch → Python)',
  'First programs, loops, and debugging habits for Grade 6–9.'
)
on conflict (id) do update set title = excluded.title, description = excluded.description, updated_at = now();

insert into lms_enrollments (course_id, student_id) values
  ('cccccccc-cccc-cccc-cccc-ccccccccccc4', '99999999-9999-9999-9999-999999999993'),
  ('cccccccc-cccc-cccc-cccc-ccccccccccc1', '99999999-9999-9999-9999-999999999992'),
  ('cccccccc-cccc-cccc-cccc-ccccccccccc4', '99999999-9999-9999-9999-999999999991')
on conflict do nothing;

insert into lms_materials (id, course_id, title, external_url) values
  ('dddddddd-dddd-dddd-dddd-ddddddddddd4', 'cccccccc-cccc-cccc-cccc-ccccccccccc4', 'Scratch getting started', 'https://scratch.mit.edu/ideas'),
  ('dddddddd-dddd-dddd-dddd-ddddddddddd5', 'cccccccc-cccc-cccc-cccc-ccccccccccc1', 'Decimals on the number line', 'https://www.khanacademy.org/math/cc-fifth-grade-math/imp-decimals'),
  ('dddddddd-dddd-dddd-dddd-ddddddddddd6', 'cccccccc-cccc-cccc-cccc-ccccccccccc2', 'Congruent triangles checklist', 'https://www.khanacademy.org/math/geometry')
on conflict (id) do update set title = excluded.title, external_url = excluded.external_url;

insert into lms_assignments (id, course_id, title, instructions, due_at, max_score) values
  ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee4', 'cccccccc-cccc-cccc-cccc-ccccccccccc4', 'Build a Scratch maze', 'Share the project link and write what each sprite does.', now() + interval '9 days', 25),
  ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee5', 'cccccccc-cccc-cccc-cccc-ccccccccccc1', 'Decimal word problems', 'Solve 8 problems; show place-value reasoning.', now() + interval '4 days', 20)
on conflict (id) do update set title = excluded.title, instructions = excluded.instructions, due_at = excluded.due_at;

insert into lms_progress (id, student_id, course_id, materials_done, assignments_graded, lessons_attended) values
  ('16161616-1616-1616-1616-161616161611', '99999999-9999-9999-9999-999999999991', 'cccccccc-cccc-cccc-cccc-ccccccccccc1', 2, 1, 2),
  ('16161616-1616-1616-1616-161616161612', '99999999-9999-9999-9999-999999999993', 'cccccccc-cccc-cccc-cccc-ccccccccccc2', 2, 1, 1),
  ('16161616-1616-1616-1616-161616161613', '99999999-9999-9999-9999-999999999992', 'cccccccc-cccc-cccc-cccc-ccccccccccc3', 1, 0, 1),
  ('16161616-1616-1616-1616-161616161614', '99999999-9999-9999-9999-999999999993', 'cccccccc-cccc-cccc-cccc-ccccccccccc4', 1, 0, 0),
  ('16161616-1616-1616-1616-161616161615', '99999999-9999-9999-9999-999999999991', 'cccccccc-cccc-cccc-cccc-ccccccccccc4', 0, 0, 0)
on conflict (id) do update set
  materials_done = excluded.materials_done,
  assignments_graded = excluded.assignments_graded,
  lessons_attended = excluded.lessons_attended,
  updated_at = now();
