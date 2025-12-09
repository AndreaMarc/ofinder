const { execSync } = require('child_process');
const fs = require('fs');

// Funzione per eseguire un comando e restituire l'output
function execCommand(command) {
  return execSync(command, { stdio: 'inherit' });
}

// Funzione per ottenere l'URL del repository remoto
function getRemoteUrl() {
  try {
    return execSync('git config --get remote.origin.url').toString().trim();
  } catch (error) {
    console.error("Errore nel recupero dell'URL del repository remoto:", error);
    return '';
  }
}

// URL del repository del framework
const fwkRepoUrl = 'ember_fwk.git';

// Ottengo l'URL del repository remoto corrente
const remoteUrl = getRemoteUrl();
console.log('CURRENT URL', remoteUrl);

// Determino se il repository corrente è quello del framework
const isProjectA = remoteUrl.includes(fwkRepoUrl);

// Eseguo commit-and-tag-version
execCommand('commit-and-tag-version');

// Se siamo nel framework, eseguo anche lo script versioning-updateForkedFromVersion.js
if (isProjectA) {
  execCommand('node versioning-updateForkedFromVersion.js');
}
