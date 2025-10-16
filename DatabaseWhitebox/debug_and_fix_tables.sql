-- Debug script to check table status and fix issues
-- Run this to diagnose what's happening

-- ========================================
-- 1. CHECK IF SUBJECTS TABLE EXISTS
-- ========================================
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'subjects'
ORDER BY ordinal_position;

-- ========================================
-- 2. CHECK IF TOPICS TABLE EXISTS
-- ========================================
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'topics'
ORDER BY ordinal_position;

-- ========================================
-- 3. DROP AND RECREATE TABLES (if needed)
-- ========================================

-- Drop topics table first (if it exists)
DROP TABLE IF EXISTS topics CASCADE;

-- Drop subjects table (if it exists)
DROP TABLE IF EXISTS subjects CASCADE;

-- ========================================
-- 4. CREATE SUBJECTS TABLE (STANDALONE)
-- ========================================
CREATE TABLE subjects (
    subject_id SERIAL PRIMARY KEY,
    subject_code VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    year INTEGER NOT NULL CHECK (year >= 1 AND year <= 5),
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Insert sample subjects data
INSERT INTO subjects (subject_code, name, description, year, is_active) VALUES
('SEN381', 'Software Engineering 3', 'Advanced software engineering concepts and practices', 3, true),
('BUM281', 'Business Management 2', 'Business management principles and strategies', 2, true),
('ENT381', 'Entrepreneurship 3', 'Entrepreneurship and business development', 3, true),
('MAT181', 'Mathematics 1', 'Fundamental mathematics and calculus', 1, true)
ON CONFLICT (subject_code) DO NOTHING;

-- ========================================
-- 5. CREATE TOPICS TABLE (STANDALONE)
-- ========================================
CREATE TABLE topics (
    topic_id SERIAL PRIMARY KEY,
    subject_id INTEGER NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    order_number INTEGER NOT NULL DEFAULT 1,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    -- Ensure topic titles are unique within a subject
    UNIQUE(subject_id, title),
    
    -- Add foreign key constraint separately
    CONSTRAINT fk_topics_subject_id 
        FOREIGN KEY (subject_id) 
        REFERENCES subjects(subject_id) 
        ON DELETE CASCADE
);

-- ========================================
-- 6. CREATE INDEXES
-- ========================================
CREATE INDEX idx_subjects_subject_code ON subjects(subject_code);
CREATE INDEX idx_subjects_is_active ON subjects(is_active);

CREATE INDEX idx_topics_subject_id ON topics(subject_id);
CREATE INDEX idx_topics_order_number ON topics(subject_id, order_number);
CREATE INDEX idx_topics_is_active ON topics(is_active);

-- ========================================
-- 7. INSERT SAMPLE TOPICS DATA
-- ========================================

-- Insert sample data for SEN381 (Software Engineering 3)
INSERT INTO topics (subject_id, title, description, order_number, is_active) 
SELECT s.subject_id, t.title, t.description, t.order_number, t.is_active
FROM subjects s,
(VALUES 
    ('Project Management', 'Software project management methodologies and tools', 1, true),
    ('System Architecture', 'Software architecture patterns and design principles', 2, true),
    ('APIs', 'Application Programming Interfaces design and implementation', 3, true),
    ('Study Tips', 'Effective study strategies for software engineering', 4, true),
    ('Project Tips', 'Best practices for software development projects', 5, true)
) AS t(title, description, order_number, is_active)
WHERE s.subject_code = 'SEN381'
ON CONFLICT (subject_id, title) DO NOTHING;

-- Insert sample data for BUM281 (Business Management 2)
INSERT INTO topics (subject_id, title, description, order_number, is_active) 
SELECT s.subject_id, t.title, t.description, t.order_number, t.is_active
FROM subjects s,
(VALUES 
    ('Leadership', 'Leadership principles and management styles', 1, true),
    ('Lead her ship', 'Women in leadership and management', 2, true),
    ('How to not fail BUM. Trust.', 'Business management success strategies', 3, true)
) AS t(title, description, order_number, is_active)
WHERE s.subject_code = 'BUM281'
ON CONFLICT (subject_id, title) DO NOTHING;

-- Insert sample data for ENT381 (Entrepreneurship 3)
INSERT INTO topics (subject_id, title, description, order_number, is_active) 
SELECT s.subject_id, t.title, t.description, t.order_number, t.is_active
FROM subjects s,
(VALUES 
    ('Technical Feasibility', 'Assessing technical feasibility of business ideas', 1, true),
    ('S.W.O.T.', 'Strengths, Weaknesses, Opportunities, and Threats analysis', 2, true),
    ('The Life of a Business Gyal', 'Entrepreneurship journey and experiences', 3, true)
) AS t(title, description, order_number, is_active)
WHERE s.subject_code = 'ENT381'
ON CONFLICT (subject_id, title) DO NOTHING;

-- Insert sample data for MAT181 (Mathematics 1)
INSERT INTO topics (subject_id, title, description, order_number, is_active) 
SELECT s.subject_id, t.title, t.description, t.order_number, t.is_active
FROM subjects s,
(VALUES 
    ('Parabolas and the Quadratic function', 'Quadratic functions and their graphs', 1, true),
    ('Where tf the function?', 'Function concepts and applications', 2, true),
    ('Numbers and stuff like that', 'Number theory and mathematical foundations', 3, true)
) AS t(title, description, order_number, is_active)
WHERE s.subject_code = 'MAT181'
ON CONFLICT (subject_id, title) DO NOTHING;

-- ========================================
-- 8. VERIFY CREATION
-- ========================================
SELECT 'Subjects table created successfully' as status, COUNT(*) as subject_count FROM subjects;
SELECT 'Topics table created successfully' as status, COUNT(*) as topic_count FROM topics;

