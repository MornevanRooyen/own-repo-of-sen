-- Create user_follows table for follow/unfollow functionality
CREATE TABLE IF NOT EXISTS user_follows (
    id SERIAL PRIMARY KEY,
    follower_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    following_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Ensure a user can't follow the same person twice
    UNIQUE(follower_id, following_id),
    
    -- Ensure a user can't follow themselves
    CHECK (follower_id != following_id)
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_user_follows_follower ON user_follows(follower_id);
CREATE INDEX IF NOT EXISTS idx_user_follows_following ON user_follows(following_id);

-- Grant necessary permissions (adjust as needed for your setup)
-- GRANT SELECT, INSERT, DELETE ON user_follows TO your_app_user;
-- GRANT USAGE, SELECT ON SEQUENCE user_follows_id_seq TO your_app_user;


