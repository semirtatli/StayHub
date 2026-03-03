import { useQuery } from '@tanstack/react-query';
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from 'recharts';
import { DollarSign, Hotel, TrendingUp, Users } from 'lucide-react';
import { api } from '@/lib/api';
import { formatCurrency } from '@/lib/utils';
import type { DashboardKpi, RevenueDataPoint } from '@/types';
import { Card, CardContent, Skeleton } from '@/components/ui';

export function AdminDashboardPage() {
  const { data: kpis, isLoading: kpisLoading } = useQuery<DashboardKpi>({
    queryKey: ['admin-kpis'],
    queryFn: () => api.get<DashboardKpi>('/analytics/dashboard').then((r) => r.data),
  });

  const { data: revenue, isLoading: revenueLoading } = useQuery<RevenueDataPoint[]>({
    queryKey: ['admin-revenue'],
    queryFn: () => api.get<RevenueDataPoint[]>('/analytics/revenue?period=Monthly&months=12').then((r) => r.data),
  });

  const stats = [
    { label: 'Total Revenue', value: formatCurrency(kpis?.totalRevenue ?? 0), icon: DollarSign, color: 'bg-green-100 text-green-600' },
    { label: 'Total Bookings', value: kpis?.totalBookings ?? 0, icon: TrendingUp, color: 'bg-blue-100 text-blue-600' },
    { label: 'Active Hotels', value: kpis?.activeHotels ?? 0, icon: Hotel, color: 'bg-purple-100 text-purple-600' },
    { label: 'Avg Rating', value: kpis?.averageRating?.toFixed(1) ?? 'N/A', icon: Users, color: 'bg-amber-100 text-amber-600' },
  ];

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
      <p className="mt-1 text-sm text-gray-500">Overview of your platform performance</p>

      {/* KPI Cards */}
      <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {kpisLoading
          ? Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-28 rounded-xl" />
            ))
          : stats.map(({ label, value, icon: Icon, color }) => (
              <Card key={label}>
                <CardContent className="flex items-center gap-4">
                  <div className={`flex h-12 w-12 items-center justify-center rounded-lg ${color}`}>
                    <Icon size={24} />
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">{label}</p>
                    <p className="text-2xl font-bold text-gray-900">{value}</p>
                  </div>
                </CardContent>
              </Card>
            ))}
      </div>

      {/* Charts */}
      <div className="mt-8 grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Revenue Chart */}
        <Card>
          <CardContent>
            <h2 className="text-lg font-semibold text-gray-900">Monthly Revenue</h2>
            {revenueLoading ? (
              <Skeleton className="mt-4 h-64 w-full" />
            ) : (
              <div className="mt-4 h-64">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={revenue}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" fontSize={12} />
                    <YAxis fontSize={12} />
                    <Tooltip formatter={(value) => formatCurrency(value as number)} />
                    <Line type="monotone" dataKey="revenue" stroke="#2563eb" strokeWidth={2} dot={false} />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Bookings Chart */}
        <Card>
          <CardContent>
            <h2 className="text-lg font-semibold text-gray-900">Bookings per Month</h2>
            {revenueLoading ? (
              <Skeleton className="mt-4 h-64 w-full" />
            ) : (
              <div className="mt-4 h-64">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={revenue}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" fontSize={12} />
                    <YAxis fontSize={12} />
                    <Tooltip />
                    <Bar dataKey="bookingCount" fill="#f59e0b" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
