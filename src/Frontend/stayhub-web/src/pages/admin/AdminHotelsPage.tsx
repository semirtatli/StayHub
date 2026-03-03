import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { MapPin, Star, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';
import { api } from '@/lib/api';
import type { HotelSummary } from '@/types';
import { Button, Input, Skeleton } from '@/components/ui';

export function AdminHotelsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');

  const { data: hotels, isLoading } = useQuery<HotelSummary[]>({
    queryKey: ['admin-hotels'],
    queryFn: () => api.get<HotelSummary[]>('/api/hotels/all').then((r) => r.data),
  });

  const deleteHotel = useMutation({
    mutationFn: (hotelId: string) => api.delete(`/api/hotels/${hotelId}`),
    onSuccess: () => {
      toast.success('Hotel deleted.');
      queryClient.invalidateQueries({ queryKey: ['admin-hotels'] });
    },
    onError: () => toast.error('Failed to delete hotel.'),
  });

  const filtered = hotels?.filter(
    (h) =>
      h.name.toLowerCase().includes(search.toLowerCase()) ||
      h.city.toLowerCase().includes(search.toLowerCase()),
  );

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900">Hotels</h1>
      <p className="mt-1 text-sm text-gray-500">View and manage all hotels on the platform</p>

      <div className="mt-6 max-w-sm">
        <Input placeholder="Search by name or city…" value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>

      {isLoading ? (
        <div className="mt-6 space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-16 w-full rounded-lg" />
          ))}
        </div>
      ) : (
        <div className="mt-6 overflow-hidden rounded-lg border border-gray-200">
          <table className="w-full text-left text-sm">
            <thead className="bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="px-4 py-3">Hotel</th>
                <th className="px-4 py-3">Location</th>
                <th className="px-4 py-3">Rating</th>
                <th className="px-4 py-3">Rooms</th>
                <th className="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filtered?.map((hotel) => (
                <tr key={hotel.id}>
                  <td className="whitespace-nowrap px-4 py-3 font-medium text-gray-900">{hotel.name}</td>
                  <td className="px-4 py-3 text-gray-500">
                    <span className="flex items-center gap-1">
                      <MapPin size={14} />
                      {hotel.city}, {hotel.country}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className="flex items-center gap-1 text-accent-500">
                      <Star size={14} fill="currentColor" />
                      {hotel.averageRating?.toFixed(1) ?? 'N/A'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-500">{hotel.totalRooms ?? 0}</td>
                  <td className="px-4 py-3">
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-red-600 hover:text-red-700"
                      onClick={() => {
                        if (window.confirm('Are you sure you want to delete this hotel?')) {
                          deleteHotel.mutate(hotel.id);
                        }
                      }}
                    >
                      <Trash2 size={14} />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {filtered?.length === 0 && (
            <div className="py-8 text-center text-sm text-gray-500">No hotels found.</div>
          )}
        </div>
      )}
    </div>
  );
}
