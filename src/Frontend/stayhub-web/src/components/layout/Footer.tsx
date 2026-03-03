import { Hotel } from 'lucide-react';
import { Link } from 'react-router-dom';

export function Footer() {
  return (
    <footer className="border-t border-gray-200 bg-white">
      <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 gap-8 md:grid-cols-4">
          {/* Brand */}
          <div>
            <Link to="/" className="flex items-center gap-2 text-lg font-bold text-primary-600">
              <Hotel size={24} />
              <span>StayHub</span>
            </Link>
            <p className="mt-3 text-sm text-gray-500">
              Find and book your perfect stay. Quality hotels at the best prices worldwide.
            </p>
          </div>

          {/* Links */}
          <div>
            <h3 className="text-sm font-semibold text-gray-900">Quick Links</h3>
            <ul className="mt-3 space-y-2 text-sm text-gray-500">
              <li><Link to="/hotels" className="hover:text-primary-600">Browse Hotels</Link></li>
              <li><Link to="/register" className="hover:text-primary-600">Sign Up</Link></li>
              <li><Link to="/login" className="hover:text-primary-600">Sign In</Link></li>
            </ul>
          </div>

          <div>
            <h3 className="text-sm font-semibold text-gray-900">For Owners</h3>
            <ul className="mt-3 space-y-2 text-sm text-gray-500">
              <li><Link to="/owner/hotels" className="hover:text-primary-600">List Your Property</Link></li>
              <li><Link to="/owner/bookings" className="hover:text-primary-600">Manage Bookings</Link></li>
            </ul>
          </div>

          <div>
            <h3 className="text-sm font-semibold text-gray-900">Support</h3>
            <ul className="mt-3 space-y-2 text-sm text-gray-500">
              <li><span className="cursor-default">Help Center</span></li>
              <li><span className="cursor-default">Contact Us</span></li>
              <li><span className="cursor-default">Privacy Policy</span></li>
            </ul>
          </div>
        </div>

        <div className="mt-8 border-t border-gray-200 pt-8 text-center text-sm text-gray-400">
          &copy; {new Date().getFullYear()} StayHub. All rights reserved.
        </div>
      </div>
    </footer>
  );
}
