INSERT INTO applications (name, description, owner, repository, updated_date_time) VALUES
('clud', 'The solution to all your deployment concerns. Maybe', 'Charles & Alex', 'https://github.com/CharlesRea/clud', now());

INSERT INTO services (application_id, name) SELECT application_id, 'postgres' from applications WHERE name = 'clud';

INSERT INTO application_histories (application_id, message, updated_date_time) SELECT application_id, 'Application deployed', NOW() from applications WHERE name = 'clud';