-- +goose Up
-- +goose StatementBegin
create table chats (
      id        bigint primary key generated always as identity
    , name      varchar(255)     not null
    , user_id   bigint           not null references users (id) on delete cascade
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table if exists chats;
-- +goose StatementEnd
