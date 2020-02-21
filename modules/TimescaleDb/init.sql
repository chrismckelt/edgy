create table if not exists table_001
(
   "Timestamp" timestamp with time zone not null,
   value_varchar varchar not null,
   value_numeric numeric not null,
   confidence integer not null,
   processedtimestamp timestamp with time zone not null,
   tagkey integer not null
);
 
alter table table_001 owner to postgres;
 
SELECT create_hypertable ('table_001', 'Timestamp');


-- Grafana access
CREATE USER grafana WITH PASSWORD 'jw8s0F4' CREATEDB;

CREATE DATABASE grafana OWNER grafana;