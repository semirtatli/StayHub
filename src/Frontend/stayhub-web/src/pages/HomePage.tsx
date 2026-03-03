import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Hotel, MapPin, Search, Star } from 'lucide-react';
import { Button } from '@/components/ui';

export function HomePage() {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    const params = new URLSearchParams();
    if (searchQuery) params.set('city', searchQuery);
    navigate(`/hotels?${params.toString()}`);
  }

  return (
    <div>
      {/* Hero */}
      <section className="relative bg-gradient-to-br from-primary-600 to-primary-800 py-24 text-white">
        <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
          <h1 className="text-4xl font-bold tracking-tight sm:text-5xl lg:text-6xl">
            Find Your Perfect Stay
          </h1>
          <p className="mx-auto mt-4 max-w-2xl text-lg text-primary-100">
            Discover amazing hotels at the best prices. Book with confidence and enjoy unforgettable experiences.
          </p>

          {/* Search Bar */}
          <form onSubmit={handleSearch} className="mx-auto mt-8 flex max-w-xl gap-3">
            <div className="relative flex-1">
              <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={20} />
              <input
                type="text"
                placeholder="Where are you going?"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full rounded-lg border-0 py-3 pl-10 pr-4 text-gray-900 shadow-lg focus:ring-2 focus:ring-accent-500"
              />
            </div>
            <Button type="submit" className="shadow-lg">
              <Search size={18} className="mr-2" />
              Search
            </Button>
          </form>
        </div>
      </section>

      {/* Features */}
      <section className="py-20">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <h2 className="text-center text-3xl font-bold text-gray-900">Why Choose StayHub?</h2>
          <div className="mt-12 grid grid-cols-1 gap-8 md:grid-cols-3">
            {[
              {
                icon: Hotel,
                title: 'Wide Selection',
                description: 'Choose from thousands of hotels worldwide, from budget-friendly to luxury stays.',
              },
              {
                icon: Star,
                title: 'Verified Reviews',
                description: 'Read honest reviews from real guests to make the best decision for your trip.',
              },
              {
                icon: Search,
                title: 'Best Prices',
                description: 'Get the best rates with our price guarantee. No hidden fees, no surprises.',
              },
            ].map(({ icon: Icon, title, description }) => (
              <div key={title} className="rounded-xl border border-gray-200 bg-white p-8 text-center shadow-sm">
                <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-primary-100">
                  <Icon size={28} className="text-primary-600" />
                </div>
                <h3 className="mt-4 text-lg font-semibold text-gray-900">{title}</h3>
                <p className="mt-2 text-sm text-gray-500">{description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-gray-50 py-16">
        <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
          <h2 className="text-2xl font-bold text-gray-900">Own a Hotel?</h2>
          <p className="mt-2 text-gray-600">List your property on StayHub and reach millions of travelers.</p>
          <Button className="mt-6" onClick={() => navigate('/register')}>
            Get Started
          </Button>
        </div>
      </section>
    </div>
  );
}
