echo Shutting Down
docker compose down
echo Removing Image
docker rmi algiro/pool-scraper:latest
echo Pulling new Image
docker pull algiro/pool-scraper:latest
echo Starting up....
docker compose up -d --build