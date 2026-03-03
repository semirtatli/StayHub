import { useState, useEffect } from 'react';
import { useMutation } from '@tanstack/react-query';
import { User } from 'lucide-react';
import toast from 'react-hot-toast';
import { api } from '@/lib/api';
import { useAuth } from '@/features/auth/AuthContext';
import { Button, Input, Card, CardContent } from '@/components/ui';

export function ProfilePage() {
  const { user } = useAuth();

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    phoneNumber: '',
  });

  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });

  useEffect(() => {
    if (user) {
      setForm({
        firstName: user.firstName ?? '',
        lastName: user.lastName ?? '',
        phoneNumber: user.phoneNumber ?? '',
      });
    }
  }, [user]);

  const updateProfile = useMutation({
    mutationFn: (data: typeof form) => api.put('/api/identity/profile', data),
    onSuccess: () => toast.success('Profile updated.'),
    onError: () => toast.error('Failed to update profile.'),
  });

  const changePassword = useMutation({
    mutationFn: (data: { currentPassword: string; newPassword: string }) =>
      api.post('/api/identity/change-password', data),
    onSuccess: () => {
      toast.success('Password changed.');
      setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
    },
    onError: () => toast.error('Failed to change password.'),
  });

  function handleProfileSubmit(e: React.FormEvent) {
    e.preventDefault();
    updateProfile.mutate(form);
  }

  function handlePasswordSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      toast.error('Passwords do not match.');
      return;
    }
    changePassword.mutate({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
    });
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
      <h1 className="flex items-center gap-2 text-2xl font-bold text-gray-900">
        <User size={24} /> My Profile
      </h1>

      {/* Profile Info */}
      <Card className="mt-6">
        <CardContent>
          <h2 className="text-lg font-semibold text-gray-900">Personal Information</h2>
          <form onSubmit={handleProfileSubmit} className="mt-4 space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="First Name"
                value={form.firstName}
                onChange={(e) => setForm({ ...form, firstName: e.target.value })}
              />
              <Input
                label="Last Name"
                value={form.lastName}
                onChange={(e) => setForm({ ...form, lastName: e.target.value })}
              />
            </div>
            <Input label="Email" value={user?.email ?? ''} disabled />
            <Input
              label="Phone Number"
              value={form.phoneNumber}
              onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })}
              placeholder="+1 234 567 890"
            />
            <Button type="submit" isLoading={updateProfile.isPending}>
              Save Changes
            </Button>
          </form>
        </CardContent>
      </Card>

      {/* Change Password */}
      <Card className="mt-6">
        <CardContent>
          <h2 className="text-lg font-semibold text-gray-900">Change Password</h2>
          <form onSubmit={handlePasswordSubmit} className="mt-4 space-y-4">
            <Input
              label="Current Password"
              type="password"
              required
              value={passwordForm.currentPassword}
              onChange={(e) => setPasswordForm({ ...passwordForm, currentPassword: e.target.value })}
            />
            <Input
              label="New Password"
              type="password"
              required
              value={passwordForm.newPassword}
              onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })}
            />
            <Input
              label="Confirm New Password"
              type="password"
              required
              value={passwordForm.confirmPassword}
              onChange={(e) => setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })}
            />
            <Button type="submit" variant="outline" isLoading={changePassword.isPending}>
              Change Password
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
