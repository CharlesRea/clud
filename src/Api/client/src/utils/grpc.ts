import { ApplicationsClient } from '../grpc/CludServiceClientPb';

// TODO parameterise this
const hostname = 'https://localhost:5001';

export const applicationsClient = new ApplicationsClient(hostname);
