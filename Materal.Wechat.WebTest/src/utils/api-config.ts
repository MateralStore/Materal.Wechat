const STORAGE_KEY_API_URL = 'api_base_url'

export const getApiBaseUrl = (): string => {
  const stored = localStorage.getItem(STORAGE_KEY_API_URL)
  return stored || import.meta.env.VITE_API_TARGET || ''
}

export const setApiBaseUrl = (url: string): void => {
  if (url.trim()) {
    localStorage.setItem(STORAGE_KEY_API_URL, url.trim())
  } else {
    localStorage.removeItem(STORAGE_KEY_API_URL)
  }
}

export const getDefaultApiUrl = (): string => {
  return import.meta.env.VITE_API_TARGET || ''
}
