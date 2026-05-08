using System.Collections.Generic;
using UnityEngine;

namespace AlgorithmOcean.Dohyeon
{
    public sealed class PreferredGenreStore : MonoBehaviour
    {
        [SerializeField] private List<string> preferredGenres = new();

        public IReadOnlyList<string> PreferredGenres => preferredGenres;

        public void SetPreferredGenres(List<string> genres)
        {
            preferredGenres = genres != null ? new List<string>(genres) : new List<string>();
        }

        public void AddPreferredGenre(string genre)
        {
            if (string.IsNullOrWhiteSpace(genre) || ContainsGenre(genre))
            {
                return;
            }

            preferredGenres.Add(genre.Trim());
        }

        public void RemovePreferredGenre(string genre)
        {
            for (int i = preferredGenres.Count - 1; i >= 0; i--)
            {
                if (IsSameGenre(preferredGenres[i], genre))
                {
                    preferredGenres.RemoveAt(i);
                }
            }
        }

        public bool ContainsGenre(string genre)
        {
            foreach (string preferredGenre in preferredGenres)
            {
                if (IsSameGenre(preferredGenre, genre))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSameGenre(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
            {
                return false;
            }

            return string.Equals(a.Trim(), b.Trim(), System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
