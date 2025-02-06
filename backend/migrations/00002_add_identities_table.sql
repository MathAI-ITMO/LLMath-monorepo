-- +goose Up
-- +goose StatementBegin
create table identities (
      id            bigint                                primary key generated always as identity
    , user_id       int                       not null    references users(id) on delete cascade
    , email         text unique               not null
    , password_hash text                      not null
    , created_at    timestamp with time zone              default current_timestamp
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table if exists identities;
-- +goose StatementEnd
