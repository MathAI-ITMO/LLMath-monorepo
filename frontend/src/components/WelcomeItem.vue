<template>
  <div class="welcome-item pa-4">
    <div class="d-flex align-center">
      <div class="icon-container mr-4">
        <slot name="icon"></slot>
      </div>
      <div class="content flex-grow-1">
        <h3 class="text-h6 mb-2">
          <slot name="heading"></slot>
        </h3>
        <div class="text-body-1">
          <slot></slot>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.welcome-item {
  border-bottom: 1px solid var(--v-border-color);
}

.welcome-item:last-child {
  border-bottom: none;
}

.icon-container {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
}

@media (max-width: 600px) {
  .welcome-item {
    padding: 1rem;
  }

  .icon-container {
    width: 32px;
    height: 32px;
  }
}
</style>
