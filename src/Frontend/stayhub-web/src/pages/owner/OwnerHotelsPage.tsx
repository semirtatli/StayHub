import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Hotel, MapPin, Plus, Star } from 'lucide-react';
import { api } from '@/lib/api';
import type { HotelSummary } from '@/types';
import { Button, Card, CardContent, HotelCardSkeleton } from '@/components/ui';

export function OwnerHotelsPage() {
  const { data: hotels, isLoading } = useQuery<HotelSummary[]>({
    queryKey: ['owner-hotels'],
    queryFn: () => api.get<HotelSummary[]>('/hotels/my').then((r) => r.data),
  });

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">My Hotels</h1>
        <Link to="/owner/hotels/new">
          <Button>
            <Plus size={16} className="mr-2" />
            Add Hotel
          </Button>
        </Link>
      </div>

      {isLoading ? (
        <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <HotelCardSkeleton key={i} />
          ))}
        </div>
      ) : hotels && hotels.length > 0 ? (
        <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {hotels.map((hotel) => (
            <Link key={hotel.id} to={`/owner/hotels/${hotel.id}/edit`}>
              <Card hoverable>
                <div className="aspect-video overflow-hidden rounded-t-lg bg-gray-100">
                  {hotel.coverImageUrl ? (
                    <img src={hotel.coverImageUrl} alt={hotel.name} className="h-full w-full object-cover" />
                  ) : (
                    <div className="flex h-full items-center justify-center text-gray-300">
                      <Hotel size={48} />
                    </div>
                  )}
                </div>
                <CardContent>
                  <h3 className="font-semibold text-gray-900">{hotel.name}</h3>
                  <p className="mt-1 flex items-center gap-1 text-sm text-gray-500">
                    <MapPin size={14} />
                    {hotel.city}, {hotel.country}
                  </p>
                  <div className="mt-2 flex items-center gap-4 text-sm">
                    <span className="flex items-center gap-1 font-medium text-accent-500">
                      <Star size={14} fill="currentColor" />
                      {hotel.starRating} stars
                    </span>
                    <span className="text-gray-400">•</span>
                    <span className="text-gray-500">{hotel.roomCount ?? 0} rooms</span>
                  </div>
                </CardContent>
              </Card>
            </Link>
          ))}
        </div>
      ) : (
        <div className="mt-16 text-center text-gray-500">
          <Hotel className="mx-auto" size={48} />
          <p className="mt-4">You haven't listed any hotels yet.</p>
          <Link to="/owner/hotels/new">
            <Button className="mt-4">List Your First Hotel</Button>
          </Link>
        </div>
      )}
    </div>
  );
}
