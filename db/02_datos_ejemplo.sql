-- Datos de ejemplo para probar la aplicación.
-- Cuenta inicial: usuario admin / contraseña Admin2026 (cámbiela al entrar).

INSERT INTO idm_personas (per_cedula, per_nombre, per_apellido, per_direccion, per_telefono, per_email)
VALUES ('0102030400', 'Paola', 'Andrade Mora', 'Av. Huayna Cápac 1-45, Cuenca', '0987001122', 'pandrade@centromedico.ec');
INSERT INTO idm_empleados (idm_personas_per_id, emp_tipo)
VALUES ((SELECT per_id FROM idm_personas WHERE per_cedula = '0102030400'), 'Administracion');
INSERT INTO idm_usuarios (usu_usuario, usu_contrasena, usu_permiso, idm_empleados_emp_id)
VALUES ('admin', 'pbkdf2$210000$pUvMANbtkbgQ+5RKx+qyXA==$RvAbzP4nrk5J2gP4pZFaSt1pIbw754PUQTip26Oqlg8=',
        'Administrador', (SELECT MAX(emp_id) FROM idm_empleados));

INSERT INTO idm_personas (per_cedula, per_nombre, per_apellido, per_direccion, per_telefono, per_email)
VALUES ('1710034065', 'Andrés', 'Vintimilla Ríos', 'Calle Larga 7-20, Cuenca', '0991234567', 'avintimilla@centromedico.ec');
INSERT INTO idm_empleados (idm_personas_per_id, emp_tipo, emp_especialidad)
VALUES ((SELECT per_id FROM idm_personas WHERE per_cedula = '1710034065'), 'Medico', 'Medicina general');

INSERT INTO idm_personas (per_cedula, per_nombre, per_apellido, per_direccion, per_telefono, per_email)
VALUES ('0301122339', 'Ana Lucía', 'Medina Coronel', 'Av. Solano 3-12, Cuenca', '0984455667', 'amedina@centromedico.ec');
INSERT INTO idm_empleados (idm_personas_per_id, emp_tipo, emp_especialidad)
VALUES ((SELECT per_id FROM idm_personas WHERE per_cedula = '0301122339'), 'Medico', 'Pediatría');

INSERT INTO idm_personas (per_cedula, per_nombre, per_apellido, per_direccion, per_telefono, per_email)
VALUES ('1103456123', 'Rosa', 'Cabrera Loja', 'Av. de las Américas 5-60, Cuenca', '0978899001', 'rcabrera@centromedico.ec');
INSERT INTO idm_empleados (idm_personas_per_id, emp_tipo)
VALUES ((SELECT per_id FROM idm_personas WHERE per_cedula = '1103456123'), 'Atencion');

INSERT INTO idm_servicios (ser_nombre, ser_precio, ser_grava_iva) VALUES ('Consulta de medicina general', 25, 0);
INSERT INTO idm_servicios (ser_nombre, ser_precio, ser_grava_iva) VALUES ('Consulta pediátrica', 30, 0);
INSERT INTO idm_servicios (ser_nombre, ser_precio, ser_grava_iva) VALUES ('Control de presión y glucosa', 8, 0);
INSERT INTO idm_servicios (ser_nombre, ser_precio, ser_grava_iva) VALUES ('Curación con insumos', 12.50, 1);
INSERT INTO idm_servicios (ser_nombre, ser_precio, ser_grava_iva) VALUES ('Certificado médico', 10, 1);

INSERT INTO idm_personas (per_cedula, per_nombre, per_apellido, per_direccion, per_telefono, per_email)
VALUES ('0104567896', 'Lucía', 'Peralta Sigüenza', 'Av. 12 de Abril 2-30, Cuenca', '0987654321', 'lperalta@correo.ec');
INSERT INTO idm_pacientes (idm_personas_per_id, pac_fecha_nacimiento)
VALUES ((SELECT per_id FROM idm_personas WHERE per_cedula = '0104567896'), DATE '1990-08-20');

INSERT INTO idm_personas (per_cedula, per_nombre, per_apellido, per_direccion, per_telefono, per_email)
VALUES ('0923456784', 'Diego', 'Ochoa Pesántez', 'Remigio Crespo 4-18, Cuenca', '072845123', NULL);
INSERT INTO idm_pacientes (idm_personas_per_id, pac_fecha_nacimiento)
VALUES ((SELECT per_id FROM idm_personas WHERE per_cedula = '0923456784'), DATE '2015-03-02');

COMMIT;
