import { Navigate, Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "./router/ProtectedRoute";
import Shell from "./lms/components/layout/Shell";
import SiteFooter from "./lms/components/layout/SiteFooter";
import LoadingModal from "./lms/components/layout/LoadingModal";
import Login from "./lms/components/auth/Login";
import Register from "./lms/components/auth/Register";
import ForgotPassword from "./lms/components/auth/ForgotPassword";
import ResetPassword from "./lms/components/auth/ResetPassword";
import Activate from "./lms/components/auth/Activate";
import Dashboard from "./lms/components/dashboard/Dashboard";
import StudentList from "./lms/components/students/List";
import FindTutors from "./lms/components/tutors/Find";
import TutorView from "./lms/components/tutors/Profile";
import { AdminPage, AssignmentsPage, CoursesPage, LessonsPage, ProgressPage, SchedulePage, TutorMePage } from "./lms/components/lessons/Pages";
import { roles, staffRoles } from "./lms/enums/roles";

export default function App() {
  return (
    <div className="app-frame">
      <div className="app-body">
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route path="/reset-password" element={<ResetPassword />} />
          <Route path="/activate" element={<Activate />} />
          <Route path="/" element={<ProtectedRoute><Shell /></ProtectedRoute>}>
            <Route index element={<Dashboard />} />
            <Route path="children" element={<StudentList />} />
            <Route path="tutors" element={<FindTutors />} />
            <Route path="tutors/:id" element={<TutorView />} />
            <Route path="schedule" element={<SchedulePage />} />
            <Route path="lessons" element={<LessonsPage />} />
            <Route path="courses" element={<CoursesPage />} />
            <Route path="assignments" element={<AssignmentsPage />} />
            <Route path="progress" element={<ProgressPage />} />
            <Route path="profile" element={<ProtectedRoute allowedRoles={[roles.tutor, ...staffRoles]}><TutorMePage /></ProtectedRoute>} />
            <Route path="admin" element={<ProtectedRoute allowedRoles={[...staffRoles]}><AdminPage /></ProtectedRoute>} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>
      <SiteFooter />
      <LoadingModal />
    </div>
  );
}
