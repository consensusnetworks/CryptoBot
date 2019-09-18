docker stop bot
docker rm bot
docker build -t cryptobot https://github.com/consensusnetworks/CryptoBot.git
cls
docker run --restart=always -d -e "BOTURL=https://docs.google.com/spreadsheets/d/1D6QsKlXhJXN3HKxSxrLxi9gAvB5QfjP01nt9o-VvubU/edit?usp=sharing" --name bot cryptobot
docker logs bot