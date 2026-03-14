import { useState, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Hotel } from 'lucide-react';
import { useAuth } from '@/features/auth/AuthContext';
import { Button, Input } from '@/components/ui';

/* ── Validation helpers (mirrors backend FluentValidation rules) ── */

interface FieldErrors {
  firstName?: string;
  lastName?: string;
  email?: string;
  password?: string;
  confirmPassword?: string;
}

const NAME_REGEX = /^[\p{L}\s\-']+$/u;
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function validateFirstName(value: string): string | undefined {
  if (!value.trim()) return 'First name is required.';
  if (value.trim().length < 2) return 'First name must be at least 2 characters.';
  if (value.trim().length > 100) return 'First name must not exceed 100 characters.';
  if (!NAME_REGEX.test(value.trim()))
    return 'First name can only contain letters, spaces, hyphens, and apostrophes.';
  return undefined;
}

function validateLastName(value: string): string | undefined {
  if (!value.trim()) return 'Last name is required.';
  if (value.trim().length < 2) return 'Last name must be at least 2 characters.';
  if (value.trim().length > 100) return 'Last name must not exceed 100 characters.';
  if (!NAME_REGEX.test(value.trim()))
    return 'Last name can only contain letters, spaces, hyphens, and apostrophes.';
  return undefined;
}

function validateEmail(value: string): string | undefined {
  if (!value.trim()) return 'Email is required.';
  if (value.length > 256) return 'Email must not exceed 256 characters.';
  if (!EMAIL_REGEX.test(value.trim())) return 'Please enter a valid email address.';
  return undefined;
}

function validatePassword(value: string): string | undefined {
  if (!value) return 'Password is required.';
  if (value.length < 8) return 'Password must be at least 8 characters long.';
  if (!/[A-Z]/.test(value)) return 'Password must contain at least one uppercase letter.';
  if (!/[a-z]/.test(value)) return 'Password must contain at least one lowercase letter.';
  if (!/[0-9]/.test(value)) return 'Password must contain at least one digit.';
  if (!/[^a-zA-Z0-9]/.test(value)) return 'Password must contain at least one special character.';
  return undefined;
}

function validateConfirmPassword(value: string, password: string): string | undefined {
  if (!value) return 'Password confirmation is required.';
  if (value !== password) return 'Passwords do not match.';
  return undefined;
}

function validateAll(form: {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}): FieldErrors {
  return {
    firstName: validateFirstName(form.firstName),
    lastName: validateLastName(form.lastName),
    email: validateEmail(form.email),
    password: validatePassword(form.password),
    confirmPassword: validateConfirmPassword(form.confirmPassword, form.password),
  };
}

function hasErrors(errors: FieldErrors): boolean {
  return Object.values(errors).some(Boolean);
}

/* ── Field-name mapping for backend validation errors ── */
const FIELD_MAP: Record<string, keyof FieldErrors> = {
  FirstName: 'firstName',
  LastName: 'lastName',
  Email: 'email',
  Password: 'password',
  ConfirmPassword: 'confirmPassword',
  firstname: 'firstName',
  lastname: 'lastName',
  email: 'email',
  password: 'password',
  confirmpassword: 'confirmPassword',
};

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
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [generalError, setGeneralError] = useState('');
  const [loading, setLoading] = useState(false);
  const [touched, setTouched] = useState<Record<string, boolean>>({});

  function update(field: string, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
    // Clear field error while the user is typing
    if (fieldErrors[field as keyof FieldErrors]) {
      setFieldErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  }

  /** Validate a single field on blur so the user gets immediate feedback. */
  const handleBlur = useCallback(
    (field: keyof FieldErrors) => {
      setTouched((prev) => ({ ...prev, [field]: true }));
      let error: string | undefined;
      switch (field) {
        case 'firstName':
          error = validateFirstName(form.firstName);
          break;
        case 'lastName':
          error = validateLastName(form.lastName);
          break;
        case 'email':
          error = validateEmail(form.email);
          break;
        case 'password':
          error = validatePassword(form.password);
          break;
        case 'confirmPassword':
          error = validateConfirmPassword(form.confirmPassword, form.password);
          break;
      }
      setFieldErrors((prev) => ({ ...prev, [field]: error }));
    },
    [form],
  );

  /**
   * Extract per-field and general errors from the API response.
   *
   * The backend can return:
   * 1. Validation errors:  { errors: [{ field: "Email", message: "..." }, ...] }
   * 2. Domain/business errors: { error: "User.DuplicateEmail", message: "..." }
   */
  function applyBackendErrors(err: unknown) {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const axiosErr = err as { response?: { data?: Record<string, unknown> } };
    const data = axiosErr?.response?.data;

    if (!data) {
      setGeneralError('Registration failed. Please check your connection and try again.');
      return;
    }

    // Case 1: Structured validation errors array (from our ValidationBehavior)
    if (Array.isArray(data.errors) && data.errors.length > 0) {
      const newFieldErrors: FieldErrors = {};
      const unmapped: string[] = [];

      for (const e of data.errors as Array<{ field?: string; message: string }>) {
        const fieldKey = e.field ? FIELD_MAP[e.field] : undefined;
        if (fieldKey && !newFieldErrors[fieldKey]) {
          newFieldErrors[fieldKey] = e.message;
        } else {
          unmapped.push(e.message);
        }
      }

      setFieldErrors((prev) => ({ ...prev, ...newFieldErrors }));
      if (unmapped.length > 0) {
        setGeneralError(unmapped.join(' '));
      }
      return;
    }

    // Case 2: ASP.NET model-state style: { errors: { Field: ["msg1", "msg2"] } }
    if (data.errors && typeof data.errors === 'object' && !Array.isArray(data.errors)) {
      const newFieldErrors: FieldErrors = {};
      const unmapped: string[] = [];

      for (const [key, messages] of Object.entries(data.errors as Record<string, string[]>)) {
        const fieldKey = FIELD_MAP[key];
        if (fieldKey && Array.isArray(messages) && messages.length > 0) {
          newFieldErrors[fieldKey] = messages[0];
        } else if (Array.isArray(messages)) {
          unmapped.push(...messages);
        }
      }

      setFieldErrors((prev) => ({ ...prev, ...newFieldErrors }));
      if (unmapped.length > 0) {
        setGeneralError(unmapped.join(' '));
      }
      return;
    }

    // Case 3: Single error message (e.g., DuplicateEmail, RegistrationFailed)
    if (typeof data.message === 'string') {
      // Map known error codes to specific fields
      const code = typeof data.error === 'string' ? data.error : '';
      if (code === 'User.DuplicateEmail') {
        setFieldErrors((prev) => ({ ...prev, email: data.message as string }));
      } else {
        setGeneralError(data.message as string);
      }
      return;
    }

    setGeneralError('Registration failed. Please try again.');
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setGeneralError('');
    setFieldErrors({});

    // Run client-side validation
    const errors = validateAll(form);
    if (hasErrors(errors)) {
      setFieldErrors(errors);
      // Mark all fields as touched so validation shows
      setTouched({ firstName: true, lastName: true, email: true, password: true, confirmPassword: true });
      return;
    }

    setLoading(true);
    try {
      await register({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        password: form.password,
        confirmPassword: form.confirmPassword,
        role: 'Guest',
      });

      // Registration succeeded + auto-login → navigate to home
      navigate('/');
    } catch (err: unknown) {
      applyBackendErrors(err);
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

        <form onSubmit={handleSubmit} className="mt-8 space-y-5" noValidate>
          {generalError && (
            <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700">{generalError}</div>
          )}

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="First Name"
              required
              value={form.firstName}
              onChange={(e) => update('firstName', e.target.value)}
              onBlur={() => handleBlur('firstName')}
              error={touched.firstName ? fieldErrors.firstName : undefined}
              placeholder="John"
            />
            <Input
              label="Last Name"
              required
              value={form.lastName}
              onChange={(e) => update('lastName', e.target.value)}
              onBlur={() => handleBlur('lastName')}
              error={touched.lastName ? fieldErrors.lastName : undefined}
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
            onBlur={() => handleBlur('email')}
            error={touched.email ? fieldErrors.email : undefined}
            placeholder="you@example.com"
          />

          <Input
            label="Password"
            type="password"
            required
            autoComplete="new-password"
            value={form.password}
            onChange={(e) => update('password', e.target.value)}
            onBlur={() => handleBlur('password')}
            error={touched.password ? fieldErrors.password : undefined}
            placeholder="••••••••"
          />
          {/* Password strength hints (always visible for guidance) */}
          <ul className="-mt-3 ml-1 space-y-0.5 text-xs text-gray-400">
            <li className={form.password.length >= 8 ? 'text-green-600' : ''}>
              ✓ At least 8 characters
            </li>
            <li className={/[A-Z]/.test(form.password) ? 'text-green-600' : ''}>
              ✓ One uppercase letter
            </li>
            <li className={/[a-z]/.test(form.password) ? 'text-green-600' : ''}>
              ✓ One lowercase letter
            </li>
            <li className={/[0-9]/.test(form.password) ? 'text-green-600' : ''}>
              ✓ One digit
            </li>
            <li className={/[^a-zA-Z0-9]/.test(form.password) ? 'text-green-600' : ''}>
              ✓ One special character
            </li>
          </ul>

          <Input
            label="Confirm Password"
            type="password"
            required
            value={form.confirmPassword}
            onChange={(e) => update('confirmPassword', e.target.value)}
            onBlur={() => handleBlur('confirmPassword')}
            error={touched.confirmPassword ? fieldErrors.confirmPassword : undefined}
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
