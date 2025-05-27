using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace ESOF.WebApp.WebAPI.DTOs;

public class TalentProfileDto
{
    public int ProfileId { get; set; }
    public string ProfileName { get; set; }
    public string Country { get; set; }
    public string Email { get; set; }
    public float Price { get; set; }
    public int Privacy { get; set; }
    public string Category { get; set; }
    public int FkUserId { get; set; }
    public float TotalValue { get; set; } // <- Adicionado


    public List<SkillDto> Skills { get; set; }
    public List<ExperienceDto> Experiences { get; set; }
}

public class SkillDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; }
    public int YearsOfExperience { get; set; }
}

public class ExperienceDto
{
    public int ExperienceId { get; set; }
    public string CompanyName { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    
    public string ProfileName  { get; set; }
}


