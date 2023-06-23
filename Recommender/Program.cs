using CsvHelper;
using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using Recommender.Model;
using System.Globalization;
using Recommender.FeatureEng;

namespace Recommender;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine(DateTime.Now);

        var context = new Context("Data Source=model.db");
        context.Database.EnsureCreated();

        var featureEngineering = new FeatureEngineering(context);

        List<int> movies = context.Movies.Select(e => e.MovieId).ToList();
        List<int> users = context.Ratings.Select(e => e.UserId).Distinct().ToList();

        Console.WriteLine(movies.Count);
        Console.WriteLine(users.Count);

        Console.WriteLine(DateTime.Now);

        for (int i = 0; i < users.Count; i++)
        {
            if (i % 1000 == 0)
            {
                featureEngineering.GenerateUserFeature(users[i]);
                Console.WriteLine($"{i}: {DateTime.Now}");
            }
            else
            {
                featureEngineering.GenerateUserFeature(users[i], false);
            }
        }

        Console.WriteLine(DateTime.Now);

        for (int i = 0; i < movies.Count; i++)
        {
            if (i % 1000 == 0)
            {
                featureEngineering.GenerateMovieFeature(movies[i]);
                Console.WriteLine($"{i}: {DateTime.Now}");
            }
            else
            {
                featureEngineering.GenerateMovieFeature(movies[i], false);
            }
        }

        Console.WriteLine(DateTime.Now);
    }
}