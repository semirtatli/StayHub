import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { CheckCircle } from 'lucide-react';
import { api } from '@/lib/api';
import { formatCurrency, formatDate } from '@/lib/utils';
import type { Booking } from '@/types';
import { Button, Card, CardContent, Skeleton } from '@/components/ui';

export function BookingConfirmationPage() {
  const { bookingId } = useParams<{ bookingId: string }>();

  const { data: booking, isLoading } = useQuery<Booking>({
    queryKey: ['booking', bookingId],
    queryFn: () => api.get<Booking>(`/bookings/${bookingId}`).then((r) => r.data),
    enabled: !!bookingId,
  });

  if (isLoading) {
    return (
      <div className="mx-auto max-w-lg space-y-4 px-4 py-16">
        <Skeleton className="mx-auto h-16 w-16 rounded-full" />
        <Skeleton className="h-6 w-2/3 mx-auto" />
        <Skeleton className="h-32 w-full" />
      </div>
    );
  }

  if (!booking) {
    return <div className="py-16 text-center text-gray-500">Booking not found.</div>;
  }

  return (
    <div className="mx-auto max-w-lg px-4 py-16 text-center">
      <CheckCircle className="mx-auto text-green-500" size={56} />
      <h1 className="mt-4 text-2xl font-bold text-gray-900">Booking Confirmed!</h1>
      <p className="mt-2 text-gray-500">Your reservation has been successfully created.</p>

      <Card className="mt-8 text-left">
        <CardContent className="space-y-3 text-sm">
          <div className="flex justify-between">
            <span className="text-gray-500">Booking ID</span>
            <span className="font-mono text-gray-900">{booking.id.slice(0, 8)}…</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Hotel</span>
            <span className="font-medium text-gray-900">{booking.hotelName}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Check-in</span>
            <span className="text-gray-900">{formatDate(booking.checkIn)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Check-out</span>
            <span className="text-gray-900">{formatDate(booking.checkOut)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Status</span>
            <span className="font-medium capitalize text-green-600">{booking.status}</span>
          </div>
          <div className="flex justify-between border-t pt-3">
            <span className="text-gray-500">Total</span>
            <span className="text-lg font-bold text-primary-600">{formatCurrency(booking.priceBreakdown.total)}</span>
          </div>
        </CardContent>
      </Card>

      <div className="mt-8 flex justify-center gap-4">
        <Link to="/my-bookings">
          <Button>View My Bookings</Button>
        </Link>
        <Link to="/hotels">
          <Button variant="outline">Browse Hotels</Button>
        </Link>
      </div>
    </div>
  );
}
