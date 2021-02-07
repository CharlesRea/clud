import { createGlobalStyle } from 'styled-components/macro';
import normalize from 'styled-normalize';

import { neutral900, teal700 } from './colours';
import { fontBase, fontFamily, monospacedFontFamily, normalFontWeight } from './fonts';

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
    color: ${neutral900};
    font-weight: ${normalFontWeight};
    line-height: 1.5;
  }
  
  code, pre {
    font-family: ${monospacedFontFamily};
  }
  
  // Remove default margins from user-agent stylesheet - you should be explicitly using the
  // spacing scale values where margins are required
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
  
  // Remove default heading styles - you should be using the font size scale values, and making an
  // explicit decision on heading sizes based on the context they appear in, not on the header's
  // semantic level
  h1,
  h2,
  h3,
  h4,
  h5,
  h6 {
    font-size: inherit;
    font-weight: inherit;
  }
  
  ol,
  ul {
    list-style: none;
    margin: 0;
    padding: 0;
  }
  
  a {
    text-decoration: none;
    color: ${neutral900};
    
    &:hover {
      color: ${teal700};
    }
  }
`;
