import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Hotel } from 'lucide-react';
import { api } from '@/lib/api';
import { Button, Input } from '@/components/ui';

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await api.post('/auth/forgot-password', { email });
      setSubmitted(true);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr?.response?.data?.message || 'Something went wrong. Please try again.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center px-4 py-12">
      <div className="w-full max-w-md">
        <div className="text-center">
          <Hotel className="mx-auto text-primary-600" size={40} />
          <h1 className="mt-4 text-2xl font-bold text-gray-900">Reset your password</h1>
          <p className="mt-1 text-sm text-gray-500">
            Enter your email and we'll send you instructions to reset your password.
          </p>
        </div>

        {submitted ? (
          <div className="mt-8">
            <div className="rounded-lg bg-green-50 p-4 text-sm text-green-700">
              If an account exists for <strong>{email}</strong>, you will receive password reset
              instructions shortly. Please check your email.
            </div>
            <p className="mt-6 text-center text-sm text-gray-500">
              <Link to="/login" className="font-medium text-primary-600 hover:text-primary-500">
                Back to sign in
              </Link>
            </p>
          </div>
        ) : (
          <>
            <form onSubmit={handleSubmit} className="mt-8 space-y-5">
              {error && (
                <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</div>
              )}

              <Input
                label="Email"
                type="email"
                required
                autoComplete="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@example.com"
              />

              <Button type="submit" className="w-full" isLoading={loading}>
                Send Reset Instructions
              </Button>
            </form>

            <p className="mt-6 text-center text-sm text-gray-500">
              Remember your password?{' '}
              <Link to="/login" className="font-medium text-primary-600 hover:text-primary-500">
                Sign in
              </Link>
            </p>
          </>
        )}
      </div>
    </div>
  );
}
