import { useState } from 'react';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import { Hotel } from 'lucide-react';
import { api } from '@/lib/api';
import { Button, Input } from '@/components/ui';

function validatePassword(value: string): string | undefined {
  if (!value) return 'Password is required.';
  if (value.length < 8) return 'Password must be at least 8 characters long.';
  if (!/[A-Z]/.test(value)) return 'Password must contain at least one uppercase letter.';
  if (!/[a-z]/.test(value)) return 'Password must contain at least one lowercase letter.';
  if (!/[0-9]/.test(value)) return 'Password must contain at least one digit.';
  if (!/[^a-zA-Z0-9]/.test(value)) return 'Password must contain at least one special character.';
  return undefined;
}

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const email = searchParams.get('email') ?? '';
  const token = searchParams.get('token') ?? '';

  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const missingParams = !email || !token;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');

    const passwordError = validatePassword(password);
    if (passwordError) {
      setError(passwordError);
      return;
    }

    if (password !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    setLoading(true);

    try {
      await api.post('/auth/reset-password', {
        email,
        token,
        newPassword: password,
      });
      setSuccess(true);
      setTimeout(() => navigate('/login'), 3000);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(
        axiosErr?.response?.data?.message ||
          'Password reset failed. The link may have expired. Please request a new one.',
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center px-4 py-12">
      <div className="w-full max-w-md">
        <div className="text-center">
          <Hotel className="mx-auto text-primary-600" size={40} />
          <h1 className="mt-4 text-2xl font-bold text-gray-900">Set new password</h1>
          <p className="mt-1 text-sm text-gray-500">Enter your new password below.</p>
        </div>

        {missingParams ? (
          <div className="mt-8">
            <div className="rounded-lg bg-red-50 p-4 text-sm text-red-700">
              Invalid password reset link. Please request a new one.
            </div>
            <p className="mt-6 text-center text-sm text-gray-500">
              <Link
                to="/forgot-password"
                className="font-medium text-primary-600 hover:text-primary-500"
              >
                Request new reset link
              </Link>
            </p>
          </div>
        ) : success ? (
          <div className="mt-8">
            <div className="rounded-lg bg-green-50 p-4 text-sm text-green-700">
              Your password has been reset successfully. Redirecting to sign in...
            </div>
            <p className="mt-6 text-center text-sm text-gray-500">
              <Link to="/login" className="font-medium text-primary-600 hover:text-primary-500">
                Sign in now
              </Link>
            </p>
          </div>
        ) : (
          <>
            <form onSubmit={handleSubmit} className="mt-8 space-y-5" noValidate>
              {error && (
                <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</div>
              )}

              <Input
                label="New Password"
                type="password"
                required
                autoComplete="new-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
              />
              {/* Password strength hints */}
              <ul className="-mt-3 ml-1 space-y-0.5 text-xs text-gray-400">
                <li className={password.length >= 8 ? 'text-green-600' : ''}>
                  At least 8 characters
                </li>
                <li className={/[A-Z]/.test(password) ? 'text-green-600' : ''}>
                  One uppercase letter
                </li>
                <li className={/[a-z]/.test(password) ? 'text-green-600' : ''}>
                  One lowercase letter
                </li>
                <li className={/[0-9]/.test(password) ? 'text-green-600' : ''}>One digit</li>
                <li className={/[^a-zA-Z0-9]/.test(password) ? 'text-green-600' : ''}>
                  One special character
                </li>
              </ul>

              <Input
                label="Confirm New Password"
                type="password"
                required
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="••••••••"
              />

              <Button type="submit" className="w-full" isLoading={loading}>
                Reset Password
              </Button>
            </form>

            <p className="mt-6 text-center text-sm text-gray-500">
              <Link to="/login" className="font-medium text-primary-600 hover:text-primary-500">
                Back to sign in
              </Link>
            </p>
          </>
        )}
      </div>
    </div>
  );
}
