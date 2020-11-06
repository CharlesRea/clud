CREATE TABLE applications (
   application_id serial PRIMARY KEY,
   name text NOT NULL,
   description text NOT NULL,
   owner text NOT NULL,
   repository text NOT NULL,
   has_entry_point boolean NOT NULL,
   updated_date_time timestamptz NOT NULL
);

CREATE TABLE deployments (
  deployment_id serial PRIMARY KEY,
  application_id INTEGER NOT NULL REFERENCES applications(application_id),
  version INTEGER NOT NULL,
  commit_hash TEXT NULL,
  application_config TEXT NOT NULL,
  deployment_date_time timestamptz NOT NULL
);
