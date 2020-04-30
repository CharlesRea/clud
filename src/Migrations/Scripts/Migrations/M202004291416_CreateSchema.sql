CREATE TABLE applications (
   application_id serial PRIMARY KEY,
   name text NOT NULL,
   description text NOT NULL,
   owner text NOT NULL,
   repository text NOT NULL,
   updated_date_time timestamptz NOT NULL
);

CREATE TABLE application_histories (
  application_history_id serial PRIMARY KEY,
  application_id INTEGER NOT NULL REFERENCES applications(application_id),
  message text NOT NULL,
  updated_date_time timestamptz NOT NULL
);

CREATE TABLE services (
  service_id serial PRIMARY KEY,
  application_id INTEGER NOT NULL REFERENCES applications(application_id),
  name text NOT NULL
);
