import { createGlobalStyle } from 'styled-components';

export const HtmlBodyAndContentFullHeight = createGlobalStyle`
	html {
    height: 100%;
  }

  body {
    height: 100%;
    margin: 0;
  }

  #content {
    height: 100%;
  }
`;
