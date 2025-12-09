import Model, { attr, belongsTo } from '@ember-data/model';

export default class ToDoModel extends Model {
  @belongsTo('user', { async: true, inverse: null }) author;
  @belongsTo('to-do-relation', { async: true, inverse: 'fatherToDo' })
  toDoRelationsAsFather;
  @belongsTo('to-do-relation', { async: true, inverse: 'childToDo' })
  toDoRelationsAsChild;

  @attr('string', { defaultValue: '' }) authorId;
  @attr('string', { defaultValue: '' }) title;
  @attr('string', { defaultValue: '' }) note;
  @attr('string', { defaultValue: '' }) status;
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  createdAt;
  @attr('date-utc') closedAt;
}
