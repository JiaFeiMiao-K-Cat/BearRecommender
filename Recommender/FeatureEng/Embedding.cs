using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Text;
using Recommender.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.FeatureEng;

public static class Embedding
{
    static MLContext mlContext = new MLContext();

    static DataViewSchema schema;

    static ITransformer model;

    /// <summary>
    /// 加载本地模型
    /// </summary>
    /// <returns>是否存在本地模型</returns>
    public static bool LoadModel()
    {
        if (File.Exists("WordEmbedding.zip"))
        {
            model = mlContext.Model.Load("WordEmbedding.zip", out schema);
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 计算多个用户的特征
    /// </summary>
    /// <param name="users">用户序列</param>
    public static void UseModelForMultiPrediction(IEnumerable<UserFeature> users)
    {
        var predictionEngine = mlContext.Model.CreatePredictionEngine<TextData,
            TransformedTextData>(model);

        int count = 0;

        foreach (var user in users)
        {
            user.FeaturesEncoded = predictionEngine.Predict(new TextData { Text = user.MovieIdsString }).Features;
            count++;
            if (count % 1000 == 0)
            {
                Console.WriteLine($"{count} {DateTime.Now}");
            }
        }
    }
    /// <summary>
    /// 计算单个用户的特征
    /// </summary>
    /// <param name="user">用户</param>
    public static void UseModelForSinglePrediction(UserFeature user)
    {
        var predictionEngine = mlContext.Model.CreatePredictionEngine<TextData,
            TransformedTextData>(model);
                
        user.FeaturesEncoded = predictionEngine.Predict(new TextData { Text = user.MovieIdsString }).Features;
    }
    private class TextData
    {
        public string Text { get; set; }
    }

    private class TransformedTextData : TextData
    {
        public string[] Words { get; set; }
        public float[] Features { get; set; }
    }
    /// <summary>
    /// 创建并训练模型
    /// </summary>
    /// <param name="users">训练集</param>
    public static void BuildAndTrainModel(IEnumerable<UserFeature> users)
    {
        var data = users.Select(e => new TextData() { Text = e.MovieIdsString });

        var trainingDataView = mlContext.Data.LoadFromEnumerable(data);
        
        schema = trainingDataView.Schema;

        var textPipeline = mlContext.Transforms.Text.NormalizeText("Text")
                .Append(mlContext.Transforms.Text.TokenizeIntoWords("Words", "Text"))
                .Append(mlContext.Transforms.Text.ApplyWordEmbedding("Features", "Words", 
                    WordEmbeddingEstimator.PretrainedModelKind.GloVe100D));

        Console.WriteLine($"Start Fit: {DateTime.Now}");

        model = textPipeline.Fit(trainingDataView);

        Console.WriteLine($"End Fit: {DateTime.Now}");

        SaveModel();
    }
    /// <summary>
    /// 保存模型到本地
    /// </summary>
    public static void SaveModel()
    {
        Console.WriteLine("=============== Saving the model to a file ===============");
        mlContext.Model.Save(model, schema, "WordEmbedding.zip");
    }
}
