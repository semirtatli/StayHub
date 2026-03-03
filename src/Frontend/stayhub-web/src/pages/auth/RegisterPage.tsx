import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Hotel } from 'lucide-react';
import { useAuth } from '@/features/auth/AuthContext';
import { Button, Input } from '@/components/ui';

export function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  function update(field: string, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');

    if (form.password !== form.confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    setLoading(true);
    try {
      await register({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        password: form.password,
        confirmPassword: form.confirmPassword,
        role: 'Guest',
      });
      navigate('/');
    } catch (err: unknown) {
      // Extract real validation/error messages from the API response
      interface ErrorItem { field?: string; message: string }
      const axiosErr = err as { response?: { data?: { message?: string; errors?: ErrorItem[] | Record<string, string[]> } } };
      const data = axiosErr?.response?.data;

      if (data?.errors && Array.isArray(data.errors)) {
        // Our custom error format: { errors: [{ field, message }] }
        const messages = (data.errors as ErrorItem[]).map((e) => e.message);
        setError(messages.join(' '));
      } else if (data?.errors && typeof data.errors === 'object') {
        // ASP.NET validation format: { errors: { Field: ["msg"] } }
        const messages = Object.values(data.errors as Record<string, string[]>).flat();
        setError(messages.join(' '));
      } else if (data?.message) {
        setError(data.message);
      } else {
        setError('Registration failed. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-[calc(100vh-4rem)] items-center justify-center px-4 py-12">
      <div className="w-full max-w-md">
        <div className="text-center">
          <Hotel className="mx-auto text-primary-600" size={40} />
          <h1 className="mt-4 text-2xl font-bold text-gray-900">Create an account</h1>
          <p className="mt-1 text-sm text-gray-500">Join StayHub to start booking hotels</p>
        </div>

        <form onSubmit={handleSubmit} className="mt-8 space-y-5">
          {error && (
            <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</div>
          )}

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="First Name"
              required
              value={form.firstName}
              onChange={(e) => update('firstName', e.target.value)}
              placeholder="John"
            />
            <Input
              label="Last Name"
              required
              value={form.lastName}
              onChange={(e) => update('lastName', e.target.value)}
              placeholder="Doe"
            />
          </div>

          <Input
            label="Email"
            type="email"
            required
            autoComplete="email"
            value={form.email}
            onChange={(e) => update('email', e.target.value)}
            placeholder="you@example.com"
          />

          <Input
            label="Password"
            type="password"
            required
            autoComplete="new-password"
            value={form.password}
            onChange={(e) => update('password', e.target.value)}
            placeholder="••••••••"
          />

          <Input
            label="Confirm Password"
            type="password"
            required
            value={form.confirmPassword}
            onChange={(e) => update('confirmPassword', e.target.value)}
            placeholder="••••••••"
          />

          <Button type="submit" className="w-full" isLoading={loading}>
            Create Account
          </Button>
        </form>

        <p className="mt-6 text-center text-sm text-gray-500">
          Already have an account?{' '}
          <Link to="/login" className="font-medium text-primary-600 hover:text-primary-500">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
