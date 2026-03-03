/* ─────────────────────────── Auth ─────────────────────────── */

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  roles: string[];
}

export interface User {
  userId: string;
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  roles: string[];
}

/* ─────────────────────────── Hotel ─────────────────────────── */

export interface Hotel {
  id: string;
  name: string;
  description: string;
  city: string;
  country: string;
  address: string;
  zipCode?: string;
  contactEmail?: string;
  contactPhone?: string;
  latitude: number;
  longitude: number;
  starRating: number;
  averageRating: number;
  totalReviews: number;
  amenities: string[];
  photos: HotelPhoto[];
  rooms: Room[];
  isApproved: boolean;
  ownerId: string;
}

export interface HotelPhoto {
  id: string;
  url: string;
  caption?: string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface HotelSummary {
  id: string;
  name: string;
  city: string;
  country: string;
  starRating: number;
  averageRating: number;
  totalReviews: number;
  priceFrom: number;
  totalRooms?: number;
  primaryPhotoUrl?: string;
}

export interface HotelSearchParams {
  city?: string;
  country?: string;
  checkIn?: string;
  checkOut?: string;
  guests?: number;
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  amenities?: string[];
  page?: number;
  pageSize?: number;
  sortBy?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/* ─────────────────────────── Room ─────────────────────────── */

export interface Room {
  id: string;
  hotelId: string;
  name: string;
  description: string;
  roomType: string;
  pricePerNight: number;
  capacity: number;
  totalRooms: number;
  availableRooms: number;
  amenities: string[];
}

/* ─────────────────────────── Booking ─────────────────────────── */

export interface Booking {
  id: string;
  hotelId: string;
  hotelName: string;
  roomId: string;
  roomName: string;
  userId: string;
  checkInDate: string;
  checkOutDate: string;
  guests: number;
  totalAmount: number;
  status: BookingStatus;
  createdAt: string;
}

export type BookingStatus =
  | 'Pending'
  | 'Confirmed'
  | 'CheckedIn'
  | 'Completed'
  | 'Cancelled';

export interface CreateBookingRequest {
  hotelId: string;
  roomId: string;
  checkInDate: string;
  checkOutDate: string;
  guests: number;
  specialRequests?: string;
}

/* ─────────────────────────── Payment ─────────────────────────── */

export interface PaymentIntent {
  clientSecret: string;
  paymentIntentId: string;
  amount: number;
  currency: string;
}

/* ─────────────────────────── Review ─────────────────────────── */

export interface Review {
  id: string;
  hotelId: string;
  hotelName?: string;
  userId: string;
  userDisplayName: string;
  overallRating: number;
  cleanliness: number;
  comfort: number;
  location: number;
  facilities: number;
  staff: number;
  title: string;
  comment: string;
  managementResponse?: string;
  createdAt: string;
}

export interface SubmitReviewRequest {
  hotelId: string;
  bookingId: string;
  cleanliness: number;
  comfort: number;
  location: number;
  facilities: number;
  staff: number;
  title: string;
  comment: string;
}

export interface HotelRatingSummary {
  hotelId: string;
  averageOverall: number;
  averageCleanliness: number;
  averageComfort: number;
  averageLocation: number;
  averageFacilities: number;
  averageStaff: number;
  totalReviews: number;
}

/* ─────────────────────────── Analytics ─────────────────────────── */

export interface DashboardKpi {
  totalRevenue: number;
  totalBookings: number;
  totalCancellations: number;
  cancellationRate: number;
  averageBookingValue: number;
  averageRating: number;
  totalReviews: number;
  averageOccupancyRate: number;
  activeHotels: number;
  revenueChangePercent: number;
  bookingChangePercent: number;
}

export interface RevenueDataPoint {
  date: string;
  revenue: number;
  bookingCount: number;
  averageBookingValue: number;
  cancellationCount: number;
  refundAmount: number;
}

export interface HotelPerformance {
  hotelId: string;
  hotelName: string;
  totalRevenue: number;
  totalBookings: number;
  averageRating: number;
  totalReviews: number;
  cancellationRate: number;
  occupancyRate: number;
}

/* ─────────────────────────── API Error ─────────────────────────── */

export interface ApiError {
  status: number;
  error: string;
  message: string;
  errors?: { code: string; message: string }[];
}
