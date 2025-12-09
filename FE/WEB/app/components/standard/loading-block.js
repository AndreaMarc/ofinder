import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
//import { action } from '@ember/object';
//import $ from 'jquery';

export default class StandardLoadingBlockComponent extends Component {
  @service taskTracker;

  constructor(...attributes) {
    super(...attributes);
  }
}
