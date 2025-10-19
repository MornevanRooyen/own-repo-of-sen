-- Create subject subscription system
-- This script creates the necessary tables and views for subject subscriptions

-- ========================================
-- 1. CREATE SUBJECT_SUBSCRIPTIONS TABLE
-- ========================================
CREATE TABLE IF NOT EXISTS subject_subscriptions (
    subscription_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    subject_id INTEGER NOT NULL REFERENCES subjects(subject_id) ON DELETE CASCADE,
    subscribed_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT true,
    
    -- Ensure a user can only have one subscription per subject
    UNIQUE(user_id, subject_id)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_subject_subscriptions_user_id ON subject_subscriptions(user_id);
CREATE INDEX IF NOT EXISTS idx_subject_subscriptions_subject_id ON subject_subscriptions(subject_id);
CREATE INDEX IF NOT EXISTS idx_subject_subscriptions_active ON subject_subscriptions(is_active);

-- Add comment for documentation
COMMENT ON TABLE subject_subscriptions IS 'Tracks which users are subscribed to which subjects';

-- ========================================
-- 2. CREATE VIEW FOR ACTIVE SUBSCRIPTIONS
-- ========================================
CREATE OR REPLACE VIEW active_subject_subscriptions AS
SELECT 
    ss.subscription_id,
    ss.user_id,
    ss.subject_id,
    ss.subscribed_at,
    ss.is_active,
    u.first_name,
    u.last_name,
    u.email,
    s.subject_code,
    s.name as subject_name,
    s.year as subject_year
FROM subject_subscriptions ss
JOIN users u ON ss.user_id = u.user_id
JOIN subjects s ON ss.subject_id = s.subject_id
WHERE ss.is_active = true AND s.is_active = true;

-- Add comment for documentation
COMMENT ON VIEW active_subject_subscriptions IS 'View of active subscriptions with user and subject details';

-- ========================================
-- 3. INSERT DEFAULT SUBSCRIPTIONS (OPTIONAL)
-- ========================================
-- You can uncomment this section to add default subscriptions for existing users
-- This would give all users access to all subjects initially

/*
INSERT INTO subject_subscriptions (user_id, subject_id, is_active)
SELECT 
    u.user_id,
    s.subject_id,
    true
FROM users u
CROSS JOIN subjects s
WHERE s.is_active = true
ON CONFLICT (user_id, subject_id) DO NOTHING;
*/

-- ========================================
-- 4. GRANT PERMISSIONS
-- ========================================
-- Grant necessary permissions (adjust as needed for your setup)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON subject_subscriptions TO your_app_user;
-- GRANT SELECT ON active_subject_subscriptions TO your_app_user;
