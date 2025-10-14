-- Create direct_messages table for user-to-user messaging
CREATE TABLE IF NOT EXISTS direct_messages (
    id SERIAL PRIMARY KEY,
    sender_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    receiver_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    message_text TEXT NOT NULL,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_read BOOLEAN DEFAULT FALSE,
    
    -- Ensure a user can't message themselves
    CHECK (sender_id != receiver_id)
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_direct_messages_sender ON direct_messages(sender_id);
CREATE INDEX IF NOT EXISTS idx_direct_messages_receiver ON direct_messages(receiver_id);
CREATE INDEX IF NOT EXISTS idx_direct_messages_conversation ON direct_messages(sender_id, receiver_id);
CREATE INDEX IF NOT EXISTS idx_direct_messages_sent_at ON direct_messages(sent_at);

-- Grant necessary permissions (adjust as needed for your setup)
-- GRANT SELECT, INSERT, UPDATE ON direct_messages TO your_app_user;
-- GRANT USAGE, SELECT ON SEQUENCE direct_messages_id_seq TO your_app_user;

