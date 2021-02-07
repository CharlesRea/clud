module.exports = {
  extends: [
    'react-app',
    'react-app/jest',
    'eslint:recommended',
    'plugin:react/recommended',
    'prettier',
    'plugin:@typescript-eslint/recommended',
    'plugin:import/errors',
    'plugin:import/warnings',
  ],
  plugins: ['simple-import-sort'],
  ignorePatterns: ['/grpc/*.js'],
  overrides: [
    {
      files: ['**/*.ts?(x)'],
      rules: {
        '@typescript-eslint/explicit-module-boundary-types': 'off',
        'react/jsx-no-undef': 'off',
        'react/no-unescaped-entities': 'off',
        'react/react-in-jsx-scope': 'off',
        'simple-import-sort/imports': 'warn',
        'import/order': 'off',
        'import/no-unresolved': 'off',
        'import/named': 'off',
        'react/display-name': 'off',
        'no-restricted-imports': [
          'error',
          {
            paths: [
              {
                name: 'styled-components',
                message: 'Please import from styled-components/macro.',
              },
            ],
            patterns: ['!styled-components/macro'],
          },
        ],
      },
    },
  ],
};
