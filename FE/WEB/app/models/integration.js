import Model, { attr } from '@ember-data/model';

export default class IntegrationModel extends Model {
  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) code;
  @attr('string', { defaultValue: '' }) url;
  @attr('boolean', { defaultValue: false }) active;
  @attr('string', { defaultValue: '' }) encryptionKey;
}
