import { createGlobalStyle } from 'styled-components/macro';
import normalize from 'styled-normalize';

import {
  neutral050,
  neutral100,
  neutral900,
  orange100,
  orange200,
  orange300,
  orange400,
  orange500,
  orange700,
} from './colours';
import {
  fontBase,
  fontFamily,
  lineHeightRegular,
  lineHeightTight,
  monospacedFontFamily,
  regularWeight,
} from './fonts';

export const GlobalStyles = createGlobalStyle`
  ${normalize}
  
  *, *::before, *::after {
    box-sizing: border-box;
  }

  html,
  body {
    margin: 0;
    font-family: ${fontFamily};
    font-size: ${fontBase};
    color: ${neutral050};
    font-weight: ${regularWeight};
    line-height: ${lineHeightRegular};
  }
  
  code, pre {
    font-family: ${monospacedFontFamily};
  }
  
  blockquote,
  dl,
  dd,
  h1,
  h2,
  h3,
  h4,
  h5,
  h6,
  hr,
  figure,
  p,
  pre {
    margin: 0;
  }
  
  h1,
  h2,
  h3,
  h4,
  h5,
  h6 {
    font-size: inherit;
    font-weight: inherit;
    
    line-height: ${lineHeightTight};
  }
  
  ol,
  ul {
    list-style: none;
    margin: 0;
    padding: 0;
  }
  
  a {
    text-decoration: none;
  }
`;
