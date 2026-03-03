import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api } from '@/lib/api';
import type { AuthResponse, LoginRequest, RegisterRequest, User } from '@/types';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  hasRole: (role: string) => boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Restore user from localStorage on mount
  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    const token = localStorage.getItem('accessToken');
    if (storedUser && token) {
      setUser(JSON.parse(storedUser));
    }
    setIsLoading(false);
  }, []);

  async function login(data: LoginRequest) {
    const { data: auth } = await api.post<AuthResponse>('/auth/login', data);
    persistAuth(auth);
  }

  async function register(data: RegisterRequest) {
    const { data: auth } = await api.post<AuthResponse>('/auth/register', data);
    persistAuth(auth);
  }

  function logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setUser(null);
  }

  function hasRole(role: string): boolean {
    return user?.roles.includes(role) ?? false;
  }

  function persistAuth(auth: AuthResponse) {
    localStorage.setItem('accessToken', auth.accessToken);
    localStorage.setItem('refreshToken', auth.refreshToken);
    const u: User = {
      userId: auth.userId,
      email: auth.email,
      roles: auth.roles,
    };
    localStorage.setItem('user', JSON.stringify(u));
    setUser(u);
  }

  const value = useMemo<AuthContextType>(
    () => ({ user, isAuthenticated: !!user, isLoading, login, register, logout, hasRole }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [user, isLoading],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextType {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
