-- +goose Up
-- +goose StatementBegin
create type message_type as enum('system', 'user', 'assistant');
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop type if exists message_type;
-- +goose StatementEnd
