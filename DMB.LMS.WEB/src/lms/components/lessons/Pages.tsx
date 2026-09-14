import { FormEvent, useEffect, useState } from "react";
import http from "../../services/http.service";
import type { Assignment, Availability, Booking, Course, Material, Progress, Student, Subject, TutorCard, TutorProfile, AdminUser } from "../../models";
import { useAuth } from "../../../contexts/JWTAuthContext";

export function LessonsPage() {
  const { locationId, currentRole } = useAuth();
  const [rows, setRows] = useState<Booking[]>([]);
  const [note, setNote] = useState("");
  const load = () => http.get<Booking[]>("/bookings").then((r) => setRows(r.data));
  useEffect(() => { if (locationId) load(); }, [locationId]);

  return (
    <div>
      <h1>Lessons</h1>
      {rows.map((l) => (
        <div className="lesson-row" key={l.id}>
          <div><strong>{l.studentName}</strong> with {l.tutorName} <span className="badge">{l.status}</span></div>
          <div className="muted">{new Date(l.startsAt).toLocaleString()} · {l.subjectName} · ${l.price}</div>
          {l.note ? <p>{l.note}</p> : null}
          {l.present != null ? <div className="muted">Attendance: {l.present ? "Present" : "Absent"}</div> : null}
          {l.meetingUrl ? <a href={l.meetingUrl} target="_blank" rel="noreferrer">Meeting link</a> : null}
          {currentRole === "tutor" && l.status === "requested" ? (
            <div>
              <button type="button" onClick={() => http.post(`/bookings/${l.id}/accepted`).then(load)}>Accept</button>
              <button className="ghost" type="button" onClick={() => http.post(`/bookings/${l.id}/rejected`).then(load)}>Reject</button>
            </div>
          ) : null}
          {currentRole === "tutor" && l.status === "accepted" ? (
            <form onSubmit={(e) => { e.preventDefault(); http.post(`/bookings/${l.id}/complete`, { present: true, note }).then(() => { setNote(""); load(); }); }}>
              <input placeholder="Lesson note" value={note} onChange={(e) => setNote(e.target.value)} />
              <button type="submit">Complete + mark present</button>
            </form>
          ) : null}
          {currentRole === "parent" && l.status === "requested" ? (
            <button className="ghost" type="button" onClick={() => http.post(`/bookings/${l.id}/cancelled`).then(load)}>Cancel</button>
          ) : null}
        </div>
      ))}
    </div>
  );
}

export function SchedulePage() {
  const { locationId } = useAuth();
  const [students, setStudents] = useState<Student[]>([]);
  const [tutors, setTutors] = useState<TutorCard[]>([]);
  const [studentId, setStudentId] = useState("");
  const [tutorId, setTutorId] = useState("");
  const [startsAt, setStartsAt] = useState("");
  const [msg, setMsg] = useState("");

  useEffect(() => {
    if (!locationId) return;
    http.get<Student[]>("/students").then((r) => { setStudents(r.data); setStudentId(r.data[0]?.id ?? ""); });
    http.get<TutorCard[]>("/search/tutors").then((r) => { setTutors(r.data); setTutorId(r.data[0]?.tutorProfileId ?? ""); });
  }, [locationId]);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const starts = new Date(startsAt);
    await http.post("/bookings", { studentId, tutorProfileId: tutorId, startsAt: starts.toISOString(), endsAt: new Date(starts.getTime() + 3600000).toISOString() });
    setMsg("Schedule request placed.");
  };

  return (
    <div>
      <h1>Flexi scheduling</h1>
      <form className="card" onSubmit={onSubmit}>
        <div className="field"><label>1. Student</label>
          <select value={studentId} onChange={(e) => setStudentId(e.target.value)}>{students.map((s) => <option key={s.id} value={s.id}>{s.firstName} {s.lastName} · {s.gradeLevel}</option>)}</select>
        </div>
        <div className="field"><label>2. Tutor</label>
          <div className="grid">{tutors.map((t) => (
            <button type="button" key={t.tutorProfileId} className={tutorId === t.tutorProfileId ? "" : "ghost"} onClick={() => setTutorId(t.tutorProfileId)}>
              {t.firstName} {t.lastName} · ${t.hourlyRate}
            </button>
          ))}</div>
        </div>
        <div className="field"><label>3. Slot</label><input type="datetime-local" value={startsAt} onChange={(e) => setStartsAt(e.target.value)} required /></div>
        <button type="submit">Place schedule request</button>
        {msg ? <p>{msg}</p> : null}
      </form>
    </div>
  );
}

