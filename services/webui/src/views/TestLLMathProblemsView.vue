<template>
  <v-container fluid class="pa-4">
    <v-card class="mx-auto" max-width="1400">
      <!-- Tabs Navigation -->
      <v-tabs v-model="activeTab" bg-color="primary" class="mb-4">
        <v-tab value="management">
          <v-icon start>mdi-file-document-edit</v-icon>
          Управление задачами
        </v-tab>
        <v-tab value="videos">
          <v-icon start>mdi-video</v-icon>
          Управление видео
        </v-tab>
      </v-tabs>

      <v-card-text>
        <!-- Вкладка 1: Управление задачами -->
        <v-window v-model="activeTab">
          <v-window-item value="management">
            <!-- Action Buttons -->
            <div class="d-flex gap-2 mb-4">
              <v-btn
                color="primary"
                prepend-icon="mdi-download"
                @click="showGeolinImportModal = true">
                Импорт из GeoLin
              </v-btn>
              <v-btn
                color="success"
                prepend-icon="mdi-plus"
                @click="showCreateModal = true">
                Создать задачу
              </v-btn>
            </div>

            <!-- Problems List -->
            <v-card variant="outlined">
              <v-card-title class="d-flex align-center justify-space-between flex-wrap">
                <span class="text-h6">Список задач ({{ problems.length }})</span>
                <div v-if="totalPages > 1" class="d-flex align-center gap-1">
                  <v-btn
                    icon="mdi-chevron-left"
                    size="small"
                    variant="text"
                    @click="goToPage(currentPage - 1)"
                    :disabled="currentPage === 1">
                  </v-btn>
                  <v-btn
                    v-for="page in totalPages"
                    :key="page"
                    size="small"
                    :variant="page === currentPage ? 'flat' : 'text'"
                    :color="page === currentPage ? 'primary' : undefined"
                    @click="goToPage(page)">
                    {{ page }}
                  </v-btn>
                  <v-btn
                    icon="mdi-chevron-right"
                    size="small"
                    variant="text"
                    @click="goToPage(currentPage + 1)"
                    :disabled="currentPage === totalPages">
                  </v-btn>
                </div>
              </v-card-title>

              <v-card-text>
                <v-progress-circular
                  v-if="loading && !problems.length"
                  indeterminate
                  color="primary"
                  class="d-block mx-auto my-8">
                </v-progress-circular>

                <v-alert
                  v-if="!loading && problems.length === 0 && attemptedLoad"
                  type="info"
                  variant="tonal"
                  class="mb-4">
                  Задачи не найдены. Используйте кнопки выше для добавления задач.
                </v-alert>

                <v-data-table
                  v-if="problems.length > 0"
                  :headers="tableHeaders"
                  :items="paginatedProblems"
                  :loading="loading"
                  :item-value="(item: any) => item.id || item._id"
                  class="elevation-0"
                  hide-default-footer>
                  <template v-slot:item.type="{ item }">
                    <v-chip
                      v-if="getProblemAssignedTypes(item.id)"
                      color="primary"
                      size="small"
                      variant="flat">
                      {{ getProblemAssignedTypes(item.id) }}
                    </v-chip>
                    <span v-else class="text-grey">Без типа</span>
                  </template>

                  <template v-slot:item.title="{ item }">
                    <div class="font-weight-medium">{{ item.title || 'Без названия' }}</div>
                  </template>

                  <template v-slot:item.statement="{ item }">
                    <div class="problem-statement" v-html="renderTruncatedStatement(item.statement, 450)"></div>
                  </template>

                  <template v-slot:item.actions="{ item }">
                    <div class="d-flex gap-2">
                      <v-btn
                        icon="mdi-pencil"
                        size="small"
                        color="warning"
                        variant="text"
                        @click="editProblem(item)"
                        title="Редактировать">
                      </v-btn>
                      <v-btn
                        icon="mdi-delete"
                        size="small"
                        color="error"
                        variant="text"
                        @click="deleteProblemByIdAndRefresh(item.id)"
                        :disabled="apiCallLoading.deleteProblem"
                        title="Удалить">
                      </v-btn>
                    </div>
                  </template>
                </v-data-table>
              </v-card-text>
            </v-card>
          </v-window-item>

          <!-- Вкладка 2: Управление видео -->
          <v-window-item value="videos">
            <div class="video-iframe-wrapper">
              <VideoApp embedded />
            </div>
          </v-window-item>
        </v-window>
      </v-card-text>
    </v-card>

    <!-- GeoLin Import Modal -->
    <v-dialog v-model="showGeolinImportModal" max-width="800" persistent>
      <v-card>
        <v-card-title class="d-flex align-center justify-space-between">
          <span class="text-h5">Импорт задачи из GeoLin</span>
          <v-btn icon="mdi-close" variant="text" @click="showGeolinImportModal = false"></v-btn>
        </v-card-title>

        <v-card-text>
          <v-combobox
            v-model="geolinPrefixToLoad"
            label="Префикс GeoLin *"
            placeholder="tasks.linalg.linear_operators..."
            :items="availableGeolinPrefixes"
            variant="outlined"
            class="mb-4"
            hint="Выберите префикс из списка или введите свой"
            persistent-hint>
          </v-combobox>

          <v-progress-circular
            v-if="geolinLoading"
            indeterminate
            color="primary"
            class="d-block mx-auto my-4">
          </v-progress-circular>
          <p v-if="geolinLoading" class="text-center mb-4">Загрузка задачи из GeoLin...</p>

          <v-alert
            v-if="geolinResponse.loadFromGeolin?.error"
            type="error"
            variant="tonal"
            class="mb-4">
            <strong>Ошибка:</strong> {{ geolinResponse.loadFromGeolin.error }}
          </v-alert>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="showGeolinImportModal = false">Отмена</v-btn>
          <v-btn
            color="primary"
            variant="flat"
            @click="handleGeolinImport"
            :disabled="!geolinPrefixToLoad || geolinLoading">
            Загрузить
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Create Problem Modal -->
    <v-dialog v-model="showCreateModal" max-width="1200" persistent scrollable>
      <v-card>
        <v-card-title class="d-flex align-center justify-space-between">
          <span class="text-h5">Создать задачу</span>
          <v-btn icon="mdi-close" variant="text" @click="closeCreateModal"></v-btn>
        </v-card-title>

        <v-card-text>
          <v-form>
            <!-- Название -->
            <v-text-field
              v-model="managementNewProblem.title"
              label="Название задачи *"
              placeholder="Введите название задачи"
              variant="outlined"
              class="mb-4"
              required>
            </v-text-field>

            <!-- Тип -->
            <v-select
              v-model="managementNewProblemType"
              :items="taskTypeOptions"
              item-title="title"
              item-value="value"
              label="Тип задачи *"
              placeholder="Выберите тип"
              variant="outlined"
              class="mb-4"
              required
              hint="Выберите тип задачи из списка (обязательно)"
              persistent-hint>
            </v-select>

            <!-- Видео теории -->
            <v-select
              v-model="managementNewProblem.theoryLink"
              :items="videoSelectItems"
              item-title="title"
              item-value="value"
              label="Видео теории"
              variant="outlined"
              class="mb-4"
              @focus="fetchAvailableVideos"
              hint="Выберите видео из списка"
              persistent-hint>
            </v-select>

            <!-- Условие (MathEditor) -->
            <div class="mb-4">
              <label class="text-subtitle-2 mb-2 d-block">Условие задачи *</label>
              <MathEditor
                v-model="managementNewProblem.statement"
                placeholder="Введите условие задачи. Используйте $ для формул, например: $x^2 + y^2 = 1$"
                :rows="8"
              />
            </div>

            <!-- Решение LLM (MathEditor) -->
            <div class="mb-4">
              <div class="d-flex align-center justify-space-between mb-2">
                <label class="text-subtitle-2">Решение LLM</label>
                <div class="d-flex gap-2">
                  <v-btn
                    size="small"
                    color="info"
                    prepend-icon="mdi-robot"
                    variant="outlined"
                    @click.prevent="getLlmSolution('management')"
                    :disabled="llmLoading || !managementNewProblem.statement"
                    :loading="llmLoading && activeForm === 'management'">
                    Получить решение LLM
                  </v-btn>
                  <v-btn
                    size="small"
                    color="success"
                    prepend-icon="mdi-check"
                    variant="outlined"
                    @click.prevent="checkSolution('management')"
                    :disabled="geolinLoading || !managementNewProblem.statement || !managementNewProblemLlmSolutionJson || !managementNewProblem.geolinHash">
                    Проверить решение
                  </v-btn>
                </div>
              </div>
              <MathEditor
                v-model="managementNewProblemLlmSolutionJson"
                placeholder="Решение появится здесь после нажатия кнопки 'Получить решение LLM'"
                :rows="10"
              />
              <v-progress-circular
                v-if="llmLoading && activeForm === 'management'"
                indeterminate
                color="primary"
                size="small"
                class="mt-2">
              </v-progress-circular>
            </div>

            <!-- GeoLin данные -->
            <v-card variant="outlined" class="mb-4 pa-3">
              <div class="d-flex gap-4 mb-2">
                <div>
                  <span class="text-caption text-grey">GeoLin Hash:</span>
                  <div class="text-body-2 font-weight-medium">{{ managementNewProblem.geolinHash || 'не указан' }}</div>
                </div>
                <div>
                  <span class="text-caption text-grey">GeoLin Seed:</span>
                  <div class="text-body-2 font-weight-medium">{{ managementNewProblem.geolinSeed || 0 }}</div>
                </div>
              </div>
              <div class="text-caption text-grey">Эти данные заполняются автоматически при импорте из GeoLin</div>
            </v-card>
          </v-form>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="closeCreateModal">Отмена</v-btn>
          <v-btn
            color="primary"
            variant="flat"
            @click="handleCreateProblem"
            :disabled="!managementNewProblem.statement || !managementNewProblem.title || !managementNewProblemType || apiCallLoading.managementAddProblem"
            :loading="apiCallLoading.managementAddProblem">
            Создать задачу
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Edit Problem Modal -->
    <v-dialog v-model="showEditModal" max-width="1200" persistent scrollable>
      <v-card>
        <v-card-title class="d-flex align-center justify-space-between">
          <span class="text-h5">Редактировать задачу</span>
          <v-btn icon="mdi-close" variant="text" @click="closeEditModal"></v-btn>
        </v-card-title>

        <v-card-text>
          <v-form>
            <!-- Название -->
            <v-text-field
              v-model="currentEditProblem.title"
              label="Название задачи *"
              placeholder="Введите название задачи"
              variant="outlined"
              class="mb-4"
              required>
            </v-text-field>

            <!-- Тип -->
            <v-select
              v-model="currentEditProblemType"
              :items="taskTypeOptions"
              item-title="title"
              item-value="value"
              label="Тип задачи *"
              placeholder="Выберите тип"
              variant="outlined"
              class="mb-4"
              required
              hint="Выберите тип задачи из списка (обязательно)"
              persistent-hint>
            </v-select>

            <!-- Видео теории -->
            <v-select
              v-model="currentEditProblem.theoryLink"
              :items="videoSelectItems"
              item-title="title"
              item-value="value"
              label="Видео теории"
              variant="outlined"
              class="mb-4"
              @focus="fetchAvailableVideos"
              hint="Выберите видео из списка"
              persistent-hint>
            </v-select>

            <!-- Условие (MathEditor) -->
            <div class="mb-4">
              <label class="text-subtitle-2 mb-2 d-block">Условие задачи *</label>
              <MathEditor
                v-model="currentEditProblem.statement"
                placeholder="Введите условие задачи. Используйте $ для формул, например: $x^2 + y^2 = 1$"
                :rows="8"
              />
            </div>

            <!-- Решение LLM (MathEditor) -->
            <div class="mb-4">
              <div class="d-flex align-center justify-space-between mb-2">
                <label class="text-subtitle-2">Решение LLM</label>
                <div class="d-flex gap-2">
                  <v-btn
                    size="small"
                    color="info"
                    prepend-icon="mdi-robot"
                    variant="outlined"
                    @click.prevent="getLlmSolution('edit')"
                    :disabled="llmLoading || !currentEditProblem.statement"
                    :loading="llmLoading && activeForm === 'edit'">
                    Получить решение LLM
                  </v-btn>
                  <v-btn
                    size="small"
                    color="success"
                    prepend-icon="mdi-check"
                    variant="outlined"
                    @click.prevent="checkSolution('edit')"
                    :disabled="geolinLoading || !currentEditProblem.statement || !currentEditProblemLlmSolutionJson || !editingProblem?.geolinHash">
                    Проверить решение
                  </v-btn>
                </div>
              </div>
              <MathEditor
                v-model="currentEditProblemLlmSolutionJson"
                placeholder="Решение LLM"
                :rows="10"
              />
              <v-progress-circular
                v-if="llmLoading && activeForm === 'edit'"
                indeterminate
                color="primary"
                size="small"
                class="mt-2">
              </v-progress-circular>
            </div>

            <!-- GeoLin данные (только для чтения) -->
            <v-card v-if="editingProblem" variant="outlined" class="mb-4 pa-3">
              <div class="d-flex gap-4 mb-2">
                <div>
                  <span class="text-caption text-grey">GeoLin Hash:</span>
                  <div class="text-body-2 font-weight-medium">{{ editingProblem.geolinHash || 'не указан' }}</div>
                </div>
                <div>
                  <span class="text-caption text-grey">GeoLin Seed:</span>
                  <div class="text-body-2 font-weight-medium">{{ editingProblem.geolinSeed || 0 }}</div>
                </div>
              </div>
              <div class="text-caption text-grey">GeoLin данные нельзя изменить после создания</div>
            </v-card>
          </v-form>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="closeEditModal">Отмена</v-btn>
          <v-btn
            color="primary"
            variant="flat"
            @click="handleUpdateProblem"
            :disabled="!currentEditProblem.statement || !currentEditProblem.title || !currentEditProblemType || apiCallLoading.managementUpdateProblem"
            :loading="apiCallLoading.managementUpdateProblem">
            Сохранить изменения
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Модальное окно результатов проверки решения -->
    <v-dialog v-model="checkResultModal.show" max-width="900" scrollable persistent>
      <v-card>
        <v-card-title class="d-flex align-center justify-space-between">
          <span class="text-h5">Результат проверки решения</span>
          <v-btn icon="mdi-close" variant="text" @click="closeCheckResultModal"></v-btn>
        </v-card-title>

        <v-card-text>
          <v-card variant="outlined" class="mb-4 pa-3">
            <v-card-title class="text-subtitle-1 mb-2">📝 Проверенное решение:</v-card-title>
            <v-card-text>
              <pre class="text-body-2">{{ checkResultModal.solution.substring(0, 300) }}{{ checkResultModal.solution.length > 300 ? '...' : '' }}</pre>
            </v-card-text>
          </v-card>

          <v-card variant="outlined" class="mb-4 pa-3">
            <v-card-title class="text-subtitle-1 mb-2">🎯 Извлеченный ответ:</v-card-title>
            <v-card-text>
              <v-chip color="info" variant="flat" size="large" class="font-weight-bold">
                {{ checkResultModal.extractedAnswer }}
              </v-chip>
            </v-card-text>
          </v-card>

          <v-card
            variant="outlined"
            :color="checkResultModal.checkResult?.isCorrect ? 'success' : 'error'"
            :class="['mb-4', checkResultModal.checkResult?.isCorrect ? 'bg-success' : 'bg-error']"
            class="pa-4">
            <v-card-title class="d-flex align-center gap-2 text-h6">
              <v-icon>{{ checkResultModal.checkResult?.isCorrect ? 'mdi-check-circle' : 'mdi-close-circle' }}</v-icon>
              <span>{{ checkResultModal.checkResult?.isCorrect ? 'ПРАВИЛЬНО' : 'НЕПРАВИЛЬНО' }}</span>
            </v-card-title>
            <v-card-text v-if="checkResultModal.checkResult?.message" class="text-body-1">
              {{ checkResultModal.checkResult.message }}
            </v-card-text>
          </v-card>

          <v-card variant="outlined" class="pa-3">
            <v-card-title class="text-subtitle-1 mb-2">🔧 Детали проверки:</v-card-title>
            <v-card-text>
              <div class="d-flex flex-column gap-2">
                <div><strong>Hash задачи:</strong> {{ checkResultModal.hash }}</div>
                <div><strong>Seed:</strong> {{ checkResultModal.seed || 'не указан' }}</div>
                <div><strong>Отправленный ответ в GeoLin:</strong> <code class="pa-1 bg-grey-lighten-3 rounded">{{ checkResultModal.extractedAnswer }}</code></div>
              </div>
            </v-card-text>
          </v-card>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" variant="flat" @click="closeCheckResultModal">Закрыть</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Success Snackbar -->
    <v-snackbar
      v-model="successToast.show"
      :timeout="2000"
      color="success"
      location="top">
      {{ successToast.message }}
      <template v-slot:actions>
        <v-btn variant="text" @click="successToast.show = false">Закрыть</v-btn>
      </template>
    </v-snackbar>

    <!-- Error Snackbar -->
    <v-snackbar
      v-model="errorToast.show"
      :timeout="3000"
      color="error"
      location="top">
      {{ errorToast.message }}
      <template v-slot:actions>
        <v-btn variant="text" @click="errorToast.show = false">Закрыть</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import MathEditor from '@/components/MathEditor.vue';
