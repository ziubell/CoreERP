<script setup lang="ts">
definePage({
  meta: {
    subject: 'dashboard',
    action: 'read',
  },
})
</script>

<template>
  <div>
    <VRow>
      <VCol cols="12">
        <VCard>
          <VCardItem>
            <VCardTitle class="text-h5">
              CoreERP - Dashboard
            </VCardTitle>
          </VCardItem>
          <VCardText>
            <p class="text-body-1">
              Benvenuto nel sistema ERP. I moduli saranno disponibili a breve.
            </p>
          </VCardText>
        </VCard>
      </VCol>
    </VRow>
  </div>
</template>
