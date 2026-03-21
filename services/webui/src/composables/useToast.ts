import { ref } from 'vue';

export function useToast() {
  const errorToast = ref({
    show: false,
    message: ''
  });

  const successToast = ref({
    show: false,
    message: ''
  });

  function showError(message: string) {
    errorToast.value.message = message;
    errorToast.value.show = true;
    setTimeout(() => {
      errorToast.value.show = false;
    }, 3000);
  }

  function showSuccess(message: string) {
    successToast.value.message = message;
    successToast.value.show = true;
    setTimeout(() => {
      successToast.value.show = false;
    }, 2000);
  }

  return {
    errorToast,
    successToast,
    showError,
    showSuccess
  };
}