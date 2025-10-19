-- Fix booking_sessions table sequence issue
-- This script ensures the booking_id sequence is properly set up

-- First, let's check if the sequence exists and is properly configured
SELECT setval('booking_sessions_booking_id_seq', (SELECT COALESCE(MAX(booking_id), 0) + 1 FROM booking_sessions), false);

-- Alternative approach: recreate the sequence if needed
-- DROP SEQUENCE IF EXISTS booking_sessions_booking_id_seq;
-- CREATE SEQUENCE booking_sessions_booking_id_seq;
-- ALTER TABLE booking_sessions ALTER COLUMN booking_id SET DEFAULT nextval('booking_sessions_booking_id_seq');
-- ALTER SEQUENCE booking_sessions_booking_id_seq OWNED BY booking_sessions.booking_id;

-- Grant permissions on the sequence
GRANT USAGE, SELECT ON SEQUENCE booking_sessions_booking_id_seq TO authenticated;

-- Verify the sequence is working
SELECT nextval('booking_sessions_booking_id_seq') as next_booking_id;

