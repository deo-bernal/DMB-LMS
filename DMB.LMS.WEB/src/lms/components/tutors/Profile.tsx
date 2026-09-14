import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import http from "../../services/http.service";
import type { Student, Subject, TutorProfile } from "../../models";
import { useAuth } from "../../../contexts/JWTAuthContext";

export default function TutorView() {
  const { id } = useParams();
  const { locationId } = useAuth();
  const navigate = useNavigate();
  const [tutor, setTutor] = useState<TutorProfile | null>(null);
  const [students, setStudents] = useState<Student[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [studentId, setStudentId] = useState("");
  const [subjectId, setSubjectId] = useState("");
  const [startsAt, setStartsAt] = useState("");
  const [message, setMessage] = useState("");

  useEffect(() => {
    if (!locationId || !id) return;
    http.get<TutorProfile>(`/tutors/${id}`).then((r) => setTutor(r.data));
    http.get<Student[]>("/students").then((r) => { setStudents(r.data); setStudentId(r.data[0]?.id ?? ""); });
    http.get<Subject[]>("/subjects").then((r) => setSubjects(r.data));
  }, [locationId, id]);

  const book = async (start: string, hours = 1) => {
    const starts = new Date(start);
    const ends = new Date(starts.getTime() + hours * 60 * 60 * 1000);
    await http.post("/bookings", { studentId, tutorProfileId: id, subjectId: subjectId || null, startsAt: starts.toISOString(), endsAt: ends.toISOString() });
    setMessage("Request sent. The tutor will accept or propose another time.");
  };

  if (!tutor) return <div className="card">Loading tutor…</div>;

  return (
    <div className="grid" style={{ gridTemplateColumns: "280px 1fr" }}>
      <div className="card">
        <h1>{tutor.firstName} {tutor.lastName}</h1>
        <div>${tutor.hourlyRate}/hour</div>
        <div className="chips">{tutor.subjects.split(", ").filter(Boolean).map((s) => <span className="chip" key={s}>{s}</span>)}</div>
        <p>{tutor.bio}</p>
      </div>
      <div className="card">
        <h2>Available sessions</h2>
        <div className="field"><label>Child</label>
          <select value={studentId} onChange={(e) => setStudentId(e.target.value)}>{students.map((s) => <option key={s.id} value={s.id}>{s.firstName} {s.lastName}</option>)}</select>
        </div>
        <div className="field"><label>Subject</label>
          <select value={subjectId} onChange={(e) => setSubjectId(e.target.value)}><option value="">Any</option>{subjects.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}</select>
        </div>
        {tutor.availability.map((a) => (
          <div className="lesson-row" key={a.id}>
            <strong>{["Sun","Mon","Tue","Wed","Thu","Fri","Sat"][a.weekday]} {a.startTime}–{a.endTime}</strong>
            <span className="muted">{tutor.onlineOnly ? "Online only" : "In person or online"} · ${tutor.hourlyRate}</span>
            <button type="button" onClick={() => {
              const d = new Date();
              const add = (a.weekday - d.getDay() + 7) % 7 || 7;
              d.setDate(d.getDate() + add);
              const [h, m] = a.startTime.split(":");
              d.setHours(Number(h), Number(m), 0, 0);
              void book(d.toISOString());
            }}>Book</button>
          </div>
        ))}
        <div className="field"><label>Request a different time</label><input type="datetime-local" value={startsAt} onChange={(e) => setStartsAt(e.target.value)} /></div>
        <button className="ghost" type="button" onClick={() => startsAt && book(startsAt)}>Request different time</button>
        {message ? <p>{message}</p> : null}
        <button className="ghost" type="button" onClick={() => navigate("/tutors")}>Back to search</button>
      </div>
    </div>
  );
}
