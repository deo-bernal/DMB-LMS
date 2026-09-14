import { FormEvent, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import http from "../../services/http.service";
import type { TutorCard } from "../../models";
import { useAuth } from "../../../contexts/JWTAuthContext";

export default function FindTutors() {
  const { locationId } = useAuth();
  const [q, setQ] = useState({ subject: "", grade: "", maxRate: "" });
  const [rows, setRows] = useState<TutorCard[]>([]);

  const search = async (e?: FormEvent) => {
    e?.preventDefault();
    const res = await http.get<TutorCard[]>("/search/tutors", { params: { subject: q.subject || undefined, grade: q.grade || undefined, maxRate: q.maxRate || undefined } });
    setRows(res.data);
  };

  useEffect(() => { if (locationId) void search(); }, [locationId]);

  return (
    <div>
      <div className="hero">
        <h1>Find the tutor your child needs in minutes.</h1>
        <form className="search-row" onSubmit={search}>
          <input placeholder="Subject" value={q.subject} onChange={(e) => setQ({ ...q, subject: e.target.value })} />
          <input placeholder="Grade" value={q.grade} onChange={(e) => setQ({ ...q, grade: e.target.value })} />
          <input placeholder="Max rate" value={q.maxRate} onChange={(e) => setQ({ ...q, maxRate: e.target.value })} />
          <button type="submit">Search</button>
        </form>
      </div>
      <div className="grid">
        {rows.map((t) => (
          <div className="tutor-card" key={t.tutorProfileId}>
            <strong>{t.firstName} {t.lastName}</strong>
            <span>{"★".repeat(Math.round(t.rating))} {t.reviewCount} reviews</span>
            <div className="chips">{t.subjects.split(", ").filter(Boolean).map((s) => <span className="chip" key={s}>{s}</span>)}</div>
            <p className="muted">{t.headline}</p>
            <div><strong>${t.hourlyRate}</strong> / hour</div>
            <Link to={`/tutors/${t.tutorProfileId}`}><button type="button">Book a lesson</button></Link>
          </div>
        ))}
      </div>
    </div>
  );
}
