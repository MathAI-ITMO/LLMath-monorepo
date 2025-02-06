-- +goose Up
-- +goose StatementBegin
create table sessions (
      id          bigint                         primary key generated always as identity
    , user_id     int                 not null   references users(id) on delete cascade
    , token       uuid unique         not null   default gen_random_uuid()
    , created_at  timestamp           not null   default current_timestamp
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table if exists sessions;
-- +goose StatementEnd
