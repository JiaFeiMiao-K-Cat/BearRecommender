using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Text;
using Recommender.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Matching;

public static class MatrixFactorization
{
    static MLContext mlContext = new MLContext();

    static DataViewSchema schema;

    static ITransformer model; 
    public static bool LoadModel()
    {
        if (File.Exists("MovieRecommenderModel.zip"))
        {
            model = mlContext.Model.Load("MovieRecommenderModel.zip", out schema);
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void BuildAndTrainModel(IEnumerable<Rating> trainingData)
    {
        IDataView trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

        IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: "UserId")
            .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "movieIdEncoded", inputColumnName: "MovieId"));

        // Set algorithm options and append algorithm
        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = "userIdEncoded",
            MatrixRowIndexColumnName = "movieIdEncoded",
            LabelColumnName = "UserRating",
            NumberOfIterations = 20,
            ApproximationRank = 100
        };

        var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));

        Console.WriteLine("=============== Training the model ===============");
        model = trainerEstimator.Fit(trainingDataView);
    }
    public static void EvaluateModel(IEnumerable<Rating> testData)
    {
        IDataView testDataView = mlContext.Data.LoadFromEnumerable(testData);
        // Evaluate model on test data & print evaluation metrics
        Console.WriteLine("=============== Evaluating the model ===============");
        var prediction = model.Transform(testDataView);

        var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "UserRating", scoreColumnName: "Score");

        Console.WriteLine("Root Mean Squared Error : " + metrics.RootMeanSquaredError.ToString());
        Console.WriteLine("RSquared: " + metrics.RSquared.ToString());
    }
    public static float UseModelForSinglePrediction(Rating rating)
    {
        var predictionEngine = mlContext.Model.CreatePredictionEngine<Rating, MovieRatingPrediction>(model);

        var movieRatingPrediction = predictionEngine.Predict(rating);

        return movieRatingPrediction.Score;
    }
    public static void SaveModel()
    {
        Console.WriteLine("=============== Saving the model to a file ===============");
        mlContext.Model.Save(model, schema, "MovieRecommenderModel.zip");
    }
}
public class MovieRatingPrediction
{
    public float Label;
    public float Score;
}
