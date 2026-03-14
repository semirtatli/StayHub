import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { MessageSquare } from 'lucide-react';
import toast from 'react-hot-toast';
import { api } from '@/lib/api';
import { formatDate } from '@/lib/utils';
import type { Review } from '@/types';
import { Button, Input, Card, CardContent, StarRating, Skeleton, Modal } from '@/components/ui';

export function MyReviewsPage() {
  const queryClient = useQueryClient();
  const [showModal, setShowModal] = useState(false);
  const [editReview, setEditReview] = useState<Review | null>(null);

  const [title, setTitle] = useState('');
  const [comment, setComment] = useState('');
  const [rating, setRating] = useState(5);

  const { data: reviews, isLoading } = useQuery<Review[]>({
    queryKey: ['my-reviews'],
    queryFn: () => api.get<Review[]>('/reviews/my').then((r) => r.data),
  });

  const submitReview = useMutation({
    mutationFn: (data: { title: string; body: string; overallRating: number; reviewId?: string }) => {
      if (data.reviewId) {
        return api.put(`/reviews/${data.reviewId}`, data);
      }
      return api.post('/reviews', data);
    },
    onSuccess: () => {
      toast.success(editReview ? 'Review updated.' : 'Review submitted.');
      queryClient.invalidateQueries({ queryKey: ['my-reviews'] });
      closeModal();
    },
    onError: () => toast.error('Failed to save review.'),
  });

  const deleteReview = useMutation({
    mutationFn: (reviewId: string) => api.delete(`/reviews/${reviewId}`),
    onSuccess: () => {
      toast.success('Review deleted.');
      queryClient.invalidateQueries({ queryKey: ['my-reviews'] });
    },
    onError: () => toast.error('Failed to delete review.'),
  });

  function openEdit(review: Review) {
    setEditReview(review);
    setTitle(review.title ?? '');
    setComment(review.body);
    setRating(review.overallRating);
    setShowModal(true);
  }

  function closeModal() {
    setShowModal(false);
    setEditReview(null);
    setTitle('');
    setComment('');
    setRating(5);
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    submitReview.mutate({
      title,
      body: comment,
      overallRating: rating,
      reviewId: editReview?.id,
    });
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="flex items-center gap-2 text-2xl font-bold text-gray-900">
        <MessageSquare size={24} /> My Reviews
      </h1>

      {isLoading ? (
        <div className="mt-6 space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-28 w-full rounded-lg" />
          ))}
        </div>
      ) : reviews && reviews.length > 0 ? (
        <div className="mt-6 space-y-4">
          {reviews.map((review) => (
            <Card key={review.id}>
              <CardContent>
                <div className="flex items-start justify-between">
                  <div>
                    <h3 className="font-semibold text-gray-900">{review.hotelName ?? 'Hotel'}</h3>
                    <StarRating rating={review.overallRating} size={16} />
                  </div>
                  <span className="text-xs text-gray-400">{formatDate(review.createdAt)}</span>
                </div>
                {review.title && <h4 className="mt-2 font-medium text-gray-800">{review.title}</h4>}
                <p className="mt-1 text-sm text-gray-600">{review.body}</p>
                {review.managementResponse && (
                  <div className="mt-3 rounded-lg bg-gray-50 p-3 text-sm">
                    <span className="font-medium text-gray-700">Response:</span>
                    <p className="mt-1 text-gray-600">{review.managementResponse}</p>
                  </div>
                )}
                <div className="mt-3 flex gap-2">
                  <Button variant="ghost" size="sm" onClick={() => openEdit(review)}>
                    Edit
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-red-600 hover:text-red-700"
                    onClick={() => deleteReview.mutate(review.id)}
                  >
                    Delete
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="mt-16 text-center text-gray-500">
          <p>You haven't written any reviews yet.</p>
        </div>
      )}

      {/* Edit Modal */}
      <Modal isOpen={showModal} onClose={closeModal} title={editReview ? 'Edit Review' : 'Write Review'}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Rating</label>
            <StarRating rating={rating} onChange={setRating} interactive size={24} />
          </div>
          <Input label="Title" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Summary…" />
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Comment</label>
            <textarea
              rows={4}
              required
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:ring-1 focus:ring-primary-500"
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder="Tell us about your experience…"
            />
          </div>
          <div className="flex justify-end gap-3">
            <Button variant="ghost" type="button" onClick={closeModal}>Cancel</Button>
            <Button type="submit" isLoading={submitReview.isPending}>Save</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
