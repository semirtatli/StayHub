import { Link } from 'react-router-dom';
import { ShieldX } from 'lucide-react';
import { Button } from '@/components/ui';

export function ForbiddenPage() {
  return (
    <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center px-4">
      <div className="text-center">
        <ShieldX className="mx-auto text-red-300" size={64} />
        <h1 className="mt-4 text-4xl font-bold text-gray-900">403</h1>
        <p className="mt-2 text-lg text-gray-500">Access Forbidden</p>
        <p className="mt-1 text-sm text-gray-400">
          You don't have permission to access this page.
        </p>
        <Link to="/">
          <Button className="mt-6">Go Home</Button>
        </Link>
      </div>
    </div>
  );
}
