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
  accessTokenExpiresAt: string;
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
    avatarUrl?: string;
    role: string;
    emailConfirmed: boolean;
    createdAt: string;
  };
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

export interface AddressDto {
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
}

export interface GeoLocationDto {
  latitude: number;
  longitude: number;
}

export interface ContactInfoDto {
  phone: string;
  email: string;
  website?: string;
}

export interface CancellationPolicyDto {
  policyType: string;
  freeCancellationDays: number;
  partialRefundPercentage: number;
  partialRefundDays: number;
}

/** Full hotel detail (single hotel view) */
export interface Hotel {
  id: string;
  name: string;
  description: string;
  starRating: number;
  address: AddressDto;
  location?: GeoLocationDto;
  contactInfo: ContactInfoDto;
  ownerId: string;
  status: string;
  statusReason?: string;
  checkInTime: string;
  checkOutTime: string;
  coverImageUrl?: string;
  photoUrls: string[];
  rooms: Room[];
  cancellationPolicy: CancellationPolicyDto;
  createdAt: string;
  lastModifiedAt?: string;
}

/** Hotel search result */
export interface HotelSearchResult {
  id: string;
  name: string;
  description: string;
  starRating: number;
  city: string;
  country: string;
  status: string;
  coverImageUrl?: string;
  roomCount: number;
  minPrice?: number;
  currency?: string;
  photoCount: number;
  distanceKm?: number;
  createdAt: string;
}

/** Hotel summary for owner dashboard */
export interface HotelSummary {
  id: string;
  name: string;
  starRating: number;
  city: string;
  country: string;
  status: string;
  coverImageUrl?: string;
  roomCount: number;
  minPrice?: number;
  currency?: string;
  photoCount: number;
  createdAt: string;
}

export interface HotelPhoto {
  id: string;
  url: string;
  caption?: string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface HotelSearchParams {
  q?: string;
  city?: string;
  country?: string;
  minStarRating?: number;
  maxStarRating?: number;
  minPrice?: number;
  maxPrice?: number;
  roomType?: string;
  latitude?: number;
  longitude?: number;
  radiusKm?: number;
  sortBy?: string;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/* ─────────────────────────── Room ─────────────────────────── */

export interface Room {
  id: string;
  hotelId: string;
  name: string;
  description: string;
  roomType: string;
  maxOccupancy: number;
  basePrice: number;
  currency: string;
  totalInventory: number;
  sizeInSquareMeters?: number;
  bedConfiguration?: string;
  isActive: boolean;
  amenities: string[];
  photoUrls: string[];
}

/* ─────────────────────────── Booking ─────────────────────────── */

export interface GuestInfoDto {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
}

export interface PriceBreakdownDto {
  nightlyRate: number;
  nights: number;
  subtotal: number;
  taxAmount: number;
  serviceFee: number;
  total: number;
  currency: string;
}

export interface RefundInfoDto {
  refundPercentage: number;
  refundAmount: number;
  currency: string;
}

/** Full booking detail */
export interface Booking {
  id: string;
  hotelId: string;
  roomId: string;
  guestUserId: string;
  hotelName: string;
  roomName: string;
  confirmationNumber: string;
  checkIn: string;
  checkOut: string;
  nights: number;
  numberOfGuests: number;
  guestInfo: GuestInfoDto;
  priceBreakdown: PriceBreakdownDto;
  status: BookingStatus;
  paymentStatus: string;
  specialRequests?: string;
  cancellationReason?: string;
  refundInfo?: RefundInfoDto;
  cancelledAt?: string;
  checkedInAt?: string;
  completedAt?: string;
  createdAt: string;
  lastModifiedAt?: string;
}

/** Booking summary for list views */
export interface BookingSummary {
  id: string;
  confirmationNumber: string;
  hotelName: string;
  roomName: string;
  checkIn: string;
  checkOut: string;
  nights: number;
  status: BookingStatus;
  paymentStatus: string;
  totalAmount: number;
  currency: string;
  createdAt: string;
}

export type BookingStatus =
  | 'Pending'
  | 'Confirmed'
  | 'CheckedIn'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow';

export interface CreateBookingRequest {
  hotelId: string;
  roomId: string;
  checkIn: string;
  checkOut: string;
  numberOfGuests: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
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
  guestName: string;
  title: string;
  body: string;
  cleanliness: number;
  service: number;
  location: number;
  comfort: number;
  valueForMoney: number;
  overallRating: number;
  managementResponse?: string;
  stayedFrom: string;
  stayedTo: string;
  createdAt: string;
}

export interface SubmitReviewRequest {
  hotelId: string;
  bookingId: string;
  guestName: string;
  title: string;
  body: string;
  cleanliness: number;
  service: number;
  location: number;
  comfort: number;
  valueForMoney: number;
  stayedFrom: string;
  stayedTo: string;
}

export interface HotelRatingSummary {
  hotelId: string;
  averageOverall: number;
  averageCleanliness: number;
  averageService: number;
  averageLocation: number;
  averageComfort: number;
  averageValueForMoney: number;
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
  errors?: { field: string; message: string }[];
}
