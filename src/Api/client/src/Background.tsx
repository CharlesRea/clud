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

const Clouds = styled.div`
  overflow: hidden;
  width: 1px;
  height: 1px;
  transform: translate(-100%, -100%);
  border-radius: 50%;
  filter: url(#filter);
  box-shadow: rgb(219, 241, 245) 32vw 94vh 30vmin 13vmin, rgb(144, 211, 244) 34vw 2vh 33vmin 13vmin,
    rgb(219, 241, 245) 58vw 67vh 37vmin 11vmin, rgb(144, 211, 244) 63vw 68vh 39vmin 18vmin,
    rgb(144, 211, 244) 67vw 77vh 38vmin 9vmin, rgb(219, 241, 245) 92vw 98vh 40vmin 20vmin,
    rgb(144, 211, 244) 99vw 33vh 28vmin 7vmin, rgb(240, 255, 243) 43vw 10vh 28vmin 5vmin,
    rgb(144, 211, 244) 83vw 98vh 29vmin 4vmin, rgb(144, 211, 244) 65vw 19vh 21vmin 13vmin,
    rgb(144, 211, 244) 42vw 85vh 22vmin 13vmin, rgb(144, 211, 244) 76vw 98vh 26vmin 4vmin,
    rgb(219, 241, 245) 87vw 34vh 23vmin 6vmin, rgb(144, 211, 244) 45vw 13vh 30vmin 12vmin,
    rgb(144, 211, 244) 42vw 85vh 31vmin 7vmin, rgb(219, 241, 245) 67vw 32vh 38vmin 8vmin,
    rgb(219, 241, 245) 62vw 7vh 39vmin 12vmin, rgb(240, 255, 243) 81vw 94vh 29vmin 18vmin,
    rgb(144, 211, 244) 35vw 96vh 40vmin 11vmin, rgb(144, 211, 244) 36vw 83vh 26vmin 5vmin,
    rgb(144, 211, 244) 50vw 62vh 28vmin 10vmin, rgb(240, 255, 243) 98vw 1vh 23vmin 8vmin,
    rgb(144, 211, 244) 87vw 9vh 32vmin 2vmin, rgb(144, 211, 244) 17vw 100vh 33vmin 4vmin,
    rgb(240, 255, 243) 12vw 59vh 24vmin 13vmin, rgb(240, 255, 243) 16vw 5vh 24vmin 14vmin,
    rgb(144, 211, 244) 34vw 37vh 26vmin 3vmin, rgb(144, 211, 244) 63vw 5vh 23vmin 2vmin,
    rgb(240, 255, 243) 49vw 58vh 39vmin 18vmin, rgb(144, 211, 244) 94vw 38vh 29vmin 20vmin,
    rgb(240, 255, 243) 3vw 40vh 28vmin 3vmin, rgb(219, 241, 245) 41vw 2vh 38vmin 17vmin,
    rgb(219, 241, 245) 48vw 53vh 28vmin 4vmin, rgb(144, 211, 244) 15vw 16vh 20vmin 16vmin,
    rgb(219, 241, 245) 12vw 3vh 26vmin 20vmin, rgb(144, 211, 244) 43vw 18vh 34vmin 4vmin,
    rgb(240, 255, 243) 50vw 64vh 34vmin 16vmin, rgb(144, 211, 244) 30vw 10vh 27vmin 17vmin,
    rgb(219, 241, 245) 12vw 69vh 33vmin 1vmin, rgb(219, 241, 245) 95vw 17vh 37vmin 3vmin,
    rgb(219, 241, 245) 70vw 66vh 38vmin 5vmin, rgb(144, 211, 244) 20vw 78vh 34vmin 19vmin,
    rgb(144, 211, 244) 27vw 36vh 20vmin 3vmin, rgb(240, 255, 243) 37vw 47vh 34vmin 20vmin,
    rgb(144, 211, 244) 96vw 37vh 32vmin 13vmin, rgb(219, 241, 245) 5vw 72vh 25vmin 19vmin,
    rgb(240, 255, 243) 49vw 30vh 33vmin 2vmin, rgb(219, 241, 245) 32vw 39vh 31vmin 1vmin,
    rgb(144, 211, 244) 4vw 53vh 40vmin 7vmin, rgb(219, 241, 245) 71vw 3vh 24vmin 15vmin,
    rgb(240, 255, 243) 58vw 70vh 40vmin 9vmin, rgb(144, 211, 244) 80vw 75vh 28vmin 1vmin,
    rgb(240, 255, 243) 25vw 13vh 40vmin 6vmin, rgb(144, 211, 244) 34vw 22vh 39vmin 17vmin,
    rgb(219, 241, 245) 58vw 90vh 25vmin 2vmin, rgb(144, 211, 244) 8vw 43vh 23vmin 1vmin,
    rgb(144, 211, 244) 25vw 48vh 22vmin 13vmin, rgb(240, 255, 243) 25vw 47vh 21vmin 13vmin,
    rgb(240, 255, 243) 89vw 60vh 40vmin 13vmin, rgb(144, 211, 244) 12vw 83vh 30vmin 15vmin,
    rgb(144, 211, 244) 88vw 94vh 25vmin 16vmin, rgb(144, 211, 244) 78vw 7vh 38vmin 11vmin,
    rgb(219, 241, 245) 39vw 8vh 37vmin 20vmin, rgb(144, 211, 244) 32vw 23vh 21vmin 5vmin,
    rgb(144, 211, 244) 19vw 99vh 27vmin 4vmin, rgb(144, 211, 244) 48vw 83vh 29vmin 10vmin,
    rgb(219, 241, 245) 10vw 94vh 25vmin 12vmin, rgb(144, 211, 244) 54vw 27vh 28vmin 19vmin,
    rgb(144, 211, 244) 25vw 97vh 20vmin 14vmin, rgb(144, 211, 244) 3vw 86vh 37vmin 8vmin,
    rgb(144, 211, 244) 71vw 33vh 28vmin 15vmin, rgb(219, 241, 245) 11vw 97vh 22vmin 14vmin,
    rgb(240, 255, 243) 70vw 99vh 34vmin 2vmin, rgb(144, 211, 244) 68vw 70vh 32vmin 20vmin,
    rgb(144, 211, 244) 11vw 61vh 40vmin 13vmin, rgb(240, 255, 243) 14vw 38vh 20vmin 10vmin,
    rgb(240, 255, 243) 96vw 53vh 40vmin 4vmin, rgb(219, 241, 245) 37vw 97vh 29vmin 14vmin,
    rgb(144, 211, 244) 80vw 47vh 25vmin 5vmin, rgb(144, 211, 244) 68vw 88vh 20vmin 8vmin,
    rgb(240, 255, 243) 28vw 8vh 34vmin 6vmin, rgb(144, 211, 244) 65vw 21vh 31vmin 12vmin,
    rgb(219, 241, 245) 50vw 9vh 24vmin 3vmin, rgb(144, 211, 244) 38vw 4vh 36vmin 13vmin,
    rgb(219, 241, 245) 68vw 7vh 29vmin 2vmin, rgb(219, 241, 245) 98vw 8vh 31vmin 19vmin,
    rgb(219, 241, 245) 3vw 65vh 33vmin 16vmin, rgb(144, 211, 244) 72vw 75vh 38vmin 13vmin,
    rgb(219, 241, 245) 1vw 33vh 32vmin 17vmin, rgb(144, 211, 244) 34vw 63vh 34vmin 8vmin,
    rgb(144, 211, 244) 31vw 93vh 26vmin 18vmin, rgb(240, 255, 243) 35vw 75vh 24vmin 7vmin,
    rgb(219, 241, 245) 9vw 84vh 37vmin 10vmin, rgb(144, 211, 244) 38vw 56vh 30vmin 6vmin,
    rgb(240, 255, 243) 55vw 19vh 22vmin 16vmin, rgb(219, 241, 245) 99vw 88vh 20vmin 4vmin,
    rgb(144, 211, 244) 86vw 85vh 29vmin 16vmin, rgb(240, 255, 243) 26vw 38vh 39vmin 20vmin,
    rgb(144, 211, 244) 4vw 44vh 21vmin 13vmin, rgb(144, 211, 244) 95vw 92vh 29vmin 11vmin;
`;

const CloudSvg = styled.svg`
  position: absolute;
`;
