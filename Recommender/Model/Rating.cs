using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Model;

/// <summary>
/// 评分
/// </summary>
[Table("ratings"), PrimaryKey(nameof(UserId), nameof(MovieId), nameof(Timestamp))]
public class Rating
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Column("userId"), Name("userId")]
    public int UserId { get; set; }

    /// <summary>
    /// 电影ID
    /// </summary>
    [Column("movieId"), Name("movieId")]
    public int MovieId { get; set; }

    /// <summary>
    /// 用户评分
    /// </summary>
    /// <remarks>
    /// 避免命名冲突, 重命名为UserRating
    /// </remarks>
    [Column("rating"), Name("rating")]
    public double UserRating { get; set; }

    /// <summary>
    /// 评分操作的时间戳
    /// </summary>
    [Column("timestamp"), Name("timestamp")]
    public long Timestamp { get; set; }
}
