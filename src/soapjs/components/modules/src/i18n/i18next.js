import i18next from 'i18next';
import en from './translations/en';

i18next.init({
  interpolation: {
    // React already does escaping
    escapeValue: false,
  },
  lng: 'en',
  fallbackLng: 'en',
  resources: {
    en,
  },
});

export const useNewLanguage = (language, translations) => {
  const validLanguageAndTranslations =
    language &&
    translations &&
    typeof language === 'string' &&
    typeof translations === 'object';

  if (validLanguageAndTranslations) {
    i18next.addResourceBundle(language, 'translation', translations);
    i18next.changeLanguage(language);
  }
};

export default i18next;
