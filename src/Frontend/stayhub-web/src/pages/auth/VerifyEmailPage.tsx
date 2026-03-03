import { useLocation, Link } from 'react-router-dom';
import { MailCheck } from 'lucide-react';
import { Button } from '@/components/ui';

export function VerifyEmailPage() {
  const location = useLocation();
  const email = (location.state as { email?: string })?.email;

  return (
    <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center px-4 py-12">
      <div className="w-full max-w-md text-center">
        <MailCheck className="mx-auto text-primary-600" size={56} />
        <h1 className="mt-6 text-2xl font-bold text-gray-900">Check your email</h1>
        <p className="mt-3 text-gray-600">
          We've sent a verification link to{' '}
          {email ? <strong>{email}</strong> : 'your email'}. Please click the link to verify your
          account.
        </p>
        <p className="mt-4 text-sm text-gray-500">
          Didn't receive the email? Check your spam folder or try registering again.
        </p>
        <Link to="/login">
          <Button variant="outline" className="mt-8">
            Go to Sign In
          </Button>
        </Link>
      </div>
    </div>
  );
}
