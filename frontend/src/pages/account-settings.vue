<script setup lang="ts">
import AccountSettingsNotifications from '@/views/pages/account-settings/AccountSettingsNotifications.vue'
import AccountSettingsProfile from '@/views/pages/account-settings/AccountSettingsProfile.vue'
import AccountSettingsSecurity from '@/views/pages/account-settings/AccountSettingsSecurity.vue'

definePage({
  meta: {
    navActiveLink: 'account-settings',
  },
})

const route = useRoute()
const activeTab = ref(route.query.tab?.toString() ?? 'profilo')

const tabs = [
  { title: 'Profilo', icon: 'tabler-user', tab: 'profilo' },
  { title: 'Sicurezza', icon: 'tabler-lock', tab: 'sicurezza' },
  { title: 'Notifiche', icon: 'tabler-bell', tab: 'notifiche' },
]
</script>

<template>
  <VRow>
    <!-- Left: Navigation -->
    <VCol cols="12" md="3">
      <h6 class="text-h6 mb-4">Impostazioni</h6>
      <VTabs
        v-model="activeTab"
        direction="vertical"
        class="v-tabs-pill"
      >
        <VTab
          v-for="item in tabs"
          :key="item.tab"
          :value="item.tab"
          :prepend-icon="item.icon"
        >
          {{ item.title }}
        </VTab>
      </VTabs>
    </VCol>

    <!-- Right: Content -->
    <VCol cols="12" md="9">
      <AccountSettingsProfile v-if="activeTab === 'profilo'" />
      <AccountSettingsSecurity v-if="activeTab === 'sicurezza'" />
      <AccountSettingsNotifications v-if="activeTab === 'notifiche'" />
    </VCol>
  </VRow>
</template>
