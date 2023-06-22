using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Model;

/// <summary>
/// 电影特征
/// </summary>
[Table("movieFeatures")]
public class MovieFeature
{
    /// <summary>
    /// 电影ID
    /// </summary>
    [Key, Column("movieId")]
    public int MovieId { get; set; }

    /// <summary>
    /// 平均评分
    /// </summary>
    [Column("averageRating")]
    public double AverageRating { get; set; }

    /// <summary>
    /// [1,2)间评分数
    /// </summary>
    /// <remark>
    /// 为了避免浮点误差和方便更新, 采用int存储, 实际计算相似度采用比值(下同)
    /// </remark>
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
    /// 分类(多热编码形式)
    /// </summary>
    /// <remarks>
    /// 目前只用19位,为方便后期扩充用long类型
    /// </remarks>
    [Column("genres")]
    public long Genres { get; set; }
}
