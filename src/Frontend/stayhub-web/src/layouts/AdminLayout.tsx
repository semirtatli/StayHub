import { Link, Outlet, useLocation } from 'react-router-dom';
import { BarChart3, Hotel, LayoutDashboard, Users } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Navbar } from '@/components/layout/Navbar';

const adminLinks = [
  { to: '/admin', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/admin/users', label: 'Users', icon: Users },
  { to: '/admin/hotels', label: 'Hotels', icon: Hotel },
];

/**
 * Admin layout with sidebar navigation.
 */
export function AdminLayout() {
  const location = useLocation();

  return (
    <div className="flex min-h-screen flex-col">
      <Navbar />
      <div className="flex flex-1">
        {/* Sidebar */}
        <aside className="hidden w-64 border-r border-gray-200 bg-white px-4 py-6 md:block">
          <h2 className="mb-4 flex items-center gap-2 px-3 text-sm font-semibold text-gray-500 uppercase">
            <BarChart3 size={16} />
            Admin Panel
          </h2>
          <nav className="space-y-1">
            {adminLinks.map(({ to, label, icon: Icon }) => (
              <Link
                key={to}
                to={to}
                className={cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  location.pathname === to
                    ? 'bg-primary-50 text-primary-700'
                    : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900',
                )}
              >
                <Icon size={18} />
                {label}
              </Link>
            ))}
          </nav>
        </aside>

        {/* Main content */}
        <main className="flex-1 bg-gray-50 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