import { renderMessage } from '@/utils/renderMessage';
import { useProblemManagement } from '@/composables/useProblemManagement';
import { useGeolinProxy } from '@/composables/useGeolinProxy';
import { useLlmSolution } from '@/composables/useLlmSolution';
import { useProblemForm } from '@/composables/useProblemForm';
import { useToast } from '@/composables/useToast';
import { useVideoManagement } from '@/composables/useVideoManagement';
import VideoApp from '@/components/video/VideoApp.vue';
import { usePagination } from '@/composables/usePagination';
import { TaskType } from '@/api/generated/api';

// Task type options for dropdown
const taskTypeOptions = [
  { title: 'Default', value: String(TaskType.Default) },
  { title: 'Learning', value: String(TaskType.Learning) },
  { title: 'Guided', value: String(TaskType.Guided) },
  { title: 'Exam', value: String(TaskType.Exam) },
];

// Composables
const { 
  problems, 
  loading, 
  error, 
  attemptedLoad, 
  problemTypesMap,
  apiCallLoading, 
  apiResponse, 
  fetchAllProblems, 
  getProblemAssignedTypes, 
  deleteProblemByIdAndRefresh,
  createProblem: createProblemApi,
  updateProblem: updateProblemApi,
  tryParseJson,
  taskTypeToString
} = useProblemManagement();

