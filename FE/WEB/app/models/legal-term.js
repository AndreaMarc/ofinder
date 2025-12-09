import Model, { attr } from '@ember-data/model';

export default class LegalTermModel extends Model {
  @attr('string', { defaultValue: '' }) title; // varchar 250
  @attr('string', { defaultValue: '' }) note; // varchar max
  @attr('string', { defaultValue: '' }) code; // varchar 35
  @attr('string', { defaultValue: '' }) language; // varchar 2
  @attr('string', { defaultValue: '' }) content; // varchar max
  @attr('boolean', { defaultValue: false }) active;
  @attr('string', { defaultValue: '' }) version; // varchar 15
  @attr('date') dataActivation; // dateTime
}
