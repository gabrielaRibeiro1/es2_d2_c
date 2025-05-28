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

-- 1) Usuários (Users) — para servir de dono de perfis e de propostas
INSERT INTO es2.public."Users" (user_id, username, "passwordHash", "passwordSalt", fk_role_id)
VALUES
    -- donos de perfis
    (201, 'alice', ''::bytea, ''::bytea, 1),
    (202, 'bruno', ''::bytea, ''::bytea, 1),
    (203, 'carla', ''::bytea, ''::bytea, 1),
    (204, 'daniel',''::bytea, ''::bytea, 1),
    (1001,'companyA',''::bytea,''::bytea,1),
    (1002,'companyB',''::bytea,''::bytea,1);
  

-- 2) Propostas de trabalho (WorkProposals)
INSERT INTO es2.public."WorkProposals" (
    proposal_id,
    proposal_name,
    category,
    necessary_skills,
    years_of_experience,
    description,
    total_hours,
    fk_user_id
)
VALUES
    (11, 'Desenvolvimento API', 'Backend',
     'C#,ASP.NET Core', 3,
     'Criar endpoints RESTful', 120, 1001),
    (12, 'UI React',             'Frontend',
     'JavaScript,React', 2,
     'Desenvolver UI em React',  80, 1002);

-- 3) Perfis de talento (TalentProfiles)
INSERT INTO es2.public."TalentProfiles" (
    profile_id,
    profile_name,
    country,
    email,
    price,
    privacy,
    category,
    fk_user_id
)
VALUES
    (11, 'Alice Silva',    'Portugal', 'alice@mail.com',    30.0, 0, 'Backend', 201),
    (12, 'Bruno Costa',    'Portugal', 'bruno@mail.com',    50.0, 0, 'Backend', 202),
    (13, 'Carla Ferreira', 'Portugal', 'carla@mail.com',    40.0, 0, 'Frontend',203),
    (14, 'Daniel Gomes',   'Portugal', 'daniel@mail.com',   60.0, 1, 'Backend',204);
-- Daniel tem privacy = 1, não deverá surgir no resultado

-- 4) Catálogo de skills (Skills)
INSERT INTO es2.public."Skills" (
    skill_id,
    name,
    area
)
VALUES
    (11, 'C#',            'Backend'),
    (12, 'ASP.NET Core',  'Backend'),
    (13, 'JavaScript',    'Frontend'),
    (14, 'React',         'Frontend');

-- 5) Relação TalentProfile ↔ Skill (TalentProfileSkills)
--   (Id é PK auto-increment, mas colocamos valores >10 para evitar choque)
INSERT INTO es2.public."TalentProfileSkills" (
    "Id",
    "TalentProfileId",
    "SkillId",
    "YearsOfExperience"
)
VALUES
    (21, 11, 11, 3),  -- Alice: C# 3 anos
    (22, 11, 12, 2),  -- Alice: ASP.NET Core 2 anos
    (23, 12, 11, 5),  -- Bruno: C# 5 anos
    (24, 12, 12, 4),  -- Bruno: ASP.NET Core 4 anos
    (25, 13, 13, 2),  -- Carla: JS 2 anos
    (26, 13, 14, 1);  -- Carla: React 1 ano

-- 6) Experiências (Experiences)
INSERT INTO es2.public."Experiences" (
    experience_id,
    company_name,
    start_year,
    end_year,
    fk_profile_id
)
VALUES
    -- Alice (profile_id = 11)
    (1011, 'Empresa A', 2018, 2020, 11),
    (1012, 'Empresa B', 2020, 2023, 11),
    -- Bruno (profile_id = 12)
    (1021, 'Startup X', 2015, 2018, 12),
    (1022, 'Corp Y',    2018, 2024, 12),
    -- Carla (profile_id = 13)
    (1031, 'Agência Z', 2021, 2023, 13);
