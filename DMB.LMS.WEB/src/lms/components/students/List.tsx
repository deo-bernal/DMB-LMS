import { FormEvent, useEffect, useState } from "react";
import http from "../../services/http.service";
import type { Student } from "../../models";
import { useAuth } from "../../../contexts/JWTAuthContext";

export default function StudentList() {
  const { locationId } = useAuth();
  const [rows, setRows] = useState<Student[]>([]);
  const [form, setForm] = useState({ firstName: "", lastName: "", gradeLevel: "", notes: "" });

  const load = () => { http.get<Student[]>("/students").then((r) => setRows(r.data)); };
  useEffect(() => { if (locationId) load(); }, [locationId]);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    await http.post("/students", form);
    setForm({ firstName: "", lastName: "", gradeLevel: "", notes: "" });
    load();
  };

  return (
    <div>
      <h1>Children</h1>
      <div className="grid">
        {rows.map((s) => (
          <div className="card" key={s.id}>
            <strong>{s.firstName} {s.lastName}</strong>
            <div className="muted">{s.gradeLevel}</div>
            {s.notes ? <p>{s.notes}</p> : null}
          </div>
        ))}
      </div>
      <form className="card" style={{ marginTop: "1rem" }} onSubmit={onSubmit}>
        <h2>Add a child</h2>
        <div className="field"><label>First name</label><input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required /></div>
        <div className="field"><label>Last name</label><input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} /></div>
        <div className="field"><label>Grade</label><input value={form.gradeLevel} onChange={(e) => setForm({ ...form, gradeLevel: e.target.value })} placeholder="Grade 5" /></div>
        <button type="submit">Save</button>
      </form>
    </div>
  );
}
