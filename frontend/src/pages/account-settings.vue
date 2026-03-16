<script setup lang="ts">
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
]
</script>

<template>
  <div>
    <VTabs
      v-model="activeTab"
      class="v-tabs-pill"
    >
      <VTab
        v-for="item in tabs"
        :key="item.tab"
        :value="item.tab"
      >
        <VIcon
          size="20"
          start
          :icon="item.icon"
        />
        {{ item.title }}
      </VTab>
    </VTabs>

    <VWindow
      v-model="activeTab"
      class="mt-6 disable-tab-transition"
    >
      <VWindowItem value="profilo">
        <AccountSettingsProfile />
      </VWindowItem>

      <VWindowItem value="sicurezza">
        <AccountSettingsSecurity />
      </VWindowItem>
    </VWindow>
  </div>
</template>
