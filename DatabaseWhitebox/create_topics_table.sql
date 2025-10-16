-- Create topics table
CREATE TABLE IF NOT EXISTS topics (
    topic_id SERIAL PRIMARY KEY,
    subject_id INTEGER NOT NULL REFERENCES subjects(subject_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    order_number INTEGER NOT NULL DEFAULT 1,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    -- Ensure topic titles are unique within a subject
    UNIQUE(subject_id, title)
);

-- Create index on subject_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_topics_subject_id ON topics(subject_id);

-- Create index on order_number for sorting
CREATE INDEX IF NOT EXISTS idx_topics_order_number ON topics(subject_id, order_number);

-- Create index on is_active for filtering active topics
CREATE INDEX IF NOT EXISTS idx_topics_is_active ON topics(is_active);

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


