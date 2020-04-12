CREATE TABLE IF NOT EXISTS table_001
(
   "Timestamp" timestamp with time zone not null,
   IsAirConditionerOn smallint not null,
   Temperature decimal not null,
   tagkey varchar not null
);
 
ALTER TABLE table_001 OWNER TO postgres;

SELECT create_hypertable ('table_001', 'Timestamp');
-- Grafana access
CREATE USER grafana WITH PASSWORD 'LgrQE5gXzm2L' CREATEDB;

CREATE DATABASE grafana OWNER grafana;