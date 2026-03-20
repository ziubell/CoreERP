import { ofetch } from 'ofetch'

export const $api = ofetch.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  async onRequest({ options }) {
    const accessToken = useCookie('accessToken').value

    // Ensure headers is always a proper Headers instance.
    // ofetch's mergeHeaders should do this, but some environments
    // or versions may leave it as a plain object.
    if (!(options.headers instanceof Headers)) {
      options.headers = new Headers(options.headers as HeadersInit || {})
    }

    if (accessToken) {
      options.headers.set('Authorization', `Bearer ${accessToken}`)
    }
  },
  async onResponseError({ response }) {
    if (response.status === 401) {
      useCookie('accessToken').value = null
      useCookie('userData').value = null

      // Redirect al login se non siamo già sulla pagina login
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
  },
})