const { 
  loading: geolinLoading, 
  response: geolinResponse, 
  checkResultModal, 
  loadFromGeolin: loadFromGeolinApi, 
  checkSolution: checkSolutionApi,
  closeCheckResultModal
} = useGeolinProxy();

const { 
  loading: llmLoading, 
  activeForm, 
  getLlmSolution: getLlmSolutionApi 
} = useLlmSolution();

const {
  managementNewProblem,
  managementNewProblemLlmSolutionJson,
  managementNewProblemType,
  editingProblem,
  currentEditProblem,
  currentEditProblemType,
  currentEditProblemLlmSolutionJson,
  resetCreateForm,
  resetEditForm,
  populateEditForm,
  populateFromGeolin
} = useProblemForm();

const { errorToast, successToast, showError, showSuccess } = useToast();
const showGeolinImportModal = ref(false);
const showCreateModal = ref(false);
const showEditModal = ref(false);

function openGeolinImport() { showGeolinImportModal.value = true; }
function closeGeolinImport() { showGeolinImportModal.value = false; }
function openCreate() { showCreateModal.value = true; }
function closeCreate() { showCreateModal.value = false; }
function openEdit() { showEditModal.value = true; }
function closeEdit() { showEditModal.value = false; }

const { 
  videoAppUrl, 
  videoSelectItems, 
  fetchAvailableVideos, 
  loadingVideos 
} = useVideoManagement();

