import styled from 'styled-components/macro';

import { orange300 } from '../../styles/colours';
import { ReactComponent as Bitbucket } from './bitbucket.svg';
import { ReactComponent as Git } from './git.svg';
import { ReactComponent as Github } from './github.svg';
import { ReactComponent as Gitlab } from './gitlab.svg';

export const GitRepoLink = ({ repository }: { repository: string }) => {
  const fullyQualifiedUrl = repository.startsWith('http') ? repository : `https://${repository}`;
  return (
    <Link href={fullyQualifiedUrl}>
      <Icon repository={repository} />
    </Link>
  );
};

const Icon = ({ repository }: { repository: string }) => {
  if (repository.includes('github.com')) {
    return <Github />;
  } else if (repository.includes('bitbucket.com')) {
    return <Bitbucket />;
  } else if (repository.includes('gitlab.com')) {
    return <Gitlab />;
  } else {
    return <Git />;
  }
};

const Link = styled.a`
  width: 20px;
  height: 20px;
  fill: currentColor;
  display: block;

  &:hover {
    color: ${orange300};
  }
`;
