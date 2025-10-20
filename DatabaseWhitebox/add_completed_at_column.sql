-- Add completed_at column to booking_sessions table
-- This column will store the timestamp when a session is completed

ALTER TABLE booking_sessions 
ADD COLUMN IF NOT EXISTS completed_at TIMESTAMP WITH TIME ZONE;

-- Add comment to the column
COMMENT ON COLUMN booking_sessions.completed_at IS 'Timestamp when the tutoring session was completed';



