CREATE DATABASE IF NOT EXISTS mockula;
USE mockula;

CREATE TABLE IF NOT EXISTS MockRoutes (
    Id int NOT NULL AUTO_INCREMENT,
    RouteId varchar(255) NOT NULL,
    Method varchar(255),
    Path varchar(255),
    HttpStatusCode int NOT NULL,
    Mock JSON,
    Enabled boolean NOT NULL DEFAULT true,
    PRIMARY KEY (Id)
);

CREATE TABLE IF NOT EXISTS Widgets (
    Id int NOT NULL AUTO_INCREMENT,
    Name varchar(255) NOT NULL,
    Description text,
    PRIMARY KEY (Id)
);
