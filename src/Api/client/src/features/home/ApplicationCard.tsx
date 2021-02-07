import styled from 'styled-components/macro';

import { ListApplicationsResponse } from '../../grpc/clud_pb';
import { borderRadius2 } from '../../styles/borders';
import {
  neutral100,
  neutral200,
  neutral300,
  neutral700,
  neutral800,
  neutral900,
} from '../../styles/colours';
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
  background-color: rgba(255, 255, 255, 0.8);
  //backdrop-filter: blur(10px);
  padding: ${spacing8};
  box-shadow: ${shadowLg};
  border: 1px solid rgba(255, 255, 255, 0.1);
  width: 100%;
  cursor: pointer;
  color: ${neutral900};
  //border-radius: 5px;

  &:hover {
    background-color: rgba(255, 255, 255, 0.2);
    color: ${neutral100};
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
  color: ${neutral800};
`;

const Description = styled.div`
  padding-top: ${spacing2};
`;
