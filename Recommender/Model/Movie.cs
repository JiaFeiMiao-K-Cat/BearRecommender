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
/// 电影
/// </summary>
[Table("movies")]
public class Movie
{
    /// <summary>
    /// 电影ID
    /// </summary>
    [Key, Column("movieId"), Name("movieId")]
    public int MovieId { get; set; }

    /// <summary>
    /// 电影标题
    /// </summary>
    [Column("title"), Name("title")]
    public string Title { get; set; }

    /// <summary>
    /// 电影分类
    /// </summary>
    /// <remarks>
    /// 共19类, 多个类间用'|'分割
    /// </remarks>
    [Column("genres"), Name("genres")]
    public string Genres { get; set; }

}
