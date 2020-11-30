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
        lng: "en",
        fallbackLng: 'en',
        defaultNs,
        debug: true,
        resources: {
            en,
        }
    });
})();

export const addTranslations = (translations) => {
    i18next.addResourceBundle('en', defaultNs, translations);
};
export const translate = key => i18next.t(key);

export default i18next;
