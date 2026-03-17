<script setup lang="ts">
import { $api } from '@/utils/api'

const props = defineProps<{
  entitaTipo: string
  entitaId: number
}>()

const following = ref(false)
const loading = ref(true)

onMounted(async () => {
  try {
    const data = await $api<{ following: boolean }>(`/v1/sottoscrizioni/${props.entitaTipo}/${props.entitaId}`)
    following.value = data.following
  }
  catch {
    following.value = false
  }
  finally {
    loading.value = false
  }
})

const toggle = async () => {
  loading.value = true
  try {
    if (following.value) {
      await $api(`/v1/sottoscrizioni/${props.entitaTipo}/${props.entitaId}`, { method: 'DELETE' })
      following.value = false
    }
    else {
      await $api(`/v1/sottoscrizioni/${props.entitaTipo}/${props.entitaId}`, { method: 'POST' })
      following.value = true
    }
  }
  finally {
    loading.value = false
  }
}
</script>

<template>
  <VBtn
    :variant="following ? 'elevated' : 'tonal'"
    :color="following ? 'primary' : 'default'"
    :loading="loading"
    size="small"
    :prepend-icon="following ? 'tabler-bell-check' : 'tabler-bell-plus'"
    @click="toggle"
  >
    {{ following ? 'Seguendo' : 'Segui' }}
  </VBtn>
</template>
