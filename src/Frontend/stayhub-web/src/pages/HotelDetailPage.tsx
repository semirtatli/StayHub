import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { MapPin, Star, Users, BedDouble, ChevronLeft, ChevronRight } from 'lucide-react';
import { api } from '@/lib/api';
import { formatCurrency } from '@/lib/utils';
import type { Hotel, Review, Room } from '@/types';
import { Button, Card, CardContent, StarRating, Skeleton } from '@/components/ui';

export function HotelDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [photoIdx, setPhotoIdx] = useState(0);

  const { data: hotel, isLoading } = useQuery<Hotel>({
    queryKey: ['hotel', id],
    queryFn: () => api.get<Hotel>(`/hotels/${id}`).then((r) => r.data),
    enabled: !!id,
  });

  const { data: rooms } = useQuery<Room[]>({
    queryKey: ['hotel-rooms', id],
    queryFn: () => api.get<Room[]>(`/hotels/${id}/rooms`).then((r) => r.data),
    enabled: !!id,
  });

  const { data: reviews } = useQuery<Review[]>({
    queryKey: ['hotel-reviews', id],
    queryFn: () => api.get<Review[]>(`/reviews/hotel/${id}`).then((r) => r.data),
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <div className="mx-auto max-w-7xl space-y-6 px-4 py-8">
        <Skeleton className="h-96 w-full rounded-xl" />
        <Skeleton className="h-8 w-1/3" />
        <Skeleton className="h-4 w-2/3" />
      </div>
    );
  }

  if (!hotel) {
    return <div className="py-16 text-center text-gray-500">Hotel not found.</div>;
  }

  const photos = hotel.photos ?? [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Photo Gallery */}
      {photos.length > 0 && (
        <div className="relative mb-8 overflow-hidden rounded-xl">
          <img
            src={photos[photoIdx]?.url}
            alt={photos[photoIdx]?.caption ?? hotel.name}
            className="h-96 w-full object-cover"
          />
          {photos.length > 1 && (
            <>
              <button
                className="absolute left-3 top-1/2 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white hover:bg-black/70"
                onClick={() => setPhotoIdx((prev) => (prev === 0 ? photos.length - 1 : prev - 1))}
              >
                <ChevronLeft size={20} />
              </button>
              <button
                className="absolute right-3 top-1/2 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white hover:bg-black/70"
                onClick={() => setPhotoIdx((prev) => (prev === photos.length - 1 ? 0 : prev + 1))}
              >
                <ChevronRight size={20} />
              </button>
              <div className="absolute bottom-3 left-1/2 -translate-x-1/2 rounded-full bg-black/50 px-3 py-1 text-xs text-white">
                {photoIdx + 1} / {photos.length}
              </div>
            </>
          )}
        </div>
      )}

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        {/* Hotel Info */}
        <div className="lg:col-span-2">
          <h1 className="text-3xl font-bold text-gray-900">{hotel.name}</h1>
          <p className="mt-1 flex items-center gap-1 text-gray-500">
            <MapPin size={16} />
            {hotel.address}, {hotel.city}, {hotel.country}
          </p>
          <div className="mt-2 flex items-center gap-2">
            <Star size={18} className="text-accent-500" fill="currentColor" />
            <span className="text-sm font-semibold">{hotel.starRating} stars</span>
          </div>

          <div className="mt-6">
            <h2 className="text-lg font-semibold text-gray-900">About</h2>
            <p className="mt-2 whitespace-pre-line text-gray-600">{hotel.description}</p>
          </div>

          {/* Rooms */}
          <div className="mt-10">
            <h2 className="text-lg font-semibold text-gray-900">Available Rooms</h2>
            {rooms && rooms.length > 0 ? (
              <div className="mt-4 space-y-4">
                {rooms.map((room) => (
                  <Card key={room.id}>
                    <CardContent className="flex items-center justify-between">
                      <div>
                        <h3 className="font-semibold text-gray-900">{room.roomType}</h3>
                        <div className="mt-1 flex items-center gap-4 text-sm text-gray-500">
                          <span className="flex items-center gap-1">
                            <Users size={14} /> {room.capacity} guests
                          </span>
                          <span className="flex items-center gap-1">
                            <BedDouble size={14} /> {room.roomType}
                          </span>
                        </div>
                      </div>
                      <div className="text-right">
                        <div className="text-xl font-bold text-primary-600">
                          {formatCurrency(room.pricePerNight)}
                          <span className="text-xs font-normal text-gray-400">/night</span>
                        </div>
                        <Button
                          size="sm"
                          className="mt-2"
                          onClick={() => navigate(`/booking/${hotel.id}?roomId=${room.id}`)}
                        >
                          Book Now
                        </Button>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            ) : (
              <p className="mt-4 text-sm text-gray-500">No rooms available at the moment.</p>
            )}
          </div>

          {/* Reviews */}
          <div className="mt-10">
            <h2 className="text-lg font-semibold text-gray-900">Guest Reviews</h2>
            {reviews && reviews.length > 0 ? (
              <div className="mt-4 space-y-4">
                {reviews.map((review) => (
                  <Card key={review.id}>
                    <CardContent>
                      <div className="flex items-center justify-between">
                        <span className="text-sm font-medium text-gray-700">{review.userDisplayName ?? 'Guest'}</span>
                        <StarRating rating={review.overallRating} size={16} />
                      </div>
                      {review.title && <h4 className="mt-2 font-medium text-gray-900">{review.title}</h4>}
                      <p className="mt-1 text-sm text-gray-600">{review.comment}</p>
                      {review.managementResponse && (
                        <div className="mt-3 rounded-lg bg-gray-50 p-3 text-sm">
                          <span className="font-medium text-gray-700">Management Response:</span>
                          <p className="mt-1 text-gray-600">{review.managementResponse}</p>
                        </div>
                      )}
                    </CardContent>
                  </Card>
                ))}
              </div>
            ) : (
              <p className="mt-4 text-sm text-gray-500">No reviews yet. Be the first to review!</p>
            )}
          </div>
        </div>

        {/* Booking Sidebar */}
        <div className="lg:col-span-1">
          <div className="sticky top-24 rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
            <h3 className="text-lg font-semibold text-gray-900">Book This Hotel</h3>
            <p className="mt-2 text-sm text-gray-500">
              Select a room above or click below to start your booking.
            </p>
            <Button className="mt-4 w-full" onClick={() => navigate(`/booking/${hotel.id}`)}>
              Check Availability
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
