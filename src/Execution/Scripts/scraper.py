import sys
import json
import random

def main():
    if len(sys.argv) < 2:
        print(json.dumps({"error": "No keyword provided"}))
        sys.exit(1)

    keyword = sys.argv[1]
    
    # Mock data generation
    # In a real scenario, this would involve Scrapy or Selenium
    mentions = random.randint(50, 500)
    sentiment = random.uniform(-1.0, 1.0)
    growth_rate = random.uniform(-0.2, 0.5)

    data = {
        "keyword": keyword,
        "mentions": mentions,
        "sentiment_score": sentiment,
        "growth_rate": growth_rate,
        "source": "MockSocialScraper"
    }

    print(json.dumps(data))

if __name__ == "__main__":
    main()
