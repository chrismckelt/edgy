create table if not exists table_001
(
   "Timestamp" timestamp with time zone not null,
   isalive smallint not null,
   Temperature decimal not null,
   tagkey varchar not null
);
 
alter table table_001 owner to postgres;
 
SELECT create_hypertable ('table_001', 'Timestamp');


-- Grafana access
CREATE USER grafana WITH PASSWORD 'jw8s0F4' CREATEDB;

CREATE DATABASE grafana OWNER grafana;