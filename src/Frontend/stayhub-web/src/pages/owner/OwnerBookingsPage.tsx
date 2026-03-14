import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Calendar, MapPin } from 'lucide-react';
import toast from 'react-hot-toast';
import { api } from '@/lib/api';
import { formatCurrency, formatDate, getStatusColor } from '@/lib/utils';
import type { BookingSummary } from '@/types';
import { Button, Card, CardContent, Skeleton } from '@/components/ui';

export function OwnerBookingsPage() {
  const queryClient = useQueryClient();

  const { data: bookings, isLoading } = useQuery<BookingSummary[]>({
    queryKey: ['owner-bookings'],
    queryFn: () => api.get<BookingSummary[]>('/bookings/my').then((r) => r.data),
  });

  const updateStatus = useMutation({
    mutationFn: ({ bookingId, action }: { bookingId: string; action: string }) =>
      api.post(`/bookings/${bookingId}/${action}`),
    onSuccess: () => {
      toast.success('Booking updated.');
      queryClient.invalidateQueries({ queryKey: ['owner-bookings'] });
    },
    onError: () => toast.error('Failed to update booking.'),
  });

  return (
    <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="text-2xl font-bold text-gray-900">Guest Bookings</h1>
      <p className="mt-1 text-sm text-gray-500">Manage bookings for your hotels</p>

      {isLoading ? (
        <div className="mt-6 space-y-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-24 w-full rounded-lg" />
          ))}
        </div>
      ) : bookings && bookings.length > 0 ? (
        <div className="mt-6 space-y-4">
          {bookings.map((booking) => (
            <Card key={booking.id}>
              <CardContent className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex-1">
                  <h3 className="font-semibold text-gray-900">{booking.hotelName}</h3>
                  <div className="mt-1 flex flex-wrap items-center gap-4 text-sm text-gray-500">
                    <span className="flex items-center gap-1">
                      <Calendar size={14} />
                      {formatDate(booking.checkIn)} — {formatDate(booking.checkOut)}
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
                    <span className="font-semibold text-primary-600">{formatCurrency(booking.totalAmount ?? 0)}</span>
                  </div>
                </div>

                <div className="flex gap-2">
                  {booking.status === 'Confirmed' && (
                    <Button
                      size="sm"
                      onClick={() => updateStatus.mutate({ bookingId: booking.id, action: 'check-in' })}
                    >
                      Check In
                    </Button>
                  )}
                  {booking.status === 'CheckedIn' && (
                    <Button
                      size="sm"
                      onClick={() => updateStatus.mutate({ bookingId: booking.id, action: 'complete' })}
                    >
                      Complete
                    </Button>
                  )}
                  {(booking.status === 'Confirmed' || booking.status === 'Pending') && (
                    <Button
                      variant="danger"
                      size="sm"
                      onClick={() => updateStatus.mutate({ bookingId: booking.id, action: 'cancel' })}
                    >
                      Cancel
                    </Button>
                  )}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="mt-16 text-center text-gray-500">
          <p>No bookings yet for your hotels.</p>
        </div>
      )}
    </div>
  );
}