const { paginatedItems: paginatedProblems, totalPages, currentPage, goToPage } = usePagination(problems, 10);

const route = useRoute();
const router = useRouter();
const VALID_TABS = ['management', 'videos'] as const;
const activeTab = ref<string>(
  VALID_TABS.includes(route.query.subtab as any) ? (route.query.subtab as string) : 'management'
);
watch(activeTab, (tab) => {
  router.replace({ query: { ...route.query, subtab: tab } });
});
const geolinPrefixToLoad = ref('');
const availableGeolinPrefixes = ref<string[]>([
  "tasks.linalg.linear_operators.matrix_decompositions.LU_decomposition.LU_decomposition_3x3",
  "tasks.linalg.linear_operators.matrix_decompositions.LU_decomposition.LU_decomposition_4x4",
  "tasks.linalg.linear_space.basis_transformation.basis_transformation_vector",
]);

// Table headers for v-data-table
const tableHeaders = [
  { title: 'Тип задачи', key: 'type', width: '150px', align: 'start' as const },
  { title: 'Название', key: 'title', width: '200px', align: 'start' as const },
  { title: 'Условие (фрагмент)', key: 'statement', align: 'start' as const },
  { title: 'Действия', key: 'actions', width: '180px', align: 'center' as const, sortable: false },
];

