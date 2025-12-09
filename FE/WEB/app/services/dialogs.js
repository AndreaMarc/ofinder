/* eslint-disable ember/no-jquery */
import Service from '@ember/service';
import $ from 'jquery';

export default class DialogsService extends Service {
  /**
   * CONFIRM
   *
   * @param {html string} title : titolo del box. Se omesso, lo spazio del titolo non verrà mostrato
   * @param {html string} message : testo del messaggio di conferma (obbligatorio)
   * @param {function/null} ok_cb : callback eseguita alla pressione del tasto di conferma
   * @param {function/null} cancel_cb : callback eseguita alla pressione del tasto di annullamento
   * @param {array} labels : array di stringhe contenente, nell'ordine, il testo del pulsante di conferma e di quello di annullamento
   * @param {int} autoExec : In secondi. Se >0, indica il tempo dopo cui verrà automaticamente premuto ok. Se <0, indica il tempo di auto-cancellazione. Viene mostrato un contatore.
   */
  confirm(
    title = '',
    message = '',
    ok_cb = null,
    cancel_cb = null,
    labels,
    autoExec = 0
  ) {
    if (!message || message.toString().trim() === '') return false;

    if (!labels || typeof labels !== 'object' || labels.length < 2)
      labels = ['Ok', 'Annulla'];

    alertify.confirm().set({
      labels: { ok: labels[0].toString(), cancel: labels[1].toString() },
    });

    alertify.confirm().set({
      maximizable: false,
      reverseButtons: true,
      defaultFocus: 'cancel',
    });

    if (!title || title.toString().trim() === '') title = 'CONFERMA';

    alertify.confirm(title, message.toString().trim(), ok_cb, cancel_cb);

    if (autoExec && typeof autoExec !== 'undefined' && parseInt(autoExec) > 0) {
      alertify.confirm().autoOk(parseInt(autoExec));
    } else if (
      autoExec &&
      typeof autoExec !== 'undefined' &&
      parseInt(autoExec) < 0
    ) {
      alertify.confirm().autoCancel(parseInt(autoExec));
    }
  }

  /**
   * ALERT
   *
   * @param {html string} title : titolo della finestra di alert
   * @param {html string} message : messaggio dell'alert (obbligatorio)
   * @param {*} label : testo del pulsante
   * @param {*} cb : eventuale callback invocata al click dell'utente sul pulsante di conferma
   */
  alert(title = ' ', message = '', label = '', cb = null) {
    if (!message || message.toString().trim() === '') return false;
    if (!title || title.toString().trim() === '') title = '<br />';
    if (!label || label.toString().trim() === '') label = 'Ok';

    alertify.alert().set({ closable: false, label: label, movable: false });
    alertify.alert(title, message, cb);
  }

  /**
   * TOAST (notification)
   *
   * @param {html string} message : messaggio da mostrare (obbligatorio)
   * @param {string} type : stabilisce la colorazione. Possibili valori: message (bianco), success (verde), warning (arancio), error (rosso)
   * @param {string} position : posizione (top-left/center/right o bottom-left/center/right)
   * @param {int} delay : durata del tooltip in secondi. Se 0, rimane fino al click dell'utente
   * @param {function/null} close_cb : eventuale callback da eseguire alla chiusura del tooltip
   */
  toast(message = '', type = '', position, delay, close_cb = null) {
    if (message.toString().trim() === '') return false;

    type = type.toString().trim().toLowerCase();
    if ($.inArray(type, ['error', 'message', 'success', 'warning']) < 0)
      type = 'message';

    position = position.toString().trim().toLowerCase();
    if (
      $.inArray(position, [
        'top-left',
        'top-center',
        'top-right',
        'bottom-left',
        'bottom-center',
        'bottom-right',
      ]) < 0
    )
      position = 'bottom-right';

    alertify.set('notifier', 'position', position);
    alertify.notify(message.toString().trim(), type, delay, close_cb);
  }

  constructor(...attributes) {
    super(...attributes);
    //override defaults
    alertify.defaults.transition = 'zoom';
    alertify.defaults.theme.ok = 'btn btn-primary';
    alertify.defaults.theme.cancel = 'btn btn-danger';
    alertify.defaults.theme.input = 'form-control';
  }
}
