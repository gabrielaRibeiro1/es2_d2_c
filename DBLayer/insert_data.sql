-- Inserir Roles
INSERT INTO es2.public."Roles" (role, role_id) VALUES
                                                   ('Admin', 7),
                                                   ('UserManager', 8),
                                                   ('User', 9);

-- Inserir Users
INSERT INTO es2.public."Users" (username, fk_role_id, user_id) VALUES
                                                                   ('admin_user', 'adminpass', 1, 1),
                                                                   ('recruiter1', 'recruitpass', 2, 2),
                                                                   ('talent1', 'talentpass', 3, 3);

-- Inserir Skills
INSERT INTO es2.public."Skills" (name, area, skill_id) VALUES
                                                           ('Python', 'Programming', 1),
                                                           ('SQL', 'Database', 2),
                                                           ('Project Management', 'Management', 3);

-- Inserir UserSkills
INSERT INTO es2.public."UserSkills" ("UserId", "SkillId") VALUES
                                                              (3, 1),
                                                              (3, 2),
                                                              (2, 3);

-- Inserir TalentProfiles
INSERT INTO es2.public."TalentProfiles" (profile_name, country, email, price, privacy, fk_user_id, user_id, profile_id) VALUES
    ('John Doe', 'USA', 'john@example.com', 50.0, 1, 3, 3, 1);

-- Inserir Experiences
INSERT INTO es2.public."Experiences" (company_name, start_year, end_year, fk_profile_id, experience_id) VALUES
                                                                                                            ('Tech Corp', '2018', '2021', 1, 1),
                                                                                                            ('Data Solutions', '2021', 'Present', 1, 2);

-- Inserir WorkProposals
INSERT INTO es2.public."WorkProposals" (proposal_name, category, necessary_skills, years_of_experience, description, total_hours, fk_user_id, proposal_id) VALUES
                                                                                                                                                               ('Data Engineer Project', 'IT', 'Python, SQL', 3, 'Develop data pipelines', 200, 2, 1),
                                                                                                                                                               ('Project Manager Role', 'Management', 'Project Management', 5, 'Oversee IT projects', 300, 2, 2);
