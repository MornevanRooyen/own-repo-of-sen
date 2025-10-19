-- Create calendar_events table
CREATE TABLE IF NOT EXISTS calendar_events (
    event_id SERIAL PRIMARY KEY,
    booking_id INTEGER REFERENCES booking_sessions(booking_id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    title VARCHAR(100) NOT NULL,
    description TEXT,
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    event_type VARCHAR(20) NOT NULL DEFAULT 'booking', -- booking, reminder, etc.
    is_all_day BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Ensure end time is after start time
    CONSTRAINT check_time_order CHECK (end_time > start_time)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_calendar_events_user_id ON calendar_events(user_id);
CREATE INDEX IF NOT EXISTS idx_calendar_events_booking_id ON calendar_events(booking_id);
CREATE INDEX IF NOT EXISTS idx_calendar_events_start_time ON calendar_events(start_time);
CREATE INDEX IF NOT EXISTS idx_calendar_events_end_time ON calendar_events(end_time);
CREATE INDEX IF NOT EXISTS idx_calendar_events_event_type ON calendar_events(event_type);

-- Create a view for calendar events with user details
CREATE OR REPLACE VIEW calendar_events_details AS
SELECT
    ce.event_id,
    ce.booking_id,
    ce.user_id,
    ce.title,
    ce.description,
    ce.start_time,
    ce.end_time,
    ce.event_type,
    ce.is_all_day,
    ce.created_at,
    ce.updated_at,
    
    -- User details
    u.first_name,
    u.last_name,
    u.email,
    
    -- Booking details (if applicable)
    bs.subject_id,
    bs.status as booking_status,
    s.subject_code,
    s.name as subject_name
FROM calendar_events ce
JOIN users u ON ce.user_id = u.user_id
LEFT JOIN booking_sessions bs ON ce.booking_id = bs.booking_id
LEFT JOIN subjects s ON bs.subject_id = s.subject_id;

-- Add comment for documentation
COMMENT ON TABLE calendar_events IS 'Calendar events for users, including booking sessions';
COMMENT ON VIEW calendar_events_details IS 'View of calendar events with user and booking details';

-- ========================================
-- 3. GRANT PERMISSIONS
-- ========================================
-- Grant necessary permissions to the authenticated role
GRANT SELECT, INSERT, UPDATE, DELETE ON calendar_events TO authenticated;
GRANT SELECT ON calendar_events_details TO authenticated;

