import { useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import toast from 'react-hot-toast';
import { useAuth } from '@/features/auth/AuthContext';

interface NotificationMessage {
  type: string;
  payload: Record<string, unknown>;
  timestamp: string;
}

export function useNotifications() {
  const { isAuthenticated } = useAuth();
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const connect = useCallback(() => {
    if (!isAuthenticated || connectionRef.current) return;

    const token = localStorage.getItem('accessToken');
    if (!token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('ReceiveNotification', (message: NotificationMessage) => {
      const messages: Record<string, string> = {
        BookingConfirmed: 'Your booking has been confirmed!',
        BookingCancelled: 'A booking has been cancelled.',
        PaymentCompleted: 'Payment processed successfully.',
        ReviewPosted: 'A new review has been posted.',
      };

      toast.success(messages[message.type] || `Notification: ${message.type}`);
    });

    connection.start().catch(console.error);
    connectionRef.current = connection;
  }, [isAuthenticated]);

  useEffect(() => {
    connect();
    return () => {
      connectionRef.current?.stop();
      connectionRef.current = null;
    };
  }, [connect]);
}
