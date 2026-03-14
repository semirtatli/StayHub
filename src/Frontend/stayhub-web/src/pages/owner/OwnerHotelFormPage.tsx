import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { api } from '@/lib/api';
import type { Hotel } from '@/types';
import { Button, Input, Card, CardContent } from '@/components/ui';

export function OwnerHotelFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;
  const navigate = useNavigate();

  const [form, setForm] = useState({
    name: '',
    description: '',
    street: '',
    city: '',
    state: '',
    country: '',
    zipCode: '',
    starRating: 3,
    email: '',
    phone: '',
    website: '',
    checkInTime: '15:00',
    checkOutTime: '11:00',
  });

  const { data: hotel } = useQuery<Hotel>({
    queryKey: ['hotel', id],
    queryFn: () => api.get<Hotel>(`/hotels/${id}`).then((r) => r.data),
    enabled: isEdit,
  });

  useEffect(() => {
    if (hotel) {
      setForm({
        name: hotel.name,
        description: hotel.description ?? '',
        street: hotel.address.street,
        city: hotel.address.city,
        state: hotel.address.state ?? '',
        country: hotel.address.country,
        zipCode: hotel.address.zipCode ?? '',
        starRating: hotel.starRating,
        email: hotel.contactInfo.email ?? '',
        phone: hotel.contactInfo.phone ?? '',
        website: hotel.contactInfo.website ?? '',
        checkInTime: hotel.checkInTime ?? '15:00',
        checkOutTime: hotel.checkOutTime ?? '11:00',
      });
    }
  }, [hotel]);

  const saveMutation = useMutation({
    mutationFn: (data: typeof form) => {
      if (isEdit) return api.put<Hotel>(`/hotels/${id}`, data).then((r) => r.data);
      return api.post<Hotel>('/hotels', data).then((r) => r.data);
    },
    onSuccess: () => {
      toast.success(isEdit ? 'Hotel updated.' : 'Hotel created.');
      navigate('/owner/hotels');
    },
    onError: () => toast.error('Failed to save hotel.'),
  });

  function update(field: string, value: string | number) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    saveMutation.mutate(form);
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6">
      <h1 className="text-2xl font-bold text-gray-900">
        {isEdit ? 'Edit Hotel' : 'Add New Hotel'}
      </h1>

      <form onSubmit={handleSubmit} className="mt-8 space-y-6">
        <Card>
          <CardContent>
            <h2 className="text-lg font-semibold text-gray-900">Basic Information</h2>
            <div className="mt-4 space-y-4">
              <Input label="Hotel Name" required value={form.name} onChange={(e) => update('name', e.target.value)} placeholder="Grand Hotel" />
              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">Description</label>
                <textarea
                  rows={4}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:ring-1 focus:ring-primary-500"
                  value={form.description}
                  onChange={(e) => update('description', e.target.value)}
                  placeholder="Describe your hotel…"
                />
              </div>
              <Input
                label="Star Rating"
                type="number"
                min={1}
                max={5}
                required
                value={form.starRating}
                onChange={(e) => update('starRating', Number(e.target.value))}
              />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <h2 className="text-lg font-semibold text-gray-900">Location</h2>
            <div className="mt-4 space-y-4">
              <Input label="Street" required value={form.street} onChange={(e) => update('street', e.target.value)} placeholder="123 Main St" />
              <div className="grid grid-cols-2 gap-4">
                <Input label="City" required value={form.city} onChange={(e) => update('city', e.target.value)} placeholder="New York" />
                <Input label="State" value={form.state} onChange={(e) => update('state', e.target.value)} placeholder="NY" />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <Input label="Country" required value={form.country} onChange={(e) => update('country', e.target.value)} placeholder="USA" />
                <Input label="Zip Code" value={form.zipCode} onChange={(e) => update('zipCode', e.target.value)} placeholder="10001" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <h2 className="text-lg font-semibold text-gray-900">Contact & Hours</h2>
            <div className="mt-4 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <Input label="Email" type="email" value={form.email} onChange={(e) => update('email', e.target.value)} placeholder="info@hotel.com" />
                <Input label="Phone" value={form.phone} onChange={(e) => update('phone', e.target.value)} placeholder="+1 234 567" />
              </div>
              <Input label="Website" value={form.website} onChange={(e) => update('website', e.target.value)} placeholder="https://hotel.com" />
              <div className="grid grid-cols-2 gap-4">
                <Input label="Check-in Time" type="time" value={form.checkInTime} onChange={(e) => update('checkInTime', e.target.value)} />
                <Input label="Check-out Time" type="time" value={form.checkOutTime} onChange={(e) => update('checkOutTime', e.target.value)} />
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex justify-end gap-3">
          <Button variant="ghost" type="button" onClick={() => navigate('/owner/hotels')}>
            Cancel
          </Button>
          <Button type="submit" isLoading={saveMutation.isPending}>
            {isEdit ? 'Save Changes' : 'Create Hotel'}
          </Button>
        </div>
      </form>
    </div>
  );
}
