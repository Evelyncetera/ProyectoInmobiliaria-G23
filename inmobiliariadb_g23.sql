/*----- BD ----*/
CREATE DATABASE IF NOT EXISTS `inmobiliariadb_g23` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_spanish_ci;
USE `inmobiliariadb_g23`;

/*---- Tablas ----*/
CREATE TABLE IF NOT EXISTS `propietario` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `dni` VARCHAR(20) NOT NULL UNIQUE,
  `nombre` VARCHAR(50) NOT NULL,
  `apellido` VARCHAR(50) NOT NULL,
  `telefono` VARCHAR(30) NOT NULL,
  `email` VARCHAR(100) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `inquilino` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `dni` VARCHAR(20) NOT NULL UNIQUE,
  `nombre` VARCHAR(50) NOT NULL,
  `apellido` VARCHAR(50) NOT NULL,
  `telefono` VARCHAR(30) NOT NULL,
  `email` VARCHAR(100) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `tipo_inmueble` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `nombre` VARCHAR(100) NOT NULL UNIQUE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `inmueble` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `id_propietario` INT NOT NULL,
    `id_tipo_inmueble` INT NOT NULL,
    `direccion` VARCHAR(200) NOT NULL,
    `cupo` INT NOT NULL,
    `latitud` DECIMAL(10,7) NOT NULL,
    `longitud` DECIMAL(10,7) NOT NULL,
    `precio_por_dia` DECIMAL(12,2) NOT NULL,
    `porcentaje_reserva` DECIMAL(5,2) NOT NULL,
    `disponible` BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT `fk_inmueble_propietario`
        FOREIGN KEY (`id_propietario`)
        REFERENCES `propietario` (`id`),

    CONSTRAINT `fk_inmueble_tipo`
        FOREIGN KEY (`id_tipo_inmueble`)
        REFERENCES `tipo_inmueble` (`id`),

    CONSTRAINT `chk_inmueble_cupo`
        CHECK (`cupo` > 0),

    CONSTRAINT `chk_inmueble_precio`
        CHECK (`precio_por_dia` > 0),

    CONSTRAINT `chk_inmueble_porcentaje`
        CHECK (`porcentaje_reserva` >= 0
              AND `porcentaje_reserva` <= 100)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `reserva` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `id_inquilino` INT NOT NULL,
    `id_inmueble` INT NOT NULL,
    `fecha_desde` DATE NOT NULL,
    `fecha_hasta` DATE NOT NULL,
    `monto_por_dia` DECIMAL(12,2) NOT NULL,
    `anulada` BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT `fk_reserva_inquilino`
        FOREIGN KEY (`id_inquilino`)
        REFERENCES `inquilino` (`id`),

    CONSTRAINT `fk_reserva_inmueble`
        FOREIGN KEY (`id_inmueble`)
        REFERENCES `inmueble` (`id`),

    CONSTRAINT `chk_reserva_fechas`
        CHECK (`fecha_hasta` >= `fecha_desde`),

    CONSTRAINT `chk_reserva_monto`
        CHECK (`monto_por_dia` > 0)
) ENGINE=InnoDB;

/* ---- Seeders ---- */ 

INSERT INTO `propietario` (`dni`, `nombre`, `apellido`, `telefono`, `email`) VALUES
('11111111', 'Juan Alberto', 'Perez', '2664000001', 'juan.perez@email.com'),
('22222222', 'Maria Angelica', 'Gomez', '2664000002', 'maria.gomez@email.com')
ON DUPLICATE KEY UPDATE `dni`=`dni`;

INSERT INTO `inquilino` (`dni`, `nombre`, `apellido`, `telefono`, `email`) VALUES
('33333333', 'Carlos Eduardo', 'Lopez', '2664000003', 'carlos.lopez@email.com'),
('44444444', 'Ana Beatriz', 'Martinez', '2664000004', 'ana.martinez@email.com')
ON DUPLICATE KEY UPDATE `dni`=`dni`;

INSERT INTO `tipo_inmueble` (`nombre`) VALUES
('Casa'),
('Departamento'),
('Monoambiente'),
('Loft')
ON DUPLICATE KEY UPDATE `nombre` = `nombre`;

          /* --- Query para evitar duplicados usando *NOT EXISTS* --- */
INSERT INTO `inmueble`(`id_propietario`, `id_tipo_inmueble`, `direccion`, `cupo`, `latitud`,
    `longitud`, `precio_por_dia`, `porcentaje_reserva`, `disponible`)
SELECT p.id, t.id, 'Av. España 1250', 6, -33.3017234, -66.3378901, 85000.00, 30.00, TRUE
FROM propietario p
INNER JOIN tipo_inmueble t ON t.nombre = 'Casa'
WHERE p.dni = '11111111'
  AND NOT EXISTS (
      SELECT 1
      FROM inmueble i
      WHERE i.id_propietario = p.id
        AND i.id_tipo_inmueble = t.id
        AND i.direccion = 'Av. España 1250'
  );

INSERT INTO `inmueble`  (`id_propietario`, `id_tipo_inmueble`, `direccion`, `cupo`, 
    `latitud`, `longitud`, `precio_por_dia`, `porcentaje_reserva`, `disponible`)
SELECT p.id, t.id, 'San Martín 450', 4, -33.2968123, -66.3345127, 65000.00, 25.00, TRUE
FROM propietario p
INNER JOIN tipo_inmueble t ON t.nombre = 'Departamento'
WHERE p.dni = '11111111'
  AND NOT EXISTS (
      SELECT 1
      FROM inmueble i
      WHERE i.id_propietario = p.id
        AND i.id_tipo_inmueble = t.id
        AND i.direccion = 'San Martín 450'
  );

