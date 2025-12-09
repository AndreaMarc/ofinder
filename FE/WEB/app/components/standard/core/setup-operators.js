import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';

export default class StandardCoreSetupOperatorsComponent extends Component {
  @service jsUtility;
  @service dialogs;

  @tracked recordApp;
  @tracked recordWeb;
  @tracked serviceAvailable = 'waiting';
  @tracked newUser = { lastName: '', firstName: '', email: '' };
  @tracked maeUsers = [];
  @tracked allSaved = true;
  @tracked emptyLastName = [];
  @tracked emptyFirstName = [];
  @tracked errorEmail = [];

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    // recupero i record di setup
    this.recordWeb = this.args.recordWeb;
    this.recordApp = this.args.recordApp;
    this.serviceAvailable = this.args.serviceAvailable;

    this.maeUsers =
      typeof this.recordWeb.maeUsers === 'object'
        ? this.recordWeb.maeUsers
        : [];
  }

  @action
  newValue(field, event) {
    this.newUser[field] = event.target.value.trim();
  }

  @action
  saveNewUser() {
    if (
      this.newUser.lastName === '' ||
      this.newUser.firstName === '' ||
      this.newUser.email === ''
    ) {
      this.dialogs.toast(
        'Tutti i campi sono obbligatori!',
        'error',
        'bottom-right',
        3
      );
      return false;
    }

    let regex = this.jsUtility.regex('email');
    if (!regex.test(this.newUser.email)) {
      this.dialogs.toast(
        'Il formato del campo email non è corretto!',
        'error',
        'bottom-right',
        5
      );
      return false;
    }
    let users = this.maeUsers;
    users.push(this.newUser);
    this.maeUsers = users;
    this.newUser = { lastName: '', firstName: '', email: '' };
    this.saveAll.perform();
  }

  saveAll = task({ drop: true }, async () => {
    let regex = this.jsUtility.regex('email');
    let error = false;
    let a = [];
    let b = [];
    let c = [];

    this.maeUsers.forEach((element, index) => {
      if (element.lastName === '') {
        a.push(index);
        error = true;
      }

      if (element.firstName === '') {
        b.push(index);
        error = true;
      }
      if (element.email === '' || !regex.test(element.email)) {
        c.push(index);
        error = true;
      }
    });
    if (error) {
      this.emptyLastName = a;
      this.emptyFirstName = b;
      this.errorEmail = c;

      this.dialogs.toast(
        'Tutti i campi sono obbligatori e le email devono rispettare il formato corretto.<br /><br />I campi errati sono evidenziati in rosso',
        'error',
        'bottom-right',
        7
      );
      return false;
    }

    this.maeUsers.sort((a, b) => {
      if (a.lastName < b.lastName) {
        return -1;
      }
      if (a.lastName > b.lastName) {
        return 1;
      }
      return 0;
    });

    this.recordWeb.maeUsers = this.maeUsers;
    this.recordApp.maeUsers = this.maeUsers;
    this.maeUsers = this.recordWeb.maeUsers;
    await this.recordWeb.save();
    await this.recordApp.save();
  });

  get eln() {
    return this.emptyLastName;
  }

  @action
  updateValue(index, field, event) {
    this.maeUsers[index][field] = event.target.value.trim();
    this.allSaved = false;
  }

  @action
  deleteUser(index) {
    let users = this.maeUsers;
    users.splice(index, 1);
    this.maeUsers = users;
    this.allSaved = false;
  }
}
