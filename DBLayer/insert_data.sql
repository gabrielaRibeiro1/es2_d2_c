-- Inserir Roles
INSERT INTO es2.public."Roles" (role, role_id) VALUES
                                                  ('Admin', 1),
                                                  ('UserManager', 2),
                                                  ('User', 3);

--IMPORTANTE : CRIAR UM USER ATRAVÉS DA API POR CAUSA DA PASSWORD HASH 
-- DEPOIS DISSO PODEM CORRER O RESTO DAS QUERIES

-- Inserir Skills
INSERT INTO es2.public."Skills" (name, area, skill_id) VALUES
                                              ('JAVA', 'Programming', 4),
                                              ('ORACLE', 'Database', 5),
                                              ('Product Owner', 'Management', 6);

-- Inserir UserSkills
INSERT INTO es2.public."UserSkills" ("UserId", "SkillId") VALUES
                                             (4, 1),
                                             (4, 2),
                                             (4, 3);

-- Inserir TalentProfiles
INSERT INTO es2.public."TalentProfiles"  (profile_name, country, email, price, privacy, category, fk_user_id)
VALUES ('John Doe', 'Brazil', 'john@example.com', 50.0, 0, 'Backend', 4);

INSERT INTO es2.public."TalentProfileSkills" ("TalentProfileId", "SkillId", "YearsOfExperience")
VALUES
    (4, 1, 3),
    (4, 2, 5);
-- Inserir Experiences
INSERT INTO es2.public."Experiences" (company_name, start_year, end_year, fk_profile_id, experience_id) VALUES
                                                                                               ('Tech Corp', '2018', '2021', 4, 1),
                                                                                               ('Data Solutions', '2021', 'Present', 4, 2);

-- Inserir WorkProposals
INSERT INTO es2.public."WorkProposals" (proposal_name, category, necessary_skills, years_of_experience, description, total_hours, fk_user_id, proposal_id) VALUES
                                                                                                                                                  ('Data Engineer Project', 'IT', 'Python, SQL', 3, 'Develop data pipelines', 200, 2, 1),
                                                                                                                                                  ('Project Manager Role', 'Management', 'Project Management', 5, 'Oversee IT projects', 300, 2, 2);
