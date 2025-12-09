import Transform from '@ember-data/serializer/transform';

export default class DateUtcTransform extends Transform {
  deserialize(serialized) {
    if (serialized) {
      return new Date(serialized);
    }
    return serialized;
  }

  serialize(deserialized) {
    if (deserialized instanceof Date) {
      const serializedDate = deserialized.toISOString();
      console.log('Serializing date:', serializedDate); // Log per debug
      return serializedDate;
    }
    return deserialized;
  }
}
