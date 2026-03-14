import { Routes, Route } from 'react-router-dom';
import { MainLayout } from '@/layouts/MainLayout';
import { AdminLayout } from '@/layouts/AdminLayout';
import { ProtectedRoute } from '@/features/auth/ProtectedRoute';

// ── Public pages ──
import { HomePage } from '@/pages/HomePage';
import { HotelSearchPage } from '@/pages/HotelSearchPage';
import { HotelDetailPage } from '@/pages/HotelDetailPage';
import { LoginPage } from '@/pages/auth/LoginPage';
import { RegisterPage } from '@/pages/auth/RegisterPage';
import { VerifyEmailPage } from '@/pages/auth/VerifyEmailPage';
import { NotFoundPage } from '@/pages/NotFoundPage';
import { ForbiddenPage } from '@/pages/ForbiddenPage';

// ── Booking flow ──
import { BookingPage } from '@/pages/booking/BookingPage';
import { BookingConfirmationPage } from '@/pages/booking/BookingConfirmationPage';

// ── Guest dashboard ──
import { MyBookingsPage } from '@/pages/guest/MyBookingsPage';
import { ProfilePage } from '@/pages/guest/ProfilePage';
import { MyReviewsPage } from '@/pages/guest/MyReviewsPage';

// ── Owner panel ──
import { OwnerHotelsPage } from '@/pages/owner/OwnerHotelsPage';
import { OwnerHotelFormPage } from '@/pages/owner/OwnerHotelFormPage';
import { OwnerBookingsPage } from '@/pages/owner/OwnerBookingsPage';

// ── Admin panel ──
import { AdminDashboardPage } from '@/pages/admin/AdminDashboardPage';
import { AdminUsersPage } from '@/pages/admin/AdminUsersPage';
import { AdminHotelsPage } from '@/pages/admin/AdminHotelsPage';

export function App() {
  return (
    <Routes>
      {/* ── Public routes ── */}
      <Route element={<MainLayout />}>
        <Route index element={<HomePage />} />
        <Route path="hotels" element={<HotelSearchPage />} />
        <Route path="hotels/:id" element={<HotelDetailPage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="verify-email" element={<VerifyEmailPage />} />

        {/* ── Authenticated guest routes ── */}
        <Route element={<ProtectedRoute />}>
          <Route path="booking/:hotelId" element={<BookingPage />} />
          <Route path="booking/confirmation/:bookingId" element={<BookingConfirmationPage />} />
          <Route path="my-bookings" element={<MyBookingsPage />} />
          <Route path="profile" element={<ProfilePage />} />
          <Route path="my-reviews" element={<MyReviewsPage />} />
        </Route>

        {/* ── Owner routes ── */}
        <Route element={<ProtectedRoute requiredRole="HotelOwner" />}>
          <Route path="owner/hotels" element={<OwnerHotelsPage />} />
          <Route path="owner/hotels/new" element={<OwnerHotelFormPage />} />
          <Route path="owner/hotels/:id/edit" element={<OwnerHotelFormPage />} />
          <Route path="owner/bookings" element={<OwnerBookingsPage />} />
        </Route>

        <Route path="403" element={<ForbiddenPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>

      {/* ── Admin routes ── */}
      <Route element={<ProtectedRoute requiredRole="Admin" />}>
        <Route element={<AdminLayout />}>
          <Route path="admin" element={<AdminDashboardPage />} />
          <Route path="admin/users" element={<AdminUsersPage />} />
          <Route path="admin/hotels" element={<AdminHotelsPage />} />
        </Route>
      </Route>
    </Routes>
  );
}
