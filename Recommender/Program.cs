using CsvHelper;
using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using Recommender.Model;
using System.Globalization;
using Recommender.FeatureEng;
using Recommender.Matching;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace Recommender;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine(DateTime.Now);

        var context = new Context("Data Source=model.db");
        context.Database.EnsureCreated();

        /*MLContext mlContext = new MLContext();
        (IDataView trainingDataView, IDataView testDataView) = LoadData(mlContext, context);
        ITransformer model = BuildAndTrainModel(mlContext, trainingDataView);
        EvaluateModel(mlContext, testDataView, model);
        UseModelForSinglePrediction(mlContext, model);*/

        var filter = new CollaborativeFiltering(context);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture);

        using var reader = new StreamReader("test.csv");
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<Rating>();
        //double mae = 0;
        int count = 0;
        double tn = 0, tp = 0, fn = 0, fp = 0;
        foreach (var record in records)
        {
            double predict = filter.Predict(record.UserId, record.MovieId);
            if (predict >= 3.5)
            {
                if (record.UserRating >= 3.5)
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
                if (record.UserRating >= 3.5)
                {
                    fn++;
                }
                else
                {
                    tn++;
                }
            }
            //Console.WriteLine($"{record.UserId} {record.MovieId}: {record.UserRating} -- {predict}");
            //mae += Math.Abs(predict - record.UserRating);
            count++;
            if (count % 100 == 0)
            {
                Console.WriteLine($"{tp}, {tn}, {fp}, {fn}");
                //Console.WriteLine($"{count}: {mae / count} at {DateTime.Now}");
                Console.WriteLine($"{count}: accuracy={(tp + tn) / (tp + tn + fp + fn)}, precision={(tp) / (tp + fp)}, recall={(tp) / (tp + fn)}, f1={(2 * tp) / (2 * tp + fp + fn)}");
            }
        }
        //mae /= records.Count();
        Console.WriteLine(DateTime.Now);
        //Console.WriteLine(mae);
    }
    public static (IDataView training, IDataView test) LoadData(MLContext mlContext, Context context)
    {
        // Load training & test datasets using datapaths
        // <SnippetLoadData>

        IDataView trainingDataView = mlContext.Data.LoadFromEnumerable(context.Ratings);
        IDataView testDataView = mlContext.Data.LoadFromTextFile<Rating>("test.csv", hasHeader: true, separatorChar: ',');

        return (trainingDataView, testDataView);
        // </SnippetLoadData>
    }
    public static ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainingDataView)
    {
        // Add data transformations
        // <SnippetDataTransformations>
        IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: "UserId")
            .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "movieIdEncoded", inputColumnName: "MovieId"));
        // </SnippetDataTransformations>

        // Set algorithm options and append algorithm
        // <SnippetAddAlgorithm>
        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = "userIdEncoded",
            MatrixRowIndexColumnName = "movieIdEncoded",
            LabelColumnName = "UserRating",
            NumberOfIterations = 20,
            ApproximationRank = 100
        };

        var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));
        // </SnippetAddAlgorithm>

        // <SnippetFitModel>
        Console.WriteLine("=============== Training the model ===============");
        ITransformer model = trainerEstimator.Fit(trainingDataView);

        return model;
        // </SnippetFitModel>
    }
    public static void EvaluateModel(MLContext mlContext, IDataView testDataView, ITransformer model)
    {
        // Evaluate model on test data & print evaluation metrics
        // <SnippetTransform>
        Console.WriteLine("=============== Evaluating the model ===============");
        var prediction = model.Transform(testDataView);
        // </SnippetTransform>

        // <SnippetEvaluate>
        var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "UserRating", scoreColumnName: "Score");
        // </SnippetEvaluate>

        // <SnippetPrintMetrics>
        Console.WriteLine("Root Mean Squared Error : " + metrics.RootMeanSquaredError.ToString());
        Console.WriteLine("RSquared: " + metrics.RSquared.ToString());
        // </SnippetPrintMetrics>
    }
    public static void UseModelForSinglePrediction(MLContext mlContext, ITransformer model)
    {
        // <SnippetPredictionEngine>
        Console.WriteLine("=============== Making a prediction ===============");
        var predictionEngine = mlContext.Model.CreatePredictionEngine<Rating, MovieRatingPrediction>(model);
        // </SnippetPredictionEngine>

        // Create test input & make single prediction
        // <SnippetMakeSinglePrediction>
        var testInput = new Rating { UserId = 6, MovieId = 10 };

        var movieRatingPrediction = predictionEngine.Predict(testInput);
        // </SnippetMakeSinglePrediction>

        // <SnippetPrintResults>
        if (Math.Round(movieRatingPrediction.Score, 1) > 3.5)
        {
            Console.WriteLine("Movie " + testInput.MovieId + " is recommended for user " + testInput.UserId);
        }
        else
        {
            Console.WriteLine("Movie " + testInput.MovieId + " is not recommended for user " + testInput.UserId);
        }
        // </SnippetPrintResults>
    }
}


public class MovieRatingPrediction
{
    public float Label;
    public float Score;
}