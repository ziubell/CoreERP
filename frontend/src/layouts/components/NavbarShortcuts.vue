<script setup lang="ts">
const shortcuts = [
  {
    icon: 'tabler-smart-home',
    title: 'Dashboard',
    subtitle: 'Panoramica',
    to: { name: 'dashboard' },
  },
]
</script>

<template>
  <Shortcuts :shortcuts="shortcuts" />
</template>
