using CsvHelper;
using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Recommender.FeatureEng;
using Recommender.Model;
using System.Formats.Asn1;
using System.Globalization;

namespace DataConverter;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine($"Start: {DateTime.Now}");

        var context = new Context($"Data Source={args[0]}");
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture);

        #region Convert movies.csv to sqlite file
        using var moviesReader = new StreamReader(args[1]);
        using var moviesCsv = new CsvReader(moviesReader, config);
        var moviesRecords = moviesCsv.GetRecords<Movie>();
        await context.BulkInsertAsync<Movie>(moviesRecords.ToList()); //不使用ToList()会导致无法写入数据库
        await context.BulkSaveChangesAsync();
        #endregion

        Console.WriteLine($"Movies converted: {DateTime.Now}");

        #region Convert ratings.csv to sqlite file
        using var ratingsReader = new StreamReader(args[2]);
        using var ratingsCsv = new CsvReader(ratingsReader, config);
        var ratingsRecords = ratingsCsv.GetRecords<Rating>();
        await context.BulkInsertAsync<Rating>(ratingsRecords.ToList()); //不使用ToList()会导致无法写入数据库
        await context.BulkSaveChangesAsync();
        #endregion

        Console.WriteLine($"Ratings converted: {DateTime.Now}"); 
        
        var featureEngineering = new FeatureEngineering(context);

        #region Feature Engineering for users
        List<int> users = context.Ratings.Select(e => e.UserId).Distinct().ToList();

        Console.WriteLine($"Start Feature Engineering: {DateTime.Now}");

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
        context.SaveChanges();
        Console.WriteLine(DateTime.Now);
        var userFeatures = context.UserFeatures.ToList();

        if (!Embedding.LoadModel())
        {
            Embedding.BuildAndTrainModel(userFeatures);
        }

        Embedding.UseModelForMultiPrediction(userFeatures);
        foreach (var user in userFeatures)
        {
            context.UserFeatures.Update(user);
        }
        context.SaveChanges();
        #endregion

        Console.WriteLine($"Users Feature Engineering Finished: {DateTime.Now}");

        /*#region Feature Engineering for movies
        List<int> movies = context.Movies.Select(e => e.MovieId).ToList();

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

        Console.WriteLine($"Movies Feature Engineering Finished: {DateTime.Now}");
        #endregion*/
    }
}