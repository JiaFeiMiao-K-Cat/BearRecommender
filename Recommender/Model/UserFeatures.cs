using CsvHelper.Configuration.Attributes;
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
public class UserFeatures
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
    /// [1,2)间评分数
    /// </summary>
    /// <remarks>
    /// 为了避免浮点误差和方便更新, 采用int存储, 实际计算相似度采用比值(下同)
    /// </remarks>
    [Column("ratingCount1")]
    public int RatingCount1 { get; set; }

    /// <summary>
    /// [2,3)间评分数
    /// </summary>
    [Column("ratingCount2")]
    public int RatingCount2 { get; set; }

    /// <summary>
    /// [3,4)间评分数
    /// </summary>
    [Column("ratingCount3")]
    public int RatingCount3 { get; set; }

    /// <summary>
    /// [4,5]间评分数
    /// </summary>
    [Column("ratingCount4")]
    public int RatingCount4 { get; set; }

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
}
