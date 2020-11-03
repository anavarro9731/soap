import i18next from 'i18next';
import { en } from './translations';
import languageKeys from './languages'

const defaultNs = 'translation';

(async function() {
  await i18next.init({
    interpolation: {
      //* react already does escaping
      escapeValue: false
    },
    lng: languageKeys.en,
    fallbackLng: languageKeys.en
  });
})();

i18next.addResourceBundle(languageKeys.en, defaultNs, en);

export const useNewLanguage = async (language, translations) => {
  const validLanguageAndTranslations =
    language &&
    translations &&
    typeof language === 'string' &&
    typeof translations === 'object';

  if (validLanguageAndTranslations) {
    i18next.addResourceBundle(language, defaultNs, translations);
    await i18next.changeLanguage(language);
  }
};
export const translate = key => i18next.t(key);

export default i18next;
