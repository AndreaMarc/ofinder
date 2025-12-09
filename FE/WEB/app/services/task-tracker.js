/**
 * Servizio da utilizzare con i Task per far comparire il
 * loader di caricamento in overlay.
 *
 * UTILIZZO:
 * Nei componenti/controller, usare

  @service taskTracker;

  myTask = task({ drop: true }, async () => {
    return this.taskTracker.track(async () => { // copiare questa riga!
      // @il_tuo_codice_asincrono
      await this.fetchData();
    });
  });

 *
  NOTE: il componente loading-block.js é inserito in application.hbs
 */

import Service from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { next } from '@ember/runloop';

export default class TaskTrackerService extends Service {
  @tracked activeTasks = 0;

  get isBusy() {
    return this.activeTasks > 0;
  }

  async track(fn) {
    next(this, () => {
      this.activeTasks++;
    });

    try {
      return await fn();
    } finally {
      next(this, () => {
        this.activeTasks--;
      });
    }
  }
}
