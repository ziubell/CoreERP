<script setup lang="ts">
definePage({
  meta: {
    layout: 'blank',
    public: true,
  },
})

const route = useRoute()
const router = useRouter()
const ability = useAbility()

onMounted(async () => {
  const { accessToken, userData, userAbilityRules, returnUrl } = route.query

  if (!accessToken || !userData) {
    await router.replace({ name: 'login' })
    return
  }

  try {
    const parsedUserData = JSON.parse(decodeURIComponent(userData as string))
    const parsedRules = JSON.parse(decodeURIComponent(userAbilityRules as string))

    console.log('Microsoft callback - parsedUserData:', parsedUserData)
    console.log('Microsoft callback - parsedRules:', parsedRules)
    console.log('Microsoft callback - accessToken length:', (accessToken as string).length)

    useCookie('accessToken').value = decodeURIComponent(accessToken as string)
    useCookie('userData').value = parsedUserData
    useCookie('userAbilityRules').value = parsedRules

    ability.update(parsedRules)

    console.log('Microsoft callback - ability updated, can read dashboard:', ability.can('read', 'dashboard'))
    console.log('Microsoft callback - cookies set, redirecting...')

    await nextTick(() => {
      router.replace(returnUrl ? decodeURIComponent(returnUrl as string) : '/')
    })
  }
  catch (err) {
    console.error('Microsoft callback error:', err)
    await router.replace({ name: 'login' })
  }
})
</script>

<template>
  <div class="d-flex align-center justify-center" style="min-height: 100vh;">
    <VProgressCircular
      indeterminate
      color="primary"
      size="64"
    />
  </div>
</template>
