/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { htmlSafe } from '@ember/template';
import { task } from 'ember-concurrency';
export default class StandardCoreSetupSecurityComponent extends Component {
  @service jsUtility;
  @service siteSetup;
  @service dialogs;
  @service store;

  @tracked converted = '';
  @tracked convertedInverse = '';
  @tracked recordApp;
  @tracked recordWeb;
  @tracked serviceAvailable = 'waiting';
  @tracked savedChanges = true;
  @tracked passwordError = '';

  @tracked defaultClaims = [];
  @tracked newClaim = {};

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    this.recordWeb = this.args.recordWeb;
    this.recordApp = this.args.recordApp;
    this.serviceAvailable = this.args.serviceAvailable;
    this.defaultClaims = this.recordWeb.defaultClaims || [];
    this.newClaim = { name: '', description: '' };
  }

  @action
  changeValue(environment, field, event) {
    let val = event.target.value;
    if (['useUrlStaticFiles', 'canChangeTenants', 'useMD5'].includes(field)) {
      val = !!val;
    }

    if (field === 'defaultUserPassword' && event.target.value !== '') {
      this.passwordError = htmlSafe(
        this.jsUtility.verifyPassword(event.target.value)
      );
    }

    if (
      typeof environment === 'undefined' ||
      !['app', 'web'].includes(environment)
    ) {
      environment = 'both';
    }

    switch (environment) {
      case 'both':
        this.recordWeb[field] = val;
        this.recordApp[field] = val;
        break;
      case 'app':
        this.recordApp[field] = val;
        break;
      case 'web':
        this.recordWeb[field] = val;
        break;
    }

    if (
      this.recordWeb.hasDirtyAttributes ||
      this.recordApp.hasDirtyAttributes
    ) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }
  }

  save = task({ drop: true }, async () => {
    try {
      let regex = this.jsUtility.regex('validPassword');
      if (!regex.test(this.recordWeb.defaultUserPassword)) {
        this.dialogs.toast(
          'Il formato della password non è corretto',
          'error',
          'bottom-right',
          3
        );
        return false;
      }

      if (
        this.recordApp.accessTokenExpiresIn >
          this.recordApp.refreshTokenExpiresIn ||
        this.recordWeb.accessTokenExpiresIn >
          this.recordWeb.refreshTokenExpiresIn
      ) {
        this.dialogs.toast(
          `La durata dell'Access Token non può essere superiore a quella del Refresh Token`,
          'error',
          'bottom-right',
          3
        );
        return false;
      }

      let arr = this.recordWeb.defaultClaims;
      arr.sort((a, b) => a.name.localeCompare(b.name)); // ordinamento alfabetico
      this.recordWeb.defaultClaims = arr;
      this.recordApp.defaultClaims = arr;

      await this.recordWeb.save();
      await this.recordApp.save();

      this.defaultClaims = arr;

      let origin =
        typeof window.cordova !== 'undefined' ? this.recordApp : this.recordWeb;

      this.siteSetup.setSetup('maintenance', origin.maintenance);
      this.siteSetup.setSetup('minAppVersion', origin.minAppVersion);
      this.siteSetup.setSetup('publicRegistration', origin.publicRegistration);
      this.siteSetup.setSetup('disableLog', origin.disableLog);
      this.savedChanges = true;

      this.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 3);
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
      );
    }
  });

  // creazione nuovo claim
  @action
  updateClaim(field, index, event) {
    this.defaultClaims[index][field] = event.target.value.trim();

    this.recordWeb.defaultClaims = this.defaultClaims;
    this.recordApp.defaultClaims = this.defaultClaims;
    this.savedChanges = false;
  }

  @action
  removeClaim(index) {
    let claimsArray = this.defaultClaims;
    claimsArray.splice(index, 1);
    this.defaultClaims = claimsArray;

    this.recordWeb.defaultClaims = this.defaultClaims;
    this.recordApp.defaultClaims = this.defaultClaims;

    this.savedChanges = false;
  }

  @action
  populateNewClaim(field, event) {
    this.newClaim[field] = event.target.value.trim();
  }

  @action
  addClaim() {
    if (
      !this.newClaim.name ||
      this.newClaim.name === '' ||
      !this.newClaim.description ||
      this.newClaim.description === ''
    ) {
      this.dialogs.toast(
        'Nome e descrizione sono obbligatori!',
        'warning',
        'bottom-right',
        3
      );
      return false;
    }

    let claimsArray = this.defaultClaims;
    claimsArray.push(this.newClaim);
    claimsArray.sort((a, b) => a.name.localeCompare(b.name)); // ordinamento alfabetico
    this.defaultClaims = claimsArray;
    this.newClaim = { name: '', description: '' };

    this.recordWeb.defaultClaims = this.defaultClaims;
    this.recordApp.defaultClaims = this.defaultClaims;
    this.siteSetup.siteSetup.defaultClaims = this.defaultClaims;

    this.savedChanges = false;
  }

  // converte giorni in minuti
  @action
  converter(event) {
    if (event.target.value === '') {
      this.converted = '';
    } else if (isNaN(event.target.value)) {
      this.converted = '?';
    } else {
      this.converted = parseInt(event.target.value) * 24 * 60;
    }
  }
  // converte minuti in giorni
  @action
  converterInverse(event) {
    if (event.target.value === '') {
      this.convertedInverse = '';
    } else if (isNaN(event.target.value)) {
      this.convertedInverse = '?';
    } else {
      this.convertedInverse = parseInt(parseInt(event.target.value) / 24 / 60);
    }
  }
}
