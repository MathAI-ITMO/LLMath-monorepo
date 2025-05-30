<template>
  <div class="llmath-problems-view">
    <h1>LLMath-Problems</h1>

    <div class="tabs">
      <button :class="{ active: activeTab === 'management' }" @click="activeTab = 'management'">
        Управление задачами
      </button>
      <button :class="{ active: activeTab === 'database' }" @click="activeTab = 'database'">
        База задач
      </button>
    </div>

    <!-- Вкладка 1: Управление задачами -->
    <div v-if="activeTab === 'management'" class="tab-content management-tab">
      <h2>Список задач</h2>
      <div v-if="loading && !problems.length" class="loading-message">Загрузка списка задач...</div>
      <div v-if="!loading && problems.length === 0 && attemptedLoad" class="info-message">Задачи не найдены. Вы можете добавить их ниже.</div>

      <table v-if="problems.length > 0" class="problems-list-simple">
        <thead>
          <tr>
            <th>Тип задачи</th>
            <th>Название задачи</th>
            <th>Условие (фрагмент)</th>
            <th>Решение (шаги)</th>
            <th>Решение LLM (фрагмент)</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="problem in problems" :key="problem._id || problem.id">
            <td>{{ getProblemAssignedTypes(problem._id || problem.id) || '-' }}</td>
            <td>{{ problem.title ? (problem.title.length > 5 ? problem.title.substring(0, 5) + '...' : problem.title) : '-' }}</td>
            <td>
              <pre class="statement-simple">{{ problem.statement.substring(0, 100) + (problem.statement.length > 100 ? '...' : '') }}</pre>
            </td>
            <td>{{ problem.solution?.steps?.length || 0 }}</td>
            <td>
              <pre class="llm-solution-preview-simple" v-if="problem.llm_solution">{{ typeof problem.llm_solution === 'string' ? problem.llm_solution.substring(0,50) + '...' : (problem.llm_solution && typeof problem.llm_solution === 'object' ? 'Объект (см. детали)' : 'См. детали') }}</pre>
              <span v-else>-</span>
            </td>
            <td>
              <button @click="showEditForm(problem)" class="small-btn btn-edit">Изменить</button>
              <button @click="deleteProblemByIdAndRefresh(problem._id || problem.id)" class="small-btn btn-delete" :disabled="apiCallLoading.deleteProblem">Удалить</button>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Форма редактирования задачи (на вкладке Управление задачами) -->
      <section v-if="editingProblem" class="api-section form-section edit-form-section">
        <h2>Редактировать задачу (ID: {{ editingProblem._id || editingProblem.id }})</h2>
        <div class="form-group">
          <label for="editTitle">Название задачи:</label>
          <input type="text" id="editTitle" v-model="currentEditProblem.title">
        </div>
        <div class="form-group">
          <label for="editProblemType">Тип задачи:</label>
          <input type="text" id="editProblemType" v-model="currentEditProblemType" list="existingTypesDatalist">
        </div>
        <div class="form-group">
          <label for="editStatement">Условие:</label>
          <textarea id="editStatement" v-model="currentEditProblem.statement" rows="3"></textarea>
        </div>
        <div class="form-group">
          <label for="editSolutionSteps">Решение (шаги, JSON массив объектов Step):</label>
          <textarea id="editSolutionSteps" v-model="currentEditProblemSolutionStepsJson" rows="4"></textarea>
        </div>
        <div class="form-group">
          <label for="editLlmSolution">Решение LLM (JSON/text):</label>
          <textarea id="editLlmSolution" v-model="currentEditProblemLlmSolutionJson" rows="3"></textarea>
        </div>
        <button @click="updateProblemFromManagementTab" :disabled="apiCallLoading.managementUpdateProblem">Обновить задачу</button>
        <button @click="cancelEdit" class="btn-cancel">Отмена</button>
      </section>

      <!-- Секция загрузки из GeoLin -->
      <section v-if="!editingProblem" class="api-section form-section geolin-load-section">
        <h2>Загрузить из GeoLin</h2>
        <div class="form-group">
          <label for="geolinPrefix">Префикс GeoLin:</label>
          <input type="text" id="geolinPrefix" v-model="geolinPrefixToLoad" list="geolinPrefixesDatalist">
          <datalist id="geolinPrefixesDatalist">
            <option v-for="prefix in availableGeolinPrefixes" :key="prefix" :value="prefix"></option>
          </datalist>
        </div>
        <button @click="loadFromGeolin" :disabled="apiCallLoading.loadFromGeolin">Загрузить данные из GeoLin</button>
      </section>

      <section v-if="!editingProblem" class="api-section form-section">
        <h2>Добавить новую задачу</h2>
        <div class="form-group">
          <label for="mgmtNewTitle">Название задачи:</label>
          <input type="text" id="mgmtNewTitle" v-model="managementNewProblem.title">
        </div>
        <div class="form-group">
          <label for="mgmtNewProblemType">Тип задачи (опционально):</label>
          <input type="text" id="mgmtNewProblemType" v-model="managementNewProblemType" list="existingTypesDatalist">
        </div>
        <div class="form-group">
          <label for="mgmtNewStatement">Условие:</label>
          <textarea id="mgmtNewStatement" v-model="managementNewProblem.statement" rows="12"></textarea>
        </div>
        <div class="form-group">
          <label for="mgmtNewGeoHash">GeoLin Hash:</label>
          <input type="text" id="mgmtNewGeoHash" v-model="managementNewProblem.geolin_ans_key.hash">
        </div>
         <div class="form-group">
          <label for="mgmtNewGeoSeed">GeoLin Seed (опционально, по умолчанию 0):</label>
          <input type="number" id="mgmtNewGeoSeed" v-model.number="managementNewProblem.geolin_ans_key.seed">
        </div>
        <div class="form-group">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
            <label for="mgmtNewLlmSolution">Решение LLM (опционально, JSON/text):</label>
            <button @click="getLlmSolution('management')" class="small-btn btn-llm-solution" :disabled="apiCallLoading.getLlmSolution || !managementNewProblem.statement">Получить решение LLM</button>
          </div>
          <div class="textarea-container">
            <textarea id="mgmtNewLlmSolution" v-model="managementNewProblemLlmSolutionJson" rows="12"></textarea>
            <div v-if="apiCallLoading.getLlmSolution && activeForm === 'management'" class="solution-loader">
              <div class="loader"></div>
              <div class="loader-text">Получаем решение от LLM...</div>
            </div>
          </div>
        </div>
        <div class="form-group">
          <label for="mgmtNewSolutionSteps">Решение (шаги, JSON массив объектов Step):</label>
          <textarea id="mgmtNewSolutionSteps" v-model="managementNewProblemSolutionStepsJson" rows="4"></textarea>
          <small>Пример: <code>[{"order": 1, "prerequisites": {}, "transition": {}, "outcomes": {}}]</code></small>
        </div>
        <button @click="addProblemFromManagementTab" :disabled="apiCallLoading.managementAddProblem">Добавить задачу</button>
      </section>

      <datalist id="existingTypesDatalist">
        <option v-for="type in allTypes" :key="type" :value="type"></option>
      </datalist>

    </div>

    <!-- Вкладка 2: База задач (Существующий контент) -->
    <div v-if="activeTab === 'database'" class="tab-content database-tab">
      <div class="controls main-controls">
        <button @click="fetchAllProblems" :disabled="loading">
          {{ loading ? 'Загрузка...' : 'Обновить Базу задач из LLMath-Problems' }}
        </button>
      </div>

      <div v-if="error" class="error-message">
        <p>Ошибка при загрузке задач:</p>
        <pre>{{ error }}</pre>
      </div>

      <div v-if="problems.length > 0" class="problems-list">
        <h2>Список задач в Базе ({{ problems.length }})</h2>
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Тип задачи</th>
              <th>Название задачи</th>
              <th>Условие</th>
              <th>GeoLin Key Hash</th>
              <th>GeoLin Key Seed</th>
              <th>Результат</th>
              <th>Решение LLM</th>
              <th>Действия</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="problem in problems" :key="problem._id || problem.id">
              <td>{{ problem._id || problem.id || '' }}</td>
              <td>{{ getProblemAssignedTypes(problem._id || problem.id) }}</td>
              <td>{{ problem.title ? (problem.title.length > 5 ? problem.title.substring(0, 5) + '...' : problem.title) : '-' }}</td>
              <td>
                <pre class="statement">{{ problem.statement }}</pre>
              </td>
              <td>{{ problem.geolin_ans_key?.hash ? (problem.geolin_ans_key.hash.length > 5 ? problem.geolin_ans_key.hash.substring(0, 5) + '...' : problem.geolin_ans_key.hash) : '' }}</td>
              <td>{{ problem.geolin_ans_key?.seed }}</td>
              <td><pre class="result">{{ problem.result || 'N/A' }}</pre></td>
              <td>
                <pre class="llm-solution-preview" v-if="problem.llm_solution">{{ typeof problem.llm_solution === 'string' ? problem.llm_solution.substring(0,50) + '...' : 'См. детали' }}</pre>
                <span v-else>-</span>
              </td>
              <td>
                <button @click="showProblemDetails(problem)" class="small-btn btn-details">Показать детали</button>
                <button @click="setProblemToUpdate(problem)" class="small-btn btn-edit">Изменить</button>
                <button @click="setProblemToDelete(problem._id || problem.id)" class="small-btn btn-delete">Удалить</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else-if="!loading && !error && attemptedLoad">
        <p>Задачи не найдены или еще не загружены.</p>
      </div>
      <section class="api-section">
        <h2>Создать новую задачу (в Базу)</h2>
        <div class="form-group">
          <label for="newTitle">Название задачи:</label>
          <input type="text" id="newTitle" v-model="newProblem.title">
        </div>
        <div class="form-group">
          <label for="newGeoHash">GeoLin Hash:</label>
          <input type="text" id="newGeoHash" v-model="newProblem.geolin_ans_key.hash">
        </div>
        <div class="form-group">
          <label for="newGeoSeed">GeoLin Seed:</label>
          <input type="number" id="newGeoSeed" v-model.number="newProblem.geolin_ans_key.seed">
        </div>
        <div class="form-group">
          <label for="newStatement">Условие:</label>
          <textarea id="newStatement" v-model="newProblem.statement" rows="12"></textarea>
        </div>
        <div class="form-group">
          <label for="newResult">Результат (опционально):</label>
          <input type="text" id="newResult" v-model="newProblem.result">
        </div>
        <div class="form-group">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
            <label for="newLlmSolution">Решение LLM (опционально, JSON/text):</label>
            <button @click="getLlmSolution('database')" class="small-btn btn-llm-solution" :disabled="apiCallLoading.getLlmSolution || !newProblem.statement">Получить решение LLM</button>
          </div>
          <div class="textarea-container">
            <textarea id="newLlmSolution" v-model="newProblemLlmSolutionJson" rows="12"></textarea>
            <div v-if="apiCallLoading.getLlmSolution && activeForm === 'database'" class="solution-loader">
              <div class="loader"></div>
              <div class="loader-text">Получаем решение от LLM...</div>
            </div>
          </div>
        </div>
        <div class="form-group">
          <label for="newSolutionSteps">Решение (шаги, JSON массив объектов Step):</label>
          <textarea id="newSolutionSteps" v-model="newProblemSolutionStepsJson" rows="5"></textarea>
          <small>Пример: <code>[{"order": 1, "prerequisites": {}, "transition": {}, "outcomes": {}}]</code></small>
        </div>
        <button @click="createProblem" :disabled="apiCallLoading.createProblem">Создать задачу</button>
        <div v-if="apiResponse.createProblem" class="api-response">
          <strong>Ответ:</strong> <pre>{{ JSON.stringify(apiResponse.createProblem, null, 2) }}</pre>
        </div>
      </section>
      <section class="api-section">
        <h2>Получить задачу по ID (из Базы)</h2>
        <div class="form-group">
          <label for="problemIdToFetch">ID Задачи:</label>
          <input type="text" id="problemIdToFetch" v-model="problemIdToFetch">
        </div>
        <button @click="fetchProblemById" :disabled="apiCallLoading.fetchProblemById">Получить задачу</button>
        <div v-if="foundProblemByIdList.length > 0" class="problems-list result-table">
          <h3>Результат:</h3>
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Тип задачи</th>
                <th>Название задачи</th>
                <th>Условие</th>
                <th>GeoLin Key Hash</th>
                <th>GeoLin Key Seed</th>
                <th>Результат</th>
                <th>Решение LLM</th>
                <th>Действия</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="problem in foundProblemByIdList" :key="problem._id || problem.id">
                <td>{{ problem._id || problem.id || '' }}</td>
                <td>{{ getProblemAssignedTypes(problem._id || problem.id) }}</td>
                <td>{{ problem.title ? (problem.title.length > 5 ? problem.title.substring(0, 5) + '...' : problem.title) : '-' }}</td>
                <td><pre class="statement">{{ problem.statement }}</pre></td>
                <td>{{ problem.geolin_ans_key?.hash ? (problem.geolin_ans_key.hash.length > 5 ? problem.geolin_ans_key.hash.substring(0, 5) + '...' : problem.geolin_ans_key.hash) : '' }}</td>
                <td>{{ problem.geolin_ans_key?.seed }}</td>
                <td><pre class="result">{{ problem.result || 'N/A' }}</pre></td>
                <td>
                  <pre class="llm-solution-preview" v-if="problem.llm_solution">{{ typeof problem.llm_solution === 'string' ? problem.llm_solution.substring(0,50) + '...' : 'См. детали' }}</pre>
                  <span v-else>-</span>
                </td>
                <td>
                  <button @click="showProblemDetails(problem)" class="small-btn btn-details">Показать детали</button>
                  <button @click="setProblemToUpdate(problem)" class="small-btn btn-edit">Изменить</button>
                  <button @click="setProblemToDelete(problem._id || problem.id)" class="small-btn btn-delete">Удалить</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="apiResponse.fetchProblemById && !foundProblemByIdList.length" class="api-response">
          <strong>Ответ/Ошибка:</strong> <pre>{{ JSON.stringify(apiResponse.fetchProblemById, null, 2) }}</pre>
        </div>
      </section>
      <section class="api-section">
        <h2>Обновить задачу (в Базе) (ID: {{ updateProblemData.id || 'не выбран'}})</h2>
         <p><small>Сначала выберите задачу для изменения из списка выше или введите ID и загрузите данные.</small></p>
        <div class="form-group">
          <label for="updateId">ID для обновления:</label>
          <input type="text" id="updateId" v-model="updateProblemData.id" placeholder="Введите ID и нажмите 'Загрузить для обновления'">
          <button @click="loadProblemForUpdate" :disabled="!updateProblemData.id || apiCallLoading.loadProblemForUpdate">Загрузить для обновления</button>
        </div>
        <div class="form-group">
          <label for="updateTitle">Название задачи:</label>
          <input type="text" id="updateTitle" v-model="updateProblemData.title">
        </div>
        <div class="form-group">
          <label for="updateStatement">Условие:</label>
          <textarea id="updateStatement" v-model="updateProblemData.statement"></textarea>
        </div>
        <div class="form-group">
          <label for="updateGeoHash">GeoLin Hash:</label>
          <input type="text" id="updateGeoHash" v-model="updateProblemData.geolin_ans_key.hash">
        </div>
        <div class="form-group">
          <label for="updateGeoSeed">GeoLin Seed:</label>
          <input type="number" id="updateGeoSeed" v-model.number="updateProblemData.geolin_ans_key.seed">
        </div>
        <div class="form-group">
          <label for="updateResult">Результат:</label>
          <input type="text" id="updateResult" v-model="updateProblemData.result">
        </div>
        <div class="form-group">
          <label for="updateSolutionSteps">Решение (шаги, JSON):</label>
          <textarea id="updateSolutionSteps" v-model="updateProblemSolutionStepsJson" rows="5"></textarea>
        </div>
        <div class="form-group">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
            <label for="updateLlmSolution">Решение LLM (JSON/text):</label>
            <button @click="getLlmSolution('update')" class="small-btn btn-llm-solution" :disabled="apiCallLoading.getLlmSolution || !updateProblemData.statement">Получить решение LLM</button>
          </div>
          <div class="textarea-container">
            <textarea id="updateLlmSolution" v-model="updateProblemLlmSolutionJson" rows="6"></textarea>
            <div v-if="apiCallLoading.getLlmSolution && activeForm === 'update'" class="solution-loader">
              <div class="loader"></div>
              <div class="loader-text">Получаем решение от LLM...</div>
            </div>
          </div>
        </div>
        <button @click="updateProblem" :disabled="!updateProblemData.id || apiCallLoading.updateProblem">Обновить задачу</button>
        <div v-if="apiResponse.updateProblem" class="api-response">
          <strong>Ответ:</strong> <pre>{{ JSON.stringify(apiResponse.updateProblem, null, 2) }}</pre>
        </div>
      </section>
      <section class="api-section">
        <h2>Удалить задачу по ID (из Базы)</h2>
        <div class="form-group">
          <label for="problemIdToDelete">ID Задачи:</label>
          <input type="text" id="problemIdToDelete" v-model="problemIdToDeleteValue">
        </div>
        <button @click="deleteProblemFromDbTab" :disabled="!problemIdToDeleteValue || apiCallLoading.deleteProblem">Удалить задачу</button>
        <div v-if="apiResponse.deleteProblem" class="api-response">
          <strong>Ответ:</strong> <pre>{{ JSON.stringify(apiResponse.deleteProblem, null, 2) }}</pre>
        </div>
      </section>
      <section class="api-section">
        <h2>Присвоить тип задаче (в Базе)</h2>
        <div class="form-group">
          <label for="problemIdToAssignType">ID Задачи:</label>
          <input type="text" id="problemIdToAssignType" v-model="typeAssignment.problem_id">
        </div>
        <div class="form-group">
          <label for="problemTypeToAssign">Тип:</label>
          <input type="text" id="problemTypeToAssign" v-model="typeAssignment.type_name">
        </div>
        <button @click="assignTypeToProblem" :disabled="!typeAssignment.problem_id || !typeAssignment.type_name || apiCallLoading.assignType">Присвоить тип</button>
        <div v-if="apiResponse.assignType" class="api-response">
          <strong>Ответ:</strong> <pre>{{ JSON.stringify(apiResponse.assignType, null, 2) }}</pre>
        </div>
      </section>
      <section class="api-section">
        <h2>Получить задачи по типу (из Базы)</h2>
        <div class="form-group">
          <label for="typeToFetchBy">Тип задачи:</label>
          <input type="text" id="typeToFetchBy" v-model="typeToFetchProblemsBy">
        </div>
        <button @click="fetchProblemsByType" :disabled="!typeToFetchProblemsBy || apiCallLoading.fetchProblemsByType">Найти задачи</button>
        <div v-if="foundProblemsByTypeList.length > 0" class="problems-list result-table">
          <h3>Результаты поиска по типу: "{{ typeToFetchProblemsBy }}"</h3>
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Тип задачи</th>
                <th>Название задачи</th>
                <th>Условие</th>
                <th>GeoLin Key Hash</th>
                <th>GeoLin Key Seed</th>
                <th>Результат</th>
                <th>Решение LLM</th>
                <th>Действия</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="problem in foundProblemsByTypeList" :key="problem._id || problem.id">
                <td>{{ problem._id || problem.id || '' }}</td>
                <td>{{ typeToFetchProblemsBy }}</td>
                <td>{{ problem.title ? (problem.title.length > 5 ? problem.title.substring(0, 5) + '...' : problem.title) : '-' }}</td>
                <td><pre class="statement">{{ problem.statement }}</pre></td>
                <td>{{ problem.geolin_ans_key?.hash ? (problem.geolin_ans_key.hash.length > 5 ? problem.geolin_ans_key.hash.substring(0, 5) + '...' : problem.geolin_ans_key.hash) : '' }}</td>
                <td>{{ problem.geolin_ans_key?.seed }}</td>
                <td><pre class="result">{{ problem.result || 'N/A' }}</pre></td>
                 <td>
                  <pre class="llm-solution-preview" v-if="problem.llm_solution">{{ typeof problem.llm_solution === 'string' ? problem.llm_solution.substring(0,50) + '...' : 'См. детали' }}</pre>
                  <span v-else>-</span>
                </td>
                 <td>
                  <button @click="showProblemDetails(problem)" class="small-btn btn-details">Показать детали</button>
                  <button @click="setProblemToUpdate(problem)" class="small-btn btn-edit">Изменить</button>
                  <button @click="setProblemToDelete(problem._id || problem.id)" class="small-btn btn-delete">Удалить</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="apiResponse.fetchProblemsByType && !foundProblemsByTypeList.length" class="api-response">
          <strong>Ответ/Ошибка:</strong> <pre>{{ JSON.stringify(apiResponse.fetchProblemsByType, null, 2) }}</pre>
        </div>
      </section>
      <section class="api-section">
        <h2>Все типы задач (в Базе)</h2>
        <button @click="fetchAllTypes" :disabled="apiCallLoading.fetchAllTypes">Загрузить все типы</button>
        <div v-if="allTypes.length > 0" class="api-response">
          <strong>Типы:</strong>
          <ul>
            <li v-for="typeItem in allTypes" :key="typeItem">{{ typeItem }}</li>
          </ul>
        </div>
        <div v-if="apiResponse.fetchAllTypesError" class="error-message">
           <pre>{{ JSON.stringify(apiResponse.fetchAllTypesError, null, 2) }}</pre>
        </div>
      </section>
    </div>

    <div v-if="selectedProblem" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <button class="close-button" @click="closeModal">×</button>
        <h2>Детали задачи: {{ selectedProblem._id }}</h2>
        <div>
          <strong>Название задачи:</strong>
          <pre>{{ selectedProblem.title || 'N/A' }}</pre>
        </div>
        <div>
          <strong>Условие:</strong>
          <pre>{{ selectedProblem.statement }}</pre>
        </div>
        <div>
          <strong>GeoLin Ans Key:</strong>
          <pre>{{ JSON.stringify(selectedProblem.geolin_ans_key, null, 2) }}</pre>
        </div>
        <div>
          <strong>Результат:</strong>
          <pre>{{ selectedProblem.result || 'N/A' }}</pre>
        </div>
        <div>
          <strong>Решение (шаги):</strong>
          <div v-if="selectedProblem.solution && selectedProblem.solution.steps && selectedProblem.solution.steps.length > 0">
            <ul>
              <li v-for="(step, index) in selectedProblem.solution.steps" :key="index">
                <strong>Шаг {{ step.order }}:</strong>
                <pre>{{ JSON.stringify(step, null, 2) }}</pre>
              </li>
            </ul>
          </div>
          <div v-else>
            <p>Решение отсутствует или не содержит шагов.</p>
          </div>
        </div>
         <div>
          <strong>Решение LLM:</strong>
          <pre>{{ selectedProblem.llm_solution ? JSON.stringify(selectedProblem.llm_solution, null, 2) : 'N/A' }}</pre>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, watch, computed } from 'vue';
