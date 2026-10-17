-- La imagen gvenzl/oracle-free corre este script como SYS al crear la base.
ALTER SESSION SET CONTAINER = FREEPDB1;
ALTER SESSION SET CURRENT_SCHEMA = CENTROMEDICO;
@/opt/centromedico/01_esquema.sql
@/opt/centromedico/02_datos_ejemplo.sql
