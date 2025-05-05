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

    public List<SkillDto> Skills { get; set; }
}

public class SkillDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; }
    public int YearsOfExperience { get; set; }
}
