using Recommender.Model;
using Recommender.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.FeatureEng;

public class FeatureEngineering
{
    private Context _context { get; set; }

    public FeatureEngineering(Context context)
    {
        _context = context;
    }

    /// <summary>
    /// 离线更新
    /// </summary>
    /// <returns>
    /// 更新用户特征数量
    /// </returns>
    public int Offline()
    {
        var users = _context.UserFeatures
            .Where(e => e.NewRecordsCount > 0); // 无新记录的不需要更新
        foreach (var user in users)
        {
            UpdateUserFeature(user.UserId);
            user.NewRecordsCount = 0; // 计数器归零
        }
        return users.Count();
    }

    /// <summary>
    /// 生成电影特征
    /// </summary>
    /// <param name="movieId">电影ID</param>
    /// <param name="saveChanges">是否保存更新到数据库, 默认为true</param>
    /// <remarks>为了提高性能可间隔保存, 但可能导致数据丢失</remarks>
    public void GenerateMovieFeature(int movieId, bool saveChanges = true)
    {
        var feature = _context.MovieFeatures.FirstOrDefault(e => e.MovieId == movieId);
        if (feature != null)
        {
            return;
        } // 已存在特征, 无需更新
        else
        {
            feature = new MovieFeature();
            feature.MovieId = movieId;
            var ratings = _context.Ratings.Where(e => e.MovieId == movieId);
            feature.TotalRating = ratings.Sum(e => e.UserRating);
            var ratingCount = ratings.ToList() // 此处使用的函数不能转换为SQL, 因此需要先转成List
                .GroupBy(e => Math.Ceiling(e.UserRating)) // 上取整转换为整数方便统计评分分布
                .Select(p => new { Key = p.Key, Value = p.Count() });
            feature.RatingCount1 = ratingCount.FirstOrDefault(e => e.Key == 1)?.Value ?? 0;
            feature.RatingCount2 = ratingCount.FirstOrDefault(e => e.Key == 2)?.Value ?? 0;
            feature.RatingCount3 = ratingCount.FirstOrDefault(e => e.Key == 3)?.Value ?? 0;
            feature.RatingCount4 = ratingCount.FirstOrDefault(e => e.Key == 4)?.Value ?? 0;
            feature.RatingCount5 = ratingCount.FirstOrDefault(e => e.Key == 5)?.Value ?? 0;
            feature.Genres = MultiHotEncoding.GetMultiEncoding(_context.Movies.First(e => e.MovieId == movieId).Genres);
            _context.MovieFeatures.Add(feature);
            if (saveChanges)
            {
                _context.SaveChanges();
            }            
        }
    }

    /// <summary>
    /// 计算用户偏好
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户偏好的多热编码</returns>
    public ulong FindUserPerfer(int userId)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        var ratings = _context.Ratings
            .Where(e => e.UserId == userId)
            .ToList();
        foreach (var rating in ratings)
        {
            string[]? geners = _context.Movies
                .FirstOrDefault(e => e.MovieId == rating.MovieId)?.Genres
                .Split('|');
            if (geners != null)
            {
                foreach (var gener in geners)
                {
                    if (dict.ContainsKey(gener))
                    {
                        dict[gener]++;
                    }
                    else
                    {
                        dict.Add(gener, 1);
                    }
                }
            }
        }
        return MultiHotEncoding.GetPerferCoding(dict);
    }

    private void UpdateUserFeature(int userId)
    {
        var feature = _context.UserFeatures.FirstOrDefault(e => e.UserId == userId);
        feature!.Perfer = FindUserPerfer(userId);
        Embedding.UseModelForSinglePrediction(feature);
        _context.UserFeatures.Update(feature);
        _context.SaveChanges();
    }

    /// <summary>
    /// 生成用户特征
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="saveChanges">是否保存更新到数据库, 默认为true</param>
    /// <remarks>不生成Embedding特征, 需要手动调用相关方法</remarks>
    public void GenerateUserFeature(int userId, bool saveChanges = true)
    {
        var feature = _context.UserFeatures.FirstOrDefault(e => e.UserId == userId);
        if (feature != null)
        {
            return;
        } // 已存在特征, 无需更新
        else
        {
            feature = new UserFeature();
            feature.UserId = userId;
            var ratings = _context.Ratings.Where(e => e.UserId == userId);
            feature.TotalRating = ratings.Sum(e => e.UserRating);
            var ratingCount = ratings.ToList()
                .GroupBy(e => Math.Ceiling(e.UserRating))
                .Select(p => new { Key = p.Key, Value = p.Count() });
            feature.RatingCount1 = ratingCount.FirstOrDefault(e => e.Key == 1)?.Value ?? 0;
            feature.RatingCount2 = ratingCount.FirstOrDefault(e => e.Key == 2)?.Value ?? 0;
            feature.RatingCount3 = ratingCount.FirstOrDefault(e => e.Key == 3)?.Value ?? 0;
            feature.RatingCount4 = ratingCount.FirstOrDefault(e => e.Key == 4)?.Value ?? 0;
            feature.RatingCount5 = ratingCount.FirstOrDefault(e => e.Key == 5)?.Value ?? 0;
            feature.Perfer = FindUserPerfer(userId);
            feature.MovieIds = ratings
                .Where(e => e.UserRating >= 3.5) // 小于3.5的电影认为不喜欢, 不做统计
                .OrderBy(e => e.Timestamp) // 时间升序方便后续更新
                .Select(e => e.MovieId)
                .ToList();
            _context.UserFeatures.Add(feature);
            if (saveChanges)
            {
                _context.SaveChanges();
            }
        }
    }
}
