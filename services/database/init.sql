CREATE EXTENSION if not exists pgcrypto;

CREATE TABLE users_user (
    id UUID NOT NULL DEFAULT gen_random_uuid(), 
    name VARCHAR(256) NOT NULL,
    password VARCHAR(256) NOT NULL,
    CONSTRAINT user_user_id PRIMARY KEY (id)
);