import axios from 'axios';

const activeTab = ref('management'); // По умолчанию открыта первая вкладка

watch(activeTab, (newTab) => {
  if (newTab === 'management' && problems.value.length === 0 && !loading.value && !attemptedLoad.value) {
    // Если переключились на "Управление задачами" и задачи еще не загружались, загружаем их.
    // Это нужно, чтобы список задач на этой вкладке был актуален при первом открытии.
    fetchAllProblems();
  }
  if (newTab === 'management' && allTypes.value.length === 0 && !apiCallLoading.fetchAllTypes) {
    fetchAllTypes(); // Также загружаем типы, если их нет, при переключении на вкладку
  }
  if (newTab === 'database') {
    editingProblem.value = null; // Скрываем форму редактирования, если уходим с вкладки управления
  }
});

const LLMATH_PROBLEMS_API_URL_BASE = 'https://math-llm-problems.dev.mgsds.com';
const LLMATH_PROBLEMS_API_URL = `${LLMATH_PROBLEMS_API_URL_BASE}/api`;
const MATHLLM_BACKEND_API_URL = 'https://math-llm-back.dev.mgsds.com'; // URL основного бэкенда

interface GeoilonAnsKey {
  hash: string;
  seed: number;
}

interface Step {
  order: number;
  prerequisites?: Record<string, any>;
  transition?: Record<string, any>;
  outcomes?: Record<string, any>;
}

