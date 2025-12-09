/**
 * COMPONENTE PER LA CREAZIONE DI UN ISTANZA DI CKEDITOR
 *
 * @param {string} element : ID da applicare alla text-area. Se vuoto viene utilizzato un valore univoco.
 * @param {string} defaultValue : stringa contenente il valore (html) di default
 * @param {string} reload : tracked. Al cambiamento, ricarica il componente.
 * @param {function} changeCallback : funzione da chiamare al cambiamento del contenuto html
 * @param {object} editorConfig : eventuale configurazione custom. es: { toolbar: ['bold', 'italic', 'underline'] };
 *
 *
 * ESEMPIO DI UTILIZZO
 * <Standard::CkEditor @element="myCkeEditor" @defaultValue="<h4>test</h4>" @changeCallback={{this.someThink}}/>
 *
 * NOTA:
 * Per aggiornare il contenuto dell'editor, modificare il valore del parametro @reload. Il cambiamento di defaultValue
 *  non viene monitorato.
 */
import Component from '@glimmer/component';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { v4 } from 'ember-uuid';

export default class StandardCkEditorComponent extends Component {
  @tracked defaultValue = '';
  @tracked element = '';
  @tracked available = true;
  editor = null;
  changeCallback = null;
  editorConfig = {};

  constructor(...attributes) {
    super(...attributes);

    if (this.args.element) {
      this.element = this.args.element;
    } else {
      this.element = `ck-editor-${v4()}`;
    }

    if (this.args.defaultValue && this.args.defaultValue !== '') {
      this.defaultValue = this.args.defaultValue;
    }

    if (this.args.changeCallback && this.args.changeCallback !== '') {
      this.changeCallback = this.args.changeCallback;
    } else {
      this.available = false;
    }

    if (this.args.editorConfig) {
      this.editorConfig = this.args.editorConfig;
    }
  }

  @action
  start() {
    setTimeout(() => {
      if (this.editor) {
        this.editor
          .destroy()
          .then(() => {
            this.editor = null;
            this.createEditor();
          })
          .catch((error) => {
            console.error(`Errore durante la distruzione dell'editor:`, error);
          });
      } else {
        this.createEditor();
      }
    }, 80);
  }

  createEditor() {
    if (this.args.defaultValue && this.args.defaultValue !== '') {
      this.defaultValue = this.args.defaultValue;
    }

    // eslint-disable-next-line no-undef
    ClassicEditor.create(
      document.querySelector(`#${this.element}`),
      this.editorConfig
    )
      .then((editor) => {
        this.editor = editor;

        // Imposta il contenuto iniziale dell'editor
        this.editor.setData(this.defaultValue);

        // Ascolta l'evento 'change:data' e chiama onEditorChange ogni volta che viene emesso
        this.editor.model.document.on('change:data', () => {
          this.onEditorChange(this.editor.getData());
        });
      })
      .catch((error) => {
        console.error(error);
      });
  }

  @action
  onEditorChange(newData) {
    // codice che viene eseguito quando il contenuto dell'editor cambia
    this.changeCallback(newData);
  }

  willDestroy() {
    super.willDestroy(...arguments);
    if (this.editor) {
      this.editor.destroy().catch((e) => {
        console.error("Errore durante la distruzione dell'editor:", e);
      });
    }
  }
}
