using CsvHelper.Configuration;
using CsvHelper;
using Recommender.Model;
using System.Globalization;
using System.Linq;

namespace DataSpliter;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine($"Start: {DateTime.Now}");

        int size = Int32.Parse(args[0]);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture);

        using var ratingsReader = new StreamReader(args[1]);
        using var ratingsCsv = new CsvReader(ratingsReader, config);
        var ratingRecords = ratingsCsv.GetRecords<Rating>();

        var userRecords = ratingRecords.GroupBy(e => e.UserId)
            .Where(e => e.Count() > 10) // 过滤出超过十条数据的用户, 以便于拆分训练集和测试集
            .OrderBy(e => Guid.NewGuid()) // shuffle the list
            .ToList();

        var groups = userRecords.Take(size);

        List<Rating> testset = new List<Rating>();
        foreach ( var ratings in groups)
        {
            testset.AddRange(ratings.OrderByDescending(e => e.Timestamp).Take(5).ToList());
        }

        using var testsetWriter = new StreamWriter(args[3]);
        using var testsetCsvWriter = new CsvWriter(testsetWriter, config);
        testsetCsvWriter.WriteRecords(testset);

        List<Rating> trainset = new List<Rating>();
        foreach ( var ratings in groups)
        {
            trainset.AddRange(ratings.OrderByDescending(e => e.Timestamp).Skip(5).ToList());
        }

        using var trainsetWriter = new StreamWriter(args[2]);
        using var trainsetCsvWriter = new CsvWriter(trainsetWriter, config);
        trainsetCsvWriter.WriteRecords(trainset);

        Console.WriteLine($"End: {DateTime.Now}");
    }
}