const fs = require('fs');
const path = require('path');

// Get API URL from environment variable or use default
const apiUrl = process.env['VITE_API_URL'] || process.env['API_URL'] || '';

const envConfigFile = `export const environment = {
  production: true,
  apiUrl: '${apiUrl}'
};
`;

const targetPath = path.join(__dirname, '../src/environments/environment.ts');
fs.writeFileSync(targetPath, envConfigFile);

console.log(`Environment file generated at ${targetPath}`);
console.log(`API URL: ${apiUrl}`);
