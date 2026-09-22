import { useEffect, useState } from "react";
import http from "../../services/http.service";
import type { LocationStats, Booking, Course, Progress } from "../../models";
import { useAuth } from "../../../contexts/JWTAuthContext";
import TablePaginationBar from "../common/TablePaginationBar";
import { useClientPagination } from "../common/useClientPagination";

export default function Dashboard() {
  const { locationId, currentRole } = useAuth();
  const [stats, setStats] = useState<LocationStats | null>(null);
  const [lessons, setLessons] = useState<Booking[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [progress, setProgress] = useState<Progress[]>([]);
  const { page, setPage, rowsPerPage, setRowsPerPage, pageItems, total } = useClientPagination(progress, 10);

  useEffect(() => {
    if (!locationId) return;
    http.get<LocationStats>("/location/stats").then((r) => setStats(r.data));
    http.get<Booking[]>("/bookings").then((r) => setLessons(r.data));
    http.get<Course[]>("/courses").then((r) => setCourses(r.data));
    http.get<Progress[]>("/progress").then((r) => setProgress(r.data));
  }, [locationId]);

  const upcoming = lessons.filter((l) => l.status === "accepted" && new Date(l.startsAt) > new Date());

  return (
    <div>
      <h1>{currentRole === "tutor" ? "Tutor dashboard" : currentRole === "parent" ? "Find the tutor your child needs" : "Location dashboard"}</h1>
      <div className="stats">
        <div className="stat"><div className="muted">Upcoming</div><strong>{stats?.upcomingLessons ?? upcoming.length}</strong></div>
        <div className="stat"><div className="muted">Courses</div><strong>{courses.length}</strong></div>
        <div className="stat"><div className="muted">Open requests</div><strong>{stats?.openRequests ?? 0}</strong></div>
        <div className="stat"><div className="muted">Students</div><strong>{stats?.studentCount ?? 0}</strong></div>
      </div>
      <div className="card">
        <h2>Upcoming lessons</h2>
        {upcoming.length === 0 ? <p className="muted">No upcoming lessons.</p> : upcoming.map((l) => (
          <div className="lesson-row" key={l.id}>
            <strong>{l.studentName} · {l.tutorName}</strong>
            <span className="muted">{new Date(l.startsAt).toLocaleString()} · {l.subjectName}</span>
            {l.meetingUrl ? <a href={l.meetingUrl} target="_blank" rel="noreferrer">Join</a> : null}
          </div>
        ))}
      </div>
      <div className="card" style={{ marginTop: "1rem" }}>
        <h2>Progress</h2>
        {progress.length === 0 ? <p className="muted">No progress rows yet.</p> : (
          <>
            <table className="table">
              <thead><tr><th>Student</th><th>Course</th><th>Materials</th><th>Graded</th><th>Lessons</th></tr></thead>
              <tbody>{pageItems.map((p) => (
                <tr key={`${p.studentId}-${p.courseId}`}><td>{p.studentName}</td><td>{p.courseTitle}</td><td>{p.materialsDone}</td><td>{p.assignmentsGraded}</td><td>{p.lessonsAttended}</td></tr>
              ))}</tbody>
            </table>
            <TablePaginationBar
              page={page}
              rowsPerPage={rowsPerPage}
              total={total}
              onPageChange={setPage}
              onRowsPerPageChange={setRowsPerPage}
            />
          </>
        )}
      </div>
    </div>
  );
}
