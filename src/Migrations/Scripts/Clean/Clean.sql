        DO $$
        DECLARE
            r RECORD;
        BEGIN
            -- triggers
                    FOR r IN (SELECT pns.nspname, pc.relname, pt.tgname
                  FROM pg_trigger pt, pg_class pc, pg_namespace pns
                  WHERE pns.oid=pc.relnamespace AND pc.oid=pt.tgrelid
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                    AND pt.tgisinternal=false
        ) LOOP
            EXECUTE format('DROP TRIGGER %I ON %I.%I;',
                               r.tgname, r.nspname, r.relname);
        END LOOP;
        -- constraints #1: foreign key
                FOR r IN (SELECT pns.nspname, pc.relname, pcon.conname
                  FROM pg_constraint pcon, pg_class pc, pg_namespace pns
                  WHERE pns.oid=pc.relnamespace AND pc.oid=pcon.conrelid
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                    AND pcon.contype='f'
        ) LOOP
        EXECUTE format('ALTER TABLE ONLY %I.%I DROP CONSTRAINT %I;',
                               r.nspname, r.relname, r.conname);
        END LOOP;
        -- constraints #2: the rest
                FOR r IN (SELECT pns.nspname, pc.relname, pcon.conname
                  FROM pg_constraint pcon, pg_class pc, pg_namespace pns
                  WHERE pns.oid=pc.relnamespace AND pc.oid=pcon.conrelid
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                    AND pcon.contype<>'f'
        ) LOOP
        EXECUTE format('ALTER TABLE ONLY %I.%I DROP CONSTRAINT %I;',
                               r.nspname, r.relname, r.conname);
        END LOOP;
        -- indicēs
                FOR r IN (SELECT pns.nspname, pc.relname
                  FROM pg_class pc, pg_namespace pns
                  WHERE pns.oid=pc.relnamespace
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                    AND pc.relkind='i'
        ) LOOP
        EXECUTE format('DROP INDEX %I.%I;',
                               r.nspname, r.relname);
        END LOOP;
        -- normal and materialised views
                FOR r IN (SELECT pns.nspname, pc.relname
                  FROM pg_class pc, pg_namespace pns
                  WHERE pns.oid=pc.relnamespace
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                    AND pc.relkind IN ('v', 'm')
        ) LOOP
        EXECUTE format('DROP VIEW %I.%I;',
                               r.nspname, r.relname);
        END LOOP;
        -- tables
                FOR r IN (SELECT pns.nspname, pc.relname
                  FROM pg_class pc, pg_namespace pns
                  WHERE pns.oid=pc.relnamespace
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                    AND pc.relkind='r'
        ) LOOP
        EXECUTE format('DROP TABLE %I.%I;',
                               r.nspname, r.relname);
        END LOOP;
        -- sequences
                FOR r IN (SELECT pns.nspname, pc.relname
                  FROM pg_class pc, pg_namespace pns
                  WHERE pns.oid=pc.relnamespace
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                    AND pc.relkind='S'
        ) LOOP
        EXECUTE format('DROP SEQUENCE %I.%I;',
                               r.nspname, r.relname);
        END LOOP;
        -- extensions (see below), only if necessary
                FOR r IN (SELECT pns.nspname, pe.extname
                  FROM pg_extension pe, pg_namespace pns
                  WHERE pns.oid=pe.extnamespace
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
        ) LOOP
        EXECUTE format('DROP EXTENSION %I;', r.extname);
        END LOOP;
        -- functions / procedures
                FOR r IN (SELECT pns.nspname, pp.proname, pp.oid
                  FROM pg_proc pp, pg_namespace pns
                  WHERE pns.oid=pp.pronamespace
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
        ) LOOP
        EXECUTE format('DROP FUNCTION %I.%I(%s);',
                               r.nspname, r.proname,
                               pg_get_function_identity_arguments(r.oid));
        END LOOP;
        -- nōn-default schemata we own; assume to be run by a not-superuser
                FOR r IN (SELECT pns.nspname
                  FROM pg_namespace pns, pg_roles pr
                  WHERE pr.oid=pns.nspowner
                    AND pns.nspname NOT IN ('information_schema', 'pg_catalog', 'pg_toast', 'public')
                    AND pr.rolname=current_user
        ) LOOP
        EXECUTE format('DROP SCHEMA %I;', r.nspname);
        END LOOP;
    END; $$;