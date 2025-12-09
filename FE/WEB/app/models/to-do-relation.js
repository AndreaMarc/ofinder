import Model, { attr, belongsTo } from '@ember-data/model';

export default class ToDoRelationModel extends Model {
  @belongsTo('tenant', { async: true, inverse: null }) tenantDestination;
  @belongsTo('to-do', { async: true, inverse: 'toDoRelationsAsFather' })
  fatherToDo;
  @belongsTo('to-do', { async: true, inverse: 'toDoRelationsAsChild' })
  childToDo;

  @attr('string', { defaultValue: '' }) fatherToDoId;
  @attr('string', { defaultValue: '' }) childToDoId;
  @attr('string', { defaultValue: '' }) typology;
  @attr('number', { defaultValue: 0 }) tenantDestinationId;
}
