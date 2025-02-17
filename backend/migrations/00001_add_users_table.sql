-- +goose Up
-- +goose StatementBegin
create table users (
    id                bigint            primary key generated always as identity
  , first_name        VARCHAR(255)      null
  , last_name         VARCHAR(255)      null
  , created_at        timestamptz       not null default CURRENT_TIMESTAMP
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table if exists users;
-- +goose StatementEnd
