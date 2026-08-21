CREATE DATABASE IF NOT EXISTS `inmobiliariadb_g23` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_spanish_ci;
USE `inmobiliariadb_g23`;


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

INSERT INTO `propietario` (`dni`, `nombre`, `apellido`, `telefono`, `email`) VALUES
('11111111', 'Juan Alberto', 'Perez', '2664000001', 'juan.perez@email.com'),
('22222222', 'Maria Angelica', 'Gomez', '2664000002', 'maria.gomez@email.com')
ON DUPLICATE KEY UPDATE `dni`=`dni`;

INSERT INTO `inquilino` (`dni`, `nombre`, `apellido`, `telefono`, `email`) VALUES
('33333333', 'Carlos Eduardo', 'Lopez', '2664000003', 'carlos.lopez@email.com'),
('44444444', 'Ana Beatriz', 'Martinez', '2664000004', 'ana.martinez@email.com')
ON DUPLICATE KEY UPDATE `dni`=`dni`;
