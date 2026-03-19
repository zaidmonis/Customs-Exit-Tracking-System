ALTER SESSION SET CONTAINER = CUSTOMSDB;
ALTER SESSION SET CURRENT_SCHEMA = CUSTOMS_APP;

CREATE TABLE person_identity (
    person_id            RAW(16) PRIMARY KEY,
    national_id          VARCHAR2(50 CHAR) NOT NULL UNIQUE,
    full_name            VARCHAR2(200 CHAR) NOT NULL,
    date_of_birth        DATE NOT NULL,
    nationality_code     CHAR(3 CHAR) NOT NULL,
    gender               VARCHAR2(20 CHAR),
    created_at           TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at           TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE TABLE exit_record (
    exit_id              RAW(16) PRIMARY KEY,
    person_id            RAW(16) NOT NULL,
    departed_at          TIMESTAMP NOT NULL,
    from_country_code    CHAR(3 CHAR) NOT NULL,
    to_country_code      CHAR(3 CHAR) NOT NULL,
    port_of_exit         VARCHAR2(120 CHAR) NOT NULL,
    travel_doc_no        VARCHAR2(50 CHAR),
    purpose              VARCHAR2(80 CHAR),
    created_at           TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at           TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT fk_exit_record_person
        FOREIGN KEY (person_id) REFERENCES person_identity (person_id)
);
