import { Link, useNavigate } from 'react-router-dom';
import { Hotel, LogIn, LogOut, Menu, User, X } from 'lucide-react';
import { useState } from 'react';
import { useAuth } from '@/features/auth/AuthContext';
import { Button } from '@/components/ui';

export function Navbar() {
  const { user, isAuthenticated, logout, hasRole } = useAuth();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);

  function handleLogout() {
    logout();
    navigate('/');
  }

  return (
    <header className="sticky top-0 z-40 border-b border-gray-200 bg-white/95 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        {/* Logo */}
        <Link to="/" className="flex items-center gap-2 text-xl font-bold text-primary-600">
          <Hotel size={28} />
          <span>StayHub</span>
        </Link>

        {/* Desktop nav */}
        <nav className="hidden items-center gap-6 md:flex">
          <Link to="/hotels" className="text-sm font-medium text-gray-600 hover:text-primary-600">
            Browse Hotels
          </Link>
          {isAuthenticated && (
            <>
              <Link to="/my-bookings" className="text-sm font-medium text-gray-600 hover:text-primary-600">
                My Bookings
              </Link>
              {hasRole('Owner') && (
                <Link to="/owner/hotels" className="text-sm font-medium text-gray-600 hover:text-primary-600">
                  My Hotels
                </Link>
              )}
              {hasRole('Admin') && (
                <Link to="/admin" className="text-sm font-medium text-gray-600 hover:text-primary-600">
                  Admin
                </Link>
              )}
            </>
          )}
        </nav>

        {/* Desktop auth buttons */}
        <div className="hidden items-center gap-3 md:flex">
          {isAuthenticated ? (
            <>
              <Link to="/profile" className="flex items-center gap-2 text-sm text-gray-700 hover:text-primary-600">
                <User size={18} />
                <span>{user?.email}</span>
              </Link>
              <Button variant="ghost" size="sm" onClick={handleLogout}>
                <LogOut size={16} className="mr-1" />
                Logout
              </Button>
            </>
          ) : (
            <>
              <Button variant="ghost" size="sm" onClick={() => navigate('/login')}>
                <LogIn size={16} className="mr-1" />
                Sign In
              </Button>
              <Button size="sm" onClick={() => navigate('/register')}>
                Sign Up
              </Button>
            </>
          )}
        </div>

        {/* Mobile hamburger */}
        <button className="rounded-md p-2 md:hidden" onClick={() => setMobileOpen(!mobileOpen)}>
          {mobileOpen ? <X size={24} /> : <Menu size={24} />}
        </button>
      </div>

      {/* Mobile menu */}
      {mobileOpen && (
        <div className="border-t border-gray-200 bg-white px-4 py-4 md:hidden">
          <nav className="flex flex-col gap-3">
            <Link to="/hotels" className="text-sm font-medium text-gray-700" onClick={() => setMobileOpen(false)}>
              Browse Hotels
            </Link>
            {isAuthenticated && (
              <>
                <Link to="/my-bookings" className="text-sm font-medium text-gray-700" onClick={() => setMobileOpen(false)}>
                  My Bookings
                </Link>
                <Link to="/profile" className="text-sm font-medium text-gray-700" onClick={() => setMobileOpen(false)}>
                  Profile
                </Link>
                {hasRole('Owner') && (
                  <Link to="/owner/hotels" className="text-sm font-medium text-gray-700" onClick={() => setMobileOpen(false)}>
                    My Hotels
                  </Link>
                )}
                {hasRole('Admin') && (
                  <Link to="/admin" className="text-sm font-medium text-gray-700" onClick={() => setMobileOpen(false)}>
                    Admin Panel
                  </Link>
                )}
                <button className="text-left text-sm font-medium text-red-600" onClick={handleLogout}>
                  Logout
                </button>
              </>
            )}
            {!isAuthenticated && (
              <>
                <Link to="/login" className="text-sm font-medium text-primary-600" onClick={() => setMobileOpen(false)}>
                  Sign In
                </Link>
                <Link to="/register" className="text-sm font-medium text-primary-600" onClick={() => setMobileOpen(false)}>
                  Sign Up
                </Link>
              </>
            )}
          </nav>
        </div>
      )}
    </header>
  );
}
