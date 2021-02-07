import React from 'react';
import styled from 'styled-components/macro';

export const Background = () => (
  <>
    <Clouds />
    <CloudSvg width="0">
      <filter id="filter">
        <feTurbulence type="fractalNoise" baseFrequency=".01" numOctaves="10" />
        <feDisplacementMap in="SourceGraphic" scale="240" />
      </filter>
    </CloudSvg>
  </>
);

// Light
// const colour1 = 'hsl(210,77%,61%)';
// const colour2 = 'hsl(205,82%,90%)';
// const colour3 = 'hsl(200,82%,66%)';
// const colour4 = 'hsl(200,82%,70%)';
//

// Dark
// const colour1 = 'hsl(210,77%,41%)';
// const colour2 = 'hsl(205,82%,30%)';
// const colour3 = 'hsl(200,82%,36%)';
// const colour4 = 'hsl(200,82%,20%)';

const colour1 = 'hsla(210,77%,21%, 0.1)';
const colour2 = 'hsl(205,82%,30%)';
const colour3 = 'hsl(200,82%,36%)';
const colour4 = 'hsl(200,82%,30%)';

const Clouds = styled.div`
  overflow: hidden;
  width: 1px;
  height: 1px;
  transform: translate(-100%, -100%);
  border-radius: 50%;
  filter: url(#filter);
  box-shadow: ${colour1} 51vw 30vh 34vmin 10vmin, ${colour2} 12vw 49vh 21vmin 5vmin,
    ${colour1} 23vw 44vh 40vmin 7vmin, ${colour3} 68vw 2vh 20vmin 6vmin,
    ${colour4} 6vw 56vh 32vmin 14vmin, ${colour3} 20vw 16vh 39vmin 10vmin,
    ${colour2} 41vw 48vh 37vmin 2vmin, ${colour1} 1vw 45vh 35vmin 4vmin,
    ${colour4} 97vw 88vh 23vmin 18vmin, ${colour4} 63vw 83vh 39vmin 3vmin,
    ${colour3} 93vw 28vh 27vmin 7vmin, ${colour2} 7vw 50vh 20vmin 6vmin,
    ${colour3} 84vw 56vh 38vmin 14vmin, ${colour4} 71vw 77vh 27vmin 3vmin,
    ${colour2} 8vw 32vh 35vmin 18vmin, ${colour1} 54vw 50vh 21vmin 15vmin,
    ${colour1} 33vw 49vh 25vmin 16vmin, ${colour4} 55vw 32vh 24vmin 1vmin,
    ${colour3} 53vw 57vh 28vmin 14vmin, ${colour4} 42vw 32vh 36vmin 12vmin,
    ${colour3} 80vw 21vh 28vmin 4vmin, ${colour3} 73vw 45vh 23vmin 20vmin,
    ${colour2} 15vw 84vh 37vmin 3vmin, ${colour4} 43vw 65vh 40vmin 4vmin,
    ${colour3} 62vw 11vh 32vmin 10vmin, ${colour1} 53vw 42vh 32vmin 9vmin,
    ${colour4} 61vw 50vh 22vmin 20vmin, ${colour3} 58vw 86vh 31vmin 20vmin,
    ${colour4} 36vw 78vh 36vmin 13vmin, ${colour2} 42vw 56vh 28vmin 4vmin,
    ${colour4} 4vw 24vh 27vmin 15vmin, ${colour3} 99vw 13vh 37vmin 2vmin,
    ${colour3} 51vw 59vh 34vmin 8vmin, ${colour3} 22vw 90vh 38vmin 13vmin,
    ${colour3} 46vw 59vh 40vmin 7vmin, ${colour4} 49vw 56vh 20vmin 1vmin,
    ${colour2} 27vw 10vh 20vmin 11vmin, ${colour4} 88vw 95vh 40vmin 19vmin,
    ${colour3} 9vw 21vh 22vmin 15vmin, ${colour4} 30vw 32vh 21vmin 16vmin,
    ${colour3} 29vw 59vh 20vmin 18vmin, ${colour3} 98vw 97vh 37vmin 20vmin,
    ${colour4} 21vw 93vh 21vmin 13vmin, ${colour2} 47vw 72vh 22vmin 10vmin,
    ${colour3} 93vw 48vh 36vmin 2vmin, ${colour3} 6vw 91vh 32vmin 12vmin,
    ${colour3} 46vw 47vh 40vmin 18vmin, ${colour1} 56vw 99vh 33vmin 11vmin,
    ${colour1} 86vw 16vh 33vmin 10vmin, ${colour2} 63vw 27vh 29vmin 5vmin,
    ${colour3} 37vw 22vh 27vmin 11vmin, ${colour3} 13vw 45vh 37vmin 11vmin,
    ${colour4} 98vw 65vh 28vmin 2vmin, ${colour1} 83vw 15vh 24vmin 18vmin,
    ${colour3} 50vw 40vh 25vmin 3vmin, ${colour3} 26vw 93vh 30vmin 11vmin,
    ${colour2} 33vw 73vh 23vmin 17vmin, ${colour2} 40vw 92vh 32vmin 9vmin,
    ${colour2} 91vw 60vh 33vmin 15vmin, ${colour1} 16vw 38vh 31vmin 3vmin,
    ${colour2} 36vw 65vh 25vmin 1vmin, ${colour4} 33vw 34vh 24vmin 5vmin,
    ${colour3} 18vw 15vh 21vmin 15vmin, ${colour1} 38vw 40vh 36vmin 1vmin,
    ${colour3} 69vw 92vh 39vmin 5vmin, ${colour4} 59vw 56vh 21vmin 18vmin;
`;

const CloudSvg = styled.svg`
  position: absolute;
`;