INSERT INTO `inmueble` (`id_propietario`, `id_tipo_inmueble`, `direccion`, `cupo`,
    `latitud`, `longitud`, `precio_por_dia`, `porcentaje_reserva`, `disponible`)
SELECT p.id, t.id, 'Junín 780', 2, -33.2995412, -66.3361024, 45000.00, 20.00, TRUE
FROM propietario p
INNER JOIN tipo_inmueble t ON t.nombre = 'Monoambiente'
WHERE p.dni = '22222222'
  AND NOT EXISTS (
      SELECT 1
      FROM inmueble i
      WHERE i.id_propietario = p.id
        AND i.id_tipo_inmueble = t.id
        AND i.direccion = 'Junín 780'
  );


INSERT INTO `inmueble` (`id_propietario`, `id_tipo_inmueble`, `direccion`, `cupo`, 
    `latitud`, `longitud`, `precio_por_dia`,`porcentaje_reserva`, `disponible`)
SELECT p.id, t.id, 'Rivadavia 920', 3, -33.3008711, -66.3392456, 55000.00, 30.00, FALSE
FROM propietario p
INNER JOIN tipo_inmueble t ON t.nombre = 'Loft'
WHERE p.dni = '22222222'
  AND NOT EXISTS (
      SELECT 1
      FROM inmueble i
      WHERE i.id_propietario = p.id
        AND i.id_tipo_inmueble = t.id
        AND i.direccion = 'Rivadavia 920'
  );

/* ---- Seeders de Reservas (fechas relativas a la fecha actual) ---- */
/* Reserva 1: vigente (hoy está dentro del rango) */
INSERT INTO `reserva` (`id_inquilino`, `id_inmueble`, `fecha_desde`, `fecha_hasta`, `monto_por_dia`, `anulada`)
SELECT i.id, inm.id, DATE_SUB(CURDATE(), INTERVAL 5 DAY), DATE_ADD(CURDATE(), INTERVAL 10 DAY), 85000.00, FALSE
FROM inquilino i
INNER JOIN inmueble inm ON inm.direccion = 'Av. España 1250'
WHERE i.dni = '33333333'
  AND NOT EXISTS (
      SELECT 1 FROM reserva r
      WHERE r.id_inquilino = i.id
        AND r.id_inmueble = inm.id
        AND r.fecha_desde = DATE_SUB(CURDATE(), INTERVAL 5 DAY)
  );

/* Reserva 2: histórica/vencida */
INSERT INTO `reserva` (`id_inquilino`, `id_inmueble`, `fecha_desde`, `fecha_hasta`, `monto_por_dia`, `anulada`)
SELECT i.id, inm.id, DATE_SUB(CURDATE(), INTERVAL 60 DAY), DATE_SUB(CURDATE(), INTERVAL 30 DAY), 65000.00, FALSE
FROM inquilino i
INNER JOIN inmueble inm ON inm.direccion = 'San Martín 450'
WHERE i.dni = '44444444'
  AND NOT EXISTS (
      SELECT 1 FROM reserva r
      WHERE r.id_inquilino = i.id
        AND r.id_inmueble = inm.id
        AND r.fecha_desde = DATE_SUB(CURDATE(), INTERVAL 60 DAY)
  );

/* Reserva 3: futura */
INSERT INTO `reserva` (`id_inquilino`, `id_inmueble`, `fecha_desde`, `fecha_hasta`, `monto_por_dia`, `anulada`)
SELECT i.id, inm.id, DATE_ADD(CURDATE(), INTERVAL 15 DAY), DATE_ADD(CURDATE(), INTERVAL 45 DAY), 68000.00, FALSE
FROM inquilino i
INNER JOIN inmueble inm ON inm.direccion = 'San Martín 450'
WHERE i.dni = '33333333'
  AND NOT EXISTS (
      SELECT 1 FROM reserva r
      WHERE r.id_inquilino = i.id
        AND r.id_inmueble = inm.id
        AND r.fecha_desde = DATE_ADD(CURDATE(), INTERVAL 15 DAY)
  );

/* Reserva 4: vigente */
INSERT INTO `reserva` (`id_inquilino`, `id_inmueble`, `fecha_desde`, `fecha_hasta`, `monto_por_dia`, `anulada`)
SELECT i.id, inm.id, DATE_SUB(CURDATE(), INTERVAL 2 DAY), DATE_ADD(CURDATE(), INTERVAL 20 DAY), 45000.00, FALSE
FROM inquilino i
INNER JOIN inmueble inm ON inm.direccion = 'Junín 780'
WHERE i.dni = '44444444'
  AND NOT EXISTS (
      SELECT 1 FROM reserva r
      WHERE r.id_inquilino = i.id
        AND r.id_inmueble = inm.id
        AND r.fecha_desde = DATE_SUB(CURDATE(), INTERVAL 2 DAY)
  );

/* Reserva 5: anulada (baja lógica). El inmueble queda 'sin reservas' para los reportes */
INSERT INTO `reserva` (`id_inquilino`, `id_inmueble`, `fecha_desde`, `fecha_hasta`, `monto_por_dia`, `anulada`)
SELECT i.id, inm.id, DATE_SUB(CURDATE(), INTERVAL 10 DAY), DATE_ADD(CURDATE(), INTERVAL 5 DAY), 55000.00, TRUE
FROM inquilino i
INNER JOIN inmueble inm ON inm.direccion = 'Rivadavia 920'
WHERE i.dni = '33333333'
  AND NOT EXISTS (
      SELECT 1 FROM reserva r
      WHERE r.id_inquilino = i.id
        AND r.id_inmueble = inm.id
        AND r.fecha_desde = DATE_SUB(CURDATE(), INTERVAL 10 DAY)
        AND r.anulada = TRUE
  );
