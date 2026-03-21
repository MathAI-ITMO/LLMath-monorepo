import { ref } from 'vue';

export function useModal() {
  const showGeolinImportModal = ref(false);
  const showCreateModal = ref(false);
  const showEditModal = ref(false);

  function openGeolinImport() {
    showGeolinImportModal.value = true;
  }

  function closeGeolinImport() {
    showGeolinImportModal.value = false;
  }

  function openCreate() {
    showCreateModal.value = true;
  }

  function closeCreate() {
    showCreateModal.value = false;
  }

  function openEdit() {
    showEditModal.value = true;
  }

  function closeEdit() {
    showEditModal.value = false;
  }

  return {
    showGeolinImportModal,
    showCreateModal,
    showEditModal,
    openGeolinImport,
    closeGeolinImport,
    openCreate,
    closeCreate,
    openEdit,
    closeEdit
  };
}