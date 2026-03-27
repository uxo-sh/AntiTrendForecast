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
        hits = data.get("hits", [])

        if not hits:
            return get_mock_result(keyword, "No direct HN matches found - using simulated fatigue estimate")

        mentions = len(hits)
        total_score = sum(hit.get("points", 0) for hit in hits)
        
        # Calculate fatigue metrics
        # If many hits are found quickly with high points, it suggests peak hype.
        sentiment = 0.1 
        growth = (total_score / 2000.0) - 0.5 

        return {
            "keyword": keyword,
            "mentions": mentions * 20, # Scale for visualization
            "sentiment_score": sentiment,
            "growth_rate": growth,
            "source": f"HackerNews Algolia API ({len(hits)} hits)"
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
