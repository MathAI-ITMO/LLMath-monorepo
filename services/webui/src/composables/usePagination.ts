import { ref, computed } from 'vue';

export function usePagination<T>(items: { value: T[] }, itemsPerPage: number = 10) {
  const currentPage = ref(1);

  const totalPages = computed(() => Math.ceil(items.value.length / itemsPerPage));

  const paginatedItems = computed(() => {
    const start = (currentPage.value - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    return items.value.slice(start, end);
  });

  function goToPage(page: number) {
    if (page >= 1 && page <= totalPages.value) {
      currentPage.value = page;
    }
  }

  function nextPage() {
    if (currentPage.value < totalPages.value) {
      currentPage.value++;
    }
  }

  function prevPage() {
    if (currentPage.value > 1) {
      currentPage.value--;
    }
  }

  function reset() {
    currentPage.value = 1;
  }

  return {
    currentPage,
    totalPages,
    paginatedItems,
    goToPage,
    nextPage,
    prevPage,
    reset
  };
}