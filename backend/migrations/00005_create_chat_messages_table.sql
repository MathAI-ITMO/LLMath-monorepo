-- +goose Up
-- +goose StatementBegin
create table chat_messages (
      id            bigint        primary key generated always as identity
    , chat_id       bigint        not null references chats(id)
    , message_type  message_type  not null
    , text          varchar(2000) not null
    , created_at    timestamptz   not null default now()
);

-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
SELECT 'down SQL query';
-- +goose StatementEnd
