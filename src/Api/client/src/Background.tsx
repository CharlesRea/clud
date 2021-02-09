import React from 'react';
import styled from 'styled-components/macro';

export const Background = () => (
  <Container>
    <Clouds />
    <CloudSvg width="0">
      <filter id="filter">
        <feTurbulence type="fractalNoise" baseFrequency=".01" numOctaves="10" />
        <feDisplacementMap in="SourceGraphic" scale="240" />
      </filter>
    </CloudSvg>
  </Container>
);

const Container = styled.div`
  position: fixed;
`;

const colour1 = 'hsla(210,77%,21%, 0.1)';
const colour2 = 'hsl(205,82%,30%)';
const colour3 = 'hsl(200,82%,36%)';
const colour4 = 'hsl(200,82%,30%)';

const Clouds = styled.div`
  position: absolute;
  overflow: hidden;
  width: 1px;
  height: 1px;
  transform: translate(-100%, -100%);
  border-radius: 50%;
  filter: url(#filter);
  box-shadow: ${colour1} 51vw 30vh 34vmin 10vmin, ${colour2} 12vw 49vh 21vmin 5vmin,
    ${colour1} 23vw 44vh 40vmin 7vmin, ${colour3} 68vw 2vh 20vmin 6vmin,
    ${colour4} 6vw 56vh 6vmin 6vmin, ${colour3} 20vw 16vh 39vmin 10vmin,
    ${colour2} 41vw 48vh 37vmin 2vmin, ${colour1} 1vw 45vh 35vmin 4vmin,
    ${colour1} 85vw 100vh 26vmin 8vmin, ${colour2} 31vw 66vh 21vmin 15vmin,
    ${colour1} 52vw 20vh 26vmin 4vmin, ${colour2} 18vw 3vh 40vmin 15vmin,
    ${colour4} 79vw 59vh 27vmin 18vmin, ${colour2} 47vw 78vh 40vmin 8vmin,
    ${colour1} 46vw 98vh 30vmin 3vmin, ${colour2} 68vw 61vh 33vmin 18vmin,
    ${colour3} 28vw 11vh 36vmin 17vmin, ${colour3} 3vw 83vh 29vmin 9vmin,
    ${colour2} 16vw 30vh 25vmin 10vmin, ${colour4} 73vw 2vh 28vmin 2vmin,
    ${colour4} 32vw 46vh 37vmin 18vmin, ${colour1} 80vw 3vh 24vmin 14vmin,
    ${colour4} 41vw 7vh 28vmin 18vmin, ${colour3} 10vw 42vh 26vmin 6vmin,
    ${colour2} 72vw 34vh 25vmin 18vmin, ${colour1} 45vw 62vh 24vmin 7vmin,
    ${colour1} 65vw 96vh 22vmin 9vmin, ${colour3} 52vw 29vh 31vmin 9vmin,
    ${colour3} 41vw 28vh 21vmin 13vmin, ${colour1} 73vw 63vh 38vmin 1vmin,
    ${colour4} 62vw 15vh 33vmin 12vmin, ${colour2} 29vw 86vh 30vmin 7vmin,
    ${colour2} 86vw 35vh 28vmin 20vmin, ${colour2} 46vw 59vh 36vmin 20vmin,
    ${colour1} 91vw 13vh 36vmin 16vmin, ${colour4} 87vw 43vh 21vmin 20vmin,
    ${colour2} 98vw 19vh 28vmin 7vmin, ${colour2} 85vw 91vh 20vmin 8vmin,
    ${colour4} 42vw 92vh 27vmin 17vmin, ${colour2} 69vw 15vh 36vmin 13vmin,
    ${colour3} 44vw 86vh 28vmin 1vmin, ${colour1} 61vw 7vh 23vmin 10vmin,
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
    ${colour3} 51vw 59vh 34vmin 8vmin, ${colour3} 22vw 90vh 38vmin 13vmin,
    ${colour3} 46vw 59vh 40vmin 7vmin, ${colour4} 49vw 56vh 20vmin 1vmin,
    ${colour2} 27vw 10vh 20vmin 11vmin, ${colour4} 88vw 95vh 40vmin 19vmin,
    ${colour3} 9vw 21vh 22vmin 15vmin, ${colour4} 30vw 32vh 21vmin 16vmin,
    ${colour3} 29vw 59vh 20vmin 18vmin, ${colour4} 21vw 93vh 21vmin 13vmin,
    ${colour2} 47vw 72vh 22vmin 10vmin, ${colour3} 93vw 48vh 36vmin 2vmin,
    ${colour3} 6vw 91vh 32vmin 12vmin, ${colour3} 46vw 47vh 40vmin 18vmin,
    ${colour1} 56vw 99vh 33vmin 11vmin, ${colour1} 86vw 16vh 33vmin 10vmin,
    ${colour2} 63vw 27vh 29vmin 5vmin, ${colour3} 37vw 22vh 27vmin 11vmin,
    ${colour3} 13vw 45vh 37vmin 11vmin, ${colour4} 98vw 65vh 28vmin 2vmin,
    ${colour1} 83vw 15vh 24vmin 18vmin, ${colour3} 50vw 40vh 25vmin 3vmin,
    ${colour3} 26vw 93vh 30vmin 11vmin, ${colour2} 33vw 73vh 23vmin 17vmin,
    ${colour2} 40vw 92vh 32vmin 9vmin, ${colour1} 16vw 38vh 31vmin 3vmin,
    ${colour2} 36vw 65vh 25vmin 1vmin, ${colour4} 33vw 34vh 24vmin 5vmin,
    ${colour3} 18vw 15vh 21vmin 15vmin, ${colour1} 38vw 40vh 36vmin 1vmin,
    ${colour3} 50vw 15vh 53vmin 15vmin, ${colour1} 38vw 40vh 36vmin 1vmin,
    ${colour3} 69vw 92vh 39vmin 5vmin, ${colour4} 59vw 56vh 21vmin 18vmin;
`;

const CloudSvg = styled.svg`
  position: absolute;
`;
