import Transform from '@ember-data/serializer/transform';

export default class ObjectTransform extends Transform {
  // in lettura dal server
  deserialize(deserialized) {
    if (deserialized === '') deserialized = '{}';
    return JSON.parse(deserialized);
  }

  // in scrittura sul server
  serialize(serialized) {
    return JSON.stringify(serialized);
  }
}
