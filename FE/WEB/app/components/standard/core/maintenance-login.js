/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
//import { htmlSafe } from '@ember/template';
import $ from 'jquery';
//import { getOwner } from '@ember/application';

export default class StandardCoreMaintenanceLoginComponent extends Component {
  @service('siteSetup') stp;
  @service dialogs;

  @tracked bgColor = 'bg-secondary';
  @tracked logoStyle = 'app-logo-inverse';
  @tracked showLogin = false;

  constructor(...attributes) {
    super(...attributes);
    let setup = this.stp.siteSetup;

    this.bgColor = setup.headerBackground;
    this.logoStyle =
      setup.headerLight === 'black' ? 'app-logo' : 'app-logo-inverse';

    setTimeout(() => {
      $('#maintenance-smartwizard').smartWizard({
        selected: 0,
        autoAdjustHeight: false,
        backButtonSupport: false,
        enableUrlHash: false,
        transition: {
          animation: 'slideHorizontal', // Animation effect on navigation, none|fade|slideHorizontal|slideVertical|slideSwing|css(Animation CSS class also need to specify)
          speed: '400', // Animation speed. Not used if animation is 'css'
        },
        toolbar: {
          position: 'none', // none|top|bottom|both
          showNextButton: false, // show/hide a Next button
          showPreviousButton: false, // show/hide a Previous button
        },
        keyboard: {
          keyNavigation: false,
        },
      });
      $('#maintenance-smartwizard').smartWizard('next');
    }, 80);
  }

  @action
  changeStep(stepNumber) {
    if (stepNumber === 1) {
      $('#maintenance-smartwizard').smartWizard('next');
    } else {
      $('#maintenance-smartwizard').smartWizard('prev');
    }

    this.showLogin = !this.showLogin;
  }
}
