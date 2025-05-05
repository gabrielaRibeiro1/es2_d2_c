using ESOF.WebApp.DBLayer.Context;
using ESOF.WebApp.DBLayer.Entities;
namespace ESOF.WebApp.Services.Reports;


public class CategoryCountryReport : ReportGenerator
{
    private readonly ApplicationDbContext _context;

    public CategoryCountryReport(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override List<TalentProfile> GetData()
    {
        return _context.TalentProfiles.ToList();
    }

    protected override Dictionary<string, List<float>> GroupData(List<TalentProfile> data)
    {
        return data
            .GroupBy(tp => $"{tp.category}-{tp.country}")
            .ToDictionary(g => g.Key, g => g.Select(tp => tp.price).ToList());
    }
}
