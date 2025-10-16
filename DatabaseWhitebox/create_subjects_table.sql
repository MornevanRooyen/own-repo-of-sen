-- Create subjects table
CREATE TABLE IF NOT EXISTS subjects (
    subject_id SERIAL PRIMARY KEY,
    subject_code VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    year INTEGER NOT NULL CHECK (year >= 1 AND year <= 5),
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Create index on subject_code for faster lookups
CREATE INDEX IF NOT EXISTS idx_subjects_subject_code ON subjects(subject_code);

-- Create index on is_active for filtering active subjects
CREATE INDEX IF NOT EXISTS idx_subjects_is_active ON subjects(is_active);

-- Insert sample data
INSERT INTO subjects (subject_code, name, description, year, is_active) VALUES
('SEN381', 'Software Engineering 3', 'Advanced software engineering concepts and practices', 3, true),
('BUM281', 'Business Management 2', 'Business management principles and strategies', 2, true),
('ENT381', 'Entrepreneurship 3', 'Entrepreneurship and business development', 3, true),
('MAT181', 'Mathematics 1', 'Fundamental mathematics and calculus', 1, true)
ON CONFLICT (subject_code) DO NOTHING;
