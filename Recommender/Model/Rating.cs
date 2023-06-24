using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.Data;
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
    [Column("userId"), Name("userId"), LoadColumn(0)]
    public int UserId { get; set; }

    /// <summary>
    /// 电影ID
    /// </summary>
    [Column("movieId"), Name("movieId"), LoadColumn(1)]
    public int MovieId { get; set; }

    /// <summary>
    /// 用户评分
    /// </summary>
    /// <remarks>
    /// 避免命名冲突, 重命名为UserRating
    /// </remarks>
    [Column("rating"), Name("rating"), LoadColumn(2)]
    public float UserRating { get; set; }

    /// <summary>
    /// 评分操作的时间戳
    /// </summary>
    [Column("timestamp"), Name("timestamp"), LoadColumn(3)]
    public long Timestamp { get; set; }
}
