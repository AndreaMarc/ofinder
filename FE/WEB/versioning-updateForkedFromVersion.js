const fs = require('fs');

const packageJsonPath = './package.json';

// Legge il contenuto del package.json
const packageJson = JSON.parse(fs.readFileSync(packageJsonPath, 'utf8'));

// Aggiornare la versione di forkedFrom
function updateForkedFromVersion(newVersion) {
  packageJson.forkedFrom = packageJson.forkedFrom || {};
  packageJson.forkedFrom.version = newVersion;
}

// Aggiorna la versione di forkedFrom nel package.json del framework
updateForkedFromVersion(packageJson.version);

// Scrive il contenuto aggiornato nel package.json
fs.writeFileSync(packageJsonPath, JSON.stringify(packageJson, null, 2));

console.log(`Aggiornato forkedFrom.version a ${packageJson.version}`);
