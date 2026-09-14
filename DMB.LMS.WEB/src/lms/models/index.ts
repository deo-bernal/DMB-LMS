export type LocationMembership = { locationId: string; name: string; role: string };
export type LoginResponse = { token: string; locations: LocationMembership[]; currentLocationId?: string; firstName?: string };
export type LocationStats = { parentCount: number; tutorCount: number; studentCount: number; upcomingLessons: number; openRequests: number };
export type Student = { id: string; parentUserId: string; firstName: string; lastName: string; gradeLevel: string; notes?: string };
export type Subject = { id: string; name: string };
export type TutorCard = {
  tutorProfileId: string; userId: string; firstName: string; lastName: string; headline: string; bio: string;
  hourlyRate: number; currency: string; experienceYears: number; onlineOnly: boolean; rating: number; reviewCount: number;
  avatarPath?: string; subjects: string; score: number;
};
export type Availability = { id: string; weekday: number; startTime: string; endTime: string };
export type TutorProfile = TutorCard & { availability: Availability[] };
export type Booking = {
  id: string; studentId: string; studentName: string; tutorProfileId: string; tutorName: string; subjectName: string;
  startsAt: string; endsAt: string; status: string; price: number; meetingUrl?: string; note?: string; present?: boolean;
};
export type Course = { id: string; tutorProfileId: string; tutorName: string; title: string; description: string; enrolledStudents: string[] };
export type Material = { id: string; courseId: string; title: string; externalUrl?: string; storagePath?: string; signedUrl?: string };
export type Assignment = {
  id: string; courseId: string; courseTitle: string; title: string; instructions: string; dueAt?: string; maxScore: number;
  submissionId?: string; submissionText?: string; score?: number; feedback?: string; studentId?: string; studentName?: string;
};
export type Progress = { studentId: string; studentName: string; courseId: string; courseTitle: string; materialsDone: number; assignmentsGraded: number; lessonsAttended: number };
export type AdminUser = { userId: string; email: string; firstName: string; lastName: string; role: string };
