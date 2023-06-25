using NetTopologySuite.Utilities;
using Recommender.Model;
using Recommender.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Matching;

public class CollaborativeFiltering
{
    private Context _context { get; set; }
    public CollaborativeFiltering(Context context)
    {
        _context = context;
    }

    /// <summary>
    /// 预测用户评分
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="movieId">电影ID</param>
    /// <returns>预测评分</returns>
    public double PredictUserRating(int userId, int movieId)
    {
        double numerator = 0;
        double denominator = 0;
        var user = _context.UserFeatures.FirstOrDefault(x => x.UserId == userId);
        if (user == null)
        {
            return 0;
        }
        var sim = _context.UserFeatures
            .Where(x => _context.Ratings.Any(e => e.UserId == x.UserId && e.MovieId == movieId)) // 从看过这部电影的人里面计算相似度
            .ToList()
            .OrderByDescending(x => Similarity.UserSimilarity(x, user)) // 按相似度降序
            .Take(10)
            .ToList();
        foreach (var item in sim)
        {
            var rating = _context.Ratings
                .FirstOrDefault(e => e.UserId == item.UserId && e.MovieId == movieId);
            if (rating!.UserRating <= 0)
            {
                continue;
            } // 忽略无效评分
            else
            {
                double similarity = Similarity.UserSimilarity(item, user);
                numerator += similarity * rating.UserRating;
                denominator += similarity;
                // 按相似度加权平均
            }
        }
        if (denominator == 0)
        {
            numerator = user!.AverageRating;
            denominator = 1;
            // 没有其他用户看过这部电影, 用当前用户的平均评分代替
        }
        return numerator / denominator;
    }

    /// <summary>
    /// 推荐电影
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="k">电影数量, 实际数量为[0, k^{2}]</param>
    /// <returns>电影ID列表</returns>
    public List<int>? RecommendMovies(int userId, int k)
    {
        var list = new List<int>();
        var user = _context.UserFeatures.FirstOrDefault(x => x.UserId == userId);
        if (user == null)
        {
            return null;
        }
        var sim = _context.UserFeatures
            .ToList()
            .OrderByDescending(x => Similarity.UserSimilarity(x, user))
            .Take(k); // 最相似的k位用户

        foreach ( var item in sim)
        {
            var movies = _context.Ratings
                .Where(e => e.UserId == item.UserId)
                .OrderByDescending(p => p.Timestamp)
                .ThenByDescending(q => q.UserRating)
                .Take(k)
                .Select(e => e.MovieId); // 每位用户最近观看的k部影片

            foreach (var movie in movies)
            {
                list.Add(movie);
            }
        }

        return list.Distinct().ToList(); // 去重后再返回
    }
}
