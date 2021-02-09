import { Timestamp } from 'google-protobuf/google/protobuf/timestamp_pb';
import styled from 'styled-components/macro';

import { ListApplicationsResponse } from '../../grpc/clud_pb';
import { shadowSm } from '../../styles/shadows';
import { spacing12 } from '../../styles/spacing';
import { ApplicationCard } from './ApplicationCard';
import { ApplicationsList } from './Applications';
import { HomeHeader } from './Home';

export default {
  title: 'Features/Home',
};

const application1 = new ListApplicationsResponse.Application();
application1.setName('awesome-app');
application1.setDescription('A very useful app for building things');
application1.setHasentrypoint(true);
const lastUpdatedDate = new Timestamp();
lastUpdatedDate.fromDate(new Date());
application1.setLastupdatedtime(lastUpdatedDate);
application1.setOwner('Charles');

const minimalApplication = new ListApplicationsResponse.Application();
minimalApplication.setName('another-cool-app');
minimalApplication.setHasentrypoint(false);
minimalApplication.setLastupdatedtime(lastUpdatedDate);

const noOwner = new ListApplicationsResponse.Application();
noOwner.setName('no-owner-app');
noOwner.setDescription('This app is responsible for absolutely nothing');
noOwner.setHasentrypoint(true);
noOwner.setLastupdatedtime(lastUpdatedDate);

const applicationsResponse = new ListApplicationsResponse();
applicationsResponse.setApplicationsList([application1, minimalApplication, noOwner]);

export const header = () => (
  <Container>
    <HomeHeader />
  </Container>
);

export const applicationCard = () => (
  <Container>
    <ApplicationCard application={application1} />
  </Container>
);

export const applications = () => (
  <Container>
    <ApplicationsList applications={applicationsResponse} />
  </Container>
);

const Container = styled.div`
  background-color: rgba(0, 20, 40, 0.1);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  padding: ${spacing12};
  box-shadow: ${shadowSm};
  width: 800px;
  max-width: 100%;
`;
