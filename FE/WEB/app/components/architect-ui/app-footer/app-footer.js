import Component from '@glimmer/component';

export default class ArchitectUiAppFooterAppFooterComponent extends Component {
  constructor(...attributes) {
    super(...attributes);
  }

  get isApp() {
    return typeof window.cordova !== 'undefined';
  }
}
