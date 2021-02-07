import styled from 'styled-components/macro';

import { ListApplicationsResponse } from '../../grpc/clud_pb';
import { borderRadius2 } from '../../styles/borders';
import { neutral700 } from '../../styles/colours';
import { font2xl, semibold } from '../../styles/fonts';
import { shadowLg, shadowMd, shadowSm } from '../../styles/shadows';
import { spacing2, spacing8 } from '../../styles/spacing';

type ApplicationCardProps = {
  application: ListApplicationsResponse.Application;
  className?: string;
};

const baseHostname = 'clud.ghyston.com';

export const ApplicationCard = ({ application, className }: ApplicationCardProps) => (
  <Card className={className}>
    <Url>
      <ApplicationName>{application.getName()}</ApplicationName>
      {application.getHasentrypoint() && <BaseHostname>.{baseHostname}</BaseHostname>}
    </Url>
    {application.getDescription() && <Description>{application.getDescription()}</Description>}
  </Card>
);

const Card = styled.div`
  background-color: rgba(255, 255, 255, 0.3);
  backdrop-filter: blur(10px);
  padding: ${spacing8};
  border-radius: ${borderRadius2};
  border: 1px solid rgba(255, 255, 255, 0.3);
  box-shadow: ${shadowSm};
  width: 100%;

  &:hover {
    background-color: rgba(255, 255, 255, 0.5);
  }
`;

const Url = styled.h3`
  font-size: ${font2xl};
  line-height: 1;
`;

const ApplicationName = styled.span`
  font-weight: ${semibold};
`;

const BaseHostname = styled.span`
  color: ${neutral700};
`;

const Description = styled.div`
  padding-top: ${spacing2};
`;