interface Solution {
  steps: Step[];
}

interface Problem {
  _id?: string;
  id?: string;
  title?: string;
  statement: string;
  geolin_ans_key: GeoilonAnsKey;
  result?: string;
  solution: Solution;
  llm_solution?: any;
}

interface ProblemWithTypePayload {
  type_name: string;
  problem_id: string;
}

// Интерфейс для ответа от GeoLin прокси
interface GeolinProblemData {
  name?: string;
  hash?: string;
  condition?: string;
  seed?: number;
  error?: string;
  problemParams?: string; // Добавляем поле для полного объекта problem_params
}

const problems = ref<Problem[]>([]);
const loading = ref(false);
const error = ref<any>(null);
const attemptedLoad = ref(false);
const selectedProblem = ref<Problem | null>(null);

const problemTypesMap = ref<Record<string, string[]>>({});

const foundProblemsByTypeList = ref<Problem[]>([]);
const foundProblemByIdList = ref<Problem[]>([]);

const apiCallLoading = reactive({
  createProblem: false,
  fetchProblemById: false,
  updateProblem: false,
  deleteProblem: false,
  assignType: false,
  fetchProblemsByType: false,
  fetchAllTypes: false,
  loadProblemForUpdate: false,
  managementAddProblem: false,
  managementUpdateProblem: false,
  loadFromGeolin: false,
  getLlmSolution: false,
});

