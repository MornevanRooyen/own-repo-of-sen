-- Create session_messages table
CREATE TABLE IF NOT EXISTS session_messages (
    message_id SERIAL PRIMARY KEY,
    session_id INTEGER NOT NULL REFERENCES booking_sessions(booking_id) ON DELETE CASCADE,
    sender_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    sender_name VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    is_file BOOLEAN NOT NULL DEFAULT FALSE,
    file_id INTEGER, -- Will add foreign key constraint after session_resources table is created
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for session_messages
CREATE INDEX IF NOT EXISTS idx_session_messages_session_id ON session_messages(session_id);
CREATE INDEX IF NOT EXISTS idx_session_messages_sender_id ON session_messages(sender_id);
CREATE INDEX IF NOT EXISTS idx_session_messages_created_at ON session_messages(created_at);

-- Create session_resources table
CREATE TABLE IF NOT EXISTS session_resources (
    file_id SERIAL PRIMARY KEY,
    session_id INTEGER NOT NULL REFERENCES booking_sessions(booking_id) ON DELETE CASCADE,
    file_name VARCHAR(255) NOT NULL,
    file_size BIGINT NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    uploaded_by VARCHAR(255) NOT NULL,
    uploaded_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for session_resources
CREATE INDEX IF NOT EXISTS idx_session_resources_session_id ON session_resources(session_id);
CREATE INDEX IF NOT EXISTS idx_session_resources_uploaded_by ON session_resources(uploaded_by);
CREATE INDEX IF NOT EXISTS idx_session_resources_uploaded_at ON session_resources(uploaded_at);

-- Add foreign key constraint to session_messages.file_id
ALTER TABLE session_messages 
ADD CONSTRAINT fk_session_messages_file_id 
FOREIGN KEY (file_id) REFERENCES session_resources(file_id) ON DELETE SET NULL;

-- Add comments for documentation
COMMENT ON TABLE session_messages IS 'Stores chat messages for tutoring sessions';
COMMENT ON TABLE session_resources IS 'Stores files and resources shared during tutoring sessions';

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON session_messages TO authenticated;
GRANT SELECT, INSERT, UPDATE, DELETE ON session_resources TO authenticated;
GRANT USAGE, SELECT ON SEQUENCE session_messages_message_id_seq TO authenticated;
GRANT USAGE, SELECT ON SEQUENCE session_resources_file_id_seq TO authenticated;
