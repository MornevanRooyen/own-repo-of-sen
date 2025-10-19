-- Clear all tutoring sessions from the database
-- WARNING: This will delete ALL booking sessions and related data

-- First, let's see how many sessions we have
SELECT COUNT(*) as total_sessions FROM booking_sessions;

-- Delete all calendar events related to booking sessions
DELETE FROM calendar_events WHERE booking_id IS NOT NULL;

-- Delete all session messages
DELETE FROM session_messages;

-- Delete all session resources
DELETE FROM session_resources;

-- Delete all booking sessions
DELETE FROM booking_sessions;

-- Show the results
SELECT COUNT(*) as remaining_sessions FROM booking_sessions;
SELECT COUNT(*) as remaining_calendar_events FROM calendar_events WHERE booking_id IS NOT NULL;
SELECT COUNT(*) as remaining_session_messages FROM session_messages;
SELECT COUNT(*) as remaining_session_resources FROM session_resources;

-- Reset the sequence for booking_id to start from 1 again
SELECT setval('booking_sessions_booking_id_seq', 1, false);
