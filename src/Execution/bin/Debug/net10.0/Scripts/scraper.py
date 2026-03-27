import sys
import json
import random

try:
    import requests
    REQUESTS_AVAILABLE = True
except ImportError:
    REQUESTS_AVAILABLE = False

def get_hacker_news_trends(keyword):
    """
    Scrapes Hacker News using the Algolia Search API.
    This is extremely fast because it performs keyword filtering on the server.
    """
    if not REQUESTS_AVAILABLE:
        return get_mock_result(keyword, "Requests library missing - falling back to simulation")

    try:
        # Algolia Search API for Hacker News
        # Returns matches for the keyword in one single request.
        search_url = f"https://hn.algolia.com/api/v1/search?query={keyword}&tags=story"
        response = requests.get(search_url, timeout=10) # 10s internal timeout
        
        data = response.json()
        nb_hits = data.get("nbHits", 0)
        hits = data.get("hits", [])

        if nb_hits == 0:
            return get_mock_result(keyword, "No direct HN matches found - using simulated fatigue estimate")

        # nbHits is the total number of matches across all of HN
        # This gives us a much better 'Fatigue' resolution than just the current page
        # nbHits is the total number of matches across all of HN
        # This gives us a much better 'Fatigue' resolution than just the current page
        mentions = nb_hits
        
        # Sentiment based on density of points (Hype density)
        # High points per hit usually means more 'hype' - we want this to influence the score
        avg_points = sum(hit.get("points", 0) for hit in hits) / len(hits) if hits else 0
        sentiment = min(avg_points / 300.0, 1.0) # 0 to 1 scale
        
        # Growth: Relative volume compared to a 'saturation floor'
        # Logarithmic scale works best for extreme ranges like HN nbHits
        import math
        growth = math.log10(nb_hits) / 6.0 - 0.5 # Normalizing log10(1,000,000) around 0.5

        return {
            "keyword": keyword,
            "mentions": mentions,
            "sentiment_score": sentiment,
            "growth_rate": growth,
            "source": f"HackerNews Index ({nb_hits} total hits)"
        }

    except Exception as e:
        return get_mock_result(keyword, f"Search API Error: {str(e)}")

def get_mock_result(keyword, source_note):
    mentions = random.randint(150, 480)
    sentiment = random.uniform(-0.5, 0.5)
    growth = random.uniform(-0.3, 0.3)
    
    return {
        "keyword": keyword,
        "mentions": mentions,
        "sentiment_score": sentiment,
        "growth_rate": growth,
        "source": source_note
    }

if __name__ == "__main__":
    # Handle keyword from command line
    kw = sys.argv[1] if len(sys.argv) > 1 else "AI"
    result = get_hacker_news_trends(kw)
    # Ensure ONE single JSON line for C# to parse
    print(json.dumps(result))
