import { toast as toastify } from 'react-toastify';

export const TOAST_TYPES = {
  ERROR: 'error',
  SUCCESS: 'success',
  WARNING: 'warning',
};

export const displayToast = (
  toastType = TOAST_TYPES.SUCCESS,
  message = '',
  position = toastify.POSITION.TOP_RIGHT,
) =>
  toastify[toastType](message, {
    position,
  });