export function CoursesPage() {
  const { locationId, currentRole } = useAuth();
  const [courses, setCourses] = useState<Course[]>([]);
  const [materials, setMaterials] = useState<Material[]>([]);
  const [title, setTitle] = useState("");
  const [mat, setMat] = useState({ courseId: "", title: "", externalUrl: "" });

  const load = () => {
    http.get<Course[]>("/courses").then((r) => { setCourses(r.data); setMat((m) => ({ ...m, courseId: r.data[0]?.id ?? "" })); });
    http.get<Material[]>("/materials").then((r) => setMaterials(r.data));
  };
  useEffect(() => { if (locationId) load(); }, [locationId]);

  return (
    <div>
      <h1>Courses & materials</h1>
      {courses.map((c) => (
        <div className="card" key={c.id} style={{ marginBottom: "0.8rem" }}>
          <strong>{c.title}</strong>
          <div className="muted">{c.tutorName} · {c.enrolledStudents.join(", ")}</div>
          <p>{c.description}</p>
          {materials.filter((m) => m.courseId === c.id).map((m) => (
            <div key={m.id}><a href={m.signedUrl || m.externalUrl || "#"} target="_blank" rel="noreferrer">{m.title}</a></div>
          ))}
        </div>
      ))}
      {currentRole !== "parent" ? (
        <form className="card" onSubmit={(e) => { e.preventDefault(); http.post("/courses", { title }).then(() => { setTitle(""); load(); }); }}>
          <h2>New course</h2>
          <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Course title" required />
          <button type="submit">Create</button>
        </form>
      ) : null}
      {currentRole !== "parent" ? (
        <form className="card" style={{ marginTop: "1rem" }} onSubmit={(e) => { e.preventDefault(); http.post("/materials", mat).then(() => { setMat({ ...mat, title: "", externalUrl: "" }); load(); }); }}>
          <h2>Add material URL</h2>
          <select value={mat.courseId} onChange={(e) => setMat({ ...mat, courseId: e.target.value })}>{courses.map((c) => <option key={c.id} value={c.id}>{c.title}</option>)}</select>
          <input placeholder="Title" value={mat.title} onChange={(e) => setMat({ ...mat, title: e.target.value })} required />
          <input placeholder="https://…" value={mat.externalUrl} onChange={(e) => setMat({ ...mat, externalUrl: e.target.value })} />
          <button type="submit">Save material</button>
        </form>
      ) : null}
    </div>
  );
}

