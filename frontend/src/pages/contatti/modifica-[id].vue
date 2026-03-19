<script setup lang="ts">
import ContattoForm from '@/components/contatti/ContattoForm.vue'

const route = useRoute()
const id = computed(() => Number(route.params.id))
</script>

<template>
  <ContattoForm :id="id" />
</template>
