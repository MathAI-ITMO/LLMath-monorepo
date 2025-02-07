-- +goose Up
-- +goose StatementBegin
create table users (
    id                bigint            primary key generated always as identity
  , first_name        VARCHAR(255)      null
  , last_name         VARCHAR(255)      null
  , isu_id            bigint            null
  , created_at        timestamptz       not null default CURRENT_TIMESTAMP
  , updated_at        timestamptz       null 
  , deleted_at        timestamptz       null
);

create unique index user_isu_id_uindex ON users (isu_id) nulls not distinct;
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table if exists users;
-- +goose StatementEnd