const apiResponse = reactive<Record<string, any>>({
  createProblem: null,
  fetchProblemById: null,
  updateProblem: null,
  deleteProblem: null,
  assignType: null,
  fetchProblemsByType: null,
  fetchAllTypesError: null,
  managementAddProblem: null,
  managementUpdateProblem: null,
  loadFromGeolin: null,
});

const newProblem = reactive<Omit<Problem, '_id' | 'id'>>({
  title: '',
  statement: '',
  geolin_ans_key: { hash: '', seed: 0 },
  result: '',
  solution: { steps: [] },
  llm_solution: null,
});
const newProblemSolutionStepsJson = ref('[]');
const newProblemLlmSolutionJson = ref('');

const managementNewProblem = reactive<Omit<Problem, '_id' | 'id' | 'result'>>({
  title: '',
  statement: '',
  geolin_ans_key: { hash: '', seed: 0 },
  solution: { steps: [] },
  llm_solution: null,
});
const managementNewProblemSolutionStepsJson = ref('[]');
const managementNewProblemLlmSolutionJson = ref('');
const managementNewProblemType = ref('');

const problemIdToFetch = ref('');
const updateProblemData = reactive<Problem>({
  _id: '',
  title: '',
  statement: '',
  geolin_ans_key: { hash: '', seed: 0 },
  result: '',
  solution: { steps: [] },
  llm_solution: null,
});
const updateProblemSolutionStepsJson = ref('[]');
const updateProblemLlmSolutionJson = ref('');
const problemIdToDeleteValue = ref('');

const typeAssignment = reactive<ProblemWithTypePayload>({
  type_name: '',
  problem_id: '',
});
const typeToFetchProblemsBy = ref('');
const allTypes = ref<string[]>([]);

const editingProblem = ref<Problem | null>(null);
const currentEditProblem = reactive<Omit<Problem, '_id' | 'id' | 'geolin_ans_key' | 'result'>>({
  title: '',
  statement: '',
  solution: { steps: [] },
  llm_solution: null,
});
const currentEditProblemType = ref('');
const currentEditProblemSolutionStepsJson = ref('[]');
const currentEditProblemLlmSolutionJson = ref('');

const geolinPrefixToLoad = ref('');
const availableGeolinPrefixes = ref<string[]>([
  "tasks.linalg.linear_operators.matrix_decompositions.LU_decomposition.LU_decomposition_3x3",
  "tasks.linalg.linear_operators.matrix_decompositions.LU_decomposition.LU_decomposition_4x4",
  "tasks.linalg.linear_space.basis_transformation.basis_transformation_vector",
]);

const activeForm = ref<'management' | 'database' | 'update' | null>(null);

function showEditForm(problem: Problem) {
  editingProblem.value = JSON.parse(JSON.stringify(problem)); // Глубокое копирование
  if (editingProblem.value) {
    currentEditProblem.title = editingProblem.value.title || '';
    currentEditProblem.statement = editingProblem.value.statement;
    currentEditProblem.solution = { ...(editingProblem.value.solution || { steps: [] }) };
    currentEditProblem.llm_solution = editingProblem.value.llm_solution !== undefined ? editingProblem.value.llm_solution : null;

    currentEditProblemSolutionStepsJson.value = JSON.stringify(currentEditProblem.solution.steps, null, 2);
    currentEditProblemLlmSolutionJson.value = currentEditProblem.llm_solution
      ? (typeof currentEditProblem.llm_solution === 'string' ? currentEditProblem.llm_solution : JSON.stringify(currentEditProblem.llm_solution, null, 2))
      : '';

    // Получаем текущий тип задачи для редактирования
    const assignedTypes = problemTypesMap.value[editingProblem.value._id || editingProblem.value.id || ''] || [];
    currentEditProblemType.value = assignedTypes.length > 0 ? assignedTypes[0] : ''; // Берем первый тип, если их несколько (для простоты формы)
  }
}

function cancelEdit() {
  editingProblem.value = null;
  apiResponse.managementUpdateProblem = null;
}

