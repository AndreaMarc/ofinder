import Model, { attr } from '@ember-data/model';

export default class TranslationModel extends Model {
  @attr('string', { defaultValue: 'it' }) languageCode;
  @attr('arrays', { defaultValue: '{}' }) translationWeb;
  @attr('arrays', { defaultValue: '{}' }) translationApp;
}
