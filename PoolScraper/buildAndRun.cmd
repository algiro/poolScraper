docker build . -t pool-scraper-local
docker run -p 80:8080 pool-scraper-local