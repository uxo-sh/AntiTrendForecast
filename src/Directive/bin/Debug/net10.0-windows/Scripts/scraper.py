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
    Scrapes Hacker News top stories and filters by keyword.
    If no matches or requests missing, returns a mock simulation.
    """
    if not REQUESTS_AVAILABLE:
        return get_mock_result(keyword, "Requests library missing - falling back to simulation")

    try:
        # Get the top stories
        top_ids_url = "https://hacker-news.firebaseio.com/v0/topstories.json"
        response = requests.get(top_ids_url, timeout=5)
        story_ids = response.json()[:30]  # Check more stories for better matching

        mentions = 0
        total_score = 0
        total_comments = 0
        
        for s_id in story_ids:
            item_url = f"https://hacker-news.firebaseio.com/v0/item/{s_id}.json"
            item = requests.get(item_url, timeout=2).json()
            if not item:
                continue
                
            title = item.get("title", "").lower()
            
            if keyword.lower() in title:
                mentions += 1
                total_score += item.get("score", 0)
                total_comments += item.get("descendants", 0)
        
        # If no real mentions found, simulate some based on the keyword's generic "popularity"
        if mentions == 0:
            return get_mock_result(keyword, "No direct HN matches found - using simulated fatigue estimate")

        # Map HN metrics to our fatigue model
        # For HN: high scores/comments on a few posts suggest emerging or peak interest.
        # High mentions across many posts suggest "saturation" or "hype fatigue".
        
        sentiment = 0.1 # Real sentiment analysis would go here
        growth = (total_score / 1000.0) - 0.5 # Dummy growth estimation

        return {
            "keyword": keyword,
            "mentions": mentions * 50, # Boost for visualization
            "sentiment_score": sentiment,
            "growth_rate": growth,
            "source": "HackerNews API"
        }

    except Exception as e:
        return get_mock_result(keyword, f"HN API Error: {str(e)}")

def get_mock_result(keyword, source_note):
    # Fallback simulation
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
    kw = sys.argv[1] if len(sys.argv) > 1 else "AI"
    result = get_hacker_news_trends(kw)
    print(json.dumps(result))
