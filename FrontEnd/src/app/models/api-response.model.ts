export interface ApiEnvelope<T> {
  success?: boolean;
  message?: string | null;
  errors?: string[];
  data?: T;
}