export function AssignmentsPage() {
  const { locationId, currentRole } = useAuth();
  const [rows, setRows] = useState<Assignment[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [students, setStudents] = useState<Student[]>([]);
  const [form, setForm] = useState({ courseId: "", title: "", instructions: "" });
  const [submit, setSubmit] = useState({ assignmentId: "", studentId: "", bodyText: "" });

  const load = () => http.get<Assignment[]>("/assignments").then((r) => setRows(r.data));
  useEffect(() => {
    if (!locationId) return;
    load();
    http.get<Course[]>("/courses").then((r) => { setCourses(r.data); setForm((f) => ({ ...f, courseId: r.data[0]?.id ?? "" })); });
    http.get<Student[]>("/students").then((r) => { setStudents(r.data); setSubmit((s) => ({ ...s, studentId: r.data[0]?.id ?? "" })); });
  }, [locationId]);

  return (
    <div>
      <h1>Assignments</h1>
      {rows.map((a, i) => (
        <div className="card" key={`${a.id}-${a.submissionId || i}`} style={{ marginBottom: "0.7rem" }}>
          <strong>{a.title}</strong> <span className="muted">{a.courseTitle}{a.studentName ? ` · ${a.studentName}` : ""}</span>
          <p>{a.instructions}</p>
          {a.score != null ? <div>Grade: {a.score}/{a.maxScore} — {a.feedback}</div> : a.submissionId ? (
            currentRole !== "parent" ? (
              <form onSubmit={(e) => { e.preventDefault(); const fd = new FormData(e.currentTarget); http.post(`/submissions/${a.submissionId}/grade`, { score: Number(fd.get("score")), feedback: String(fd.get("feedback") || "") }).then(load); }}>
                <input name="score" type="number" placeholder="Score" required />
                <input name="feedback" placeholder="Feedback" />
                <button type="submit">Grade</button>
              </form>
            ) : <div className="muted">Submitted, awaiting grade.</div>
          ) : <div className="muted">Not submitted</div>}
        </div>
      ))}
      {currentRole === "parent" ? (
        <form className="card" onSubmit={(e) => { e.preventDefault(); http.post(`/assignments/${submit.assignmentId}/submit`, { studentId: submit.studentId, bodyText: submit.bodyText }).then(() => { setSubmit({ ...submit, bodyText: "" }); load(); }); }}>
          <h2>Submit work</h2>
          <select value={submit.assignmentId} onChange={(e) => setSubmit({ ...submit, assignmentId: e.target.value })}>
            <option value="">Assignment</option>
            {[...new Map(rows.map((r) => [r.id, r])).values()].map((a) => <option key={a.id} value={a.id}>{a.title}</option>)}
          </select>
          <select value={submit.studentId} onChange={(e) => setSubmit({ ...submit, studentId: e.target.value })}>{students.map((s) => <option key={s.id} value={s.id}>{s.firstName}</option>)}</select>
          <textarea value={submit.bodyText} onChange={(e) => setSubmit({ ...submit, bodyText: e.target.value })} placeholder="Answer or notes" />
          <button type="submit">Submit</button>
        </form>
      ) : (
        <form className="card" onSubmit={(e) => { e.preventDefault(); http.post("/assignments", form).then(() => { setForm({ ...form, title: "", instructions: "" }); load(); }); }}>
          <h2>New assignment</h2>
          <select value={form.courseId} onChange={(e) => setForm({ ...form, courseId: e.target.value })}>{courses.map((c) => <option key={c.id} value={c.id}>{c.title}</option>)}</select>
          <input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder="Title" required />
          <textarea value={form.instructions} onChange={(e) => setForm({ ...form, instructions: e.target.value })} placeholder="Instructions" />
          <button type="submit">Create</button>
        </form>
      )}
    </div>
  );
}

export function ProgressPage() {
  const { locationId } = useAuth();
  const [rows, setRows] = useState<Progress[]>([]);
  useEffect(() => { if (locationId) http.get<Progress[]>("/progress").then((r) => setRows(r.data)); }, [locationId]);
  return (
    <div>
      <h1>Learning progress</h1>
      <table className="table">
        <thead><tr><th>Student</th><th>Course</th><th>Materials</th><th>Assignments graded</th><th>Lessons attended</th></tr></thead>
        <tbody>{rows.map((p) => <tr key={`${p.studentId}-${p.courseId}`}><td>{p.studentName}</td><td>{p.courseTitle}</td><td>{p.materialsDone}</td><td>{p.assignmentsGraded}</td><td>{p.lessonsAttended}</td></tr>)}</tbody>
      </table>
    </div>
  );
}

export function TutorMePage() {
  const { locationId } = useAuth();
  const [me, setMe] = useState<TutorProfile | null>(null);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [form, setForm] = useState({ headline: "", bio: "", hourlyRate: 25, experienceYears: 1, onlineOnly: true, subjectIds: [] as string[] });
  const [avail, setAvail] = useState({ weekday: 1, startTime: "18:00", endTime: "20:00" });

  const load = () => {
    http.get<TutorProfile>("/tutors/me").then((r) => {
      setMe(r.data);
      if (r.data) setForm({ headline: r.data.headline, bio: r.data.bio, hourlyRate: r.data.hourlyRate, experienceYears: r.data.experienceYears, onlineOnly: r.data.onlineOnly, subjectIds: [] });
    });
    http.get<Subject[]>("/subjects").then((r) => setSubjects(r.data));
  };
  useEffect(() => { if (locationId) load(); }, [locationId]);

  return (
    <div>
      <h1>My tutor profile</h1>
      <form className="card" onSubmit={(e) => { e.preventDefault(); http.put("/tutors/me", form).then(load); }}>
        <input value={form.headline} onChange={(e) => setForm({ ...form, headline: e.target.value })} placeholder="Headline" />
        <textarea value={form.bio} onChange={(e) => setForm({ ...form, bio: e.target.value })} placeholder="Bio" />
        <input type="number" value={form.hourlyRate} onChange={(e) => setForm({ ...form, hourlyRate: Number(e.target.value) })} />
        <div className="chips">{subjects.map((s) => (
          <label key={s.id} className="chip"><input type="checkbox" checked={form.subjectIds.includes(s.id)} onChange={(e) => setForm({
            ...form,
            subjectIds: e.target.checked ? [...form.subjectIds, s.id] : form.subjectIds.filter((id) => id !== s.id),
          })} /> {s.name}</label>
        ))}</div>
        <button type="submit">Save profile</button>
      </form>
      <div className="card" style={{ marginTop: "1rem" }}>
        <h2>Availability</h2>
        {me?.availability?.map((a: Availability) => (
          <div key={a.id}>{["Sun","Mon","Tue","Wed","Thu","Fri","Sat"][a.weekday]} {a.startTime}–{a.endTime}
            <button className="ghost" type="button" onClick={() => http.delete(`/tutors/me/availability/${a.id}`).then(load)}>Remove</button>
          </div>
        ))}
        <form onSubmit={(e) => { e.preventDefault(); http.post("/tutors/me/availability", avail).then(load); }}>
          <select value={avail.weekday} onChange={(e) => setAvail({ ...avail, weekday: Number(e.target.value) })}>
            {["Sun","Mon","Tue","Wed","Thu","Fri","Sat"].map((d, i) => <option key={d} value={i}>{d}</option>)}
          </select>
          <input type="time" value={avail.startTime} onChange={(e) => setAvail({ ...avail, startTime: e.target.value })} />
          <input type="time" value={avail.endTime} onChange={(e) => setAvail({ ...avail, endTime: e.target.value })} />
          <button type="submit">Add slot</button>
        </form>
      </div>
    </div>
  );
}

export function AdminPage() {
  const { locationId } = useAuth();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [bookings, setBookings] = useState<Booking[]>([]);
  useEffect(() => {
    if (!locationId) return;
    http.get<AdminUser[]>("/admin/users").then((r) => setUsers(r.data));
    http.get<Booking[]>("/bookings").then((r) => setBookings(r.data));
  }, [locationId]);
  return (
    <div>
      <h1>Admin</h1>
      <div className="card">
        <h2>Users</h2>
        <table className="table"><thead><tr><th>Name</th><th>Email</th><th>Role</th></tr></thead>
          <tbody>{users.map((u) => <tr key={u.userId}><td>{u.firstName} {u.lastName}</td><td>{u.email}</td><td>{u.role}</td></tr>)}</tbody>
        </table>
      </div>
      <div className="card" style={{ marginTop: "1rem" }}>
        <h2>Bookings</h2>
        {bookings.map((b) => <div key={b.id}>{b.studentName} · {b.tutorName} · {b.status} · {new Date(b.startsAt).toLocaleString()}</div>)}
      </div>
    </div>
  );
}
