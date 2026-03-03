import { describe, it, expect } from 'vitest';
import {
  formatCurrency,
  formatDate,
  getInitials,
  getStatusColor,
  calculateNights,
} from '@/lib/utils';

describe('formatCurrency', () => {
  it('formats USD amount correctly', () => {
    const result = formatCurrency(150);
    expect(result).toContain('150');
    expect(result).toContain('$');
  });

  it('formats with decimal places', () => {
    const result = formatCurrency(99.99);
    expect(result).toContain('99.99');
  });

  it('formats EUR currency', () => {
    const result = formatCurrency(200, 'EUR');
    expect(result).toContain('200');
  });
});

describe('formatDate', () => {
  it('formats a date string', () => {
    const result = formatDate('2026-06-15');
    expect(result).toContain('Jun');
    expect(result).toContain('2026');
  });

  it('formats a Date object', () => {
    const result = formatDate(new Date(2026, 5, 15));
    expect(result).toContain('Jun');
    expect(result).toContain('15');
  });
});

describe('getInitials', () => {
  it('returns first letters of name parts', () => {
    expect(getInitials('John Doe')).toBe('JD');
  });

  it('handles single name', () => {
    expect(getInitials('John')).toBe('J');
  });

  it('limits to 2 characters', () => {
    expect(getInitials('John Michael Doe')).toBe('JM');
  });
});

describe('getStatusColor', () => {
  it('returns green for Confirmed', () => {
    expect(getStatusColor('Confirmed')).toContain('green');
  });

  it('returns yellow for Pending', () => {
    expect(getStatusColor('Pending')).toContain('yellow');
  });

  it('returns red for Cancelled', () => {
    expect(getStatusColor('Cancelled')).toContain('red');
  });

  it('returns gray for unknown status', () => {
    expect(getStatusColor('Unknown')).toContain('gray');
  });
});

describe('calculateNights', () => {
  it('calculates nights between two date strings', () => {
    expect(calculateNights('2026-06-01', '2026-06-05')).toBe(4);
  });

  it('calculates 1 night for consecutive days', () => {
    expect(calculateNights('2026-06-01', '2026-06-02')).toBe(1);
  });

  it('works with Date objects', () => {
    const checkIn = new Date(2026, 5, 1);
    const checkOut = new Date(2026, 5, 3);
    expect(calculateNights(checkIn, checkOut)).toBe(2);
  });
});
