<script setup lang="ts">
import { PerfectScrollbar } from 'vue3-perfect-scrollbar'

const router = useRouter()
const ability = useAbility()

// TODO: Get type from backend
const userData = useCookie<any>('userData')

const initials = computed(() => {
  if (!userData.value)
    return '?'

  const fullName = userData.value.fullName ?? ''
  const parts = fullName.trim().split(/\s+/)

  if (parts.length >= 2)
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()

  if (parts.length === 1 && parts[0])
    return parts[0][0].toUpperCase()

  return '?'
})

const logout = async () => {
  // Remove "accessToken" from cookie
  useCookie('accessToken').value = null

  // Remove "userData" from cookie
  userData.value = null

  // Redirect to login page
  await router.push('/login')

  // Remove "userAbilities" from cookie
  useCookie('userAbilityRules').value = null

  // Reset ability to initial ability
  ability.update([])
}

const userProfileList = [
  { type: 'divider' },
  {
    type: 'navItem',
    icon: 'tabler-user-cog',
    title: 'Profilo',
    to: { name: 'account-settings' },
  },
]
</script>

<template>
  <VBadge
    v-if="userData"
    dot
    bordered
    location="bottom right"
    offset-x="1"
    offset-y="2"
    color="success"
  >
    <VAvatar
      size="38"
      class="cursor-pointer"
      :color="!(userData && userData.avatar) ? 'primary' : undefined"
      :variant="!(userData && userData.avatar) ? 'tonal' : undefined"
    >
      <VImg
        v-if="userData && userData.avatar"
        :src="userData.avatar"
      />
      <span
        v-else
        class="text-sm font-weight-medium"
      >
        {{ initials }}
      </span>

      <!-- SECTION Menu -->
      <VMenu
        activator="parent"
        width="240"
        location="bottom end"
        offset="12px"
      >
        <VList>
          <VListItem>
            <div class="d-flex gap-2 align-center">
              <VListItemAction>
                <VBadge
                  dot
                  location="bottom right"
                  offset-x="3"
                  offset-y="3"
                  color="success"
                  bordered
                >
                  <VAvatar
                    :color="!(userData && userData.avatar) ? 'primary' : undefined"
                    :variant="!(userData && userData.avatar) ? 'tonal' : undefined"
                  >
                    <VImg
                      v-if="userData && userData.avatar"
                      :src="userData.avatar"
                    />
                    <span
                      v-else
                      class="text-sm font-weight-medium"
                    >
                      {{ initials }}
                    </span>
                  </VAvatar>
                </VBadge>
              </VListItemAction>

              <div>
                <h6 class="text-h6 font-weight-medium">
                  {{ userData.fullName || userData.username }}
                </h6>
                <VListItemSubtitle class="text-capitalize text-disabled">
                  {{ userData.role }}
                </VListItemSubtitle>
              </div>
            </div>
          </VListItem>

          <PerfectScrollbar :options="{ wheelPropagation: false }">
            <template
              v-for="item in userProfileList"
              :key="item.title"
            >
              <VListItem
                v-if="item.type === 'navItem'"
                :to="item.to"
              >
                <template #prepend>
                  <VIcon
                    :icon="item.icon"
                    size="22"
                  />
                </template>

                <VListItemTitle>{{ item.title }}</VListItemTitle>

                <template
                  v-if="item.badgeProps"
                  #append
                >
                  <VBadge
                    rounded="sm"
                    class="me-3"
                    v-bind="item.badgeProps"
                  />
                </template>
              </VListItem>

              <VDivider
                v-else
                class="my-2"
              />
            </template>

            <div class="px-4 py-2">
              <VBtn
                block
                size="small"
                color="error"
                append-icon="tabler-logout"
                @click="logout"
              >
                Logout
              </VBtn>
            </div>
          </PerfectScrollbar>
        </VList>
      </VMenu>
      <!-- !SECTION -->
    </VAvatar>
  </VBadge>
</template>
