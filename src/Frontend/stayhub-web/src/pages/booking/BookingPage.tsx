import { useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { CalendarDays, CreditCard, Users } from 'lucide-react';
import toast from 'react-hot-toast';
import { api } from '@/lib/api';
import { formatCurrency, calculateNights } from '@/lib/utils';
import { useAuth } from '@/features/auth/AuthContext';
import type { Booking, Hotel, Room } from '@/types';
import { Button, Input, Card, CardContent } from '@/components/ui';

export function BookingPage() {
  const { hotelId } = useParams<{ hotelId: string }>();
  const [searchParams] = useSearchParams();
  const roomIdParam = searchParams.get('roomId');
  const navigate = useNavigate();
  const { user } = useAuth();

  const [checkIn, setCheckIn] = useState('');
  const [checkOut, setCheckOut] = useState('');
  const [selectedRoomId, setSelectedRoomId] = useState(roomIdParam ?? '');
  const [guestCount, setGuestCount] = useState(1);
  const [firstName, setFirstName] = useState(user?.firstName ?? '');
  const [lastName, setLastName] = useState(user?.lastName ?? '');
  const [email, setEmail] = useState(user?.email ?? '');
  const [phone, setPhone] = useState('');
  const [specialRequests, setSpecialRequests] = useState('');

  const { data: hotel } = useQuery<Hotel>({
    queryKey: ['hotel', hotelId],
    queryFn: () => api.get<Hotel>(`/hotels/${hotelId}`).then((r) => r.data),
    enabled: !!hotelId,
  });

  const { data: rooms } = useQuery<Room[]>({
    queryKey: ['hotel-rooms', hotelId],
    queryFn: () => api.get<Room[]>(`/hotels/${hotelId}/rooms`).then((r) => r.data),
    enabled: !!hotelId,
  });

  const selectedRoom = rooms?.find((r) => r.id === selectedRoomId);
  const nights = checkIn && checkOut ? calculateNights(checkIn, checkOut) : 0;
  const totalPrice = selectedRoom ? selectedRoom.basePrice * Math.max(nights, 0) : 0;

  const createBooking = useMutation({
    mutationFn: (data: {
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
    }) => api.post<Booking>('/bookings', data).then((r) => r.data),
    onSuccess: (data) => {
      toast.success('Booking created successfully!');
      navigate(`/booking/confirmation/${data.id}`);
    },
    onError: () => toast.error('Failed to create booking. Please try again.'),
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedRoomId || !checkIn || !checkOut) {
      toast.error('Please fill in all required fields.');
      return;
    }
    if (nights <= 0) {
      toast.error('Check-out date must be after check-in date.');
      return;
    }
    if (!firstName.trim() || !lastName.trim() || !email.trim()) {
      toast.error('Please fill in your contact information.');
      return;
    }
    createBooking.mutate({
      hotelId: hotelId!,
      roomId: selectedRoomId,
      checkIn,
      checkOut,
      numberOfGuests: guestCount,
      firstName: firstName.trim(),
      lastName: lastName.trim(),
      email: email.trim(),
      phone: phone.trim() || undefined,
      specialRequests: specialRequests.trim() || undefined,
    });
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="text-2xl font-bold text-gray-900">Complete Your Booking</h1>
      {hotel && (
        <p className="mt-1 text-gray-500">{hotel.name} — {hotel.address.city}, {hotel.address.country}</p>
      )}

      <form onSubmit={handleSubmit} className="mt-8 grid grid-cols-1 gap-8 lg:grid-cols-3">
        {/* Booking Form */}
        <div className="space-y-6 lg:col-span-2">
          {/* Dates */}
          <Card>
            <CardContent>
              <h2 className="flex items-center gap-2 text-lg font-semibold text-gray-900">
                <CalendarDays size={20} /> Select Dates
              </h2>
              <div className="mt-4 grid grid-cols-2 gap-4">
                <Input
                  label="Check-in"
                  type="date"
                  required
                  value={checkIn}
                  onChange={(e) => setCheckIn(e.target.value)}
                  min={new Date().toISOString().split('T')[0]}
                />
                <Input
                  label="Check-out"
                  type="date"
                  required
                  value={checkOut}
                  onChange={(e) => setCheckOut(e.target.value)}
                  min={checkIn || new Date().toISOString().split('T')[0]}
                />
              </div>
            </CardContent>
          </Card>

          {/* Room Selection */}
          <Card>
            <CardContent>
              <h2 className="flex items-center gap-2 text-lg font-semibold text-gray-900">
                <Users size={20} /> Select Room
              </h2>
              {rooms && rooms.length > 0 ? (
                <div className="mt-4 space-y-3">
                  {rooms.map((room) => (
                    <label
                      key={room.id}
                      className={`flex cursor-pointer items-center justify-between rounded-lg border p-4 transition-colors ${
                        selectedRoomId === room.id
                          ? 'border-primary-500 bg-primary-50'
                          : 'border-gray-200 hover:border-gray-300'
                      }`}
                    >
                      <div className="flex items-center gap-3">
                        <input
                          type="radio"
                          name="room"
                          value={room.id}
                          checked={selectedRoomId === room.id}
                          onChange={() => setSelectedRoomId(room.id)}
                          className="text-primary-600"
                        />
                        <div>
                          <span className="font-medium text-gray-900">{room.roomType}</span>
                          <span className="ml-2 text-sm text-gray-500">({room.maxOccupancy} guests)</span>
                        </div>
                      </div>
                      <span className="font-semibold text-primary-600">
                        {formatCurrency(room.basePrice)}/night
                      </span>
                    </label>
                  ))}
                </div>
              ) : (
                <p className="mt-4 text-sm text-gray-500">Loading rooms…</p>
              )}

              <div className="mt-4">
                <Input
                  label="Number of Guests"
                  type="number"
                  min={1}
                  max={selectedRoom?.maxOccupancy ?? 10}
                  value={guestCount}
                  onChange={(e) => setGuestCount(Number(e.target.value))}
                />
              </div>
            </CardContent>
          </Card>

          {/* Guest Information */}
          <Card>
            <CardContent>
              <h2 className="text-lg font-semibold text-gray-900">Guest Information</h2>
              <div className="mt-4 space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <Input
                    label="First Name"
                    required
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    placeholder="John"
                  />
                  <Input
                    label="Last Name"
                    required
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    placeholder="Doe"
                  />
                </div>
                <Input
                  label="Email"
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@example.com"
                />
                <Input
                  label="Phone (optional)"
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  placeholder="+1 234 567 890"
                />
                <div>
                  <label className="mb-1 block text-sm font-medium text-gray-700">Special Requests (optional)</label>
                  <textarea
                    rows={3}
                    className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:ring-1 focus:ring-primary-500"
                    value={specialRequests}
                    onChange={(e) => setSpecialRequests(e.target.value)}
                    placeholder="Any special requests…"
                  />
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Summary Sidebar */}
        <div>
          <div className="sticky top-24 rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
            <h3 className="flex items-center gap-2 text-lg font-semibold text-gray-900">
              <CreditCard size={20} /> Booking Summary
            </h3>
            <div className="mt-4 space-y-2 text-sm text-gray-600">
              {selectedRoom && <p>Room: {selectedRoom.roomType}</p>}
              {nights > 0 && <p>Duration: {nights} night(s)</p>}
              {selectedRoom && nights > 0 && (
                <p>Rate: {formatCurrency(selectedRoom.basePrice)} × {nights}</p>
              )}
            </div>
            <div className="mt-4 border-t pt-4">
              <div className="flex items-center justify-between text-lg font-bold">
                <span>Total</span>
                <span className="text-primary-600">{formatCurrency(totalPrice)}</span>
              </div>
            </div>
            <Button
              type="submit"
              className="mt-6 w-full"
              isLoading={createBooking.isPending}
              disabled={!selectedRoomId || nights <= 0}
            >
              Confirm Booking
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
}
