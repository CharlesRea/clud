CREATE TABLE applications (
   application_id serial PRIMARY KEY,
   name VARCHAR(50) NOT NULL,
   namespace VARCHAR(50) NOT NULL
);

CREATE TABLE services (
  service_id serial PRIMARY KEY,
  application_id INTEGER NOT NULL REFERENCES applications(application_id),
  name VARCHAR(50) NOT NULL
);