watch(activeTab, (newTab) => {
  if (newTab === 'management' && problems.value.length === 0 && !loading.value && !attemptedLoad.value) {
    fetchAllProblems();
  }
  if (newTab === 'database') {
    editingProblem.value = null;
  }
});

// Helper function for rendering truncated statements with math
function renderTruncatedStatement(statement: string, maxLength: number): string {
  if (!statement) return ''

  let text = statement;

  // 1. Обрезаем по ключевым фразам (с учётом \textbf{})
  const cutoffPhrases = ['Пример ввода', 'Пример ответа', 'Ответ'];
  let earliestCutIndex = -1;

  for (const phrase of cutoffPhrases) {
    // Ищем фразу с префиксом \textbf{
    const withPrefix = `\\textbf{${phrase}`;
    const prefixIndex = text.indexOf(withPrefix);

    if (prefixIndex !== -1) {
      // Нашли с префиксом - обрезаем по префиксу
      if (earliestCutIndex === -1 || prefixIndex < earliestCutIndex) {
        earliestCutIndex = prefixIndex;
      }
    } else {
      // Ищем без префикса
      const phraseIndex = text.indexOf(phrase);
      if (phraseIndex !== -1) {
        if (earliestCutIndex === -1 || phraseIndex < earliestCutIndex) {
          earliestCutIndex = phraseIndex;
        }
      }
    }
  }

  // Обрезаем по самой ранней найденной фразе
  if (earliestCutIndex !== -1) {
    text = text.substring(0, earliestCutIndex);
  }

  // 2. Убираем множественные переносы строк (различные варианты)
  // Заменяем все варианты двойных переносов на одинарные
  text = text
    // HTML переносы
    .replace(/<br\s*\/?>\s*<br\s*\/?>/gi, '<br>') // двойной <br>
    .replace(/<\/span>\s*<br\s*\/?>\s*<br\s*\/?>/gi, '</span><br>') // </span><br><br>
    .replace(/<br\s*\/?>\s*<\/span>\s*<br\s*\/?>/gi, '<br></span>') // <br></span><br>
    // Текстовые переносы
    .replace(/\r?\n\s*\r?\n/g, '\n') // двойной \n или \r\n
    .replace(/\r\n\s*\r\n/g, '\r\n') // двойной \r\n
    // Смешанные варианты
    .replace(/<br\s*\/?>\s*\n/gi, '<br>') // <br> + \n
    .replace(/\n\s*<br\s*\/?>/gi, '<br>') // \n + <br>
    // Тройные и более переносы
    .replace(/(<br\s*\/?>){3,}/gi, '<br><br>') // три и более <br>
    .replace(/(\r?\n){3,}/g, '\n\n') // три и более \n
    // Убираем лишние пробелы вокруг переносов
    .replace(/\s*<br\s*\/?>\s*/gi, '<br>')
    .trim();

  // 3. Обрезаем до maxLength
  const truncated = text.length <= maxLength ? text : text.substring(0, maxLength);

  // 4. Рендерим с KaTeX
  try {
    return renderMessage(truncated + (text.length > maxLength ? '...' : ''));
  } catch (e) {
    // Fallback если рендеринг не удался
    return truncated + (text.length > maxLength ? '...' : '');
  }
}