async function updateProblemFromManagementTab() {
  if (!editingProblem.value || !(editingProblem.value._id || editingProblem.value.id)) {
    apiResponse.managementUpdateProblem = { error: true, message: "ID редактируемой задачи не найден." };
    return;
  }
  const problemIdToUpdate = editingProblem.value._id || editingProblem.value.id;

  try {
    const steps = JSON.parse(currentEditProblemSolutionStepsJson.value || '[]');
    const llmSolution = tryParseJson(currentEditProblemLlmSolutionJson.value, currentEditProblem.llm_solution);

    // В PUT запросе отправляем только те поля, которые редактируются на этой вкладке + GeoLin (т.к. он часть Problem)
    // Не отправляем result, т.к. его нет в форме редактирования на этой вкладке.
    const payload: Partial<Problem> = {
      title: currentEditProblem.title,
      statement: currentEditProblem.statement,
      solution: { steps },
      llm_solution: llmSolution,
      // Важно: geolin_ans_key нужно взять из оригинального editingProblem.value, т.к. оно не редактируется в этой форме
      geolin_ans_key: editingProblem.value.geolin_ans_key
    };

    const updateResponse = await makeApiCall(`/problems/${problemIdToUpdate}`, 'PUT', payload, 'managementUpdateProblem', 'managementUpdateProblem');

    if (updateResponse && !updateResponse.error) {
      // Обновление типа задачи, если он изменился
      const oldAssignedTypes = problemTypesMap.value[problemIdToUpdate || ''] || [];
      const oldType = oldAssignedTypes.length > 0 ? oldAssignedTypes[0] : '';
      const newType = currentEditProblemType.value.trim();

      if (newType !== oldType && problemIdToUpdate) {
        // Логика удаления старого типа (если он был) и присвоения нового
        // Это упрощенная логика: API не поддерживает удаление конкретной привязки тип-задача.
        // Мы просто присвоим новый тип. Если API /assign_type перезаписывает или добавляет, это ок.
        // Если нужно именно "изменить" тип, то бэкенд должен поддерживать удаление старой связи.
        // Пока предполагаем, что присвоение нового типа достаточно, или пользователь должен будет вручную управлять типами через вкладку "База задач"
        if (newType) { // Если новый тип не пустой
            const typePayload: ProblemWithTypePayload = { problem_id: problemIdToUpdate, type_name: newType };
            await makeApiCall('/assign_type', 'POST', typePayload, 'assignType', 'assignType'); // Можно использовать общий assignType ключ
        }
        // Удаление старого типа не реализовано через API (только удаление задачи целиком удаляет связи)
      }

      await fetchAllProblems(); // Обновляем список и карту типов
      cancelEdit(); // Скрываем форму
    }
  } catch (e) {
    console.error("Ошибка при обновлении задачи (management tab):", e);
    apiResponse.managementUpdateProblem = { error: true, message: "Ошибка парсинга JSON или API (management tab)", details: e };
  }
}

async function makeApiCall(endpoint: string, method: string, body?: any, loadingKey?: keyof typeof apiCallLoading, responseKey?: keyof typeof apiResponse) {
  if (loadingKey) apiCallLoading[loadingKey] = true;
  if (responseKey) apiResponse[responseKey] = null;
  if (responseKey === 'fetchAllTypesError' || loadingKey === 'fetchAllTypes') {
    apiResponse.fetchAllTypesError = null;
  }

  try {
    const options: RequestInit = {
      method,
      headers: {
        'Content-Type': 'application/json',
      },
    };
    if (body && (method === 'POST' || method === 'PUT')) {
      options.body = JSON.stringify(body);
    }
    const response = await fetch(`${LLMATH_PROBLEMS_API_URL}${endpoint}`, options);

    let responseData;
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.indexOf("application/json") !== -1) {
      responseData = await response.json();
    } else {
      responseData = await response.text();
    }

    if (!response.ok) {
      const errorDetail = typeof responseData === 'object' ? responseData : { message: responseData, status: response.status };
      throw errorDetail;
    }

    if (responseKey && responseKey !== 'fetchAllTypesError') {
      apiResponse[responseKey] = responseData;
    } else if (endpoint === '/types' && method === 'GET') {
        allTypes.value = responseData as string[];
    }
    return responseData;
  } catch (e: any) {
    console.error(`Ошибка при вызове ${method} ${LLMATH_PROBLEMS_API_URL}${endpoint}:`, e);
    if (responseKey && responseKey !== 'fetchAllTypesError') {
      apiResponse[responseKey] = { error: true, details: e };
    } else if (loadingKey === 'fetchAllTypes' || responseKey === 'fetchAllTypesError') {
        apiResponse.fetchAllTypesError = { error: true, details: e };
    } else if (endpoint === '/problems' && method === 'GET' && !loadingKey && !responseKey) {
        error.value = e;
    }
    return { error: true, details: e };
  } finally {
    if (loadingKey) apiCallLoading[loadingKey] = false;
  }
}

async function populateProblemTypesMap() {
  console.log("Attempting to populate problem types map...");
  if (!apiCallLoading.fetchAllTypes && allTypes.value.length === 0) {
      await fetchAllTypes(); // Убедимся, что типы загружены
  }

  if (allTypes.value && allTypes.value.length > 0 && !apiResponse.fetchAllTypesError) {
    console.log("Fetched unique types for map:", allTypes.value);

    const tempMap: Record<string, string[]> = {};

    for (const typeStr of allTypes.value) {
      const problemsForTypeResponse = await makeApiCall(`/get_problems_by_type?problem_type=${encodeURIComponent(typeStr)}`, 'GET');

      if (problemsForTypeResponse && !problemsForTypeResponse.error && Array.isArray(problemsForTypeResponse)) {
        const problemsWithType: Problem[] = problemsForTypeResponse;
        for (const problem of problemsWithType) {
          const problemId = problem._id || problem.id;
          if (problemId) {
            if (!tempMap[problemId]) {
              tempMap[problemId] = [];
            }
            if (!tempMap[problemId].includes(typeStr)) {
              tempMap[problemId].push(typeStr);
            }
          }
        }
      }
    }
    problemTypesMap.value = tempMap;
    console.log("Problem types map populated:", problemTypesMap.value);
  } else {
    console.warn('Could not fetch all types to build problemTypesMap or no types found.', apiResponse.fetchAllTypesError);
    problemTypesMap.value = {};
  }
}

function getProblemAssignedTypes(problemId: string | undefined): string {
  if (!problemId) return '';
  return problemTypesMap.value[problemId]?.join(', ') || '';
}

async function fetchAllProblems() {
  loading.value = true;
  error.value = null;
  attemptedLoad.value = true;
  foundProblemByIdList.value = [];
  foundProblemsByTypeList.value = [];
  apiResponse.fetchProblemById = null;
  apiResponse.fetchProblemsByType = null;

  try {
    const response = await fetch(`${LLMATH_PROBLEMS_API_URL}/problems`);
    if (!response.ok) {
      const errorData = await response.text();
      throw new Error(`HTTP error! status: ${response.status}, message: ${errorData}`);
    }
    const data = await response.json();
    problems.value = data;
    await fetchAllTypes(); // Загружаем типы
    if (problems.value.length > 0) {
      await populateProblemTypesMap();
    } else {
      problemTypesMap.value = {};
    }
  } catch (e) {
    console.error('Failed to fetch problems:', e);
    error.value = e;
  } finally {
    loading.value = false;
  }
}

function tryParseJson(jsonString: string, defaultValue: any = null) {
  if (!jsonString || jsonString.trim() === '') return defaultValue;
  try {
    return JSON.parse(jsonString);
  } catch (e) {
    console.warn("Failed to parse JSON, returning as text: ", jsonString, e);
    return jsonString;
  }
}

