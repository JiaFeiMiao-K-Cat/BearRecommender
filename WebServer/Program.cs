
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Recommender.FeatureEng;
using Recommender.Matching;
using Recommender.Model;
using Recommender.Rank;
using System.Net;

namespace WebServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        var context = new Context(builder.Configuration["DbConnect"]);
        context.Database.EnsureCreated();
        var filter = new CollaborativeFiltering(context);
        var featureEng = new FeatureEngineering(context);
        var rank = new Rank(context);
        if (!MatrixFactorization.LoadModel())
        {
            MatrixFactorization.BuildAndTrainModel(context.Ratings);
            MatrixFactorization.SaveModel();
        }
        if (!Embedding.LoadModel())
        {
            Embedding.BuildAndTrainModel(context.UserFeatures);
            Embedding.SaveModel();
        }

        app.MapGet("/getRatingPrediction", (int userId, int movieId) =>
        {
            double predict = filter.PredictUserRating(userId, movieId);
            var mfPrediction = MatrixFactorization.UseModelForSinglePrediction(new Rating() { UserId = userId, MovieId = movieId});
            if (!double.IsNaN(mfPrediction))
            {
                predict *= 0.45;
                predict += 0.55 * mfPrediction;
            }
            predict = Math.Round(predict, 1);
            return predict;
        })
        .WithName("GetRatingPrediction")
        .WithOpenApi();

        app.MapPost("/addRating", (int userId, int movieId, float rating) => 
        {
            context.AddRating(new Rating { UserId = userId, MovieId = movieId, UserRating = rating, Timestamp = ConvertToTimestamp(DateTime.UtcNow)});
        })
        .WithName("AddRating")
        .WithOpenApi();

        app.MapPost("/addUser", (string perfer) =>
        {
            int id = context.AddNewUser(perfer);
            return context.UserFeatures.FirstOrDefault(e => e.UserId == id);
        })
        .WithName("AddUser")
        .WithOpenApi();

        app.MapPost("/recommendMovies", (int userId, int k) =>
        {
            return rank.RecommendMovies(userId, k);
        })
        .WithName("RecommendMovies")
        .WithOpenApi();

        app.MapPost("/offlineDeal", (string key) =>
        {
            if (key == builder.Configuration["OfflineDealKey"])
            {
                int count = featureEng.Offline();
                return Results.Ok(count);
            }
            else
            {
                return Results.StatusCode(StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("OfflineDeal")
        .WithOpenApi();

        app.Run();
    }
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static long ConvertToTimestamp(DateTime value)
    {
        TimeSpan elapsedTime = value.ToUniversalTime() - Epoch;
        return (long)elapsedTime.TotalSeconds;
    }
}