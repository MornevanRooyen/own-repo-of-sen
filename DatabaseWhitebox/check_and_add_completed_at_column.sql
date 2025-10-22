-- Check if completed_at column exists and add it if it doesn't
-- This script will safely add the column without errors if it already exists

-- First, check if the column exists
DO $$
BEGIN
    -- Check if the column exists
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'booking_sessions' 
        AND column_name = 'completed_at'
    ) THEN
        -- Add the column if it doesn't exist
        ALTER TABLE booking_sessions 
        ADD COLUMN completed_at TIMESTAMP WITH TIME ZONE;
        
        -- Add comment to the column
        COMMENT ON COLUMN booking_sessions.completed_at IS 'Timestamp when the tutoring session was completed';
        
        RAISE NOTICE 'Column completed_at added to booking_sessions table';
    ELSE
        RAISE NOTICE 'Column completed_at already exists in booking_sessions table';
    END IF;
END $$;

-- Show the current structure of the booking_sessions table
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'booking_sessions' 
ORDER BY ordinal_position;