async function createProblem() {
  try {
    const steps = JSON.parse(newProblemSolutionStepsJson.value || '[]');
    const llmSolution = tryParseJson(newProblemLlmSolutionJson.value, null);

    const problemToCreate: Omit<Problem, '_id' | 'id'> = {
      ...newProblem,
      solution: { steps },
      llm_solution: llmSolution,
    };
    await makeApiCall('/problems', 'POST', problemToCreate, 'createProblem', 'createProblem');
    fetchAllProblems();
    newProblem.title = '';
    newProblem.statement = '';
    newProblem.geolin_ans_key = { hash: '', seed: 0 };
    newProblem.result = '';
    newProblemSolutionStepsJson.value = '[]';
    newProblemLlmSolutionJson.value = '';
    newProblem.llm_solution = null;

  } catch (e) {
    console.error("Ошибка парсинга JSON или при создании задачи:", e);
    apiResponse.createProblem = { error: true, message: "Ошибка парсинга JSON для шагов/LLM решения или API", details: e };
  }
}

async function fetchProblemById() {
  if (!problemIdToFetch.value) return;
  foundProblemByIdList.value = [];
  const responseData = await makeApiCall(`/problems/${problemIdToFetch.value}`, 'GET', undefined, 'fetchProblemById', 'fetchProblemById');
  if (responseData && !responseData.error) {
    foundProblemByIdList.value = [responseData as Problem];
    apiResponse.fetchProblemById = null;
  }
}

function setProblemToUpdate(problem: Problem) {
  updateProblemData.id = problem._id || problem.id || '';
  updateProblemData.title = problem.title || '';
  updateProblemData.statement = problem.statement;
  updateProblemData.geolin_ans_key = { ...(problem.geolin_ans_key || {hash:'', seed:0}) };
  updateProblemData.result = problem.result || '';
  updateProblemData.solution = { ...(problem.solution || { steps: [] }) };
  updateProblemData.llm_solution = problem.llm_solution !== undefined ? problem.llm_solution : null;

  updateProblemSolutionStepsJson.value = JSON.stringify(problem.solution?.steps || [], null, 2);
  updateProblemLlmSolutionJson.value = problem.llm_solution ? (typeof problem.llm_solution === 'string' ? problem.llm_solution : JSON.stringify(problem.llm_solution, null, 2)) : '';
}

async function loadProblemForUpdate() {
  if (!updateProblemData.id) return;
  apiCallLoading.loadProblemForUpdate = true;
  const problem = await makeApiCall(`/problems/${updateProblemData.id}`, 'GET');
  apiCallLoading.loadProblemForUpdate = false;
  if (problem && !problem.error) {
    setProblemToUpdate(problem);
    apiResponse.updateProblem = null;
  } else {
    apiResponse.updateProblem = { error: true, details: "Не удалось загрузить задачу для обновления." };
  }
}

async function updateProblem() {
  const idForUpdate = updateProblemData._id || updateProblemData.id;
  if (!idForUpdate) {
    apiResponse.updateProblem = { error: true, message: "ID для обновления не найден" };
    return;
  }
  try {
    const steps = JSON.parse(updateProblemSolutionStepsJson.value || '[]');
    const llmSolution = tryParseJson(updateProblemLlmSolutionJson.value, updateProblemData.llm_solution);

    const problemToUpdatePayload: Omit<Problem, '_id' | 'id'> & { id?: string } = {
      title: updateProblemData.title,
      statement: updateProblemData.statement,
      geolin_ans_key: updateProblemData.geolin_ans_key,
      result: updateProblemData.result,
      solution: { steps },
      llm_solution: llmSolution,
    };

    await makeApiCall(`/problems/${idForUpdate}`, 'PUT', problemToUpdatePayload, 'updateProblem', 'updateProblem');
    fetchAllProblems();
  } catch (e) {
     console.error("Ошибка парсинга JSON или при обновлении задачи:", e);
    apiResponse.updateProblem = { error: true, message: "Ошибка парсинга JSON или API", details: e };
  }
}

function setProblemToDelete(id: string | undefined) {
    if (id) {
        problemIdToDeleteValue.value = id;
    } else {
        console.warn("ID для удаления не предоставлен");
        apiResponse.deleteProblem = { error: true, message: "ID для удаления не предоставлен" };
    }
}

async function deleteProblemFromDbTab() {
  if (!problemIdToDeleteValue.value) return;
  await makeApiCall(`/problems/${problemIdToDeleteValue.value}`, 'DELETE', undefined, 'deleteProblem', 'deleteProblem');
  fetchAllProblems();
  problemIdToDeleteValue.value = '';
}

async function deleteProblemByIdAndRefresh(problemId: string | undefined) {
  if (!problemId) {
    console.warn("ID для удаления не предоставлен (management tab)");
    apiResponse.deleteProblem = { error: true, message: "ID для удаления не предоставлен (management tab)" };
    return;
  }
  await makeApiCall(`/problems/${problemId}`, 'DELETE', undefined, 'deleteProblem', 'deleteProblem');
  await fetchAllProblems();
}

async function addProblemFromManagementTab() {
  apiResponse.managementAddProblem = null;
  try {
    const steps = JSON.parse(managementNewProblemSolutionStepsJson.value || '[]');
    const llmSolution = tryParseJson(managementNewProblemLlmSolutionJson.value, null);

    const problemToCreatePayload: Omit<Problem, '_id' | 'id' | 'result'> = {
      title: managementNewProblem.title,
      statement: managementNewProblem.statement,
      geolin_ans_key: {
        hash: managementNewProblem.geolin_ans_key.hash,
        seed: Number(managementNewProblem.geolin_ans_key.seed) || 0,
      },
      solution: { steps },
      llm_solution: llmSolution,
    };

    const createdProblemResponse = await makeApiCall('/problems', 'POST', problemToCreatePayload, 'managementAddProblem', 'managementAddProblem');

    if (createdProblemResponse && !createdProblemResponse.error && (createdProblemResponse._id || createdProblemResponse.id)) {
      const newProblemId = createdProblemResponse._id || createdProblemResponse.id;
      apiResponse.managementAddProblem = { success: true, createdProblem: createdProblemResponse };

      if (managementNewProblemType.value.trim() !== '') {
        const typePayload: ProblemWithTypePayload = {
          problem_id: newProblemId,
          type_name: managementNewProblemType.value.trim(),
        };
        const assignTypeResponse = await makeApiCall('/assign_type', 'POST', typePayload, 'assignType', 'assignType');
         if (assignTypeResponse && !assignTypeResponse.error) {
            console.log("Тип успешно присвоен:", assignTypeResponse);
            if (typeof apiResponse.managementAddProblem === 'object' && apiResponse.managementAddProblem !== null) {
                 apiResponse.managementAddProblem.typeAssignment = assignTypeResponse;
            }
        } else {
            console.warn("Ошибка при присвоении типа:", assignTypeResponse?.details || 'Неизвестная ошибка');
             if (typeof apiResponse.managementAddProblem === 'object' && apiResponse.managementAddProblem !== null) {
                apiResponse.managementAddProblem.typeAssignmentError = assignTypeResponse?.details || 'Неизвестная ошибка при присвоении типа';
             }
        }
      }

      await fetchAllProblems();

      managementNewProblem.title = '';
      managementNewProblem.statement = '';
      managementNewProblem.geolin_ans_key = { hash: '', seed: 0 };
      managementNewProblemSolutionStepsJson.value = '[]';
      managementNewProblemLlmSolutionJson.value = '';
      managementNewProblem.llm_solution = null;
      managementNewProblemType.value = '';

    } else {
      console.error("Ошибка при создании задачи (management tab):", createdProblemResponse?.details);
    }

  } catch (e: any) {
    console.error("Ошибка парсинга JSON или другая ошибка при добавлении задачи (management tab):", e);
    apiResponse.managementAddProblem = { error: true, message: "Ошибка парсинга JSON или API (management tab)", details: e };
  }
}

