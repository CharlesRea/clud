import styled from 'styled-components/macro';

import { orange200, orange300 } from '../styles/colours';
import { letterSpacingWide, semibold } from '../styles/fonts';

type ButtonProps = {
  children: React.ReactNode;
  onClick: () => void;
  className?: string;
};

export const Button = (props: ButtonProps) => (
  <StyledButton className={props.className} onClick={props.onClick}>
    {props.children}
  </StyledButton>
);

const StyledButton = styled.button`
  text-transform: uppercase;
  letter-spacing: ${letterSpacingWide};
  font-weight: ${semibold};
  color: ${orange300};
  background: none;
  border: none;
  outline: none;
  cursor: pointer;

  &:hover {
    color: ${orange200};
  }
`;
