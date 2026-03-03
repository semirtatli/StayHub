import { Link } from 'react-router-dom';
import { SearchX } from 'lucide-react';
import { Button } from '@/components/ui';

export function NotFoundPage() {
  return (
    <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center px-4">
      <div className="text-center">
        <SearchX className="mx-auto text-gray-300" size={64} />
        <h1 className="mt-4 text-4xl font-bold text-gray-900">404</h1>
        <p className="mt-2 text-lg text-gray-500">Page not found</p>
        <p className="mt-1 text-sm text-gray-400">
          The page you're looking for doesn't exist or has been moved.
        </p>
        <Link to="/">
          <Button className="mt-6">Go Home</Button>
        </Link>
      </div>
    </div>
  );
}
