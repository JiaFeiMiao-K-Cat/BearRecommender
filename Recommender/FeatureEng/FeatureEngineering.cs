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
            var ratings = _context.Ratings.Where(e => e.MovieId == movieId).ToList();
            feature.TotalRating = ratings.Sum(e => e.UserRating);
            var ratingCount = ratings.GroupBy(e => Math.Ceiling(e.UserRating)).Select(p => new { Key = p.Key, Value = p.Count() });
            feature.RatingCount1 = ratingCount.FirstOrDefault(e => e.Key == 1)?.Value ?? 0;
            feature.RatingCount2 = ratingCount.FirstOrDefault(e => e.Key == 2)?.Value ?? 0;
            feature.RatingCount3 = ratingCount.FirstOrDefault(e => e.Key == 3)?.Value ?? 0;
            feature.RatingCount4 = ratingCount.FirstOrDefault(e => e.Key == 4)?.Value ?? 0;
            feature.RatingCount5 = ratingCount.FirstOrDefault(e => e.Key == 5)?.Value ?? 0;
            feature.Genres = MultiEncoding.GetMultiEncoding(_context.Movies.First(e => e.MovieId == movieId).Genres);
            _context.MovieFeatures.Add(feature);
            if (saveChanges)
            {
                _context.SaveChanges();
            }            
        }
    }

    public long FindUserPerfer(int userId)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        var ratings = _context.Ratings.Where(e => e.UserId == userId).ToList();
        foreach (var rating in ratings)
        {
            string[]? geners = _context.Movies.FirstOrDefault(e => e.MovieId == rating.MovieId)?.Genres.Split('|');
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
        return MultiEncoding.GetPerferCoding(dict);
    }

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
            var ratingCount = ratings.ToList().GroupBy(e => Math.Ceiling(e.UserRating)).Select(p => new { Key = p.Key, Value = p.Count() });
            feature.RatingCount1 = ratingCount.FirstOrDefault(e => e.Key == 1)?.Value ?? 0;
            feature.RatingCount2 = ratingCount.FirstOrDefault(e => e.Key == 2)?.Value ?? 0;
            feature.RatingCount3 = ratingCount.FirstOrDefault(e => e.Key == 3)?.Value ?? 0;
            feature.RatingCount4 = ratingCount.FirstOrDefault(e => e.Key == 4)?.Value ?? 0;
            feature.RatingCount5 = ratingCount.FirstOrDefault(e => e.Key == 5)?.Value ?? 0;
            feature.Perfer = FindUserPerfer(userId);
            feature.MovieIds = ratings
                .Where(e => e.UserRating > 3.5)
                .OrderBy(e => e.Timestamp)
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
