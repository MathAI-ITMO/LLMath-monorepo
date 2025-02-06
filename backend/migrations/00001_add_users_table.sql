-- +goose Up
-- +goose StatementBegin
create table users (
    id           bigint    primary key generated always as identity
  , first_name        text      null
  , last_name         text      null
  , isu_id            bigint    null
  , created_at        timestamp not null default CURRENT_TIMESTAMP
);

create unique index user_isu_id_uindex ON users (isu_id) nulls not distinct;
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table if exists users;
-- +goose StatementEnd
