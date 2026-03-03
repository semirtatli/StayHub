import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { MapPin, Search, Star } from 'lucide-react';
import { api } from '@/lib/api';
import { formatCurrency } from '@/lib/utils';
import type { HotelSummary, PaginatedResult } from '@/types';
import { Button, Input, Card, CardContent, HotelCardSkeleton } from '@/components/ui';

export function HotelSearchPage() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [city, setCity] = useState(searchParams.get('city') ?? '');
  const [minPrice, setMinPrice] = useState(searchParams.get('minPrice') ?? '');
  const [maxPrice, setMaxPrice] = useState(searchParams.get('maxPrice') ?? '');
  const [minRating, setMinRating] = useState(searchParams.get('minRating') ?? '');
  const [page, setPage] = useState(Number(searchParams.get('page') ?? '1'));

  const queryString = new URLSearchParams();
  if (city) queryString.set('city', city);
  if (minPrice) queryString.set('minPrice', minPrice);
  if (maxPrice) queryString.set('maxPrice', maxPrice);
  if (minRating) queryString.set('minRating', minRating);
  queryString.set('page', String(page));
  queryString.set('pageSize', '12');

  const { data, isLoading } = useQuery<PaginatedResult<HotelSummary>>({
    queryKey: ['hotels', city, minPrice, maxPrice, minRating, page],
    queryFn: () => api.get<PaginatedResult<HotelSummary>>(`/api/hotels?${queryString}`).then((r) => r.data),
  });

  useEffect(() => {
    const params = new URLSearchParams();
    if (city) params.set('city', city);
    if (minPrice) params.set('minPrice', minPrice);
    if (maxPrice) params.set('maxPrice', maxPrice);
    if (minRating) params.set('minRating', minRating);
    if (page > 1) params.set('page', String(page));
    setSearchParams(params, { replace: true });
  }, [city, minPrice, maxPrice, minRating, page, setSearchParams]);

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    setPage(1);
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="text-2xl font-bold text-gray-900">Browse Hotels</h1>

      {/* Filters */}
      <form onSubmit={handleSearch} className="mt-6 flex flex-wrap items-end gap-4 rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
        <div className="flex-1">
          <Input label="City" placeholder="Any city" value={city} onChange={(e) => setCity(e.target.value)} />
        </div>
        <div className="w-32">
          <Input label="Min Price" type="number" placeholder="0" value={minPrice} onChange={(e) => setMinPrice(e.target.value)} />
        </div>
        <div className="w-32">
          <Input label="Max Price" type="number" placeholder="Any" value={maxPrice} onChange={(e) => setMaxPrice(e.target.value)} />
        </div>
        <div className="w-32">
          <Input label="Min Rating" type="number" placeholder="0" min={0} max={5} step={0.5} value={minRating} onChange={(e) => setMinRating(e.target.value)} />
        </div>
        <Button type="submit">
          <Search size={16} className="mr-2" />
          Search
        </Button>
      </form>

      {/* Results */}
      <div className="mt-8">
        {isLoading ? (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <HotelCardSkeleton key={i} />
            ))}
          </div>
        ) : data && data.items.length > 0 ? (
          <>
            <p className="mb-4 text-sm text-gray-500">{data.totalCount} hotel(s) found</p>
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {data.items.map((hotel) => (
                <Link key={hotel.id} to={`/hotels/${hotel.id}`}>
                  <Card hoverable>
                    <div className="aspect-video overflow-hidden rounded-t-lg bg-gray-100">
                      {hotel.primaryPhotoUrl ? (
                        <img src={hotel.primaryPhotoUrl} alt={hotel.name} className="h-full w-full object-cover" />
                      ) : (
                        <div className="flex h-full items-center justify-center text-gray-300">
                          <MapPin size={48} />
                        </div>
                      )}
                    </div>
                    <CardContent>
                      <h3 className="font-semibold text-gray-900">{hotel.name}</h3>
                      <p className="mt-1 text-sm text-gray-500">
                        <MapPin size={14} className="mr-1 inline" />
                        {hotel.city}, {hotel.country}
                      </p>
                      <div className="mt-2 flex items-center justify-between">
                        <span className="flex items-center gap-1 text-sm font-medium text-accent-500">
                          <Star size={14} fill="currentColor" />
                          {hotel.averageRating?.toFixed(1) ?? 'New'}
                        </span>
                        <span className="text-lg font-bold text-primary-600">
                          {formatCurrency(hotel.priceFrom ?? 0)}
                          <span className="text-xs font-normal text-gray-400">/night</span>
                        </span>
                      </div>
                    </CardContent>
                  </Card>
                </Link>
              ))}
            </div>

            {/* Pagination */}
            {data.totalPages > 1 && (
              <div className="mt-8 flex justify-center gap-2">
                <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage(page - 1)}>
                  Previous
                </Button>
                <span className="flex items-center px-3 text-sm text-gray-600">
                  Page {page} of {data.totalPages}
                </span>
                <Button variant="outline" size="sm" disabled={page >= data.totalPages} onClick={() => setPage(page + 1)}>
                  Next
                </Button>
              </div>
            )}
          </>
        ) : (
          <div className="py-16 text-center text-gray-500">
            No hotels found. Try adjusting your filters.
          </div>
        )}
      </div>
    </div>
  );
}
