import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Calendar, MapPin } from 'lucide-react';
import toast from 'react-hot-toast';
import { api } from '@/lib/api';
import { formatCurrency, formatDate, getStatusColor } from '@/lib/utils';
import type { Booking } from '@/types';
import { Button, Card, CardContent, Skeleton } from '@/components/ui';

export function MyBookingsPage() {
  const queryClient = useQueryClient();

  const { data: bookings, isLoading } = useQuery<Booking[]>({
    queryKey: ['my-bookings'],
    queryFn: () => api.get<Booking[]>('/api/bookings/my').then((r) => r.data),
  });

  const cancelBooking = useMutation({
    mutationFn: (bookingId: string) => api.post(`/api/bookings/${bookingId}/cancel`),
    onSuccess: () => {
      toast.success('Booking cancelled.');
      queryClient.invalidateQueries({ queryKey: ['my-bookings'] });
    },
    onError: () => toast.error('Failed to cancel booking.'),
  });

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="text-2xl font-bold text-gray-900">My Bookings</h1>

      {isLoading ? (
        <div className="mt-6 space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-32 w-full rounded-lg" />
          ))}
        </div>
      ) : bookings && bookings.length > 0 ? (
        <div className="mt-6 space-y-4">
          {bookings.map((booking) => (
            <Card key={booking.id}>
              <CardContent className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex-1">
                  <Link to={`/hotels/${booking.hotelId}`} className="text-lg font-semibold text-gray-900 hover:text-primary-600">
                    {booking.hotelName}
                  </Link>
                  <div className="mt-1 flex flex-wrap items-center gap-4 text-sm text-gray-500">
                    <span className="flex items-center gap-1">
                      <Calendar size={14} />
                      {formatDate(booking.checkInDate)} — {formatDate(booking.checkOutDate)}
                    </span>
                    <span className="flex items-center gap-1">
                      <MapPin size={14} />
                      {booking.roomName ?? 'Standard'}
                    </span>
                  </div>
                  <div className="mt-2 flex items-center gap-3">
                    <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusColor(booking.status)}`}>
                      {booking.status}
                    </span>
                    <span className="font-semibold text-primary-600">{formatCurrency(booking.totalAmount)}</span>
                  </div>
                </div>

                <div className="flex gap-2">
                  {booking.status === 'Confirmed' && (
                    <Button
                      variant="danger"
                      size="sm"
                      isLoading={cancelBooking.isPending}
                      onClick={() => cancelBooking.mutate(booking.id)}
                    >
                      Cancel
                    </Button>
                  )}
                  {booking.status === 'Completed' && (
                    <Link to={`/my-reviews?bookingId=${booking.id}`}>
                      <Button variant="outline" size="sm">Leave Review</Button>
                    </Link>
                  )}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="mt-16 text-center text-gray-500">
          <p>You don't have any bookings yet.</p>
          <Link to="/hotels">
            <Button className="mt-4">Browse Hotels</Button>
          </Link>
        </div>
      )}
    </div>
  );
}