// Edit problem handler
function editProblem(problem: any) {
  const assignedTypes = problemTypesMap.value[problem._id || problem.id || ''] || [];
  populateEditForm(problem, assignedTypes);
  openEdit();
}

function closeEditModal() {
  closeEdit();
  resetEditForm();
  if (apiResponse.managementUpdateProblem) {
    apiResponse.managementUpdateProblem = null;
  }
}

async function handleUpdateProblem() {
  if (!editingProblem.value || !editingProblem.value.id) {
    showError('ID редактируемой задачи не найден');
    return;
  }

  const problemId = editingProblem.value.id;
  if (!problemId) {
    showError('ID редактируемой задачи не найден');
    return;
  }

  if (!currentEditProblemType.value) {
    showError('Пожалуйста, выберите тип задачи');
    return;
  }

  const result = await updateProblemApi(problemId, {
    title: currentEditProblem.title || '',
    statement: currentEditProblem.statement,
    geolin_ans_key: { hash: editingProblem.value.geolinHash || '', seed: editingProblem.value.geolinSeed || 0 },
    llmSolutionJson: currentEditProblemLlmSolutionJson.value,
    theory_link: currentEditProblem.theoryLink,
    type: currentEditProblemType.value,
  });

  if (result?.success) {
    showSuccess('✅ Задача успешно сохранена!');
    closeEditModal();
    await fetchAllProblems();
  } else if (result?.error) {
    showError(apiResponse.managementUpdateProblem?.message || 'Не удалось обновить задачу');
  }
}


