using ESOF.WebApp.DBLayer.Entities;
namespace ESOF.WebApp.Services.Reports;

public abstract class ReportGenerator
{
    protected const float MonthlyHours = 176;

    public Dictionary<string, float> GenerateReport()
    {
        var data = GetData();
        var groupedData = GroupData(data);
        return CalculateMonthlyAverage(groupedData);
    }

    protected abstract List<TalentProfile> GetData();
    protected abstract Dictionary<string, List<float>> GroupData(List<TalentProfile> data);

    protected Dictionary<string, float> CalculateMonthlyAverage(Dictionary<string, List<float>> groupedData)
    {
        return groupedData.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Average() * MonthlyHours
        );
    }
}
