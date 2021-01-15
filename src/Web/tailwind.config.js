const colors = require('tailwindcss/colors')

module.exports = {
    theme: {
        extend: {},
        colors: {
          transparent: 'transparent',
          current: 'currentColor',
          black: colors.black,
          white: colors.white,
          gray: colors.trueGray,
          red: colors.red,
          yellow: colors.amber,
          green: colors.emerald,
          teal: colors.teal,
          blue: colors.lightBlue,
          indigo: colors.indigo,
          purple: colors.violet,
          pink: colors.pink,
          orange: colors.orange,

          primary: colors.teal,
          secondary: colors.purple,
        }
    },
    variants: {
        rounded: ['first', 'last'],
        borderRadius: ['first', 'last'],
        borderWidth: ['focus'],
        padding: ['focus'],
        textColor: ['responsive', 'hover', 'focus', 'group-hover'],
    },
    purge: [
        './Features/**/*.razor',
        './Shared/**/*.razor',
        './wwwroot/**/*.html',
    ],
    plugins: [],
}