// These functions are now provided by composables - removed duplicate implementations

// Removed old function implementations - now using composables

// Modal handlers
async function handleGeolinImport() {
  const data = await loadFromGeolinApi(geolinPrefixToLoad.value);
  if (data && !data.error) {
    populateFromGeolin(data);
    closeGeolinImport();
    openCreate();
  } else {
    showError(geolinResponse.loadFromGeolin?.error || 'Не удалось загрузить задачу из GeoLin');
  }
}

function closeCreateModal() {
  closeCreate();
  resetCreateForm();
  if (apiResponse.managementAddProblem) {
    apiResponse.managementAddProblem = null;
  }
}

async function handleCreateProblem() {
  if (!managementNewProblemType.value) {
    showError('Пожалуйста, выберите тип задачи');
    return;
  }

  const result = await createProblemApi({
    title: managementNewProblem.title || '',
    statement: managementNewProblem.statement,
    geolin_ans_key: { hash: managementNewProblem.geolinHash || '', seed: managementNewProblem.geolinSeed || 0 },
    llmSolutionJson: managementNewProblemLlmSolutionJson.value,
    theory_link: managementNewProblem.theoryLink,
    type: managementNewProblemType.value,
  });

  if (result?.success) {
    showSuccess('✅ Задача успешно создана!');
    closeCreateModal();
    await fetchAllProblems();
  } else if (result?.error) {
    showError(apiResponse.managementAddProblem?.message || 'Не удалось создать задачу');
  }
}

