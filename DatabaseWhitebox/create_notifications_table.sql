-- Create notifications table if it doesn't exist
CREATE TABLE IF NOT EXISTS notifications (
    notification_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL,
    subject_id INTEGER,
    title VARCHAR(255) NOT NULL,
    body TEXT NOT NULL,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    priority VARCHAR(20) DEFAULT 'normal',
    is_read BOOLEAN DEFAULT FALSE
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_notifications_user ON notifications(user_id);
CREATE INDEX IF NOT EXISTS idx_notifications_sent_at ON notifications(sent_at DESC);
CREATE INDEX IF NOT EXISTS idx_notifications_is_read ON notifications(is_read);
CREATE INDEX IF NOT EXISTS idx_notifications_user_unread ON notifications(user_id, is_read) WHERE is_read = FALSE;

-- =============================
-- IMPORTANT: Set up Row Level Security (RLS)
-- =============================

-- Enable RLS on notifications table
ALTER TABLE notifications ENABLE ROW LEVEL SECURITY;

-- Policy: Allow service role to do everything (for backend API)
CREATE POLICY "Allow service role full access" ON notifications
    FOR ALL
    USING (true)
    WITH CHECK (true);

-- Policy: Users can view their own notifications
CREATE POLICY "Users can view own notifications" ON notifications
    FOR SELECT
    USING (auth.uid()::text::int = user_id);

-- Policy: Users can update their own notifications (mark as read)
CREATE POLICY "Users can update own notifications" ON notifications
    FOR UPDATE
    USING (auth.uid()::text::int = user_id);

-- If the above policies don't work, you can temporarily disable RLS for testing:
-- ALTER TABLE notifications DISABLE ROW LEVEL SECURITY;
-- (NOT RECOMMENDED FOR PRODUCTION)

