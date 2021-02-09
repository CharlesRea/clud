import { formatDistanceToNow } from 'date-fns';
import { Link } from 'react-router-dom';
import styled from 'styled-components/macro';

import { baseHostname } from '../../config';
import { ListApplicationsResponse } from '../../grpc/clud_pb';
import { borderRadius2 } from '../../styles/borders';
import {
  neutral100,
  neutral200,
  neutral300,
  neutral700,
  neutral800,
  neutral900,
  orange200,
  orange300,
} from '../../styles/colours';
import { font2xl, semibold } from '../../styles/fonts';
import { shadowInner, shadowLg, shadowMd, shadowSm } from '../../styles/shadows';
import { spacing2, spacing4, spacing6, spacing8 } from '../../styles/spacing';
import { assertNotNull } from '../../utils/assertNotNull';

type ApplicationCardProps = {
  application: ListApplicationsResponse.Application;
  className?: string;
};

export const ApplicationCard = ({ application, className }: ApplicationCardProps) => (
  <Card className={className} to={`/apps/${application.getName()}`}>
    <Header>
      <Url>
        <ApplicationName>{application.getName()}</ApplicationName>
        {application.getHasentrypoint() && <BaseHostname>.{baseHostname}</BaseHostname>}
      </Url>
      {application.getDescription() && <Description>{application.getDescription()}</Description>}
    </Header>
    <Metadata>
      {application.getLastupdatedtime() && (
        <DeployDate>
          Updated {formatDistanceToNow(assertNotNull(application.getLastupdatedtime()).toDate())}{' '}
          ago
        </DeployDate>
      )}
      {application.getOwner() && <Owner>{application.getOwner()}</Owner>}
    </Metadata>
  </Card>
);

const ApplicationName = styled.span`
  font-weight: ${semibold};
  color: ${orange300};
`;

const Card = styled(Link)`
  background-color: rgba(255, 255, 255, 0.05);
  padding: ${spacing6};
  border: 1px solid rgba(255, 255, 255, 0.2);
  width: 100%;
  cursor: pointer;
  display: flex;
  flex-direction: row;
  justify-content: space-between;

  &:hover {
    background-color: rgba(255, 255, 255, 0.1);

    ${ApplicationName} {
      color: ${orange200};
    }
  }
`;

const Header = styled.div``;

const Url = styled.h3`
  font-size: ${font2xl};
  line-height: 1;
`;

const BaseHostname = styled.span`
  color: ${neutral200};
`;

const Description = styled.div`
  padding-top: ${spacing4};
`;

const Metadata = styled.div`
  color: ${neutral200};
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  justify-content: space-between;
`;

const DeployDate = styled.div``;

const Owner = styled.div``;