// Get LLM solution handler
async function getLlmSolution(formType: 'management' | 'edit') {
  activeForm.value = formType;
  let problemStatement = '';

  if (formType === 'management') {
    problemStatement = managementNewProblem.statement;
  } else if (formType === 'edit') {
    problemStatement = currentEditProblem.statement;
  }

  if (!problemStatement) {
    showError('Поле "Условие" не может быть пустым для получения решения LLM');
    return;
  }

  try {
    const solution = await getLlmSolutionApi(problemStatement);

    if (formType === 'management') {
      managementNewProblemLlmSolutionJson.value = solution;
      managementNewProblem.llmSolution = solution;
    } else if (formType === 'edit') {
      currentEditProblemLlmSolutionJson.value = solution;
      currentEditProblem.llmSolution = solution;
    }
  } catch (error) {
    showError(error instanceof Error ? error.message : 'Ошибка при получении решения от LLM');
  }
}

// Check solution handler
async function checkSolution(formType: 'management' | 'edit') {
  let problemStatement = '';
  let solution = '';
  let hash = '';
  let seed: number | undefined;

  if (formType === 'management') {
    problemStatement = managementNewProblem.statement;
    solution = managementNewProblemLlmSolutionJson.value;
    hash = managementNewProblem.geolinHash || '';
    seed = managementNewProblem.geolinSeed;
  } else if (formType === 'edit') {
    problemStatement = currentEditProblem.statement;
    solution = currentEditProblemLlmSolutionJson.value;
    hash = editingProblem.value?.geolinHash || '';
    seed = editingProblem.value?.geolinSeed;
  }

  if (!problemStatement) {
    showError('Поле "Условие" не может быть пустым для проверки решения');
    return;
  }

  if (!solution) {
    showError('Поле "Решение LLM" не может быть пустым для проверки');
    return;
  }

  if (!hash) {
    showError('Hash задачи отсутствует. Невозможно проверить решение.');
    return;
  }

  try {
    await checkSolutionApi(problemStatement, solution, hash, seed);
  } catch (error) {
    showError(error instanceof Error ? error.message : 'Ошибка при проверке решения');
  }
}

onMounted(() => {
  fetchAllProblems(); // Это также вызовет fetchAllTypes и populateProblemTypesMap
});

</script>

<style scoped>
/* Video iframe wrapper for full-height display */
.video-iframe-wrapper {
  width: 100%;
  height: calc(100vh - 200px);
  min-height: 600px;
  margin: 0;
  padding: 0;
}

.video-iframe {
  width: 100%;
  height: 100%;
  border: none;
  display: block;
}

/* Problem statement with math rendering */
.problem-statement :deep(.katex) {
  font-size: 1em;
  color: rgb(var(--v-theme-primary));
}

/* Gap utility class for flex layouts */
.gap-1 {
  gap: 4px;
}

.gap-2 {
  gap: 8px;
}

.gap-4 {
  gap: 16px;
}

/* Ensure proper spacing in table cells */
.problem-statement {
  color: rgb(var(--v-theme-on-surface));
  line-height: 1.7;
  font-size: 14px;
}

</style>

