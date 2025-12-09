import Plugin from '@ckeditor/ckeditor5-core/src/plugin';
import ButtonView from '@ckeditor/ckeditor5-ui/src/button/buttonview';
import './fullscreen.css';

export default class Fullscreen extends Plugin {
  constructor(editor) {
    super(editor);

    editor.ui.componentFactory.add('fullscreen', (locale) => {
      const view = new ButtonView(locale);
      const fullscreenIcon = `
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path fill="currentColor" d="M5 3h4V0H3v6h2V3zm16-3h-4v3h2v3h6V0h-4zm-2 20h-2v3h6v-6h-2v3h-2v-3zm-12 0v-2H3v6h6v-2H5v-2h4v-3z"/>
          </svg>
      `;

      view.set({
        label: 'Toggle Fullscreen',
        icon: fullscreenIcon,
        tooltip: true,
      });

      // Callback executed when the button is clicked
      view.on('execute', () => {
        const editorElement = editor.ui.view.element; // editor.ui.view.editable.element;

        if (editorElement.classList.contains('ckeditor-fullscreen')) {
          editorElement.classList.remove('ckeditor-fullscreen');
        } else {
          editorElement.classList.add('ckeditor-fullscreen');
        }
      });

      return view;
    });
  }
}
