-- Create booking_sessions table
CREATE TABLE IF NOT EXISTS booking_sessions (
    booking_id SERIAL PRIMARY KEY,
    tutor_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    student_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    subject_id INTEGER NOT NULL REFERENCES subjects(subject_id) ON DELETE CASCADE,
    title VARCHAR(100) NOT NULL,
    description TEXT,
    session_date TIMESTAMP WITH TIME ZONE NOT NULL,
    duration_minutes INTEGER NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending', -- pending, confirmed, cancelled, completed
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Ensure valid duration
    CONSTRAINT check_duration CHECK (duration_minutes > 0 AND duration_minutes <= 480), -- max 8 hours
    -- Ensure valid status
    CONSTRAINT check_status CHECK (status IN ('pending', 'confirmed', 'cancelled', 'completed'))
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_booking_sessions_tutor_id ON booking_sessions(tutor_id);
CREATE INDEX IF NOT EXISTS idx_booking_sessions_student_id ON booking_sessions(student_id);
CREATE INDEX IF NOT EXISTS idx_booking_sessions_subject_id ON booking_sessions(subject_id);
CREATE INDEX IF NOT EXISTS idx_booking_sessions_session_date ON booking_sessions(session_date);
CREATE INDEX IF NOT EXISTS idx_booking_sessions_status ON booking_sessions(status);

-- Create a view for booking sessions with user and subject details
CREATE OR REPLACE VIEW booking_sessions_details AS
SELECT
    bs.booking_id,
    bs.tutor_id,
    bs.student_id,
    bs.subject_id,
    bs.title,
    bs.description,
    bs.session_date,
    bs.duration_minutes,
    bs.status,
    bs.created_at,
    bs.updated_at,
    
    -- Tutor details
    t.first_name as tutor_first_name,
    t.last_name as tutor_last_name,
    t.email as tutor_email,
    
    -- Student details
    s.first_name as student_first_name,
    s.last_name as student_last_name,
    s.email as student_email,
    
    -- Subject details
    sub.subject_code,
    sub.name as subject_name,
    sub.year as subject_year
FROM booking_sessions bs
JOIN users t ON bs.tutor_id = t.user_id
JOIN users s ON bs.student_id = s.user_id
JOIN subjects sub ON bs.subject_id = sub.subject_id;

-- Add comment for documentation
COMMENT ON TABLE booking_sessions IS 'Records tutoring session bookings between students and tutors';
COMMENT ON VIEW booking_sessions_details IS 'View of booking sessions with user and subject details';

-- ========================================
-- 3. GRANT PERMISSIONS
-- ========================================
-- Grant necessary permissions to the authenticated role
GRANT SELECT, INSERT, UPDATE, DELETE ON booking_sessions TO authenticated;
GRANT SELECT ON booking_sessions_details TO authenticated;

