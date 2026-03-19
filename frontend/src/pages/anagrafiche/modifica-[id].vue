<script setup lang="ts">
import AnagraficaForm from '@/components/anagrafiche/AnagraficaForm.vue'

const route = useRoute()
const id = computed(() => Number(route.params.id))
</script>

<template>
  <AnagraficaForm :id="id" />
</template>
