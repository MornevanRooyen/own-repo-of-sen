-- Create tutor_applications table
CREATE TABLE IF NOT EXISTS tutor_applications (
    application_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL,
    phone_num VARCHAR(20),
    student_no VARCHAR(20),
    major VARCHAR(100),
    year_of_study INTEGER,
    completed_sessions INTEGER DEFAULT 0,
    min_required_grade INTEGER,
    profile_picture_path TEXT,
    transcript_path TEXT,
    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'approved', 'declined')),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    reviewed_at TIMESTAMP WITH TIME ZONE,
    reviewed_by INTEGER REFERENCES users(user_id) ON DELETE SET NULL,
    review_notes TEXT
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_tutor_applications_user_id ON tutor_applications(user_id);
CREATE INDEX IF NOT EXISTS idx_tutor_applications_status ON tutor_applications(status);
CREATE INDEX IF NOT EXISTS idx_tutor_applications_created_at ON tutor_applications(created_at DESC);

-- Add comments for documentation
COMMENT ON TABLE tutor_applications IS 'Stores tutor applications submitted by students';
COMMENT ON COLUMN tutor_applications.application_id IS 'Unique identifier for the application';
COMMENT ON COLUMN tutor_applications.user_id IS 'Reference to the user who submitted the application';
COMMENT ON COLUMN tutor_applications.status IS 'Application status: pending, approved, or declined';
COMMENT ON COLUMN tutor_applications.reviewed_by IS 'Admin user ID who reviewed the application';
COMMENT ON COLUMN tutor_applications.transcript_path IS 'Path to the uploaded transcript PDF file';

