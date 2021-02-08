import { useLocation, useParams } from 'react-router-dom';

import { PageContent } from '../../shared/PageContent';
import { assertNotNull } from '../../utils/assertNotNull';

export const ApplicationDashboard = () => {
  const { applicationName } = useParams();
  assertNotNull(applicationName);

  return <PageContent>{applicationName}</PageContent>;
};