async function assignTypeToProblem() {
  if (!typeAssignment.problem_id || !typeAssignment.type_name) return;
  const response = await makeApiCall('/assign_type', 'POST', typeAssignment, 'assignType', 'assignType');
  if (response && !response.error) {
    typeAssignment.type_name = '';
    typeAssignment.problem_id = '';
    await populateProblemTypesMap();
  }
}

async function fetchProblemsByType() {
  if (!typeToFetchProblemsBy.value) return;
  foundProblemsByTypeList.value = [];
  const responseData = await makeApiCall(`/get_problems_by_type?problem_type=${encodeURIComponent(typeToFetchProblemsBy.value)}`, 'GET', undefined, 'fetchProblemsByType', 'fetchProblemsByType');
  if (responseData && !responseData.error && Array.isArray(responseData)) {
    foundProblemsByTypeList.value = responseData as Problem[];
    apiResponse.fetchProblemsByType = null;
  }
}

async function fetchAllTypes() {
 await makeApiCall('/types', 'GET', undefined, 'fetchAllTypes', 'fetchAllTypesError');
}

function showProblemDetails(problem: Problem) {
  selectedProblem.value = problem;
}

function closeModal() {
  selectedProblem.value = null;
}

// Функция для вызова GeoLin прокси эндпоинта
async function fetchFromGeolinProxy(prefix: string) {
  apiCallLoading.loadFromGeolin = true;
  apiResponse.loadFromGeolin = null;
  try {
    // Запрашиваем задачу со случайным seed, генерируемым на сервере
    const url = `${MATHLLM_BACKEND_API_URL}/api/v1/geolin-proxy/problem-data?prefix=${encodeURIComponent(prefix)}`;
    console.log("Запрашиваем задачу со случайным seed");

    const response = await fetch(url);
    const data = await response.json();

    if (!response.ok) {
      const errorMsg = data.error || `HTTP error! status: ${response.status}`;
      console.error("Ошибка от GeoLin API:", data);
      throw new Error(typeof errorMsg === 'object' ? JSON.stringify(errorMsg) : errorMsg);
    }

    // Добавляем проверку полученных данных
    console.log("Получены данные от GeoLin:", data);

    apiResponse.loadFromGeolin = data;
    return data;
  } catch (e: any) {
    console.error("Ошибка при загрузке из GeoLin прокси:", e);
    apiResponse.loadFromGeolin = { error: e.message || 'Неизвестная ошибка при запросе к GeoLin прокси' };
    return null;
  } finally {
    apiCallLoading.loadFromGeolin = false;
  }
}

async function loadFromGeolin() {
  if (!geolinPrefixToLoad.value) {
    apiResponse.loadFromGeolin = { error: "Префикс GeoLin не может быть пустым." };
    return;
  }
  const data = await fetchFromGeolinProxy(geolinPrefixToLoad.value);
  if (data && !data.error) {
    console.log("Успешно получены данные от GeoLin:", data);

    managementNewProblem.title = data.name || '';
    managementNewProblem.statement = data.condition || '';
    managementNewProblem.geolin_ans_key.hash = data.hash || '';

    // Обновленная логика работы с seed - приоритет отдаём непосредственно seed из ответа
    if (data.seed !== undefined && data.seed !== null) {
      managementNewProblem.geolin_ans_key.seed = Number(data.seed);
      console.log("Полученный seed из GeoLin API:", data.seed);

      // Добавляем сообщение для пользователя
      const seedDiv = document.createElement('div');
      seedDiv.innerHTML = `<div style="color: green; margin-top: 10px; font-weight: bold;">Загружена задача с уникальным seed: ${data.seed}</div>`;
      setTimeout(() => {
        try {
          const seedField = document.getElementById('mgmtNewGeoSeed');
          if (seedField && seedField.parentNode) {
            seedField.parentNode.appendChild(seedDiv);
            setTimeout(() => {
              if (seedField.parentNode && seedDiv.parentNode === seedField.parentNode) {
                seedField.parentNode.removeChild(seedDiv);
              }
            }, 5000); // Убираем сообщение через 5 секунд
          }
        } catch (e) {
          console.error("Ошибка при создании уведомления о seed:", e);
        }
      }, 100);
    } else if (data.problemParams) {
      try {
        const paramObj = JSON.parse(data.problemParams);
        if (paramObj && typeof paramObj.seed === 'number') {
          managementNewProblem.geolin_ans_key.seed = paramObj.seed;
          console.log("Извлечён seed из problem_params:", paramObj.seed);
        } else {
          managementNewProblem.geolin_ans_key.seed = 0;
          console.log("В problem_params нет поля seed, устанавливаем значение по умолчанию: 0");
        }
      } catch (e) {
        console.error("Ошибка при парсинге problem_params:", e, data.problemParams);
        managementNewProblem.geolin_ans_key.seed = 0;
        console.log("Невозможно разобрать problem_params, устанавливаем seed=0");
      }
    } else {
      managementNewProblem.geolin_ans_key.seed = 0;
      console.log("Ни seed, ни problem_params не получены от GeoLin, устанавливаем значение по умолчанию: 0");
    }

    // Очищаем поля решения, так как они не приходят от GeoLin
    managementNewProblemSolutionStepsJson.value = '[]';
    managementNewProblem.solution = { steps: [] };
    managementNewProblemLlmSolutionJson.value = '';
    managementNewProblem.llm_solution = null;
  } else {
    console.error("Не удалось получить данные из GeoLin:", data?.error || "неизвестная ошибка");
  }
}

