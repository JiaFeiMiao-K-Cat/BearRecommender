using CsvHelper.Configuration.Attributes;
using Recommender.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Model;

/// <summary>
/// 用户特征
/// </summary>
[Table("userFeatures")]
public class UserFeature
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Key, Column("userId")]
    public int UserId { get; set; }

    /// <summary>
    /// 总评分
    /// </summary>
    /// <remarks>
    /// 为了减小误差和便于更新, 记录总评分而非直接记录平均评分
    /// </remarks>
    [Column("totalRating")]
    public double TotalRating { get; set; }
    
    /// <summary>
    /// 评分总数, 用于计算平均评分
    /// </summary>
    [NotMapped]
    public int TotalRatingCount { get { return RatingCount1 + RatingCount2 + RatingCount3 + RatingCount4 + RatingCount5; } }

    /// <summary>
    /// 平均评分
    /// </summary>
    /// <remarks>
    /// 防止除零, 实际公式为: 总评分/(总评分数+1e-18)
    /// </remarks>
    [NotMapped]
    public double AverageRating { get { return TotalRating / (TotalRatingCount + 1e-18); } }

    /// <summary>
    /// (0,1]间评分数
    /// </summary>
    /// <remark>
    /// 为了减小误差和方便更新, 采用int存储, 实际计算相似度采用比值(下同)
    /// </remark>
    [Column("ratingCount1")]
    public int RatingCount1 { get; set; }

    /// <summary>
    /// (0,1]间评分率
    /// </summary>
    [NotMapped]
    public double RatingRate1 { get { return RatingCount1 / (TotalRatingCount + 1e-18); } }

    /// <summary>
    /// (1,2]间评分数
    /// </summary>
    [Column("ratingCount2")]
    public int RatingCount2 { get; set; }

    /// <summary>
    /// (1,2]间评分率
    /// </summary>
    [NotMapped]
    public double RatingRate2 { get { return RatingCount2 / (TotalRatingCount + 1e-18); } }

    /// <summary>
    /// (2,3]间评分数
    /// </summary>
    [Column("ratingCount3")]
    public int RatingCount3 { get; set; }

    /// <summary>
    /// (2,3]间评分率
    /// </summary>
    [NotMapped]
    public double RatingRate3 { get { return RatingCount3 / (TotalRatingCount + 1e-18); } }

    /// <summary>
    /// (3,4]间评分数
    /// </summary>
    [Column("ratingCount4")]
    public int RatingCount4 { get; set; }

    /// <summary>
    /// (3,4]间评分率
    /// </summary>
    [NotMapped]
    public double RatingRate4 { get { return RatingCount4 / (TotalRatingCount + 1e-18); } }

    /// <summary>
    /// (4,5]间评分数
    /// </summary>
    [Column("ratingCount5")]
    public int RatingCount5 { get; set; }

    /// <summary>
    /// (4,5]间评分率
    /// </summary>
    [NotMapped]
    public double RatingRate5 { get { return RatingCount5 / (TotalRatingCount + 1e-18); } }

    /// <summary>
    /// 观看过且评分大于3.5的电影编号列表
    /// </summary>
    [NotMapped]
    public List<int> MovieIds { get; set; } = new List<int>();

    /// <summary>
    /// MovieIds的字符串映射
    /// </summary>
    /// <remarks>
    /// 分隔符是空格, 方便后续Embedding操作
    /// </remarks>
    [Column("movieIds")]
    public string MovieIdsString
    {
        get => string.Join(" ", MovieIds);
        set => MovieIds = value == null ? new List<int>() : value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(int.Parse);
    }

    /// <summary>
    /// Embedding得到的特征向量
    /// </summary>
    [NotMapped]
    public float[] FeaturesEncoded { get; set; }

    /// <summary>
    /// 特征的字符串映射
    /// </summary>
    [Column("featuresEncoded")]
    public string FeaturesEncodedString
    {
        get => string.Join(",", FeaturesEncoded ?? Array.Empty<float>());
        set => FeaturesEncoded = value == null ? Array.Empty<float>() : Array.ConvertAll(value.Split(',', StringSplitOptions.RemoveEmptyEntries), float.Parse);
    }

    /// <summary>
    /// 电影偏好, 多热编码, 最多五项, 分类定义与电影分类一致
    /// </summary>
    /// <remarks>
    /// 为了保证位数一致方便运算, 使用long存储
    /// </remarks>
    [Column("perfer")]
    public ulong Perfer { get; set; }

    /// <summary>
    /// 上次计算偏好后的新增记录数
    /// </summary>
    /// <remarks>
    /// 超过一定数值后应当更新电影偏好
    /// </remarks>
    [Column("newRecordsCount")]
    public int NewRecordsCount { get; set; }

    public double SquareSum()
    {
        double square = 0;
        square += Math.Pow(AverageRating, 2);
        square += Math.Pow(RatingRate1, 2);
        square += Math.Pow(RatingRate2, 2);
        square += Math.Pow(RatingRate3, 2);
        square += Math.Pow(RatingRate4, 2);
        square += Math.Pow(RatingRate5, 2);
        square += MultiHotEncoding.Count(Perfer);
        return square;
    }

    public double Average()
    {
        double avg = AverageRating / 5 + RatingRate1 + RatingRate2 + RatingRate3 + RatingRate4 + RatingRate5;
        avg /= 6;
        return avg;
    }

    public double Variance()
    {
        double var = 0;
        double avg = Average();
        var += Math.Pow(AverageRating / 5 - avg, 2);
        var += Math.Pow(RatingRate1 - avg, 2);
        var += Math.Pow(RatingRate2 - avg, 2);
        var += Math.Pow(RatingRate3 - avg, 2);
        var += Math.Pow(RatingRate4 - avg, 2);
        var += Math.Pow(RatingRate5 - avg, 2);
        return var;
    }
}
