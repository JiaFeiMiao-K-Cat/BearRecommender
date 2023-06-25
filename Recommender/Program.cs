using CsvHelper;
using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using Recommender.Model;
using System.Globalization;
using Recommender.FeatureEng;
using Recommender.Matching;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using NetTopologySuite.Utilities;

namespace Recommender;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine(DateTime.Now);

        var context = new Context("Data Source=model.db");
        context.Database.EnsureCreated();
        var filter = new CollaborativeFiltering(context);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture);

        using var reader = new StreamReader("test.csv");
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<Rating>();

        if (!MatrixFactorization.LoadModel())
        {
            MatrixFactorization.BuildAndTrainModel(context.Ratings);
            MatrixFactorization.EvaluateModel(records);
            MatrixFactorization.SaveModel();
        }

        double mae = 0;
        int count = 0;
        double tn = 0, tp = 0, fn = 0, fp = 0;
        double threshold = 3.5;
        foreach (var record in records)
        {
            var movieRatingPrediction = MatrixFactorization.UseModelForSinglePrediction(record);
            double predict = filter.PredictUserRating(record.UserId, record.MovieId);
            if (!double.IsNaN(movieRatingPrediction))
            {
                predict *= 0.45;
                predict += 0.55 * movieRatingPrediction;
            }
            predict = Math.Round(predict, 1);
            if (predict >= threshold)
            {
                if (record.UserRating >= threshold)
                {
                    tp++;
                }
                else
                {
                    fp++;
                }
            }
            else
            {
                if (record.UserRating >= threshold)
                {
                    fn++;
                }
                else
                {
                    tn++;
                }
            }
            mae += Math.Abs(predict - record.UserRating);
            count++;
            if (double.IsNaN(mae))
            {
                Console.WriteLine($"{count}: {mae} {predict} {movieRatingPrediction}");
                return;
            }
            if (count % 1000 == 0)
            {
                Console.WriteLine($"{count} at {DateTime.Now}: mae={mae / count}, accuracy={(tp + tn) / (tp + tn + fp + fn)}, " +
                    $"precision={(tp) / (tp + fp)}, recall={(tp) / (tp + fn)}, f1={(2 * tp) / (2 * tp + fp + fn)}");
            }
        }
        mae /= count;
        Console.WriteLine(DateTime.Now);
        Console.WriteLine($"mae={mae}, accuracy={(tp + tn) / (tp + tn + fp + fn)}, precision={(tp) / (tp + fp)}, " +
            $"recall={(tp) / (tp + fn)}, f1={(2 * tp) / (2 * tp + fp + fn)}");
    }
}