// Функция для получения решения от LLM
async function getLlmSolution(formType: 'management' | 'database' | 'update') {
  activeForm.value = formType;
  let problemStatement = '';

  if (formType === 'management') {
    problemStatement = managementNewProblem.statement;
  } else if (formType === 'database') {
    problemStatement = newProblem.statement;
  } else if (formType === 'update') {
    problemStatement = updateProblemData.statement;
  }

  if (!problemStatement) {
    alert('Поле "Условие" не может быть пустым для получения решения LLM');
    return;
  }

  apiCallLoading.getLlmSolution = true;

  try {
    // Создаем клиент axios с базовым URL и настройками для авторизации
    const client = axios.create({
      baseURL: MATHLLM_BACKEND_API_URL,
      withCredentials: true, // Важно для передачи cookies аутентификации
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // Отправляем запрос через axios
    const response = await client.post('/api/v1/llm/solve-problem', {
      problemDescription: problemStatement
    });

    const solution = response.data.solution;

    // Обновляем соответствующее поле в зависимости от типа формы
    if (formType === 'management') {
      managementNewProblemLlmSolutionJson.value = solution;
      managementNewProblem.llm_solution = solution;
    } else if (formType === 'database') {
      newProblemLlmSolutionJson.value = solution;
      newProblem.llm_solution = solution;
    } else if (formType === 'update') {
      updateProblemLlmSolutionJson.value = solution;
      updateProblemData.llm_solution = solution;
    }
  } catch (error) {
    console.error('Ошибка при получении решения от LLM:', error);
    // Более информативное сообщение об ошибке
    let errorMessage = 'Неизвестная ошибка';
    if (axios.isAxiosError(error)) {
      errorMessage = error.response?.status === 401
        ? 'Ошибка авторизации. Возможно, вам нужно выполнить вход в систему.'
        : `Ошибка: ${error.response?.status || 'сетевая ошибка'} - ${error.response?.data || error.message}`;
    } else if (error instanceof Error) {
      errorMessage = error.message;
    }
    alert(`Ошибка при получении решения от LLM: ${errorMessage}`);
  } finally {
    apiCallLoading.getLlmSolution = false;
  }
}

onMounted(() => {
  fetchAllProblems(); // Это также вызовет fetchAllTypes и populateProblemTypesMap
});

</script>

<style scoped>
.llmath-problems-view {
  padding: 20px;
  font-family: sans-serif;
}

.tabs {
  margin-bottom: 20px;
  border-bottom: 2px solid #ccc;
}

.tabs button {
  padding: 10px 20px;
  font-size: 16px;
  background-color: #00318b;
  color: white;
  border: none;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  margin-right: 5px;
  border-top-left-radius: 5px;
  border-top-right-radius: 5px;
}

.tabs button.active {
  background-color: #fff;
  color: #00318b;
  border-left: 1px solid #ccc;
  border-top: 1px solid #ccc;
  border-right: 1px solid #ccc;
  border-bottom: 2px solid #fff;
  font-weight: bold;
}

.tab-content {
  padding-top: 20px;
}

.controls {
  margin-bottom: 20px;
  padding: 10px;
  background-color: #f0f0f0;
  border: 1px solid #ccc;
  border-radius: 5px;
}

.main-controls {
  display: flex;
  align-items: center;
  gap: 15px;
}

.controls button {
  padding: 10px 15px;
  font-size: 16px;
  cursor: pointer;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
}

.controls button:disabled {
  background-color: #aaa;
  cursor: not-allowed;
}

.error-message {
  color: red;
  background-color: #ffe0e0;
  border: 1px solid red;
  padding: 10px;
  margin-bottom: 20px;
  border-radius: 4px;
}

.error-message pre, .api-response pre {
  white-space: pre-wrap;
  word-wrap: break-word;
  padding: 8px;
  border-radius: 4px;
}

.error-message pre {
  background-color: #f8f8f8;
  border: 1px solid #eee;
}

.problems-list table, .problems-list-simple table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 10px;
}

.problems-list th,
.problems-list td,
.problems-list-simple th,
.problems-list-simple td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: left;
  vertical-align: top;
}

.problems-list th, .problems-list-simple th {
  background-color: #00318b;
  color: #fff;
}

.problems-list-simple td {
  background-color: #2e2e2e;
  color: #f0f0f0;
  border: 1px solid #555;
}
.problems-list td {
  background-color: #3a3a3a;
  color: #f0f0f0;
  border: 1px solid #555;
}

.statement, .result, .llm-solution-preview, .llm-solution-preview-simple {
  white-space: pre-wrap;
  word-wrap: break-word;
  max-height: 100px;
  overflow-y: auto;
  background-color: #004b3b;
  color: #f0f0f0;
  padding: 5px;
  border-radius: 3px;
  font-size: 0.9em;
}
.llm-solution-preview, .llm-solution-preview-simple {
  max-height: 70px;
  background-color: #3b004b;
}
.llm-solution-preview-simple {
    background-color: #4a003b; /* Немного другой оттенок для simple таблицы, если нужно */
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background-color: #466579;
  color: #f0f0f0;
  padding: 30px;
  border-radius: 8px;
  width: 80%;
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
}

.close-button {
  position: absolute;
  top: 10px;
  right: 10px;
  font-size: 24px;
  background: none;
  border: none;
  cursor: pointer;
  color: #fff;
}

.modal-content pre {
  background-color: #000000;
  color: #f0f0f0;
  padding: 10px;
  border-radius: 4px;
  white-space: pre-wrap;
  word-wrap: break-word;
}

.api-section {
  margin-top: 30px;
  padding: 20px;
  border: 1px solid #444;
  border-radius: 8px;
  background-color: #333;
  color: #f0f0f0;
}
.api-section h2 {
  margin-top: 0;
  border-bottom: 1px solid #555;
  padding-bottom: 10px;
  margin-bottom: 20px;
  color: #fff;
}
.form-group {
  margin-bottom: 15px;
}
.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: bold;
  color: #e0e0e0;
}

.form-group input[type="text"],
.form-group input[type="number"],
.form-group textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #555;
  border-radius: 4px;
  box-sizing: border-box;
  background-color: #fff;
  color: #333;
}
.form-group textarea {
  min-height: 70px;
  resize: vertical;
}
.form-group small {
  font-size: 0.85em;
  color: #bbb;
}
.api-section button {
   padding: 10px 15px;
   font-size: 15px;
   background-color: #007bff;
   color: white;
   border: none;
   border-radius: 4px;
   cursor: pointer;
   margin-top:10px;
   margin-right: 10px;
}

.api-section button.btn-cancel {
    background-color: #6c757d;
}

.api-section button:disabled {
  background-color: #555;
  color: #999;
}
.api-response {
  margin-top: 20px;
  padding: 15px;
  background-color: #282c34;
  border: 1px solid #444;
  border-radius: 4px;
  color: #f0f0f0;
}
.api-response pre {
  background-color: #1e1e1e;
  padding: 10px;
  border-radius: 4px;
  border: 1px solid #383838;
  color: #d4d4d4;
}

.btn-details {
  background-color: #007bff !important;
  color: white !important;
}
.btn-edit {
  background-color: #ffc107 !important;
  color: #212529 !important;
}
.btn-delete {
    background-color: #dc3545 !important;
    color: white !important;
}
.small-btn {
    padding: 4px 8px !important;
    font-size: 0.8em !important;
    margin-right: 5px;
    margin-left: 0;
}
.small-btn:last-of-type {
    margin-right: 0;
}
.result-table {
  margin-top: 15px;
}
.result-table table {
    width: 100%;
    border-collapse: collapse;
}

.statement-simple {
  white-space: pre-wrap;
  word-wrap: break-word;
  max-height: 60px;
  overflow-y: auto;
  font-size: 0.9em;
  color: #ddd;
  margin: 0;
  padding: 0;
  background-color: transparent;
}

.form-section {
  padding: 20px;
  border-radius: 8px;
  border: 1px solid #444;
}

.edit-form-section {
  margin-bottom: 30px; /* Добавим отступ снизу для формы редактирования */
}

.form-section h2 {
   border-bottom: 1px solid #555;
   padding-bottom: 10px;
   margin-bottom: 20px;
}

.loading-message, .info-message {
    padding: 15px;
    margin-bottom: 20px;
    border-radius: 5px;
    text-align: center;
}
.loading-message {
    background-color: #2c3e50;
    color: #ecf0f1;
}
.info-message {
    background-color: #1abc9c;
    color: white;
}

.geolin-load-section {
  margin-bottom: 30px; /* Отступ между секцией GeoLin и формой добавления */
}

.btn-llm-solution {
  background-color: #17a2b8 !important;
  color: white !important;
  margin-top: 0 !important; /* Убираем верхний отступ */
  padding: 5px 15px !important; /* Немного увеличиваем горизонтальные отступы */
  font-size: 0.9em !important; /* Чуть уменьшаем размер шрифта */
}

/* Стили для контейнера с лоадером */
.textarea-container {
  position: relative;
}

.solution-loader {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  background-color: rgba(51, 51, 51, 0.7);
  border-radius: 4px;
}

.loader {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  width: 30px;
  height: 30px;
  animation: spin 1s linear infinite;
  margin-bottom: 10px;
}

.loader-text {
  color: #fff;
  font-size: 14px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
