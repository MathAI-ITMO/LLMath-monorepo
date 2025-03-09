-- +goose Up
-- +goose StatementBegin
create table users (
    id                uuid              primary key generated always as identity
  , name              VARCHAR(255)      not null
  , normalized_name   VARCHAR(255)      not null unique 
  , created_at        timestamptz       not null default CURRENT_TIMESTAMP
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table if exists users;
-- +goose StatementEnd
