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
    public int TotalRatingCount { get { return RatingCount1 + RatingCount2 + RatingCount3 + RatingCount4; } }

    /// <summary>
    /// 平均评分
    /// </summary>
    /// <remarks>
    /// 防止除零, 实际公式为: 总评分/(总评分数+1e-9)
    /// </remarks>
    [NotMapped]
    public double AverageRating { get { return TotalRating / (TotalRatingCount + 1e-9); } }

    /// <summary>
    /// [1,2)间评分数
    /// </summary>
    /// <remark>
    /// 为了减小误差和方便更新, 采用int存储, 实际计算相似度采用比值(下同)
    /// </remark>
    [Column("ratingCount1")]
    public int RatingCount1 { get; set; }

    /// <summary>
    /// [1,2)间评分率
    /// </summary>
    [NotMapped]
    public double RatingRate1 { get { return RatingCount1 / (TotalRatingCount + 1e-9); } }

    /// <summary>
    /// [2,3)间评分数
    /// </summary>
    [Column("ratingCount2")]
    public int RatingCount2 { get; set; }

    /// <summary>
    /// [2,3)间评分率
    /// </summary>
    [NotMapped]
    public double RatingRate2 { get { return RatingCount2 / (TotalRatingCount + 1e-9); } }

    /// <summary>
    /// [3,4)间评分数
    /// </summary>
    [Column("ratingCount3")]
    public int RatingCount3 { get; set; }

    /// <summary>
    /// [3,4)间评分率
    /// </summary>
    [NotMapped]
    public double RatingRate3 { get { return RatingCount3 / (TotalRatingCount + 1e-9); } }

    /// <summary>
    /// [4,5]间评分数
    /// </summary>
    [Column("ratingCount4")]
    public int RatingCount4 { get; set; }

    /// <summary>
    /// [4,5]间评分率
    /// </summary>
    [NotMapped]
    public double RatingRate4 { get { return RatingCount4 / (TotalRatingCount + 1e-9); } }

    /// <summary>
    /// 电影偏好, 多热编码, 最多五项, 分类定义与电影分类一致
    /// </summary>
    /// <remarks>
    /// 为了保证位数一致方便运算, 使用long存储
    /// </remarks>
    [Column("perfer")]
    public long Perfer { get; set; }

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
        square += Math.Pow(MultiEncoding.Count(Perfer), 2);
        return square;
    }
}
