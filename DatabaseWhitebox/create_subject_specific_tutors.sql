-- Create subject-specific tutor system
-- This script modifies the existing tutor system to be subject-specific

-- ========================================
-- 1. UPDATE TUTOR_APPLICATIONS TABLE TO INCLUDE SUBJECT_ID
-- ========================================
-- Add subject_id column to tutor_applications table
ALTER TABLE tutor_applications 
ADD COLUMN IF NOT EXISTS subject_id INTEGER REFERENCES subjects(subject_id) ON DELETE CASCADE;

-- Create index for better performance
CREATE INDEX IF NOT EXISTS idx_tutor_applications_subject_id ON tutor_applications(subject_id);

-- Update comment for documentation
COMMENT ON COLUMN tutor_applications.subject_id IS 'Subject the user is applying to tutor for';

-- ========================================
-- 2. CREATE SUBJECT_TUTORS TABLE (MANY-TO-MANY RELATIONSHIP)
-- ========================================
CREATE TABLE IF NOT EXISTS subject_tutors (
    subject_tutor_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    subject_id INTEGER NOT NULL REFERENCES subjects(subject_id) ON DELETE CASCADE,
    is_active BOOLEAN NOT NULL DEFAULT true,
    approved_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    approved_by INTEGER REFERENCES users(user_id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Ensure a user can only be a tutor for each subject once
    UNIQUE(user_id, subject_id)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_subject_tutors_user_id ON subject_tutors(user_id);
CREATE INDEX IF NOT EXISTS idx_subject_tutors_subject_id ON subject_tutors(subject_id);
CREATE INDEX IF NOT EXISTS idx_subject_tutors_is_active ON subject_tutors(is_active);

-- Add comments for documentation
COMMENT ON TABLE subject_tutors IS 'Maps users to subjects they can tutor (many-to-many relationship)';
COMMENT ON COLUMN subject_tutors.subject_tutor_id IS 'Unique identifier for the subject-tutor relationship';
COMMENT ON COLUMN subject_tutors.user_id IS 'User who is a tutor';
COMMENT ON COLUMN subject_tutors.subject_id IS 'Subject the user can tutor';
COMMENT ON COLUMN subject_tutors.is_active IS 'Whether this tutor is currently active for this subject';
COMMENT ON COLUMN subject_tutors.approved_at IS 'When this tutor was approved for this subject';
COMMENT ON COLUMN subject_tutors.approved_by IS 'Admin user who approved this tutor for this subject';

-- ========================================
-- 3. UPDATE EXISTING DATA (IF ANY)
-- ========================================
-- For existing approved tutor applications, create subject_tutors entries
-- Note: This assumes all existing approved tutors can tutor for all subjects
-- You may want to customize this based on your specific needs
INSERT INTO subject_tutors (user_id, subject_id, is_active, approved_at, approved_by)
SELECT DISTINCT 
    ta.user_id,
    s.subject_id,
    true,
    ta.reviewed_at,
    ta.reviewed_by
FROM tutor_applications ta
CROSS JOIN subjects s
WHERE ta.status = 'approved' 
  AND s.is_active = true
ON CONFLICT (user_id, subject_id) DO NOTHING;

-- ========================================
-- 4. CREATE VIEW FOR EASY TUTOR QUERIES
-- ========================================
CREATE OR REPLACE VIEW active_subject_tutors AS
SELECT 
    st.subject_tutor_id,
    st.user_id,
    st.subject_id,
    st.is_active,
    st.approved_at,
    st.approved_by,
    u.first_name,
    u.last_name,
    u.email,
    u.profile_picture_path,
    s.subject_code,
    s.name as subject_name,
    s.year as subject_year
FROM subject_tutors st
JOIN users u ON st.user_id = u.user_id
JOIN subjects s ON st.subject_id = s.subject_id
WHERE st.is_active = true 
  AND s.is_active = true
  AND u.role = 'tutor';

-- Add comment for documentation
COMMENT ON VIEW active_subject_tutors IS 'View of all active tutors for each subject with user and subject details';

-- ========================================
-- 5. VERIFY CREATION
-- ========================================
-- Uncomment these lines to verify the tables were created successfully
-- SELECT 'Subject tutors table created successfully' as status, COUNT(*) as tutor_count FROM subject_tutors;
-- SELECT 'Active subject tutors view created successfully' as status, COUNT(*) as active_tutor_count FROM active_subject_tutors;
